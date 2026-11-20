using System;
using System.Linq;
using ProjectSpark.SensorMatrix;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed partial class SparkLevel103Process
    {
        [Serializable] private sealed class SavedBoard
        {
            public string design;
            public bool[] placed,soldered,routes,vias,repairs,segmentA,segmentB,flux,hot;
            public int revision,stage,fault,pendingRepair,selectedPart,selectedFootprint,selectedRoute,orientation;
            public bool lossObserved,recoveryReset,latched,diagnosed;
            public SparkLevel103Evidence[] evidence;
            public SparkSensorMatrixAcquisition calibration;
        }

        public void SaveBoard()
        {
            if(!Active)return;
            ReadGraph();
            var state=new SavedBoard {design=design.designRevision,placed=placed.ToArray(),soldered=soldered.ToArray(),
                flux=flux.ToArray(),hot=hot.ToArray(),selectedPart=selectedPart,selectedFootprint=selectedFootprint,selectedRoute=selectedRoute,orientation=orientation,
                routes=Enumerable.Range(0,routes.Length).Select(RouteReady).ToArray(),
                vias=routes.Select(route=>route.via && route.via.IsClosed).ToArray(),
                segmentA=routes.Select(route=>Edge(route.from,route.via?route.viaTop:route.to)).ToArray(),
                segmentB=routes.Select(route=>route.via && Edge(route.viaBottom,route.to)).ToArray(),
                repairs=repairProved.ToArray(),stage=stage,revision=revision,fault=fault,pendingRepair=pendingRepair,diagnosed=diagnosed,lossObserved=lossObserved,
                recoveryReset=recoveryReset,latched=latched,evidence=evidence.ToArray(),calibration=acquisition};
            PlayerPrefs.SetString("ProjectSpark.Level103.Board.v1",JsonUtility.ToJson(state));
            PlayerPrefs.Save();
            notice="Board topology, installed identities, calibration provenance and objective evidence saved.";
        }

        public void LoadBoard()
        {
            if(!Active || supply.IsOutputActive){notice="Isolate power before loading board state.";return;}
            string json=PlayerPrefs.GetString("ProjectSpark.Level103.Board.v1","");
            if(string.IsNullOrWhiteSpace(json)){notice="No Level 103 board save exists.";return;}
            SavedBoard saved;
            try{saved=JsonUtility.FromJson<SavedBoard>(json);}
            catch(Exception){notice="Board save could not be read.";return;}
            if(saved==null || saved.design!=design.designRevision || saved.placed?.Length!=44 || saved.soldered?.Length!=44 ||
                saved.routes?.Length!=59 || saved.vias?.Length!=59 || saved.repairs?.Length!=3 || saved.stage<0 || saved.stage>6 ||
                saved.evidence?.Length!=saved.stage || saved.calibration?.channels?.Length!=4 || saved.selectedPart<0 || saved.selectedPart>=44 || saved.selectedFootprint<0 || saved.selectedFootprint>=44 || saved.selectedRoute<0 || saved.selectedRoute>=59 || saved.orientation<0 || saved.orientation>3 || saved.fault< -1 || saved.fault>2 || saved.pendingRepair< -1 || saved.pendingRepair>2 || saved.calibration.channels.Any(c=>c==null||!double.IsFinite(c.stimulus)||!double.IsFinite(c.low)||!double.IsFinite(c.high)||c.high<=c.low||c.calibrated && (!c.zeroCaptured||c.spanCode-c.zeroCode<100)))
            {notice="Save schema or educational schematic revision does not match.";return;}
            for(int i=0;i<44;i++)
                if(saved.soldered[i]&&!saved.placed[i]){notice="Save has a soldered part without its installed identity.";return;}
            for(int i=0;i<saved.evidence.Length;i++)
                if(saved.evidence[i].objective!=i+1 || string.IsNullOrWhiteSpace(saved.evidence[i].proof))
                {notice="Save contains incomplete objective evidence.";return;}
            // A valid save replays only this board's owned route endpoints.
            ReadGraph();
            foreach(var route in routes)
                foreach(var edge in connections.ToArray())
                    if((edge.A==route.from && (edge.B==route.to || edge.B==route.viaTop) ||
                        edge.B==route.from && (edge.A==route.to || edge.A==route.viaTop) ||
                        route.via && (edge.A==route.viaBottom && edge.B==route.to || edge.B==route.viaBottom && edge.A==route.to)))
                        RemoveWire(edge.Id);
            ReadGraph(); // Remove stale edge snapshots before replaying owned copper.
            placed=saved.placed;soldered=saved.soldered;
            flux=saved.flux?.Length==44?saved.flux:new bool[44];hot=saved.hot?.Length==44?saved.hot:new bool[44];
            selectedPart=saved.selectedPart;selectedFootprint=saved.selectedFootprint;selectedRoute=saved.selectedRoute;orientation=saved.orientation;
            for(int i=0;i<parts.Length;i++)
            {
                parts[i].art.SetActive(placed[i]);
                foreach(var joint in parts[i].joints)joint.SetState(soldered[i]?SparkSwitchState.Closed:SparkSwitchState.Open);
                foreach(var device in parts[i].devices)device.SetElectricalEnabled(soldered[i]);
            }
            var created=new System.Collections.Generic.List<ulong>();
            bool restored=true;
            for(int i=0;i<routes.Length;i++)
            {
                var route=routes[i];
                if(route.via)route.via.SetState(saved.vias[i]?SparkSwitchState.Closed:SparkSwitchState.Open);
                bool firstPresent=saved.segmentA?.Length==59?saved.segmentA[i]:saved.routes[i];
                bool secondPresent=saved.segmentB?.Length==59?saved.segmentB[i]:saved.routes[i];
                if(firstPresent)
                {
                    if(!Connect(route.from,route.via?route.viaTop:route.to,out ulong first)){restored=false;break;}
                    if(first!=0)created.Add(first);
                }
                if(route.via && secondPresent)
                {
                    if(!Connect(route.viaBottom,route.to,out ulong second)){restored=false;break;}
                    if(second!=0)created.Add(second);
                }
            }
            if(!restored)
            {
                foreach(ulong edge in created)RemoveWire(edge);
                foreach(var gate in Gates)loader.ActiveBindings.ClearRuntimeComponentBinding(gate);
                stage=0;evidence.Clear();running=false;
                notice="Saved copper replay was rejected. Replayed connections rolled back; qualification withdrawn.";
                return;
            }
            acquisition=saved.calibration;
            // Older v1 saves predate the explicit mux settling parameter.
            if(!json.Contains("\"muxSettlingSeconds\""))acquisition.muxSettlingSeconds=.0014;
            foreach(var channel in acquisition.channels){channel.status=SensorSampleStatus.Stale;channel.timestamp=-100;}
            acquisition.referenceChecked=false;
            revision=saved.revision+1;stage=saved.stage;repairProved=saved.repairs;
            fault=saved.fault;pendingRepair=saved.pendingRepair;diagnosed=saved.diagnosed;
            opticalFeedback.SetResistance(fault==2?470000:47000);
            lossObserved=saved.lossObserved;recoveryReset=saved.recoveryReset;latched=saved.latched;
            evidence=saved.evidence.ToList();referenceGoodSince=-1;running=false;
            for(int i=0;i<Gates.Length;i++)
            {
                loader.ActiveBindings.ClearRuntimeComponentBinding(Gates[i]);
                if(i<stage)loader.ActiveBindings.SetRuntimeComponentBinding(Gates[i],certification);
            }
            manager.QueueEvaluation();ReadGraph();
            notice="Board restored with bench power OFF. Historical evidence retained; reacquire fresh trusted reference.";
        }
    }
}



