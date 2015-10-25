using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using System.Linq;
using System;
using System.Collections;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Class for talk view models.
    /// </summary>
    public class TalkUserControlViewModel : ReactiveObject
    {
        /// <summary>
        /// Get the talk we are showing. Used by others to see if we are "the same" when new updates show up.
        /// </summary>
        public ITalk Talk { get; private set; }

        /// <summary>
        /// The list of material associated with the talk, organized by name (so duplicate files get grouped in the UI).
        /// </summary>
        public ReactiveList<TalkFileCollectionUserControlViewModel> TalkFiles { get; private set; }

        /// <summary>
        /// List of any subtalks that this guy might have. Normally this is zero.
        /// </summary>
        public ReactiveList<TalkUserControlViewModel> SubTalks { get; private set; }

        /// <summary>
        /// Init with the various items for a talk.
        /// </summary>
        /// <param name="t"></param>
        public TalkUserControlViewModel(ITalk t)
        {
            Title = t.Title;
            Talk = t;

            // Split the talk out by file names, and put them out to be displayed everywhere.
            // We screen out everything here that doesn't have a good file type (one of the requirements of IsValid for now).
            // TODO: fix up so we can deal with links to other material.
            var byName = from f in t.AllTalkFiles
                         where f.IsValid
                         group f by f.DisplayName;

            TalkFiles = new ReactiveList<TalkFileCollectionUserControlViewModel>();
            TalkFiles.AddRange(byName.Select(fs => new TalkFileCollectionUserControlViewModel(fs.ToArray(), t)));

            SubTalks = new ReactiveList<TalkUserControlViewModel>(t.SubTalks.Select(st => new TalkUserControlViewModel(st)));
        }

        /// <summary>
        /// Get the title (for the UI).
        /// </summary>
        public string Title { get; private set; }
    }
}
