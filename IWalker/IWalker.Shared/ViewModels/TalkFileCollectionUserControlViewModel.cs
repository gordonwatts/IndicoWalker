﻿using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using IWalker.Util;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Hold onto a collection of files with the same name
    /// </summary>
    public class TalkFileCollectionUserControlViewModel : ReactiveObject
    {
        /// <summary>
        /// The name of the title of the talk
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// List of files for this grouping.
        /// </summary>
        public ReactiveList<FileUserControlViewModel> TalkFiles { get; private set; }

#if false
        /// <summary>
        /// The VM for the list of thumbnails associated with this talk.
        /// </summary>
        public FileSlideListViewModel TalkThumbnails { get; private set; }
#endif

        /// <summary>
        /// Configure for showing multiple files.
        /// </summary>
        /// <param name="files"></param>
        public TalkFileCollectionUserControlViewModel(IFile[] files, ITalk t)
        {
            // The title we use is what we grab from the first file.
            Title = files.Length > 0 ? files[0].DisplayName : "";

            // Show the list of files that can downloaded/opened. These guys can opened by other
            // apps in the system by clicking or pressing on them.
            var allFilesVM = (from f in files
                             select new
                             {
                                 FilePointer = f,
                                 UserControl = new FileUserControlViewModel(f)
                             })
                             .ToArray();

            TalkFiles = new ReactiveList<FileUserControlViewModel>();
            TalkFiles.AddRange(allFilesVM.Select(f => f.UserControl));

#if false
            var pdf = allFilesVM.Where(f => f.Item1.FileType == "pdf" && f.Item1.IsValid).FirstOrDefault();
            if (pdf != null)
            {
                var timeSpan = new TimePeriod(t.StartTime, t.EndTime);
                TalkThumbnails = new FileSlideListViewModel(pdf.Item2.FileDownloader, timeSpan);
            }
#endif
        }
    }
}
