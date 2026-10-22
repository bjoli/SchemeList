using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SchemeList;

/// <summary>
/// A mutable builder for constructing <see cref="SchemeList{T}"/> instances efficiently from front to back
/// by mutating the tail cons cell until converted.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
/// <remarks>
/// The last cell has a null cdr until <see cref="ToSchemeList"/> sets it to the
/// empty list. Each <see cref="Add"/> thus writes one reference less than
/// with the empty list in the cdr: a null store needs no GC write barrier.
/// It also does not read <c>Nil&lt;T&gt;.Instance</c>, which in shared generic
/// code needs a runtime type lookup.
/// </remarks>
[NonUserCode]
public sealed class SchemeListBuilder<T> : IEnumerable<T>, IReadOnlyCollection<T>
{
    private Cons<T>? _head;
    private Cons<T>? _tail;
    private int _count;
    private bool _isInvalidated;

    /// <summary>
    /// Initializes a new empty instance of <see cref="SchemeListBuilder{T}"/>.
    /// </summary>
    public SchemeListBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SchemeListBuilder{T}"/> populated with items from the specified collection.
    /// </summary>
    public SchemeListBuilder(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        AddRange(items);
    }

    /// <summary>
    /// Gets the number of elements contained in the builder.
    /// Throws <see cref="InvalidOperationException"/> if the builder has already been invalidated.
    /// </summary>
    public int Count
    {
        get
        {
            EnsureNotInvalidated();
            return _count;
        }
    }

    /// <summary>
    /// Returns true if the builder contains no elements.
    /// Throws <see cref="InvalidOperationException"/> if the builder has already been invalidated.
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Adds an element to the end of the list being built.
    /// Mutates the last pair so that the list is constructed front-to-back in O(1) time per item.
    /// Throws <see cref="InvalidOperationException"/> if the builder has already been invalidated.
    /// </summary>
    /// <param name="item">The element to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        EnsureNotInvalidated();

        var node = new Cons<T>(item);
        if (_head is null)
        {
            _head = node;
            _tail = node;
        }
        else
        {
            _tail!._cdr = node;
            _tail = node;
        }
        _count++;
    }

    /// <summary>
    /// Adds the elements of the specified span to the end of the list being built.
    /// Throws <see cref="InvalidOperationException"/> if the builder has already been invalidated.
    /// </summary>
    /// <param name="items">The span of elements to add.</param>
    public void AddRange(ReadOnlySpan<T> items)
    {
        EnsureNotInvalidated();
        for (int i = 0; i < items.Length; i++)
        {
            Add(items[i]);
        }
    }

    /// <summary>
    /// Adds the elements of the specified sequence to the end of the list being built.
    /// Throws <see cref="InvalidOperationException"/> if the builder has already been invalidated.
    /// </summary>
    /// <param name="items">The collection of elements to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        EnsureNotInvalidated();
        ArgumentNullException.ThrowIfNull(items);

        if (items is T[] array)
        {
            AddRange(array.AsSpan());
            return;
        }
        if (items is List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Add(list[i]);
            }
            return;
        }

        foreach (var item in items)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Converts the builder into an immutable <see cref="SchemeList{T}"/>.
    /// Invalidates the builder so that it cannot be modified or inspected again.
    /// </summary>
    /// <returns>The resulting <see cref="SchemeList{T}"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the builder has already been converted.</exception>
    public SchemeList<T> ToSchemeList()
    {
        EnsureNotInvalidated();
        _isInvalidated = true;

        if (_head is null)
        {
            return SchemeList<T>.Empty;
        }

        _tail!._cdr = SchemeList<T>.Empty;
        var result = _head;
        _head = null;
        _tail = null;
        _count = 0;
        return result;
    }

    /// <summary>
    /// Removes all elements from the builder.
    /// Throws <see cref="InvalidOperationException"/> if the builder has already been invalidated.
    /// </summary>
    public void Clear()
    {
        EnsureNotInvalidated();
        _head = null;
        _tail = null;
        _count = 0;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the builder.
    /// Throws <see cref="InvalidOperationException"/> if the builder has already been invalidated.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        EnsureNotInvalidated();

        // The cells do not end in the empty list yet, so the walk reads the
        // fields and stops at the null cdr of the last cell.
        SchemeList<T>? current = _head;
        while (current is not null)
        {
            EnsureNotInvalidated();
            yield return current._car;
            current = current._cdr;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureNotInvalidated()
    {
        if (_isInvalidated)
        {
            throw new InvalidOperationException("The SchemeListBuilder has already been converted to a SchemeList via ToSchemeList() and cannot be used again.");
        }
    }
}
