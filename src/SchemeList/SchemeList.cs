using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace SchemeList;

/// <summary>
/// Represents an immutable, functional Scheme-style linked list.
/// Implements standard Scheme primitives: Cons, Car, Cdr, and Empty (Nil).
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[CollectionBuilder(typeof(SchemeList), nameof(SchemeList.Create))]
public abstract class SchemeList<T> : IEnumerable<T>, IEquatable<SchemeList<T>>
{
    // The fields of a cell are declared here and not in Cons<T>, so that Car,
    // Cdr and IsEmpty are field reads and not virtual calls. The empty list is
    // the only node whose _cdr is null.
    internal readonly T _car;
    internal SchemeList<T>? _cdr;

    private protected SchemeList(T car, SchemeList<T>? cdr)
    {
        _car = car;
        _cdr = cdr;
    }

    /// <summary>
    /// Returns true if the list is empty (Nil); otherwise false.
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _cdr is null;
    }

    /// <summary>
    /// Gets the first element of the cons cell (Car / Head).
    /// Throws <see cref="InvalidOperationException"/> if the list is empty.
    /// </summary>
    public T Car
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_cdr is null) ThrowEmpty(nameof(Car));
            return _car;
        }
    }

    /// <summary>
    /// Gets the rest of the list (Cdr / Tail).
    /// Throws <see cref="InvalidOperationException"/> if the list is empty.
    /// </summary>
    public SchemeList<T> Cdr
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var cdr = _cdr;
            if (cdr is null) ThrowEmpty(nameof(Cdr));
            return cdr;
        }
    }

    [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowEmpty(string operation) =>
        throw new InvalidOperationException($"{operation} is not defined on an empty list ()");

    /// <summary>
    /// Alias for <see cref="Car"/>.
    /// </summary>
    public T Head => Car;

    /// <summary>
    /// Alias for <see cref="Cdr"/>.
    /// </summary>
    public SchemeList<T> Tail => Cdr;

    /// <summary>
    /// Gets the canonical empty list singleton for type <typeparamref name="T"/>.
    /// </summary>
    public static SchemeList<T> Empty => Nil<T>.Instance;

    /// <summary>
    /// Deconstructs the list into boolean state, car, and cdr.
    /// </summary>
    public void DeconstructState(out bool isEmpty, out T car, out SchemeList<T> cdr)
    {
        isEmpty = _cdr is null;
        car = _car;
        cdr = _cdr ?? this;
    }

    /// <summary>
    /// Deconstructs the list into head element and tail list.
    /// E.g. (var head, var tail)
    /// </summary>
    public void Deconstruct(out T car1, out SchemeList<T>? cdr)
    {
        cdr = ExtractHead(out car1);
    }

    /// <summary>
    /// Deconstructs the list into 2 head elements and tail list.
    /// E.g. (var c1, var c2, var cdr)
    /// </summary>
    public void Deconstruct(out T car1, out T car2, out SchemeList<T>? cdr)
    {
        car2 = default!;
        var cur = ExtractHead(out car1);
        cur = cur?.ExtractHead(out car2);
        cdr = cur;
    }

    /// <summary>
    /// Deconstructs the list into 3 head elements and tail list.
    /// E.g. (var c1, var c2, var c3, var cdr)
    /// </summary>
    public void Deconstruct(out T car1, out T car2, out T car3, out SchemeList<T>? cdr)
    {
        car2 = default!;
        car3 = default!;
        var cur = ExtractHead(out car1);
        cur = cur?.ExtractHead(out car2);
        cur = cur?.ExtractHead(out car3);
        cdr = cur;
    }

    /// <summary>
    /// Deconstructs the list into 4 head elements and tail list.
    /// </summary>
    public void Deconstruct(out T car1, out T car2, out T car3, out T car4, out SchemeList<T>? cdr)
    {
        car2 = default!;
        car3 = default!;
        car4 = default!;
        var cur = ExtractHead(out car1);
        cur = cur?.ExtractHead(out car2);
        cur = cur?.ExtractHead(out car3);
        cur = cur?.ExtractHead(out car4);
        cdr = cur;
    }

    /// <summary>
    /// Deconstructs the list into 5 head elements and tail list.
    /// </summary>
    public void Deconstruct(out T car1, out T car2, out T car3, out T car4, out T car5, out SchemeList<T>? cdr)
    {
        car2 = default!;
        car3 = default!;
        car4 = default!;
        car5 = default!;
        var cur = ExtractHead(out car1);
        cur = cur?.ExtractHead(out car2);
        cur = cur?.ExtractHead(out car3);
        cur = cur?.ExtractHead(out car4);
        cur = cur?.ExtractHead(out car5);
        cdr = cur;
    }

    /// <summary>
    /// Deconstructs the list into 6 head elements and tail list.
    /// </summary>
    public void Deconstruct(out T car1, out T car2, out T car3, out T car4, out T car5, out T car6, out SchemeList<T>? cdr)
    {
        car2 = default!;
        car3 = default!;
        car4 = default!;
        car5 = default!;
        car6 = default!;
        var cur = ExtractHead(out car1);
        cur = cur?.ExtractHead(out car2);
        cur = cur?.ExtractHead(out car3);
        cur = cur?.ExtractHead(out car4);
        cur = cur?.ExtractHead(out car5);
        cur = cur?.ExtractHead(out car6);
        cdr = cur;
    }

    /// <summary>
    /// Helper method to safely extract the head element and return the tail.
    /// Returns null if the list is empty to allow C# pattern matching to fail on non-null tail patterns.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SchemeList<T>? ExtractHead(out T item)
    {
        if (IsEmpty)
        {
            item = default!;
            return null;
        }
        item = Car;
        return Cdr;
    }

    /// <summary>
    /// Fast, non-recursive left fold over the list elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TState Fold<TState>(TState initial, Func<TState, T, TState> folder)
    {
        ArgumentNullException.ThrowIfNull(folder);
        var current = this;
        var acc = initial;
        while (!current.IsEmpty)
        {
            acc = folder(acc, current.Car);
            current = current.Cdr;
        }
        return acc;
    }

    /// <summary>
    /// Fast, stack-safe right fold over the list elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TState FoldRight<TState>(TState initial, Func<T, TState, TState> folder)
    {
        ArgumentNullException.ThrowIfNull(folder);
        var current = this;
        var buffer = new List<T>();
        while (!current.IsEmpty)
        {
            buffer.Add(current.Car);
            current = current.Cdr;
        }
        var acc = initial;
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            acc = folder(buffer[i], acc);
        }
        return acc;
    }

    /// <summary>
    /// Executes the specified action on each element of the list in order.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForEach(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var current = this;
        while (!current.IsEmpty)
        {
            action(current.Car);
            current = current.Cdr;
        }
    }

    /// <summary>
    /// Executes the specified action on each element along with its zero-based index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ForEach(Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var current = this;
        int index = 0;
        while (!current.IsEmpty)
        {
            action(current.Car, index++);
            current = current.Cdr;
        }
    }

    /// <summary>
    /// Returns the length of the list.
    /// </summary>
    public int Length
    {
        get
        {
            int count = 0;
            var current = this;
            while (!current.IsEmpty)
            {
                count++;
                current = current.Cdr;
            }
            return count;
        }
    }

    /// <summary>
    /// Alias for <see cref="Length"/> to support C# collection patterns.
    /// </summary>
    public int Count => Length;

    /// <summary>
    /// Indexer to get element at the specified zero-based index.
    /// </summary>
    public T this[int index]
    {
        get
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            var current = this;
            int i = 0;
            while (!current.IsEmpty)
            {
                if (i == index) return current.Car;
                i++;
                current = current.Cdr;
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    /// <summary>
    /// Slice method supporting C# 11+ List Patterns (e.g. `[a, b, .. var rest]`).
    /// </summary>
    public SchemeList<T> Slice(int start, int length)
    {
        if (start < 0 || length < 0) throw new ArgumentOutOfRangeException();
        var current = this;
        int i = 0;
        while (i < start && !current.IsEmpty)
        {
            current = current.Cdr;
            i++;
        }

        var buffer = new List<T>(length);
        while (length > 0 && !current.IsEmpty)
        {
            buffer.Add(current.Car);
            current = current.Cdr;
            length--;
        }

        return SchemeList.FromEnumerable(buffer);
    }

    /// <summary>
    /// Reverses the list in O(N) time.
    /// </summary>
    public SchemeList<T> Reverse()
    {
        SchemeList<T> result = SchemeList<T>.Empty;
        var current = this;
        while (!current.IsEmpty)
        {
            result = SchemeList.Cons(current.Car, result);
            current = current.Cdr;
        }
        return result;
    }

    /// <summary>
    /// Maps each element using the selector function to create a new list.
    /// </summary>
    public SchemeList<U> Map<U>(Func<T, U> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return FoldRight(SchemeList<U>.Empty, (item, acc) => SchemeList.Cons(selector(item), acc));
    }

    /// <summary>
    /// Creates a mutable <see cref="SchemeListBuilder{T}"/> initialized with a copy of this list's elements.
    /// Subsequent mutations to the builder will not affect this immutable list.
    /// </summary>
    public SchemeListBuilder<T> ToBuilder()
    {
        var builder = new SchemeListBuilder<T>();
        var current = this;
        while (!current.IsEmpty)
        {
            builder.Add(current.Car);
            current = current.Cdr;
        }
        return builder;
    }

    /// <summary>
    /// Filters the list based on a predicate.
    /// </summary>
    public SchemeList<T> Filter(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return FoldRight(SchemeList<T>.Empty, (item, acc) => predicate(item) ? SchemeList.Cons(item, acc) : acc);
    }

    /// <summary>
    /// Structural equality comparison for list nodes.
    /// Ensures all empty lists evaluate equal to each other.
    /// </summary>
    public virtual bool Equals(SchemeList<T>? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is null) return false;

        var a = this;
        var b = other;

        while (!a.IsEmpty && !b.IsEmpty)
        {
            if (ReferenceEquals(a, b)) return true;
            if (!EqualityComparer<T>.Default.Equals(a.Car, b.Car)) return false;
            a = a.Cdr;
            b = b.Cdr;
        }

        return a.IsEmpty == b.IsEmpty;
    }

    public override bool Equals(object? obj)
    {
        return obj is SchemeList<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hc = new HashCode();
        var current = this;
        while (!current.IsEmpty)
        {
            hc.Add(current.Car);
            current = current.Cdr;
        }
        return hc.ToHashCode();
    }

    public static bool operator ==(SchemeList<T>? left, SchemeList<T>? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(SchemeList<T>? left, SchemeList<T>? right)
    {
        return !(left == right);
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// High-performance allocation-free struct enumerator for foreach iteration.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private SchemeList<T> _currentList;
        private T _current;
        private bool _started;

        internal Enumerator(SchemeList<T> list)
        {
            _currentList = list;
            _current = default!;
            _started = false;
        }

        public readonly T Current => _current;
        readonly object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
            }
            else if (!_currentList.IsEmpty)
            {
                _currentList = _currentList.Cdr;
            }

            if (_currentList.IsEmpty)
            {
                _current = default!;
                return false;
            }

            _current = _currentList.Car;
            return true;
        }

        public void Reset() => throw new NotSupportedException();
        public readonly void Dispose() { }
    }
}

/// <summary>
/// Represents a non-empty Cons cell in a Scheme list.
/// </summary>
/// <typeparam name="T">The type of the Car element.</typeparam>
public sealed class Cons<T> : SchemeList<T>
{
    // A null cdr would make this cell read as the empty list, so it is refused.
    public Cons(T car, SchemeList<T> cdr)
        : base(car, cdr ?? throw new ArgumentNullException(nameof(cdr)))
    {
    }

    // A cell for SchemeListBuilder, with a null cdr. The cell reads as the
    // empty list until the builder sets its cdr, so it must not be visible
    // outside the builder before then.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Cons(T car)
        : base(car, null)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetCdr(SchemeList<T> next)
    {
        _cdr = next;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('(');
        SchemeList<T> cur = this;
        bool first = true;
        while (!cur.IsEmpty)
        {
            if (!first) sb.Append(' ');
            sb.Append(cur.Car);
            first = false;
            cur = cur.Cdr;
        }
        sb.Append(')');
        return sb.ToString();
    }
}

/// <summary>
/// Represents the empty Scheme list (Nil).
/// All empty lists are equal.
/// </summary>
/// <typeparam name="T">The type of elements.</typeparam>
public sealed class Nil<T> : SchemeList<T>
{
    /// <summary>
    /// The singleton instance of Nil for type <typeparamref name="T"/>.
    /// </summary>
    public static Nil<T> Instance { get; } = new Nil<T>();

    public Nil()
        : base(default!, null)
    {
    }

    public override bool Equals(SchemeList<T>? other)
    {
        return other is not null && other.IsEmpty;
    }

    public override bool Equals(object? obj)
    {
        return obj is SchemeList<T> list && list.IsEmpty;
    }

    public override int GetHashCode() => typeof(Nil<T>).GetHashCode();

    public override string ToString() => "()";
}

/// <summary>
/// Static helper class providing Scheme primitives, creation methods, and CollectionBuilder support.
/// </summary>
public static class SchemeList
{
    /// <summary>
    /// Returns the empty list singleton for type <typeparamref name="T"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeList<T> Empty<T>() => SchemeList<T>.Empty;

    /// <summary>
    /// Constructs a Cons cell with element <paramref name="car"/> and tail <paramref name="cdr"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeList<T> Cons<T>(T car, SchemeList<T> cdr) => new Cons<T>(car, cdr);

    /// <summary>
    /// Scheme primitive: Returns the Car (first element) of the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Car<T>(SchemeList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.Car;
    }

    /// <summary>
    /// Scheme primitive: Returns the Cdr (tail) of the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeList<T> Cdr<T>(SchemeList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.Cdr;
    }

    /// <summary>
    /// Alias for <see cref="Car{T}(SchemeList{T})"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Head<T>(SchemeList<T> list) => Car(list);

    /// <summary>
    /// Alias for <see cref="Cdr{T}(SchemeList{T})"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeList<T> Tail<T>(SchemeList<T> list) => Cdr(list);

    /// <summary>
    /// Scheme primitive: Returns true if the list is empty.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty<T>(SchemeList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.IsEmpty;
    }

    /// <summary>
    /// Returns the length of the list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Length<T>(SchemeList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.Length;
    }

    /// <summary>
    /// Alias for <see cref="Length{T}(SchemeList{T})"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count<T>(SchemeList<T> list) => Length(list);

    /// <summary>
    /// Gets the element at the specified zero-based index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Item<T>(SchemeList<T> list, int index)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list[index];
    }

    /// <summary>
    /// Fast, non-recursive left fold over the list elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TState Fold<T, TState>(SchemeList<T> list, TState initial, Func<TState, T, TState> folder)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.Fold(initial, folder);
    }

    /// <summary>
    /// Fast, stack-safe right fold over the list elements.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TState FoldRight<T, TState>(SchemeList<T> list, TState initial, Func<T, TState, TState> folder)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.FoldRight(initial, folder);
    }

    /// <summary>
    /// Executes the specified action on each element of the list in order.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ForEach<T>(SchemeList<T> list, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(list);
        list.ForEach(action);
    }

    /// <summary>
    /// Executes the specified action on each element along with its zero-based index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ForEach<T>(SchemeList<T> list, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(list);
        list.ForEach(action);
    }

    /// <summary>
    /// Reverses the list in O(N) time.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeList<T> Reverse<T>(SchemeList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.Reverse();
    }

    /// <summary>
    /// Maps each element using the selector function to create a new list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeList<U> Map<T, U>(SchemeList<T> list, Func<T, U> selector)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.Map(selector);
    }

    /// <summary>
    /// Filters the list based on a predicate.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeList<T> Filter<T>(SchemeList<T> list, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.Filter(predicate);
    }

    /// <summary>
    /// Slices the list given a start index and length.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeList<T> Slice<T>(SchemeList<T> list, int start, int length)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.Slice(start, length);
    }

    /// <summary>
    /// Collection builder method supporting C# 12 collection expressions: `SchemeList&lt;int&gt; list = [1, 2, 3];`
    /// </summary>
    public static SchemeList<T> Create<T>(ReadOnlySpan<T> items)
    {
        SchemeList<T> result = SchemeList<T>.Empty;
        for (int i = items.Length - 1; i >= 0; i--)
        {
            result = Cons(items[i], result);
        }
        return result;
    }

    /// <summary>
    /// Creates a Scheme list from an <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static SchemeList<T> FromEnumerable<T>(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items is IList<T> list)
        {
            SchemeList<T> result = SchemeList<T>.Empty;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                result = Cons(list[i], result);
            }
            return result;
        }

        var array = new List<T>(items);
        SchemeList<T> res = SchemeList<T>.Empty;
        for (int i = array.Count - 1; i >= 0; i--)
        {
            res = Cons(array[i], res);
        }
        return res;
    }

    /// <summary>
    /// Creates a new empty <see cref="SchemeListBuilder{T}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeListBuilder<T> CreateBuilder<T>() => new SchemeListBuilder<T>();

    /// <summary>
    /// Creates a mutable <see cref="SchemeListBuilder{T}"/> initialized with a copy of the specified list.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SchemeListBuilder<T> ToBuilder<T>(SchemeList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.ToBuilder();
    }
}
