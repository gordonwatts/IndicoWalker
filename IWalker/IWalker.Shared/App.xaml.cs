using Akavache;
using IWalker.DataModel.Interfaces;
using IWalker.DataModel.MRU;
using IWalker.ViewModels;
using IWalker.Views;
using Newtonsoft.Json;
using ReactiveUI;
using Splat;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reactive.Linq;
using IWalker.Util;

#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
#endif
#if WINDOWS_PHONE_APP
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
#endif

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace IWalker
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// Suspension helper.
        /// </summary>
        readonly IWalker.Util.AutoSuspendHelper autoSuspendHelper;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Register everything so the ReactiveUI view model finder can do the wiring it needs to do.

            Locator.CurrentMutable.Register(() => new StartPage(), typeof(IViewFor<StartPageViewModel>));
            Locator.CurrentMutable.Register(() => new MeetingPage(), typeof(IViewFor<MeetingPageViewModel>));
            Locator.CurrentMutable.Register(() => new CategoryPageView(), typeof(IViewFor<CategoryPageViewModel>));
            Locator.CurrentMutable.Register(() => new TalkView(), typeof(IViewFor<TalkUserControlViewModel>));
            Locator.CurrentMutable.Register(() => new CategoryConfigUserControl(), typeof(IViewFor<CategoryConfigViewModel>));
#if WINDOWS_APP
            Locator.CurrentMutable.Register(() => new SlideThumbUserControl(), typeof(IViewFor<SlideThumbViewModel>));
            Locator.CurrentMutable.Register(() => new PDFPageUserControl(), typeof(IViewFor<PDFPageViewModel>));
            Locator.CurrentMutable.Register(() => new FullTalkAsStripView(), typeof(IViewFor<FullTalkAsStripViewModel>));
            Locator.CurrentMutable.Register(() => new SecuritySettingsPage(), typeof(IViewFor<BasicSettingsViewModel>));
#endif
#if WINDOWS_PHONE_APP
            Locator.CurrentMutable.Register(() => new BasicSettingsView(), typeof(IViewFor<BasicSettingsViewModel>));
#endif

            // Create the main view model, and register that.
            var r = new RoutingState();
            Locator.CurrentMutable.RegisterConstant(r, typeof(RoutingState));
            Locator.CurrentMutable.RegisterConstant(new MainPageViewModel(r), typeof(IScreen));

            // Setup suspend and resume. Note we need to do this once
            // we have routine info.
            this.Suspending += this.OnSuspending;

            autoSuspendHelper = new IWalker.Util.AutoSuspendHelper(this);
            RxApp.SuspensionHost.CreateNewAppState = () => new MainPageViewModel(r);
            RxApp.SuspensionHost.SetupDefaultSuspendResume();
            Locator.CurrentMutable.RegisterConstant(autoSuspendHelper, typeof(IWalker.Util.AutoSuspendHelper));

            // The Most Recently Used (MUR) database
            var mruDB = new MRUDatabaseAccess();
            Locator.CurrentMutable.RegisterConstant(mruDB, typeof(IMRUDatabase));

            // Make sure the JSON serializer for our cache can deal with interface objects:
            Locator.CurrentMutable.Register(() => new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
            }, typeof(JsonSerializerSettings), null);

#if WINDOWS_PHONE_APP
            // And the back button on windows phone.
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += (o, args) =>
            {
                if (r.NavigateBack.CanExecute(null))
                {
                    r.NavigateBack.Execute(null);
                    args.Handled = true;
                }
            };
#endif
            // Setup the internal data cache
            BlobCache.ApplicationName = "IndicoWalker";
            Blobs.Register();

            // Get all background tasks setup.
            IWalker.Util.BackgroundTasks.Register();
        }

        /// <summary>
        /// When we come back in, make sure anyone that wants to know knows...
        /// </summary>
        /// <param name="args"></param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            autoSuspendHelper.OnActivated(args);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            base.OnLaunched(e);
            autoSuspendHelper.OnLaunched(e);
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_APP
        /// <summary>
        /// When the window is created, attach items to the settings pane for the Windows Store version of this app.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            // Show the setting fly out. Connect a VM to it.
            var sc = new SettingsCommand(new Guid(), "Basic Settings", h => new BasicSettingsFlyout(Locator.Current.GetService<IScreen>()).Show());

            // Add it as one of the things we are going to show.
            SettingsPane.GetForCurrentView().Events().CommandsRequested
                .Subscribe(sargs => sargs.Request.ApplicationCommands.Add(sc));

            // And finally, do everything else that needs to be done.
            base.OnWindowCreated(args);
        }
#endif

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            Blobs.LocalStorage.Flush().FirstAsync().Wait();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}