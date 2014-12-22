using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IWalker.Util;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IWalker.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            _meetingURL = Settings.LastViewedMeeting;
        }

        /// <summary>
        /// The current view model for the meeting we are looking at
        /// </summary>
        public SimpleMeetingViewModel MeetingAddress
        {
            get { return new SimpleMeetingViewModel(_meetingURL); }
        }

        /// <summary>
        /// The <see cref="MeetingURL" /> property's name.
        /// </summary>
        public const string MeetingURLPropertyName = "MeetingURL";

        private string _meetingURL = "";

        /// <summary>
        /// Sets and gets the MeetingURL property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string MeetingURL
        {
            get
            {
                return _meetingURL;
            }

            set
            {
                if (_meetingURL == value)
                {
                    return;
                }

                _meetingURL = value;
                Settings.LastViewedMeeting = _meetingURL;
                RaisePropertyChanged(() => MeetingURL);
            }
        }

        /// <summary>
        /// Backing field for doing a command
        /// </summary>
        private RelayCommand _doitCommand;

        /// <summary>
        /// Do the command
        /// </summary>
        public RelayCommand DoItCommand
        {
            get
            {
                return _doitCommand
                    ?? (_doitCommand = new RelayCommand(() => ((Frame)Window.Current.Content).Navigate(typeof(SimpleMeetingView))));
            }
        }
    }
}