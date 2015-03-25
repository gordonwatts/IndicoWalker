using IWalker.DataModel.Inidco;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Test_MRUDatabase.DataModel.Indico
{
    [TestClass]
    public class t_IndicoApiKeyAccess
    {
        [TestInitialize]
        public void ResetApiKeyStore()
        {
            IndicoApiKeyAccess.RemoveAllKeys();
        }

        [TestMethod]
        public void LoadMissingApiKey()
        {
            var k = IndicoApiKeyAccess.GetKey("dummy");
            Assert.IsNull(k);
        }

        [TestMethod]
        public void LoadPresentApiKey()
        {
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            var fk = IndicoApiKeyAccess.GetKey("indico.cern.ch");
            Assert.IsNotNull(fk);
            Assert.AreEqual("key", fk.ApiKey);
            Assert.AreEqual("noway", fk.SecretKey);
            Assert.AreEqual("indico.cern.ch", fk.Site);
        }

        [TestMethod]
        public void LoadPresentApi2KeysFirst()
        {
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            k.ApiKey = "key2";
            k.Site = "indico.fnal.gov";
            IndicoApiKeyAccess.UpdateKey(k);

            var fk = IndicoApiKeyAccess.GetKey("indico.cern.ch");
            Assert.IsNotNull(fk);
            Assert.AreEqual("key", fk.ApiKey);
            Assert.AreEqual("noway", fk.SecretKey);
            Assert.AreEqual("indico.cern.ch", fk.Site);
        }

        [TestMethod]
        public void LoadPresentApi2KeysSecond()
        {
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            k.ApiKey = "key2";
            k.Site = "indico.fnal.gov";
            IndicoApiKeyAccess.UpdateKey(k);

            var fk = IndicoApiKeyAccess.GetKey("indico.fnal.gov");
            Assert.IsNotNull(fk);
            Assert.AreEqual("key2", fk.ApiKey);
            Assert.AreEqual("noway", fk.SecretKey);
            Assert.AreEqual("indico.fnal.gov", fk.Site);
        }

        [TestMethod]
        public void UpdateExistingKey()
        {
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            k.ApiKey = "bogus";
            IndicoApiKeyAccess.UpdateKey(k);
            var fk = IndicoApiKeyAccess.GetKey("indico.cern.ch");
            Assert.IsNotNull(fk);
            Assert.AreEqual("bogus", fk.ApiKey);
            Assert.AreEqual("noway", fk.SecretKey);
            Assert.AreEqual("indico.cern.ch", fk.Site);
        }

        [TestMethod]
        public void RemoveKey()
        {
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            IndicoApiKeyAccess.RemoveKey("indico.cern.ch");
            var fk = IndicoApiKeyAccess.GetKey("indico.cern.ch");
            Assert.IsNull(fk);
        }

        [TestMethod]
        public void RemoveAllKeys()
        {
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            IndicoApiKeyAccess.RemoveAllKeys();
            var fk = IndicoApiKeyAccess.GetKey("indico.cern.ch");
            Assert.IsNull(fk);
        }

        [TestMethod]
        public void RemoveAllKeysDontTouchOthers()
        {
            ApplicationData.Current.RoamingSettings.Values["bogus"] = "yo dude";
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            IndicoApiKeyAccess.RemoveAllKeys();
            Assert.IsTrue(ApplicationData.Current.RoamingSettings.Values.ContainsKey("bogus"));
            ApplicationData.Current.RoamingSettings.Values.Remove("bogus");
        }
    }
}
