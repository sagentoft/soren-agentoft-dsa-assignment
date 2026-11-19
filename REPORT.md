# REPORT.md - Individual Assignment

Keep this concise and specific to your implementation.

## 1. Initial analysis

### Initial Baseline Results

Before modifying the supplied implementation, I built and tested the starter project.

- Build succeeded with no errors
- Tests: 3 passed, 3 skipped, 0 failed.
- Workload: 2,500 locations, 9,800 connections, 5,000 interations.
- Execution time: 448 ms.
- Checksum: 6,267,100

### Baseline Analysis

`WorldData.cs` contains a Location array and a Connection array. It seems to not have 
any meaningful functionality outside of holding data.

`Location.cs` is a class that consists of an Id, Name, Position, and Type. 
The *Position* property is of type WorldPosition, which is just a struct that consists X,Y coordinates.

`Connection.cs` is a class that stores fromLocationId, toLocationId, and travelCost.
These appear to be literal connections between nodes or *Locations* based on Id, going in one direction.

`WorldQueries.cs` stores a WorldData reference.

It has two methods: `FindLocationById` and `FindOutgoingConnections`.

`FindLocationById` iterates over the entire `WorldData` `Location` array using linear search.
It creates a local variable for the `Location` and checks its Id against the method argument Id.
If it matches, it returns the location. If the loop does not find the `Location`, it returns null.
This is at worst **O(n)** time complexity. Best case is **O(1)**

`FindOutgoingConnections` creates a new `Connection` List called result.
It then iterates over all `Connections` in the `WorldData` `Connection` array.
It creates a local variable for each connection and compares against the method argument fromLocationId.
If it matches, the `Connection` is added to the result List.
When the loop is done, the method returns result.
The method has **O(n)** time complexity in all cases, since it must iterate through every connection.

### Baseline Workload

The baseline runs 5,000 iterations, calling both `FindLocationByID` and `FindOutgoingConnections` once per
iteration, using different location IDs.

Both methods are therefore called 5,000 times.

Since `FindOutgoingConnections` always iterates over the entire Connection array, which contains 9,800 connections,
it performs 49,000,000 connection checks during the baseline.
 
`FindLocationById` may terminate early when it finds a matching location, but still has **O(n)** worst-case
time complexity.

I expect both methods to scale poorly as the world grows, particularly `FindOutgoingConnections`, since it must examine 
every connection regardless of how many actually belong to the requested location.

### Next Steps

I would first investigate replacing the linear searches with hash-based lookups.

`FindLocationById` could use a dictionary with the location ID as the key and the Location as the value.
This could replace lookup time from **O(n)** to **O(1)** on average.

For `FindOutgoingConnections`, I would investigate using a dictionary where the key is the `fromLocationId` and the value
is a list of Connection objects. This would allow us to retrieve only the connections originating from a specific location,
rather than iterating over every connection in the world.

Since each `Connection` already contains the origin location, destination, and travel cost, this representation
would also be useful when implementing pathfinding algorithms such as Dijkstra's.

## 2. Data representation

I replaced the linear searches with dictionary-based lookups in `WorldData`.

For locations, I used a `Dictionary<int, Location>`, where the key is the location ID and the value is the corresponding
Location object.

For outgoing connections, I used a `Dictionary<int, List<Connection>>`, where the key is the originating location ID and
the value is a list of its outgoing connections. This is effectively an adjacency list.

Both dictionaries are populated once when WorldData is constructed. WorldQueries then uses these prepared dictionaries
instead of searching through the original arrays.

The main trade-off is additional memory usage and preparation time when constructing the world. In return, both lookup
operations have **O(1)** average time complexity instead of **O(n)**.

I retained the original location and connection arrays, since they are still useful for storing and enumerating the 
complete world data. The dictionaries provide additional indexes for efficient queries rather than replacing the original collections entirely.

### Initial Performance Comparison

| Measurement | Before | After |
|---|---:|---:|
| Locations | 2,500 | 2,500 |
| Connections | 9,800 | 9,800 |
| Iterations | 5,000 | 5,000 |
| Execution time | 448 ms | 0.526 ms |
| Checksum | 6,267,100 | 6,267,100 |

The optimized implementation was approximately 852 times faster in this initial measurement.
These initial measurements were useful for identifying the scale of the improvement, but they were not collected using the same repeated 
benchmarking procedure as the later comparison in Section 6. The controlled measurements provide a more reliable estimate of the speedup.

The checksum remained identical, suggesting that the changes preserved the expected behavior.

The improvement is likely due to eliminating repeated linear searches, particularly in `FindOutgoingConnections`,
which previously examined all 9,800 connections for every query.

These measurements only cover the query workload, not the time required to construct the dictionaries. Further
measurements will be needed to evaluate preparation time and memory usage.

## 3. Algorithms and adaptation

### Dijkstra's Algorithm

I implemented Dijkstra's algorithm in `DijkstraRouteFinder.cs` to find the route with the lowest total travel cost between two locations.

The algorithm uses a dictionary to store the lowest known cumulative cost for each location, another dictionary to track the previous location along
 the cheapest known route, and my binary min-heap priority queue to determine which location to explore next.

The starting location is assigned a cost of 0 and added to the priority queue.

The algorithm repeatedly removes the location with the lowest cumulative cost and examines its outgoing connections using the dictionary-based
 representation in `WorldData`.

For each connection, it calculates the cumulative cost of reaching the destination through the current location. If this is cheaper than the previously
 known cost, the distance and previous location are updated, and the destination is added to the priority queue.

Because my priority queue does not support updating priorities directly, a location can be added multiple times when a cheaper route is discovered.
 A `HashSet` tracks which locations have already been explored so that they are not processed again.

When the destination is reached, the algorithm reconstructs the route by following the previous-location dictionary backwards from the destination
to the start, then reversing the resulting list.

If the destination cannot be reached, the algorithm returns a RouteResult with Found = false, an empty route, and TotalCost = 0. The total cost is not 
meaningful when no route exists.

The algorithm relies on nonnegative connection costs, which are enforced by the `Connection` constructor.

### Bounded Reachability

I implemented ReachabiliyQuery by adapting the Dijkstra algorithm from Phase 4.

Instead of finding the cheapest route to one specific location, the algorithm finds all locations that can be reached from a starting location without
exceeding a given travel-cost budget.

It uses a dictionary to track the lowest known cost of each location, a binary min-heap priority queue to explore locations in order of cumulative cost,
and a HashSet to avoid exploring the same location multiple times.

For each outgoing connection, the algorithm calculates the new cumulative cost. If this exceeds the budget, the connection is ignored. Otherwise, the
destination is added to the queue if it has not been discovered before or if a cheaper route has been found. 

Unlike the route finder, this algorithm does not need to track previous locations or reconstruct a path, since it only returns the reachable location
IDs and their minimum travel costs.

The result is a dictionary containing the reachable locations and their cheapest known travel costs.

A budget of zero still includes the starting location and any other locations reachable through connections with a total travel cost of zero.

## 4. Complexity

| Operation | Your representation/algorithm | Complexity | Short explanation |
| --- | --- | --- | --- |
| Find location by ID | Dictionary lookup | O(1) average | Uses the location ID as a dictionary key instead of searching the location array. |
| Get outgoing connections | Dictionary of connection lists | O(1) average lookup, O(n) to iterate | Retrieves the adjacency list in O(1) average time. Iterating its d outgoing connections requires an additional O(n) time. |
| Priority queue enqueue | Binary min-heap | O(log n) | Adds the entry and sifts it upwards until the heap order is correct. |
| Priority queue dequeue-min | Binary min-heap | O(log n) | Replaces the root with the last entry and sifts it downwards. |
| Route finding | Dijkstra with binary min-heap and adjacency lists | O((V + E) log V) | Explores outgoing connections and uses the priority queue to process locations in order of cumulative cost. |
| Bounded Reachability| Dijkstra with min-heap and adjecency lists | O((V + E) log V) | Worst-case is similar to Dijkstra because the algorithm uses the same graph traversal and priority queue. However, the budget can reduce the amount of the graph that needs to be explored. |

## 5. Correctness and testing

After implementing the binary min-heap priority queue, I enabled the provided Phase 3 test and added additional tests.

The tests verify that:

- Items are removed in ascending priority order.
- Peek returns the minimum-priority item without removing it.
- Count updates correctly after inseration and removal.
- Duplicate priorites are handled without losing items.
- A queue containing only one item works correctly.

The heap uses sift-up when inserting entries nas sift-down when removing the minimum entry. Both operations preserve the property that a parent cannot have a higher
priortiy than its children.

### Dijkstra Tests

I enabled the supplied Phase 4 test and added three additional tests.

The tests verify that:

- Starting and ending at the same location produces a route containing only that location, with a total cost of 0.
- The algorithm chooses the route with the lowest cumulative travel cost, even when a more expensive direct connection exists.
- Unreachable destinations return an unsuccessful result.
- Connections are directional, so a connection from A to B does not automatically allow travel from B to A.

I constructed small test worlds with known connection costs so that the expected results could be verified.

After Phase 4, the test results were **11 passed, 1 skipped, 0 failed**. The remaining skipped test belongs to Phase 5.

### Bounded Reachability Tests

I tested the bounded reachability algorithm using the provided `WorldDatasetFactory.CreateSmall()` dataset and small manually constructed graphs.

The tests cover the following cases:

- A budget of zero includes the starting location at cost zero.
- A location with a travel cost exactly equal to the budget is included.
- Locations requiring more than the available budget are excluded.
- When multiple routes lead to the same location, the algorithm returns the lowest travel cost.
- An invalid starting location returns an empty result.
- A negative travel-cost budget returns an empty result.
- A budget of zero includes locations reachable through zero-cost connections, while excluding locations that require a positive travel cost.

For tests requiring specific connection costs, I created a small `CreateWorld()` helper to construct predictable graphs. This allowed me to verify the expected results
 without depending on the generated dataset's layout and connection costs.

After Phase 5, the test results were **18 passed, 0 skipped, 0 failed**.

## 6. Performance investigation

### Before and After Performance Comparison

To compare the original and optimized implementations, I used a separate Git worktree containing the original starter code.

I added the same performance measurements to both versions and ran them in Release mode, using 20 repetitions and the same medium-sized world containing 2,500 locations and 9,800 connections.

| Measurement | Original | Optimized |
|---|---:|---:|
| Average world construction | 0.719 ms | 0.967 ms |
| Average construction allocations | 1,067.45 KiB | 1,703.48 KiB |
| Average repeated query time | 48.1985 ms | 0.3886 ms |
| Average query allocations | 440,040 bytes | 40 bytes |
| Checksum | 6,267,100 | 6,267,100 |

The optimized implementation was approximately 124 times faster when executing the repeated query workload.

However, world construction took approximately 34.5% longer and allocated about 636 KiB more memory.

This is a trade-off caused by preparing the dictionaries and adjacency lists when the world is constructed.

The optimized query workload also allocated substantially less memory, because outgoing connection lists are prepared in advance rather than created for every query.

The remaining 40 bytes of allocation in the optimized workload are consistent with overhead from the benchmark itself, since the allocation measurement also includes creating the Stopwatch.

Both implementations produced the same checksum, suggesting that the expected query behavior was preserved.

For a world that is constructed once but queried many times, the additional preparation cost is worthwhile because it avoids repeated linear searches and allocations.

### World Size Scaling

I also investigated how world construction performance changes as the dataset grows, using the deterministic worlds provided by `WorldDatasetFactory`.

Each dataset was constructed 20 times in Release mode.

| Dataset | Locations | Connections | Average construction time | Average allocated memory |
|---|---:|---:|---:|---:|
| Small | 25 | 80 | 0.011 ms | 15.84 KiB |
| Medium | 2,500 | 9,800 | 1.073 ms | 1,703.48 KiB |
| Large | 40,000 | 159,200 | 33.216 ms | 28,575.66 KiB |

From the medium to the large dataset, the number of locations increased by a factor of 16, while allocated memory increased by approximately 16.8 times.

This suggests that memory allocation grows roughly proportionally to the size of the world, which is consistent with the expected complexity of the representation.

Construction time increased by approximately 31 times over the same interval. This is greater than the increase in world size, although the measurements alone do not establish the cause.

Possible contributing factors include memory allocation overhead, garbage collection, and CPU cache behavior.

These measurements include generating locations and connections as well as constructing the dictionaries, so they do not isolate the preparation cost of the optimized representation.

### Performance Interpretation

**Which change had the largest measured effect, and why?**

Replacing the linear searches with dictionary lookups had the largest effect. The repeated query workload decreased from 48.1985 ms to 0.3886 ms, approximately 124 times faster.

Previously, each query had to search through the locations or connections. The optimized implementation uses dictionaries to find locations and their outgoing connections directly, avoiding repeated scans of the entire collections.

**Did the improvement trade additional memory for time, or vice versa?**

The improvement trades additional memory and construction time for faster queries.

World construction allocations increased from 1,067.45 KiB to 1,703.48 KiB, and average construction time increased from 0.719 ms to 0.967 ms.

However, allocations during the repeated query workload decreased from 440,040 bytes to 40 bytes, since the outgoing connection lists are prepared in advance instead of being recreated for each query.

**Why is an asymptotically better approach not guaranteed to be faster on every tiny input?**

Big-O notation describes how an algorithm scales as the input grows, rather than its exact execution time.

For very small collections, a linear search may be faster because it has little overhead. Dictionary lookups involve hashing and additional data structures, which may cost more than simply checking a few elements.

**What would you investigate next if the world became much larger?**

I would investigate the memory overhead and construction time of the dictionaries and adjacency lists, particularly for larger worlds.

I would also profile route finding to identify whether the priority queue or graph traversal becomes a significant bottleneck, and consider whether a more compact graph representation could reduce memory usage.

## 7. Development reflection

One unexpected result I encountered was when I first benchmarked the optimized dictionary-based lookups.

The original implementation took approximately 448 ms to complete the baseline workload, but after implementing the dictionaries, the benchmark reported an execution time of 0 ms.

I investigated the measurement and realized that the benchmark used `Stopwatch.ElapsedMilliseconds`, which reports elapsed time as a whole number of milliseconds. Since the optimized workload completed in less than one millisecond, the result was reported as zero.

I changed the measurement to use `Stopwatch.Elapsed.TotalMilliseconds`, which provides a fractional millisecond value. This allowed me to measure the optimized workload more accurately, with an initial result of approximately 0.526 ms.

Later, when investigating performance more thoroughly, I also realized that measuring query execution alone did not tell the whole story.

The dictionary-based implementation was significantly faster during repeated queries, but constructing the world required additional time and memory to prepare the dictionaries and adjacency lists.

This highlighted the importance of considering when and how often an operation is performed. In this case, the additional construction cost is worthwhile because the world is constructed once and queried repeatedly.

The experience taught me that performance measurements need both appropriate precision and context. An optimization that improves one operation may introduce costs elsewhere, and those trade-offs need to be measured rather than assumed.

## 8. Tools and AI assistance

I used AI (as well as some programmer friends) to help with forming the appropriate mental models to help me retain information. It was less a case of "Give me the solution to problem x", and more a case of stating an assumption or asking a question about certain concepts, and then learning from that.

I also did use AI to help with creating tests, which I am not super familiar with, as well as formatting the report (I am not very familiar with markdown).

## 9. VG extension

I will not be attempting VG for this assignment. I do not feel I have the time required to do it properly. 

I recently did an A* implementation in a personal project, and while it might have been interesting to compare the two algorithms, now is not the time for that. Perhaps I will do it on my own time, one day.
