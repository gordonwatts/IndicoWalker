using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

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
    }
}
