using System.Collections.Generic;
using System.Linq;

namespace TombLauncher.Core.Collections;

public class CircularBuffer<T>
{
    private readonly T[] _buffer;
    private readonly object _lock = new();
    private int _head;
    private int _count;

    public CircularBuffer(int capacity) => _buffer = new T[capacity];

    public T this[int index]
    {
        get
        {
            lock (_lock)
                return _buffer[(_head - _count + index + _buffer.Length) % _buffer.Length];
        }
    }

    public void Add(T item)
    {
        lock (_lock)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % _buffer.Length;
            if (_count < _buffer.Length) _count++;
        }
    }

    public IEnumerable<T> Items
    {
        get
        {
            lock (_lock)
                return Enumerable.Range(0, _count)
                    .Select(i => _buffer[(_head - _count + i + _buffer.Length) % _buffer.Length])
                    .ToList();
        }
    }

    public int Count
    {
        get { lock (_lock) return _count; }
    }
}
