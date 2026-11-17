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
        public bool IsResolved(out string reason)
        {
            reason="";
            if(designRevision!="SMX4-EDU-1"||parts?.Length!=44||contacts?.Length!=187||routes?.Length!=59)
            {reason="Educational schematic revision or source counts are unresolved.";return false;}
            var pins=new System.Collections.Generic.Dictionary<string,Contact>();
            foreach(var pin in contacts)
                if(pin==null||string.IsNullOrWhiteSpace(pin.id)||pins.ContainsKey(pin.id))
                {reason="Missing or duplicate contact identity.";return false;}
                else pins.Add(pin.id,pin);
            for(int i=0;i<routes.Length;i++)
            {
                var route=routes[i];
                if(route==null||!pins.TryGetValue(route.from,out var a)||!pins.TryGetValue(route.to,out var b)||
                    string.IsNullOrWhiteSpace(route.net)||route.net.Contains("SMX4_NET")||route.net.Contains("UNASSIGNED")||
                    string.IsNullOrWhiteSpace(a.net)||string.IsNullOrWhiteSpace(b.net)||a.net.StartsWith("NC_")||b.net.StartsWith("NC_")||
                    a.net.Contains("UNASSIGNED")||b.net.Contains("UNASSIGNED")||i<58 && a.net!=b.net)
                {reason="Unresolved or incompatible schematic at route "+(i+1);return false;}
            }
            return true;
        }
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
