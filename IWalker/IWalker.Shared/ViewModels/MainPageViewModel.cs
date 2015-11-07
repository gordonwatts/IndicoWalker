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

        public void MoveAlong()
        {
            // Go to the first page and get this show ion the road.
            //Router.Navigate.Execute(new StartPageViewModel(this));
            Router.Navigate.Execute(new FirstRunViewModel(this));
        }
    }
}
