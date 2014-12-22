using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IWalker.ViewModel
{
    public class SimpleMeetingViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="MeetingTitle" /> property's name.
        /// </summary>
        public const string MeetingTitlePropertyName = "MeetingTitle";

        private string _meetingTitle = "";

        public SimpleMeetingViewModel(string url)
        {
            MeetingTitle = url;
        }

        /// <summary>
        /// Sets and gets the MeetingTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MeetingTitle
        {
            get
            {
                return _meetingTitle;
            }
            set
            {
                Set(() => MeetingTitle, ref _meetingTitle, value);
            }
        }
    }
}
