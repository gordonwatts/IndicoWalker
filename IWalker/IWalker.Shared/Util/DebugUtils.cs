using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;
using System.Diagnostics;

namespace IWalker.Util
{
    public static class DebugUtils
    {
        /// <summary>
        /// Helper to write a debug message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IObservable<T> WriteLine<T>(this IObservable<T> source, string message, params object[] args)
        {
            return source.Do(x => Debug.WriteLine(message, args));
        }
    }
}
