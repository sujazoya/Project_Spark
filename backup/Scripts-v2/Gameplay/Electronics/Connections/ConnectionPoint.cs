using System.Collections.Generic;

namespace ProjectSpark.Gameplay.Electronics
{
    public sealed class ConnectionPoint
    {
        public string Id { get; }

        public ElectronicComponent Owner { get; }

        public List<ConnectionPoint> Connections { get; }

        public ConnectionPoint(string id, ElectronicComponent owner)
        {
            Id = id;
            Owner = owner;
            Connections = new List<ConnectionPoint>();
        }

        public bool IsConnected => Connections.Count > 0;

        public void Connect(ConnectionPoint other)
        {
            if (other == null)
                return;

            if (Connections.Contains(other))
                return;

            Connections.Add(other);

            if (!other.Connections.Contains(this))
                other.Connections.Add(this);
        }

        public void Disconnect(ConnectionPoint other)
        {
            if (other == null)
                return;

            Connections.Remove(other);
            other.Connections.Remove(this);
        }

        public void DisconnectAll()
        {
            foreach (var connection in Connections.ToArray())
                Disconnect(connection);
        }
    }
}
