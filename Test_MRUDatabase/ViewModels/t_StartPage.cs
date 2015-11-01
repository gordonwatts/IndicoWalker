using IWalker.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using Test_MRUDatabase.Util;

namespace Test_MRUDatabase.ViewModels
{
    [TestClass]
    public class t_StartPage
    {
        [TestMethod]
        public void CTor()
        {
            var ds = new dummyScreen();
            var t = new StartPageViewModel(ds);
        }

    }
}
