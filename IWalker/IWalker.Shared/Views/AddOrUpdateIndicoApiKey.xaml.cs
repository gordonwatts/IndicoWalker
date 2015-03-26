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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    /// <summary>
    /// Hook up the view
    /// </summary>
    public sealed partial class AddOrUpdateIndicoApiKey : UserControl, IViewFor<AddOrUpdateIndicoApiKeyViewModel>
    {
        public AddOrUpdateIndicoApiKey()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Track the view models
        /// </summary>
        public AddOrUpdateIndicoApiKeyViewModel ViewModel
        {
            get { return (AddOrUpdateIndicoApiKeyViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AddOrUpdateIndicoApiKeyViewModel), typeof(AddOrUpdateIndicoApiKey), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (AddOrUpdateIndicoApiKeyViewModel)value; }
        }
    }
}
