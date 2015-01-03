using IWalker.ViewModels;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Main page host
    /// </summary>
    class MainPageViewModel : ReactiveObject, IScreen
    {
        /// <summary>
        /// Return the routing state.
        /// </summary>
        public RoutingState Router { get; private set; }

        public MainPageViewModel(RoutingState state = null)
        {
            Router = state;

            // Go to the first page and get this show ion the road.
            Router.Navigate.Execute(new StartPageViewModel(this));
        }
    }
}
