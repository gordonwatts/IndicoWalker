using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Class for talk view models.
    /// </summary>
    public class TalkUserControlViewModel : ReactiveObject
    {
        /// <summary>
        /// Hold onto the talk we need
        /// </summary>
        private ITalk _talk;

        /// <summary>
        /// Init with the various items for a talk.
        /// </summary>
        /// <param name="t"></param>
        public TalkUserControlViewModel(ITalk t)
        {
            this._talk = t;
#if WINDOWS_APP
            _fileSlides = new Lazy<FileSlideListViewModel>(() => new FileSlideListViewModel(t.TalkFile));
#endif
        }

        /// <summary>
        /// Get the title (for the UI).
        /// </summary>
        public string Title { get { return _talk.Title; } }

        /// <summary>
        /// The talk file
        /// </summary>
        public FileUserControlViewModel File { get { return new FileUserControlViewModel(_talk.TalkFile); } }

        /// <summary>
        /// True if the file should be visible
        /// </summary>
        public bool HasValidMainFile
        {
            get
            {
                return _talk.TalkFile.IsValid;
            }
        }

#if WINDOWS_APP
        private readonly Lazy<FileSlideListViewModel> _fileSlides;
        public FileSlideListViewModel FileSlides { get { return _fileSlides.Value;}}
#endif
    }
}
