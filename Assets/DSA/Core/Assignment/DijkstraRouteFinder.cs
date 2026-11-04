using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    // Assignment Phase 4.

    /// <summary>
    /// Finds minimum-total-cost routes through a Frontier Dispatch world
    /// using Dijkstra's shortest-path algorithm.
    ///
    /// This class is intentionally incomplete in the assignment starter.
    /// It should eventually use the student's prepared world representation
    /// and MinPriorityQueue implementation.
    /// </summary>
    public sealed class DijkstraRouteFinder
    {
        private readonly WorldData _world;

        /// <summary>
        /// Constructor. Creates a route finder operating on the supplied world.
        /// </summary>
        public DijkstraRouteFinder(WorldData world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>
        /// Finds a minimum-total-travel-cost route from the start location
        /// to the goal location.
        ///
        /// A successful RouteResult should contain the route as location IDs,
        /// including both the start and goal locations.
        /// </summary>
        public RouteResult FindRoute(int startLocationId, int goalLocationId)
        {
            // TODO: implement Dijkstra using your prepared world representation
            var distances = new Dictionary<int, int>();
            var previous = new Dictionary<int, int>();
            var queue = new MinPriorityQueue<int>();
            var visited = new HashSet<int>();

            int expandedCount = 0;
            
            // validate the requested locations
            if (_world.FindLocationById(startLocationId) == null ||
                _world.FindLocationById(goalLocationId) == null)
            {
                return new RouteResult(false, Array.Empty<int>(), 0, expandedCount);
            }

            distances[startLocationId] = 0;
            queue.Enqueue(startLocationId, 0);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                
                // Ignore locations that have already been expanded
                if (visited.Contains(currentId))
                {
                    continue;
                }

                visited.Add(currentId);
                expandedCount++;

                if (currentId == goalLocationId)
                {
                    var path = new List<int>();
                    int step = goalLocationId;
                    
                    path.Add(step);

                    while (step != startLocationId)
                    {
                        step = previous[step];
                        path.Add(step);
                    }
                    
                    path.Reverse();
                    
                    return new RouteResult(true, path, distances[goalLocationId], expandedCount);
                }

                foreach (Connection connection in _world.FindOutgoingConnections(currentId))
                {
                    int neighborId = connection.ToLocationId;
                    int newCost = distances[currentId] + connection.TravelCost;

                    if (!distances.TryGetValue(neighborId, out int knownCost) || newCost < knownCost)
                    {
                        distances[neighborId] = newCost;
                        previous[neighborId] = currentId;
                        queue.Enqueue(neighborId, newCost);
                    }
                }
            }
            
            return new RouteResult(false, Array.Empty<int>(), 0, expandedCount);
        }
    }
}
