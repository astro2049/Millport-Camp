using System.Collections.Generic;

namespace PCG.Generators.Roads
{
    public class PriorityQueue<T> where T : System.IComparable<T>
    {
        private readonly List<(T item, int priority)> elements = new List<(T, int)>();

        public int count
        {
            get { return elements.Count; }
        }

        public void Enqueue(T item, int priority)
        {
            elements.Add((item, priority));
            HeapifyUp(elements.Count - 1);
        }

        public T Dequeue()
        {
            if (elements.Count == 0) {
                throw new System.InvalidOperationException("The queue is empty.");
            }

            T item = elements[0].item;
            elements[0] = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
            HeapifyDown(0);
            return item;
        }

        public bool Contains(T item)
        {
            return elements.Exists(element => element.item.Equals(item));
        }

        public T GetNode(T item)
        {
            int index = elements.FindIndex(element => element.item.Equals(item));
            if (index >= 0) {
                return elements[index].item;
            }
            return default(T);
        }

        public void UpdatePriority(T item, int newPriority)
        {
            int index = elements.FindIndex(element => element.item.Equals(item));
            if (index >= 0) {
                elements[index] = (item, newPriority);
                HeapifyUp(index);
                HeapifyDown(index);
            }
        }

        private void HeapifyUp(int index)
        {
            while (index > 0) {
                int parentIndex = (index - 1) / 2;
                if (elements[index].priority >= elements[parentIndex].priority) {
                    break;
                }
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIndex = elements.Count - 1;
            while (true) {
                int leftChildIndex = 2 * index + 1;
                int rightChildIndex = 2 * index + 2;
                int smallestIndex = index;

                if (leftChildIndex <= lastIndex && elements[leftChildIndex].priority < elements[smallestIndex].priority) {
                    smallestIndex = leftChildIndex;
                }
                if (rightChildIndex <= lastIndex && elements[rightChildIndex].priority < elements[smallestIndex].priority) {
                    smallestIndex = rightChildIndex;
                }
                if (smallestIndex == index) {
                    break;
                }
                Swap(index, smallestIndex);
                index = smallestIndex;
            }
        }

        private void Swap(int indexA, int indexB)
        {
            (elements[indexA], elements[indexB]) = (elements[indexB], elements[indexA]);
        }
    }
}
