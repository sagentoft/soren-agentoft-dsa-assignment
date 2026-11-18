
using NUnit.Framework;
using FrontierDispatch.Core;

namespace FrontierDispatch.Tests
{
    public class ReachabilityQueryTests
    {
        [Test]
        public void Reachability_InvalidStart_ReturnsEmpty()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(-1, 10);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Reachability_NegativeBudget_ReturnsEmpty()
        {
            var query = new ReachabilityQuery(WorldDatasetFactory.CreateSmall());
            var result = query.FindReachable(0, -1);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Reachability_ExactBudget_IncludesLocation()
        {
            var world = CreateWorld(2, new Connection(0, 1, 5));
            var query = new ReachabilityQuery(world);
            var result = query.FindReachable(0, 5);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(0));
            Assert.That(result[1], Is.EqualTo(5));
        }

        [Test]
        public void Reachability_OverBudget_ExcludesLocation()
        {
            var world = CreateWorld(3, new Connection(0, 1, 3),
                new Connection(1, 2, 4));
            var query = new ReachabilityQuery(world);
            var result = query.FindReachable(0, 5);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(0));
            Assert.That(result[1], Is.EqualTo(3));
            Assert.That(result.ContainsKey(2), Is.False);
        }

        [Test]
        public void Reachability_BetterPath_UpdatesKnownCost()
        {
            var world = CreateWorld(3, new Connection(0, 2, 10),
                new Connection(0, 1, 3), new Connection(1, 2, 4));
            var query = new ReachabilityQuery(world);
            var result = query.FindReachable(0, 10);

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[2], Is.EqualTo(7));
        }

        private static WorldData CreateWorld(int locationCount, params Connection[] connections)
        {
            var locations = new Location[locationCount];

            for (int i = 0; i < locationCount; i++)
            {
                locations[i] = new Location(i, $"Location {i}", new WorldPosition(i, 0), "Outpost");
            }

            return new WorldData(locations, connections);
        }
    }
}