using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace IWalker.DataModel.Interfaces
{
    /// <summary>
    /// Represents a file for the talk
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Is the file valid... should we even display an icon on the UI for
        /// this file?
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Return a storage file on the local system that contains the data
        /// for this file.
        /// </summary>
        /// <returns></returns>
        Task<StorageFile> DownloadFile();
    }
}
