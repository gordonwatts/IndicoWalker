using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Pickers;

namespace IWalker.Util
{
    /// <summary>
    /// Help with all things having to do with files.
    /// </summary>
    static class Files
    {
        /// <summary>
        /// Configure a file picker for certificate files we can understand.
        /// </summary>
        /// <param name="op">The file picker</param>
        /// <returns>The same file picker with various options configured for opening a file.</returns>
        public static FileOpenPicker ForCert(this FileOpenPicker op)
        {
            op.CommitButtonText = "Use Certificate";
            op.SettingsIdentifier = "OpenCert";
            op.FileTypeFilter.Add(".p12");
            op.FileTypeFilter.Add(".pfx");
            op.ViewMode = PickerViewMode.List;

            return op;
        }

    }
}
