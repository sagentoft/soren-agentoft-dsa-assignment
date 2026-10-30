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

Describe important changes/retained choices, operations affected, and time/memory trade-offs.

## 3. Algorithms and adaptation

Explain your route finder at a useful high level and how the travel-budget query adapts the same ideas.

## 4. Complexity

| Operation | Your representation/algorithm | Complexity | Short explanation |
| --- | --- | --- | --- |
| Find location by ID | | | |
| Get outgoing connections | | | |
| Priority queue enqueue | | | |
| Priority queue dequeue-min | | | |
| Route finding | | | |

## 5. Correctness and testing

Important tests/edge cases and what they demonstrate.

## 6. Performance investigation

Record reproducible before/after measurements for loading/preparation, repeated runtime work, and relevant allocation behavior. Explain what changed and why.

## 7. Development reflection

Describe one bug, wrong assumption, failed approach, or surprising result and how you investigated it.

## 8. Tools and AI assistance

Briefly describe relevant assistance, if any.

## 9. VG extension

Only if attempting VG: heuristic, traversal constraints, correctness, and Dijkstra/A* comparison.
