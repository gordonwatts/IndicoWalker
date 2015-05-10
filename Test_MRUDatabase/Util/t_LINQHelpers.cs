using IWalker.Util;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ReactiveUI.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Test_MRUDatabase.Util
{
    /// <summary>
    /// Tests for some of the linq helpers we have
    /// </summary>
    [TestClass]
    public class t_LINQHelpers
    {
        [TestMethod]
        public async Task LimitNone()
        {
            var source = Observable.Empty<int>();

            var sequence = source.LimitGlobally(s => s.Select(i => "hi"), 10);
            var r = await sequence.ToArray();
            Assert.AreEqual(0, r.Length);
        }

        [TestMethod]
        public async Task LimitOne()
        {
            var source = Observable.Return(10);

            var sequence = source.LimitGlobally(s => s.Select(i => "hi"), 10);
            var r = await sequence.ToArray();
            Assert.AreEqual(1, r.Length);
        }

        [TestMethod]
        public async Task LimitOneExpanding()
        {
            var source = Observable.Return(10);

            var sequence = source.LimitGlobally(s => s.SelectMany(i => new string[] { "hi", "there" }), 10);
            var r = await sequence.ToArray();
            Assert.AreEqual(2, r.Length);
        }

        [TestMethod]
        public async Task LimitNothingBeforeSubscribe()
        {
            var source = Observable.Return(10);

            bool hit = false;
            var sequence = source.LimitGlobally(s => s.Do(_ => hit = true).SelectMany(i => new string[] { "hi", "there" }), 10);
            await Task.Delay(10);
            Assert.IsFalse(hit);
        }

        [TestMethod]
        public void LimitInvalidNumberZero()
        {
            try
            {
                var source = Observable.Empty<int>();

                var sequence = source.LimitGlobally(s => Observable.Return("hi"), 0);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return;
            }
            Assert.Fail("Should have thrown!");
        }

        [TestMethod]
        public void LimitInvalidNumberNegative()
        {
            try
            {
                var source = Observable.Empty<int>();

                var sequence = source.LimitGlobally(s => Observable.Return("hi"), -5);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return;
            }
            Assert.Fail("Should have thrown!");
        }

        [TestMethod]
        public void Limit2SequenceToOne()
        {
            new TestScheduler().With(shed =>
            {
                var source = Observable.Concat(Observable.Return(5), Observable.Return(10));

                var sequence = source.LimitGlobally(s => s.Delay(TimeSpan.FromMilliseconds(100), shed), 1);
                var results = new List<int>();

                sequence.Subscribe(v =>
                {
                    lock (results)
                    {
                        results.Add(v);
                    }
                });

                Assert.AreEqual(0, results.Count);

                shed.AdvanceByMs(50);
                Assert.AreEqual(0, results.Count);
                shed.AdvanceByMs(51);
                Assert.AreEqual(1, results.Count);
                Assert.IsTrue(results.Contains(5));
                shed.AdvanceByMs(100);
                Assert.AreEqual(2, results.Count);
                Assert.IsTrue(results.Contains(10));

            });
        }

        [TestMethod]
        public void Limit2SequenceToTwo()
        {
            new TestScheduler().With(shed =>
            {
                var source = Observable.Concat(Observable.Return(5), Observable.Return(10));

                var sequence = source.LimitGlobally(s => s.Delay(TimeSpan.FromMilliseconds(100), shed), 2);
                var results = new List<int>();

                sequence.Subscribe(v =>
                {
                    lock (results)
                    {
                        results.Add(v);
                    }
                });

                Assert.AreEqual(0, results.Count);

                shed.AdvanceByMs(50);
                Assert.AreEqual(0, results.Count);
                shed.AdvanceByMs(51);
                Assert.AreEqual(2, results.Count);
                Assert.IsTrue(results.Contains(5));
                Assert.IsTrue(results.Contains(10));

            });
        }
    }
}
