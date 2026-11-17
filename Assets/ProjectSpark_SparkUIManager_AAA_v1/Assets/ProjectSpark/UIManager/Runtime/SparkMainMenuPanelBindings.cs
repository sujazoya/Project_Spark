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
            if(workshop && workshop.TryGetComponent<Canvas>(out var monitorCanvas))
                dashboard.AttachToWorkshopMonitor(monitorCanvas);

            var computer=Node("ProjectSparkUI/MonitorScreen canvas/Motitor Parent");
            RegisterObject("workshop","Workshop Computer",SparkUILayer.Main,computer,"workshop");

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
            if(voltage)
                Native("voltage","Voltage Monitor",SparkUILayer.Tool,visible=>{if(visible)voltage.Open();else voltage.Close();},
                    ()=>voltage && voltage.IsOpen);
            MaskLauncher(Node("Spark_VoltageMonitor_Canvas/VoltageMonitor_Panel/VoltageDrawer_ToggleTab"));

            var experiments=Find<SparkCircuitExperimentLab>();
            var experimentWindow=Node("SparkCircuitExperimentLabCanvas/ExperimentWindow");
            if(experiments && experimentWindow)
                Native("experiments","Circuit Experiment Lab",SparkUILayer.Tool,visible=>{if(visible)experiments.Show();else experiments.Hide();},
                    ()=>experimentWindow && experimentWindow.activeSelf);

            MaskLauncher(Node("SparkCircuitExperimentLabCanvas/OpenExperimentLab"));
            var assistant=Find<SparkAIUnifiedAssistant>();
            var conversation=Node("SparkAIConversation_UI");
            if(assistant && conversation)
                Native("assistant","Ask Spark",SparkUILayer.Main,visible=>{if(visible)assistant.Show();else assistant.Hide();},
                    ()=>conversation && conversation.activeSelf);

            RegisterCanvas("live-coach","Live Circuit Coach",SparkUILayer.Overlay,
                Node("ProjectSpark_Gameplay/Spark AI/AI_TutorialSystem/Spark_AI_LiveCircuitHUD"),"workshop");
            RegisterObject("quick-ai","Quick AI Question",SparkUILayer.Overlay,
                Node("ProjectSpark_Gameplay/Spark AI/Canvas/AI Question Panel"),"workshop");
            RegisterCanvas("learning","Learning Library",SparkUILayer.Main,Node("AILearningPanel"),"workshop");
            RegisterObject("whiteboard","Teaching Whiteboard",SparkUILayer.Main,
                Node("SparkAIVisualTeachingCanvas/WhiteboardPanel"),"workshop");
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
            RegisterObject("recycle","Component Recycling",SparkUILayer.Tool,Node("SparkShelf_RecycleCanvas"),"workshop");

            RegisterObject("tools","Workshop Tool Rail",SparkUILayer.HUD,Node(Tools+"Tools-Panel"));
            RegisterObject("measurement","Geometry Measurement",SparkUILayer.Tool,Node(Tools+"MeasurementHUD"),"workshop");
            RegisterObject("view-tools","View and Inspection Tools",SparkUILayer.Tool,Node(Tools+"HUD_ViewControls"),"workshop");

            var camera=Find<SparkGameCameraRig>();
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

            MaskLauncher(Node(Tools+"SparkThreeModeTools/ModeToggle"));
            MaskLauncher(Node("ProjectSparkUI/Main_HUD/ShopQuickAccess/DrawerToggle"));
            MaskLauncher(Node("ProjectSparkUI/Main_HUD/TutorialHubRoot/Drawer/DrawerToggle"));
            MaskLauncher(Node("SparkAIVisualTeachingCanvas/OpenVisualTeacherButton"));
            MaskLauncher(Node("SparkAIVisualTeachingCanvas/SparkAdaptiveExplanationRoot/SparkAINaturalExplanationRoot/AskAILauncher"));
            MaskLauncher(Node(Tools+"Level-7"));

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
            dashboard.SetVisible(true);
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
            var canvas=Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .FirstOrDefault(candidate=>candidate.name is "Board Foundry controls" or "Pulse Architect controls");
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
            rect.anchoredPosition=new Vector2(-x,-y);
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
