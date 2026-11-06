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
            var distances = new Dictionary<int, int>();
            var previous = new Dictionary<int, int>();
            var queue = new MinPriorityQueue<int>();
            var visited = new HashSet<int>();

            int expandedCount = 0;

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

                if (!visited.Add(currentId))
                {
                    continue;
                }

                expandedCount++;

                if (currentId == goalLocationId)
                {
                    List<int> path = ReconstructRoute(
                        startLocationId, goalLocationId, previous);

                    return new RouteResult(true, path, distances[goalLocationId], expandedCount);
                }

                ExploreConnections(currentId, distances, previous, queue);
            }

            return new RouteResult(false, Array.Empty<int>(), 0, expandedCount);
        }
        
        private void ExploreConnections(int currentId, Dictionary<int, int> distances,
            Dictionary<int, int> previous, MinPriorityQueue<int> queue)
        {
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
        
        private static List<int> ReconstructRoute(int startLocationId, int goalLocationId,
            Dictionary<int, int> previous)
        {
            var path = new List<int>();
            int currentId = goalLocationId;

            path.Add(currentId);

            while (currentId != startLocationId)
            {
                currentId = previous[currentId];
                path.Add(currentId);
            }

            path.Reverse();

            return path;
        }
    }
}
