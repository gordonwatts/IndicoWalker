
using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Windows.Data.Pdf;
using Windows.Storage;
namespace IWalker.ViewModels
{
    /// <summary>
    /// Control to render slides in PDF.
    /// </summary>
    public class FileSlideListViewModel : ReactiveObject
    {
        /// <summary>
        /// Cache the file we are responsible for.
        /// </summary>
        private IFile _file;

        /// <summary>
        /// Triggers the rendering of the PDF file in small little images.
        /// </summary>
        private ReactiveCommand<StorageFile> _renderPDF;

        /// <summary>
        /// The list of thumbnails
        /// </summary>
        public ReactiveList<SlideThumbViewModel> SlideThumbnails { get; private set; }

        /// <summary>
        /// Setup the VM for the associated file.
        /// </summary>
        /// <param name="f"></param>
        public FileSlideListViewModel(IFile f)
        {
            Debug.Assert(f != null);

            // Get the object consitent.
            _file = f;
            SlideThumbnails = new ReactiveList<SlideThumbViewModel>();

            // Run a rendering and populate the renderpdf control with all the
            // thumbnails we can.
            _renderPDF = ReactiveCommand.CreateAsyncTask(_ => f.DownloadFile());
            _renderPDF
                .SelectMany(sf => PdfDocument.LoadFromFileAsync(sf))
                .Select(sf => Enumerable.Range(0, (int)sf.PageCount - 1)
                                .Select(index => sf.GetPage((uint)index))
                                .Select(p => new SlideThumbViewModel(p)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(pages => SlideThumbnails.AddRange(pages));

            // TODO: Normally this would not kick off a download, but in this case
            // we will until we get a real background store in there. Then we can kick this
            // off only if the file is actually downloaded.

            _renderPDF.ExecuteAsync().Subscribe();
        }
    }
}
