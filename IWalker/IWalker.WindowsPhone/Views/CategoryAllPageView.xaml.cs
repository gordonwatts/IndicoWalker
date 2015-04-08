using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace IWalker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CategoryAllPageView : Page, IViewFor<CategoryAllPageViewModel>
    {
        public CategoryAllPageView()
        {
            this.InitializeComponent();
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.ListOfCalendars, y => y.CategoryNames.ItemsSource));
                disposeOfMe(this.ObservableForProperty(x => x.ViewModel)
                    .Select(vm => vm.Value)
                    .Where(vm => vm != null)
                    .Subscribe(vm =>
                    {
                        disposeOfMe(vm.ViewCategory
                            .Subscribe(nextCi => ViewModel.HostScreen.Router.Navigate.Execute(new CategoryPageViewModel(ViewModel.HostScreen, nextCi.MeetingList))));
                    }));

                // Run the master/detail stuff
                Observable.FromEventPattern<ItemClickEventArgs>(CategoryNames, "ItemClick")
                    .Select(args => args.EventArgs.ClickedItem)
                    .Subscribe(args => ViewModel.ShowCategoryDetails.Execute(args));

                // Each time the page is shown, make sure to update the list.
                ViewModel.UpdateCategoryList.Execute(null);
            });
        }

        /// <summary>
        /// The viewmodel that backs this page
        /// </summary>
        public CategoryAllPageViewModel ViewModel
        {
            get { return (CategoryAllPageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CategoryAllPageViewModel), typeof(CategoryAllPageView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (CategoryAllPageViewModel)value; }
        }
    }
}
