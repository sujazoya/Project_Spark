using System;
using UnityEngine;

namespace ProjectSpark.Gameplay.Repair
{
    [Serializable]
    public sealed class Fault
    {
        public string Id;

        public FaultType Type;

        public FaultSeverity Severity;

        public string Description;

        public bool Hidden = true;

        public bool Repaired;
    }
}
