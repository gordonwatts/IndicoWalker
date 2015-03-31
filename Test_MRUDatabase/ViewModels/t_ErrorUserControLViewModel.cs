using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Reactive.Subjects;

namespace Test_MRUDatabase.ViewModels
{
    /// <summary>
    /// Test out the errors
    /// </summary>
    [TestClass]
    public class t_ErrorUserControLViewModel
    {
        [TestMethod]
        public void PlainCTor()
        {
            var errors = new Subject<Exception>();
            var e = new ErrorUserControlViewModel(errors);
        }

        [TestMethod]
        public void SeeErrorWhenNothingThere()
        {
            var errors = new Subject<Exception>();
            var e = new ErrorUserControlViewModel(errors);
            var dummy = e.ErrorSeen;
            var errorString = "";
            e.DisplayErrors.Subscribe(msg => errorString = msg);
            e.ViewRequest.Execute(null);
            Assert.AreEqual("", errorString);
        }

        [TestMethod]
        public void NoErrorWhenSendOne()
        {
            var errors = new Subject<Exception>();
            var e = new ErrorUserControlViewModel(errors);
            var dummy = e.ErrorSeen;
            var errorString = "";
            e.DisplayErrors.Subscribe(msg => errorString = msg);
            errors.OnNext(new ArgumentException("bogus"));
            Assert.AreEqual("", errorString);
        }

        [TestMethod]
        public void ErrorWhenSendOne()
        {
            var errors = new Subject<Exception>();
            var e = new ErrorUserControlViewModel(errors);
            var dummy = e.ErrorSeen;
            var errorString = "";
            e.DisplayErrors.Subscribe(msg => errorString = msg);
            errors.OnNext(new ArgumentException("bogus"));
            e.ViewRequest.Execute(null);
            Assert.AreEqual("bogus", errorString);
        }

        [TestMethod]
        public void ErrorWhenSendOneNoActivateErrorSeen()
        {
            var errors = new Subject<Exception>();
            var e = new ErrorUserControlViewModel(errors);
            var errorString = "";
            e.DisplayErrors.Subscribe(msg => errorString = msg);
            errors.OnNext(new ArgumentException("bogus"));
            e.ViewRequest.Execute(null);
            Assert.AreEqual("bogus", errorString);
        }

        [TestMethod]
        public void LastErrorWhenSendTwo()
        {
            var errors = new Subject<Exception>();
            var e = new ErrorUserControlViewModel(errors);
            var dummy = e.ErrorSeen;
            var errorString = "";
            e.DisplayErrors.Subscribe(msg => errorString = msg);
            errors.OnNext(new ArgumentException("bogus"));
            errors.OnNext(new ArgumentException("help"));
            e.ViewRequest.Execute(null);
            Assert.AreEqual("help", errorString);
        }
    }
}
