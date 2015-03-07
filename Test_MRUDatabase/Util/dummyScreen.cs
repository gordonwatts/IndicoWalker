using ReactiveUI;

namespace Test_MRUDatabase.Util
{
    class dummyScreen : IScreen
    {
        public dummyScreen()
        {
            Router = new RoutingState();
        }
        public RoutingState Router { get; private set; }
    }
}
