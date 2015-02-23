using IWalker.DataModel.Interfaces;
using IWalker.Util;
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
        public ITalk Talk { get; set; }

        /// <summary>
        /// Init with the various items for a talk.
        /// </summary>
        /// <param name="t"></param>
        public TalkUserControlViewModel(ITalk t)
        {
            this.Talk = t;
#if WINDOWS_APP
            var timeSpan = new TimePeriod(t.StartTime, t.EndTime);
            _fileSlides = new Lazy<FileSlideListViewModel>(() => new FileSlideListViewModel(t.TalkFile, timeSpan));
#endif
        }

        /// <summary>
        /// Get the title (for the UI).
        /// </summary>
        public string Title { get { return Talk.Title; } }

        /// <summary>
        /// The talk file
        /// </summary>
        public FileUserControlViewModel File { get { return new FileUserControlViewModel(Talk.TalkFile); } }

        /// <summary>
        /// True if the file should be visible
        /// </summary>
        public bool HasValidMainFile
        {
            get
            {
                return Talk.TalkFile.IsValid;
            }
        }

#if WINDOWS_APP
        private readonly Lazy<FileSlideListViewModel> _fileSlides;
        public FileSlideListViewModel FileSlides { get { return _fileSlides.Value;}}
#endif
    }
}
