using System;
using System.Collections.Generic;
using System.Linq;  // LINQ를 사용하기 위해 추가

public class PriorityQueue<TElement, TPriority>
{
    private SortedDictionary<TPriority, Queue<TElement>> _dictionary = new();

    public void Enqueue(TElement element, TPriority priority)
    {
        if (!_dictionary.ContainsKey(priority))
        {
            _dictionary[priority] = new Queue<TElement>();
        }
        _dictionary[priority].Enqueue(element);
    }

    public TElement Dequeue()
    {
        if (_dictionary.Count == 0)
            throw new InvalidOperationException("The queue is empty.");

        var firstKey = _dictionary.Keys.Min();  // Min 사용을 위해 LINQ 필요
        var element = _dictionary[firstKey].Dequeue();

        if (_dictionary[firstKey].Count == 0)
        {
            _dictionary.Remove(firstKey);
        }

        return element;
    }

     public bool Contains(TElement element)
    {
        // 각 우선순위 큐를 순회하면서 포함된 요소가 있는지 확인
        foreach (var queue in _dictionary.Values)
        {
            if (queue.Contains(element))
                return true;
        }
        return false;
    }

    public int Count => _dictionary.Values.Sum(q => q.Count);  // Sum 사용을 위해 LINQ 필요
}
