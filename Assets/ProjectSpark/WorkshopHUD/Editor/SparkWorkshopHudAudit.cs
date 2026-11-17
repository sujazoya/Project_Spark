#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class SparkWorkshopHudAudit
{
    private const string Request="Logs/SparkWorkshopHudAudit.request";
    private const string Report="Logs/SparkWorkshopHudAudit.txt";
    private const string Running="Spark.WorkshopHud.Audit";
    static SparkWorkshopHudAudit()
    {
        EditorApplication.update+=Poll;
        EditorApplication.playModeStateChanged+=Change;
    }
    private static void Poll()
    {
        if(!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().name!="MainMenu") return;
        var activeScene=SceneManager.GetActiveScene();
        if(activeScene.isDirty)
        {
            File.Delete(Request);
            File.WriteAllText(Report,"FAIL: MainMenu has unsaved changes.");
            return;
        }
        File.Delete(Request);
        EditorSceneManager.OpenScene(activeScene.path, OpenSceneMode.Single);
        SessionState.SetBool(Running,true);
        EditorApplication.EnterPlaymode();
    }
    private static async void Change(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Running,false))return;
        if(state==PlayModeStateChange.EnteredEditMode) {SessionState.EraseBool(Running);return;}
        if(state!=PlayModeStateChange.EnteredPlayMode)return;
        bool background=Application.runInBackground;
        Application.runInBackground=true;
        try
        {
            await Task.Delay(1600);
            var dashboard=Object.FindFirstObjectByType<ProjectSpark.UIManagement.SparkUIMonitorDashboard>();
            var manager=ProjectSpark.UIManagement.SparkUIManager.Instance;
            var text=new StringBuilder($"Workshop Play Mode audit {Screen.width}x{Screen.height}\n");
            if(!dashboard || !manager || !ProjectSpark.UIManagement.SparkUIMonitorDashboard.IsConsoleVisible)
                throw new InvalidOperationException($"Master Monitor did not take the initial view. Dashboard={dashboard!=null}, manager={manager!=null}, visible={ProjectSpark.UIManagement.SparkUIMonitorDashboard.IsConsoleVisible}.");
            text.AppendLine($"Registered {manager.Panels.Count} panels; {manager.OpenCount} open at startup.");
            foreach(var p in manager.Panels)text.AppendLine($"REGISTRY {p.Id} | {p.DisplayName} | {p.Layer} | {p.State}");
            File.WriteAllText(Report,text.ToString());
            if(manager.Panels.Count<16)throw new InvalidOperationException("Incomplete native panel discovery.");
            ScreenCapture.CaptureScreenshot("Logs/SparkWorkshopMasterMonitor.png");
            await Task.Delay(300);
            dashboard.SetVisible(false);
            await Task.Delay(250);
            if(!GameObject.Find("ProjectSparkUI/MonitorScreen canvas/Spark Master Monitor Launcher") || manager.OpenCount!=0)
                throw new InvalidOperationException("Monitor launcher absent or a panel opened on the idle worktop.");
            foreach(var graphic in Object.FindObjectsByType<Graphic>(FindObjectsSortMode.None))
            {
                if(!graphic.isActiveAndEnabled || graphic.canvasRenderer.GetInheritedAlpha()<0.05f ||
                    graphic.canvas.renderMode!=RenderMode.ScreenSpaceOverlay)continue;
                var rect=graphic.rectTransform;
                var corners=new Vector3[4];
                rect.GetWorldCorners(corners);
                if(corners[2].y>Screen.height-50 && corners[0].y<Screen.height &&
                    (corners[2].x>Screen.width-100 || corners[0].x<110))
                {
                    var name=graphic.name;
                    for(var t=graphic.transform.parent;t;t=t.parent)name=t.name+"/"+name;
                    text.AppendLine($"CORNER {name}");
                }
            }
            ScreenCapture.CaptureScreenshot("Logs/SparkWorkshopClean.png");
            await Task.Delay(250);
            dashboard.SetVisible(true);
            if(!dashboard.OpenForUse("wires") || !GameObject.Find("Wire Registry Panel") ||
               ProjectSpark.UIManagement.SparkUIMonitorDashboard.IsConsoleVisible)
                throw new InvalidOperationException("Wire Manager did not open through the F8 monitor.");
            if(!dashboard.OpenForUse("meter") || manager.Find("wires").IsVisible || GameObject.Find("Wire Registry Panel"))
                throw new InvalidOperationException("Opening the meter did not close the wire list.");
            var meter=Object.FindFirstObjectByType<ProjectSpark.UI.VisualTeaching.SparkVirtualMultimeter>();
            if(!meter || meter.CurrentDrawerMode!=ProjectSpark.UI.VisualTeaching.SparkVirtualMultimeter.DrawerMode.Expanded)
                throw new InvalidOperationException("Native meter was not expanded.");
            meter.Hide();
            await Task.Delay(350);
            if(manager.Find("meter").IsVisible)throw new InvalidOperationException("Native close did not update the registry.");
            if(!dashboard.OpenForUse("faults") || !manager.Minimize("faults") ||
                Object.FindFirstObjectByType<ProjectSpark.UI.VisualTeaching.SparkWorkbenchFaultInspector>().Expanded)
                throw new InvalidOperationException("Fault Inspector did not minimize.");
            if(!dashboard.OpenForUse("workshop") || !GameObject.Find("Motitor Parent"))
                throw new InvalidOperationException("Workshop Computer did not open.");
            ScreenCapture.CaptureScreenshot("Logs/SparkWorkshopPanelOpen.png");
            await Task.Delay(250);
            manager.Close("workshop");
            if(!dashboard.OpenForUse("active-level"))
                throw new InvalidOperationException("Current level controls are unregistered.");
            if(!dashboard.OpenForUse("shop") ||
                !Object.FindFirstObjectByType<ProjectSpark.UI.ProjectSparkShopRuntime>().transform.Find("Backdrop").gameObject.activeInHierarchy)
                throw new InvalidOperationException("Native shop failed to open.");
            if(!dashboard.OpenForUse("inventory") || manager.Find("shop").IsVisible ||
                !Object.FindFirstObjectByType<ProjectSpark.UI.ProjectSparkInventoryPanel>().transform.Find("Backdrop").gameObject.activeInHierarchy)
                throw new InvalidOperationException("Native inventory failed to replace the shop.");
            manager.CloseAll(true);
            if(manager.Panels.Any(p=>p && p.IsVisible))
                throw new InvalidOperationException("Close All left panels open.");
            text.AppendLine("PASS: clean startup, monitor launcher, panel handoff, exclusive wire/meter focus, native close, minimize, level controls and Close All.");
            string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
            string BoundsOf(RectTransform rect)
            {
                var corners=new Vector3[4];rect.GetWorldCorners(corners);
                var root=rect.GetComponentInParent<Canvas>();
                var camera=root && root.renderMode!=RenderMode.ScreenSpaceOverlay ? root.worldCamera : null;
                var p=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
                var q=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
                return $"{p.x:F0},{p.y:F0} to {q.x:F0},{q.y:F0}";
            }
            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .OrderBy(c=>PathOf(c.transform)))
                text.AppendLine($"CANVAS {(canvas.isActiveAndEnabled?"+":"-")} {canvas.sortingOrder} {canvas.renderMode} {PathOf(canvas.transform)}");
            foreach(var rect in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Where(r=>r.name is "Motitor Parent" or "LiveElectricalTelemetryCard" or "Wire Registry Panel" or "AI Question Panel" or
                    "Panel" or "ExperimentWindow" or "ExpandedPanel" or "ShopQuickAccess" or "MultimeterPanel" or "MultimeterSmallPanel" or
                    "AILearningPanel" or "SparkThreeModeTools" or "HUD_Header" or "HUD_Footer" or "CollapsedTab" or "VoltageMonitor_Panel" or
                    "VoltageDrawer_ToggleTab" or "HUD_ViewControls" or "LevelSelectScreen" or "ASK-AI-Panel" or "WireController" or "Main_HUD" or
                    "Drawer" or "HUD-Panel " or "SparkAIVisualTeachingCanvas")
                .OrderBy(r=>PathOf(r)))
                text.AppendLine($"PANEL {(rect.gameObject.activeInHierarchy?"+":"-")} {BoundsOf(rect)} {PathOf(rect)}");
            foreach(var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
                .Where(b=>b.transform is RectTransform).OrderBy(b=>PathOf(b.transform)).Take(180))
                text.AppendLine($"BUTTON {BoundsOf((RectTransform)button.transform)} {PathOf(button.transform)}");
            File.WriteAllText(Report,text.ToString());

            await Task.Delay(450);
        }
        catch(Exception ex){File.AppendAllText(Report,"\nFAIL "+ex+"\n");}
        finally{Application.runInBackground=background;EditorApplication.ExitPlaymode();}
    }
}
#endif
