using System;
using UnityEngine;

namespace ProjectSpark.AAAArchitecture
{
    // Render-only references. Never stores or instantiates the source gameplay prefab.
    public sealed class SparkLivePreviewAsset : ScriptableObject
    {
        [Serializable] public struct Part
        {
            public Mesh mesh;
            public Material material;
            public Matrix4x4 matrix;
            public int submesh;
        }
        public int levelNumber;
        public long indexCount;
        public string definitionGuid;
        public ScriptableObject definition;
        public string sourceSignature;
        public Part[] parts = Array.Empty<Part>();
    }
}
