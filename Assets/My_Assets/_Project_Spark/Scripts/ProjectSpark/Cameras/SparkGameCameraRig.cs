using System;
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
    [DefaultExecutionOrder(9500)]
    public sealed class SparkGameCameraRig : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera[] views;
        [SerializeField] private Transform room;
        [SerializeField] private HolographicViewerCamera legacyViewer;
        private Camera output;
#if UNITY_ANDROID && !UNITY_EDITOR
        private int cameraTouchA, cameraTouchB;
        private Vector2 cameraTouchCenter, cameraTouchSpan;
        private Vector2Int cameraTouchViewport;
#endif
        private CinemachineBrain brain;
        private SparkLevelLayoutLoader loader;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool initialOrthographic;
        private float initialFov,initialNear,initialFar,initialSize;
        private int activeIndex;
        private bool ownsCamera,legacyWasEnabled,sharedOverride;
        private SparkLevelLayout overrideLayout;
        private RectTransform wallDisplay;
        private CinemachineCamera wallCamera;
        private WallViewState wallState;
        private const OutputChannels WallChannel = OutputChannels.Channel10;
        private OutputChannels levelChannel;
        private CinemachineBrain.UpdateMethods levelUpdate;
        private CinemachineBrain.BrainUpdateMethods levelBlendUpdate;
        private CinemachineBlendDefinition levelBlend;
        private CinemachineBrain.LensModeOverrideSettings levelLens;
        private bool levelBrainEnabled, levelIgnoreTimeScale;

        private bool HasSharedViews => views != null && views.Length == ViewNames.Length &&
            System.Array.TrueForAll(views, view => view != null);
        private bool UsesAnotherChannel => brain && brain.enabled && brain.ChannelMask != OutputChannels.Channel14;
        private float yaw,pitch,distance;
        private Vector2 pan,goalPan,lastPointer;
        private float goalYaw,goalPitch,goalDistance;
        private enum Drag{None,Orbit,Pan}
        private Drag drag;

        private static readonly string[] ViewNames={"Play Station","Worktop","Detail","Shelf","Showcase"};
        private const float ViewHudWidth=650f;
        private const float ViewHudHeight=76f;
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
        public RectTransform ActiveWallDisplay => wallDisplay;
        public SparkLevelLayout ActiveLayout => loader ? loader.ActiveLayout : null;
        public bool ViewControlsVisible { get; set; } = true;
        public event Action WallDisplayChanged;

        private struct WallViewState
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public bool Orthographic, BrainEnabled, IgnoreTimeScale, LegacyEnabled;
            public float FieldOfView, Size, Near, Far;
            public OutputChannels Channel;
            public CinemachineBrain.UpdateMethods Update;
            public CinemachineBrain.BrainUpdateMethods BlendUpdate;
            public CinemachineBlendDefinition Blend;
            public CinemachineBrain.LensModeOverrideSettings Lens;
        }

        private void Awake()
        {
            output=GetComponent<Camera>();
            brain=GetComponent<CinemachineBrain>();
            initialPosition=transform.position;initialRotation=transform.rotation;
            initialOrthographic=output.orthographic;initialFov=output.fieldOfView;
            initialNear=output.nearClipPlane;initialFar=output.farClipPlane;initialSize=output.orthographicSize;
            legacyWasEnabled=legacyViewer&&legacyViewer.enabled;
            loader=FindFirstObjectByType<SparkLevelLayoutLoader>();
            if (loader)
            {
                loader.LayoutLoaded += OnLayoutLoaded;
                loader.LayoutUnloaded += OnLayoutUnloaded;
            }
        }

        private bool HasLegacyLevelCamera()
        {
            var layout=loader?loader.ActiveLayout:null;
            return layout&&(layout.GetComponent<SparkEquipmentWorkbenchView>()||
                layout.GetComponent<SparkLevel84ArenaView>()||layout.GetComponent<SparkLevel85ArenaView>()||
                layout.GetComponent<SparkLevel86ArenaView>());
        }

        private bool DedicatedCameraActive() =>
            sharedOverride || HasLegacyLevelCamera() || UsesAnotherChannel;

        private void LateUpdate()
        {
            if (wallCamera) return;
            if(sharedOverride&&loader&&loader.ActiveLayout!=overrideLayout)
            {
                sharedOverride=false;
                overrideLayout=null;
            }
            if (!sharedOverride && DedicatedCameraActive())
            {
                ReleaseCamera();
                return;
            }
            if(!ownsCamera)
            {
                if (!HasSharedViews) return;
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
            legacyWasEnabled=legacyViewer&&legacyViewer.enabled;
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
            ownsCamera=false;drag=Drag.None;
            if (UsesAnotherChannel) return;
            if (HasLegacyLevelCamera())
            {
                brain.enabled = false;
                if (legacyViewer) legacyViewer.enabled = false;
                return;
            }
            if(brain)brain.enabled=false;
            if(legacyViewer)legacyViewer.enabled=legacyWasEnabled;
            transform.SetPositionAndRotation(initialPosition,initialRotation);
            output.orthographic=initialOrthographic;output.orthographicSize=initialSize;
            output.fieldOfView=initialFov;output.nearClipPlane=initialNear;output.farClipPlane=initialFar;
        }

        public void SelectView(int index)
        {
            if (SignalWireBuilder.AnyWireDrawing || !HasSharedViews || index < 0 || index >= views.Length) return;
            EndWallView(true);
            if(!ownsCamera)
            {
                if (!DedicatedCameraActive()) return;
                levelBrainEnabled = brain.enabled;
                levelChannel = brain.ChannelMask;
                levelUpdate = brain.UpdateMethod;
                levelBlendUpdate = brain.BlendUpdateMethod;
                levelBlend = brain.DefaultBlend;
                levelLens = brain.LensModeOverride;
                levelIgnoreTimeScale = brain.IgnoreTimeScale;
                sharedOverride=true;
                overrideLayout=loader?loader.ActiveLayout:null;
                TakeCamera();
            }
            views[activeIndex].Priority=10;
            activeIndex=index;
            ResetMotion();
            Apply(activeIndex,0,0,1,Vector2.zero);
            views[activeIndex].Priority=100;
            views[activeIndex].Prioritize();
        }

        public void FocusWallDisplay(RectTransform display, CinemachineCamera view)
        {
            if (!display || !view || !output || !brain || SignalWireBuilder.AnyWireDrawing || wallCamera == view) return;
            if (!wallCamera)
            {
                wallState = new WallViewState
                {
                    Position = transform.position, Rotation = transform.rotation,
                    Orthographic = output.orthographic, Size = output.orthographicSize,
                    FieldOfView = output.fieldOfView, Near = output.nearClipPlane, Far = output.farClipPlane,
                    BrainEnabled = brain.enabled, IgnoreTimeScale = brain.IgnoreTimeScale,
                    LegacyEnabled = legacyViewer && legacyViewer.enabled, Channel = brain.ChannelMask,
                    Update = brain.UpdateMethod, BlendUpdate = brain.BlendUpdateMethod,
                    Blend = brain.DefaultBlend, Lens = brain.LensModeOverride
                };
                if (legacyViewer) legacyViewer.enabled = false;
                brain.ChannelMask = WallChannel;
                brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
                brain.BlendUpdateMethod = CinemachineBrain.BrainUpdateMethods.LateUpdate;
                brain.IgnoreTimeScale = true;
                brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, .35f);
                brain.LensModeOverride = new CinemachineBrain.LensModeOverrideSettings
                    { Enabled = true, DefaultMode = LensSettings.OverrideModes.Perspective };
            }
            else
            {
                wallCamera.enabled = false;
            }
            wallDisplay = display;
            wallCamera = view;
            wallCamera.OutputChannel = WallChannel;
            wallCamera.Priority = 100;
            RefreshWallFraming();
            wallCamera.enabled = true;
            wallCamera.Prioritize();
            brain.enabled = true;
            drag = Drag.None;
            WallDisplayChanged?.Invoke();
        }

        public void LeaveWallDisplay(Transform display)
        {
            if (wallDisplay == display && !SignalWireBuilder.AnyWireDrawing) EndWallView(true);
        }

        public void RefreshWallFraming()
        {
            if (!wallDisplay || !wallCamera) return;
            var bounds = wallDisplay.rect.size;
            float height = bounds.y * Mathf.Abs(wallDisplay.lossyScale.y);
            float width = bounds.x * Mathf.Abs(wallDisplay.lossyScale.x);
            const float fieldOfView = 42f;
            float visibleHeight = Mathf.Max(height, width / Mathf.Max(.1f, output.aspect));
            float distanceToScreen = visibleHeight * .63f / Mathf.Tan(fieldOfView * .5f * Mathf.Deg2Rad);
            Vector3 forward = wallDisplay.forward;
            wallCamera.transform.SetPositionAndRotation(wallDisplay.position - forward * distanceToScreen,
                Quaternion.LookRotation(forward, wallDisplay.up));
            var lens = LensSettings.Default;
            lens.ModeOverride = LensSettings.OverrideModes.Perspective;
            lens.FieldOfView = fieldOfView;
            lens.NearClipPlane = .12f;
            lens.FarClipPlane = wallState.Far;
            wallCamera.Lens = lens;
        }

        private void OnLayoutLoaded(SparkLevelLayout layout) => EndWallView(false);
        private void OnLayoutUnloaded() => EndWallView(false);

        private void EndWallView(bool restorePose)
        {
            if (!wallCamera) return;
            wallCamera.enabled = false;
            wallCamera = null;
            wallDisplay = null;
            if (brain && brain.ChannelMask == WallChannel)
            {
                brain.ChannelMask = wallState.Channel;
                brain.UpdateMethod = wallState.Update;
                brain.BlendUpdateMethod = wallState.BlendUpdate;
                brain.DefaultBlend = wallState.Blend;
                brain.LensModeOverride = wallState.Lens;
                brain.IgnoreTimeScale = wallState.IgnoreTimeScale;
                brain.enabled = wallState.BrainEnabled;
                if (brain.enabled) brain.ResetState();
                if (restorePose)
                {
                    if (legacyViewer) legacyViewer.enabled = wallState.LegacyEnabled;
                    transform.SetPositionAndRotation(wallState.Position, wallState.Rotation);
                    output.orthographic = wallState.Orthographic;
                    output.orthographicSize = wallState.Size;
                    output.fieldOfView = wallState.FieldOfView;
                    output.nearClipPlane = wallState.Near;
                    output.farClipPlane = wallState.Far;
                }
            }
            WallDisplayChanged?.Invoke();
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
            if (wallCamera) return;
            if(!ownsCamera&& !DedicatedCameraActive())return;
            if(ownsCamera&&brain.ChannelMask!=OutputChannels.Channel14)return;
            if (!Application.isFocused || SignalWireBuilder.AnyWireDrawing || ProjectSpark.UIManagement.SparkUIMonitorDashboard.IsConsoleVisible)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                cameraTouchA = cameraTouchB = 0;
#endif
                drag=Drag.None;goalYaw=yaw;goalPitch=pitch;goalDistance=distance;goalPan=pan;
                return;
            }
            var keyboard=Keyboard.current;
            var selected = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            bool textEntryFocused = selected &&
                (selected.GetComponentInParent<UnityEngine.UI.InputField>() || selected.GetComponentInParent<TMPro.TMP_InputField>());
            if(keyboard!=null&&!textEntryFocused)
            {
                if(keyboard.digit1Key.wasPressedThisFrame||keyboard.numpad1Key.wasPressedThisFrame)SelectView(0);
                if(keyboard.digit2Key.wasPressedThisFrame||keyboard.numpad2Key.wasPressedThisFrame)SelectView(1);
                if(keyboard.digit3Key.wasPressedThisFrame||keyboard.numpad3Key.wasPressedThisFrame)SelectView(2);
                if(keyboard.digit4Key.wasPressedThisFrame||keyboard.numpad4Key.wasPressedThisFrame)SelectView(3);
                if(keyboard.digit5Key.wasPressedThisFrame||keyboard.numpad5Key.wasPressedThisFrame)SelectView(4);
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            UpdateTouchCamera();
#else
            var mouse=Mouse.current;
            if(mouse==null)return;
            bool overUI=EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            Vector2 pointer=mouse.position.ReadValue();
            if (IsPointerOverViewControls(pointer))
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
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void UpdateTouchCamera()
        {
            if (!SparkBenchPointer.TryGetCameraPair(out int firstId, out int secondId, out var first, out var second) ||
                IsPointerOverViewControls(first) || IsPointerOverViewControls(second))
            {
                cameraTouchA = cameraTouchB = 0;
                return;
            }
            Vector2 center = (first + second) * .5f;
            Vector2 span = second - first;
            var viewport = new Vector2Int(Screen.width, Screen.height);
            if (cameraTouchA == firstId && cameraTouchB == secondId && cameraTouchViewport == viewport)
            {
                float scale = 720f / Mathf.Max(1, Mathf.Min(viewport.x, viewport.y));
                Vector2 delta = (center - cameraTouchCenter) * scale;
                goalPan = Vector2.ClampMagnitude(goalPan - delta * (.0038f * distance), 2.15f);
                if (span.sqrMagnitude > 64f && cameraTouchSpan.sqrMagnitude > 64f)
                {
                    ZoomStep(Mathf.Clamp(Mathf.Log(span.magnitude / cameraTouchSpan.magnitude) * 9f, -2f, 2f));
                    goalYaw = Mathf.Clamp(goalYaw + Vector2.SignedAngle(cameraTouchSpan, span), -95f, 95f);
                }
            }
            cameraTouchA = firstId;
            cameraTouchB = secondId;
            cameraTouchViewport = viewport;
            cameraTouchCenter = center;
            cameraTouchSpan = span;
        }
#endif

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
            if (wallCamera || !isActiveAndEnabled || !ViewControlsVisible || ProjectSpark.UIManagement.SparkUIMonitorDashboard.IsConsoleVisible) return;
            int previousDepth=GUI.depth;
            bool previousEnabled=GUI.enabled;
            GUI.depth=-100;
            GUILayout.BeginArea(ViewHudRect,GUI.skin.box);
            GUILayout.BeginHorizontal();
            bool dedicated=DedicatedCameraActive();
            bool available=HasSharedViews && !SignalWireBuilder.AnyWireDrawing && (dedicated || ownsCamera);
            GUI.enabled=available;
            GUILayout.Label(dedicated&&!sharedOverride?"SHARED VIEWS · 1–5":"CAMERA · 1–5",GUILayout.Width(142),GUILayout.Height(34));
            for(int i=0;i<ViewNames.Length;i++)
            {
                var previous=GUI.backgroundColor;
                if(i==activeIndex&&available)GUI.backgroundColor=new Color(.18f,.78f,.88f);
                if(GUILayout.Button($"{i+1}  {ViewNames[i]}",GUILayout.Height(34)))SelectView(i);
                GUI.backgroundColor=previous;
            }
            GUI.enabled=true;
            GUILayout.EndHorizontal();
            if(sharedOverride)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Shared view active. Return to this level's camera:",GUILayout.Width(390));
                GUI.enabled = available;
                if(GUILayout.Button("LEVEL VIEW",GUILayout.Height(20)))ReturnToLevelView();
                GUILayout.EndHorizontal();
            }
            else if(dedicated)
                GUILayout.Label("Click a view or press 1–5. Use LEVEL VIEW to hand control back.");
            GUILayout.EndArea();
            GUI.enabled=previousEnabled;
            GUI.depth=previousDepth;
        }

        private Rect ViewHudRect
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                Rect safe = Screen.safeArea;
                float width = Mathf.Min(ViewHudWidth, safe.width);
                return new Rect(safe.center.x - width * .5f, Screen.height - safe.yMax + 78f,
                    width, ViewHudHeight);
#else
                return new Rect((Screen.width - ViewHudWidth) * .5f, 78, ViewHudWidth, ViewHudHeight);
#endif
            }
        }

        public bool IsPointerOverViewControls(Vector2 screenPoint) =>
            !wallCamera && isActiveAndEnabled && ViewControlsVisible && HasSharedViews &&
            ViewHudRect.Contains(new Vector2(screenPoint.x, Screen.height - screenPoint.y));

        public void ReturnToLevelView()
        {
            if (wallCamera && !SignalWireBuilder.AnyWireDrawing)
            {
                EndWallView(true);
                return;
            }
            if (!sharedOverride || SignalWireBuilder.AnyWireDrawing) return;
            RestoreLevelCamera();
        }

        private void RestoreLevelCamera()
        {
            sharedOverride = false;
            overrideLayout = null;
            ownsCamera = false;
            drag = Drag.None;
            brain.ChannelMask = levelChannel;
            brain.UpdateMethod = levelUpdate;
            brain.BlendUpdateMethod = levelBlendUpdate;
            brain.DefaultBlend = levelBlend;
            brain.LensModeOverride = levelLens;
            brain.IgnoreTimeScale = levelIgnoreTimeScale;
            brain.enabled = levelBrainEnabled;
            if (brain.enabled) brain.ResetState();
        }

        private void OnDisable()
        {
            EndWallView(true);
            if (sharedOverride && loader && loader.ActiveLayout == overrideLayout)
                RestoreLevelCamera();
            else
            {
                sharedOverride = false;
                overrideLayout = null;
                ReleaseCamera();
            }
        }

        private void OnDestroy()
        {
            if (!loader) return;
            loader.LayoutLoaded -= OnLayoutLoaded;
            loader.LayoutUnloaded -= OnLayoutUnloaded;
        }
    }
}
