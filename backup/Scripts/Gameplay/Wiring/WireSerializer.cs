// ============================================================================
// WireSerializer.cs
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class WireSerializer : MonoBehaviour
    {
        [SerializeField]
        WireController[] wires;

        public List<WireSaveData> Save()
        {
            List<WireSaveData> save = new();

            foreach (WireController wire in wires)
            {
                WireSaveData data = new();

                data.WireId = wire.name;

                data.Connected = wire.IsConnected;

                data.Powered =
                    wire.State == WireState.Powered;

                if (wire.StartConnector != null)
                    data.StartConnectorId =
                        wire.StartConnector
                            .GetComponent<ConnectorId>()
                            .Id;

                if (wire.EndConnector != null)
                    data.EndConnectorId =
                        wire.EndConnector
                            .GetComponent<ConnectorId>()
                            .Id;

                save.Add(data);
            }

            return save;
        }
    }
}