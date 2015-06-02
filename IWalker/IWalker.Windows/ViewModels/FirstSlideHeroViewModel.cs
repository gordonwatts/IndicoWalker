using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Given a PDF file, display a hero slide which is the first slide. Take up no space and
    /// no thought if we aren't given something to show. :-)
    /// </summary>
    public class FirstSlideHeroViewModel : ReactiveObject
    {
        /// <summary>
        /// Get/Set the page VM.
        /// </summary>
        /// <remarks>Null if we don't have anything to show</remarks>
        public PDFPageViewModel HeroPageUC { get; private set; }

        /// <summary>
        /// Returns true if we have a hero slide to be shown.
        /// </summary>
        public bool HaveHeroSlide { get; private set; }

        /// <summary>
        /// Create the VM. We generate a first page hero slide.
        /// </summary>
        /// <param name="file">The PDF File to generate. If null, we will make this VM as invalid</param>
        public FirstSlideHeroViewModel(PDFFile file)
        {
            if (file == null)
            {
                HeroPageUC = null;
                HaveHeroSlide = false;
            }
            else
            {
                HeroPageUC = new PDFPageViewModel(file.GetPageStreamAndCacheInfo(0));
                HaveHeroSlide = true;
            }
        }

        /// <summary>
        /// Create the VM starting from a file downloader
        /// </summary>
        /// <param name="fileDownloader"></param>
        public FirstSlideHeroViewModel(FileDownloadController fileDownloader)
            : this(fileDownloader == null ? null : new PDFFile(fileDownloader))
        {
        }
    }
}
