using System;
using System.Collections.Generic;
using Xunit;
using SchemeList;

namespace SchemeList.Tests;

public class SchemeListBuilderTests
{
    [Fact]
    public void TestEmptyBuilder_ReturnsEmptyList()
    {
        var builder = new SchemeListBuilder<int>();
        Assert.Empty(builder);
        Assert.True(builder.IsEmpty);

        var list = builder.ToSchemeList();
        Assert.True(list.IsEmpty);
        Assert.Same(SchemeList<int>.Empty, list);
    }

    [Fact]
    public void TestAdd_BuildsFrontToBack()
    {
        var builder = new SchemeListBuilder<int>();
        builder.Add(1);
        Assert.Single(builder);
        Assert.False(builder.IsEmpty);

        builder.Add(2);
        builder.Add(3);
        Assert.Equal(3, builder.Count);

        var list = builder.ToSchemeList();
        Assert.Equal(3, list.Length);
        Assert.Equal(1, list.Car);
        Assert.Equal(2, list.Cdr.Car);
        Assert.Equal(3, list.Cdr.Cdr.Car);
        Assert.True(list.Cdr.Cdr.Cdr.IsEmpty);
        Assert.Equal(SchemeList.Create([1, 2, 3]), list);
    }

    [Fact]
    public void TestAddRange_AppendsAllElements()
    {
        var builder = new SchemeListBuilder<string>();
        builder.Add("first");
        builder.AddRange(new[] { "second", "third", "fourth" });
        builder.Add("fifth");

        Assert.Equal(5, builder.Count);

        var list = builder.ToSchemeList();
        Assert.Equal(SchemeList.Create(["first", "second", "third", "fourth", "fifth"]), list);
    }

    [Fact]
    public void TestConstructorWithEnumerable()
    {
        var items = new[] { 10, 20, 30 };
        var builder = new SchemeListBuilder<int>(items);
        Assert.Equal(3, builder.Count);

        builder.Add(40);
        var list = builder.ToSchemeList();
        Assert.Equal(SchemeList.Create([10, 20, 30, 40]), list);
    }

    [Fact]
    public void TestConstructorWithNullEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SchemeListBuilder<int>(null!));
        var builder = new SchemeListBuilder<int>();
        Assert.Throws<ArgumentNullException>(() => builder.AddRange((IEnumerable<int>)null!));
    }

    [Fact]
    public void TestIteration_BeforeInvalidation()
    {
        var builder = new SchemeListBuilder<int>();
        builder.AddRange([10, 20, 30]);

        var collected = new List<int>();
        foreach (var item in builder)
        {
            collected.Add(item);
        }

        Assert.Equal(new[] { 10, 20, 30 }, collected);
    }

    [Fact]
    public void TestClear_ResetsBuilder()
    {
        var builder = new SchemeListBuilder<int>();
        builder.AddRange([1, 2, 3]);
        Assert.Equal(3, builder.Count);

        builder.Clear();
        Assert.Empty(builder);
        Assert.True(builder.IsEmpty);

        builder.Add(42);
        var list = builder.ToSchemeList();
        Assert.Equal(SchemeList.Create([42]), list);
    }

    [Fact]
    public void TestInvalidation_ThrowsOnAllOperationsAfterToSchemeList()
    {
        var builder = new SchemeListBuilder<int>();
        builder.AddRange([1, 2, 3]);

        var list = builder.ToSchemeList();
        Assert.Equal(SchemeList.Create([1, 2, 3]), list);

        // All subsequent operations must throw InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => builder.Add(4));
        Assert.Throws<InvalidOperationException>(() => builder.AddRange([5, 6]));
        Assert.Throws<InvalidOperationException>(() => builder.ToSchemeList());
        Assert.Throws<InvalidOperationException>(() => _ = builder.Count);
        Assert.Throws<InvalidOperationException>(() => _ = builder.IsEmpty);
        Assert.Throws<InvalidOperationException>(() => builder.Clear());
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var _ in builder) { }
        });
    }

    [Fact]
    public void TestEmptyBuilder_Invalidation()
    {
        var builder = new SchemeListBuilder<int>();
        var list = builder.ToSchemeList();
        Assert.True(list.IsEmpty);

        Assert.Throws<InvalidOperationException>(() => builder.Add(1));
        Assert.Throws<InvalidOperationException>(() => builder.ToSchemeList());
        Assert.Throws<InvalidOperationException>(() => _ = builder.Count);
    }

    [Fact]
    public void TestToBuilder_CopiesListAndMaintainsIsolation()
    {
        SchemeList<int> original = [1, 2, 3];
        var builder = original.ToBuilder();

        Assert.Equal(3, builder.Count);

        // Mutating builder
        builder.Add(4);
        builder.Add(5);
        var newList = builder.ToSchemeList();

        // Original list must remain untouched
        Assert.Equal(3, original.Length);
        Assert.Equal(SchemeList.Create([1, 2, 3]), original);

        // New list must have all elements
        Assert.Equal(5, newList.Length);
        Assert.Equal(SchemeList.Create([1, 2, 3, 4, 5]), newList);
    }

    [Fact]
    public void TestToBuilder_EmptyList()
    {
        var empty = SchemeList<string>.Empty;
        var builder = empty.ToBuilder();
        Assert.Empty(builder);
        Assert.True(builder.IsEmpty);

        builder.Add("hello");
        var list = builder.ToSchemeList();
        Assert.Equal(SchemeList.Create(["hello"]), list);
    }

    [Fact]
    public void TestStaticHelpers_CreateBuilder_And_ToBuilder()
    {
        var builder1 = SchemeList.CreateBuilder<double>();
        builder1.Add(3.14);
        Assert.Equal(SchemeList.Create([3.14]), builder1.ToSchemeList());

        SchemeList<int> list = [100, 200];
        var builder2 = SchemeList.ToBuilder(list);
        builder2.Add(300);
        Assert.Equal(SchemeList.Create([100, 200, 300]), builder2.ToSchemeList());
        Assert.Equal(SchemeList.Create([100, 200]), list);

        Assert.Throws<ArgumentNullException>(() => SchemeList.ToBuilder<int>(null!));
    }

    [Fact]
    public void TestLargeListBuilder_PerformanceAndCorrectness()
    {
        const int count = 100_000;
        var builder = new SchemeListBuilder<int>();

        for (int i = 0; i < count; i++)
        {
            builder.Add(i);
        }

        Assert.Equal(count, builder.Count);

        var list = builder.ToSchemeList();
        Assert.Equal(count, list.Length);
        Assert.Equal(0, list.Car);
        Assert.Equal(1, list.Cdr.Car);

        // Validate last element via fold
        int last = list.Fold(0, (_, x) => x);
        Assert.Equal(count - 1, last);
    }
}
