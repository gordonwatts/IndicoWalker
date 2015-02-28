using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Windows.UI.Popups;

namespace IWalker.ViewModels
{
    /// <summary>
    /// A single meeting. A meeting has a single session, a conference has multiple sessions.
    /// </summary>
    public class MeetingPageViewModel : ReactiveObject, IRoutableViewModel
    {
        public MeetingPageViewModel(IScreen hs, IMeetingRef mRef)
        {
            // Initial default values
            HostScreen = hs;
            Talks = new ReactiveList<TalkUserControlViewModel>();

            // And start off a background guy to populate everything.
            LoadMeeting(mRef);
        }

        /// <summary>
        /// Return a stream of meetings, as needed.
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        /// <remarks>
        /// The algorithm for this:
        /// 1. Always fetch the meeting at least one time.
        /// 2. If we are in the outer buffer region, check once an hour
        /// 3. If we are in the inner buffer region, then check every 5 minutes.
        /// 
        /// Inner buffer region: meeting start and end times, with 30 minutes before the meeting, and 2 hours after the meeting.
        /// Outer buffer region: 1 day before the meeting to 1 day after the meeting.
        /// </remarks>
        private IObservable<IMeeting> MeetingLoader(IMeetingRef meeting)
        {
            // Get the first copy of the meeting.
            // We are careful on exceptions. If we can't find it from the cache or online, then
            // we are in some trouble: we can't load the meeting. So we need to surface an error
            // to be reported to the user.
            var firstMeeting = Observable.FromAsync(() => meeting.GetMeeting())
                .Catch(
                    Blobs.LocalStorage.GetObject<IMeeting>(meeting.AsReferenceString())
                    )
                .Publish();

            // Determine the two buffer times for this meeting, and create a "pulse" that will
            // fire at the required intervals during those buffered times.
            var bufferTimes = firstMeeting
                .SelectMany(m =>
                {
                    var bufInner = new TimePeriod(m.StartTime - TimeSpan.FromMinutes(30), m.EndTime + TimeSpan.FromHours(2));
                    var bufOutter = new TimePeriod(m.StartTime - TimeSpan.FromDays(1), m.EndTime + TimeSpan.FromDays(1));

                    IObservable<Unit> reloadPulse;
                    if (!bufOutter.Contains(DateTime.Now))
                    {
                        // This meeting is way late or way early.
                        // Don't waste any time doing the loading.
                        reloadPulse = Observable.Empty<Unit>();
                        Debug.WriteLine("Not triggering re-fetches for the meeting");
                    }
                    else
                    {
                        // We do both inner and outer so that if someone leaves this up for a long
                        // time it will work will.
                        var outter = Observable.Timer(TimeSpan.FromMinutes(30))
                            .Where(_ => bufOutter.Contains(DateTime.Now))
                            .WriteLine("Getting meeting in outer buffer")
                            .Select(_ => default(Unit));
                        var inner = Observable.Timer(TimeSpan.FromMinutes(5))
                            .Where(_ => bufInner.Contains(DateTime.Now))
                            .WriteLine("Getting meeting in inner buffer")
                            .Select(_ => default(Unit));
                        reloadPulse = Observable.Merge(outter, inner);
                    }

                    // Now, use it to trigger the meetings.
                    return reloadPulse
                        .SelectMany(_ => Observable.FromAsync(() => meeting.GetMeeting()).Catch(Observable.Empty<IMeeting>()));
                });

            var meetingSequence = Observable.Merge(firstMeeting, bufferTimes);
            firstMeeting.Connect();
            return meetingSequence;
        }

        /// <summary>
        /// Given a meeting, load the info. Since this is an asynchronous command, we have to schedule stuff off it.
        /// </summary>
        /// <param name="meeting"></param>
        private void LoadMeeting(IMeetingRef meeting)
        {
            // Fetch the guy from the local cache. MeetingLoader will actually return a continuous stream
            // of updates (when there is a difference) if we are close to the meeting time.
            var ldrCmd = ReactiveCommand.Create();
            var ldrCmdReady = ldrCmd
                .SelectMany(_ => Blobs.LocalStorage.GetAndFetchLatest(meeting.AsReferenceString(), () => MeetingLoader(meeting), null, DateTime.Now + Settings.CacheAgendaTime))
                .Catch(MeetingLoadFailed(meeting))
                .Publish();

            ldrCmdReady
                .Select(m => m.Title)
                .ToProperty(this, x => x.MeetingTitle, out _title, "", RxApp.MainThreadScheduler);

            ldrCmdReady
                .Select(m => m.StartTime)
                .Select(dt => dt.ToString())
                .ToProperty(this, x => x.StartTime, out _startTime, "", RxApp.MainThreadScheduler);

            ldrCmdReady
                .WriteLine("Got a new meeting.")
                .Select(m => m.Sessions)
                .Where(s => s != null && s.Length > 0)
                .Select(s => s[0])
                .Where(t => t.Talks != null)
                .Select(t => t.Talks)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(talks => SetAsTalks(talks));

            // And mark this meeting as having been viewed by the user!
            var db = Locator.Current.GetService<IMRUDatabase>();
            Debug.Assert(db != null);
            ldrCmdReady
                .Subscribe(m => db.MarkVisitedNow(m));

            // Start everything off.
            ldrCmdReady.Connect();
            ldrCmd.Execute(null);
        }

        /// <summary>
        /// The meeting load failed for some reason. Offline and no cache, most likely. So we need to display something.
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        private IObservable<IMeeting> MeetingLoadFailed(IMeetingRef meeting)
        {
            var d = new MessageDialog("Unable to contact the meeting server. Either you are offline, or it doesn't exist.");
            return Observable.FromAsync(_ => d.ShowAsync().AsTask())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(_ => HostScreen.Router.NavigateBack.Execute(null))
                .Where(_ => false)
                .Select(_ => (IMeeting)null);
        }

        /// <summary>
        /// Given the talk list, make our current list look like it.
        /// We integrate the current talks into the current list.
        /// </summary>
        /// <param name="talks"></param>
        private void SetAsTalks(ITalk[] talks)
        {
            Debug.WriteLine("Setting up display for {0} talks.", talks.Length);
            // Normally, we'd want to use this. However, this causes a total list reset,
            // and often the below code will not touch the list at all.
            //using (Talks.SuppressChangeNotifications())
            {
                Talks.MakeListLookLike(talks,
                    (oItem, dItem) => oItem.Talk.Equals(dItem),
                    dItem => new TalkUserControlViewModel(dItem)
                    );
            }
            Debug.WriteLine("  Display now contains {0} talks.", Talks.Count);
        }

        /// <summary>
        /// Track the home screen.
        /// </summary>
        public IScreen HostScreen { get; private set; }

        /// <summary>
        /// The meeting title
        /// </summary>
        public string MeetingTitle
        {
            get { return _title.Value; }
        }
        private ObservableAsPropertyHelper<string> _title;

        /// <summary>
        /// The start time.
        /// </summary>
        public string StartTime
        {
            get { return _startTime.Value; }
        }
        private ObservableAsPropertyHelper<string> _startTime;

        /// <summary>
        /// Get the list of talks
        /// </summary>
        public ReactiveList<TalkUserControlViewModel> Talks { get; private set; }

        /// <summary>
        /// Where we will be located.
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/meeting"; }
        }
    }
}
