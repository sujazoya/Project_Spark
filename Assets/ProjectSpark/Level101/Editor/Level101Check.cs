#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProjectSpark.Gameplay;
using ProjectSpark.Tools;
using AAAUI.VFX;
using ProjectSpark.Cameras;
using Unity.Cinemachine;
using ProjectSpark.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class Level101Check
{
    private const string Request="Logs/SparkLevel101Check.request", Report="Logs/SparkLevel101Check.txt", Key="Spark.Level101.Test";
    static Level101Check()
    {
        EditorApplication.update+=Poll;
        EditorApplication.playModeStateChanged+=PlayChanged;
    }

    private static void Poll()
    {
        if(!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
            EditorApplication.isUpdating || SceneManager.GetActiveScene().name!="MainMenu")return;
        File.Delete(Request);
        File.WriteAllText(Report,"Level 101 — The Board Foundry\n");
        try
        {
            SparkLevel101Builder.Create();
            SessionState.SetBool(Key,true);
            EditorApplication.EnterPlaymode();
        }
        catch(Exception ex){Record("FAIL: "+ex);}
    }

    private static async void PlayChanged(PlayModeStateChange state)
    {
        if(!SessionState.GetBool(Key,false))return;
        if(state==PlayModeStateChange.EnteredEditMode){SessionState.EraseBool(Key);Record("Returned to Edit Mode.");}
        if(state!=PlayModeStateChange.EnteredPlayMode)return;
        var logFilter=Debug.unityLogger.filterLogType;
        var background=Application.runInBackground;
        Debug.unityLogger.filterLogType=LogType.Error;
        Application.runInBackground=true;
        try
        {
            var manager=Object.FindFirstObjectByType<LevelGamePlayManager>();
            var s=new SerializedObject(manager);
            s.FindProperty("enforceProgressionAccess").boolValue=false;
            s.ApplyModifiedPropertiesWithoutUndo();
            if(manager.LevelProgression)manager.LevelProgression.enabled=false;
            var levels=s.FindProperty("levels");
            int index=Enumerable.Range(0,levels.arraySize).Single(i=>
                levels.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition l && l.LevelId=="level_101");
            if(!manager.StartLevel(index))throw new InvalidOperationException("Cannot open Level 101.");
            await Task.Delay(1800);
            var pcb=Object.FindFirstObjectByType<SparkLevel101Process>();
            var modeReadyBy=DateTime.UtcNow.AddSeconds(10);
            while(pcb && manager.SatisfiedTargetCount!=2 && DateTime.UtcNow<modeReadyBy)
                await Task.Delay(100);
            if(!pcb || pcb.parts.Length!=27 || pcb.parts.Any(p=>!p.activeInHierarchy) ||
                pcb.routes.Length!=35 || manager.ActiveTargetCount!=6 || manager.SatisfiedTargetCount!=2)
                throw new InvalidOperationException($"Normal start: parts {pcb?.parts.Count(p=>p.activeInHierarchy)}/27, targets {manager.SatisfiedTargetCount}/{manager.ActiveTargetCount}, HUD {pcb?.heading.text}, evaluator {manager.LastEvaluationMessage}");
            Record("Normal Mode opened at 2/6 with all 27 components visible and all copper routes still unwired.");
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel101Opening.png");
            await Task.Delay(500);

            async Task Wait(Func<bool> condition,string title)
            {
                var until=DateTime.UtcNow.AddSeconds(14);
                while(!condition())
                {
                    if(DateTime.UtcNow>=until || !EditorApplication.isPlaying)
                        throw new InvalidOperationException(title+" | "+pcb.heading.text+" | "+pcb.detail.text+" | "+pcb.readings.text+" | "+manager.LastEvaluationMessage);
                    await Task.Delay(50);
                }
            }
            var rig=Camera.main.GetComponent<SparkGameCameraRig>();
            var brain=Camera.main.GetComponent<CinemachineBrain>();
            if(!rig || !brain) throw new InvalidOperationException("No shared camera rig on the gameplay camera.");

            async Task CheckCameraHandoff(OutputChannels channel)
            {
                await Wait(()=>brain.enabled && brain.ChannelMask==channel && !rig.OwnsCamera,
                    "Expected dedicated level camera " + channel);
                var blend=brain.DefaultBlend;
                var lens=brain.LensModeOverride;
                var update=brain.UpdateMethod;
                rig.SelectView(2);
                await Wait(()=>rig.OwnsCamera && brain.ChannelMask==OutputChannels.Channel14 &&
                    rig.SelectedView==2 && !Camera.main.orthographic,"Expected shared Detail view");
                rig.ReturnToLevelView();
                await Wait(()=>!rig.OwnsCamera && brain.ChannelMask==channel && Camera.main.orthographic,
                    "Expected restored level camera " + channel);
                if(!brain.DefaultBlend.Equals(blend) || !brain.LensModeOverride.Equals(lens) || brain.UpdateMethod!=update)
                    throw new InvalidOperationException("Camera handoff changed level lens, update or blend settings.");
                Record("Shared Detail view returned to the correct level camera ("+channel+").");
            }

            await CheckCameraHandoff(OutputChannels.Channel12);
            pcb.BoardView();
            await Task.Delay(550);
            Task At(int step)=>Wait(()=>manager.SatisfiedTargetCount==step,$"Expected {step}/6");
            var modeButton=pcb.heading.transform.parent.Find("Challenge Mode").GetComponent<UnityEngine.UI.Button>();
            var toolForStart=Object.FindFirstObjectByType<SparkWireTool>(FindObjectsInactive.Include);
            var firstPad=pcb.routes[0].from;
            Vector2 screenPoint=Camera.main.WorldToScreenPoint(firstPad.transform.position);
            if(!toolForStart.TryResolveTerminalTarget(screenPoint,out var normalHit) ||
                normalHit.Collider.GetComponent<SparkTerminal>()!=firstPad)
                throw new InvalidOperationException("W001 is unavailable at the start of Normal Mode.");
            Record("Normal Mode can pick W001 immediately.");
            modeButton.onClick.Invoke();
            await At(0);
            if(pcb.parts.Any(p=>p.activeSelf) || toolForStart.TryResolveTerminalTarget(screenPoint,out _))
                throw new InvalidOperationException("Challenge Mode did not hide components and gate wiring.");
            Record("Challenge Mode opened at 0/6 with all 27 parts hidden and wiring gated.");
            modeButton.onClick.Invoke();
            await At(2);
            if(pcb.parts.Any(p=>!p.activeInHierarchy))throw new InvalidOperationException("Disabling Challenge Mode did not restore parts.");
            modeButton.onClick.Invoke();
            await At(0);
            for(int i=0;i<34;i++)pcb.InspectNet();
            await At(1);
            for(int i=0;i<27;i++)
            {
                for(int turn=i%4;turn>0&&turn<4;turn++)pcb.RotatePart();
                pcb.FitPart();
            }
            await At(2);
            var builder=Object.FindFirstObjectByType<SignalWireBuilder>();
            var tool=Object.FindFirstObjectByType<SparkWireTool>(FindObjectsInactive.Include);
            var camera=Camera.main;
            if(!tool || !builder || !camera) throw new InvalidOperationException("Missing shared wire tool or picking camera.");

            void Drag(SparkTerminal from, SparkTerminal to)
            {
                Vector2 start=camera.WorldToScreenPoint(from.transform.position);
                Vector2 end=camera.WorldToScreenPoint(to.transform.position);
                if(!tool.TryResolveCustomTarget(start,out var hit) || hit.Collider.GetComponent<SparkTerminal>()!=from)
                    throw new InvalidOperationException("Could not pick " + from.name);
                if(!builder.BeginWire(from, WirePolarity.Neutral))
                    throw new InvalidOperationException("Could not start wire at " + from.name);
                var activeChannel=brain.ChannelMask;
                rig.SelectView(4);
                rig.ReturnToLevelView();
                if(brain.ChannelMask!=activeChannel)
                    throw new InvalidOperationException("Camera switched during an active wire drag.");
                builder.UpdateWireFromScreenPosition(end);
                if(!builder.TryEndWire(end))
                    throw new InvalidOperationException($"Could not release wire on {to.name} from {from.name}");
            }

            for(int i=0;i<34;i++)
            {
                var item=pcb.routes[i];
                pcb.FitVia();
                if(item.via)
                {
                    Drag(item.from,item.viaTop);
                    Drag(item.viaBottom,item.to);
                }
                else
                    Drag(item.from,item.to);
                await Task.Yield();
            }
            await At(3);
            Record("All 34 PCB routes were drawn using screen coordinates and the shared wire tool. Both segments across each of the six vias counted.");
            Drag(pcb.routes[34].from,pcb.routes[34].to);
            pcb.TogglePower();
            await Task.Delay(500);
            var result=manager.ElectricalSolver.LatestSolveResult;
            foreach(var terminal in new[]{pcb.certification.PositiveTerminal,pcb.supply.PositiveTerminal,pcb.routes[0].to,pcb.routes[1].from,pcb.protectedInput,pcb.rail,pcb.feedbackInput,pcb.shunt})
            {
                bool hasVoltage=result!=null && result.TryGetVoltage(terminal,pcb.supplyReturn,out _);
                float value=0;
                if(hasVoltage) result.TryGetVoltage(terminal,pcb.supplyReturn,out value);
                Record($"{terminal.name}: {hasVoltage} {value:F5}V; enabled={terminal.IsElectricalEnabled}");
            }
            foreach(var source in new[]{pcb.certification,pcb.supply,pcb.regulator,pcb.feedback})
                Record($"{source.name}: active={source.IsOutputActive}, overload={source.IsOverloaded}, volts={source.ElectricalState.Voltage:F5}, amps={source.ElectricalState.Current:F6}");
            await Wait(()=>pcb.readings.text.Contains("W035 TRUSTED"),"Expected qualified power and W035 feedback");
            pcb.TestBoard();
            await At(4);
            Record("Board commissioned after a live shunt sample.");
            foreach(var fault in new Action[]{pcb.InjectVia,pcb.InjectPullup,pcb.InjectBridge})
                        {
                fault();
                await Wait(()=>pcb.detail.text.Contains("Fault injected"), "Expected fault to become active");
                pcb.Measure();
                pcb.Repair();
                await Wait(()=>pcb.detail.text.Contains("Repair applied") && pcb.readings.text.Contains("W035 TRUSTED"),
                    "Expected repaired board and settled feedback");
            }
            pcb.TestBoard();
            await At(5);
            pcb.ArmJumper();
            pcb.UndoCopper();
            await Wait(()=>pcb.detail.text.Contains("Feedback invalid"),"W035 graph opened and load inhibited");
            if(manager.SatisfiedTargetCount!=5 || manager.IsCompleted)throw new InvalidOperationException("Historical stages were not preserved.");
            Drag(pcb.routes[34].from,pcb.routes[34].to);
            await Wait(()=>pcb.readings.text.Contains("W035 TRUSTED"),"W035 physically restored");
            pcb.Measure();pcb.ResetSafety();pcb.TestBoard();
            await At(6);
            await Wait(()=>manager.IsCompleted,"Expected campaign completion");
            Record("PASS 6/6 after manual PCB wiring, three repairs, and live W035 removal and manual reconnection.");
            pcb.Showcase();
            await Task.Delay(800);
            ScreenCapture.CaptureScreenshot("Logs/SparkLevel101Complete.png");
            await Task.Delay(500);
            int index88=Enumerable.Range(0,levels.arraySize).Single(i=>
                levels.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition l && l.LevelId=="level_88");
            if(!manager.StartLevel(index88))throw new InvalidOperationException("Cannot switch to Level 88.");
            await Task.Delay(850);
            var atmosphere=Object.FindFirstObjectByType<SparkLevel88Process>();
            if(!atmosphere || builder.TerminalPickerOverride!=null)
                throw new InvalidOperationException("The PCB picker survived a level switch.");
            await CheckCameraHandoff(OutputChannels.Channel13);
            atmosphere.SocketView();
            await Task.Delay(800);
            Drag(atmosphere.pins[0],atmosphere.pins[1]);
            Record("Level 88 used the shared picker to draw a wire after the camera switch.");

            int index87=Enumerable.Range(0,levels.arraySize).Single(i=>
                levels.GetArrayElementAtIndex(i).objectReferenceValue is SparkLevelDefinition l && l.LevelId=="level_87");
            if(!manager.StartLevel(index87))throw new InvalidOperationException("Cannot switch to Level 87.");
            await CheckCameraHandoff(OutputChannels.Channel15);
        }
        catch(Exception ex){Record("FAIL: "+ex);}
        finally
        {
            Application.runInBackground=background;
            Debug.unityLogger.filterLogType=logFilter;
            EditorApplication.ExitPlaymode();
        }
    }

    private static void Record(string message)=>File.AppendAllText(Report,message+"\n");
}
#endif
