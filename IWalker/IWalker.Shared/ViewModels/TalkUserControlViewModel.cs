using IWalker.DataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Class for talk view models.
    /// </summary>
    public class TalkUserControlViewModel
    {
        /// <summary>
        /// Hold onto the talk we need
        /// </summary>
        private ITalk _talk;
        
        /// <summary>
        /// Init with the various items for a talk.
        /// </summary>
        /// <param name="t"></param>
        public TalkUserControlViewModel(ITalk t)
        {
            this._talk = t;
        }

        /// <summary>
        /// Get the title (for the UI).
        /// </summary>
        public string Title { get { return _talk.Title; } }
    }
}
