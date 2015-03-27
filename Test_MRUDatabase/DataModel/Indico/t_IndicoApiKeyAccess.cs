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
            int count = 0;
            IndicoApiKeyAccess.IndicoApiKeysUpdated.Subscribe(x => count++);
            var k = IndicoApiKeyAccess.GetKey("dummy");
            Assert.IsNull(k);
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void LoadPresentApiKey()
        {
            int count = 0;
            IndicoApiKeyAccess.IndicoApiKeysUpdated.Subscribe(x => count++);
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            var fk = IndicoApiKeyAccess.GetKey("indico.cern.ch");
            Assert.IsNotNull(fk);
            Assert.AreEqual("key", fk.ApiKey);
            Assert.AreEqual("noway", fk.SecretKey);
            Assert.AreEqual("indico.cern.ch", fk.Site);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void LoadPresentApi2KeysFirst()
        {
            int count = 0;
            IndicoApiKeyAccess.IndicoApiKeysUpdated.Subscribe(x => count++);
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
            Assert.AreEqual(2, count);
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
        public void LoadAllKeysWith2()
        {
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            k.ApiKey = "key2";
            k.Site = "indico.fnal.gov";
            IndicoApiKeyAccess.UpdateKey(k);

            var keys = IndicoApiKeyAccess.LoadAllKeys();
            Assert.AreEqual(2, keys.Length);
        }

        [TestMethod]
        public void LoadAllKeysWith0()
        {
            int count = 0;
            IndicoApiKeyAccess.IndicoApiKeysUpdated.Subscribe(x => count++);
            var keys = IndicoApiKeyAccess.LoadAllKeys();
            Assert.AreEqual(0, keys.Length);
            Assert.AreEqual(0, count);
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
            int count = 0;
            IndicoApiKeyAccess.IndicoApiKeysUpdated.Subscribe(x => count++);
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            IndicoApiKeyAccess.RemoveKey("indico.cern.ch");
            var fk = IndicoApiKeyAccess.GetKey("indico.cern.ch");
            Assert.IsNull(fk);
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void RemoveAllKeys()
        {
            int count = 0;
            IndicoApiKeyAccess.IndicoApiKeysUpdated.Subscribe(x => count++);
            var k = new IndicoApiKey() { ApiKey = "key", SecretKey = "noway", Site = "indico.cern.ch" };
            IndicoApiKeyAccess.UpdateKey(k);
            IndicoApiKeyAccess.RemoveAllKeys();
            var fk = IndicoApiKeyAccess.GetKey("indico.cern.ch");
            Assert.IsNull(fk);
            Assert.AreEqual(2, count);
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
