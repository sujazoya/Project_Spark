// ============================================================================
// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/WireSaveData.cs
// ============================================================================

using System;

namespace ProjectSpark.Gameplay.Wiring
{
    [Serializable]
    public class WireSaveData
    {
        public string WireId;

        public string StartConnectorId;

        public string EndConnectorId;

        public bool Connected;

        public bool Powered;
    }
}