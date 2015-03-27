using IWalker.DataModel.Inidco;
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
    /// A really simple view to get past how ReactiveUI does its list displays. Just shows a string
    /// with the site name.
    /// </summary>
    public sealed partial class IndicoApiKeyView : UserControl, IViewFor<IndicoApiKey>
    {
        public IndicoApiKeyView()
        {
            this.InitializeComponent();
            this.WhenAny(x => x.ViewModel, x => x.Value)
                .Subscribe(vm => SiteNameTextBox.Text = vm == null ? "" : vm.Site);
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public IndicoApiKey ViewModel
        {
            get { return (IndicoApiKey)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IndicoApiKey), typeof(IndicoApiKeyView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IndicoApiKey)value; }
        }
    }
}
