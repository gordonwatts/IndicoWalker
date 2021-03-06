﻿using IWalker.Util;
using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

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
                disposeOfMe(this.OneWayBind(ViewModel, x => x.ConfigViewModel, y => y.CatConfig.ViewModel));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.CategoryFullListVM, y => y.CatListing.ViewModel));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.ValidCategorySelected, y => y.DetailsGrid.Visibility));
                disposeOfMe(this.OneWayBind(ViewModel, x => x.CategoryFullListVM.ErrorsVM, y => y.ErrorListing.ViewModel));

                // Run the master/detail stuff
                disposeOfMe(Observable.FromEventPattern<SelectionChangedEventArgs>(CategoryNames, "SelectionChanged")
                    .Where(args => args.EventArgs.AddedItems.Count > 0)
                    .Select(args => args.EventArgs.AddedItems[0])
                    .Subscribe(args => ViewModel.ShowCategoryDetails.Execute(args)));
            });
            backButton.WireAsBackButton();

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
