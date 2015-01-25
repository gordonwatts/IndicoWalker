using IWalker.ViewModels;
using ReactiveUI;
using Splat;
using System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IWalker.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            DataContext = Locator.Current.GetService(typeof(IScreen));

            RxApp.SuspensionHost.ObserveAppState<MainPageViewModel>()
                .Subscribe(o => o.MoveAlong());
        }
    }
}
