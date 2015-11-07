using IWalker.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IWalker.Views
{
    public sealed partial class TalkView : UserControl, IViewFor<TalkUserControlViewModel>
    {
        public TalkView()
        {
            this.InitializeComponent();

            var gc = new CompositeDisposable();
            gc.Add(this.OneWayBind(ViewModel, x => x.Title, y => y.TalkTitle.Text));
            gc.Add(this.OneWayBind(ViewModel, x => x.Time, y => y.TalkTime.Text));
            gc.Add(this.OneWayBind(ViewModel, x => x.Authors, y => y.Authors.Text));
            gc.Add(this.OneWayBind(ViewModel, x => x.TalkFiles, y => y.FileNameList.ItemsSource));
            gc.Add(this.OneWayBind(ViewModel, x => x.SubTalks, y => y.SubTalkList.ItemsSource));

            this.WhenActivated(disposeOfMe =>
            {
                if (gc != null)
                {
                    disposeOfMe(gc);
                    gc = null;
                }
            });
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
