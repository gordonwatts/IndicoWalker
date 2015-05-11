using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IWalker.Util
{
    public static class LINQHelpers
    {
        /// <summary>
        /// Change any observable stream into a Unit stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<Unit> AsUnit<T>(this IObservable<T> source)
        {
            return source.Select(_ => default(Unit));
        }

        /// <summary>
        /// Limit the # of items that move through the second sequence. This limits only in a subscrpition, not globally!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        /// <remarks>
        /// Coppied from here:
        ///     https://social.msdn.microsoft.com/Forums/en-US/379a027d-4a06-4abd-9255-d54c3807b50c/parallel-processing-of-incoming-events-observablesemaphoreobserveparallel-?forum=rx
        ///     There are significant modifications because this was a very old method!
        /// </remarks>
        public static IObservable<T> Semaphore<T>(IObservable<T> source, int maxCount, IScheduler sched = null)
        {
            // By default use the thread pool (so this will be defered, and can do multiple guys, which is what we want).
            sched = sched ?? Scheduler.Default;

            return Observable.Create<T>(o =>
            {
                var cancel = new CancellationDisposable();
                var queue = new ConcurrentQueue<Notification<T>>();
                var limitter = new SemaphoreSlim(maxCount);
                Notification<T> final = null;
                int pendingAccepts = 0;
                var subscription = source.Materialize().Subscribe(
                n =>
                {
                    if (cancel.Token.IsCancellationRequested)
                        return;

                    try { limitter.Wait(cancel.Token); }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    Interlocked.Increment(ref pendingAccepts);
                    queue.Enqueue(n);
                    sched.Schedule(() =>
                    {
                        try
                        {
                            Notification<T> notification;
                            queue.TryDequeue(out notification);
                            if (notification.Kind != NotificationKind.OnNext)
                                final = notification;
                            else
                                notification.Accept(o);
                        }
                        finally
                        {
                            limitter.Release();
                            if (Interlocked.Decrement(ref pendingAccepts) == 0)     // try to go lock-free a long a possible
                            {
                                lock (queue)    // take the queue as gate (only one thread should accept final notification)
                                {
                                    if (final != null               // make sure the final call has been received
                                        && pendingAccepts == 0      // and we are behind the decrement of that thread
                                    )
                                    {
                                        final.Accept(o);
                                        final = null;
                                    }
                                }
                            }
                        }
                    });
                });
                return new CompositeDisposable(cancel, subscription);
            });
        }

        /// <summary>
        /// Max number of items that can pass through here.
        /// </summary>
        public class LimitGlobalCounter
        {
            SemaphoreSlim _limiter = null;

            public LimitGlobalCounter(int maxCount)
            {
                if (maxCount <= 0)
                    throw new ArgumentException("Count must be ge to zero");

                _limiter = new SemaphoreSlim(maxCount);
            }

            public void Wait(CancellationToken token)
            {
                _limiter.Wait(token);
            }

            public async Task<bool> WaitAsync(CancellationToken token)
            {
                await _limiter.WaitAsync(token);
                return true;
            }

            internal void Release()
            {
                _limiter.Release();
            }
        }

        /// <summary>
        /// Limit the number of simultaniously executing "limitedSequences" to maxCount.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="source"></param>
        /// <param name="limitedSequence"></param>
        /// <param name="maxCount"></param>
        /// <param name="sched"></param>
        /// <returns></returns>
        public static IObservable<U> LimitGlobally<T,U> (this IObservable<T> source, Func<IObservable<T>, IObservable<U>> limitedSequence, int maxCount, IScheduler sched = null)
        {
            var counter = new LimitGlobalCounter(maxCount);
            return source.LimitGlobally(limitedSequence, counter, sched);
        }

        /// <summary>
        /// Limit the number of simultaniously executing "limitedSequences" to counter. Counter can be shared
        /// accross multiple calls.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="source"></param>
        /// <param name="limitedSequence">The transformation</param>
        /// <param name="limitter">An instance of LimitGlobalCounter. Can be shared between various calls of this.</param>
        /// <param name="sched">IScheduler to execute this on. If left null, defaults to Scheduler.Default</param>
        /// <returns>The resulting sequence transformed by limitedSequence.</returns>
        /// <remarks>
        /// When the source sequence completes, the terminating sequence will complete after each sequence from limitedSequence compleats.
        /// If source OnErrors, or limitedSequence on errors, then no further items will be processed. Once all ongoing sequences complete, the
        /// first OnError will be passed on to the resulting sequence.
        /// </remarks>
        public static IObservable<U> LimitGlobally<T, U>(this IObservable<T> source, Func<IObservable<T>, IObservable<U>> limitedSequence, LimitGlobalCounter limitter, IScheduler sched = null)
        {
            // Use the default scheduler.
            sched = sched ?? Scheduler.Default;

            // Create an observable that will track the various things that go wrong
            return Observable.Create<U>(o =>
            {
                var cancel = new CancellationDisposable(); // Monitor a cancel that comes along.
                var queue = new ConcurrentQueue<Notification<T>>(); // Everything that comes in get queued up as a materialized item

                Notification<U> limitSequenceError = null; // See failure symantics above
                Notification<T> sourceSequenceEndCondition = null;
                int pendingAccepts = 0;
                var subscription = source.Materialize()
                    .Where(_ => !cancel.Token.IsCancellationRequested)
                    .SelectMany(v => Observable.FromAsync(() => limitter.WaitAsync(cancel.Token)).Select(_ => v))
                    .Subscribe(n =>
                    {
                        Interlocked.Increment(ref pendingAccepts);
                        queue.Enqueue(n);
                        sched.Schedule(() =>
                        {
                            try
                            {
                                Notification<T> notification;
                                queue.TryDequeue(out notification);
                                if (notification.Kind == NotificationKind.OnNext || notification.Kind == NotificationKind.OnError)
                                {
                                    var sub = new Subject<T>();
                                    var seq = limitedSequence(sub).Materialize().Subscribe(
                                        result =>
                                        {
                                            // Did an error happen, or a completion happen before the source completed?
                                            // Ignore the completion - that is "normal".
                                            if (result.Kind == NotificationKind.OnError && limitSequenceError != null)
                                            {
                                                limitSequenceError = result;
                                            }
                                            else if (result.Kind == NotificationKind.OnNext)
                                            {
                                                result.Accept(o);
                                            }
                                        },
                                        () =>
                                        {
                                            // Our finally for this one. If this is the last one to get cleaned up, then we
                                            // move on.
                                            limitter.Release();
                                            if (Interlocked.Decrement(ref pendingAccepts) == 0)     // try to go lock-free a long a possible
                                            {
                                                lock (queue)    // take the queue as gate (only one thread should accept final notification)
                                                {
                                                    if (pendingAccepts == 0)
                                                    {
                                                        if (limitSequenceError != null)
                                                        {
                                                            // Somethign went wrong, terminate early!
                                                            limitSequenceError.Accept(o);
                                                            limitSequenceError = null;
                                                        }
                                                        else if (sourceSequenceEndCondition != null)
                                                        {
                                                            // We are done, so terminate.
                                                            o.OnCompleted();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    );
                                    notification.Accept(sub);
                                    sub.OnCompleted();
                                }
                                else
                                {
                                    // The sequence has terminated "normally".
                                    Debug.Assert(sourceSequenceEndCondition == null);
                                    sourceSequenceEndCondition = notification;
                                    if (Interlocked.Decrement(ref pendingAccepts) == 0)
                                    {
                                        lock (queue)
                                        {
                                            if (pendingAccepts == 0)
                                            {
                                                o.OnCompleted();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("Erorr on LimitGlobally: no error should ever make it here: {0}", e.Message);
                            }
                        });
                    });
                return new CompositeDisposable(cancel, subscription);
            });
        }
    }
}
