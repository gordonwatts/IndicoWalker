using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
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
        /// Get everything setup to show the PDF document
        /// </summary>
        /// <param name="doc"></param>
        public FullTalkAsStripViewModel(IScreen screen, PdfDocument doc)
        {
            Debug.Assert(doc != null);
            Debug.Assert(screen != null);

            _doc = doc;
            HostScreen = screen;

            // Setup each individual page
            Pages = new ReactiveList<PDFPageViewModel>();
            Pages.AddRange(Enumerable.Range(0, (int) doc.PageCount).Select(pageNumber => new PDFPageViewModel(doc.GetPage((uint) pageNumber))));

            // The go back gets a direct connection to the "back" bit.
            GoBack = screen.Router.NavigateBack;
        }

        /// <summary>
        /// Called to navigate to this page. This is a short-cut so that others
        /// who have access to us can load us without having to know the router, etc.
        /// </summary>
        internal void LoadPage()
        {
            HostScreen.Router.Navigate.Execute(this);
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
