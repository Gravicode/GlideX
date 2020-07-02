using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections {
    /// <summary>
    /// A circular-array implementation of a queue. Enqueue can be O(n).  Dequeue is O(1).
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable()]
    public class Queue : ICollection, ICloneable {
        private object[] _array;
        private int _head;       // First valid element in the queue
        private int _tail;       // Last valid element in the queue
        private int _size;       // Number of elements.

        // Keep in-sync with c_DefaultCapacity in CLR_RT_HeapBlock_Queue in TinyCLR_Runtime__HeapBlock.h
        private const int _defaultCapacity = 4;

        /// <summary>
        /// Initializes a new instance of the Queue class that is empty, has the default initial
        /// capacity, and uses the default growth factor (2x).
        /// </summary>
        public Queue() {
            this._array = new object[_defaultCapacity];
            this._head = 0;
            this._tail = 0;
            this._size = 0;
        }

        /// <summary>
        /// Gets the number of elements contained in the Queue.
        /// </summary>
        public virtual int Count => this._size;
        /// <summary>
        /// Creates a shallow copy of the Queue.
        /// </summary>
        /// <returns>A shallow copy of the Queue.</returns>
        public virtual object Clone() {
            var q = new Queue();

            if (this._size > _defaultCapacity) {
                // only re-allocate a new array if the size isn't what we need.
                // otherwise, the one allocated in the constructor will be just fine
                q._array = new object[this._size];
            }
            else {
                // if size is not the same as capacity, we need to adjust tail accordingly
                q._tail = this._size % _defaultCapacity;
            }

            q._size = this._size;

            CopyTo(q._array, 0);

            return q;
        }

        /// <summary>
        /// Gets a value indicating whether access to the Queue is synchronized (thread safe).
        /// Always return false.
        /// </summary>
        public virtual bool IsSynchronized => false;
        /// <summary>
        /// Gets an object that can be used to synchronize access to the Queue.
        /// </summary>
        public virtual object SyncRoot => this;
        /// <summary>
        /// Removes all objects from the Queue.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void Clear();

        /// <summary>
        /// Copies the Queue elements to an existing one-dimensional Array, starting at
        /// the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from Queue.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void CopyTo(Array array, int index);

        /// <summary>
        /// Adds an object to the end of the Queue.
        /// </summary>
        /// <param name="obj">The object to add to the Queue.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void Enqueue(object obj);

        /// <summary>
        /// Returns an enumerator that iterates through the Queue.
        /// </summary>
        /// <returns>An IEnumerator for the Queue.</returns>
        public virtual IEnumerator GetEnumerator() {
            var endIndex = this._tail;

            if (this._size > 0 && this._tail <= this._head)
                endIndex += this._array.Length;

            return new Array.SZArrayEnumerator(this._array, this._head, endIndex);
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <returns>The object that is removed from the beginning of the Queue.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual object Dequeue();

        /// <summary>
        /// Returns the object at the beginning of the Queue without removing it.
        /// </summary>
        /// <returns>The object at the beginning of the Queue.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual object Peek();

        /// <summary>
        /// Determines whether an element is in the Queue.
        /// </summary>
        /// <param name="obj">The Object to locate in the Queue.</param>
        /// <returns>true if obj is found in the Queue; otherwise, false.</returns>
        public virtual bool Contains(object obj) {
            if (this._size == 0)
                return false;

            if (this._head < this._tail) {
                return Array.IndexOf(this._array, obj, this._head, this._size) >= 0;
            }
            else {
                return (Array.IndexOf(this._array, obj, this._head, this._array.Length - this._head) >= 0) ||
                       (Array.IndexOf(this._array, obj, 0, this._tail) >= 0);
            }
        }

        /// <summary>
        /// Copies the Queue elements to a new array. The order of the elements in the new
        /// array is the same as the order of the elements from the beginning of the Queue
        /// to its end.
        /// </summary>
        /// <returns>A new array containing elements copied from the Queue.</returns>
        public virtual object[] ToArray() {
            var arr = new object[this._size];

            CopyTo(arr, 0);

            return arr;
        }
    }
}


