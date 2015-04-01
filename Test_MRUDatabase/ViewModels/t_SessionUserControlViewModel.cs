using IWalker.DataModel.Interfaces;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Reactive.Linq;

namespace Test_MRUDatabase.ViewModels
{
    /// <summary>
    /// Simple tests to see if the logic in the session VM works.
    /// </summary>
    [TestClass]
    public class t_SessionUserControlViewModel
    {
        [TestMethod]
        public void NormalSession()
        {
            var mtng = new dummyMeeting();
            var sVM = new SessionUserControlViewModel(mtng.Sessions[0], Observable.Empty<ISession[]>());
            var j = sVM.IsProperTitledSession;

            Assert.IsTrue(sVM.IsProperTitledSession);
        }

        [TestMethod]
        public void FakeSession()
        {
            var mtng = new dummyMeeting();
            (mtng.Sessions[0] as dummySession).Title = "yoman";
            var sVM = new SessionUserControlViewModel(mtng.Sessions[0], Observable.Empty<ISession[]>());
            var j = sVM.IsProperTitledSession;

            Assert.IsFalse(sVM.IsProperTitledSession);
        }

        [TestMethod]
        public void SessionTitle()
        {
            var mtng = new dummyMeeting();
            (mtng.Sessions[0] as dummySession).Title = "a grand session";
            var sVM = new SessionUserControlViewModel(mtng.Sessions[0], Observable.Empty<ISession[]>());
            var j = sVM.Title;

            Assert.AreEqual("a grand session", sVM.Title);

        }
    }
}
