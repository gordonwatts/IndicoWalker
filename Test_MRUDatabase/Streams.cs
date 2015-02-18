using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWalker.Util;
using System.IO;

namespace Test_MRUDatabase
{
    [TestClass]
    public class Streams
    {
        [TestMethod]
        public async Task ByteSteamCopyTo()
        {
            byte[] data = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var raStream = data.AsRORAByteStream();
            var ms = new MemoryStream();
            await raStream.AsStream().CopyToAsync(ms);
            Assert.AreEqual(10, ms.Length);
            ms.Seek(5, SeekOrigin.Begin);
            var d = ms.ReadByte();
            Assert.AreEqual(d, 5);
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
