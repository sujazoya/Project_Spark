using System;
using System.Collections;
using System.Linq;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Cameras;
using ProjectSpark.Diagnostics;
using ProjectSpark.Gameplay;
using ProjectSpark.UI;
using ProjectSpark.UI.VisualTeaching;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace ProjectSpark.UIManagement
{
    // MainMenu's existing panels remain responsible for their own feature data.
    // This adapter routes native Show/Hide actions through the shared manager.
    [DisallowMultipleComponent]
    public sealed class SparkMainMenuPanelBindings : MonoBehaviour
    {
        private const string Tools="ProjectSparkUI/Main_HUD/HUD-Panel /";
        private SparkUIManager manager;
        private LevelGamePlayManager levels;
        private SparkUIMonitorDashboard dashboard;
        private string levelControlsId;
        private Coroutine pendingControls;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded-=Attach;
            SceneManager.sceneLoaded+=Attach;
            Attach(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void Attach(Scene scene, LoadSceneMode mode)
        {
            if(scene.name!="MainMenu") return;
            var core=scene.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<SparkUIManager>(true))
                .FirstOrDefault();
            if(core && !core.GetComponent<SparkMainMenuPanelBindings>())
                core.gameObject.AddComponent<SparkMainMenuPanelBindings>();
        }

        private IEnumerator Start()
        {
            // The wire list, AI and diagnostics generate their visuals on startup.
            yield return null;
            manager=GetComponent<SparkUIManager>();
            dashboard=Find<SparkUIMonitorDashboard>();
            levels=Find<LevelGamePlayManager>();
            if(!manager || !dashboard)yield break;

            var workshop=Node("ProjectSparkUI/MonitorScreen canvas");
            var computer=Node("ProjectSparkUI/MonitorScreen canvas/Motitor Parent");
            var monitorCanvas = workshop ? workshop.GetComponent<Canvas>() : null;
            var camera=Find<SparkGameCameraRig>();
            var liveCoach=Find<SparkTutorialCircuitAwareness>();
            var wallDisplays=gameObject.AddComponent<SparkWorkshopWallDisplays>();
            var room=Node("PS_PRO_GAME_ROOM");
            bool wallsMounted=wallDisplays.Mount(room ? room.transform : null,camera);
            if(wallsMounted)
                wallDisplays.Connect(dashboard,manager,monitorCanvas,computer ? computer.transform as RectTransform : null,liveCoach);
            else
            {
                dashboard.AttachToWorkshopMonitor(monitorCanvas);
                RegisterObject("workshop","Workshop Computer",SparkUILayer.Main,computer,"workshop");
            }

            var wires=Find<SparkWireListUI>();
            var wirePanel=Node("ProjectSpark Wire Manager UI/Wire Registry Panel");
            if(wires && wirePanel)
            {
                Native("wires","Wire Manager",SparkUILayer.Tool,visible=>{if(visible)wires.Show();else wires.Hide();},
                    ()=>wirePanel && wirePanel.activeSelf);
                TopRight(wirePanel,24f,88f,410f);
            }

            var meter=Find<SparkVirtualMultimeter>();
            if(meter)
            {
                Native("meter","Virtual Multimeter",SparkUILayer.Tool,visible=>{if(visible)meter.ShowExpanded();else meter.Hide();},
                    ()=>meter && meter.CurrentDrawerMode!=SparkVirtualMultimeter.DrawerMode.Hidden);
                MaskLauncher(Child(meter.transform,"OpenMultimeter","OpenButton","DrawerTab","DrawerToggle","MeterDrawerTab"));
                MaskMeterLaunchers(meter.transform);
            }

            var faults=Find<SparkWorkbenchFaultInspector>();
            if(faults)
            {
                Native("faults","Fault Inspector",SparkUILayer.Tool,visible=>{if(visible)faults.Show();else faults.Hide();},
                    ()=>faults && faults.Expanded);
                TopRight(Node("SparkWorkbenchFaultInspectorCanvas/ExpandedPanel"),24f,88f,398f);
                MaskLauncher(Node("SparkWorkbenchFaultInspectorCanvas/CollapsedTab"));
            }

            var telemetry=Node("SparkAIVisualTeachingCanvas/LiveElectricalTelemetryCard");
            RegisterObject("telemetry","Live Electrical Telemetry",SparkUILayer.Tool,telemetry,"workshop");
            TopRight(telemetry,24f,88f,386f);

            var voltage=Find<SparkVoltageDrawer>();
            var voltageRoot=Node("Spark_VoltageMonitor_Canvas");
            if(voltage && voltageRoot && voltageRoot.TryGetComponent<Canvas>(out var voltageCanvas))
            {
                Native("voltage","Voltage Monitor",SparkUILayer.Tool,visible=>
                {
                    voltageCanvas.enabled=visible;
                    if(voltageRoot.TryGetComponent<GraphicRaycaster>(out var raycaster))raycaster.enabled=visible;
                    if(visible)voltage.Open();else voltage.Close();
                },()=>voltage && voltage.IsOpen);
            }
            MaskLauncher(Node("Spark_VoltageMonitor_Canvas/VoltageMonitor_Panel/VoltageDrawer_ToggleTab"));

            var experiments=Find<SparkCircuitExperimentLab>();
            var experimentWindow=Node("SparkCircuitExperimentLabCanvas/ExperimentWindow");
            if(experiments && experimentWindow)
                Native("experiments","Circuit Experiment Lab",SparkUILayer.Tool,visible=>{if(visible)experiments.Show();else experiments.Hide();},
                    ()=>experimentWindow && experimentWindow.activeSelf);

            MaskLauncher(Node("SparkCircuitExperimentLabCanvas/OpenExperimentLab"));
            var assistant=Find<SparkAIUnifiedAssistant>();
            if(assistant)assistant.ArrangeWorkshopPanel();
            if(assistant)
                Native("assistant","Ask Spark",SparkUILayer.Main,visible=>{if(visible)assistant.Show();else assistant.Hide();},
                    ()=>assistant && assistant.PanelVisible);

            if(liveCoach)
                Native("live-coach","Live Circuit Coach",SparkUILayer.Overlay,liveCoach.SetPanelOpen,
                    ()=>liveCoach && liveCoach.PanelVisible);
            var quickAI=Node("ProjectSpark_Gameplay/Spark AI/Canvas/AI Question Panel");
            RegisterObject("quick-ai","Quick AI Question",SparkUILayer.Overlay,quickAI,"workshop");
            TopRight(quickAI,24f,88f,580f);
            var learning=Find<SparkAILearningPanel>();
            var learningCanvas=learning ? learning.GetComponent<Canvas>() : null;
            if(learning && learningCanvas)
                Native("learning","Learning Library",SparkUILayer.Main,visible=>
                {
                    learningCanvas.enabled=visible;
                    if(learning.TryGetComponent<GraphicRaycaster>(out var raycaster))raycaster.enabled=visible;
                    if(visible)learning.ShowDetailed();else learning.HideDrawer();
                },()=>learning && learningCanvas.isActiveAndEnabled &&
                    learning.CurrentMode!=SparkAILearningPanel.DrawerMode.Hidden);

            var teaching=Find<SparkVisualTeachingManager>();
            if(teaching)
                Native("whiteboard","Teaching Whiteboard",SparkUILayer.Main,visible=>
                {
                    if(visible){teaching.ViewWhiteboard();teaching.OpenPanel();}
                    else teaching.ClosePanel();
                },()=>teaching && teaching.PanelOpen);
            var shop=Find<ProjectSparkShopRuntime>();
            var shopBackdrop=shop ? Child(shop.transform,"Backdrop") : null;
            if(shop && shopBackdrop)
                Native("shop","Component Shop",SparkUILayer.Main,visible=>{if(visible)shop.Show();else shop.Hide();},
                    ()=>shopBackdrop && shopBackdrop.activeInHierarchy);

            var inventory=Find<ProjectSparkInventoryPanel>();
            var inventoryBackdrop=inventory ? Child(inventory.transform,"Backdrop") : null;
            if(inventory && inventoryBackdrop)
                Native("inventory","Inventory",SparkUILayer.Main,visible=>{if(visible)inventory.Show();else inventory.Hide();},
                    ()=>inventoryBackdrop && inventoryBackdrop.activeInHierarchy);

            RegisterCanvas("bench-display","Bench Display",SparkUILayer.HUD,Node("ProjectSparkUI/Disply_Canvas"));
            if(Find<SparkShelfRemovePrompt>())dashboard.RegisterRecyclingHelp();

            RegisterObject("tools","Workshop Tool Rail",SparkUILayer.HUD,Node(Tools+"Tools-Panel"));
            var measurement=Node(Tools+"MeasurementHUD");
            if(measurement)
                Native("measurement","Geometry Measurement",SparkUILayer.Tool,visible=>
                {
                    measurement.SetActive(visible);
                    if(visible)TopRight(measurement,24f,88f,240f);
                },()=>measurement && measurement.activeInHierarchy);
            if(measurement && measurement.TryGetComponent<Animator>(out var measurementSlide))measurementSlide.enabled=false;
            if(measurement)
            {
                var root=(RectTransform)measurement.transform;
                root.sizeDelta=new Vector2(240,400);
                var buttons=measurement.GetComponentsInChildren<Button>(true);
                for(int i=0;i<buttons.Length;i++)
                {
                    var rect=(RectTransform)buttons[i].transform;
                    rect.SetParent(root,false);
                    rect.anchorMin=rect.anchorMax=new Vector2(0,1);
                    rect.pivot=new Vector2(0,1);
                    rect.anchoredPosition3D=new Vector3(0,-i*74,0);
                    rect.sizeDelta=new Vector2(62,62);
                    rect.localScale=Vector3.one;
                    var labelObject=new GameObject("Tool label",typeof(RectTransform),typeof(Text));
                    var label=labelObject.GetComponent<Text>();
                    label.rectTransform.SetParent(rect,false);
                    label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(1,.5f);
                    label.rectTransform.pivot=new Vector2(0,.5f);
                    label.rectTransform.anchoredPosition=new Vector2(12,0);
                    label.rectTransform.sizeDelta=new Vector2(155,50);
                    label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    label.fontSize=17;
                    label.color=new Color(.8f,.95f,1f);
                    label.text=buttons[i].name.Replace("Button","").Trim();
                    label.alignment=TextAnchor.MiddleLeft;
                    label.raycastTarget=false;
                }
                TopRight(measurement,24f,88f,240f);
            }
            RegisterObject("view-tools","View and Inspection Tools",SparkUILayer.Tool,Node(Tools+"HUD_ViewControls"),"workshop");

            if(camera)
                Native("camera","Camera Views",SparkUILayer.HUD,visible=>camera.ViewControlsVisible=visible,
                    ()=>camera && camera.ViewControlsVisible,"");

            var diagnostics=Find<SparkAdvancedDiagnostics>();
            if(diagnostics)
                Native("diagnostics","Advanced Diagnostics",SparkUILayer.System,visible=>diagnostics.PanelVisible=visible,
                    ()=>diagnostics && diagnostics.PanelVisible,"workshop",true);

            var tutorial=Find<ProjectSparkTutorialHub>();
            if(tutorial)
                Native("tutorial","Tutorial and Objectives",SparkUILayer.Main,visible=>tutorial.SetOpen(visible),
                    ()=>tutorial && tutorial.DrawerOpen);

            var shopDrawer=Find<ProjectSparkShopHUDDrawer>();
            if(shopDrawer)
                Native("shop-quick","Shop Quick Access",SparkUILayer.Tool,visible=>shopDrawer.SetOpen(visible),
                    ()=>shopDrawer && shopDrawer.DrawerOpen);

            var fullTools=Find<ProjectSparkThreeModeToolsPanel>();
            if(fullTools)
                BindExisting("SparkThreeModeTools","Expanded Workshop Tools",SparkUILayer.Tool,
                    visible=>fullTools.SetModeImmediate(visible ? ProjectSparkThreeModeToolsPanel.DisplayMode.Complete :
                        ProjectSparkThreeModeToolsPanel.DisplayMode.Hidden),
                    ()=>fullTools && fullTools.gameObject.activeInHierarchy &&
                        fullTools.CurrentMode!=ProjectSparkThreeModeToolsPanel.DisplayMode.Hidden);

            var assessment=Find<SparkAILearningPathAssessment>();
            if(assessment)
                BindExisting("SparkAIAssessmentCanvas","Learning Assessments",SparkUILayer.Main,
                    visible=>{if(visible)assessment.Show();else assessment.Hide();},
                    ()=>assessment && assessment.PanelVisible);
            MaskLauncher(Node(Tools+"SparkThreeModeTools/ModeToggle"));
            MaskLauncher(Node("ProjectSparkUI/Main_HUD/ShopQuickAccess/DrawerToggle"));
            MaskLauncher(Node("ProjectSparkUI/Main_HUD/TutorialHubRoot/Drawer/DrawerToggle"));
            MaskLauncher(Node("SparkAIVisualTeachingCanvas/OpenVisualTeacherButton"));
            MaskLauncher(Node("SparkAIVisualTeachingCanvas/SparkAdaptiveExplanationRoot/SparkAINaturalExplanationRoot/AskAILauncher"));
            MaskLauncher(Node(Tools+"Level-7"));
            MaskLauncher(Node("ProjectSparkUI/Main_HUD/LOGO"));
            MaskLauncher(Node("ProjectSparkUI/Main_HUD/battery"));

            var next=Node("ProjectSparkUI/Main_HUD/Next");
            var previous=Node("ProjectSparkUI/Main_HUD/previouse");
            TopRight(next,28f,86f,58f);
            TopRight(previous,94f,86f,58f);
            if(next && previous)
                Native("levels","Level Navigation",SparkUILayer.HUD,visible=>{next.SetActive(visible);previous.SetActive(visible);},
                    ()=>next && previous && (next.activeSelf || previous.activeSelf),"");

            if(levels)
            {
                levels.LevelStarted+=ScheduleLevelControls;
                ScheduleLevelControls(levels.ActiveLevel);
            }
            dashboard.AttachCampaign(levels, Find<SparkCampaignPanel>());
            dashboard.SetVisible(!wallsMounted);
        }

        private void ScheduleLevelControls(SparkLevelDefinition definition)
        {
            if(pendingControls!=null)StopCoroutine(pendingControls);
            pendingControls=StartCoroutine(BindLevelControls());
        }

        private IEnumerator BindLevelControls()
        {
            yield return null;
            yield return null;
            if(!string.IsNullOrEmpty(levelControlsId))manager.Unregister(levelControlsId);
            var activeLayout=Find<SparkLevelLayoutLoader>()?.ActiveLayout;
            if(!activeLayout)yield break;
            var canvas=activeLayout.GetComponentsInChildren<Canvas>(true)
                .FirstOrDefault(candidate=>candidate.name is "Board Foundry controls" or "Pulse Architect controls" or "Sensor Matrix controls" or "Voltage Guardian controls");
            if(!canvas)yield break;
            levelControlsId="active-level";
            RegisterCanvas(levelControlsId,"Current Level Controls",SparkUILayer.HUD,canvas.gameObject);
        }

        private void OnDestroy()
        {
            if(levels)levels.LevelStarted-=ScheduleLevelControls;
        }

        private void Native(string id,string label,SparkUILayer layer,Action<bool> apply,Func<bool> read,
            string group="workshop",bool blocks=false)
        {
            manager.RegisterNative(id,label,layer,group,blocks,apply,read);
        }

        private void BindExisting(string id,string label,SparkUILayer layer,Action<bool> apply,Func<bool> read)
        {
            var panel=manager.Find(id);
            if(panel)panel.BindNative(id,label,layer,"workshop",false,apply,read);
            else Native(id,label,layer,apply,read);
        }

        private void RegisterObject(string id,string label,SparkUILayer layer,GameObject root,string group="")
        {
            if(root)Native(id,label,layer,value=>{if(root)root.SetActive(value);},()=>root && root.activeInHierarchy,group);
        }

        private void RegisterCanvas(string id,string label,SparkUILayer layer,GameObject root,string group="")
        {
            if(!root || !root.TryGetComponent<Canvas>(out var canvas))return;
            Native(id,label,layer,value=>
            {
                if(!canvas)return;
                if(value && !root.activeSelf)root.SetActive(true);
                canvas.enabled=value;
                if(root.TryGetComponent<GraphicRaycaster>(out var raycaster))raycaster.enabled=value;
            },()=>canvas && canvas.isActiveAndEnabled,group);
        }

        private static void TopRight(GameObject root,float x,float y,float width)
        {
            if(!root || !root.TryGetComponent<RectTransform>(out var rect))return;
            var originalSize=rect.rect.size;
            if(originalSize.x<=0 || originalSize.y<=0)return;
            float scale=Mathf.Min(1f,width/originalSize.x);
            rect.anchorMin=rect.anchorMax=new Vector2(1,1);
            rect.pivot=new Vector2(1,1);
            rect.sizeDelta=originalSize;
            rect.localRotation=Quaternion.identity;
            rect.anchoredPosition3D=new Vector3(-x,-y,0f);
            rect.localScale=new Vector3(scale,scale,1);
        }

        private static void MaskMeterLaunchers(Transform meter)
        {
            foreach(var button in meter.GetComponentsInChildren<Button>(true))
            {
                if(button.transform.IsChildOf(meter.Find("MultimeterPanel")) ||
                   button.transform.IsChildOf(meter.Find("MultimeterSmallPanel")))continue;
                if(button.name.IndexOf("tab",StringComparison.OrdinalIgnoreCase)>=0)MaskLauncher(button.gameObject);
            }
        }

        private static void MaskLauncher(GameObject root)
        {
            if(!root)return;
            var group=root.GetComponent<CanvasGroup>();
            if(!group)group=root.AddComponent<CanvasGroup>();
            group.alpha=0;
            group.interactable=false;
            group.blocksRaycasts=false;
        }

        private static GameObject Child(Transform root,params string[] names)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(item=>names.Contains(item.name))?.gameObject;
        }

        private static GameObject Node(string path)
        {
            string[] route=path.Split('/');
            var scene=SceneManager.GetActiveScene();
            var root=scene.GetRootGameObjects().FirstOrDefault(item=>item.name==route[0]);
            if(!root)return null;
            var current=root.transform;
            for(int i=1;i<route.Length && current;i++)current=current.Find(route[i]);
            return current?current.gameObject:null;
        }

        private static T Find<T>() where T:Component=>Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
    }
}


