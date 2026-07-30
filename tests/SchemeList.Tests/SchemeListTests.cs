using System;
using System.Collections.Generic;
using Xunit;
using SchemeList;
using static SchemeList.SchemeList;

namespace SchemeList.Tests;

public class SchemeListTests
{
    [Fact]
    public void TestPrimitives_Cons_Car_Cdr_IsEmpty()
    {
        var empty = SchemeList<int>.Empty;
        Assert.True(empty.IsEmpty);
        Assert.True(IsEmpty(empty));

        var list1 = Cons(10, empty);
        Assert.False(list1.IsEmpty);
        Assert.Equal(10, list1.Car);
        Assert.Equal(10, list1.Head);
        Assert.Equal(10, Car(list1));
        Assert.Same(empty, list1.Cdr);
        Assert.Same(empty, list1.Tail);
        Assert.Same(empty, Cdr(list1));
    }

    [Fact]
    public void TestEmptyList_ThrowsOnCarAndCdr()
    {
        var empty = Empty<string>();
        Assert.Throws<InvalidOperationException>(() => empty.Car);
        Assert.Throws<InvalidOperationException>(() => empty.Cdr);
        Assert.Throws<InvalidOperationException>(() => Car(empty));
        Assert.Throws<InvalidOperationException>(() => Cdr(empty));
    }

    [Fact]
    public void TestAllEmptyListsAreEqual()
    {
        var empty1 = SchemeList<int>.Empty;
        var empty2 = Nil<int>.Instance;
        var empty3 = new Nil<int>();

        Assert.Equal(empty1, empty2);
        Assert.Equal(empty2, empty3);
        Assert.Equal(empty1, empty3);
        Assert.True(empty1 == empty3);
        Assert.True(empty1.Equals(empty3));
        Assert.True(empty3.Equals((object)empty1));

        // Cons with different instances of empty list
        var cons1 = Cons(42, empty1);
        var cons2 = Cons(42, empty3);
        Assert.Equal(cons1, cons2);
        Assert.True(cons1 == cons2);
    }

    [Fact]
    public void TestFold_LeftToRight()
    {
        SchemeList<int> list = [1, 2, 3, 4];
        
        // Sum using fold
        int sum = list.Fold(0, (acc, x) => acc + x);
        Assert.Equal(10, sum);

        // Concatenation order: ((("" + 1) + 2) + 3) + 4 => "1234"
        string str = list.Fold("", (acc, x) => acc + x);
        Assert.Equal("1234", str);
    }

    [Fact]
    public void TestFoldRight_RightToLeft()
    {
        SchemeList<int> list = [1, 2, 3, 4];

        // Sum using FoldRight
        int sum = list.FoldRight(0, (x, acc) => x + acc);
        Assert.Equal(10, sum);

        // Concatenation order: 1 + (2 + (3 + (4 + ""))) => "1234"
        string str = list.FoldRight("", (x, acc) => x + acc);
        Assert.Equal("1234", str);

        // Expression order check: (1 - (2 - (3 - (4 - 0)))) = 1 - (2 - (3 - 4)) = 1 - (2 - (-1)) = 1 - 3 = -2
        int subRight = list.FoldRight(0, (x, acc) => x - acc);
        Assert.Equal(-2, subRight);

        // Left fold equivalent: (((0 - 1) - 2) - 3) - 4 = -10
        int subLeft = list.Fold(0, (acc, x) => acc - x);
        Assert.Equal(-10, subLeft);
    }

    [Fact]
    public void TestForEach()
    {
        SchemeList<int> list = [10, 20, 30];
        var visited = new List<int>();

        list.ForEach(x => visited.Add(x));
        Assert.Equal(new[] { 10, 20, 30 }, visited);

        var indexTracker = new List<(int val, int idx)>();
        list.ForEach((x, i) => indexTracker.Add((x, i)));
        Assert.Equal(new[] { (10, 0), (20, 1), (30, 2) }, indexTracker);
    }

    [Fact]
    public void TestForeachLoop_Enumerator()
    {
        SchemeList<string> list = ["apple", "banana", "cherry"];
        var collected = new List<string>();

        foreach (var item in list)
        {
            collected.Add(item);
        }

        Assert.Equal(new[] { "apple", "banana", "cherry" }, collected);
    }

    [Fact]
    public void TestPatternMatching_ConsDeconstruct()
    {
        SchemeList<int> list = [1, 2, 3];

        if (list is Cons<int>(var head, var tail))
        {
            Assert.Equal(1, head);
            Assert.Equal(2, tail!.Car);
        }
        else
        {
            Assert.Fail("Pattern match failed for Cons");
        }

        // Switch expression with Cons and Nil
        string description = list switch
        {
            Cons<int>(var h, var t) => $"Head: {h}, Tail Count: {t!.Length}",
            Nil<int> => "Empty",
            _ => "Unknown"
        };

        Assert.Equal("Head: 1, Tail Count: 2", description);
    }

    [Fact]
    public void TestPatternMatching_ThreeTupleDeconstruct()
    {
        SchemeList<int> empty = Empty<int>();
        SchemeList<int> cons = Cons(42, empty);

        empty.DeconstructState(out bool isEmp1, out int car1, out _);
        Assert.True(isEmp1);

        cons.DeconstructState(out bool isEmp2, out int car2, out _);
        Assert.False(isEmp2);
        Assert.Equal(42, car2);
    }

    [Fact]
    public void TestMultiElementDeconstruct()
    {
        SchemeList<int> list = [10, 20, 30, 40, 50];

        // 2 head elements + tail
        var (c1, c2, tail2) = list;
        Assert.Equal(10, c1);
        Assert.Equal(20, c2);
        Assert.Equal(SchemeList.Create([30, 40, 50]), tail2);

        // 3 head elements + tail
        var (x1, x2, x3, tail3) = list;
        Assert.Equal(10, x1);
        Assert.Equal(20, x2);
        Assert.Equal(30, x3);
        Assert.Equal(SchemeList.Create([40, 50]), tail3);

        // Pattern matching in switch expression with multi-element deconstruct
        string result = list switch
        {
            (10, 20, 30, var rest) => $"Matched first 3 elements, rest length = {rest!.Length}",
            _ => "No match"
        };
        Assert.Equal("Matched first 3 elements, rest length = 2", result);
    }

    [Fact]
    public void TestCSharpListPatterns_ExactAndRest()
    {
        SchemeList<int> empty = Empty<int>();
        SchemeList<int> one = [10];
        SchemeList<int> two = [10, 20];
        SchemeList<int> three = [10, 20, 30];

        // Match exact number of elements using C# List Patterns []
        Assert.Equal("Empty", MatchList(empty));
        Assert.Equal("Exactly 1 item: 10", MatchList(one));
        Assert.Equal("Exactly 2 items: 10, 20", MatchList(two));
        Assert.Equal("3 items: 10, 20, rest len = 1", MatchList(three));

        static string MatchList(SchemeList<int> l) => l switch
        {
            [] => "Empty",
            [var a] => $"Exactly 1 item: {a}",
            [var a, var b] => $"Exactly 2 items: {a}, {b}",
            [var a, var b, .. var rest] => $"3 items: {a}, {b}, rest len = {rest.Length}"
        };
    }

    [Fact]
    public void TestDeconstructFailureOnInsufficientElements()
    {
        SchemeList<int> single = [42]; // Only 1 element

        // Attempting to match 3 elements using non-null rest check should FAIL and fall through to default
        string result = single switch
        {
            // Requires 3 elements (c1, c2, c3). Since single has only 1 element, cdr is null, so { } rest fails!
            (var c1, var c2, var c3, { } rest) => "Matched 3 elements",
            // Requires 2 elements (c1, c2). Since single has only 1 element, cdr is null, so { } rest fails!
            (var c1, var c2, { } rest) => "Matched 2 elements",
            // Requires 1 element (c1). Single has 1 element, so cdr is Empty (non-null), { } rest succeeds!
            (var c1, { } rest) => $"Matched 1 element: {c1}",
            _ => "Fall through"
        };

        Assert.Equal("Matched 1 element: 42", result);
    }

    [Fact]
    public void TestCollectionExpressionSyntax()
    {
        SchemeList<int> numbers = [10, 20, 30, 40];
        Assert.Equal(4, numbers.Length);
        Assert.Equal(10, numbers.Car);
        Assert.Equal(20, numbers.Cdr.Car);
        Assert.Equal(30, numbers.Cdr.Cdr.Car);
        Assert.Equal(40, numbers.Cdr.Cdr.Cdr.Car);
        Assert.True(numbers.Cdr.Cdr.Cdr.Cdr.IsEmpty);
    }

    [Fact]
    public void TestUtilityMethods_Reverse_Map_Filter()
    {
        SchemeList<int> original = [1, 2, 3, 4, 5];

        var reversed = original.Reverse();
        Assert.Equal(SchemeList.Create([5, 4, 3, 2, 1]), reversed);

        var mapped = original.Map(x => x * 2);
        Assert.Equal(SchemeList.Create([2, 4, 6, 8, 10]), mapped);

        var filtered = original.Filter(x => x % 2 == 0);
        Assert.Equal(SchemeList.Create([2, 4]), filtered);
    }

    [Fact]
    public void TestToString_SchemeFormat()
    {
        SchemeList<int> empty = Empty<int>();
        Assert.Equal("()", empty.ToString());

        SchemeList<int> list = [1, 2, 3];
        Assert.Equal("(1 2 3)", list.ToString());
    }

    [Fact]
    public void TestStackSafety_LargeList()
    {
        int count = 100_000;
        int[] arr = new int[count];
        for (int i = 0; i < count; i++) arr[i] = i;

        SchemeList<int> largeList = SchemeList.FromEnumerable(arr);

        Assert.Equal(count, largeList.Length);

        // Fold stack safety
        long sum = largeList.Fold(0L, (acc, x) => acc + x);
        long expectedSum = (long)count * (count - 1) / 2;
        Assert.Equal(expectedSum, sum);

        // FoldRight stack safety
        long rightSum = largeList.FoldRight(0L, (x, acc) => x + acc);
        Assert.Equal(expectedSum, rightSum);

        // ForEach stack safety
        long countIterated = 0;
        largeList.ForEach(_ => countIterated++);
        Assert.Equal(count, countIterated);

        // Reverse stack safety
        var rev = largeList.Reverse();
        Assert.Equal(count - 1, rev.Car);
    }

    [Fact]
    public void TestStaticAPI()
    {
        var list = SchemeList.Create([1, 2, 3, 4, 5]);

        Assert.False(SchemeList.IsEmpty(list));
        Assert.Equal(1, SchemeList.Car(list));
        Assert.Equal(1, SchemeList.Head(list));
        Assert.Equal(SchemeList.Create([2, 3, 4, 5]), SchemeList.Cdr(list));
        Assert.Equal(SchemeList.Create([2, 3, 4, 5]), SchemeList.Tail(list));
        Assert.Equal(5, SchemeList.Length(list));
        Assert.Equal(5, SchemeList.Count(list));
        Assert.Equal(3, SchemeList.Item(list, 2));

        Assert.Equal(15, SchemeList.Fold(list, 0, (acc, x) => acc + x));
        Assert.Equal(15, SchemeList.FoldRight(list, 0, (x, acc) => x + acc));

        var visited = new List<int>();
        SchemeList.ForEach(list, x => visited.Add(x));
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, visited);

        Assert.Equal(SchemeList.Create([5, 4, 3, 2, 1]), SchemeList.Reverse(list));
        Assert.Equal(SchemeList.Create([2, 4, 6, 8, 10]), SchemeList.Map(list, x => x * 2));
        Assert.Equal(SchemeList.Create([2, 4]), SchemeList.Filter(list, x => x % 2 == 0));
        Assert.Equal(SchemeList.Create([2, 3]), SchemeList.Slice(list, 1, 2));

        Assert.Throws<ArgumentNullException>(() => SchemeList.Car<int>(null!));
        Assert.Throws<ArgumentNullException>(() => SchemeList.Cdr<int>(null!));
    }
}
