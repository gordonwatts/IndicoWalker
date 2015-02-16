using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Linq;
using Windows.Storage;
using Windows.System;
using IWalker.Util;

namespace IWalker.ViewModels
{
    /// <summary>
    /// The ViewModel for a talk file. We are responsible for downloading, opening, etc. the file.
    /// </summary>
    public class FileUserControlViewModel : ReactiveObject
    {
        /// <summary>
        /// The file we represent.
        /// </summary>
        private IFile _file;

        /// <summary>
        /// Pointer to the current local file
        /// </summary>
        private IStorageFile _localFile = null;

        /// <summary>
        /// Command to fire when the user "clicks" on us.
        /// </summary>
        /// <remarks>
        /// If file isn't downloaded, then download the file.
        /// If file is downloaded, then open the file in another program.
        /// </remarks>
        public ReactiveCommand<IStorageFile> ClickedUs { get; private set; }

        /// <summary>
        /// Initialize all of our behaviors.
        /// </summary>
        /// <param name="file"></param>
        public FileUserControlViewModel(IFile file)
        {
            _file = file;

            // Get the file if it is already local.
            var cmd = ReactiveCommand.CreateAsyncObservable(token => _file.GetFile(false));

            cmd
                .Where(f => f != null)
                .Subscribe(f => _localFile = f);
            cmd.ExecuteAsync().Subscribe();

            // Now, when the user clicks, we either download or open...
            ClickedUs = ReactiveCommand.CreateAsyncObservable(cmd.IsExecuting.Select(g => !g), token => _file.GetFile(true));
            ClickedUs
                .Where(f => _localFile == null)
                .Subscribe(f => _localFile = f);

            ClickedUs
                .Where(f => _localFile != null)
                .SelectMany(f => Launcher.LaunchFileAsync(f))
                .Subscribe(g =>
                {
                    if (!g)
                    {
                        throw new InvalidOperationException(string.Format("Unable to open file {0}.", _localFile.Name));
                    }
                });
        }
    }
}
