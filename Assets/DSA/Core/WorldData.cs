using System;
using System.Collections.Generic;

namespace FrontierDispatch.Core
{
    public sealed class WorldData
    {
        private readonly Location[] _locations;
        private readonly Connection[] _connections;
        
        // mine
        private readonly Dictionary<int, Location> _locationsById;
        private readonly Dictionary<int, List<Connection>> _outgoingConnections;

        public WorldData(IEnumerable<Location> locations, IEnumerable<Connection> connections)
        {
            if (locations == null) throw new ArgumentNullException(nameof(locations));
            if (connections == null) throw new ArgumentNullException(nameof(connections));
            _locations = new List<Location>(locations).ToArray();
            _connections = new List<Connection>(connections).ToArray();
            
            // mine
            _locationsById = new Dictionary<int, Location>();
            foreach (Location location in _locations)
            {
                _locationsById.Add(location.Id, location);
            }

            _outgoingConnections = new Dictionary<int, List<Connection>>();

            foreach (Connection connection in _connections)
            {
                if (!_outgoingConnections.TryGetValue(connection.FromLocationId,
                        out List<Connection> outgoingConnections))
                {
                    outgoingConnections = new List<Connection>();
                    _outgoingConnections.Add(connection.FromLocationId, outgoingConnections);
                }
                outgoingConnections.Add(connection);
            }
        }

        public Location? FindLocationById(int id)
        {
            return _locationsById.GetValueOrDefault(id);
        }

        public IReadOnlyList<Connection> FindOutgoingConnections(int fromLocationId)
        {
            return _outgoingConnections.TryGetValue(fromLocationId, out List<Connection> connections) ? 
                connections : Array.Empty<Connection>();
        }

        public IReadOnlyList<Location> Locations => _locations;
        public IReadOnlyList<Connection> Connections => _connections;
    }
}
