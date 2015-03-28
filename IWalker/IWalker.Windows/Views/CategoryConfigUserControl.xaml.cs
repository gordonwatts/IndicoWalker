using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    /// <summary>
    /// Simple view to show the various options for an agenda
    /// </summary>
    public sealed partial class CategoryConfigUserControl : UserControl, IViewFor<CategoryConfigViewModel>
    {
        public CategoryConfigUserControl()
        {
            this.InitializeComponent();
            this.Bind(ViewModel, x => x.IsSubscribed, y => y.Subscribe.IsOn);
            this.Bind(ViewModel, x => x.IsDisplayedOnMainPage, y => y.Displayed.IsOn);
            this.Bind(ViewModel, x => x.CategoryTitle, y => y.AgendaListTitle.Text);
            this.Bind(ViewModel, x => x.CategoryTitle, y => y.AgendaListTitleEdit.Text);
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
