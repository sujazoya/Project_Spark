using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.AAAArchitecture.Editor
{
    [Serializable] public sealed class SparkAAAVisualBinding
    {
        public string key, definitionGuid;
        public GameObject prefab;
        public string lastSignature;
        public List<string> outputHashes = new List<string>();
    }
    public sealed class SparkAAAVisualSettings : ScriptableObject
    {
        public List<SparkAAAVisualBinding> bindings = new List<SparkAAAVisualBinding>();
        public bool autoRefresh;
        public bool fullSizeThumbnails;
        public float padding = 1.22f;
        public Color background = new Color(.035f,.052f,.072f,1);
    }
}
