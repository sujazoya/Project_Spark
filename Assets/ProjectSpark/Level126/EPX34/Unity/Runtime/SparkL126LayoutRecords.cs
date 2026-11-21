using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level126
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL126TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL126ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL126RouteEntry
    {
        public string id;
        public string domain;
        public string sourcePad;
        public string destinationPad;
        public string netId;
        public string layer;
        public bool certifiable;
    }

    [Serializable]
    public sealed class SparkL126ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

}
