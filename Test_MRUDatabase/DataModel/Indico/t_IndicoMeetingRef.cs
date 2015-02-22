using IWalker.DataModel.Inidco;
using IWalker.DataModel.Interfaces;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MRUDatabase.DataModel.Indico
{
    /// <summary>
    /// Test out some of the indico interaction stuff
    /// </summary>
    [TestClass]
    public class t_IndicoMeetingRef
    {
        [TestMethod]
        public async Task HeaderDateOnPublicConference()
        {
            // Make sure that we can get back the header date properly.
            var fileUri = "http://indico.cern.ch/event/336571/session/1/contribution/1/material/slides/0.pdf";
            var f = new IndicoMeetingRef.IndicoFile(fileUri, "thisIsAFile") as IFile;

            Assert.AreEqual("1/28/2015 4:53:19 PM +01:00", await f.GetFileDate());
        }
    }
}
