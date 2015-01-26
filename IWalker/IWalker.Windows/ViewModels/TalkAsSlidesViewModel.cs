using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IWalker.ViewModels
{
    /// <summary>
    /// The ViewModel for viewing a talk as a strip of slides.
    /// </summary>
    class TalkAsSlidesViewModel : ReactiveObject
    {
        public ReactiveList<SlideThumbViewModel> SlideImageList { get; set; }
    }
}
