using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Background;

namespace IWalker.Util
{
    /// <summary>
    /// Some simple utilities to deal with background tasks
    /// </summary>
    static class BackgroundTasks
    {
        /// <summary>
        /// Register all our background tasks
        /// </summary>
        public static void Register()
        {
            // Unregister everything to be just sure
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                task.Value.Unregister(true);
            }

            // Clean up the database once a date, removing old items from the cache.
            MaintenanceTrigger trigger = new MaintenanceTrigger(24 * 60, false);
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = "Clean IndicoWalker's Local Cache of Expired Items";
            builder.TaskEntryPoint = "IWalker.BackgroundTasks.CleanDBBackgroundTask";
            builder.SetTrigger(trigger);
            var ret = builder.Register();
        }
    }
}
