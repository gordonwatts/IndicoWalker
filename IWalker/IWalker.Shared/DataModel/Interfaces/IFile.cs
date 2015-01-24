using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
