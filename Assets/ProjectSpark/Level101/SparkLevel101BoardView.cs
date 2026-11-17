using Unity.Cinemachine;
using ProjectSpark.HolographicViewer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjectSpark.Gameplay
{
    [DefaultExecutionOrder(9000)]
    public sealed class SparkLevel101BoardView : MonoBehaviour
    {
        private Transform arena;
        private float frameScale=1f;
        private Camera camera;
        private Transform room;
        private readonly System.Collections.Generic.Dictionary<Renderer,bool> covered=new();
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool originalOrthographic;
        private float originalSize,originalFov,originalNear,originalFar;
        private CinemachineBrain brain;
        private bool ownsBrain,brainWasEnabled,savedIgnoreTimeScale,legacyWasEnabled;
        private OutputChannels savedChannel;
        private CinemachineBrain.UpdateMethods savedUpdate;
        private CinemachineBrain.LensModeOverrideSettings savedLensMode;
        private CinemachineBlendDefinition savedBlend;
        private HolographicViewerCamera legacy;
        private readonly CinemachineCamera[] shots=new CinemachineCamera[4];
        private readonly View[] presets={
            new(0,65,2.15f,Vector2.zero),new(0,78,1.3f,new Vector2(0,-.4f)),
            new(0,12,.82f,new Vector2(0,.26f)),new(-28,45,2.2f,new Vector2(.05f,.08f))
        };
        private readonly string[] shotNames={"Board","Pad","Inspection","Showcase"};
        private int shot;
        private Vector3? socketAim;
        private float socketZoom;
        private View desired,current;
        private enum Drag {None,Orbit,Pan}
        private Drag drag;
        private Vector2 lastPointer;
        public CinemachineBrain Brain=>brain;

        private struct View
        {
            public float Yaw,Pitch,Zoom;
            public Vector2 Pan;
            public View(float yaw,float pitch,float zoom,Vector2 pan)
            {Yaw=yaw;Pitch=pitch;Zoom=zoom;Pan=pan;}
        }

        public void Initialize(Transform anchor)
        {
            arena=anchor;
            room=GameObject.Find("PS_PRO_GAME_ROOM")?.transform;
            FitToTable();
            if(room)
                foreach(string name in new[]{"PS_Studio_CeilingStructure","PS_Studio_CeilingHaloMain","PS_Studio_CeilingLinearLighting","PS_Table_GlassStageProjectionVolume"})
                {
                    var obstruction=room.Find("_PS_ROOM_VISUAL_ART/"+name);
                    if(!obstruction)continue;
                    foreach(var renderer in obstruction.GetComponentsInChildren<Renderer>(true))
                    {covered.Add(renderer,renderer.forceRenderingOff);renderer.forceRenderingOff=true;}
                }
            camera=Camera.main;
            if(camera==null)return;
            originalPosition=camera.transform.position;
            originalRotation=camera.transform.rotation;
            originalOrthographic=camera.orthographic;
            originalSize=camera.orthographicSize;
            originalFov=camera.fieldOfView;
            originalNear=camera.nearClipPlane;
            originalFar=camera.farClipPlane;
            legacy=camera.GetComponent<HolographicViewerCamera>();
            if(legacy){legacyWasEnabled=legacy.enabled;legacy.enabled=false;}
            brain=camera.GetComponent<CinemachineBrain>();
            ownsBrain=!brain;
            if(ownsBrain)brain=camera.gameObject.AddComponent<CinemachineBrain>();
            brainWasEnabled=brain.enabled;
            savedUpdate=brain.UpdateMethod;
            savedBlend=brain.DefaultBlend;
            savedChannel=brain.ChannelMask;
            savedLensMode=brain.LensModeOverride;
            savedIgnoreTimeScale=brain.IgnoreTimeScale;
            brain.UpdateMethod=CinemachineBrain.UpdateMethods.ManualUpdate;
            brain.IgnoreTimeScale=true;
            brain.ChannelMask=OutputChannels.Channel12;
            brain.DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Custom,.48f)
                {CustomCurve=AnimationCurve.EaseInOut(0,0,1,1)};
            brain.LensModeOverride=new CinemachineBrain.LensModeOverrideSettings
                {Enabled=true,DefaultMode=LensSettings.OverrideModes.Orthographic};
            for(int i=0;i<shots.Length;i++)
            {
                var node=new GameObject(shotNames[i]+" Camera");
                node.transform.SetParent(transform,false);
                var vcam=node.AddComponent<CinemachineCamera>();
                vcam.OutputChannel=OutputChannels.Channel12;
                vcam.Priority=i==0?100:10;
                shots[i]=vcam;
                ApplyPose(vcam,presets[i]);
            }
            desired=current=presets[0];
            brain.enabled=true;
            brain.ManualUpdate(Time.frameCount,Mathf.Min(Time.unscaledDeltaTime,.045f));
        }

        private void FitToTable()
        {
            if(!room)return;
            var deck=room.Find("_PS_ROOM_WALKABLE_COLLIDERS/CentralTable");
            var stage=arena.Find("L101_ART/L94_AssemblyBenchFrame");
            if(!deck||!stage||!deck.TryGetComponent<BoxCollider>(out var table))return;
            var meshes=stage.GetComponentsInChildren<Renderer>();
            if(meshes.Length==0)return;
            Bounds bounds=meshes[0].bounds;
            foreach(var mesh in meshes)bounds.Encapsulate(mesh.bounds);
            Vector3 opening=Vector3.Scale(table.size,deck.lossyScale);
            float multiplier=Mathf.Min(opening.x*.88f/bounds.size.x,opening.z*.88f/bounds.size.z);
            var floor=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            Vector3 localFloor=arena.InverseTransformPoint(floor);
            arena.localScale*=multiplier;
            frameScale=bounds.size.x*multiplier/5.7f;
            Vector3 top=deck.TransformPoint(table.center+Vector3.up*table.size.y*.5f);
            arena.position+=top-arena.TransformPoint(localFloor);
        }

        public static Vector3 KeepCameraInsideRoom(Transform room,Vector3 position)
        {
            if(!room)return position;
            Vector3 local=room.InverseTransformPoint(position);
            local.x=Mathf.Clamp(local.x,-8.32f,8.32f);
            local.y=Mathf.Clamp(local.y,.75f,5.6f);
            local.z=Mathf.Clamp(local.z,-6.40f,6.40f);
            return room.TransformPoint(local);
        }

        public void UseOverview()=>Select(0);
        public void UseSocketView(Vector3 a, Vector3 b)
        {
            socketAim = (a + b) * .5f;
            socketZoom = Mathf.Clamp(Vector3.Distance(a,b) * .49f, .52f, 4.5f);
            Select(1);
        }

        public void UseSocketView()
        {
            socketAim = null;
            Select(1);
        }
        public void UseRotorView()=>Select(2);
        public void UseShowcaseView()=>Select(3);

        private void Select(int index)
        {
            if(!brain||brain.ChannelMask!=OutputChannels.Channel12||AAAUI.VFX.SignalWireBuilder.AnyWireDrawing)return;
            current=desired=presets[index];
            if (index==1 && socketAim.HasValue) current.Zoom=desired.Zoom=socketZoom/frameScale;
            drag=Drag.None;
            shots[shot].Priority=10;
            shot=index;
            ApplyPose(shots[shot],current);
            shots[shot].Priority=100;
            shots[shot].Prioritize();
        }

        private void Update()
        {
            if(!brain)return;
            var mouse=Mouse.current;
            if(!Application.isFocused||mouse==null||AAAUI.VFX.SignalWireBuilder.AnyWireDrawing)
            {
                drag=Drag.None;
                desired=current;
                return;
            }
            bool overUI=EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            Vector2 pointer=mouse.position.ReadValue();
            if(drag==Drag.Orbit&&!mouse.rightButton.isPressed||drag==Drag.Pan&&!mouse.middleButton.isPressed)
                drag=Drag.None;
            if(drag==Drag.None&&!overUI&&(mouse.rightButton.wasPressedThisFrame||mouse.middleButton.wasPressedThisFrame))
            {
                drag=mouse.rightButton.wasPressedThisFrame?Drag.Orbit:Drag.Pan;
                lastPointer=pointer;
            }
            Vector2 delta=pointer-lastPointer;
            lastPointer=pointer;
            if(drag==Drag.Orbit)
            {
                desired.Yaw=Mathf.Clamp(desired.Yaw+delta.x*.19f,-150,150);
                desired.Pitch=Mathf.Clamp(desired.Pitch-delta.y*.16f,12,70);
            }
            else if(drag==Drag.Pan)
            {
                Quaternion heading=Quaternion.Euler(0,current.Yaw,0);
                var direction=heading*new Vector3(-delta.x,0,-delta.y);
                float speed=2f*current.Zoom/Mathf.Max(1,camera.pixelHeight);
                desired.Pan.x=Mathf.Clamp(desired.Pan.x+direction.x*speed,-1.36f,1.36f);
                desired.Pan.y=Mathf.Clamp(desired.Pan.y+direction.z*speed,-.83f,.83f);
            }
            if(!overUI)
            {
                float wheel=mouse.scroll.ReadValue().y;
                float steps=Mathf.Abs(wheel)>8f?wheel/120f:wheel;
                desired.Zoom=Mathf.Clamp(desired.Zoom*Mathf.Exp(-.13f*Mathf.Clamp(steps,-3,3)),.18f,3.1f);
            }
        }

        private void LateUpdate()
        {
            if(!brain||brain.ChannelMask!=OutputChannels.Channel12||AAAUI.VFX.SignalWireBuilder.AnyWireDrawing)return;
            float damping=1f-Mathf.Exp(-18f*Time.unscaledDeltaTime);
            current.Yaw=Mathf.Lerp(current.Yaw,desired.Yaw,damping);
            current.Pitch=Mathf.Lerp(current.Pitch,desired.Pitch,damping);
            current.Zoom=Mathf.Lerp(current.Zoom,desired.Zoom,damping);
            current.Pan=Vector2.Lerp(current.Pan,desired.Pan,damping);
            ApplyPose(shots[shot],current);
            brain.ManualUpdate(Time.frameCount,Mathf.Min(Time.unscaledDeltaTime,.045f));
        }

        private void ApplyPose(CinemachineCamera vcam,View state)
        {
            bool focusSockets = vcam == shots[1] && socketAim.HasValue;
            Vector3 target = focusSockets
                ? socketAim.Value + arena.rotation * (new Vector3(state.Pan.x, .16f, state.Pan.y + .4f) * frameScale)
                : arena.position + arena.rotation * (new Vector3(state.Pan.x, .80f, state.Pan.y + .80f) * frameScale);
            Vector3 offset=arena.rotation*(Quaternion.Euler(state.Pitch,state.Yaw,0)*new Vector3(0,0,-5.2f)*frameScale);
            // The close-up looks down inside the table rim, away from the front fascia.
            Vector3 position = focusSockets ? target+offset*.68f : KeepCameraInsideRoom(room,target+offset);
            vcam.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position,arena.up));
            var lens=LensSettings.FromCamera(camera);
            lens.ModeOverride=LensSettings.OverrideModes.Orthographic;
            lens.OrthographicSize=frameScale*state.Zoom;
            lens.NearClipPlane=.12f;
            lens.FarClipPlane=originalFar;
            vcam.Lens=lens;
        }
        private void OnDisable()
        {
            foreach(var entry in covered)if(entry.Key)entry.Key.forceRenderingOff=entry.Value;
            covered.Clear();
            foreach(var vcam in shots)if(vcam){vcam.enabled=false;Destroy(vcam.gameObject);}
            if(brain)
            {
                brain.enabled=false;
                if(ownsBrain)Destroy(brain);
                else
                {
                    brain.ChannelMask=savedChannel;
                    brain.DefaultBlend=savedBlend;
                    brain.UpdateMethod=savedUpdate;
                    brain.LensModeOverride=savedLensMode;
                    brain.IgnoreTimeScale=savedIgnoreTimeScale;
                    brain.enabled=brainWasEnabled;
                }
            }
            if(legacy)legacy.enabled=legacyWasEnabled;
            if(camera==null)return;
            camera.fieldOfView=originalFov;
            camera.nearClipPlane=originalNear;
            camera.farClipPlane=originalFar;
            camera.transform.SetPositionAndRotation(originalPosition,originalRotation);
            camera.orthographic=originalOrthographic;
            camera.orthographicSize=originalSize;
        }
    }
}










