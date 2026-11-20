#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.IO;using System.Linq;
using UnityEngine;using ProjectSpark.Gameplay;using UnityEditor;
namespace ProjectSpark.EditorTools
{
 // Conservative lattice routing; separate layers never share occupied cells. Source factory
 // plane branches are explicit ideal plane taps, not part of these 66 player trace tasks.
 static class SparkLevel104Routing
 {
  const float Step=.2f;const int Width=801,Height=551,Count=Width*Height;
  sealed class Heap
  {
   readonly List<(int id,float priority)> items=new();public int Count=>items.Count;
   public void Add(int id,float p){items.Add((id,p));int n=items.Count-1;while(n>0){int parent=(n-1)/2;if(items[parent].priority<=p)break;items[n]=items[parent];n=parent;}items[n]=(id,p);}
   public int Take(){int result=items[0].id;var last=items[^1];items.RemoveAt(items.Count-1);if(items.Count==0)return result;int n=0;while(n*2+1<items.Count){int child=n*2+1;if(child+1<items.Count&&items[child+1].priority<items[child].priority)child++;if(last.priority<=items[child].priority)break;items[n]=items[child];n=child;}items[n]=last;return result;}
  }
  static int Cell(Vector3 mm)=>Mathf.Clamp(Mathf.RoundToInt((mm.z+55)/Step),0,Height-1)*Width+Mathf.Clamp(Mathf.RoundToInt((mm.x+80)/Step),0,Width-1);
  static Vector3 Mm(SparkTerminal p){var v=p.transform.localPosition;return new(v.x/.04f,(v.y-1.3f)/.04f,v.z/.04f);}
  static Vector3 Point(int cell,float y)=>new((cell%Width)*Step-80,y,(cell/Width)*Step-55);
  internal static bool Plan(SparkLevel104Process p,string root,Func<string,int,SparkTerminal> escape)
  {
   string Base(string name)=>name.Contains(".FactoryL")?name.Substring(0,name.IndexOf(".FactoryL",StringComparison.Ordinal)):name;
   var reports=new List<string>();var all=p.routes.Take(66).SelectMany((r,i)=>r.segments.Select((s,j)=>(segment:s,route:i,number:j,net:p.design.routes[i].net,fromId:Base(s.from.name),toId:Base(s.to.name)))).ToArray();
   var parents=Enumerable.Range(0,all.Length).ToArray();
   int Find(int n){while(parents[n]!=n){parents[n]=parents[parents[n]];n=parents[n];}return n;}
   for(int i=0;i<all.Length;i++)for(int j=i+1;j<all.Length;j++)
    if(all[i].net==all[j].net&&(all[i].fromId==all[j].fromId||all[i].fromId==all[j].toId||all[i].toId==all[j].fromId||all[i].toId==all[j].toId))parents[Find(i)]=Find(j);
   var groups=Enumerable.Range(0,all.Length).ToDictionary(i=>all[i].segment,i=>Find(i)+1);
   var endpointGroups=new Dictionary<string,int>();foreach(var item in all){endpointGroups[item.fromId]=groups[item.segment];endpointGroups[item.toId]=groups[item.segment];}
   var failures=new Dictionary<SparkLevel104Process.Segment,int>();
   var paths=new Dictionary<SparkLevel104Process.Segment,Vector3[]>();bool success=false;
   for(int attempt=0;attempt<32&&!success;attempt++)
   {
    paths.Clear();var layers=new[]{new string[Count],new string[Count],new string[Count],new string[Count]};var traces=new[]{new int[Count],new int[Count],new int[Count],new int[Count]};
    void Stamp(Vector3 point,string net,int layer,float radius)
    {
     int c=Cell(point),cx=c%Width,cz=c/Width,extent=Mathf.CeilToInt(radius/Step);
     for(int z=-extent;z<=extent;z++)for(int x=-extent;x<=extent;x++)
     {int xx=cx+x,zz=cz+z;if(xx<0||xx>=Width||zz<0||zz>=Height||x*x+z*z>extent*extent)continue;int id=zz*Width+xx;var prev=layers[layer][id];layers[layer][id]=prev==null||prev==net?net:"#OBSTACLE";}
    }
    foreach(var pin in p.design.contacts)for(int l=0;l<4;l++)Stamp(pin.positionMm,pin.net+"#"+endpointGroups.GetValueOrDefault(pin.id,-1),l,.45f);
    foreach(var v in p.design.vias){for(int l=0;l<4;l++)Stamp(v.positionMm,v.net+"#"+endpointGroups[v.id+(l==3?".L4":".L1")],l,.27f);}
    var rng=new System.Random(104+attempt);var order=all.OrderBy(t=>p.routes[t.route].vias.Length>0?0:1).ThenByDescending(t=>failures.GetValueOrDefault(t.segment,0)).ThenBy(t=>attempt==0?Vector3.Distance(Mm(t.segment.from),Mm(t.segment.to)):attempt==1?-Vector3.Distance(Mm(t.segment.from),Mm(t.segment.to)):rng.NextDouble()*100).ToArray();
    success=true;int completed=0;
    foreach(var item in order)
    {
     var s=item.segment;int layer=s.layer-1;var a=Mm(s.from);var b=Mm(s.to);int start=Cell(a),end=Cell(b);
     int[] parent=null;bool found=false;
     bool through=p.routes[item.route].vias.Length>0;
     int[] candidates=through&&item.number==1?new[]{3}:through?new[]{layer,0,1,2}.Where(l=>l<3).Distinct().ToArray():new[]{layer,0,1,2,3}.Distinct().ToArray();
     foreach(int candidate in candidates)
     {
     layer=candidate;parent=new int[Count];float[] distance=new float[Count];var closed=new bool[Count];var seen=new bool[Count];var queue=new Heap();queue.Add(start,0);seen[start]=true;distance[start]=0;
     while(queue.Count>0)
     {
      int here=queue.Take();if(closed[here])continue;closed[here]=true;if(here==end){found=true;break;}
      int x=here%Width,z=here/Width;
      foreach(int delta in new[]{-1,1,-Width,Width})
      {
       int next=here+delta;if(next<0||next>=Count||Math.Abs(next%Width-x)+Math.Abs(next/Width-z)!=1||closed[next])continue;
       int nx=next%Width,nz=next/Width;if(nx<3||nz<3||nx>Width-4||nz>Height-4)continue;
       string occupancy=layers[layer][next];if(next!=end&&next!=start&&occupancy!=null&&occupancy!=item.net+"#"+groups[s])continue;
       int trace=traces[layer][next];if(trace!=0&&trace!=groups[s])continue;
       float g=distance[here]+1;if(seen[next]&&distance[next]<=g)continue;seen[next]=true;distance[next]=g;parent[next]=here;
       float h=Math.Abs(nx-end%Width)+Math.Abs(nz-end/Width);queue.Add(next,g+h*1.02f);
      }
     }
     if(found)break;
     }
     if(!found){failures[s]=failures.GetValueOrDefault(s,0)+1;reports.Add("ATTEMPT "+attempt+" blocked "+p.design.routes[item.route].id+" segment "+item.number+" after "+completed);success=false;break;}
     if(s.layer!=layer+1)
     {
      // Additional escapes are authored, fixed factory blind vias, never player-task credit.
      s.from=layer==0?p.GetComponentsInChildren<SparkTerminal>(true).Single(t=>t.name==item.fromId):escape(item.fromId,layer+1);
      s.to=layer==0?p.GetComponentsInChildren<SparkTerminal>(true).Single(t=>t.name==item.toId):escape(item.toId,layer+1);
      s.layer=layer+1;a=Mm(s.from);b=Mm(s.to);
     }
     var cells=new List<int>();for(int at=end;;at=parent[at]){cells.Add(at);if(at==start)break;}cells.Reverse();
     var vertices=new List<Vector3>{a};int previousDirection=0;
     for(int i=0;i<cells.Count;i++)
     {
      int direction=i+1<cells.Count?cells[i+1]-cells[i]:0;
      if(i==0||direction!=previousDirection||i==cells.Count-1)vertices.Add(Point(cells[i],s.layer==4?-.9f:s.layer==3?-.25f:s.layer==2?.25f:1.02f));previousDirection=direction;
      int center=cells[i],cx=center%Width,cz=center/Width;
      for(int dz=-2;dz<=2;dz++)for(int dx=-2;dx<=2;dx++){int xx=cx+dx,zz=cz+dz;if(xx<0||xx>=Width||zz<0||zz>=Height||dx*dx+dz*dz>4)continue;int cell=zz*Width+xx;int previous=traces[layer][cell];traces[layer][cell]=previous==0||previous==groups[s]?groups[s]:-1;}
     }
     vertices.Add(b);paths.Add(s,vertices.ToArray());completed++;
    }
   }
   if(success)
   {
    float minimum=float.PositiveInfinity;
    for(int i=0;i<all.Length;i++)for(int j=i+1;j<all.Length;j++)
    {
     var first=all[i];var second=all[j];if(first.segment.layer!=second.segment.layer||groups[first.segment]==groups[second.segment])continue;
     var aa=paths[first.segment];var bb=paths[second.segment];
     for(int a=1;a<aa.Length;a++)for(int b=1;b<bb.Length;b++)
     {float distance=SegmentDistance(aa[a-1],aa[a],bb[b-1],bb[b]);minimum=Mathf.Min(minimum,distance);}
    }
    reports.Add("Independent polyline check: minimum unlike-net center distance "+minimum.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+" mm.");
    if(minimum<.349f){success=false;reports.Add("BLOCKED_DRC: unlike-net polyline clearance below 0.15 mm after accounting for 0.20 mm trace width.");}
   }
   if(success)
   {
    foreach(var pair in paths){var l=pair.Key.line;l.positionCount=pair.Value.Length;l.SetPositions(pair.Value.Select(v=>new Vector3(v.x*.04f,1.3f+v.y*.04f,v.z*.04f)).ToArray());}
    reports.Add("PASS: "+paths.Count+" player segments routed on explicit L1/L2/L3/L4; unlike-net reserved envelopes separated; 0.2 mm width / 0.15 mm nominal clearance on 0.2 mm grid.");
   }
   else reports.Add("BLOCKED_DRC: no complete collision-free routing found. Electrical certification disabled.");
   File.WriteAllLines(root+"/Reports/Player_Route_DRC.txt",reports);p.design.drcVerified=success;EditorUtility.SetDirty(p.design);return success;
  }
  static float SegmentDistance(Vector3 av,Vector3 bv,Vector3 cv,Vector3 dv)
  {
   var a=new Vector2(av.x,av.z);var b=new Vector2(bv.x,bv.z);var c=new Vector2(cv.x,cv.z);var d=new Vector2(dv.x,dv.z);
   float Cross(Vector2 x,Vector2 y)=>x.x*y.y-x.y*y.x;
   var u=b-a;var v=d-c;float den=Cross(u,v);
   if(Mathf.Abs(den)>1e-8f){float t=Cross(c-a,v)/den,s=Cross(c-a,u)/den;if(t>=0&&t<=1&&s>=0&&s<=1)return 0;}
   float ToSegment(Vector2 point,Vector2 x,Vector2 y){var delta=y-x;float t=delta.sqrMagnitude>1e-12f?Mathf.Clamp01(Vector2.Dot(point-x,delta)/delta.sqrMagnitude):0;return Vector2.Distance(point,x+delta*t);}
   return Mathf.Min(ToSegment(a,c,d),ToSegment(b,c,d),ToSegment(c,a,b),ToSegment(d,a,b));
  }
 }
}
#endif


