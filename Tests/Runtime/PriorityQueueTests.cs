// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System;
using NUnit.Framework;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class PriorityQueueTests
    {
        [Test]
        public void NewQueue_IsEmpty()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(queue.IsEmpty, Is.True);
        }

        [Test]
        public void Enqueue_IncrementsCount()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("a", 1);
            queue.Enqueue("b", 2);
            Assert.That(queue.Count, Is.EqualTo(2));
            Assert.That(queue.IsEmpty, Is.False);
        }

        [Test]
        public void Dequeue_ReturnsLowestPriorityFirst()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("medium", 5);
            queue.Enqueue("lowest", 1);
            queue.Enqueue("highest", 10);

            Assert.That(queue.Dequeue(), Is.EqualTo("lowest"));
            Assert.That(queue.Dequeue(), Is.EqualTo("medium"));
            Assert.That(queue.Dequeue(), Is.EqualTo("highest"));
        }

        [Test]
        public void Dequeue_IsFifoWithinSamePriority()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("first", 1);
            queue.Enqueue("second", 1);
            queue.Enqueue("third", 1);

            Assert.That(queue.Dequeue(), Is.EqualTo("first"));
            Assert.That(queue.Dequeue(), Is.EqualTo("second"));
            Assert.That(queue.Dequeue(), Is.EqualTo("third"));
        }

        [Test]
        public void Dequeue_DecrementsCount()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("a", 1);
            queue.Enqueue("b", 1);

            queue.Dequeue();
            Assert.That(queue.Count, Is.EqualTo(1));
            queue.Dequeue();
            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(queue.IsEmpty, Is.True);
        }

        [Test]
        public void Dequeue_OnEmptyQueue_Throws()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }

        [Test]
        public void Peek_ReturnsWithoutRemoving()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("a", 1);

            Assert.That(queue.Peek(), Is.EqualTo("a"));
            Assert.That(queue.Count, Is.EqualTo(1));
            Assert.That(queue.Peek(), Is.EqualTo("a"));
        }

        [Test]
        public void Peek_OnEmptyQueue_Throws()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            Assert.Throws<InvalidOperationException>(() => queue.Peek());
        }

        [Test]
        public void Clear_EmptiesQueue()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("a", 1);
            queue.Enqueue("b", 2);

            queue.Clear();

            Assert.That(queue.Count, Is.EqualTo(0));
            Assert.That(queue.IsEmpty, Is.True);
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }

        // A priority level whose queue drains must be removed, or the next Dequeue
        // would hit an empty inner queue for that key.
        [Test]
        public void Dequeue_AfterPriorityLevelDrains_MovesToNextLevel()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("low", 1);
            queue.Enqueue("high", 2);

            Assert.That(queue.Dequeue(), Is.EqualTo("low"));
            Assert.That(queue.Peek(), Is.EqualTo("high"));
            Assert.That(queue.Dequeue(), Is.EqualTo("high"));
        }

        [Test]
        public void ReEnqueue_AfterClear_Works()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("a", 1);
            queue.Clear();
            queue.Enqueue("b", 3);

            Assert.That(queue.Count, Is.EqualTo(1));
            Assert.That(queue.Dequeue(), Is.EqualTo("b"));
        }

        [Test]
        public void NegativeAndZeroPriorities_OrderCorrectly()
        {
            PriorityQueue<string, int> queue = new PriorityQueue<string, int>();
            queue.Enqueue("zero", 0);
            queue.Enqueue("negative", -5);
            queue.Enqueue("positive", 5);

            Assert.That(queue.Dequeue(), Is.EqualTo("negative"));
            Assert.That(queue.Dequeue(), Is.EqualTo("zero"));
            Assert.That(queue.Dequeue(), Is.EqualTo("positive"));
        }
    }
}
