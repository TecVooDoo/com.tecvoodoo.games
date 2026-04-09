// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on PriorityQueue by Adam Myhre (adammyhre)

using System.Collections.Generic;

namespace TecVooDoo.Games
{
    /// <summary>
    /// Generic priority queue. Elements are dequeued in priority order (lowest first).
    /// Supports multiple elements at the same priority level via internal queues.
    /// </summary>
    public class PriorityQueue<TElement, TPriority>
    {
        readonly SortedList<TPriority, Queue<TElement>> priorities = new SortedList<TPriority, Queue<TElement>>();

        public int Count { get; private set; }

        public void Enqueue(TElement element, TPriority priority)
        {
            if (!priorities.ContainsKey(priority))
                priorities[priority] = new Queue<TElement>();

            priorities[priority].Enqueue(element);
            Count++;
        }

        public TElement Dequeue()
        {
            if (priorities.Count == 0)
                throw new System.InvalidOperationException("The queue is empty.");

            TPriority firstKey = priorities.Keys[0];
            Queue<TElement> firstQueue = priorities[firstKey];
            TElement element = firstQueue.Dequeue();

            if (firstQueue.Count == 0)
                priorities.Remove(firstKey);

            Count--;
            return element;
        }

        public TElement Peek()
        {
            if (priorities.Count == 0)
                throw new System.InvalidOperationException("The queue is empty.");

            return priorities[priorities.Keys[0]].Peek();
        }

        public bool IsEmpty => Count == 0;

        public void Clear()
        {
            priorities.Clear();
            Count = 0;
        }
    }
}
