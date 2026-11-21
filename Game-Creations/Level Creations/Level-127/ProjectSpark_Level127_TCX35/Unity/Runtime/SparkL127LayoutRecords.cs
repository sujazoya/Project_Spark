using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level127
{
    // Authoring-only records. No invented electrical solver/gameplay API calls.
    public enum SparkL127TaskType { CopperRoute, Via }

    [Serializable]
    public sealed class SparkL127ComponentEntry
    {
        public string referenceId;
        public string assetId;
        public string kind;
        public Vector3 physicalLocationMetres;
        public bool footprintVerified;
        public bool installedAtStart;
    }

    [Serializable]
    public sealed class SparkL127RouteEntry
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
    public sealed class SparkL127ViaEntry
    {
        public string id;
        public string netId;
        public string fromLayer;
        public string toLayer;
        public bool verified;
    }

}
