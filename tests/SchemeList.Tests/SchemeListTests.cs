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
            Assert.Equal(2, tail.Car);
        }
        else
        {
            Assert.Fail("Pattern match failed for Cons");
        }

        // Switch expression with Cons and Nil
        string description = list switch
        {
            Cons<int>(var h, var t) => $"Head: {h}, Tail Count: {t.Length}",
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

        var emptyMatch = empty switch
        {
            (true, _, _) => "Is Empty",
            (false, var car, _) => $"Car: {car}"
        };
        Assert.Equal("Is Empty", emptyMatch);

        var consMatch = cons switch
        {
            (true, _, _) => "Is Empty",
            (false, var car, _) => $"Car: {car}"
        };
        Assert.Equal("Car: 42", consMatch);
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
}
