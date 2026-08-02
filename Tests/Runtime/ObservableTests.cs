// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System;
using NUnit.Framework;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class ObservableTests
    {
        [Test]
        public void Constructor_SetsInitialValue()
        {
            Observable<int> observable = new Observable<int>(42);
            Assert.That(observable.Value, Is.EqualTo(42));
        }

        [Test]
        public void Constructor_WithHandler_DoesNotFireImmediately()
        {
            int calls = 0;
            Observable<int> observable = new Observable<int>(1, value => calls++);
            Assert.That(calls, Is.EqualTo(0));
            Assert.That(observable.Value, Is.EqualTo(1));
        }

        [Test]
        public void Set_ChangedValue_RaisesEventWithNewValue()
        {
            Observable<int> observable = new Observable<int>(1);
            int received = 0;
            int calls = 0;
            observable.AddListener(value => { received = value; calls++; });

            observable.Set(7);

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(received, Is.EqualTo(7));
            Assert.That(observable.Value, Is.EqualTo(7));
        }

        // The equality short-circuit is the whole point of Observable -- setting the
        // same value must not fire listeners.
        [Test]
        public void Set_SameValue_DoesNotRaiseEvent()
        {
            Observable<int> observable = new Observable<int>(5);
            int calls = 0;
            observable.AddListener(value => calls++);

            observable.Set(5);

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void Set_SameReference_DoesNotRaiseEvent()
        {
            string shared = "hello";
            Observable<string> observable = new Observable<string>(shared);
            int calls = 0;
            observable.AddListener(value => calls++);

            observable.Set(shared);

            Assert.That(calls, Is.EqualTo(0));
        }

        [Test]
        public void Set_NullToValue_RaisesEvent()
        {
            Observable<string> observable = new Observable<string>(null);
            int calls = 0;
            observable.AddListener(value => calls++);

            observable.Set("now set");

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(observable.Value, Is.EqualTo("now set"));
        }

        [Test]
        public void ValueSetter_GoesThroughSet()
        {
            Observable<int> observable = new Observable<int>(1);
            int calls = 0;
            observable.AddListener(value => calls++);

            observable.Value = 2;
            Assert.That(calls, Is.EqualTo(1));

            observable.Value = 2;
            Assert.That(calls, Is.EqualTo(1), "Assigning the same value must not re-fire.");
        }

        [Test]
        public void RemoveListener_StopsNotifications()
        {
            Observable<int> observable = new Observable<int>(0);
            int calls = 0;
            Action<int> handler = value => calls++;

            observable.AddListener(handler);
            observable.Set(1);
            Assert.That(calls, Is.EqualTo(1));

            observable.RemoveListener(handler);
            observable.Set(2);
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void MultipleListeners_AllReceive()
        {
            Observable<int> observable = new Observable<int>(0);
            int first = 0;
            int second = 0;
            observable.AddListener(value => first = value);
            observable.AddListener(value => second = value);

            observable.Set(9);

            Assert.That(first, Is.EqualTo(9));
            Assert.That(second, Is.EqualTo(9));
        }

        [Test]
        public void Invoke_RaisesEventWithoutChangingValue()
        {
            Observable<int> observable = new Observable<int>(3);
            int received = -1;
            observable.AddListener(value => received = value);

            observable.Invoke();

            Assert.That(received, Is.EqualTo(3));
            Assert.That(observable.Value, Is.EqualTo(3));
        }

        [Test]
        public void ImplicitConversion_YieldsValue()
        {
            Observable<int> observable = new Observable<int>(11);
            int plain = observable;
            Assert.That(plain, Is.EqualTo(11));
        }

        [Test]
        public void Dispose_ClearsListenersAndValue()
        {
            Observable<int> observable = new Observable<int>(5);
            int calls = 0;
            observable.AddListener(value => calls++);

            observable.Dispose();

            Assert.That(observable.Value, Is.EqualTo(0));

            observable.Set(99);
            Assert.That(calls, Is.EqualTo(0), "Listeners must be cleared by Dispose.");
        }

        [Test]
        public void Set_NoListeners_DoesNotThrow()
        {
            Observable<int> observable = new Observable<int>(0);
            Assert.DoesNotThrow(() => observable.Set(1));
            Assert.That(observable.Value, Is.EqualTo(1));
        }
    }
}
