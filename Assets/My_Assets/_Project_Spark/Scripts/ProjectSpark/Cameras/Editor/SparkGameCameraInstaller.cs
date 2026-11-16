#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AAAUI.VFX;
using ProjectSpark.Cameras;
using ProjectSpark.Gameplay;
using ProjectSpark.HolographicViewer;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

[InitializeOnLoad]
internal static class SparkGameCameraInstaller
{
    private const string ScenePath="Assets/My_Assets/_Project_Spark/Scenes/MainMenu/MainMenu.unity";
    private const string Request="Logs/SparkGameCamera.request",Report="Logs/SparkGameCamera.txt";
    private const string Running="ProjectSpark.GameCamera.Verification";
    private static readonly string[] Names={"Play Station","Worktop","Detail","Shelf","Showcase"};

    static SparkGameCameraInstaller()
    {
        EditorApplication.update+=Poll;
        EditorApplication.playModeStateChanged+=PlayState;
    }

    [MenuItem("Tools/Project Spark/Camera/Install Shared Cinemachine Rig")]
    private static void Install()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=ScenePath)throw new InvalidOperationException("Open the MainMenu scene to install the shared game camera.");
        var objects=scene.GetRootGameObjects();
        var output=objects.SelectMany(node=>node.GetComponentsInChildren<Camera>(true))
            .Single(camera=>camera.CompareTag("MainCamera")&&camera.enabled&&camera.gameObject.activeInHierarchy);
        var legacy=output.GetComponent<HolographicViewerCamera>();
        var brain=output.GetComponent<CinemachineBrain>();
        if(!brain)brain=Undo.AddComponent<CinemachineBrain>(output.gameObject);
        Undo.RecordObject(brain,"Configure shared Cinemachine output");
        brain.enabled=false;
        brain.ChannelMask=OutputChannels.Channel14;
        brain.UpdateMethod=CinemachineBrain.UpdateMethods.ManualUpdate;
        var rig=output.GetComponent<SparkGameCameraRig>();
        if(!rig)rig=Undo.AddComponent<SparkGameCameraRig>(output.gameObject);

        var root=objects.SingleOrDefault(go=>go.name=="Gameplay Camera Views");
        if(!root)
        {
            root=new GameObject("Gameplay Camera Views");
            Undo.RegisterCreatedObjectUndo(root,"Create shared game camera views");
        }
        var room=objects.SingleOrDefault(go=>go.name=="PS_PRO_GAME_ROOM");
        var serialized=new SerializedObject(rig);
        serialized.FindProperty("legacyViewer").objectReferenceValue=legacy;
        serialized.FindProperty("room").objectReferenceValue=room?room.transform:null;
        var entries=serialized.FindProperty("views");entries.arraySize=Names.Length;
        for(int i=0;i<Names.Length;i++)
        {
            string viewName=Names[i]+" Camera";
            var existing=root.transform.Find(viewName);
            GameObject node;
            if(existing)node=existing.gameObject;
            else
            {
                node=new GameObject(viewName);
                Undo.RegisterCreatedObjectUndo(node,"Create "+viewName);
                node.transform.SetParent(root.transform,false);
                node.transform.SetPositionAndRotation(output.transform.position,output.transform.rotation);
            }
            var view=node.GetComponent<CinemachineCamera>();
            if(!view)view=Undo.AddComponent<CinemachineCamera>(node);
            Undo.RecordObject(view,"Configure "+viewName);
            view.OutputChannel=OutputChannels.Channel14;
            view.Priority=i==0?100:10;
            entries.GetArrayElementAtIndex(i).objectReferenceValue=view;
        }
        serialized.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Failed to save the shared camera in MainMenu.");
        File.AppendAllText(Report,"Saved five Cinemachine cameras in MainMenu using the existing tagged Unity output camera.\n");
    }

    private static void Poll()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||!File.Exists(Request))return;
        if(SceneManager.GetActiveScene().path!=ScenePath)return;
        string mode=File.ReadAllText(Request).Trim();
        File.Delete(Request);
        File.WriteAllText(Report,"Project Spark shared Cinemachine camera\n");
        try
        {
            if(mode=="install")Install();
            SessionState.SetBool(Running,true);
            EditorApplication.EnterPlaymode();
        }
        catch(Exception error){Record("FAIL: "+error);Debug.LogException(error);}
    }

    private static void PlayState(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Running,false))return;
        if(state==PlayModeStateChange.EnteredPlayMode) Verify();
        if(state==PlayModeStateChange.EnteredEditMode)
        {
            SessionState.EraseBool(Running);
            Record("Returned to Edit Mode.");
        }
    }

    private static async void Verify()
    {
        bool runningInBackground=Application.runInBackground;
        try
        {
            Application.runInBackground=true;
            var output=Camera.main;
            Check(output,"Main camera missing.");
            var rig=output.GetComponent<SparkGameCameraRig>();
            var brain=output.GetComponent<CinemachineBrain>();
            Check(rig&&brain,"Shared director or Cinemachine Brain missing.");
            var manager=Object.FindFirstObjectByType<LevelGamePlayManager>();
            await Wait(()=>manager&&manager.ActiveLevel!=null,"initial level");
            var levels=typeof(LevelGamePlayManager).GetField("levels",BindingFlags.Instance|BindingFlags.NonPublic);
            Check(levels!=null&&manager.StartLevel(0),"Unable to open the first standard level for verification.");
            await Wait(()=>rig.OwnsCamera&&brain.ActiveVirtualCamera?.Name=="Play Station Camera","default view");
            var wire=Object.FindFirstObjectByType<SignalWireBuilder>();
            var target=typeof(SignalWireBuilder).GetField("targetCamera",BindingFlags.Instance|BindingFlags.NonPublic);
            Check(wire&&target?.GetValue(wire)==output,"Wire interactions must keep the existing camera.");
            Check(!output.orthographic,"The shared camera should start in perspective.");
            float previousFov=output.fieldOfView;
            foreach(int index in new[]{1,2,3,4,0})
            {
                var before=output.transform.position;
                rig.SelectView(index);
                bool blended=false;
                await Wait(()=>
                {
                    blended|=brain.IsBlending;
                    return brain.ActiveVirtualCamera?.Name==Names[index]+" Camera"&&!brain.IsBlending;
                },"blend to "+Names[index]);
                Check(blended&&Vector3.Distance(before,output.transform.position)>.05f,"Expected a live camera blend and movement.");
                Record($"Blended to {Names[index]} on the original game camera.");
            }
            var unzoomed=output.transform.position;
            rig.ZoomStep(1);
            await Wait(()=>Vector3.Distance(output.transform.position,unzoomed)>.08f,"damped zoom");
            Check(Mathf.Abs(previousFov-output.fieldOfView)<.05f,"Dolly zoom must preserve the selected lens.");
            rig.SelectView(4);
            await Wait(()=>!brain.IsBlending&&brain.ActiveVirtualCamera?.Name=="Showcase Camera","showcase screenshot");
            Capture(output);
            var campaign=(SparkLevelDefinition[])levels.GetValue(manager);
            int dedicated=Array.FindIndex(campaign,level=>level&&level.name=="Level_87");
            if(dedicated>=0)
            {
                var access=typeof(LevelGamePlayManager).GetField("enforceProgressionAccess",BindingFlags.Instance|BindingFlags.NonPublic);
                access?.SetValue(manager,false);
                if(manager.LevelProgression!=null)manager.LevelProgression.enabled=false;
                Check(manager.StartLevel(dedicated),"Could not verify the dedicated camera handoff.");
                await Wait(()=>!rig.OwnsCamera&&brain.ChannelMask==OutputChannels.Channel15,"dedicated camera takes ownership");
                Check(manager.StartLevel(0),"Could not return to the normal camera.");
                await Wait(()=>rig.OwnsCamera&&brain.ChannelMask==OutputChannels.Channel14,"shared camera resumes");
                Record("Legacy level-specific camera handoff and return verified with no prefab changes.");
            }
            Record("PASS: Shared camera, five blends, zoom, and wire camera reference verified.");
        }
        catch(Exception error){Record("FAIL: "+error);}
        finally
        {
            Application.runInBackground=runningInBackground;
            EditorApplication.ExitPlaymode();
        }
    }

    private static void Capture(Camera camera)
    {
        var saved=camera.targetTexture;
        var previous=RenderTexture.active;
        var target=new RenderTexture(1600,900,24);
        var texture=new Texture2D(1600,900,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=target;
            camera.Render();
            RenderTexture.active=target;
            texture.ReadPixels(new Rect(0,0,1600,900),0,0);
            texture.Apply();
            File.WriteAllBytes("Logs/SparkGameCamera_Showcase.png",texture.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active=previous;
            camera.targetTexture=saved;
            Object.Destroy(target);
            Object.Destroy(texture);
        }
    }

    private static async Task Wait(Func<bool> ready,string label)
    {
        var until=DateTime.UtcNow.AddSeconds(25);
        while(!ready())
        {
            Check(EditorApplication.isPlaying&&DateTime.UtcNow<until,"Timed out waiting for "+label);
            await Task.Yield();
        }
    }

    private static void Check(bool pass,string reason)
    {
        if(!pass)throw new InvalidOperationException(reason);
    }

    private static void Record(string text)=>File.AppendAllText(Report,text+"\n");
}
#endif
