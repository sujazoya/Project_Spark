using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [CreateAssetMenu(menuName="Project Spark/Level 103/Sensor Matrix Design")]
    public sealed class SparkLevel103Design : ScriptableObject
    {
        public string designRevision="SMX4-EDU-1";
        [TextArea] public string provenance;
        public Part[] parts;
        public Contact[] contacts;
        public Route[] routes;
        public Branch[] factory;
        [Serializable] public sealed class Part
        {
            public string reference,kind,value,model;
            public float xMm,zMm,widthMm,depthMm;
        }
        [Serializable] public sealed class Contact
        {
            public string id,reference,net,role;
            public Vector3 positionMm;
        }
        [Serializable] public sealed class Route
        {
            public string id,from,to,net,via;
            public int channel=-1;
            public float widthMm=.25f,clearanceMm=.20f;
        }
        [Serializable] public sealed class Branch { public string from,to; }
    }

    [Serializable]
    public sealed class SparkLevel103Evidence
    {
        public int objective,topologyRevision,systemRevision,samples;
        public double timestamp,intervalStart;
        public string proof;
    }
}