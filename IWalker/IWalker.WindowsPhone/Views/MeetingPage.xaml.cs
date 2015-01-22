using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IWalker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MeetingPage : Page, IViewFor<MeetingPageViewModel>
    {
        public MeetingPage()
        {
            this.InitializeComponent();

            // Bind everything together we need.
            this.Bind(ViewModel, x => x.MeetingTitle, y => y.MeetingTitle.Text);
            this.OneWayBind(ViewModel, x => x.Talks, y => y.TalkList.ItemsSource);
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public MeetingPageViewModel ViewModel
        {
            get { return (MeetingPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MeetingPageViewModel), typeof(MeetingPage), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MeetingPageViewModel)value; }
        }
    }
}
