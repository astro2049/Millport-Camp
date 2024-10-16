using System.Collections.Generic;

namespace PCG.Generators.Roads
{
    public class PriorityQueue<T> where T : System.IComparable<T>
    {
        private readonly List<T> elements = new List<T>();

        public int count
        {
            get { return elements.Count; }
        }

        public void Enqueue(T item)
        {
            elements.Add(item);
            BubbleUp(elements.Count - 1);
        }

        public T Dequeue()
        {
            if (elements.Count == 0) {
                throw new System.InvalidOperationException("The queue is empty.");
            }

            T item = elements[0];
            elements[0] = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
            BubbleDown(0);
            return item;
        }

        public bool Contains(T item)
        {
            return elements.Exists(element => element.Equals(item));
        }

        public T GetNode(T item)
        {
            int index = elements.FindIndex(element => element.Equals(item));
            if (index >= 0) {
                return elements[index];
            }
            return default(T);
        }

        public void UpdatePriority(T item)
        {
            int index = elements.FindIndex(element => element.Equals(item));
            if (index >= 0) {
                elements[index] = item;
                BubbleUp(index);
                BubbleDown(index);
            }
        }

        private void BubbleUp(int index)
        {
            while (index > 0) {
                int parentIndex = (index - 1) / 2;
                if (elements[index].CompareTo(elements[parentIndex]) < 0) {
                    Swap(index, parentIndex);
                    index = parentIndex;
                } else {
                    break;
                }
            }
        }

        private void BubbleDown(int index)
        {
            int lastIndex = elements.Count - 1;
            while (true) {
                int leftChildIndex = 2 * index + 1;
                int rightChildIndex = 2 * index + 2;
                int newIndex = index;

                if (leftChildIndex <= lastIndex && elements[newIndex].CompareTo(elements[leftChildIndex]) > 0) {
                    newIndex = leftChildIndex;
                }
                if (rightChildIndex <= lastIndex && elements[newIndex].CompareTo(elements[rightChildIndex]) > 0) {
                    newIndex = rightChildIndex;
                }
                if (newIndex == index) {
                    break;
                }
                Swap(index, newIndex);
                index = newIndex;
            }
        }

        private void Swap(int indexA, int indexB)
        {
            (elements[indexA], elements[indexB]) = (elements[indexB], elements[indexA]);
        }
    }
}
