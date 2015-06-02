using IWalker.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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
    public sealed partial class FirstSlideHeroUserControl : UserControl, IViewFor<FirstSlideHeroViewModel>
    {
        public FirstSlideHeroUserControl()
        {
            this.InitializeComponent();

            // WHen the VM comes in, show our guy if we can.
            var gc = new CompositeDisposable();
            gc.Add(this.OneWayBind(ViewModel, x => x.HaveHeroSlide, y => y.PdfPage.Visibility));
            gc.Add(this.OneWayBind(ViewModel, x => x.HeroPageUC, y => y.PdfPage.ViewModel));

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
