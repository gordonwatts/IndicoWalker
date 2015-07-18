using IWalker.Util;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace Test_MRUDatabase
{
    [TestClass]
    public class Streams
    {
        [TestMethod]
        public async Task ByteSteamCopyTo()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var raStream = data.AsRORAByteStream();
            var ms = new MemoryStream();
            await raStream.AsStream().CopyToAsync(ms);
            Assert.AreEqual(10, ms.Length);
            ms.Seek(5, SeekOrigin.Begin);
            var d = ms.ReadByte();
            Assert.AreEqual(d, 5);
        }

        [TestMethod]
        public async Task Read1000WhenThereAre10()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var raStream = data.AsRORAByteStream();

            byte[] destData = new byte[1024];
            var destBuffer = destData.AsBuffer();

            var r = await raStream.ReadAsync(destBuffer, 1000, Windows.Storage.Streams.InputStreamOptions.ReadAhead);

            Assert.AreEqual((uint)10, r.Length);
        }

        [TestMethod]
        public async Task ReadPartial5ReadThenFull()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var raStream = data.AsRORAByteStream();

            byte[] destData = new byte[1024];
            var destBuffer = destData.AsBuffer();

            var r = await raStream.ReadAsync(destBuffer, 5, Windows.Storage.Streams.InputStreamOptions.None);
            Assert.AreEqual((uint)5, r.Length);
            r = await raStream.ReadAsync(destBuffer, 1000, Windows.Storage.Streams.InputStreamOptions.None);
            Assert.AreEqual((uint)5, r.Length);
        }

        [TestMethod]
        public async Task ReadPartial9ReadThenFull()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var raStream = data.AsRORAByteStream();

            byte[] destData = new byte[1024];
            var destBuffer = destData.AsBuffer();

            var r = await raStream.ReadAsync(destBuffer, 9, Windows.Storage.Streams.InputStreamOptions.None);
            Assert.AreEqual((uint)9, r.Length);
            r = await raStream.ReadAsync(destBuffer, 1000, Windows.Storage.Streams.InputStreamOptions.None);
            Assert.AreEqual((uint)1, r.Length);
        }

        [TestMethod]
        public async Task ReadPartial10ReadThenFull()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var raStream = data.AsRORAByteStream();

            byte[] destData = new byte[1024];
            var destBuffer = destData.AsBuffer();

            var r = await raStream.ReadAsync(destBuffer, 10, Windows.Storage.Streams.InputStreamOptions.None);
            Assert.AreEqual((uint)10, r.Length);
            r = await raStream.ReadAsync(destBuffer, 1000, Windows.Storage.Streams.InputStreamOptions.None);
            Assert.IsNull(r);
        }

        [TestMethod]
        public async Task ByteStreamCloneAndCopy()
        {
            byte[] data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var raStream = data.AsRORAByteStream();

            var s2 = raStream.CloneStream();
            s2.Seek(5);

            var ms1 = new MemoryStream();
            var ms2 = new MemoryStream();

            await raStream.AsStream().CopyToAsync(ms1);
            await s2.AsStream().CopyToAsync(ms2);

            ms1.Seek(0, SeekOrigin.Begin);
            ms2.Seek(0, SeekOrigin.Begin);

            var d = ms1.ReadByte();
            Assert.AreEqual(0, d);

            d = ms2.ReadByte();
            Assert.AreEqual(5, d);
        }
    }
}
