using System;
using System.Diagnostics;
using SchemeList;

namespace SchemeList.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=========================================================================");
        Console.WriteLine("  SchemeListBuilder vs Cons+Reverse Microbenchmark");
        Console.WriteLine("=========================================================================");
        Console.WriteLine();

        int[] sizes = [100, 1_000, 10_000, 100_000];

        foreach (var size in sizes)
        {
            RunBenchmarkForSize(size);
        }
    }

    private static void RunBenchmarkForSize(int size)
    {
        int iterations = size switch
        {
            <= 100 => 100_000,
            <= 1_000 => 10_000,
            <= 10_000 => 1_000,
            _ => 100
        };

        // Warm-up
        for (int i = 0; i < 50; i++)
        {
            _ = BuildWithConsAndReverse(size);
            _ = BuildWithBuilder(size);
            _ = BuildWithListBuffer(size);
        }

        // Benchmark Cons + Reverse
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long allocBeforeConsRev = GC.GetAllocatedBytesForCurrentThread();
        long startConsRev = Stopwatch.GetTimestamp();
        for (int i = 0; i < iterations; i++)
        {
            _ = BuildWithConsAndReverse(size);
        }
        long endConsRev = Stopwatch.GetTimestamp();
        long allocAfterConsRev = GC.GetAllocatedBytesForCurrentThread();
        double timeConsRevMs = (double)(endConsRev - startConsRev) / Stopwatch.Frequency * 1000.0 / iterations;
        double allocConsRevBytes = (double)(allocAfterConsRev - allocBeforeConsRev) / iterations;

        // Benchmark SchemeListBuilder
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long allocBeforeBuilder = GC.GetAllocatedBytesForCurrentThread();
        long startBuilder = Stopwatch.GetTimestamp();
        for (int i = 0; i < iterations; i++)
        {
            _ = BuildWithBuilder(size);
        }
        long endBuilder = Stopwatch.GetTimestamp();
        long allocAfterBuilder = GC.GetAllocatedBytesForCurrentThread();
        double timeBuilderMs = (double)(endBuilder - startBuilder) / Stopwatch.Frequency * 1000.0 / iterations;
        double allocBuilderBytes = (double)(allocAfterBuilder - allocBeforeBuilder) / iterations;

        // Benchmark List<T> buffer (pre-allocated List then Cons backwards)
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long allocBeforeBuffer = GC.GetAllocatedBytesForCurrentThread();
        long startBuffer = Stopwatch.GetTimestamp();
        for (int i = 0; i < iterations; i++)
        {
            _ = BuildWithListBuffer(size);
        }
        long endBuffer = Stopwatch.GetTimestamp();
        long allocAfterBuffer = GC.GetAllocatedBytesForCurrentThread();
        double timeBufferMs = (double)(endBuffer - startBuffer) / Stopwatch.Frequency * 1000.0 / iterations;
        double allocBufferBytes = (double)(allocAfterBuffer - allocBeforeBuffer) / iterations;

        double speedupVsConsRev = timeConsRevMs / timeBuilderMs;
        double speedupVsBuffer = timeBufferMs / timeBuilderMs;

        Console.WriteLine($"--- Elements: {size:N0} (Averaged over {iterations:N0} runs) ---");
        Console.WriteLine($"  1. SchemeListBuilder:  {timeBuilderMs * 1000,8:F2} µs | {allocBuilderBytes,10:N0} B");
        Console.WriteLine($"  2. Cons + Reverse:     {timeConsRevMs * 1000,8:F2} µs | {allocConsRevBytes,10:N0} B  ({speedupVsConsRev:F2}x faster with builder)");
        Console.WriteLine($"  3. List<T> Buffer:     {timeBufferMs * 1000,8:F2} µs | {allocBufferBytes,10:N0} B  ({speedupVsBuffer:F2}x faster with builder)");
        Console.WriteLine();
    }

    private static SchemeList<int> BuildWithConsAndReverse(int count)
    {
        SchemeList<int> list = SchemeList<int>.Empty;
        for (int i = 0; i < count; i++)
        {
            list = SchemeList.Cons(i, list);
        }
        return list.Reverse();
    }

    private static SchemeList<int> BuildWithBuilder(int count)
    {
        var builder = new SchemeListBuilder<int>();
        for (int i = 0; i < count; i++)
        {
            builder.Add(i);
        }
        return builder.ToSchemeList();
    }

    private static SchemeList<int> BuildWithListBuffer(int count)
    {
        var buffer = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            buffer.Add(i);
        }
        SchemeList<int> res = SchemeList<int>.Empty;
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            res = SchemeList.Cons(buffer[i], res);
        }
        return res;
    }
}
