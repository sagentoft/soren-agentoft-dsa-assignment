using System;
using System.Collections.Generic;
using NUnit.Framework;
using FrontierDispatch.Core;

namespace FrontierDispatch.Tests
{
    /// <summary>
    /// Small contract examples for later assignment phases.
    ///
    /// These are not an exhaustive assignment test suite.
    /// Students are expected to enable the relevant tests as they reach each
    /// phase and add meaningful tests of their own, including edge cases.
    /// </summary>
    public sealed class AssignmentStarterTests
    {
        // These tests document later assignment contracts without making the
        // untouched baseline red. Remove Ignore when you reach the named phase.

        [Test]
        public void PriorityQueue_EnqueueAndDequeue_ReturnsLowestPriorityFirst()
        {
            var queue = new MinPriorityQueue<string>();
            queue.Enqueue("expensive", 9);
            queue.Enqueue("cheap", 2);
            queue.Enqueue("medium", 5);
            Assert.That(queue.Dequeue(), Is.EqualTo("cheap"));
            Assert.That(queue.Dequeue(), Is.EqualTo("medium"));
            Assert.That(queue.Dequeue(), Is.EqualTo("expensive"));
        }
        
        [Test]
        public void PriorityQueue_Peek_DoesNotRemoveItem()
        {
            var queue = new MinPriorityQueue<string>();

            queue.Enqueue("expensive", 10);
            queue.Enqueue("cheap", 2);

            Assert.That(queue.Count, Is.EqualTo(2));
            Assert.That(queue.Peek(), Is.EqualTo("cheap"));
            Assert.That(queue.Count, Is.EqualTo(2));

            Assert.That(queue.Dequeue(), Is.EqualTo("cheap"));
            Assert.That(queue.Count, Is.EqualTo(1));
        }
        
        [Test]
        public void PriorityQueue_EmptyQueue_ThrowsException()
        {
            var queue = new MinPriorityQueue<string>();

            Assert.Throws<InvalidOperationException>(() => queue.Peek());
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }
        
        [Test]
        public void PriorityQueue_DuplicatePriorities_HandlesAllItems()
        {
            var queue = new MinPriorityQueue<string>();

            queue.Enqueue("first", 5);
            queue.Enqueue("second", 5);
            queue.Enqueue("third", 5);

            var results = new HashSet<string>
            {
                queue.Dequeue(),
                queue.Dequeue(),
                queue.Dequeue()
            };

            Assert.That(results, Is.EquivalentTo(new[]
            {
                "first",
                "second",
                "third"
            }));

            Assert.That(queue.Count, Is.Zero);

            queue.Enqueue("alone", 1);

            Assert.That(queue.Dequeue(), Is.EqualTo("alone"));
            Assert.That(queue.Count, Is.Zero);
        }

        [Test]
        public void Dijkstra_StartEqualsGoal_ReturnsZeroCostRoute()
        {
            var finder = new DijkstraRouteFinder(WorldDatasetFactory.CreateSmall());
            RouteResult result = finder.FindRoute(0, 0);
            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(0));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0 }));
        }
        
        [Test]
        public void Dijkstra_FindsCheapestRoute()
        {
            var locations = new[]
            {
                new Location(0, "A", new WorldPosition(0, 0), "Outpost"),
                new Location(1, "B", new WorldPosition(1, 0), "Outpost"),
                new Location(2, "C", new WorldPosition(2, 0), "Outpost")
            };

            var connections = new[]
            {
                new Connection(0, 2, 10),
                new Connection(0, 1, 3),
                new Connection(1, 2, 4)
            };

            var finder = new DijkstraRouteFinder(
                new WorldData(locations, connections));

            RouteResult result = finder.FindRoute(0, 2);

            Assert.That(result.Found, Is.True);
            Assert.That(result.TotalCost, Is.EqualTo(7));
            Assert.That(result.LocationIds, Is.EqualTo(new[] { 0, 1, 2 }));
        }
        
        [Test]
        public void Dijkstra_UnreachableDestination_ReturnsNotFound()
        {
            var locations = new[]
            {
                new Location(0, "A", new WorldPosition(0, 0), "Outpost"),
                new Location(1, "B", new WorldPosition(1, 0), "Outpost")
            };

            var finder = new DijkstraRouteFinder(
                new WorldData(locations, Array.Empty<Connection>()));

            RouteResult result = finder.FindRoute(0, 1);

            Assert.That(result.Found, Is.False);
            Assert.That(result.LocationIds, Is.Empty);
        }
        
        [Test]
        public void Dijkstra_RespectsConnectionDirection()
        {
            var locations = new[]
            {
                new Location(0, "A", new WorldPosition(0, 0), "Outpost"),
                new Location(1, "B", new WorldPosition(1, 0), "Outpost")
            };

            var connections = new[]
            {
                new Connection(0, 1, 5)
            };

            var finder = new DijkstraRouteFinder(
                new WorldData(locations, connections));

            Assert.That(finder.FindRoute(0, 1).Found, Is.True);
            Assert.That(finder.FindRoute(1, 0).Found, Is.False);
        }

        [Test, Ignore("Phase 5: enable when implementing bounded reachability.")]
        public void Reachability_ZeroBudget_IncludesStartAtZeroCost()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, 0);
            Assert.That(result.ContainsKey(0), Is.True);
            Assert.That(result[0], Is.EqualTo(0));
        }
    }
}
