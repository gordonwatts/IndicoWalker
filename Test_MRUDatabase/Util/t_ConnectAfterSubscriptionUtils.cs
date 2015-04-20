using IWalker.Util;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Test_MRUDatabase.Util
{
    [TestClass]
    public class t_ConnectAfterSubscriptionUtils
    {
        /// <summary>
        /// Experimenting to see how this works, in a very basic way.
        /// </summary>
        [TestMethod]
        public void HowDoseConnectedObservableWork()
        {
            bool wasRun = false;
            Debug.WriteLine("Starting");
            var obs = Observable.Return(10)
                .WriteLine("In the replay part")
                .Do(_ => wasRun = true)
                .Replay(1);
            Assert.IsFalse(wasRun);
            Debug.WriteLine("Doing to do the connect");
            obs.Connect();
            Assert.IsTrue(wasRun);
            Debug.WriteLine("Done with connect, now subscribe");
            obs.Subscribe(n => Debug.WriteLine("in the subscribe"));
        }

        /// <summary>
        /// Make sure the connect guy doesn't fire until connected.
        /// </summary>
        [TestMethod]
        public void HowDoseConnectedObservableWork2()
        {
            Debug.WriteLine("Starting");
            bool wasRun = false;
            var obs = Observable.Return(10)
                .WriteLine("In the replay part")
                .Do(_ => wasRun = true)
                .Replay(1).ConnectAfterSubscription();
            Debug.WriteLine("now subscribe");
            Assert.IsFalse(wasRun);
            obs.Subscribe(n => Debug.WriteLine("in the subscribe"));
            Assert.IsTrue(wasRun);
        }

    }
}
