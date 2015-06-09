using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using IWalker.Util;
using Splat;

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
        /// The "title" slide for the talk, as a teaser.
        /// </summary>
        public FirstSlideHeroViewModel HeroSlide { get; private set; }

        /// <summary>
        /// The list of thumbs, hidden until asked to be seen.
        /// </summary>
        public ExpandingSlideThumbViewModel Thumbs { get; private set; }

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

            // If there is a PDF file, then we use that to show a "hero" slide.
            // TODO: WARNING - this will create a PDFFile, but one may not want that here
            // if one is also going to create other PDF file guys!!
            var pdf = allFilesVM.Where(f => f.FilePointer.FileType == "pdf" && f.FilePointer.IsValid).FirstOrDefault();
            if (pdf != null)
            {
                var pdfFile = new PDFFile(pdf.UserControl.FileDownloader);
                var fullVM = new Lazy<FullTalkAsStripViewModel>(() => new FullTalkAsStripViewModel(Locator.Current.GetService<IScreen>(), pdfFile));
                HeroSlide = new FirstSlideHeroViewModel(pdfFile, fullVM);

                var timeSpan = new TimePeriod(t.StartTime, t.EndTime);
                Thumbs = new ExpandingSlideThumbViewModel(pdfFile, timeSpan);
            }
            else
            {
                HeroSlide = new FirstSlideHeroViewModel((PDFFile) null, null);
            }
        }
    }
}
