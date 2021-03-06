﻿using IWalker.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IWalker.Views
{
    /// <summary>
    /// View for the talks in a session
    /// </summary>
    public sealed partial class SessionUserControl : UserControl, IViewFor<SessionUserControlViewModel>
    {
        public SessionUserControl()
        {
            this.InitializeComponent();

            var gc = new CompositeDisposable();
            gc.Add(this.OneWayBind(ViewModel, x => x.Title, y => y.SessionTitle.Text));
            gc.Add(this.OneWayBind(ViewModel, x => x.IsProperTitledSession, y => y.SessionTitle.Visibility, isProper => isProper ? Visibility.Visible : Visibility.Collapsed));
            gc.Add(this.OneWayBind(ViewModel, x => x.Talks, y => y.TalkList.ItemsSource));
            gc.Add(this.OneWayBind(ViewModel, x => x.IsProperTitledSession, y => y.TalkList.Margin, isProper => isProper ? new Thickness(40, 0, 0, 0) : new Thickness(0, 0, 0, 0)));

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
        public SessionUserControlViewModel ViewModel
        {
            get { return (SessionUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SessionUserControlViewModel), typeof(SessionUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (SessionUserControlViewModel)value; }
        }
    }
}
