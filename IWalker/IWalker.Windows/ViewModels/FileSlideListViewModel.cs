
using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Windows.Data.Pdf;
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

            // Get the object consistent.
            _file = f;
            SlideThumbnails = new ReactiveList<SlideThumbViewModel>();

            if (_file.IsValid && _file.FileType == "pdf")
            {

                // Run a rendering and populate the render pdf control with all the
                // thumbnails we can.
                // TODO: Replace the catch below to notify bad PDF format.
                Exception userBomb;
                var renderPDF = ReactiveCommand.CreateAsyncTask(_ => f.DownloadFile());
                renderPDF
                    .SelectMany(sf => PdfDocument.LoadFromFileAsync(sf))
                    .Select(sf => Enumerable.Range(0, (int)sf.PageCount)
                                    .Select(index => sf.GetPage((uint)index))
                                    .Select(p => new SlideThumbViewModel(p)))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(pages => SlideThumbnails.AddRange(pages),
                               ex => userBomb = ex);

                // TODO: Normally this would not kick off a download, but in this case
                // we will until we get a real background store in there. Then we can kick this
                // off only if the file is actually downloaded.

                renderPDF.ExecuteAsync().Subscribe();
            }
        }
    }
}
