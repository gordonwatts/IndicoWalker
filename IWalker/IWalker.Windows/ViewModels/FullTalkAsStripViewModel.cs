using IWalker.Util;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace IWalker.ViewModels
{
    /// <summary>
    /// The VM for a talk shown as a slide-show. We have a list of slides,
    /// which are shown at full screen and scrollable, etc., and you can move
    /// around via keys and other things that are input.
    /// </summary>
    public class FullTalkAsStripViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// The list of PDF pages that we are showing
        /// </summary>
        public ReactiveList<PDFPageViewModel> Pages { get; private set; }

        /// <summary>
        /// Fires with a new page number when there is some reason to move to that page
        /// </summary>
        public IObservable<int> MoveToPage { get; private set; }

        /// <summary>
        /// The subject that we use to tell the UI to move around.
        /// </summary>
        private ISubject<int> _moveToPage;

        /// <summary>
        /// Hold onto how many pages there are in this document.
        /// </summary>
        private uint _numberPages;
        private ReplaySubject<Unit> _loaded;

        /// <summary>
        /// Request to go forward one page. The argument should be the current page that is
        /// up on the screen.
        /// </summary>
        public ReactiveCommand<object> PageForward { get; private set; }

        /// <summary>
        /// Fire this to have the UI advance a page. The current page is an argument.
        /// </summary>
        public ReactiveCommand<object> PageBack { get; private set; }

        /// <summary>
        /// Move to a particular page
        /// </summary>
        public ReactiveCommand<object> PageMove { get; private set; }

        /// <summary>
        /// Get everything setup to show the PDF document
        /// </summary>
        /// <param name="docSequence"></param>
        /// <param name="initialPage">The page that should be shown when we start up. Zero indexed</param>
        /// <param name="screen">The screen that hosts everything (routing!)</param>
        public FullTalkAsStripViewModel(IScreen screen, PDFFile file)
        {
            Debug.Assert(file != null);
            Debug.Assert(screen != null);

            HostScreen = screen;

            // We basically re-set each time a new file comes down from the top.

            Pages = new ReactiveList<PDFPageViewModel>();

            var pageSizeChanged = file.WhenAny(x => x.NumberOfPages, x => x.Value)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler);

            var b = from newPageLength in pageSizeChanged
                    let np = Pages.Count
                    from allpages in CreateNPages(newPageLength - np, np, file)
                    select new {
                        numPages = newPageLength,
                        freshPages = allpages
                    };

            b
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(info => SetNPages(info.numPages, info.freshPages));

            // Page navigation. Make sure things are clean and we don't over-burden the UI before
            // we pass the info back to the UI!
            _moveToPage = new ReplaySubject<int>(1);
            _loaded = new ReplaySubject<Unit>(1);
            MoveToPage = _moveToPage
                .CombineLatest(_loaded, (p, _) => p)
                .Select(scrubPageIndex)
                .DistinctUntilChanged();

            PageForward = ReactiveCommand.Create();
            PageForward
                .Cast<int>()
                .Select(pn => pn + 1)
                .Subscribe(_moveToPage);

            PageBack = ReactiveCommand.Create();
            PageBack
                .Cast<int>()
                .Select(pn => pn - 1)
                .Subscribe(_moveToPage);

            PageMove = ReactiveCommand.Create();
            PageMove
                .Cast<int>()
                .Subscribe(_moveToPage);
        }

        /// <summary>
        /// Create n pages, as requested, and prepare them for the UI.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<PDFPageViewModel[]> CreateNPages(int n, int start_index, PDFFile file)
        {
            if (n <= 0)
            {
                return new PDFPageViewModel[0];
            }

            // Create and initialize pages
            var newPages = Enumerable.Range(start_index, n).Select(i => new PDFPageViewModel(file.GetPageStreamAndCacheInfo(i))).ToArray();
            var sequenceOfPages = newPages.Select(p => p.LoadSize());
            foreach (var seq in sequenceOfPages)
            {
                var r = await seq.ToArray();
            }

            return newPages;
        }

        /// <summary>
        /// Configure as n pages
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private void SetNPages(int n, PDFPageViewModel[] newPages)
        {
            // Add pages...
            Pages.AddRange(newPages);

            // And then take them away...
            while (Pages.Count > n)
            {
                Pages.RemoveAt(Pages.Count - 1);
            }

            // Record what we've done for anyone else that is waiting on us.
            _numberPages = (uint)n;
            _loaded.OnNext(default(Unit));
        }

        /// <summary>
        /// Make sure we don't go too far back or too far forward when we request a new page.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private int scrubPageIndex(int page)
        {
            if (page < 0)
                return 0;
            if (page >= _numberPages)
                return (int)_numberPages - 1;
            return page;
        }

        /// <summary>
        /// Called to navigate to this page. This is a short-cut so that others
        /// who have access to us can load us without having to know the router, etc.
        /// </summary>
        internal void LoadPage(int pageNumber)
        {
            HostScreen.Router.Navigate.Execute(this);
            _moveToPage.OnNext(pageNumber);
        }

        /// <summary>
        /// The host screen
        /// </summary>
        public IScreen HostScreen { get; private set; }

        /// <summary>
        /// The URL so we know where we stand
        /// </summary>
        public string UrlPathSegment
        {
            get { return "/FullTalkAsStrip"; }
        }
    }
}
