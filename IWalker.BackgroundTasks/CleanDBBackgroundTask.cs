using Akavache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using Windows.ApplicationModel.Background;

namespace IWalker.BackgroundTasks
{
    /// <summary>
    /// The background task that is called to clean the database once a day.
    /// </summary>
    public sealed class CleanDBBackgroundTask : IBackgroundTask
    {
        /// <summary>
        /// The idea is, under power, to clean out the local DB cache.
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BlobCache.UserAccount.Vacuum().FirstAsync().Wait();
        }
    }
}
