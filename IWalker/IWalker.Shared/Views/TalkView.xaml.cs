﻿using IWalker.DataModel.Interfaces;
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
    public sealed partial class TalkView : UserControl, IViewFor<IWalker.DataModel.Inidco.IndicoMeetingRef.IndicoTalk>
    {
        public TalkView()
        {
            this.InitializeComponent();

            this.Bind(ViewModel, x => x.Title, y => y.TalkTitle.Text);
        }

        /// <summary>
        /// Stash the view model
        /// </summary>
        public IWalker.DataModel.Inidco.IndicoMeetingRef.IndicoTalk ViewModel
        {
            get { return (IWalker.DataModel.Inidco.IndicoMeetingRef.IndicoTalk)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IWalker.DataModel.Inidco.IndicoMeetingRef.IndicoTalk), typeof(TalkView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IWalker.DataModel.Inidco.IndicoMeetingRef.IndicoTalk)value; }
        }
    }
}
