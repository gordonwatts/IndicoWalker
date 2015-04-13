using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Helps coordinate and control the downloading and updating of file (IFile) data
    /// for the app. Shared amongst various VM's as necessary.
    /// </summary>
    /// <remarks>
    /// There should only be one of these at any time for a particular file.
    /// </remarks>
    public class IFileDownloadController : ReactiveObject
    {
        /// <summary>
        /// True if data is being downloaded as we speak
        /// </summary>
        public bool IsDownloading
        {
            get { return _isDownloading.Value; }
        }
        private ObservableAsPropertyHelper<bool> _isDownloading;

        /// <summary>
        /// True if there is data for this file available in the local cache
        /// </summary>
        /// <remarks>
        /// This would be true even if there is an update available on the server, or
        /// even if there was an update download in progress
        /// </remarks>
        public bool IsDownloaded
        {
            get { return _isDownloaded.Value; }
        }
        private ObservableAsPropertyHelper<bool> _isDownloaded;

        /// <summary>
        /// Create the download controller for this file
        /// </summary>
        /// <param name="file"></param>
        public IFileDownloadController(IFile file, IBlobCache cache = null)
        {
            _file = file;
            _cache = cache == null ? Blobs.LocalStorage : cache;

            // Track the status of the download
            _cache.GetCreatedAt(_file.UniqueKey)
                .Select(dt => dt.HasValue)
                .ToProperty(this, x => x.IsDownloaded, out _isDownloaded, false);
        }

        /// <summary>
        /// The file we control
        /// </summary>
        private IFile _file;

        /// <summary>
        /// Cache used to load everything up
        /// </summary>
        private IBlobCache _cache;

    }
}
