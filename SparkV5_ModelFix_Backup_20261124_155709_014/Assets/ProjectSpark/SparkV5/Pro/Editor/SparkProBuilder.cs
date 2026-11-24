using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using ProjectSpark.SparkV5.Pro;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkProBuilder
    {
        const string Base="Assets/ProjectSpark/SparkV5";
        const string Out=Base+"/Pro/Generated";
        const string Prefab=Out+"/SparkV5Pro.prefab";
        static readonly string[] Names={"Crimson","Copper","Graphite","Ceramic","Visor","Cyan","DarkCopper","Rubber"};
        static readonly Color[] Colors={new Color(.57f,.065f,.083f),new Color(.82f,.43f,.22f),new Color(.065f,.083f,.11f),new Color(.76f,.80f,.79f),new Color(.016f,.035f,.048f),new Color(.15f,.95f,1),new Color(.28f,.11f,.07f),new Color(.025f,.035f,.045f)};
        static readonly float[] Metal={.32f,.82f,.38f,.15f,.45f,.05f,.65f,0};
        static readonly float[] Rough={.28f,.24f,.38f,.28f,.16f,.25f,.38f,.66f};
        static readonly string[] Required={"Body","Head","MouthPivot","StatusCore","Eye_L","Eye_R","Gaze_L","Gaze_R","Brow_L","Brow_R","Shoulder_L","Shoulder_R","Elbow_L","Elbow_R","Wrist_L","Wrist_R","Hip_L","Hip_R","Knee_L","Knee_R","Ankle_L","Ankle_R"};
        static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;int slash=path.LastIndexOf('/');Folder(path.Substring(0,slash));AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));}
        static Material Mat(string name,Shader shader,Color col,float metal,float rough,bool emission)
        {
            string path=Out+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}m.shader=shader;
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",col);if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metal);if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",1-rough);
            if(emission&&m.HasProperty("_EmissionColor")){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",col*1.4f);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;}
            EditorUtility.SetDirty(m);return m;
        }
        static Transform Find(Transform root,string name){foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;return null;}
        [MenuItem("Project Spark/Spark V5 Pro/Build Character Prefab")]
        public static void Build()
        {
            Folder(Out+"/Materials");var lit=Shader.Find("Universal Render Pipeline/Lit");var unlit=Shader.Find("Universal Render Pipeline/Unlit");
            if(!lit||!unlit)throw new InvalidOperationException("URP shaders not found. Open a Unity URP project first.");
            var mats=new Dictionary<string,Material>();for(int i=0;i<Names.Length;i++)mats.Add(Names[i],Mat(Names[i],lit,Colors[i],Metal[i],Rough[i],Names[i]=="Cyan"));
            var face=Mat("DigitalMouth",unlit,new Color(.15f,.95f,1),0,1,false);
            var root=new GameObject("SparkV5Pro");
            try{
                var roots=new Transform[4];var lods=new LOD[4];float[] thresholds={.55f,.27f,.10f,.015f};Bounds bounds=default;
                for(int i=0;i<4;i++){
                    string path=Base+"/Models/SparkV5_LOD"+i+".fbx";var importer=AssetImporter.GetAtPath(path) as ModelImporter;
                    if(!importer)throw new InvalidOperationException("Missing model "+path);
                    importer.animationType=ModelImporterAnimationType.None;importer.optimizeGameObjects=false;importer.preserveHierarchy=true;importer.importAnimation=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
                    var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)throw new InvalidOperationException("Unity could not import "+path+". Use supplied GLB/Blender source and re-export if necessary.");
                    var model=(GameObject)PrefabUtility.InstantiatePrefab(asset);model.name="LOD"+i;model.transform.SetParent(root.transform,false);roots[i]=model.transform;
                    foreach(string n in Required)if(!Find(model.transform,n))throw new InvalidOperationException(path+" missing pivot "+n);
                    var renderers=new List<Renderer>();int meshes=0;Bounds modelBounds=default;
                    foreach(var rr in model.GetComponentsInChildren<MeshRenderer>(true)){
                        var mf=rr.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh||mf.sharedMesh.vertexCount==0)continue;
                        int sep=rr.name.LastIndexOf("__",StringComparison.Ordinal);string key=sep<0?"":rr.name.Substring(sep+2);
                        if(!mats.TryGetValue(key,out var material))throw new InvalidOperationException("Unknown material group "+rr.name);
                        var array=new Material[mf.sharedMesh.subMeshCount];for(int j=0;j<array.Length;j++)array[j]=material;rr.sharedMaterials=array;renderers.Add(rr);
                        if(meshes++==0)modelBounds=rr.bounds;else modelBounds.Encapsulate(rr.bounds);
                    }
                    if(meshes==0||modelBounds.size.y<.1f)throw new InvalidOperationException("No measurable geometry: "+path);
                    if(i==0)bounds=modelBounds;else if(Mathf.Abs(modelBounds.size.y-bounds.size.y)>.04f*Mathf.Max(bounds.size.y,.1f))throw new InvalidOperationException("LOD scale mismatch in "+path);
                    var mouth=Find(model.transform,"MouthPivot");var line=mouth.gameObject.AddComponent<LineRenderer>();line.sharedMaterial=face;line.useWorldSpace=false;line.loop=true;line.positionCount=32;line.widthMultiplier=.009f;line.forceRenderingOff=true;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;renderers.Add(line);
                    lods[i]=new LOD(thresholds[i],renderers.ToArray());
                }
                // Keep original proportions. Correct importer unit differences uniformly, then ground the visible body.
                float scale=1.89f/bounds.size.y;
                Vector3 front=Vector3.ProjectOnPlane(Find(roots[0],"MouthPivot").position-Find(roots[0],"Head").position,Vector3.up).normalized;
                Quaternion orient=front.sqrMagnitude>.5f?Quaternion.FromToRotation(front,Vector3.forward):Quaternion.identity;
                Vector3 shift=orient*(new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale);
                foreach(var t in roots){t.localScale*=scale;t.localRotation=orient*t.localRotation;t.localPosition=orient*(t.localPosition*scale)+shift;}
                var source=root.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=.65f;
                var agent=root.AddComponent<NavMeshAgent>();agent.radius=.32f;agent.height=1.89f;agent.speed=1.2f;agent.angularSpeed=250;agent.stoppingDistance=.3f;agent.enabled=false;
                var controller=root.AddComponent<SparkProCharacter>();controller.Configure(roots,face,source,agent);
                var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
                var mic=root.AddComponent<SparkProVoiceInput>();mic.Configure(controller);
                var projection=new GameObject("TeachingProjectionAnchor");projection.transform.SetParent(root.transform,false);projection.transform.localPosition=new Vector3(.8f,1.25f,.35f);
                var demo=root.AddComponent<SparkProDemo>();demo.Configure(controller);demo.enabled=false;
                var result=PrefabUtility.SaveAsPrefabAsset(root,Prefab);if(!result)throw new InvalidOperationException("Prefab save failed.");
                AssetDatabase.SaveAssets();Selection.activeObject=result;Debug.Log("[Spark Pro] Built "+Prefab+" with four validated imported LODs. Enable NavMeshAgent only on baked NavMesh. Test in Play Mode.");
            }finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        [MenuItem("Project Spark/Spark V5 Pro/Create Character Test Scene")]
        public static void TestScene()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(!AssetDatabase.LoadAssetAtPath<GameObject>(Prefab))Build();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab));model.GetComponent<SparkProDemo>().enabled=true;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="Practice Floor";
            var point=GameObject.CreatePrimitive(PrimitiveType.Sphere);point.name="Teaching Target";point.transform.position=new Vector3(.55f,1.0f,.7f);point.transform.localScale=Vector3.one*.10f;model.GetComponent<SparkProDemo>().target=point.transform;
            var camera=new GameObject("Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.position=new Vector3(2.3f,1.65f,3.6f);camera.transform.LookAt(Vector3.up*.96f);camera.gameObject.AddComponent<AudioListener>();
            var lamp=new GameObject("Key Light").AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=1.8f;lamp.transform.rotation=Quaternion.Euler(38,-35,0);
            var fill=new GameObject("Fill Light").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.65f;fill.transform.rotation=Quaternion.Euler(20,140,0);
            EditorSceneManager.MarkSceneDirty(scene);Debug.Log("[Spark Pro] Test scene created unsaved. Save as a NEW scene. Play and use demo buttons.");
        }
    }
}
