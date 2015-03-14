﻿using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using IWalker.DataModel.Categories;
using IWalker.DataModel.Interfaces;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Handles the config for a category (title, subscribed, displayed on main page, etc.)
    /// </summary>
    public class CategoryConfigViewModel : ReactiveObject
    {
        /// <summary>
        /// Get/Set if this particular feed is subscribed to.
        /// </summary>
        /// <remarks>This means that we will track it</remarks>
        public bool IsSubscribed
        {
            get { return _isSubscribed; }
            set { this.RaiseAndSetIfChanged(ref _isSubscribed, value); }
        }
        private bool _isSubscribed;

        /// <summary>
        /// Get/Set if this feed is displayed on the main page or not.
        /// </summary>
        public bool IsDisplayedOnMainPage
        {
            get { return _isDisplayedOnMainPage; }
            set { this.RaiseAndSetIfChanged(ref _isDisplayedOnMainPage, value); }
        }
        private bool _isDisplayedOnMainPage;

        /// <summary>
        /// Get/Set the title for this feed
        /// </summary>
        public string CategoryTitle
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        private string _title;

        /// <summary>
        /// Initialize the settings interface for a particular category.
        /// </summary>
        public CategoryConfigViewModel(IMeetingListRef meeting)
        {
            // First, we need to determine if this meeting is already in the
            // database.

            CategoryTitle = "not yet";

            // If they want it to be displayed on the main page, then we have to subscribe to it.
            this.WhenAny(x => x.IsDisplayedOnMainPage, x => x.Value)
                .Where(isDisplayedValue => isDisplayedValue)
                .Subscribe(v => IsSubscribed = true);

            // If they don't want to subscribe, then we can't display it.
            this.WhenAny(x => x.IsSubscribed, x => x.Value)
                .Where(isSubscribed => !isSubscribed)
                .Subscribe(x => IsDisplayedOnMainPage = false);
        }
    }
}
