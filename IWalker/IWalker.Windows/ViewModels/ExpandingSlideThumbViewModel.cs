using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using IWalker.Util;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Presents a button. When pressed, it expands so all the slides are shown as thumbnails.
    /// Only a limited number are allowed to be open at any time.
    /// </summary>
    public class ExpandingSlideThumbViewModel : ReactiveObject
    {
        /// <summary>
        /// When fired, it cause the strip of slides to be populated.
        /// </summary>
        public ReactiveCommand<object> ShowSlides { get; private set; }

        /// <summary>
        /// Fired to close all other slide shows.
        /// </summary>
        private static ReactiveCommand<object> _resetSlideShow = ReactiveCommand.Create();

        /// <summary>
        /// Set to null mostly, but when not, it contains the VM for the file slides.
        /// </summary>
        public FileSlideListViewModel TalkAsThumbs { get { return _talkAsThumbs.Value; } }
        private ObservableAsPropertyHelper<FileSlideListViewModel> _talkAsThumbs;

        /// <summary>
        /// Setup the links to run.
        /// </summary>
        public ExpandingSlideThumbViewModel(PDFFile downloader, TimePeriod talkTime)
        {
            // Showing the slides should generate it here, and nullify it everywhere else.
            _resetSlideShow = ReactiveCommand.Create();
            ShowSlides = ReactiveCommand.Create();

            ShowSlides
                .InvokeCommand(_resetSlideShow);
            var newThumbs = ShowSlides
                .Select(_ => new FileSlideListViewModel(downloader, talkTime));

            var noThumbs = _resetSlideShow
                .Select(_ => (FileSlideListViewModel)null);

            Observable.Merge(newThumbs, noThumbs)
                .ToProperty(this, x => x.TalkAsThumbs, out _talkAsThumbs, null, RxApp.MainThreadScheduler);

        }
    }
}
