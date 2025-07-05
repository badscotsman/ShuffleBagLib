using System;
using System.Collections.Generic;

namespace ShuffleBagLib
{
    public class ShuffleBag<T>
    {
        private readonly List<T> _items;
        private readonly Random _random;
        private int _currentIndex;

        public ShuffleBag()
        {
            _items = new List<T>();
            _random = new Random();
            _currentIndex = -1;
        }

        public void Add(T item)
        {
            _items.Add(item);
            _currentIndex = _items.Count - 1;
        }

        public T NextItem()
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

            int randomIndex = _random.Next(0, _currentIndex);
            T currentItem = _items[randomIndex];
            (_items[_currentIndex], _items[randomIndex]) = (_items[randomIndex], _items[_currentIndex]);
            _currentIndex--;

            return currentItem;
        }
    }
}