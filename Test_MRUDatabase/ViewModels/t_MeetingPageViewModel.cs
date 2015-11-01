using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using ReactiveUI.Testing;
using Splat;
using System;
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
        public async Task Setup()
        {
            BlobCache.ApplicationName = "Test_MRUDatabase";
            await Blobs.LocalStorage.InvalidateAll();
            await Blobs.LocalStorage.Flush();
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

            var bogus = mvm.MeetingTitle;
            mvm.StartMeetingUpdates.Execute(null);

            await TestUtils.SpinWaitAreEqual("Meeting1", () => mvm.MeetingTitle);
        }

        [TestMethod]
        public async Task MeetingReady()
        {
            var meeting = MeetingHelpers.CreateMeeting();
            var mvm = new MeetingPageViewModel(null, meeting);
            Assert.IsFalse(mvm.MeetingIsReadyForDisplay);

            var bogus = mvm.MeetingTitle;
            mvm.StartMeetingUpdates.Execute(null);

            await TestUtils.SpinWaitAreEqual(true, () => mvm.MeetingIsReadyForDisplay);
        }

        [TestMethod]
        public async Task LoadMeetingOnceAfterOneTrigger()
        {
            var meeting = MeetingHelpers.CreateMeeting();
            var mvm = new MeetingPageViewModel(null, meeting);

            var bogus = mvm.MeetingTitle;
            mvm.StartMeetingUpdates.Execute(null);

            await TestUtils.SpinWaitAreEqual("Meeting1", () => mvm.MeetingTitle);

            Assert.AreEqual(1, meeting.NumberOfTimesFetched);
        }

        [TestMethod]
        public async Task LoadMeetingOnceAfterTwoTrigger()
        {
            var meeting = MeetingHelpers.CreateMeeting();
            var mvm = new MeetingPageViewModel(null, meeting);

            var bogus = mvm.MeetingTitle;
            mvm.StartMeetingUpdates.Execute(null);

            await TestUtils.SpinWaitAreEqual("Meeting1", () => mvm.MeetingTitle);
            mvm.StartMeetingUpdates.Execute(null);

            await TestUtils.SpinWait(() => meeting.NumberOfTimesFetched != 1, 400, false);

            Assert.AreEqual(1, meeting.NumberOfTimesFetched);
        }

        [TestMethod]
        public async Task LocalMeetingFetch()
        {
            // Check the local test harness isn't having trouble.
            var m = new dummyMeetingRef();

            var meetingRightAway = await m.GetMeeting();
            Assert.IsNotNull(meetingRightAway);
        }

#if false
        [TestMethod]
        public async Task GetMeetingOnce()
        {
            // Brand new meeting fetch
            var meeting = MeetingHelpers.CreateMeeting();
            var mvm = new MeetingPageViewModel(null, meeting);

            // This line causes a hang. :(
            // Wait for something to happen to the talks...
            var s = await mvm.Talks.Changed
                .FirstAsync();

            Assert.IsNotNull(s);
           
            Assert.AreEqual(1, mvm.Talks.Count);
            Assert.AreEqual(1, meeting.NumberOfTimesFetched);
        }
#endif

#if false
        [TestMethod]
        public async Task CheckMeetingAgendaCached()
        {
            // Brand new meeting fetch
            var meeting = MeetingHelpers.CreateMeeting();
            var mvm = new MeetingPageViewModel(null, meeting);

            // TODO: This lien causes a hang in the test.
            // Wait for something to happen to the talks...
            var s = await mvm.Talks.Changed
                .FirstAsync();

            var m = await Blobs.LocalStorage.GetObject<IMeeting>(meeting.AsReferenceString()).FirstAsync();

            Assert.IsNotNull(m);
            Assert.AreEqual("Meeting 1", m.Title);
        }
#endif

#if false
        [TestMethod]
        public async Task GetMeetingFromCache()
        {
            // Install the meeting in the cache.
            var meeting = MeetingHelpers.CreateMeeting();
            await Blobs.LocalStorage.InsertObject(meeting.AsReferenceString(), await meeting.GetMeeting()).FirstAsync();

            // Go grab the meeting now. It should show up twice.
            var mvm = new MeetingPageViewModel(null, meeting);

            // TODO: This await never returns below. Find a way to look
            // for talks updated!
            // CLear, and then set
            await mvm.Talks.Changed.FirstAsync();
            await mvm.Talks.Changed.FirstAsync();

            // Clear and then set.
            await mvm.Talks.Changed.FirstAsync();
            await mvm.Talks.Changed.FirstAsync();

            Assert.AreEqual(1, mvm.Talks.Count);
            Assert.AreEqual(2, meeting.NumberOfTimesFetched);
        }
#endif

#if false
        // TODO: Just like the others, this guy can't run, because it hangs!
        [TestMethod]
        public async Task MeetingUpdated()
        {
            // Add a new talk when we have the third look (or so).
            var mr = new dummyMeetingChangerRef((meeting, count) =>
            {
                if (count > 1)
                {
                    var t = meeting.Sessions[0].Talks
                        .Concat(new ITalk[] { new dummyTalk() })
                        .ToArray();
                    var s = (meeting as dummyMeeting).Sessions[0] as dummySession;
                    s.Talks = t;
                }

                return meeting;
            });

            var m = await mr.GetMeeting();
            await Blobs.LocalStorage.InsertObject(mr.AsReferenceString(), m);
            Assert.AreEqual(1, m.Sessions[0].Talks.Length);

            // Go grab the meeting now. It should show up twice.
            // Since we are running with the test scheduler, we need to advance things, or nothing
            // will work!
            var mvm = new MeetingPageViewModel(null, mr);

            // First update:
            await mvm.Talks.Changed
                .FirstAsync();
            await mvm.Talks.Changed
                .FirstAsync();

            Assert.AreEqual(1, mr.Count);
            Assert.AreEqual(1, mvm.Talks.Count);

            await mvm.Talks.Changed
                .FirstAsync();
            await mvm.Talks.Changed
                .FirstAsync();
            await mvm.Talks.Changed
                .FirstAsync();
            Debug.WriteLine("About to check the #");
            Assert.AreEqual(2, mvm.Talks.Count);
        }
#endif

#if false
        [TestMethod]
        public async Task MeetingAutoUpdated()
        {
            // Add a new talk when we have the third look (or so).
            var mr = new dummyMeetingChangerRef((meeting, count) =>
            {
                var mMod = (meeting as dummyMeeting);
                mMod.StartTime = DateTime.Now - TimeSpan.FromMinutes(1);
                mMod.EndTime = DateTime.Now + TimeSpan.FromMinutes(10);

                return meeting;
            });

            var m = await mr.GetMeeting();
            await Blobs.LocalStorage.InsertObject(mr.AsReferenceString(), m);
            Assert.AreEqual(1, m.Sessions[0].Talks.Length);

            // Go grab the meeting now. It should show up twice.
            // Since we are running with the test scheduler, we need to advance things, or nothing
            // will work!
            var mvm = new MeetingPageViewModel(null, mr);

            // TODO: the below async doesn't work. Why not? Never returns.
            // First update:
            await mvm.Talks.Changed
                .FirstAsync();
            await mvm.Talks.Changed
                .FirstAsync();

            Assert.AreEqual(1, mr.Count);
            Assert.AreEqual(1, mvm.Talks.Count);

            await mvm.Talks.Changed
                .FirstAsync();
            await mvm.Talks.Changed
                .FirstAsync();
            await mvm.Talks.Changed
                .FirstAsync();
            Debug.WriteLine("About to check the #");
            Assert.AreEqual(2, mvm.Talks.Count);
        }
#endif

        class dummyMeetingChangerRef : IMeetingRef
        {
            public dummyMeetingChangerRef(Func<IMeeting, int, IMeeting> alterMeeting)
            {
                _callback = alterMeeting;
                Count = 0;
            }

            /// <summary>
            /// Get the meeting, allow a callback to mess with it.
            /// </summary>
            /// <returns></returns>
            public Task<IMeeting> GetMeeting()
            {
                return Task.Factory.StartNew<IMeeting>(() =>
                {
                    var m = new dummyMeeting();
                    Count++;
                    return _callback(m, Count);
                });
            }

            public string AsReferenceString()
            {
                return "meeting1";
            }

            private readonly Func<IMeeting, int, IMeeting> _callback;
            public int Count { get; set; }


            public string WebURL
            {
                get { throw new NotImplementedException(); }
            }
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
