using IWalker.ViewModels;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    public sealed partial class CategoryURIUserControl : UserControl, IViewFor<CategoryURIViewModel>
    {
        /// <summary>
        /// Configure the view for... viewing. :-)
        /// </summary>
        public CategoryURIUserControl()
        {
            this.InitializeComponent();
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.MeetingList, y => y.MeetingList.ItemsSource));
                disposeOfMe(this.ObservableForProperty(x => x.ViewModel)
                    .Select(x => x.Value)
                    .Where(x => x != null)
                    .Subscribe(vm =>
                    {
                        disposeOfMe(vm.MeetingToVisit
                            .Subscribe(m => Locator.Current.GetService<RoutingState>().Navigate.Execute(m)));
                    }));
            });
        }

        /// <summary>
        /// Track the view models
        /// </summary>
        public CategoryURIViewModel ViewModel
        {
            get { return (CategoryURIViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CategoryURIViewModel), typeof(CategoryURIUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (CategoryURIViewModel)value; }
        }

        /// <summary>
        /// Fires when the user wants to look at a particular item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeetingList_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ViewMeeting.Execute(e.ClickedItem);
        }
    }
}
