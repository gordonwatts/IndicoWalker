using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    public sealed partial class FirstSlideHeroUserControl : UserControl, IViewFor<FirstSlideHeroViewModel>
    {
        public FirstSlideHeroUserControl()
        {
            this.InitializeComponent();

            // WHen the VM comes in, show our guy if we can.
            var gc = new CompositeDisposable();
            gc.Add(this.OneWayBind(ViewModel, x => x.HaveHeroSlide, y => y.PdfPage.Visibility));
            gc.Add(this.OneWayBind(ViewModel, x => x.HeroPageUC, y => y.PdfPage.ViewModel));

            // When we are clicked, open the slides.
            var pressed = this.Events().PointerPressed;
            var released = this.Events().PointerReleased;
            var when = from pd in pressed
                       from pu in released
                       select default(Unit);
            gc.Add(when.Subscribe(e => ViewModel.OpenFullView.Execute(null)));

            // Upon activation, set everything up for disposing...
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
        /// Hold onto the view model, which we will need for doing all sorts of things.
        /// </summary>
        public FirstSlideHeroViewModel ViewModel
        {
            get { return (FirstSlideHeroViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(FirstSlideHeroViewModel), typeof(FirstSlideHeroUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (FirstSlideHeroViewModel)value; }
        }
    }
}
