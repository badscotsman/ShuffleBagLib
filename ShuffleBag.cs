using System;
using System.Collections.Generic;

namespace ShuffleBagLib
{
    /// <summary>
    /// Represents a collection that allows random, non-repeating selection of items until all items have been drawn,
    /// after which the selection is reshuffled.
    /// </summary>
    /// <remarks>
    /// A ShuffleBag is useful for scenarios where you want to randomly select items without
    /// immediate repeats, such as in games or randomized playlists. After all items have been selected once, the bag is
    /// reshuffled and selection continues.
    ///
    /// This class is thread-safe: all access to internal state is synchronized. Thread safety is achieved via explicit
    /// locking around all shared state.
    /// </remarks>
    /// <typeparam name="T">The type of items contained in the ShuffleBag.</typeparam>
    public class ShuffleBag<T>
    {
        private readonly List<T> _items;
        private readonly Random _random;
        private readonly object _syncRoot = new object();   // i.e., lock
        private int _currentIndex;

        /// <summary>
        /// Initializes a new instance of the ShuffleBag class with an empty collection of items.
        /// </summary>
        /// <remarks>
        /// This constructor creates a new ShuffleBag ready to accept items and perform random
        /// selection operations. The internal random number generator is initialized for shuffling purposes.
        /// </remarks>
        public ShuffleBag()
        {
            _items = new List<T>();
            _random = new Random();
            _currentIndex = -1;
        }

        /// <summary>
        /// Adds an item to the ShuffleBag.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item)
        {
            lock (_syncRoot)
            {
                _items.Add(item);
                _currentIndex = _items.Count - 1;
            }
        }

        /// <summary>
        /// Returns the next item from the ShuffleBag.
        /// </summary>
        /// <returns>The next randomly selected item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the bag is empty.</exception>
        public T NextItem()
        {
            lock (_syncRoot)
            {
                if (_items.Count == 0)
                {
                    throw new InvalidOperationException("The bag is empty.");
                }

                if (_currentIndex < 1)
                {
                    _currentIndex = _items.Count - 1;
                    return _items[0];
                }

                var randomIndex = _random.Next(0, _currentIndex);
                var currentItem = _items[randomIndex];

                (_items[_currentIndex], _items[randomIndex]) = (_items[randomIndex], _items[_currentIndex]);
                _currentIndex--;

                return currentItem;
            }
        }
    }
}