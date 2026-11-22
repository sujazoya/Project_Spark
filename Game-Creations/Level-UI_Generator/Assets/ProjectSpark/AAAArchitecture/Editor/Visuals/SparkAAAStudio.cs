using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
namespace ProjectSpark.AAAArchitecture.Editor
{
    // Geometry-only preview: no prefab instantiation, MonoBehaviours, colliders or gameplay.
    internal sealed class SparkAAAStudio : IDisposable
    {
        private PreviewRenderUtility preview;
        private Bounds bounds;
        private bool hasBounds;
        internal int MeshCount { get; private set; }
        internal int SkippedSkinned { get; private set; }
        internal int MissingMaterials { get; private set; }
        private readonly SparkAAAVisualSettings settings;
        internal SparkAAAStudio(GameObject source, SparkAAAVisualSettings settings)
        {
            this.settings=settings;
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new InvalidOperationException("A graphics device is required. Do not use -nographics.");
            preview=new PreviewRenderUtility();
            GameObject root=null;
            try {
                root=new GameObject("Spark isolated visual geometry");
                root.hideFlags=HideFlags.HideAndDontSave;
                preview.AddSingleGO(root);
                var exclude=new HashSet<Renderer>();
                foreach(var lod in source.GetComponentsInChildren<LODGroup>(true)){
                    var levels=lod.GetLODs();var first=new HashSet<Renderer>();
                    if(levels.Length>0)foreach(var r in levels[0].renderers)if(r!=null)first.Add(r);
                    for(int i=1;i<levels.Length;i++)foreach(var r in levels[i].renderers)if(r!=null&&!first.Contains(r))exclude.Add(r);
                }
                Copy(source.transform,root.transform,exclude,true);
                if(!hasBounds||MeshCount==0||!Finite(bounds.size.magnitude)||bounds.size.magnitude<1e-7f)throw new InvalidOperationException("No measurable active static mesh geometry. Approve a visual-only prefab or baked model.");
                if(MissingMaterials>0)throw new InvalidOperationException("Missing/unsupported materials on "+MissingMaterials+" mesh renderers. Repair materials before rendering.");
                var cam=preview.camera;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=settings.background;cam.allowHDR=false;cam.allowMSAA=true;cam.orthographic=true;cam.cullingMask=~0;
                preview.ambientColor=new Color(.27f,.31f,.36f);
                var lights=preview.lights;lights[0].intensity=1.4f;lights[0].color=new Color(1f,.93f,.82f);lights[0].transform.rotation=Quaternion.Euler(48,-35,0);
                lights[1].intensity=1.0f;lights[1].color=new Color(.57f,.82f,1f);lights[1].transform.rotation=Quaternion.Euler(25,145,0);
            } catch { Dispose(); if(root!=null)Object.DestroyImmediate(root);throw; }
        }
        private static bool Finite(float f)=>!float.IsNaN(f)&&!float.IsInfinity(f);
        private void Copy(Transform src,Transform parent,HashSet<Renderer> exclude,bool root)
        {
            if(!src.gameObject.activeSelf)return;
            var go=new GameObject(src.name);go.hideFlags=HideFlags.HideAndDontSave;go.transform.SetParent(parent,false);
            go.transform.localPosition=root?Vector3.zero:src.localPosition;go.transform.localRotation=src.localRotation;go.transform.localScale=src.localScale;
            var mf=src.GetComponent<MeshFilter>();var mr=src.GetComponent<MeshRenderer>();
            if(mf!=null&&mr!=null&&mr.enabled&&!mr.forceRenderingOff&&!exclude.Contains(mr)&&mf.sharedMesh!=null&&mf.sharedMesh.vertexCount>0){
                var mats=mr.sharedMaterials;bool bad=mats.Length<mf.sharedMesh.subMeshCount;
                foreach(var m in mats)if(m==null||m.shader==null||!m.shader.isSupported)bad=true;
                if(bad)MissingMaterials++;
                go.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;
                var copy=go.AddComponent<MeshRenderer>();copy.sharedMaterials=mats;
                copy.shadowCastingMode=ShadowCastingMode.On;copy.receiveShadows=true;
                var block=new MaterialPropertyBlock();mr.GetPropertyBlock(block);if(!block.isEmpty)copy.SetPropertyBlock(block);
                var b=copy.bounds;if(!hasBounds){bounds=b;hasBounds=true;}else bounds.Encapsulate(b);MeshCount++;
            }
            if(src.GetComponent<SkinnedMeshRenderer>()!=null)SkippedSkinned++;
            for(int i=0;i<src.childCount;i++)Copy(src.GetChild(i),go.transform,exclude,false);
        }
        private void Frame(int w,int h,Vector3 direction)
        {
            direction=direction.normalized;float radius=bounds.extents.magnitude;var cam=preview.camera;
            var rot=Quaternion.LookRotation(-direction,Mathf.Abs(Vector3.Dot(direction,Vector3.up))>.95f?Vector3.forward:Vector3.up);
            cam.transform.SetPositionAndRotation(bounds.center+direction*(radius*3f),rot);
            var inv=Quaternion.Inverse(rot);float x=0,y=0;
            for(int i=0;i<8;i++){
                var corner=new Vector3((i&1)==0?-bounds.extents.x:bounds.extents.x,(i&2)==0?-bounds.extents.y:bounds.extents.y,(i&4)==0?-bounds.extents.z:bounds.extents.z);
                var p=inv*corner;x=Mathf.Max(x,Mathf.Abs(p.x));y=Mathf.Max(y,Mathf.Abs(p.y));
            }
            cam.aspect=(float)w/h;cam.orthographicSize=Mathf.Max(y,x/cam.aspect)*Mathf.Clamp(settings.padding,1.08f,2f);
            cam.nearClipPlane=Mathf.Max(radius*.05f,1e-7f);cam.farClipPlane=radius*6f;
        }
        internal byte[] Capture(int w,int h,Vector3 direction)
        {
            Frame(w,h,direction);Texture2D result=null;bool begun=false;
            try{
                preview.BeginStaticPreview(new Rect(0,0,w,h));begun=true;
                preview.Render(true,false);
                result=preview.EndStaticPreview();begun=false;
                if(result==null||result.width!=w||result.height!=h)throw new InvalidOperationException("Capture dimensions incorrect.");
                // Reject uniform/empty captures and obvious error-shader pink. Sample across image.
                var pixels=result.GetPixels32();int stride=Mathf.Max(1,pixels.Length/30000),count=0,contrast=0,pink=0;var b=pixels[0];
                for(int i=0;i<pixels.Length;i+=stride){var c=pixels[i];count++;if(Mathf.Abs(c.r-b.r)+Mathf.Abs(c.g-b.g)+Mathf.Abs(c.b-b.b)>25)contrast++;if(c.r>210&&c.b>160&&c.g<75)pink++;}
                if(contrast<count*.008f)throw new InvalidOperationException("Capture appears blank; verify camera, geometry and render pipeline.");
                if(pink>count*.12f)throw new InvalidOperationException("Capture contains extensive magenta pixels; verify materials (or use a neutral source).");
                return result.EncodeToPNG();
            }finally{
                if(begun){try{var abandoned=preview.EndStaticPreview();if(abandoned!=null)Object.DestroyImmediate(abandoned);}catch(Exception e){Debug.LogWarning(e.Message);}}
                if(result!=null)Object.DestroyImmediate(result);
            }
        }
        public void Dispose(){if(preview!=null){preview.Cleanup();preview=null;}}
    }
}
