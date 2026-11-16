using AAAUI.VFX;
using ProjectSpark.Gameplay;
using ProjectSpark.HolographicViewer;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjectSpark.Cameras
{
    [RequireComponent(typeof(Camera),typeof(CinemachineBrain))]
    [DefaultExecutionOrder(8500)]
    public sealed class SparkGameCameraRig : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera[] views;
        [SerializeField] private Transform room;
        [SerializeField] private HolographicViewerCamera legacyViewer;
        private Camera output;
        private CinemachineBrain brain;
        private SparkLevelLayoutLoader loader;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool initialOrthographic;
        private float initialFov,initialNear,initialFar,initialSize;
        private int activeIndex;
        private bool ownsCamera,legacyWasEnabled,showViews;
        private float yaw,pitch,distance;
        private Vector2 pan,goalPan,lastPointer;
        private float goalYaw,goalPitch,goalDistance;
        private enum Drag{None,Orbit,Pan}
        private Drag drag;

        private static readonly string[] ViewNames={"Play Station","Worktop","Detail","Shelf","Showcase"};
        private static readonly Vector3[] RoomEyes={
            new(0,2.85f,5.70f),new(0,4.65f,4.10f),new(0,2.72f,2.90f),
            new(-.35f,2.55f,2.55f),new(-5.90f,3.70f,5.75f)
        };
        private static readonly Vector3[] RoomTargets={
            new(0,1.48f,.30f),new(0,1.34f,.25f),new(.0f,1.44f,.52f),
            new(0,2.05f,-2.10f),new(.38f,1.62f,-.55f)
        };
        private static readonly float[] FieldsOfView={53,55,39,43,59};
        public bool OwnsCamera=>ownsCamera;
        public int SelectedView=>activeIndex;

        private void Awake()
        {
            output=GetComponent<Camera>();
            brain=GetComponent<CinemachineBrain>();
            initialPosition=transform.position;initialRotation=transform.rotation;
            initialOrthographic=output.orthographic;initialFov=output.fieldOfView;
            initialNear=output.nearClipPlane;initialFar=output.farClipPlane;initialSize=output.orthographicSize;
            legacyWasEnabled=legacyViewer&&legacyViewer.enabled;
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
        }

        private bool DedicatedCameraActive()
        {
            var layout=loader?loader.ActiveLayout:null;
            return layout&&(layout.GetComponent<SparkLevel84ArenaView>()||layout.GetComponent<SparkLevel85ArenaView>()||
                layout.GetComponent<SparkLevel86ArenaView>()||layout.GetComponent<SparkLevel87ArenaView>());
        }

        private void LateUpdate()
        {
            bool externallyAssigned=brain.enabled&&brain.ChannelMask!=OutputChannels.Channel14;
            bool dedicated=DedicatedCameraActive()||externallyAssigned;
            if(dedicated)
            {
                ReleaseCamera();
                return;
            }
            if(!ownsCamera)
            {
                if(views==null||views.Length!=ViewNames.Length)return;
                TakeCamera();
            }
            if(SignalWireBuilder.AnyWireDrawing)return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.045f);
            float blend=1-Mathf.Exp(-17*dt);
            yaw=Mathf.Lerp(yaw,goalYaw,blend);
            pitch=Mathf.Lerp(pitch,goalPitch,blend);
            distance=Mathf.Lerp(distance,goalDistance,blend);
            pan=Vector2.Lerp(pan,goalPan,blend);
            Apply(activeIndex,yaw,pitch,distance,pan);
            brain.ManualUpdate(Time.frameCount,dt);
        }

        private void TakeCamera()
        {
            ownsCamera=true;
            if(legacyViewer)legacyViewer.enabled=false;
            brain.ChannelMask=OutputChannels.Channel14;
            brain.UpdateMethod=CinemachineBrain.UpdateMethods.ManualUpdate;
            brain.BlendUpdateMethod=CinemachineBrain.BrainUpdateMethods.LateUpdate;
            brain.IgnoreTimeScale=true;
            brain.DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Custom,.52f)
                {CustomCurve=AnimationCurve.EaseInOut(0,0,1,1)};
            brain.LensModeOverride=new CinemachineBrain.LensModeOverrideSettings
                {Enabled=true,DefaultMode=LensSettings.OverrideModes.Perspective};
            for(int i=0;i<views.Length;i++)
            {
                views[i].OutputChannel=OutputChannels.Channel14;
                views[i].Priority=i==activeIndex?100:10;
                Apply(i,0,0,1,Vector2.zero);
            }
            ResetMotion();
            brain.enabled=true;
            brain.ResetState();
        }

        private void ReleaseCamera()
        {
            if(!ownsCamera)return;
            ownsCamera=false;drag=Drag.None;showViews=false;
            if(brain&&brain.ChannelMask!=OutputChannels.Channel14)return;
            if(brain)brain.enabled=false;
            if(legacyViewer)legacyViewer.enabled=legacyWasEnabled;
            transform.SetPositionAndRotation(initialPosition,initialRotation);
            output.orthographic=initialOrthographic;output.orthographicSize=initialSize;
            output.fieldOfView=initialFov;output.nearClipPlane=initialNear;output.farClipPlane=initialFar;
        }

        public void SelectView(int index)
        {
            if(!ownsCamera||SignalWireBuilder.AnyWireDrawing||index<0||index>=views.Length)return;
            views[activeIndex].Priority=10;
            activeIndex=index;
            ResetMotion();
            Apply(activeIndex,0,0,1,Vector2.zero);
            views[activeIndex].Priority=100;
            views[activeIndex].Prioritize();
        }

        public void ZoomStep(float steps)
        {
            if(ownsCamera&&!SignalWireBuilder.AnyWireDrawing)
                goalDistance=Mathf.Clamp(goalDistance*Mathf.Exp(-.14f*steps),.52f,1.4f);
        }

        private void ResetMotion()
        {
            yaw=pitch=goalYaw=goalPitch=0;
            distance=goalDistance=1;
            pan=goalPan=Vector2.zero;
            drag=Drag.None;
        }

        private void Update()
        {
            if(!ownsCamera||brain.ChannelMask!=OutputChannels.Channel14)return;
            if(!Application.isFocused||SignalWireBuilder.AnyWireDrawing)
            {
                drag=Drag.None;goalYaw=yaw;goalPitch=pitch;goalDistance=distance;goalPan=pan;
                return;
            }
            var keyboard=Keyboard.current;
            if(keyboard!=null&&(keyboard.leftAltKey.isPressed||keyboard.rightAltKey.isPressed))
            {
                if(keyboard.digit1Key.wasPressedThisFrame)SelectView(0);
                if(keyboard.digit2Key.wasPressedThisFrame)SelectView(1);
                if(keyboard.digit3Key.wasPressedThisFrame)SelectView(2);
                if(keyboard.digit4Key.wasPressedThisFrame)SelectView(3);
                if(keyboard.digit5Key.wasPressedThisFrame)SelectView(4);
                if(keyboard.vKey.wasPressedThisFrame)showViews=!showViews;
            }
            var mouse=Mouse.current;
            if(mouse==null)return;
            bool overUI=EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            Vector2 pointer=mouse.position.ReadValue();
            if(showViews&&pointer.x>=12&&pointer.x<=460&&Screen.height-pointer.y>=60&&Screen.height-pointer.y<=118)
                overUI=true;
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
                goalYaw=Mathf.Clamp(goalYaw+delta.x*.17f,-95,95);
                goalPitch=Mathf.Clamp(goalPitch-delta.y*.15f,-20,40);
            }
            else if(drag==Drag.Pan)
                goalPan=Vector2.ClampMagnitude(goalPan-delta*(.0038f*distance),2.15f);
            if(!overUI)
            {
                float raw=mouse.scroll.ReadValue().y;
                ZoomStep(Mathf.Clamp(Mathf.Abs(raw)>8?raw/120:raw,-3,3));
            }
        }

        private void Apply(int index,float orbitYaw,float orbitPitch,float scale,Vector2 offset)
        {
            Vector3 eye=RoomEyes[index], aim=RoomTargets[index];
            var arm=Quaternion.Euler(orbitPitch,orbitYaw,0)*(eye-aim)*scale;
            var heading=Quaternion.LookRotation(-arm);
            aim+=heading*new Vector3(offset.x,offset.y,0);
            eye=aim+arm;
            if(room)
            {
                eye.x=Mathf.Clamp(eye.x,-8.1f,8.1f);
                eye.y=Mathf.Clamp(eye.y,1.65f,5.2f);
                eye.z=Mathf.Clamp(eye.z,-6.1f,6.15f);
                eye=room.TransformPoint(eye);
                aim=room.TransformPoint(aim);
            }
            else
            {
                // Legacy scenes use the installed camera as their local reference.
                eye=initialPosition+initialRotation*(eye-RoomEyes[0]);
                aim=initialPosition+initialRotation*(aim-RoomEyes[0]+Vector3.up*1.37f);
            }
            var camera=views[index];
            camera.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(aim-eye));
            var lens=LensSettings.Default;
            lens.ModeOverride=LensSettings.OverrideModes.Perspective;
            lens.FieldOfView=FieldsOfView[index];
            lens.NearClipPlane=.12f;
            lens.FarClipPlane=initialFar;
            camera.Lens=lens;
        }

        private void OnGUI()
        {
            if(!showViews||!ownsCamera)return;
            GUILayout.BeginArea(new Rect(12,60,448,58),GUI.skin.box);
            GUILayout.Label("Game Camera");
            GUILayout.BeginHorizontal();
            for(int i=0;i<ViewNames.Length;i++)
                if(GUILayout.Button(ViewNames[i]))SelectView(i);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void OnDisable()=>ReleaseCamera();
    }
}
