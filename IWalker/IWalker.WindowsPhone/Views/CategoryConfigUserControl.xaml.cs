using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    public sealed partial class CategoryConfigUserControl : UserControl, IViewFor<CategoryConfigViewModel>
    {
        public CategoryConfigUserControl()
        {
            this.InitializeComponent();

            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.Bind(ViewModel, x => x.IsSubscribed, y => y.Subscribe.IsOn));
                disposeOfMe(this.Bind(ViewModel, x => x.IsDisplayedOnMainPage, y => y.Displayed.IsOn));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.CategoryTitle, y => y.AgendaListTitle.Content));
                disposeOfMe(this.Bind(ViewModel, x => x.CategoryTitle, y => y.AgendaListTitleEdit.Text));

                // When they click on the main item, swap control visibility
                disposeOfMe(Observable.FromEventPattern(AgendaListTitle, "Click")
                    .Subscribe(_ =>
                    {
                        AgendaListTitle.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        AgendaListTitleEdit.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }));

                // And when they are done with the new name
                var rtnHit = Observable.FromEventPattern<KeyRoutedEventArgs>(AgendaListTitleEdit, "KeyUp")
                    .Where(kargs => kargs.EventArgs.Key == Windows.System.VirtualKey.Enter);

                disposeOfMe(rtnHit
                    .Subscribe(_ =>
                    {
                        AgendaListTitle.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        AgendaListTitleEdit.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }));
            });
        }

        /// <summary>
        /// The viewmodel that backs this page
        /// </summary>
        public CategoryConfigViewModel ViewModel
        {
            get { return (CategoryConfigViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CategoryConfigViewModel), typeof(CategoryConfigUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (CategoryConfigViewModel)value; }
        }
    }
}
