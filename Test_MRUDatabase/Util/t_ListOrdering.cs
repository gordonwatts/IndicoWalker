using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWalker.Util;

namespace Test_MRUDatabase.Util
{
    /// <summary>
    /// Test the list ordering helper methods.
    /// </summary>
    [TestClass]
    public class t_ListOrdering
    {
        [TestMethod]
        public void EmptyEnumerables()
        {
            var orig = new List<myObj>();
            var desired = new List<myObj>();

            var r = orig.MakeLookLike(desired);
            Assert.AreEqual(0, r.Count());
        }

        [TestMethod]
        public void EmptyLists()
        {
            var orig = new List<myObj>();
            var desired = new List<myObj>();

            orig.MakeListLookLike(desired);
            Assert.AreEqual(0, orig.Count());
        }

        [TestMethod]
        public void EmptyEnumerableToOneItem()
        {
            var orig = new List<myObj>();
            var desired = new List<myObj>() { new myObj(10) };

            var r = orig.MakeLookLike(desired).ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(10, r[0].Value);
            Assert.AreEqual(desired[0], r[0]); // Reference check
        }

        [TestMethod]
        public void EmptyListToOneItem()
        {
            var orig = new List<myObj>();
            var desired = new List<myObj>() { new myObj(10) };

            orig.MakeListLookLike(desired);
            Assert.AreEqual(1, orig.Count);
            Assert.AreEqual(desired[0], orig[0]); // Reference check
        }

        [TestMethod]
        public void OneItemToEmptyEnumerable()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var desired = new List<myObj>();

            orig.MakeListLookLike(desired);
            Assert.AreEqual(0, orig.Count);
        }

        [TestMethod]
        public void OneItemToEmptyList()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var desired = new List<myObj>();

            var r = orig.MakeLookLike(desired).ToArray();
            Assert.AreEqual(0, r.Length);
        }

        [TestMethod]
        public void OneEnumerableItemToOneItemSame()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var desired = new List<myObj>() { new myObj(10) };

            var r = orig.MakeLookLike(desired).ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(orig[0], r[0]); // Reference check
        }

        [TestMethod]
        public void OneListItemToOneItemSame()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var origSaved = new List<myObj>();
            origSaved.AddRange(orig);
            var desired = new List<myObj>() { new myObj(10) };

            orig.MakeListLookLike(desired);
            Assert.AreEqual(1, orig.Count);
            Assert.AreEqual(origSaved[0], orig[0]); // Reference check
        }

        [TestMethod]
        public void OneEnumerableItemToOneItemDifferent()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var desired = new List<myObj>() { new myObj(20) };

            var r = orig.MakeLookLike(desired).ToArray();
            Assert.AreEqual(1, r.Length);
            Assert.AreEqual(desired[0], r[0]); // Reference check
        }

        [TestMethod]
        public void OneListItemToOneItemDifferent()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var desired = new List<myObj>() { new myObj(20) };

            orig.MakeListLookLike(desired);
            Assert.AreEqual(1, orig.Count);
            Assert.AreEqual(desired[0], orig[0]); // Reference check
        }

        [TestMethod]
        public void OneEnumerableItemToTwoInFront()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var desired = new List<myObj>() { new myObj(20), new myObj(10) };

            var r = orig.MakeLookLike(desired).ToArray();
            Assert.AreEqual(2, r.Length);
            Assert.AreEqual(desired[0], r[0]);
            Assert.AreEqual(orig[0], r[1]);
        }

        [TestMethod]
        public void OneListItemToTwoInFront()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var origCopy = new List<myObj>();
            origCopy.AddRange(orig);
            var desired = new List<myObj>() { new myObj(20), new myObj(10) };

            orig.MakeListLookLike(desired);
            Assert.AreEqual(2, orig.Count);
            Assert.AreEqual(desired[0], orig[0]);
            Assert.AreEqual(origCopy[0], orig[1]);
        }

        [TestMethod]
        public void OneEnumerableItemToTwoInBack()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var desired = new List<myObj>() { new myObj(10), new myObj(20) };

            var r = orig.MakeLookLike(desired).ToArray();
            Assert.AreEqual(2, r.Length);
            Assert.AreEqual(orig[0], r[0]);
            Assert.AreEqual(desired[1], r[1]);
        }

        [TestMethod]
        public void OneListItemToTwoInBack()
        {
            var orig = new List<myObj>() { new myObj(10) };
            var origSaved = new List<myObj>();
            origSaved.AddRange(orig);
            var desired = new List<myObj>() { new myObj(10), new myObj(20) };

            orig.MakeListLookLike(desired);
            Assert.AreEqual(2, orig.Count);
            Assert.AreEqual(origSaved[0], orig[0]);
            Assert.AreEqual(desired[1], orig[1]);
        }

        [TestMethod]
        public void TwoEnumerableItemsToThreeInMiddle()
        {
            var orig = new List<myObj>() { new myObj(10), new myObj(20) };
            var desired = new List<myObj>() { new myObj(10), new myObj(15), new myObj(20) };

            var r = orig.MakeLookLike(desired).ToArray();
            Assert.AreEqual(3, r.Length);
            Assert.AreEqual(orig[0], r[0]);
            Assert.AreEqual(desired[1], r[1]);
            Assert.AreEqual(orig[1], r[2]);
        }

        [TestMethod]
        public void TwoListItemsToThreeInMiddle()
        {
            var orig = new List<myObj>() { new myObj(10), new myObj(20) };
            var origSaved = new List<myObj>();
            origSaved.AddRange(orig);
            var desired = new List<myObj>() { new myObj(10), new myObj(15), new myObj(20) };

            orig.MakeListLookLike(desired);
            Assert.AreEqual(3, orig.Count);
            Assert.AreEqual(origSaved[0], orig[0]);
            Assert.AreEqual(desired[1], orig[1]);
            Assert.AreEqual(origSaved[1], orig[2]);
        }

        [TestMethod]
        public void FourToTwoWithDifferentEnumerableTypes()
        {

            var orig = new List<myObj>() { new myObj(10), new myObj(20), new myObj(30), new myObj(40) };
            var desired = new List<myObj2>() { new myObj2(20), new myObj2(45) };

            var r = orig.MakeLookLike(desired, (oItem, dItem) => oItem.Value == dItem.Value, dItem => new myObj(dItem.Value)).ToArray();

            Assert.AreEqual(2, r.Length);
            Assert.AreEqual(orig[1], r[0]);
            Assert.AreEqual(45, r[1].Value);
        }

        [TestMethod]
        public void FourToTwoWithDifferentListTypes()
        {

            var orig = new List<myObj>() { new myObj(10), new myObj(20), new myObj(30), new myObj(40) };
            var origSaved = new List<myObj>();
            origSaved.AddRange(orig);
            var desired = new List<myObj2>() { new myObj2(20), new myObj2(45) };

            orig.MakeListLookLike(desired, (oItem, dItem) => oItem.Value == dItem.Value, dItem => new myObj(dItem.Value));

            Assert.AreEqual(2, orig.Count);
            Assert.AreEqual(origSaved[1], orig[0]);
            Assert.AreEqual(45, orig[1].Value);
        }

        /// <summary>
        /// Object we can use for tests.
        /// </summary>
        class myObj : IEquatable<myObj>
        {
            public myObj(int init)
            {
                Value = init;
            }

            public int Value { get; set; }

            public bool Equals(myObj other)
            {
                return other.Value == Value;
            }
        }

        /// <summary>
        /// Object we can use for tests.
        /// </summary>
        class myObj2 : IEquatable<myObj>
        {
            public myObj2(int init)
            {
                Value = init;
            }

            public int Value { get; set; }

            public bool Equals(myObj other)
            {
                return other.Value == Value;
            }
        }
    }
}
