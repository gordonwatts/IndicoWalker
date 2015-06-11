using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using IWalker.Util;
using System.Reactive.Linq;

namespace Test_MRUDatabase.Util
{
    /// <summary>
    /// Test out the Rx utilities we've built.
    /// </summary>
    [TestClass]
    public class t_RxUtils
    {
        [TestMethod]
        public void CatchThrowRightAway()
        {
            var seq = new Subject<int>();
            int lastValue = -1;
            seq
                .CatchAndSwallowIfAfter(1, (Exception e) => Observable.Return(10))
                .Subscribe(v => lastValue = v);

            Assert.AreEqual(-1, lastValue);
            seq.OnError(new ArgumentException());
            Assert.AreEqual(10, lastValue);
        }

        [TestMethod]
        public void CatchThrowBeforeLimitAway()
        {
            var seq = new Subject<int>();
            int lastValue = -1;
            seq
                .CatchAndSwallowIfAfter(5, (Exception e) => Observable.Return(10))
                .Subscribe(v => lastValue = v);

            Assert.AreEqual(-1, lastValue);
            seq.OnNext(1);
            seq.OnNext(2);
            seq.OnNext(3);
            seq.OnError(new ArgumentException());
            Assert.AreEqual(10, lastValue);
        }

        [TestMethod]
        public void CatchThrowNever()
        {
            var seq = new Subject<int>();
            int lastValue = -1;
            seq
                .CatchAndSwallowIfAfter(1, (Exception e) => Observable.Return(10))
                .Subscribe(v => lastValue = v);

            Assert.AreEqual(-1, lastValue);
            seq.OnNext(1);
            seq.OnNext(2);
            seq.OnNext(3);
            Assert.AreEqual(3, lastValue);
        }

        [TestMethod]
        public void CatchThrowAfterLmit()
        {
            var seq = new Subject<int>();
            int lastValue = -1;
            seq
                .CatchAndSwallowIfAfter(1, (Exception e) => Observable.Return(10))
                .Subscribe(v => lastValue = v);

            Assert.AreEqual(-1, lastValue);
            seq.OnNext(1);
            seq.OnError(new ArgumentException());
            Assert.AreEqual(1, lastValue);
        }

        [TestMethod]
        public void CatchThrowFurtherAfterLmit()
        {
            var seq = new Subject<int>();
            int lastValue = -1;
            seq
                .CatchAndSwallowIfAfter(1, (Exception e) => Observable.Return(10))
                .Subscribe(v => lastValue = v);

            Assert.AreEqual(-1, lastValue);
            seq.OnNext(1);
            seq.OnNext(2);
            seq.OnNext(3);
            seq.OnError(new ArgumentException());
            Assert.AreEqual(3, lastValue);
        }

        [TestMethod]
        public void CatchThrowAfterZero()
        {
            var seq = new Subject<int>();
            int lastValue = -1;
            seq
                .CatchAndSwallowIfAfter(0, (Exception e) => Observable.Return(10))
                .Subscribe(v => lastValue = v);

            Assert.AreEqual(-1, lastValue);
            seq.OnError(new ArgumentException());
            Assert.AreEqual(-1, lastValue);
        }

        /// <summary>
        /// Make sure only a single subscription happens.
        /// </summary>
        [TestMethod]
        public void CatchThrowMultiSubscription()
        {
            int timesSubscribed = 0;
            var seq = Observable.Defer(() =>
            {
                timesSubscribed++;
                var sub = new ReplaySubject<int>(10);
                sub.OnNext(15);
                sub.OnError(new ArgumentException());
                return sub;
            });

            int lastValue = -1;
            seq
                .CatchAndSwallowIfAfter(1, (Exception e) => Observable.Return(10))
                .Subscribe(v => lastValue = v);

            Assert.AreEqual(1, timesSubscribed);
            Assert.AreEqual(15, lastValue);
        }

        /// <summary>
        /// Make sure that an item isn't held onto inside here!
        /// </summary>
        [TestMethod]
        public void CatchThrowNoCaching()
        {
            var seq = new Subject<int>();
            int lastValue = -1;
            var initSeq = seq
                .CatchAndSwallowIfAfter(1, (Exception e) => Observable.Return(10));

            initSeq
                .Subscribe(v => lastValue = v);

            seq.OnNext(1);

            var nextLastValue = -1;
            initSeq
                .Subscribe(v => nextLastValue = v);

            Assert.AreEqual(-1, nextLastValue);
            Assert.AreEqual(1, lastValue);
            seq.OnNext(2);
            Assert.AreEqual(2, nextLastValue);
            Assert.AreEqual(2, lastValue);
        }

        [TestMethod]
        public void CatchThrowTwoSubscriptionsBefore()
        {
            var seq = new Subject<int>();
            var initSeq = seq
                .CatchAndSwallowIfAfter(1, (Exception e) => Observable.Return(10));

            int lastValue = -1;
            initSeq
                .Subscribe(v => lastValue = v);

            var nextLastValue = -1;
            initSeq
                .Subscribe(v => nextLastValue = v);

            seq.OnNext(1);

            Assert.AreEqual(1, nextLastValue);
            Assert.AreEqual(1, lastValue);
        }

        [TestMethod]
        public void CatchThrowTwoSubscriptionsAfter()
        {
            var seq = new Subject<int>();
            var initSeq = seq
                .CatchAndSwallowIfAfter(1, (Exception e) => Observable.Return(10));

            int lastValue = -1;
            initSeq
                .Subscribe(v => lastValue = v);

            var nextLastValue = -1;
            initSeq
                .Subscribe(v => nextLastValue = v);

            seq.OnNext(2);
            seq.OnNext(1);

            Assert.AreEqual(1, nextLastValue);
            Assert.AreEqual(1, lastValue);
        }
    }
}
