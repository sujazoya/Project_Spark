using System.Collections.Generic;
using UnityEngine;
namespace ProjectSpark.AAAArchitecture
{
    // Geometry only, no scene objects, colliders, scripts or gameplay lights.
    public static class SparkShowcaseStage
    {
        public static Mesh Create()
        {
            var v=new List<Vector3>();var c=new List<Color32>();var t=new List<int>();
            void Ring(float inner,float outer,float y,Color32 color,int segments=96,int stride=1)
            {
                for(int i=0;i<segments;i+=stride){float a=i*2*Mathf.PI/segments,b=(i+1)*2*Mathf.PI/segments;int k=v.Count;
                    v.Add(new Vector3(Mathf.Cos(a)*inner,y,Mathf.Sin(a)*inner));v.Add(new Vector3(Mathf.Cos(a)*outer,y,Mathf.Sin(a)*outer));
                    v.Add(new Vector3(Mathf.Cos(b)*outer,y,Mathf.Sin(b)*outer));v.Add(new Vector3(Mathf.Cos(b)*inner,y,Mathf.Sin(b)*inner));
                    for(int j=0;j<4;j++)c.Add(color);t.Add(k);t.Add(k+1);t.Add(k+2);t.Add(k);t.Add(k+2);t.Add(k+3);
                }
            }
            Ring(0,1.37f,-1.06f,new Color32(14,25,34,255));
            Ring(1.32f,1.39f,-1.05f,new Color32(44,75,88,255));
            Ring(1.39f,1.43f,-1.04f,new Color32(87,192,220,255));
            Ring(1.48f,1.50f,-1.03f,new Color32(37,102,124,255),96,2);
            Ring(1.51f,1.59f,-1.02f,new Color32(91,130,145,255),96,4);
            Ring(.99f,1.01f,-1.049f,new Color32(35,61,74,255));
            var mesh=new Mesh{name="Spark inspection platform",hideFlags=HideFlags.DontSave};mesh.SetVertices(v);mesh.SetColors(c);mesh.SetTriangles(t,0);mesh.RecalculateBounds();return mesh;
        }
    }
}
