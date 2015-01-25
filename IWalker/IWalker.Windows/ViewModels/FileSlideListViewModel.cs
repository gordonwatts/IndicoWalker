
using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
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

        private uint mine;

        /// <summary>
        /// Setup the VM for the associated file.
        /// </summary>
        /// <param name="f"></param>
        public FileSlideListViewModel(IFile f)
        {
            _file = f;

            // TODO: Normally this would not kick off a download, but in this case
            // we will until we get a real background store in there.

            _renderPDF = ReactiveCommand.CreateAsyncTask(_ => f.DownloadFile());
            _renderPDF
                .SelectMany(sf => PdfDocument.LoadFromFileAsync(sf))
                .Select(pdf => pdf.PageCount)
                .Subscribe(np => mine = np);
            _renderPDF.ExecuteAsync().Subscribe();
        }
    }
}
