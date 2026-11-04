# REPORT.md - Individual Assignment

Keep this concise and specific to your implementation.

## 1. Initial analysis

**Initial Baseline Results**

Before modifying the supplied implementation, I built and tested the starter project.

- Build succeeded with no errors
- Tests: 3 passed, 3 skipped, 0 failed.
- Workload: 2,500 locations, 9,800 connections, 5,000 interations.
- Execution time: 448 ms.
- Checksum: 6,267,100

**Baseline Analysis**

*WorldData.cs* contains a Location array and a Connection array. It seems to not have 
any meaningful functionality outside of holding data.

*Location.cs* is a class that consists of an Id, Name, Position, and Type. 
The *Position* property is of type WorldPosition, which is just a struct that consists X,Y coordinates.

*Connection.cs* is a class that stores fromLocationId, toLocationId, and travelCost.
These appear to be literal connections between nodes or *Locations* based on Id, going in one direction.

*WorldQueries.cs* stores a WorldData reference.

It has two methods: *FindLocationById* and *FindOutgoingConnections*.

*FindLocationById* iterates over the entire *WorldData Location* array using linear search.
It creates a local variable for the *Location* and checks its Id against the method argument Id.
If it matches, it returns the location. If the loop does not find the *Location*, it returns null.
This is at worst **O(n)** time complexity. Best case is **O(1)**

*FindOutgoingConnections* creates a new *Connection* List called result.
It then iterates over all *Connections* in the *WorldData Connection* array.
It creates a local variable for each connection and compares against the method argument fromLocationId.
If it matches, the *Connection* is added to the result List.
When the loop is done, the method returns result.
The method has O(n) time complexity in all cases, since it must iterate through every connection.

**Baseline Workload**

The baseline runs 5,000 iterations, calling both FindLocationByID and FindOutgoingConnections once per
iteration, using different location IDs.

Both methods are therefore called 5,000 times.

Since *FindOutgoingConnections* always iterates over the entire Connection array, which contains 9,800 connections,
it performs 49,000,000 connection checks during the baseline.

*FindLocationById* may terminate early when it finds a matching location, but still has O(n) worst-case
time complexity.

I expect both methods to scale poorly as the world grows, particularly *FindOutgoingConnections*, since it must examine 
every connection regardless of how many actually belong to the requested location.

**Next Steps**

I would first investigate replacing the linear searches with hash-based lookups.

*FindLocationById* could use a dictionary with the location ID as the key and the Location as the value.
This could replace lookup time from O(n) to O(1) on average.

For *FindOutgoingConnections*, I would investigate using a dictionary where the key is the *fromLocationId* and the value
is a list of Connection objects. This would allow us to retrieve only the connections originating from a specific location,
rather than iterating over every connection in the world.

Since each *Connection* already contains the origin location, destination, and travel cost, this representation
would also be useful when implementing pathfinding algorithms such as Dijkstra's.

## 2. Data representation

I replaced the linear searches with dictionary-based lookups in WorldData.

For locations, I used a Dictionary<int, Location>, where the key is the location ID and the value is the corresponding
Location object.

For outgoing connections, I used a `Dictionary<int, List<Connection>>`, where the key is the originating location ID and
the value is a list of its outgoing connections. This is effectively an adjacency list.

Both dictionaries are populated once when WorldData is constructed. WorldQUeries then uses these prepared dictionaries
instead of searching through the original arrays.

The main trade-off is additional memory usage and preparation time when constructing the world. In return, both lookup
operations have O(1) average time complexity instead of O(n).

### Initial Performance Comparison

| Measurement | Before | After |
|---|---:|---:|
| Locations | 2,500 | 2,500 |
| Connections | 9,800 | 9,800 |
| Iterations | 5,000 | 5,000 |
| Execution time | 448 ms | 0.526 ms |
| Checksum | 6,267,100 | 6,267,100 |

The optimized implementation was approximately 852 times faster in this initial measurement.

The checksum remained idential, suggesting that the changes preserved the expected behavior.

The improvement is likely due to eliminating repeated linear searches, particularly in FindOutgoingConnections,
which previously examined all 9,800 connetions for every query.

These measurements only cover the query workload, not the time required to construct the dictionaries. Further
measurements will be needed to evaluate preparation time and memory usage.

## 3. Algorithms and adaptation

Explain your route finder at a useful high level and how the travel-budget query adapts the same ideas.

## 4. Complexity

## 4. Complexity

| Operation | Your representation/algorithm | Complexity | Short explanation |
| --- | --- | --- | --- |
| Find location by ID | Dictionary lookup | O(1) average | Uses the location ID as a dictionary key instead of searching the location array. |
| Get outgoing connections | Dictionary of connection lists | O(1) average | Retrieves the list of outgoing connections directly using the originating location ID. |
| Priority queue enqueue | Binary min-heap | O(log n) | Adds the entry and sifts it upwards until the heap order is correct. |
| Priority queue dequeue-min | Binary min-heap | O(log n) | Replaces the root with the last entry and sifts it downwards. |
| Route finding | Not implemented yet | — | To be completed in Phase 4. |

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


## 6. Performance investigation

Record reproducible before/after measurements for loading/preparation, repeated runtime work, and relevant allocation behavior. Explain what changed and why.

## 7. Development reflection

Describe one bug, wrong assumption, failed approach, or surprising result and how you investigated it.

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.
