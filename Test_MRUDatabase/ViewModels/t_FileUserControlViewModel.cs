using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ReactiveUI.Testing;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_FileUserControlViewModel
    {
        [TestInitialize]
        public void SetupTesting()
        {
            // Turn off auto-download to make sure that we control when a download occurs
            // for various tests.
            Settings.AutoDownloadNewMeeting = false;
        }

        [TestMethod]
        public void DownloadOccursWhenAsked()
        {
            // The file - we can use dummy data b.c. we aren't feeding it to the PDF renderer.
            var f = new dummyFile();
            var data = new byte[] { 0, 1, 2, 3 };
            var mr = new MemoryStream(data);
            f.GetStream = () => Observable.Return(new StreamReader(mr));

            var dc = new dummyCache();
            var fucVM = new FileUserControlViewModel(f, dc);

            // Simulate the subscription setups
            var t = fucVM.IsDownloading;
            var fc = fucVM.FileNotCached;

            fucVM.OnLoaded.Execute(null);

            // Nothing downloaded, nothing in cache.
            Assert.IsTrue(fucVM.FileNotCached);
            Assert.IsFalse(fucVM.IsDownloading);

            // Trigger the download
            Debug.WriteLine("Triggering the download");
            fucVM.ClickedUs.Execute(null);

            // This should be an immediate download in this test, so look for it.
            Assert.IsFalse(fucVM.FileNotCached);
            Assert.IsFalse(fucVM.IsDownloading);
        }

        [TestMethod]
        public async Task IsDownloadingSetDuringDownload()
        {
            await new TestScheduler().WithAsync(async sched =>
            {
                // http://stackoverflow.com/questions/21588945/structuring-tests-or-property-for-this-reactive-ui-scenario
                var f = new dummyFile();

                f.GetStream = () =>
                {
                    var data = new byte[] { 0, 1, 2, 3 };
                    var mr = new MemoryStream(data);
                    return Observable.Return(new StreamReader(mr)).WriteLine("created stream reader").Delay(TimeSpan.FromMilliseconds(100), sched).WriteLine("done with delay for stream reader");
                };

                var dc = new dummyCache();
                var fucVM = new FileUserControlViewModel(f, dc);

                // Simulate the subscriptions
                var t = fucVM.IsDownloading;
                var fc = fucVM.FileNotCached;

                fucVM.OnLoaded.Execute(null);

                // Nothign downloaded, nothing in cache.
                Assert.IsTrue(fucVM.FileNotCached);
                Assert.IsFalse(fucVM.IsDownloading);

                // Trigger the download
                fucVM.ClickedUs.Execute(null);

                // Nothign downloaded, nothing in cache.
                sched.AdvanceByMs(50);
                Assert.IsTrue(fucVM.FileNotCached);
                Assert.IsTrue(fucVM.IsDownloading);

                // After it should have been downloaded, check again.
                sched.AdvanceByMs(51);

                // We have to wait 200 ms or the item isn't inserted into the cache.
                // It is amazing that we have to wait this long.
                await Task.Delay(200);

                // Give a chance for anything queued up to run by advancing the scheduler.
                sched.AdvanceByMs(1);

                // And do an final check.
                Assert.IsFalse(fucVM.IsDownloading);
                Assert.IsFalse(fucVM.FileNotCached);
            });
        }

        [TestMethod]
        public async Task IsDownloadingWithAutoDownload()
        {
            Settings.AutoDownloadNewMeeting = true;

            await new TestScheduler().WithAsync(async sched =>
            {
                // http://stackoverflow.com/questions/21588945/structuring-tests-or-property-for-this-reactive-ui-scenario
                var f = new dummyFile();

                f.GetStream = () =>
                {
                    var data = new byte[] { 0, 1, 2, 3 };
                    var mr = new MemoryStream(data);
                    return Observable.Return(new StreamReader(mr)).WriteLine("created stream reader").Delay(TimeSpan.FromMilliseconds(100), sched).WriteLine("done with delay for stream reader");
                };

                var dc = new dummyCache();
                var fucVM = new FileUserControlViewModel(f, dc);

                // Simulate the subscribing
                var t = fucVM.IsDownloading;
                var fc = fucVM.FileNotCached;

                fucVM.OnLoaded.Execute(null);

                // Nothign downloaded, nothing in cache.
                sched.AdvanceByMs(50);
                Assert.IsTrue(fucVM.FileNotCached);
                Assert.IsTrue(fucVM.IsDownloading);

                // After it should have been downloaded, check again.
                sched.AdvanceByMs(51);

                // We have to wait 200 ms or the item isn't inserted into the cache.
                // It is amazing that we have to wait this long.
                await Task.Delay(200);

                // Give a chance for anything queued up to run by advancing the scheduler.
                sched.AdvanceByMs(1);

                // And do an final check.
                Assert.IsFalse(fucVM.IsDownloading);
                Assert.IsFalse(fucVM.FileNotCached);
            });
        }

    }
}
