using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;

namespace IWalker.Util
{
    /// <summary>
    /// Some simple helper classes for Rx stuff.
    /// </summary>
    public static class RxUtils
    {
        /// <summary>
        /// When an exceptions occurs before itemsToAllowThrowAfter items have passed, call the handler and append that sequence.
        /// If itemsToAllowThrowAfter items have passed, then swallow the sequence and terminate gracefully.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <param name="source"></param>
        /// <param name="itemsToAllowThrowAfter"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IObservable<TSource> CatchAndSwallowIfAfter<TSource, TException>(this IObservable<TSource> source, int itemsToAllowThrowAfter, Func<TException, IObservable<TSource>> handler) where TException : Exception
        {
            //return source.Catch(handler);
#if false
            var protectedSource = source.Publish();

            var beforeItems = protectedSource
                .Take(itemsToAllowThrowAfter)
                .Catch(handler);
            var afterItems = protectedSource
                .Skip(itemsToAllowThrowAfter)
                .Catch(Observable.Empty<TSource>());

            protectedSource.Connect();

            var r = Observable.Merge(beforeItems, afterItems).Publish();
            r.Connect();
            return r;
#endif
            int counter = 0;
            return source.Materialize()
                .Select(item =>
                {
                    if (item.Kind == NotificationKind.OnError)
                    {
                        if (counter >= itemsToAllowThrowAfter)
                        {
                            return Notification.CreateOnCompleted<TSource>();
                        }
                        else
                        {
                            return item;
                        }
                    }
                    else
                    {
                        counter++;
                        return item;
                    }
                })
                .Dematerialize()
                .Catch(handler);
        }
    }
}
