using System;
using System.Collections.Generic;
using System.Diagnostics;
using FrontierDispatch.Core;

if (args.Length > 0 && args[0] == "--baseline")
{
    RunBaseline();
    return;
}

if (args.Length > 0 && args[0] == "--performance")
{
    RunPerformanceInvestigation();
    return;
}

if (args.Length > 0 && args[0] == "--scaling")
{
    RunScalingInvestigation();
    return;
}

// Small sanity-check application.
// This is separate from the larger performance workload below.
WorldData world = SampleWorldFactory.CreateSmall();
var queries = new WorldQueries(world);

Console.WriteLine("Frontier Dispatch - assignment starter");
Console.WriteLine($"Small demo locations: {world.Locations.Count}");
Console.WriteLine($"Find 734: {queries.FindLocationById(734)?.Name ?? "missing"}");
Console.WriteLine();
Console.WriteLine("Run with --baseline to record the supplied baseline workload.");

static void RunBaseline()
{
    // Build the dataset before timing the repeated runtime queries.
    // This baseline therefore measures query work, not world construction.
    WorldData world = WorldDatasetFactory.CreateMedium();
    var queries = new WorldQueries(world);

    // The checksum provides a stable sanity check that the workload still
    // produces equivalent results after implementation changes.
    int checksum = 0;

    const int iterations = 5000;

    var sw = Stopwatch.StartNew();

    for (int i = 0; i < iterations; ++i)
    {
        // Deterministically visit many different location IDs rather than
        // repeatedly querying the same location.
        int id = (i * 997) % world.Locations.Count;

        Location? location = queries.FindLocationById(id);
        checksum += location?.Id ?? 0;
        checksum += queries.FindOutgoingConnections(id).Count;
    }

    sw.Stop();

    Console.WriteLine("Baseline world-query workload");
    Console.WriteLine($"Locations: {world.Locations.Count}");
    Console.WriteLine($"Connections: {world.Connections.Count}");
    Console.WriteLine($"Iterations: {iterations}");
    Console.WriteLine($"Elapsed: {sw.Elapsed.TotalMilliseconds} ms");
    Console.WriteLine($"Checksum: {checksum}");
    Console.WriteLine("Absolute time varies by machine. Keep this result for before/after comparison.");
}


static void RunPerformanceInvestigation()
{
    const int runs = 20;

    // Pre-warmup
    WorldDatasetFactory.CreateMedium();

    double totalMilliseconds = 0;
    double minMilliseconds = double.MaxValue;
    double maxMilliseconds = 0;
    long totalAllocatedBytes = 0;

    for (int i = 0; i < runs; i++)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();

        var sw = Stopwatch.StartNew();

        WorldData world = WorldDatasetFactory.CreateMedium();

        sw.Stop();

        long after = GC.GetAllocatedBytesForCurrentThread();

        double elapsed = sw.Elapsed.TotalMilliseconds;

        totalMilliseconds += elapsed;
        minMilliseconds = Math.Min(minMilliseconds, elapsed);
        maxMilliseconds = Math.Max(maxMilliseconds, elapsed);
        totalAllocatedBytes += after - before;
    }

    Console.WriteLine("World construction performance");
    Console.WriteLine($"Runs: {runs}");
    Console.WriteLine($"Average: {totalMilliseconds / runs:F3} ms");
    Console.WriteLine($"Minimum: {minMilliseconds:F3} ms");
    Console.WriteLine($"Maximum: {maxMilliseconds:F3} ms");
    Console.WriteLine($"Average allocated: {totalAllocatedBytes / runs / 1024.0:F2} KiB");
    
    
    // Measure repeated queries separately from world construction.
    WorldData queryWorld = WorldDatasetFactory.CreateMedium();
    var queries = new WorldQueries(queryWorld);

    const int iterations = 5000;
    const int queryRuns = 20;

    // Pre-warmup
    for (int i = 0; i < iterations; i++)
    {
        int id = (i * 997) % queryWorld.Locations.Count;
        queries.FindLocationById(id);
        queries.FindOutgoingConnections(id);
    }

    double totalQueryMilliseconds = 0;
    long totalQueryAllocatedBytes = 0;
    int checksum = 0;

    for (int run = 0; run < queryRuns; run++)
    {
        checksum = 0;

        long before = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            int id = (i * 997) % queryWorld.Locations.Count;

            Location? location = queries.FindLocationById(id);
            checksum += location?.Id ?? 0;
            checksum += queries.FindOutgoingConnections(id).Count;
        }

        sw.Stop();
        long after = GC.GetAllocatedBytesForCurrentThread();

        totalQueryMilliseconds += sw.Elapsed.TotalMilliseconds;
        totalQueryAllocatedBytes += after - before;
    }

    Console.WriteLine();
    Console.WriteLine("Repeated query performance");
    Console.WriteLine($"Runs: {queryRuns}");
    Console.WriteLine($"Iterations per run: {iterations}");
    Console.WriteLine($"Average: {totalQueryMilliseconds / queryRuns:F4} ms");
    Console.WriteLine($"Average allocated: {totalQueryAllocatedBytes / queryRuns:F2} bytes");
    Console.WriteLine($"Checksum: {checksum}");
}

static void RunScalingInvestigation()
{
    const int runs = 20;

    var datasets = new (string Name, Func<WorldData> Create)[]
    {
        ("Small", WorldDatasetFactory.CreateSmall),
        ("Medium", WorldDatasetFactory.CreateMedium),
        ("Large", WorldDatasetFactory.CreateLarge)
    };

    foreach (var dataset in datasets)
    {
        // Warm up this dataset.
        dataset.Create();

        double totalMilliseconds = 0;
        long totalAllocatedBytes = 0;

        for (int i = 0; i < runs; i++)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();

            WorldData world = dataset.Create();

            sw.Stop();
            long after = GC.GetAllocatedBytesForCurrentThread();

            totalMilliseconds += sw.Elapsed.TotalMilliseconds;
            totalAllocatedBytes += after - before;
        }

        Console.WriteLine($"{dataset.Name} world");
        Console.WriteLine($"Runs: {runs}");
        Console.WriteLine($"Average construction: {totalMilliseconds / runs:F3} ms");
        Console.WriteLine($"Average allocated: {totalAllocatedBytes / runs / 1024.0:F2} KiB");
        Console.WriteLine();
    }
}


