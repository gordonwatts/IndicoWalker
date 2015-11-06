using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.ViewModels
{
    /// <summary>
    /// If the user has never ever run this before, we display the view associated with this
    /// VM, which gives them a chance to add default items (e.g. conferences).
    /// </summary>
    public class FirstRunViewModel : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// When run, we will add the default categories to the list, download them,
        /// and then move on.
        /// </summary>
        public ReactiveCommand<object> AddDefaultCategories { get; private set; }

        /// <summary>
        /// Move directly onto the home screen without adding and fetching defualt
        /// categories.
        /// </summary>
        public ReactiveCommand<object> SkipDefaultCategories { get; private set; }

        /// <summary>
        /// Path segment for the stack.
        /// </summary>
        public string UrlPathSegment { get { return "/firstrun"; } }

        /// <summary>
        /// The screen we are attached to
        /// </summary>
        public IScreen HostScreen { get; private set; }

        private Tuple<string, string>[] _defaultItems =
        {
            Tuple.Create("HEP Conferences at Argonne Laboratories", "https://indico.hep.anl.gov/indico/categoryDisplay.py?categId=2"),
            Tuple.Create("Physics and Performance of Future pp Colliders (Argonne Laboratories)", "https://indico.hep.anl.gov/indico/categoryDisplay.py?categId=26").
        };

        /// <summary>
        /// Configure the VM to add items if the user requests.
        /// </summary>
        /// <param name="screen"></param>
        public FirstRunViewModel(IScreen screen)
        {
        }
    }
}
