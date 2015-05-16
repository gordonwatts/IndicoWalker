using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ReactiveUI.Testing;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
            FileDownloadController.Reset();
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
            var fc = fucVM.FileNotCachedOrDownloading;

            fucVM.OnLoaded.Execute(null);

            // Nothing downloaded, nothing in cache.
            Assert.IsTrue(fucVM.FileNotCachedOrDownloading);
            Assert.IsFalse(fucVM.IsDownloading);

            // Trigger the download
            Debug.WriteLine("Triggering the download");
            fucVM.ClickedUs.Execute(null);

            // This should be an immediate download in this test, so look for it.
            Assert.IsFalse(fucVM.FileNotCachedOrDownloading);
            Assert.IsFalse(fucVM.IsDownloading);
        }

        [TestMethod]
        public async Task IsDownloadingSetDuringDownload()
        {
            // http://stackoverflow.com/questions/21588945/structuring-tests-or-property-for-this-reactive-ui-scenario
            var f = new dummyFile();

            var getStreamSubject = new Subject<StreamReader>();

            f.GetStream = () =>
            {
                return getStreamSubject;
            };

            var dc = new dummyCache();
            var fucVM = new FileUserControlViewModel(f, dc);

            // Simulate the subscriptions
            var t = fucVM.IsDownloading;
            var fc = fucVM.FileNotCachedOrDownloading;

            fucVM.OnLoaded.Execute(null);

            // Nothing downloaded, nothing in cache.
            Assert.IsTrue(fucVM.FileNotCachedOrDownloading);
            Assert.IsFalse(fucVM.IsDownloading);

            // Trigger the download
            Debug.WriteLine("Triggering the download");
            fucVM.ClickedUs.Execute(null);

            // Nothing downloaded, nothing in cache.
            Assert.IsTrue(fucVM.FileNotCachedOrDownloading);
            Assert.IsTrue(fucVM.IsDownloading);

            // After it should have been downloaded, check again.
            await Task.Delay(20);
            Debug.WriteLine("Sending the data");
            var data = new byte[] { 0, 1, 2, 3 };
            var mr = new MemoryStream(data);
            getStreamSubject.OnNext(new StreamReader(mr));
            getStreamSubject.OnCompleted();

            // Give a chance for anything queued up to run by advancing the scheduler.
            await TestUtils.SpinWait(() => fucVM.IsDownloading == false, 1000);
            await TestUtils.SpinWait(() => fucVM.FileNotCachedOrDownloading == false, 1000);

            // And do an final check.
            Assert.IsFalse(fucVM.IsDownloading);
            Assert.IsFalse(fucVM.FileNotCachedOrDownloading);
        }

        [TestMethod]
        public async Task IsDownloadingWithAutoDownload()
        {
            Settings.AutoDownloadNewMeeting = true;

            // http://stackoverflow.com/questions/21588945/structuring-tests-or-property-for-this-reactive-ui-scenario
            var f = new dummyFile();

            var getStreamSubject = new Subject<StreamReader>();
            f.GetStream = () =>
            {
                return getStreamSubject;
            };

            var dc = new dummyCache();
            var fucVM = new FileUserControlViewModel(f, dc);

            // Simulate the subscribing
            var t = fucVM.IsDownloading;
            var fc = fucVM.FileNotCachedOrDownloading;

            fucVM.OnLoaded.Execute(null);

            // Nothing downloaded, nothing in cache.
            Assert.IsTrue(fucVM.FileNotCachedOrDownloading);
            Assert.IsTrue(fucVM.IsDownloading);

            await Task.Delay(20);
            var data = new byte[] { 0, 1, 2, 3 };
            var mr = new MemoryStream(data);
            getStreamSubject.OnNext(new StreamReader(mr));
            getStreamSubject.OnCompleted();

            // It is amazing that we have to wait this long.
            await TestUtils.SpinWait(() => fucVM.IsDownloading == false, 400);
            await TestUtils.SpinWait(() => fucVM.FileNotCachedOrDownloading == false, 1000);

            // And do an final check.
            Assert.IsFalse(fucVM.IsDownloading);
            Assert.IsFalse(fucVM.FileNotCachedOrDownloading);
        }

    }
}
