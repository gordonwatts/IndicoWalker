using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace IWalker.Util
{
    /// <summary>
    /// Instantiate at in your Universal App's App.xaml.cs ctor, and then call OnLanched and OnActivated methods in the App's over-ridden methods
    /// of the same name. Will make sure to properly track the lifeccyle of the app.
    /// </summary>
    /// <remarks>
    /// Copied from ReactiveUI's WinRTAutoSuspendApplication
    /// </remarks>
    public class AutoSuspendHelper : IEnableLogger
    {
        /// <summary>
        /// Track the launch args as they come in. Cache them for one re-play.
        /// </summary>
        readonly ReplaySubject<LaunchActivatedEventArgs> _launched = new ReplaySubject<LaunchActivatedEventArgs>(1);

        /// <summary>
        /// Track the app activations.
        /// </summary>
        readonly ReplaySubject<IActivatedEventArgs> _activated = new ReplaySubject<IActivatedEventArgs>(1);

        /// <summary>
        /// Fires when the app is re-activated. The args come with it.
        /// </summary>
        public static IObservable<IActivatedEventArgs> IsActivated { get; private set; }

        /// <summary>
        /// Hook ourselves up to the app.
        /// </summary>
        /// <param name="app">The application. It must have OnLaunched and OnActivated overriden or this will bomb (they should call OnLaunched and OnActivated here).</param>
        public AutoSuspendHelper(Application app)
        {
            // Make sure to override these two so that all events get passed in.
            Reflection.ThrowIfMethodsNotOverloaded("AutoSuspendHelper", app, "OnLaunched");
            Reflection.ThrowIfMethodsNotOverloaded("AutoSuspendHelper", app, "OnActivated");

            // When activated
            IsActivated = _activated;

            // When we have a new launch, with no old user state.
            var launchNew = new[] { ApplicationExecutionState.ClosedByUser, ApplicationExecutionState.NotRunning, };
            RxApp.SuspensionHost.IsLaunchingNew = _launched
                .Where(x => launchNew.Contains(x.PreviousExecutionState))
                .Select(_ => Unit.Default);

            // Resuming with an old user state.
            RxApp.SuspensionHost.IsResuming = _launched
                .Where(x => x.PreviousExecutionState == ApplicationExecutionState.Terminated)
                .Select(_ => Unit.Default);

            // Starting from a suspended state
            var unpausing = new[] { ApplicationExecutionState.Suspended, ApplicationExecutionState.Running, };
            RxApp.SuspensionHost.IsUnpausing = _launched
                .Where(x => unpausing.Contains(x.PreviousExecutionState))
                .Select(_ => Unit.Default);

            // When we are suspending
            var shouldPersistState = new Subject<SuspendingEventArgs>();
            app.Suspending += (o, e) => shouldPersistState.OnNext(e);
            RxApp.SuspensionHost.ShouldPersistState =
                shouldPersistState.Select(x =>
                {
                    var deferral = x.SuspendingOperation.GetDeferral();
                    return Disposable.Create(deferral.Complete);
                });

            var shouldInvalidateState = new Subject<Unit>();
            app.UnhandledException += (o, e) => shouldInvalidateState.OnNext(Unit.Default);
            RxApp.SuspensionHost.ShouldInvalidateState = shouldInvalidateState;
        }

        /// <summary>
        /// Call to pass in launch args
        /// </summary>
        /// <param name="args">Argument passed to the App's OnLaunched method</param>
        public void OnLaunched(LaunchActivatedEventArgs args)
        {
            _launched.OnNext(args);
        }

        /// <summary>
        /// Call to pass in activation args
        /// </summary>
        /// <param name="args">Argument passed to the App's OnActivated method</param>
        public void OnActivated(IActivatedEventArgs args)
        {
            _activated.OnNext(args);
        }
    }
}
