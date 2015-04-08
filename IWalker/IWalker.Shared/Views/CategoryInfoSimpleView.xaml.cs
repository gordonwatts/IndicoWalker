using IWalker.DataModel.Categories;
using IWalker.DataModel.Interfaces;
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
    public sealed partial class CategoryInfoSimpleView : UserControl, IViewFor<CategoryConfigInfo>
    {
        public CategoryInfoSimpleView()
        {
            this.InitializeComponent();
            this.WhenActivated(disposeOfMe =>
            {
                disposeOfMe(this.OneWayBind(ViewModel, x => x.CategoryTitle, y => y.CategoryTitle.Text));
            });
        }

        /// <summary>
        /// Our view model
        /// </summary>
        public CategoryConfigInfo ViewModel
        {
            get { return (CategoryConfigInfo)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CategoryConfigInfo), typeof(CategoryInfoSimpleView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (CategoryConfigInfo)value; }
        }
    }
}
