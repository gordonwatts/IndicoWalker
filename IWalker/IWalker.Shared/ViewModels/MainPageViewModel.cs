using IWalker.Util;
using ReactiveUI;

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
        }

        /// <summary>
        /// Open up a splash screen if this is the first time we've run.
        /// </summary>
        public void MoveAlong()
        {
            // Go to the first page and get this show ion the road.
            if (Settings.FirstTimeWeHaveRun)
            {
                Router.Navigate.Execute(new FirstRunViewModel(this));
            }
            else
            {
                Router.Navigate.Execute(new StartPageViewModel(this));
            }
        }
    }
}
