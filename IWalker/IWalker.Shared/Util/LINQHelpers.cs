using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace IWalker.Util
{
    static class LINQHelpers
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
            public LimitGlobalCounter(int maxCount)
            {

            }
        }

        public static IObservable<U> LimitGlobally<T,U> (this IObservable<T> source, Func<IObservable<T>, IObservable<U>> limitedSequence, int maxCount, IScheduler sched = null)
        {
            var counter = new LimitGlobalCounter(maxCount);
            return source.LimitGlobally(limitedSequence, counter, sched);
        }

        public static IObservable<U> LimitGlobally<T, U>(this IObservable<T> source, Func<IObservable<T>, IObservable<U>> limitedSequence, LimitGlobalCounter counter, IScheduler sched = null)
        {
            return limitedSequence(source);
        }
    }
}
