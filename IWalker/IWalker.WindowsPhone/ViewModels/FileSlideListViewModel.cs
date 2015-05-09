using ReactiveUI;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Dummy VM for use building x-platform.
    /// </summary>
    public class FileSlideListViewModel : ReactiveObject
    {
        /// <summary>
        /// Dummy ctor - we don't do anythign with this in WP
        /// </summary>
        /// <param name="fileDownloadController"></param>
        /// <param name="timeSpan"></param>
        public FileSlideListViewModel(FileDownloadController fileDownloadController, Util.TimePeriod timeSpan)
        {
        }
    }
}
