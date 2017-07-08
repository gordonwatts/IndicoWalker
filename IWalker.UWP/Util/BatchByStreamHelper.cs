using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace IWalker.Util
{
    /// <summary>
    /// Helper class for batching on stream by another stream.
    /// </summary>
    public static class BatchByStreamHelper
    {
        /// <summary>
        /// Help with the batching
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <remarks>
        /// If the switch stream runs out first, we just use the last value.
        /// </remarks>
        private class BatchByStreamSubject<S, T> : ISubject<T, Tuple<S, IObservable<T>>>
        {
            private S _cache;
            public BatchByStreamSubject(IObservable<S> switchStream)
            {
                // Monitor the switch stream. We want to change as soon as we
                // are given an oportunity.
                switchStream
                    .DistinctUntilChanged()
                    .Subscribe(v =>
                    {
                        _cache = v;
                        if (_TSubject != null)
                        {
                            _TSubject.OnCompleted();
                            _TSubject = null;
                        }
                    },
                    err => OnError(err));
            }

            private Subject<Tuple<S, IObservable<T>>> _subject = new Subject<Tuple<S, IObservable<T>>>();

            /// <summary>
            /// The main source sequence is completed.
            /// </summary>
            public void OnCompleted()
            {
                _subject.OnCompleted();
            }

            /// <summary>
            /// If there is an error in either stream...
            /// </summary>
            /// <param name="error"></param>
            public void OnError(Exception error)
            {
                _subject.OnError(error);
            }

            private Subject<T> _TSubject = null;

            /// <summary>
            /// The real logic is here - split things up and pass them on.
            /// </summary>
            /// <param name="value"></param>
            public void OnNext(T value)
            {
                // Should we create a new item and send it on?
                if (_TSubject == null)
                {
                    if (_TSubject != null)
                        _TSubject.OnCompleted();

                    _TSubject = new Subject<T>();
                    _subject.OnNext(Tuple.Create(_cache, _TSubject as IObservable<T>));
                }

                // Send the T value on down.
                _TSubject.OnNext(value);
            }

            /// <summary>
            /// Subscribe to things in out output.
            /// </summary>
            /// <param name="observer"></param>
            /// <returns></returns>
            public IDisposable Subscribe(IObserver<Tuple<S, IObservable<T>>> observer)
            {
                return _subject.Subscribe(observer);
            }
        }


        /// <summary>
        /// Generate a stream of streams, each time switchSource changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="switchSource"></param>
        /// <returns></returns>
        public static IObservable<Tuple<S, IObservable<T>>> BatchByStream<S, T>(this IObservable<T> source, IObservable<S> switchSource)
        {
            var s = new BatchByStreamSubject<S, T>(switchSource);
            source.Subscribe(s);
            return s;
        }
    }
}
