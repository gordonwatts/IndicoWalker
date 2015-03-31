using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    public sealed partial class ErrorUserControl : UserControl, IViewFor<ErrorUserControlViewModel>
    {
        public ErrorUserControl()
        {
            this.InitializeComponent();
            this.OneWayBind(ViewModel, x => x.ErrorSeen, y => y.DisplayError.Visibility);
            this.ObservableForProperty(x => x.ViewModel)
                .Select(vmop => vmop.Value)
                .Where(vm => vm != null)
                .Subscribe(vm =>
                {
                    vm.DisplayErrors.Subscribe(msg =>
                    {
                        var dlg = new MessageDialog(msg, "Error Encountered Loading Category");
#pragma warning disable 4014
                        dlg.ShowAsync();
#pragma warning restore 4014
                    });
                });
            this.BindCommand(ViewModel, x => x.ViewRequest, y => y.DisplayError);
            DisplayError.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        /// <summary>
        /// Track the view models
        /// </summary>
        public ErrorUserControlViewModel ViewModel
        {
            get { return (ErrorUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ErrorUserControlViewModel), typeof(ErrorUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ErrorUserControlViewModel)value; }
        }
    }
}
