using IWalker.DataModel.Interfaces;
using IWalker.Util;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

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
        private IRandomAccessStream _localFile = null;

        /// <summary>
        /// Command to fire when the user "clicks" on us.
        /// </summary>
        /// <remarks>
        /// If file isn't downloaded, then download the file.
        /// If file is downloaded, then open the file in another program.
        /// </remarks>
        public ReactiveCommand<IRandomAccessStream> ClickedUs { get; private set; }

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

#if false
            ClickedUs
                .Where(f => _localFile != null)
                .Select(f => )
                .SelectMany(f => Launcher.LaunchFileAsync(f))
                .Subscribe(g =>
                {
                    if (!g)
                    {
                        throw new InvalidOperationException(string.Format("Unable to open file {0}.", _localFile.Name));
                    }
                });
#endif
        }
    }
}
