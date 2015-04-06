using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace IWalker.Util
{
    /// <summary>
    /// Some helper functions for dealing with UI issues and wiring up objects.
    /// </summary>
    static class UIHelpers
    {
        /// <summary>
        /// The button's click event is wired up to the navagateback experience from
        /// the App's default router.
        /// </summary>
        /// <param name="b"></param>
        public static void WireAsBackButton(this Button b)
        {
            var router = Locator.Current.GetService<RoutingState>();
            b.Click += (s, args) => router.NavigateBack.Execute(null);
        }
    }
}
