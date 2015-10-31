using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ReactiveUI;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IWalker.ViewModels;

namespace IWalker.Views
{
    /// <summary>
    /// User control for the open new meeting URL buttons
    /// </summary>
    public sealed partial class OpenURLControlView : UserControl, IViewFor<OpenURLControlViewModel>
    {
        public OpenURLControlView()
        {
            this.InitializeComponent();

            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.BindCommand(ViewModel, x => x.SwitchPages, x => x.FindIndicoUrl));
                disposeOfMe(this.Bind(ViewModel, x => x.MeetingAddress, y => y.IndicoUrl.Text));
            });
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public OpenURLControlViewModel ViewModel
        {
            get { return (OpenURLControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(OpenURLControlViewModel), typeof(OpenURLControlView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (OpenURLControlViewModel)value; }
        }
    }
}
