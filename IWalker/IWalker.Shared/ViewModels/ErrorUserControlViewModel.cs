using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace IWalker.ViewModels
{
    /// <summary>
    /// Simple VM for an error. We are represented by some small
    /// button that comes visible only when there is an error, and then
    /// we generate a formatted string to show what the error is
    /// when the user wants to see it.
    /// </summary>
    public class ErrorUserControlViewModel : ReactiveObject
    {
        /// <summary>
        /// Get if we've seen an error yet
        /// </summary>
        public bool ErrorSeen
        {
            get { return _haveSeenError.Value; }
        }
        private ObservableAsPropertyHelper<bool> _haveSeenError;

        /// <summary>
        /// Each time this fires, an error should be displayed (somehow).
        /// </summary>
        public IObservable<string> DisplayErrors { get; private set; }

        /// <summary>
        /// Fire this if you want to have all errors replayed.
        /// </summary>
        public ReactiveCommand<object> ViewRequest { get; private set; }

        /// <summary>
        /// Create, with a stream of errors. By default it only holds onto the most recent error.
        /// </summary>
        /// <param name="errors"></param>
        public ErrorUserControlViewModel (IObservable<Exception> errors)
        {
            // When an error shows up...
            errors
                .Select(_ => true)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.ErrorSeen, out _haveSeenError, false);

            // Cache the error stream as its shows up.
            var bld = new StringBuilder();
            errors
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(e =>
                {
                    bld.Clear();
                    bld.Append(e.Message);
                });

            // Show an error when it goes by
            ViewRequest = ReactiveCommand.Create();
            DisplayErrors = ViewRequest
                .Select(_ => bld.ToString())
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .ObserveOn(RxApp.MainThreadScheduler);
        }
    }
}
