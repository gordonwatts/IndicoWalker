using IWalker.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IWalker.Views
{
    public sealed partial class TalkView : UserControl, IViewFor<TalkUserControlViewModel>
    {
        public TalkView()
        {
            this.InitializeComponent();

            this.Bind(ViewModel, x => x.Title, y => y.TalkTitle.Text);
            this.OneWayBind(ViewModel, x => x.File, y => y.GoodFile.ViewModel);
            this.OneWayBind(ViewModel, x => x.HasValidMainFile, y => y.GoodFile.Visibility);
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public TalkUserControlViewModel ViewModel
        {
            get { return (TalkUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(TalkUserControlViewModel), typeof(TalkView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TalkUserControlViewModel)value; }
        }
    }
}
