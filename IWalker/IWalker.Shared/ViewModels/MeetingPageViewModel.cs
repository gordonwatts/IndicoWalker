﻿using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Windows.System;
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
            Sessions = new ReactiveList<SessionUserControlViewModel>();
            TalkFiles = new ReactiveList<FileUserControlViewModel>();

            // And start off a background guy to populate everything.
            LoadMeeting(mRef);
        }

#if false
        /// <summary>
        /// Return meetings loaded from the internet. We will continue sending them if the meeting
        /// is currently running (e.g. doing constant updates).
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns>Stream of meetings</returns>
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
            // Grab a copy of the meeting online.
            var firstMeeting = Observable.FromAsync(() => meeting.GetMeeting())
                .Catch(Observable.Empty<IMeeting>())
                .Publish();

            // Determine the two buffer times for this meeting, and create a "pulse" that will
            // fire at the required intervals during those buffered times.
            var laterUpdates = firstMeeting
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

            var meetingSequence = Observable.Merge(firstMeeting, laterUpdates);

            // TODO: why do we need this firstMeeting a published observable?
            firstMeeting.Connect();
            return meetingSequence;
        }
#endif

        /// <summary>
        /// Given a meeting, load the info. Since this is an asynchronous command, we have to schedule stuff off it.
        /// </summary>
        /// <param name="meeting"></param>
        private void LoadMeeting(IMeetingRef meeting)
        {
            Debug.WriteLine("Staring a new LoadMeeting.");
            // Fetch the guy from the local cache.
            StartMeetingUpdates = ReactiveCommand.Create();
            var ldrCmdReady = StartMeetingUpdates
                .Take(1)
                //.SelectMany(_ => Blobs.LocalStorage.GetAndFetchLatest(meeting.AsReferenceString(), () => MeetingLoader(meeting), null, DateTime.Now + Settings.CacheAgendaTime))
                .SelectMany(_ => Blobs.LocalStorage.GetAndFetchLatest(meeting.AsReferenceString(), () => meeting.GetMeeting(), null, DateTime.Now + Settings.CacheAgendaTime))
                .CatchAndSwallowIfAfter(1, (Exception e) => MeetingLoadFailed(meeting))
                .Publish();

            ldrCmdReady
                .Select(m => m.Title)
                .ToProperty(this, x => x.MeetingTitle, out _title, "", RxApp.MainThreadScheduler);

            ldrCmdReady
                .Select(m => m.StartTime.ToString(@"M\/d\/yyyy h\:mm tt") + " - " + m.EndTime.ToString(@"h\:mm tt") + " (" + (m.EndTime - m.StartTime).ToString(@"h\:mm") + " long)")
                .ToProperty(this, x => x.StartTime, out _startTime, "", RxApp.MainThreadScheduler);

            // The talks that are in the meeting header.
            ldrCmdReady
                .Select(m => m.AttachedFiles.OrderBy(mt => mt.DisplayName).ToArray())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(files => SetAsMeetingFiles(files));

            // We want to notify the user that the meeting has no talks. We need to turn on the "no talk" thing
            // and also switch off the "loading" (which is normally switched off after all the talks are loaded).
            var meetingIsEmpty = ldrCmdReady
                .Select(m => m.Sessions.SelectMany(s => s.Talks).Count() == 0 && m.Sessions.Length <= 1);

            meetingIsEmpty
                .ToProperty(this, x => x.MeetingIsEmpty, out _meetingIsEmpty, false, RxApp.MainThreadScheduler);

            meetingIsEmpty
                .Where(mcnt => mcnt)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_displayDayIndex => MeetingIsReadyForDisplay = true);

            var ldrSessions = ldrCmdReady
                .Select(m => m.Sessions)
                .Select(s => s == null ? new ISession[0] : s);

            // Multi-day meetings are a bit more complex. Here we take the sessions and extract all the days associated with them.
            // Day's is a stream of date/times.
            var days = from meetingSessions in ldrSessions
                       select (from s in meetingSessions
                               group s.StartTime by s.StartTime.DayOfYear).Select(sgroup => sgroup.First()).Select(dt => new DateTime(dt.Year, dt.Month, dt.Day)).ToArray();

            // Select the first item on the list of days.
            // Give it a reasonable initial value so when everything is wired up we don't get a crash.
            DisplayDayIndex = -1;
            Days = new ReactiveList<DateTime>();
            days
                //.Select(lst => lst.Select(dt => string.Format("{0} ({1})", dt.DayOfWeek, dt.ToString("dd MMMM"))).ToArray())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(ds =>
                {
                    Days.MakeListLookLike(ds);
                    if (DisplayDayIndex >= ds.Length)
                    {
                        DisplayDayIndex = ds.Length - 1;
                    }
                    if (ds.Length > 0 && DisplayDayIndex < 0)
                    {
                        DisplayDayIndex = 0;
                    }
                });

            // When we have a set of sessions, only display the day we want to show.
            var selectedByUserDay = this.ObservableForProperty(x => x.DisplayDayIndex)
                .Select(v => v.Value)
                .DistinctUntilChanged()
                .Select(index => index >= 0 && index < Days.Count ? Days[index] : new DateTime());
            var theDaysSessions = Observable.CombineLatest(ldrSessions, selectedByUserDay, (ses, day) => Tuple.Create(ses, day))
                .Select(x => x.Item1.Where(s => s.StartTime.DayOfYear == x.Item2.DayOfYear));

            // And prepare them for display
            theDaysSessions
                .Select(s => s.OrderBy(ss => ss.StartTime).ToArray())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(sessions => SetAsSessions(sessions, ldrSessions));

            // And mark this meeting as having been viewed by the user!
            var db = Locator.Current.GetService<IMRUDatabase>();
            Debug.Assert(db != null);
            ldrCmdReady
                .Subscribe(m => db.MarkVisitedNow(m));

            // Start everything off.
            ldrCmdReady.Connect();

            // If they want to see it in the browser
            OpenMeetingInBrowser = ReactiveCommand.Create();
            OpenMeetingInBrowser
                .Subscribe(async _ => await Launcher.LaunchUriAsync(new Uri(meeting.WebURL)));
        }

        /// <summary>
        /// The meeting load failed for some reason. Offline and no cache, most likely. So we need to display something.
        /// </summary>
        /// <param name="meeting"></param>
        /// <returns></returns>
        private IObservable<IMeeting> MeetingLoadFailed(IMeetingRef meeting)
        {
            return Observable.Return(true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => new MessageDialog("Unable to connect to the meeting server. Either you are offline, or it doesn't exist."))
                .SelectMany(d => d.ShowAsync())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(_ => Locator.Current.GetService<RoutingState>().NavigateBack.Execute(null))
                .Where(_ => false)
                .Select(_ => (IMeeting)null);
        }

        /// <summary>
        /// Given the talk list, make our current list look like it.
        /// We integrate the current talks into the current list.
        /// </summary>
        /// <param name="sessions"></param>
        private void SetAsSessions(ISession[] sessions, IObservable<ISession[]> ldrSessions)
        {
            Debug.WriteLine("Setting up display for {0} sessions.", sessions.Length);

            Sessions.MakeListLookLike(sessions,
                (oItem, dItem) => oItem.Id == dItem.Id && oItem.StartTime == dItem.StartTime,
                dItem => new SessionUserControlViewModel(dItem, ldrSessions, sessions.Length == 1)
                );
            Debug.WriteLine("  Display now contains {0} Sessions.", Sessions.Count);

            // Make sure that that propagates out - because we are ready to show everything now.
            MeetingIsReadyForDisplay = true;
        }

        /// <summary>
        /// Given the talk list, make our current list look like it.
        /// We integrate the current talks into the current list.
        /// </summary>
        /// <param name="sessions"></param>
        private void SetAsMeetingFiles(IFile[] files)
        {
            TalkFiles.MakeListLookLike(files,
                (oItem, dItem) => oItem.File.UniqueKey == dItem.UniqueKey,
                dItem => new FileUserControlViewModel(dItem)
                );
        }

        /// <summary>
        /// Start the automatic meeting update process, including the one when this VM is initially loaded.
        /// </summary>
        /// <remarks>This is only done on a time-table. Re-firing this after it is fired once will not cause the update to re-trigger unless the time interval has been reached.</remarks>
        public ReactiveCommand<object> StartMeetingUpdates { get; private set; }
        
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
        /// Goes true when we have data to populate all the fields of
        /// the meeting.
        /// </summary>
        /// <remarks>Do not access via the backing property! The Changed event won't be raised.</remarks>
        public bool MeetingIsReadyForDisplay
        {
            get { return _meetingIsReady; }
            private set { this.RaiseAndSetIfChanged(ref _meetingIsReady, value); }
        }
        private bool _meetingIsReady = false;

        /// <summary>
        /// The start time.
        /// </summary>
        public string StartTime
        {
            get { return _startTime.Value; }
        }
        private ObservableAsPropertyHelper<string> _startTime;

        /// <summary>
        /// Get a bool indicating the meeting has no talks in it.
        /// </summary>
        public bool MeetingIsEmpty
        {
            get { return _meetingIsEmpty.Value; }
        }
        private ObservableAsPropertyHelper<bool> _meetingIsEmpty;

        /// <summary>
        /// Get the list of talks
        /// </summary>
        public ReactiveList<SessionUserControlViewModel> Sessions { get; private set; }

        /// <summary>
        /// Get the list of talks that are attached to the header of this meeting
        /// </summary>
        public ReactiveList<FileUserControlViewModel> TalkFiles { get; private set; }

        /// <summary>
        /// A list of the days this meeting covers for multi-day meetings
        /// </summary>
        public ReactiveList<DateTime> Days { get; private set; }

        /// <summary>
        /// The day/date we should be displaying.
        /// </summary>
        public int DisplayDayIndex
        {
            get { return _displayDayIndex; }
            set { this.RaiseAndSetIfChanged(ref _displayDayIndex, value); }
        }
        private int _displayDayIndex;

        /// <summary>
        /// Where we will be located.
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/meeting"; }
        }


        /// <summary>
        /// Fire to open the meeting in the browser.
        /// </summary>
        public ReactiveCommand<object> OpenMeetingInBrowser { get; private set; }
    }
}
