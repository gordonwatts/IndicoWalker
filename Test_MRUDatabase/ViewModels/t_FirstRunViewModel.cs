using IWalker.DataModel.Categories;
using IWalker.DataModel.Interfaces;
using IWalker.Util;
using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_FirstRunViewModel
    {
        /// <summary>
        /// Reset the database before each run.
        /// </summary>
        [TestInitialize]
        public async Task ResetCategoryDB()
        {
            CategoryDB.ResetCategoryDB();
            Locator.CurrentMutable.Register(() => new JsonSerializerSettings()
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
            }, typeof(JsonSerializerSettings), null);
            Locator.CurrentMutable.Register(() => new factoryGenerator(), typeof(IMeetingListRefFactory));
            await Blobs.LocalStorage.Flush();
            await Blobs.LocalStorage.InvalidateAll();
        }

        /// <summary>
        /// Dummy generator
        /// </summary>
        class factoryGenerator : IMeetingListRefFactory
        {
            public IMeetingListRef GenerateMeetingListRef(string url)
            {
                return new myMeetingListRef();
            }
        }

        [TestMethod]
        public void CTorFirstRunVM()
        {
            var x = new FirstRunViewModel(null);
        }

        [TestMethod]
        public async Task WantSampleFeedsFirstRunVM()
        {
            // Say yes.
            var dumbScreen = new dummyScreen();
            var x = new FirstRunViewModel(dumbScreen);
            x.AddDefaultCategories.Execute(null);

            // Make sure that we make it to the proper place
            await TestUtils.SpinWaitAreEqual(typeof(StartPageViewModel), () => dumbScreen.CurrentVM == null ? null : dumbScreen.CurrentVM.GetType(), 1000);

            // Make sure no categories have been loaded up.
            Assert.AreNotEqual(1, CategoryDB.LoadCategories().Count);

            // Next, check that the cache db has these guys in there already.
            var keys = await Blobs.LocalStorage.GetAllKeys();
            Assert.AreNotEqual(0, keys.Count());
        }

        [TestMethod]
        public async Task SkipIntroFeedsFirstRunVM()
        {
            var dumbScreen = new dummyScreen();
            var x = new FirstRunViewModel(dumbScreen);
            x.SkipDefaultCategories.Execute(null);

            // Make sure that we make it to the proper place
            await TestUtils.SpinWaitAreEqual(typeof(StartPageViewModel), () => dumbScreen.CurrentVM == null ? null : dumbScreen.CurrentVM.GetType(), 1000);

            // Make sure no categories have been loaded up.
            Assert.AreEqual(0, CategoryDB.LoadCategories().Count);

            // Make sure nothign got cached.
            var keys = await Blobs.LocalStorage.GetAllKeys();
            Assert.AreEqual(0, keys.Count());
        }

        class dummyScreen : IScreen
        {

            public RoutingState Router { get; private set; }

            /// <summary>
            /// Return whatever VM we are currently looking at.
            /// </summary>
            public IRoutableViewModel CurrentVM { get; set; }

            public dummyScreen()
            {
                Router = new RoutingState();

                Router.CurrentViewModel
                    .Subscribe(vm => CurrentVM = vm);
            }
        }
    }
}
