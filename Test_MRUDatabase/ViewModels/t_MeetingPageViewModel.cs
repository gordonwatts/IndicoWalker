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
using System;
using System.Reactive;
using System.Threading.Tasks;
using Windows.Foundation;
using IWalker.DataModel.Inidco;
using Splat;
using Newtonsoft.Json;
using System.Collections.Specialized;

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
            Locator.CurrentMutable.Register(() => new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
            }, typeof(JsonSerializerSettings), null);
            Locator.CurrentMutable.RegisterConstant(new dummyMRUDB(), typeof(IMRUDatabase));

        }

        [TestMethod]
        public async Task TitleOnce()
        {
            var meeting = MeetingHelpers.CreateMeeting();
            var mvm = new MeetingPageViewModel(null, meeting);

            // The first value is empty as everything gets itself setup for data binding.
            var s = await mvm.WhenAny(x => x.MeetingTitle, x => x.Value)
                .Skip(1)
                .FirstAsync();

            Assert.AreEqual(s, "Meeting1");
        }

        [TestMethod]
        public async Task LocalMeetingFetch()
        {
            // Check the local test harness isn't having trouble.
            var m = new dummyMeetingRef();

            var meetingRightAway = await m.GetMeeting();
            Assert.IsNotNull(meetingRightAway);
        }

        [TestMethod]
        public async Task GetMeetingOnce()
        {
            // Brand new meeting fetch
            var meeting = MeetingHelpers.CreateMeeting();
            var mvm = new MeetingPageViewModel(null, meeting);

            // Wait for something to happen to the talks...
            var s = await mvm.Talks.Changed
                .FirstAsync();

            Assert.IsNotNull(s);
           
            Assert.AreEqual(1, mvm.Talks.Count);
            Assert.AreEqual(1, meeting.NumberOfTimesFetched);
        }

        [TestMethod]
        public async Task GetMeetingFromCache()
        {
            // Install the meeting in the cache.
            var meeting = MeetingHelpers.CreateMeeting();
            await BlobCache.UserAccount.InsertObject(meeting.AsReferenceString(), await meeting.GetMeeting()).FirstAsync();

            // Go grab the meeting now. It should show up twice.
            var mvm1 = new MeetingPageViewModel(null, meeting);
            var s = await mvm1.Talks.Changed
                .Skip(1)
                .Timeout(TimeSpan.FromMilliseconds(1000), Observable.Empty<NotifyCollectionChangedEventArgs>())
                .LastAsync();

            Assert.AreEqual(1, mvm1.Talks.Count);
            Assert.AreEqual(2, meeting.NumberOfTimesFetched);
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
            return Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Exception e = null;
                var t = new TestScheduler().With(async sched =>
                {
                    try
                    {
                        await action(sched);
                    }
                    catch (Exception exp)
                    {
                        e = exp;
                    }
                });
                t.Wait();
                if (e != null)
                    throw e;
            });
        }


        public async Task TestHarnessNoUI(Func<TestScheduler, Task> action)
        {
            Exception e = null;
            var t = new TestScheduler().With(async sched =>
            {
                try
                {
                    await action(sched);
                }
                catch (Exception exp)
                {
                    e = exp;
                }
            });
            await t;
            if (e != null)
                throw e;
        }

        /// <summary>
        /// Dummy DB to keep the VM happy.
        /// </summary>
        class dummyMRUDB : IMRUDatabase
        {
            public Task MarkVisitedNow(IMeeting meeting)
            {
                return Task.Factory.StartNew(() => { });
            }
        }

    }
}
