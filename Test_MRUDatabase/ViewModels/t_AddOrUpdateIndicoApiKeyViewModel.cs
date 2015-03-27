using IWalker.DataModel.Inidco;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_AddOrUpdateIndicoApiKeyViewModel
    {
        [TestInitialize]
        public void ResetApiKeyStore()
        {
            IndicoApiKeyAccess.RemoveAllKeys();
        }

        [TestMethod]
        public void CreateWithNullApiKey()
        {
            var vm = new AddOrUpdateIndicoApiKeyViewModel(null);
            var init1 = vm.AddOrUpdateText;
            bool canExeAdd = false;
            vm.AddUpdateCommand.CanExecuteObservable.Subscribe(v => canExeAdd = v);
            bool canExeDelete = false;
            vm.DeleteCommand.CanExecuteObservable.Subscribe(v => canExeDelete = v);
            
            Assert.AreEqual("", vm.SiteName);
            Assert.AreEqual("", vm.SecretKey);
            Assert.AreEqual("", vm.ApiKey);
            Assert.AreEqual("Add", vm.AddOrUpdateText);
            Assert.IsFalse(canExeAdd);
            Assert.IsFalse(canExeDelete);
        }

        [TestMethod]
        public void CreateWithExitingApiKey()
        {
            var apiKey = new IndicoApiKey() { Site = "full moon", ApiKey = "1234", SecretKey = "5678" };
            IndicoApiKeyAccess.UpdateKey(apiKey);

            var vm = new AddOrUpdateIndicoApiKeyViewModel(apiKey);
            var init1 = vm.AddOrUpdateText;
            bool canExeAdd = false;
            vm.AddUpdateCommand.CanExecuteObservable.Subscribe(v => canExeAdd = v);
            bool canExeDelete = false;
            vm.DeleteCommand.CanExecuteObservable.Subscribe(v => canExeDelete = v);

            Assert.AreEqual("full moon", vm.SiteName);
            Assert.AreEqual("5678", vm.SecretKey);
            Assert.AreEqual("1234", vm.ApiKey);
            Assert.AreEqual("Update", vm.AddOrUpdateText);
            Assert.IsTrue(canExeAdd);
            Assert.IsTrue(canExeDelete);
        }

        [TestMethod]
        public void NewKeyEnteredIntoDatabase()
        {
            var vm = new AddOrUpdateIndicoApiKeyViewModel(null);
            var init1 = vm.AddOrUpdateText;
            bool canExeAdd = false;
            vm.AddUpdateCommand.CanExecuteObservable.Subscribe(v => canExeAdd = v);
            bool canExeDelete = false;
            vm.DeleteCommand.CanExecuteObservable.Subscribe(v => canExeDelete = v);

            vm.SiteName = "full moon";
            Assert.IsFalse(canExeAdd);
            vm.SecretKey = "1234";
            Assert.IsFalse(canExeAdd);
            vm.ApiKey = "5678";
            Assert.IsTrue(canExeAdd);

            vm.AddUpdateCommand.Execute(null);

            var o = IndicoApiKeyAccess.GetKey("full moon");
            Assert.IsNotNull(o);
            Assert.AreEqual("full moon", o.Site);
            Assert.AreEqual("1234", o.SecretKey);
            Assert.AreEqual("5678", o.ApiKey);
        }

        [TestMethod]
        public void DeleteKeyInDatabase()
        {
            var apiKey = new IndicoApiKey() { Site = "full moon", ApiKey = "1234", SecretKey = "5678" };
            IndicoApiKeyAccess.UpdateKey(apiKey);

            var vm = new AddOrUpdateIndicoApiKeyViewModel(apiKey);
            var init1 = vm.AddOrUpdateText;
            bool canExeAdd = false;
            vm.AddUpdateCommand.CanExecuteObservable.Subscribe(v => canExeAdd = v);
            bool canExeDelete = false;
            vm.DeleteCommand.CanExecuteObservable.Subscribe(v => canExeDelete = v);

            Assert.IsTrue(canExeDelete);

            Assert.IsNotNull(IndicoApiKeyAccess.GetKey("full moon"));
            vm.DeleteCommand.Execute(null);
            Assert.IsNull(IndicoApiKeyAccess.GetKey("full moon"));

            Assert.IsFalse(canExeDelete);
            Assert.AreEqual("Add", vm.AddOrUpdateText);
        }
    }
}
