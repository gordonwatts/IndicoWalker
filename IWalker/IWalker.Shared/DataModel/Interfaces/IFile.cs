using System;
using System.IO;
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
        /// Return the file type put in lower case and without the ".".
        /// e.g. "pdf" or "pptx" or similar.
        /// </summary>
        /// <returns>Lower case string indicating a valid file extension. Or blank if not known.</returns>
        string FileType { get; }

        /// <summary>
        /// This should produce a unique key for the file, which will remain constant. It is used as a place
        /// to store files in the local cache, as well as put it various places. It will be turned into a hash
        /// before written out to disk.
        /// </summary>
        /// <returns></returns>
        string UniqueKey { get; }

        /// <summary>
        /// Return the stream that we can use to read the file
        /// </summary>
        /// <returns></returns>
        IObservable<StreamReader> GetFileStream();

        /// <summary>
        /// Return the display name of a file (generally the name without the type).
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Return the date from the remote storage location of the last
        /// update. Return as a string (a != comparison will be done).
        /// </summary>
        /// <returns></returns>
        IObservable<string> GetFileDate();
    }
}
