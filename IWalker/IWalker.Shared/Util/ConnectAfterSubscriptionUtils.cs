using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace IWalker.Util
{
    /// <summary>
    /// Helper classes to deal with connected observables
    /// </summary>
    public static class ConnectAfterSubscriptionUtils
    {
        /// <summary>
        /// After the first subscription this will connect the observable. It is never disconnected
        /// (instead relying on the GC to find the cycles and remove them).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<T> ConnectAfterSubscription<T>(this IConnectableObservable<T> source)
        {
            return new ConnectAfterSubscriptionUtil<T>(source);
        }

        /// <summary>
        /// Helper class to forward the subscription, and make sure that we connect after
        /// subscribing.
        /// </summary>
        /// <typeparam name="T">The type of the stream</typeparam>
        private class ConnectAfterSubscriptionUtil<T> : IObservable<T>
        {
            /// <summary>
            /// Cache the source for later subscription.
            /// </summary>
            private IConnectableObservable<T> _source;

            /// <summary>
            /// Get setup, memorizing the subscription.
            /// </summary>
            /// <param name="source"></param>
            public ConnectAfterSubscriptionUtil(IConnectableObservable<T> source)
            {
                this._source = source;
            }

            /// <summary>
            /// If we've seen the first subscription.
            /// </summary>
            bool _firstSubscription = true;

            /// <summary>
            /// Subscribe to our cached sequence. If this is the first one, then do
            /// the connect.
            /// </summary>
            /// <param name="observer"></param>
            /// <returns></returns>
            public IDisposable Subscribe(IObserver<T> observer)
            {
                var disp = _source.Subscribe(observer);
                lock (this)
                {
                    if (_firstSubscription)
                    {
                        _source.Connect();
                        _firstSubscription = false;
                    }
                }
                return disp;
            }
        }

    }
}
