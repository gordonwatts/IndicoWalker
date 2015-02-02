using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Basic settings class.
    /// </summary>
    public class BasicSettingsViewModel : ReactiveObject, IRoutableViewModel
    {
        public BasicSettingsViewModel(IScreen screen)
        {
            HostScreen = screen;
        }

        public IScreen HostScreen { get; private set; }

        public string UrlPathSegment
        {
            get { return "/BasicSettings.xaml"; }
        }
    }
}
