using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.ViewModels;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI.Testing;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Test_MRUDatabase.ViewModels
{
    /// <summary>
    /// Test the main meeting page
    /// </summary>
    [TestClass]
    public class t_MeetingPageViewModel
    {
        [TestInitialize]
        public void Setup()
        {
            BlobCache.ApplicationName = "Test_MRUDatabase";
            BlobCache.UserAccount.InvalidateAll();
            BlobCache.UserAccount.Flush();
        }

        [TestMethod]
        public async Task TitleOnce()
        {
            await TestHarness(async sched =>
            {
                var meeting = CreateMeeting();
                var mvm = new MeetingPageViewModel(null, meeting);

                string title = null;
                mvm.WhenAny(x => x.MeetingTitle, x => x.Value)
                    .Subscribe(t => title = t);

                await Task.Delay(10);
                Assert.AreEqual("Meeting 1", title);
            });
        }

        [TestMethod]
        public async Task GetMeetingOnce()
        {
            await TestHarness(async sched => {
                // Brand new meeting fetch

                var meeting = CreateMeeting();
                var mvm = new MeetingPageViewModel(null, meeting);

                await Task.Delay(10);
                Assert.AreEqual(1, mvm.Talks.Count);            
            });
        }

        /// <summary>
        /// Generate a meeting.
        /// </summary>
        /// <returns></returns>
        private IMeetingRef CreateMeeting()
        {
            return new dummyMeetingRef();
        }

        class dummyMeeting : IMeeting
        {
            public string Title
            {
                get { return "Meeting1"; }
            }

            public ISession[] Sessions
            {
                get { return new ISession[] { new dummySession() }; }
            }

            public DateTime StartTime
            {
                get { throw new NotImplementedException(); }
            }

            public string AsReferenceString()
            {
                throw new NotImplementedException();
            }
        }

        class dummySession : ISession
        {
            public ITalk[] Talks
            {
                get { return new ITalk[] { new dummyTalk() }; }
            }
        }

        class dummyTalk : ITalk
        {

            public string Title
            {
                get { return "talk 1"; }
            }

            public IFile TalkFile
            {
                get { return new dummyFile(); }
            }
        }

        class dummyFile : IFile
        {
            public bool IsValid
            {
                get { return true; }
            }

            public string FileType
            {
                get {return "pdf"; }
            }

            public string UniqueKey
            {
                get { return "talk1.pdf"; }
            }

            public Task<System.IO.StreamReader> GetFileStream()
            {
                throw new NotImplementedException();
            }

            public string DisplayName
            {
                get { return "talk1.pdf"; }
            }
        }



        /// <summary>
        /// A pretty simple dummy meeting.
        /// </summary>
        class dummyMeetingRef : IMeetingRef
        {
            public Task<IMeeting> GetMeeting()
            {
                return Task.Factory.StartNew(() => new dummyMeeting() as IMeeting);
            }

            public string AsReferenceString()
            {
                return "meeting";
            }
        }


        [TestMethod]
        public void GetMeetingFromCache()
        {
            // Make sure that the # of talks we get remains "good" when we
            // get from the cache.
            Assert.Inconclusive();
        }

        /// <summary>
        /// Helper to run in the UI thread.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IAsyncAction ExecuteOnUIThread(Windows.UI.Core.DispatchedHandler action)
        {
            return Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, action);
        }

        /// <summary>
        /// Run on both the test scheduler and the Main window thread.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IAsyncAction TestHarness(Func<TestScheduler, Task> action)
        {
            return Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                new TestScheduler().With(sched => action(sched));
            });
        }
    }
}
