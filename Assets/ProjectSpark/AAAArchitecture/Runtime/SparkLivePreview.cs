using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ProjectSpark.AAAArchitecture
{
    [DisallowMultipleComponent]
    public sealed class SparkLivePreview : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        private RawImage screen;
        private Image fallback;
        private TMP_Text modeLabel;
        private Button toggle, minus, plus, reset;
        private SparkLivePreviewSettings settings;
        private SparkLivePreviewAsset asset;
        private Camera cameraView;
        private Light[] lights;
        private GameObject stage;
        private RenderTexture target;
        private Mesh studioMesh;
        private Material studioMaterial;
        private int level = -1, layer = -1;
        private ScriptableObject selectedDefinition;
        private bool requested, dirty, dragging, collisionWarning;
        private float yaw = 40, pitch = 35, zoom = 1, nextFrame;
        private const float Aspect = 16f / 9f;
        private static readonly Vector3 StageOrigin = new Vector3(0, -500, 0);
        private const string Pref = "SparkLivePreview.Mode";
        private bool IsLive => requested && asset && cameraView && target;

        public static SparkLivePreview Attach(Image still)
        {
            if (!still) return null;
            var parent = (RectTransform)still.transform.parent;
            var old = parent.GetComponentInChildren<SparkLivePreview>(true);
            if (old) return old;
            var rect = SparkCinematicSkin.Rect("Live Hardware Preview", parent, Vector2.zero, Vector2.one,
                new Vector2(7, 35), new Vector2(-7, -7));
            rect.gameObject.SetActive(false);
            var preview = rect.gameObject.AddComponent<SparkLivePreview>();
            preview.fallback = still;
            // Keep the static artwork in the same region, under the live texture.
            still.rectTransform.offsetMin = new Vector2(7,35); still.rectTransform.offsetMax = new Vector2(-7,-7);
            preview.screen = rect.gameObject.AddComponent<RawImage>();
            preview.screen.raycastTarget = false; preview.screen.enabled = false;
            var fit = rect.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = Aspect;
            // AspectRatioFitter controls anchors: use fixed region parent so toolbar is never covered.
            var region = SparkCinematicSkin.Rect("Preview Image Region", parent, Vector2.zero, Vector2.one,
                new Vector2(7,35), new Vector2(-7,-7));
            rect.SetParent(region, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var toolbar = SparkCinematicSkin.Rect("Preview Controls", parent, Vector2.zero, Vector2.right,
                new Vector2(6,4), new Vector2(-6,30));
            var layout = toolbar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4; layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = false; layout.childForceExpandHeight = true;
            preview.toggle = MakeButton(toolbar,"LIVE 3D",90,preview.Toggle,out preview.modeLabel);
            preview.minus = MakeButton(toolbar,"−",30,()=>preview.ZoomBy(-.15f),out _);
            preview.plus = MakeButton(toolbar,"+",30,()=>preview.ZoomBy(.15f),out _);
            preview.reset = MakeButton(toolbar,"RESET",46,preview.ResetView,out _);
            preview.settings = Resources.Load<SparkLivePreviewSettings>(SparkLivePreviewSettings.Root+"Settings");
            preview.requested = PlayerPrefs.GetInt(Pref, preview.settings && preview.settings.defaultLive ? 1 : 0) != 0;
            rect.gameObject.SetActive(true);
            preview.UpdateControls();
            return preview;
        }
        private static Button MakeButton(RectTransform parent,string label,float width,Action action,out TMP_Text text)
        {
            var r = SparkCinematicSkin.Rect(label,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var size = r.gameObject.AddComponent<LayoutElement>(); size.preferredWidth=width; size.minWidth=width;
            var image = r.gameObject.AddComponent<Image>(); image.color = new Color32(18,45,60,255);
            var button = r.gameObject.AddComponent<Button>(); button.targetGraphic=image;
            SparkCinematicSkin.StyleButton(button); button.onClick.AddListener(()=>action());
            var t = SparkCinematicSkin.Rect("Label",r,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            text=t.gameObject.AddComponent<TextMeshProUGUI>(); text.text=label; text.fontSize=12;
            text.alignment=TextAlignmentOptions.Center; text.color=SparkCinematicSkin.Ink; text.raycastTarget=false;
            return button;
        }
        public static void AddBackdrop(RectTransform parent)
        {
            var s=Resources.Load<SparkLivePreviewSettings>(SparkLivePreviewSettings.Root+"Settings");
            if (!s || !s.laboratoryBackdrop || !parent) return;
            var r=SparkCinematicSkin.Rect("Laboratory Atmosphere",parent,Vector2.zero,Vector2.one,new Vector2(6,6),new Vector2(-6,-6));
            r.SetAsFirstSibling();
            var image=r.gameObject.AddComponent<Image>(); image.sprite=s.laboratoryBackdrop;
            image.color=new Color(1,1,1,.72f); image.raycastTarget=false;
        }
        public void SelectLevel(int number, ScriptableObject definition)
        {
            if (level == number && selectedDefinition == definition) { UpdateControls(); return; }
            level=number; selectedDefinition=definition; ReleaseAsset(); ResetView();
            if (isActiveAndEnabled) RefreshSource();
        }
        private void OnEnable() { RefreshSource(); }
        private void OnDisable() { ReleaseAsset(); ReleaseStage(); dragging=false; }
        private void OnDestroy() { ReleaseAsset(); ReleaseStage(); }
        private void RefreshSource()
        {
            if (!screen || !settings || !requested || level < 1 || !selectedDefinition) { ReleaseStage(); UpdateControls(); return; }
            layer=LayerMask.NameToLayer(SparkLivePreviewSettings.LayerName);
            if (layer < 8 || layer > 30 || !SystemInfo.supportsRenderTextures || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            { ReleaseStage(); UpdateControls(); return; }
            if (!asset) asset=Resources.Load<SparkLivePreviewAsset>(SparkLivePreviewSettings.Root+"Levels/PS-L"+level.ToString("000"));
            int budget=Application.isMobilePlatform?settings.mobileDrawBudget:settings.desktopDrawBudget;
            if (!asset || asset.levelNumber!=level || asset.definition!=selectedDefinition || asset.parts==null || asset.parts.Length==0 || asset.parts.Length>budget || asset.indexCount > (Application.isMobilePlatform ? settings.mobileIndexBudget : settings.desktopIndexBudget))
            { ReleaseAsset(); ReleaseStage(); UpdateControls(); return; }
            foreach (var p in asset.parts)
                if (!p.mesh || !p.material || !p.material.shader || !p.material.shader.isSupported || p.submesh<0 || p.submesh>=p.mesh.subMeshCount)
                { ReleaseAsset(); ReleaseStage(); UpdateControls(); return; }
            try { EnsureStage(); dirty=true; }
            catch(Exception e) { Debug.LogWarning("[Spark Live Preview] Static fallback: "+e.Message,this); ReleaseStage(); ReleaseAsset(); }
            UpdateControls();
        }
        private void EnsureStage()
        {
            if (stage && target) return;
            ReleaseStage();
            stage=new GameObject("Spark Isolated Preview Camera"); stage.hideFlags=HideFlags.DontSave;
            stage.transform.position=StageOrigin;
            var go=new GameObject("Preview Camera"); go.transform.SetParent(stage.transform,false);
            cameraView=go.AddComponent<Camera>(); cameraView.enabled=false; cameraView.clearFlags=CameraClearFlags.SolidColor;
            cameraView.backgroundColor=new Color(.025f,.05f,.077f,1); cameraView.cullingMask=1<<layer;
            cameraView.orthographic=true; cameraView.aspect=Aspect; cameraView.nearClipPlane=.05f; cameraView.farClipPlane=12; cameraView.allowHDR=false; cameraView.allowMSAA=false;
            cameraView.useOcclusionCulling=false; cameraView.depth=-100;
            int w=Mathf.Clamp(Application.isMobilePlatform?settings.mobileWidth:settings.desktopWidth,256,1920);
            target=new RenderTexture(w,Mathf.RoundToInt(w/Aspect),24,RenderTextureFormat.ARGB32);
            target.name="Spark Shared Hardware Preview"; target.antiAliasing=1; target.useMipMap=false;
            if (!target.Create()) throw new InvalidOperationException("RenderTexture creation failed.");
            cameraView.targetTexture=target;
            lights=new[]{MakeLight("Key",new Vector3(3,4,-3),new Color(1,.88f,.73f),2.0f),
                MakeLight("Rim",new Vector3(-3,2,2),new Color(.28f,.72f,1),2.5f),
                MakeLight("Fill",new Vector3(0,2,-4),new Color(.7f,.85f,1),1.2f)};
            screen.texture=target;
            studioMaterial=Resources.Load<Material>(SparkShowcaseSkin.Root+"Stage");
            if(studioMaterial && studioMaterial.shader.isSupported) studioMesh=SparkShowcaseStage.Create();
        }
        private Light MakeLight(string label,Vector3 position,Color color,float intensity)
        {
            var go=new GameObject(label); go.transform.SetParent(stage.transform,false); go.transform.localPosition=position;
            var l=go.AddComponent<Light>(); l.type=LightType.Point; l.color=color; l.intensity=intensity;
            l.range=10; l.cullingMask=1<<layer; l.shadows=LightShadows.None; l.enabled=false; return l;
        }
        private void LateUpdate()
        {
            if (!IsLive) return;
            if (!target.IsCreated())
            {
                if(!target.Create()) { ReleaseStage(); ReleaseAsset(); UpdateControls(); return; }
                dirty=true;
            }
            cameraView.enabled=false;
            if(lights!=null) foreach(var light in lights) if(light) light.enabled=false;
            if (settings.autoOrbit && !SparkCinematicClock.ReducedMotion && !dragging && Time.unscaledTime>=nextFrame)
            { yaw+=12f/Mathf.Max(10,settings.orbitFps); nextFrame=Time.unscaledTime+1f/Mathf.Max(10,settings.orbitFps); dirty=true; }
            if (!dirty) return;
            dirty=false;
            // DrawMesh is scoped to this camera; no gameplay objects or scripts are instantiated.
            var q=Quaternion.Euler(pitch,yaw,0);
            cameraView.transform.SetPositionAndRotation(StageOrigin+q*new Vector3(0,0,-5),q);
            cameraView.orthographicSize=(studioMesh?1.62f:1.18f)/zoom;
            try
            {
                var shift=Matrix4x4.Translate(StageOrigin);
                if(studioMesh && studioMaterial) Graphics.DrawMesh(studioMesh,shift,studioMaterial,layer,cameraView,0,null,ShadowCastingMode.Off,false,null,LightProbeUsage.Off,null);
                foreach(var p in asset.parts)
                    Graphics.DrawMesh(p.mesh,shift*p.matrix,p.material,layer,cameraView,p.submesh,null,ShadowCastingMode.Off,false,null,LightProbeUsage.Off,null);
                if(lights!=null) foreach(var light in lights) if(light) light.enabled=true;
                cameraView.enabled=true;
                screen.enabled=true; screen.raycastTarget=true;
            }
            catch(Exception e)
            { if(!collisionWarning){Debug.LogWarning("[Spark Live Preview] "+e.Message,this);collisionWarning=true;} ReleaseAsset();ReleaseStage();UpdateControls(); }
        }
        private void Toggle()
        {
            requested=!requested; PlayerPrefs.SetInt(Pref,requested?1:0);
            if (!requested) { ReleaseAsset(); ReleaseStage(); UpdateControls(); } else RefreshSource();
        }
        private void UpdateControls()
        {
            if (!screen) return;
            if (!IsLive) { screen.enabled=false; screen.raycastTarget=false; }
            if (modeLabel) modeLabel.text=IsLive?"STATIC 2D":requested && level>0?"2D FALLBACK":"LIVE 3D";
            if (toggle) toggle.interactable=settings && level>0;
            if (minus) minus.interactable=IsLive;
            if (plus) plus.interactable=IsLive;
            if (reset) reset.interactable=IsLive;
        }
        private void ReleaseAsset()
        {
            asset=null; // Assets may be shared with another UI; Unity owns their lifetime.
            if(screen){screen.enabled=false;screen.raycastTarget=false;}
            if(cameraView)cameraView.enabled=false;
            if(lights!=null) foreach(var light in lights) if(light) light.enabled=false;
        }
        private void ReleaseStage()
        {
            if(cameraView){cameraView.enabled=false;cameraView.targetTexture=null;}
            if(screen)screen.texture=null;
            if(target){target.Release();Destroy(target);target=null;}
            if(stage){stage.SetActive(false);Destroy(stage);stage=null;}
            if(studioMesh){Destroy(studioMesh);studioMesh=null;} studioMaterial=null;
            cameraView=null; lights=null;
        }
        public void OnBeginDrag(PointerEventData e){dragging=IsLive;}
        public void OnDrag(PointerEventData e){if(!IsLive||!dragging)return;yaw+=e.delta.x*.35f;pitch=Mathf.Clamp(pitch-e.delta.y*.32f,-70,80);dirty=true;}
        public void OnEndDrag(PointerEventData e){dragging=false;}
        public void OnScroll(PointerEventData e){if(IsLive)ZoomBy(e.scrollDelta.y*.12f);}
        private void ZoomBy(float delta){zoom=Mathf.Clamp(zoom+delta,.65f,2.3f);dirty=true;}
        private void ResetView(){yaw=40;pitch=35;zoom=1;dirty=true;}
    }
}
