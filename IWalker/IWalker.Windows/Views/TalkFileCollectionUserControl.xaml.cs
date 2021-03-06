﻿using IWalker.ViewModels;
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
    public sealed partial class TalkFileCollectionUserControl : UserControl, IViewFor<TalkFileCollectionUserControlViewModel>
    {
        /// <summary>
        /// Show the list of files
        /// </summary>
        public TalkFileCollectionUserControl()
        {
            this.InitializeComponent();

            var gc = new CompositeDisposable();
            gc.Add(this.OneWayBind(ViewModel, x => x.TalkFiles, y => y.FileLists.ItemsSource));
            gc.Add(this.OneWayBind(ViewModel, x => x.Thumbs, y => y.SlidesAsThumbs.ViewModel));
            gc.Add(this.OneWayBind(ViewModel, x => x.HeroSlide, y => y.FileHero.ViewModel));

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
        /// Track the view models
        /// </summary>
        public TalkFileCollectionUserControlViewModel ViewModel
        {
            get { return (TalkFileCollectionUserControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(TalkFileCollectionUserControlViewModel), typeof(TalkFileCollectionUserControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TalkFileCollectionUserControlViewModel)value; }
        }
    }
}
