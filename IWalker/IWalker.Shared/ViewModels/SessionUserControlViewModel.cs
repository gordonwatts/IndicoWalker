using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using IWalker.Util;

namespace IWalker.ViewModels
{
    /// <summary>
    /// VM to show a session. Mostly just a list of talks we are displaying.
    /// </summary>
    public class SessionUserControlViewModel : ReactiveObject
    {
        /// <summary>
        /// The list of talks
        /// </summary>
        public ReactiveList<TalkUserControlViewModel> Talks { get; private set; }

        /// <summary>
        /// The title for the session.
        /// </summary>
        public string Title
        {
            get { return _title.Value; }
        }
        private ObservableAsPropertyHelper<string> _title;

        /// <summary>
        /// Returns true if this is a session we should expect to be proper.
        /// </summary>
        public bool IsProperTitledSession
        {
            get { return _isProperTitledSession.Value; }
        }
        private ObservableAsPropertyHelper<bool> _isProperTitledSession;

        /// <summary>
        /// The unique Id of the session we are displaying.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Setup the VM to display the session. We are given an inital set of talks, as well as a session of updates
        /// for future talks.
        /// </summary>
        /// <param name="dItem"></param>
        /// <param name="ldrSessions"></param>
        public SessionUserControlViewModel(DataModel.Interfaces.ISession dItem, IObservable<DataModel.Interfaces.ISession[]> ldrSessions, bool isSingleSessionMeeting)
        {
            // Cache the ID. This will help with updates later on.
            Id = dItem.Id;

            // The stream of talks, which starts with our initial stuff, and then continues on will
            // anything that comes through the update stream.

            var inCommingSessionUpdates = Observable.Merge(
                Observable.Return(dItem),
                ldrSessions.SelectMany(ses => ses).Where(s => s.Id == Id)
                )
                .ObserveOn(RxApp.MainThreadScheduler);

            inCommingSessionUpdates
                .Select(s => s.Title)
                .ToProperty(this, x => x.Title, out _title, "");

            inCommingSessionUpdates
                .Select(s => !s.IsPlaceHolderSession && !isSingleSessionMeeting)
                .ToProperty(this, x => x.IsProperTitledSession, out _isProperTitledSession, true);

            Talks = new ReactiveList<TalkUserControlViewModel>();
            inCommingSessionUpdates
                .Select(s => s.Talks)
                .Subscribe(talks => SetTalks(talks));
        }

        /// <summary>
        /// Setup the talks
        /// </summary>
        /// <param name="talks"></param>
        private void SetTalks(DataModel.Interfaces.ITalk[] talks)
        {
                Talks.MakeListLookLike(talks,
                    (oItem, dItem) => oItem.Talk.Equals(dItem),
                    dItem => new TalkUserControlViewModel(dItem)
                    );
        }
    }
}
