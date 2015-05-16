using IWalker.Util;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ReactiveUI.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Test_MRUDatabase.Util
{
    /// <summary>
    /// Tests for some of the LINQ helpers we have
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
        public async Task Limit2SequenceToOne()
        {
            await new TestScheduler().WithAsync(async shed =>
            {
                var source = Observable.Concat(Observable.Return(5), Observable.Return(10));

                var sequence = source.LimitGlobally(s => s.WriteLine("Starting delay").Delay(TimeSpan.FromMilliseconds(100), shed).WriteLine("Done with delay"), 1);
                var results = new List<int>();

                sequence.Subscribe(v =>
                {
                    lock (results)
                    {
                        results.Add(v);
                    }
                });

                Assert.AreEqual(0, results.Count);

                // All these await's Task.Delay are to make sure that there is enough time for the multi-threaded
                // stuff to run. The semaphore that is being used just doesn't work on the test scheduler.

                Debug.WriteLine("Doing a delay of 1");
                shed.AdvanceByMs(1);
                await Task.Delay(10);
                Debug.WriteLine("Starting first 50");
                shed.AdvanceByMs(50);
                await Task.Delay(10);
                Assert.AreEqual(0, results.Count);
                Debug.WriteLine("Starting second 51");
                shed.AdvanceByMs(51);
                await Task.Delay(10);
                Debug.WriteLine("Doing the spin wait");
                await TestUtils.SpinWait(() => results.Count != 0, 1000);
                await Task.Delay(10);
                Assert.AreEqual(1, results.Count);
                Assert.IsTrue(results.Contains(5));
                shed.AdvanceByMs(100);
                await Task.Delay(10);
                await TestUtils.SpinWait(() => results.Count != 1, 1000);
                Assert.AreEqual(2, results.Count);
                Assert.IsTrue(results.Contains(10));

            });
        }

        [TestMethod]
        public async Task Limit2SequenceToTwo()
        {
            await new TestScheduler().WithAsync(async shed =>
            {
                var source = Observable.Concat(Observable.Return(5), Observable.Return(10), Observable.Return(60));

                int counter = 0;
                var sequence = source.LimitGlobally(s => s.Do(_ => counter++).Delay(TimeSpan.FromMilliseconds(100), shed).WriteLine("Done with delay"), 2);
                var results = new List<int>();

                sequence.Subscribe(v =>
                {
                    lock (results)
                    {
                        results.Add(v);
                    }
                });

                Assert.AreEqual(0, results.Count);

                await TestUtils.SpinWait(() => counter == 2, 100);
                await Task.Delay(20);
                Assert.AreEqual(2, counter);
            });
        }

        [TestMethod]
        public async Task LimitWithException()
        {
            // Make sure an exception inside is propagated outside.
            try
            {
                var seq = Observable.Return(Observable.Return(10))
                    .LimitGlobally(s => s.SelectMany(v => Observable.Throw<int>(new InvalidOperationException())), 1)
                    .FirstAsync();

                var r = await seq;
            }
            catch (InvalidOperationException e)
            {
                return;
            }
        }

        [TestMethod]
        public async Task LimitWithExceptionResetsCount()
        {
            // Make sure an exception inside is propagated outside.
            var sl = new LINQHelpers.LimitGlobalCounter(1);
            try
            {
                var seq = Observable.Return(Observable.Return(10))
                    .LimitGlobally(s => s.SelectMany(v => Observable.Throw<int>(new InvalidOperationException())), sl)
                    .LastAsync();

                var r = await seq;
            }
            catch (InvalidOperationException e)
            {
            }

            await Task.Delay(10);
            Debug.WriteLine("Doing count check now");
            Assert.AreEqual(1, sl.CurrentCount);
        }

        [TestMethod]
        public async Task LimitWithExceptionOnSourceWithGoodAndErrorInternally()
        {
            // Make sure an exception inside is propagated outside.
            var sl = new LINQHelpers.LimitGlobalCounter(1);
            try
            {
                var seq = Observable.Concat(Observable.Return(10), Observable.Throw<int>(new InvalidOperationException()))
                    .LimitGlobally(s => s.SelectMany(v => Observable.Throw<int>(new InvalidOperationException())), sl)
                    .FirstAsync();

                var r = await seq;
            }
            catch (InvalidOperationException e)
            {
            }

            await Task.Delay(10);
            Debug.WriteLine("Checking the semaphore");
            Assert.AreEqual(1, sl.CurrentCount);
            Debug.WriteLine("Done Checking the semaphore");
        }

        [TestMethod]
        public async Task LimitWithExceptionOnSourceWithGood()
        {
            // Make sure an exception inside is propagated outside.
            var sl = new LINQHelpers.LimitGlobalCounter(1);
            try
            {
                var seq = Observable.Concat(Observable.Return(10), Observable.Throw<int>(new InvalidOperationException()))
                    .LimitGlobally(s => s, sl)
                    .FirstAsync();

                var r = await seq;
            }
            catch (InvalidOperationException e)
            {
            }

            // Delay is required b.c. Finally sometimes executes after sequence is done.
            await Task.Delay(10);
            Assert.AreEqual(1, sl.CurrentCount);
        }

        [TestMethod]
        public async Task LimitWithExceptionOnSourceWithBadFirst()
        {
            // Make sure an exception inside is propagated outside.
            var sl = new LINQHelpers.LimitGlobalCounter(1);
            try
            {
                var seq = Observable.Concat(Observable.Throw<int>(new InvalidOperationException()), Observable.Return(10))
                    .LimitGlobally(s => s.SelectMany(v => Observable.Throw<int>(new InvalidOperationException())), sl)
                    .FirstAsync();

                var r = await seq;
            }
            catch (InvalidOperationException e)
            {
            }

            Assert.AreEqual(1, sl.CurrentCount);

        }
    }
}
