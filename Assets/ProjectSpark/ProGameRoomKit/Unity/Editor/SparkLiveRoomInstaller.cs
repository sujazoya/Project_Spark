#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectSpark.RoomStudio.EditorTools;

[InitializeOnLoad]
internal static class SparkLiveRoomInstaller
{
    private const string ScenePath="Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
    private const string Request="Logs/SparkRoom.request", Report="Logs/SparkRoom.txt";
    private const string Prefab="Assets/ProjectSpark/RoomStudio/PS_ProGameRoom.prefab";

    static SparkLiveRoomInstaller() => EditorApplication.update += Poll;
    private static void Poll()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||!File.Exists(Request))return;
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=ScenePath)return;
        File.Delete(Request);
        try { Install(); }
        catch(Exception error){File.AppendAllText(Report,error+"\n");Debug.LogException(error);}
    }

    [MenuItem("Tools/Project Spark/Room Studio/Install Live Game Room")]
    public static void Install()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=ScenePath)throw new InvalidOperationException("Open the Project Spark MainMenu scene.");
        var env=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).Single(o=>o.name=="Env"&&o.Find("Table"));
        var legacy=env.Find("Table");
        if(!legacy)throw new InvalidOperationException("Could not find the legacy table anchor.");
        var tableRenderers=legacy.GetComponentsInChildren<Renderer>(true);
        if(tableRenderers.Length==0)throw new InvalidOperationException("The legacy table has no measurable geometry.");
        Bounds bounds=Measure(tableRenderers,Quaternion.identity);
        File.WriteAllText(Report,$"Legacy table {bounds.center}, bounds {bounds.size}\n");

        PSRoomStudioBuilder.BuildRoom();
        var room=scene.GetRootGameObjects().Single(o=>o.name=="PS_PRO_GAME_ROOM");
        room.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
        room.transform.localScale=Vector3.one;
        var art=room.transform.Find("_PS_ROOM_VISUAL_ART");
        if(!art||art.childCount!=39)throw new InvalidOperationException($"Room kit expected 39 instances. Found {art?.childCount}.");
        foreach(Transform module in art) if(module.GetComponentsInChildren<MeshRenderer>(true).Length==0)throw new InvalidOperationException("No mesh in "+module.name);
        var deck=art.Find("PS_Table_SmartGlassDeck");
        if(!deck)throw new InvalidOperationException("Missing smart glass deck.");
        var deckRenderers=deck.GetComponentsInChildren<Renderer>(true);
        var originalDeck=deckRenderers.Length>0?Measure(deckRenderers,Quaternion.identity):new Bounds(new Vector3(0,1.30f,.30f),new Vector3(8.55f,.08f,4.06f));
        if(originalDeck.size.x<3.5f||originalDeck.size.x>13||originalDeck.size.y>.20f||originalDeck.size.z<3)throw new InvalidOperationException($"Unexpected deck units {originalDeck.size}.");

        // The authored operator side faces +Z. The gameplay camera approaches from -Z.
        var rotation=Quaternion.Euler(0,180,0);
        float scale=Mathf.Max(bounds.size.x/9.9f,bounds.size.z/5.4f)*1.32f;
        Vector3 deckOrigin=new(originalDeck.center.x,originalDeck.max.y,originalDeck.center.z);
        Vector3 liveOrigin=new(bounds.center.x,bounds.max.y-.075f,bounds.center.z);
        room.transform.SetPositionAndRotation(liveOrigin-rotation*(deckOrigin*scale),rotation);
        room.transform.localScale=Vector3.one*scale;
        Vector3 footprint=new(9.9f*scale,0,5.4f*scale);
        // Keep the original table physics and tools at their serialized gameplay poses.
        var replacementCollider=room.transform.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable");
        if(replacementCollider&&replacementCollider.TryGetComponent<BoxCollider>(out var tableCollider))
        {
            replacementCollider.localPosition=new Vector3(originalDeck.center.x,originalDeck.max.y-.12f,originalDeck.center.z);
            tableCollider.size=new Vector3(9.8f,.22f,5.3f);
            bool legacyHasSupport=legacy.GetComponentsInChildren<Collider>().Any(c=>c.enabled&&c.gameObject.activeInHierarchy);
            replacementCollider.gameObject.SetActive(!legacyHasSupport);
        }
        foreach(var renderer in tableRenderers)renderer.enabled=false;
        foreach(string prop in new[]{"Stool","plant","Lamp"})
        {
            var old=env.Find(prop);
            if(old)foreach(var r in old.GetComponentsInChildren<Renderer>(true))r.enabled=false;
        }
        var lights=room.transform.Find("_PS_ROOM_LIGHTS");
        foreach(var light in lights.GetComponentsInChildren<Light>(true))
        {
            if(light.type==LightType.Directional){light.intensity=.35f;continue;}
            light.range*=scale;
            light.intensity*=.70f;
        }
        PSRoomMaterialInstaller.EnsureFolder("Assets/ProjectSpark/RoomStudio");
        var previous=room.transform.Find("Play station reflections");
        if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        AddPanorama(room.transform);
        PSRoomStudioBuilder.SaveRoomPrefab();
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Could not save the upgraded MainMenu.");
        File.AppendAllText(Report,$"Built {art.childCount} room instances. Level play station {footprint.x:F2} × {footprint.z:F2} Unity units with legacy wiring and camera anchors preserved.\nSaved {Prefab} and MainMenu.\n");
    }

    private static void AddPanorama(Transform room)
    {
        var previous=room.Find("Panorama exterior");
        if(previous)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var exterior=new GameObject("Panorama exterior").transform;
        exterior.SetParent(room,false);
        var shader=Shader.Find("Universal Render Pipeline/Unlit");
        if(!shader)return;
        Material Look(string name,Color color)
        {
            string path=$"Assets/ProjectSpark/RoomStudio/{name}.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
            material.SetColor("_BaseColor",color);
            return material;
        }
        var sky=Look("Panorama dusk",new Color(.14f,.28f,.40f));
        var far=Look("Panorama skyline",new Color(.10f,.20f,.28f));
        var near=Look("Panorama spires",new Color(.058f,.12f,.17f));
        var glow=Look("Panorama guidance light",new Color(.38f,.81f,1f));
        void Block(string label,Vector3 position,Vector3 size,Material surface)
        {
            var part=GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name=label;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(exterior,false);
            part.transform.localPosition=position;part.transform.localScale=size;
            part.GetComponent<Renderer>().sharedMaterial=surface;
        }
        Block("Atmosphere",new Vector3(0,4,-14.7f),new Vector3(47,22,.12f),sky);
        for(int i=0;i<23;i++)
        {
            float x=i*1.30f-14.3f;
            float height=1.2f+Mathf.Abs(Mathf.Sin(i*2.37f))*3.1f;
            Block("Distant spire",new Vector3(x,height*.5f-.26f,-12.7f),
                new Vector3(.65f+Mathf.Abs(Mathf.Cos(i))*.38f,height,.6f),far);
        }
        for(int i=0;i<11;i++)
        {
            float x=i*2.05f-10.25f;
            float height=1.1f+Mathf.Abs(Mathf.Sin(i*1.61f+.4f))*3f;
            Block("Observation tower",new Vector3(x,height*.5f-.68f,-10.9f),new Vector3(.80f,height,.74f),near);
            Block("Tower beacon",new Vector3(x,height-.64f,-10.85f),new Vector3(.04f,.08f,.04f),glow);
        }
    }
    private static Bounds Measure(Renderer[] renderers,Quaternion rotation)
    {
        var result=new Bounds(rotation*renderers[0].bounds.center,Vector3.zero);
        foreach(var item in renderers)
        {
            var b=item.bounds;
            for(int mask=0;mask<8;mask++)
                result.Encapsulate(rotation*(b.center+Vector3.Scale(b.extents,new Vector3(
                    (mask&1)==0?-1:1,(mask&2)==0?-1:1,(mask&4)==0?-1:1))));
        }
        return result;
    }

}
#endif



