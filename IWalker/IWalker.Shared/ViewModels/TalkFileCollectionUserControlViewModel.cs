using IWalker.DataModel.Interfaces;
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

        /// <summary>
        /// The VM for the list of thumbnails associated with this talk.
        /// </summary>
        public FileSlideListViewModel TalkThumbnails { get; private set; }

        /// <summary>
        /// Configure for showing multiple files.
        /// </summary>
        /// <param name="files"></param>
        public TalkFileCollectionUserControlViewModel(IFile[] files, ITalk t)
        {
            TalkFiles = new ReactiveList<FileUserControlViewModel>();
            if (files.Length > 0)
            {
                Title = files[0].DisplayName;
                var allFilesVM = (from f in files
                                  select Tuple.Create(f, new FileUserControlViewModel(f)))
                                  .ToArray();

                TalkFiles.AddRange(allFilesVM.Select(f => f.Item2));

                var pdf = allFilesVM.Where(f => f.Item1.FileType == "pdf").FirstOrDefault();
                if (pdf != null)
                {
                    var timeSpan = new TimePeriod(t.StartTime, t.EndTime);
                    TalkThumbnails = new FileSlideListViewModel(pdf.Item1, timeSpan, pdf.Item2.DownloadedFile);
                }
            }
            else
            {
                Title = "";
            }
        }
    }
}
