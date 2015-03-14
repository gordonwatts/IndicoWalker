﻿using IWalker.DataModel.Categories;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using Splat;
using System.Collections.Generic;
using Windows.Storage;

namespace Test_MRUDatabase.DataModel.Categories
{
    /// <summary>
    /// Test out the category I/O stuff
    /// </summary>
    [TestClass]
    public class t_CategoryDB
    {
        /// <summary>
        /// Delete everythign there so we always start fresh.
        /// </summary>
        [TestInitialize]
        public void ResetCategoryDB()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("CategoryDBSerliazied"))
                ApplicationData.Current.RoamingSettings.Values.Remove("CategoryDBSerliazied");
            Locator.CurrentMutable.Register(() => new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
            }, typeof(JsonSerializerSettings), null);
        }

        [TestMethod]
        public void NewDB()
        {
            var l = CategoryDB.LoadCategories();
            Assert.IsNotNull(l);
            Assert.AreEqual(0, l.Count);
        }

        [TestMethod]
        public void EmptyDB()
        {
            var l = new List<CategoryConfigInfo>();
            CategoryDB.SaveCategories(l);

            var r = CategoryDB.LoadCategories();
            Assert.IsNotNull(r);
            Assert.AreEqual(0, r.Count);
        }

        [TestMethod]
        public void WriteOneCategory()
        {
            var l = new List<CategoryConfigInfo>() { 
                new CategoryConfigInfo(){ CategoryTitle="hi", DisplayOnHomePage = false, MeetingList = new myMeetingListRef()}
            };
            CategoryDB.SaveCategories(l);

            var r = CategoryDB.LoadCategories();
            Assert.IsNotNull(r);
            Assert.AreEqual(1, r.Count);
            Assert.AreEqual("hi", r[0].CategoryTitle);
            Assert.IsFalse(r[0].DisplayOnHomePage);
        }
    }
}
