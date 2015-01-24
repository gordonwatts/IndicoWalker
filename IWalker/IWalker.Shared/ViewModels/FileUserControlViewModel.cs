﻿using IWalker.DataModel.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Reactive.Linq;
using Windows.Storage;
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
        private StorageFile _localFile = null;

        /// <summary>
        /// Initialize all of our behaviors.
        /// </summary>
        /// <param name="file"></param>
        public FileUserControlViewModel(IFile file)
        {
            _file = file;

            ClickedUs = ReactiveCommand.Create();
            ClickedUs
                .Where(_ => _localFile == null)
                .SelectMany(async _ => await _file.DownloadFile())
                .Subscribe(f => _localFile = f);
            ClickedUs
                .Where(_ => _localFile != null)
                .Select(_ => _localFile)
                .SelectMany(async f => Tuple.Create(await Launcher.LaunchFileAsync(f), f))
                .Subscribe(g =>
                {
                    if (!g.Item1)
                    {
                        throw new InvalidOperationException(string.Format("Didn't know how to open file of type {0}", g.Item2.DisplayName));
                    }
                });
        }

        /// <summary>
        /// Command to fire when the user "clicks" on us.
        /// </summary>
        /// <remarks>
        /// If file isn't downloaded, then download the file.
        /// If file is downloaded, then open the file in another program.
        /// </remarks>
        public ReactiveCommand<object> ClickedUs { get; private set; }
    }
}
