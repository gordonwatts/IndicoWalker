﻿using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Data.Pdf;

namespace IWalker.ViewModels
{
    /// <summary>
    /// The VM for a full talk as a view model. We have a list of slides,
    /// which are shown at full screen and scrollable, etc.
    /// </summary>
    public class FullTalkAsStripViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// Our cache of the PDF document
        /// </summary>
        private PdfDocument _doc;

        /// <summary>
        /// The list of PDF pages that we are showing
        /// </summary>
        public ReactiveList<PDFPageViewModel> Pages { get; private set; }

        /// <summary>
        /// Will cause us to go back on in the stack.
        /// </summary>
        public ReactiveCommand<Unit> GoBack { get; private set; }

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
        /// <param name="doc"></param>
        /// <param name="initialPage">The page that should be shown when we start up. Zero indexed</param>
        /// <param name="screen">The screen that hosts everything (routing!)</param>
        public FullTalkAsStripViewModel(IScreen screen, PdfDocument doc)
        {
            Debug.Assert(doc != null);
            Debug.Assert(screen != null);

            _doc = doc;
            HostScreen = screen;

            // Setup each individual page
            _numberPages = doc.PageCount;
            Pages = new ReactiveList<PDFPageViewModel>();
            Pages.AddRange(Enumerable.Range(0, (int)doc.PageCount).Select(pageNumber => new PDFPageViewModel(doc.GetPage((uint)pageNumber))));

            // Page navigation. Make sure things are clean and we don't over-burden the UI before
            // we pass the info back to the UI!
            _moveToPage = new ReplaySubject<int>(1);
            MoveToPage = _moveToPage
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

            // The go back gets a direct connection to the "back" bit.
            GoBack = screen.Router.NavigateBack;
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
