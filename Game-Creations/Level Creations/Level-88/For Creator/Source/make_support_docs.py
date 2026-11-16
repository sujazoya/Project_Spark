#!/usr/bin/env python3
"""Supporting delivery notes and production guidance; no playable ECLSS code."""
from pathlib import Path
import csv,json
from build_level88 import NAMES,P,PIVOTS,WIRES,ANCHORS,LOGICAL,CONFLICTS,core
ROOT=Path(__file__).resolve().parent.parent

def out(fn,txt):(ROOT/fn).write_text(txt,encoding='utf8')
out('README_Unity.txt',f'''PROJECT SPARK LEVEL 88 — THE BREATHING RING — ACTUAL MESH ASSET DELIVERY

Package is original procedural 3D art, NOT a working Unity gameplay level.
Models/: 90 numbered binary FBX 7400 modular mesh scenes + one assembled visual reference.
GLB/: 90 matching glTF GLB scenes + one assembled visual reference.
Textures/: 12 procedural material sets, 36 1024x1024 PNG maps (reference, not UV baked).
Unity_Optional/: 3 Editor scripts + 1 runtime actual-state-only visual-rig script.
CSV/JSON: verified individual model names, common-origin assembly, movable pivots, gold socket anchor plans,
concept-only wires, logical port reconciliation, planned objectives and structural verification.

UNITY QUICK START
1. Copy `Models/*.fbx` (except showcase) into Assets/ProjectSpark/Level88/Models/.
2. Copy Textures/ into Assets/ProjectSpark/Level88/Textures/.
3. Put files ending Editor.cs inside an `Editor` directory; keep SparkLevel88VisualRig.cs OUTSIDE Editor directories.
4. In Unity's FBX model importer, confirm numeric vertex centimetres become scene metres EXACTLY ONCE.
5. Select your Level 88 layout in Hierarchy and run Tools/Project Spark/Levels/88/Place HD Art Under Selected Layout.
6. Run Tools/Project Spark/Levels/88/Create & Apply HD Materials To Selected Art (preview palette only).
7. Run Tools/Project Spark/Levels/88/Validate Imported HD Art. Check 90 models, 72 pivots, 242 art sockets.
8. Use Model_Manifest.json, Gameplay_Wiring_Concept_W001_W164.csv and
   Terminal_Logical_Port_Reconciliation_328.csv to build the REAL gameplay separately.
9. Do NOT place the assembled Level88_Complete_ArtKit_Showcase.fbx next to all 90 modular models: that would double the art.
10. Store all actual SparkTerminals, physics, safety PLC, wiring and objective components OUTSIDE replaceable art root.

All modular pieces authored with common world origin (0,0,0) and no placement transforms.
Axes: Y up, X width, Z forward. GLB geometry metres; FBX numeric coordinates centimetres;
FBX UnitScaleFactor=1 => importer should interpret centimetres; verify geometry bounds after import.

QUALITY STATUS: hard-surface original procedural preview meshes with material assignments and pivots.
Production UVs, hand-authored AAA bakes, custom LODs, collision meshes, custom VFX, authored labels,
shader finishing, Unity compilation, Play Mode and ECLSS physics are NOT included.
''')
out('AAA_Art_Delivery_Notes.md',f'''# Project Spark — Level 88 THE BREATHING RING — AAA-inspired art delivery

## Delivered

- **90** independently importable binary FBX 7400 mesh scenes and **90** matching GLBs
- 1 alternative assembled reference FBX and 1 GLB
- **72** unique visual rig pivot parents with actual mesh-owning children and metres-space origins
- **242** individually modeled gold rim art-only socket anchors
- **164** conceptual source/target player-wire rows, **328** independent proposed logical ends
- **{len(CONFLICTS)}** reused art anchors requiring functional port reconciliation
- 12 preview PBR material sets × 3 texture maps = **36** source PNGs, 1024²
- Four optional Unity art-only C# scripts, manifests, safety sequence and production plans

## Main hero models

- `02_L88_HabitatRingGlassBioDome` — greenhouse structural arches, repeated glazing, foliage, garden modules
- `12_L88_DualAdsorberTowerA`, `13_L88_DualAdsorberTowerB` — twin tall industrial adsorber vessels and diagnostic windows
- `28_L88_O2WaterElectrolysisChamber` — electrolysis showcase chamber, glass observation window, water channels and cell stacks
- `44_L88_HumidityCondensingHeatExchanger` — finned condenser / coolant coil hardware
- `56_L88_VentilationMainCirculationBlower` — detailed real rotor, housing, guard, bearings
- `61_L88_FourZoneMotorizedDamperBank` — four independently animated damper blade pivots
- `66_L88_CabinPressurePrimaryTransducer` → `77_L88_HabitatLifeSupportSafetyPLC` — planned critical W145 link
- `82_L88_HabitatEnvironmentalSCADA`, `83_L88_AtmosphericDigitalFlowHologram` — digital twin display geometry

## Source truth

These are **editable 3D geometry assets**, not a playable level. Socket nodes are artwork, not SparkTerminals;
planned cables aren't connected; holographic flow arrows are indicators, not real flow calculations;
no Unity compile/Play Mode, independent pressure safety, objective certification or full PBR baking was performed.

The modeled greenhouse and atmosphere plant are stylized for visible presentation, not dimensionally qualified spacecraft equipment.
''')
out('Gameplay_Integration_Plan.md','''# Level 88 — Unity Gameplay Integration Requirements (not implemented)

1. Inspect real existing Project Spark API signatures: SparkElectricalSolver, SparkCircuitSystem, SparkElectricalComponent, SparkTerminal, SparkSwitch, SparkLevelDefinition, SparkLevelEvaluator, SparkLevelSceneBindings, SparkObjectShelf, SparkLevelLayoutLoader, LevelGamePlayManager.
2. Preserve Levels 1–87. Build new Level_88.asset and Level_88_Layout.prefab through a separate idempotent Editor gameplay builder (proposed menu `Tools/Project Spark/Levels/Create Level 88`). Optional included art placement is **not** this builder.
3. Create 164 actual player-connectable wire declarations from Gameplay_Wiring_Concept_W001_W164.csv. Do not preconnect puzzle wires or accept decorative cable meshes as topology.
4. Preserve all 242 modeled decorative socket locations and reconcile **328 proposed logical ends**, splitting analog/digital/power/return/safety contacts when a single art socket is reused for incompatible signals. Generate an explicit post-reconciliation report; do not short incompatible types together.
5. Separate 5 V educational control circuits, fictional atmosphere/resource process, and independent safety protection. Player circuit power/status determines permitted commands, while process gas pressure/mass, flows, adsorption, electrolysis, humidity and heat are not simulated as 5 V electrical wire flows.
6. Implement a single authoritative 4-compartment mole/energy state: n(O2), n(CO2), n(N2 or inert), n(H2O vapor), temperature, pressure derived from nRT/V, finite storage, leak loss, flow and gas composition. Distinguish commanded valves from actual measured valve positions.
7. Model oxygen electrolysis with finite water/power, separated protected fictional byproduct, finite storage and regulated actual flow. Model dual CO2 adsorbent beds with capture capacity, cycle switching and regeneration losses, not instantaneous reset.
8. Water loop: condensing thermal exchanger with finite cooling response, condensate reservoir and water-quality gate. Freeze fault affects actual heat exchange and flow restriction; water inventory remains conserved.
9. Ventilation: two fans, duct restrictions and 4 independently actuated dampers. Real gas transfer across compartments needs actual flow/mixing and pressure gradients; all fans off fails required mission utility.
10. Independent safety: pressure relief, emergency isolation, protected reserve, cabin O2 and CO2 limits, high/low pressure faults, failed measurement freshness and explicit reset. Use fictional safely bounded educational thresholds, not real spaceflight safety values.
11. Three disturbances: (A) CO2 adsorption breakthrough and failed cycle switching, (B) condenser freeze reducing condensate recovery and oxygen water reserve, (C) actual gas micro-leak causing falling inventory and finite reserve usage. Changes must be measured and truly repaired; full post-repair operating run required.
12. W145 is primary trusted cabin-pressure feedback: actual circuit topology loss marks pressure sampling invalid, withdraws normal regulation, preserves independent protection and does NOT force real pressure to zero. Reconnect, calibrate, independent checks, explicit reset, and new full 4-compartment endurance run required to award 6/6.
13. Historical objectives 1–5 remain certified during the final deliberate fault; Objective 6 requires evidence with a new post-repair run ID. Real LevelGamePlayManager is the only source of final six objective state.
14. Visual rig is a read-only consumer of authoritative actual mechanism state. Never read displayed SCADA text, light output or pivot transforms to decide physics/safety.
15. Add gameplay, unit/integration tests, scene loading and regression verification, and report Unity test results accurately. No gameplay is included in the current package.
''')
out('AAA_Polish_And_Lighting_Plan.md','''# Level 88 — AAA Production Polish Plan

**Composition:** Premium ECLSS laboratory, left twin CO2 beds, central electrolysis machine and O2 buffer,
right water recovery + blowers, rear garden habitat under arched glass, front PLC/SCADA consoles.
Warm amber cable accent against cyan glass and blue glow; avoid excessive bloom over legible wiring terminals.

**Production normals and UV:** Add proper chamfers and weighted vertex normals, unwrap each numbered FBX,
bake high→low normal/AO, create trim sheets, correctly map roughness as smoothness (one minus roughness)
per URP/HDRP shader. Texture_Manifest.json's R metallic / G roughness / B AO maps are visual references,
not currently UV-authored on meshes. Transparent materials require refraction/depth sorting review.

**Detailed physical polish:** Pressure-vessel nameplates, actual service labels, gas-line color standard,
fastener heads, valve handles, instrument markings, gasket edges, overhead pipe anchors, accurate cable
harness routes, internal transparent water tubes, textured foliage, gas-flow LED rails, rotating blower hubs.

**VFX:** Process-reactor bubbles, cyclical adsorber indicator gradients, pressure trend strips,
steam/condensate effects, duct directional current, warning beacons and W145 safety alarm. Bind only to
actual values from the Unity authority; do not pretend blue hologram geometry alone proves operation.

**Optimization:** For 90 modular pieces, mesh combine static floor/piping only, instancing for repeated
coils, cartridges, vents and garden foliage; do not static-batch animation pivot children; create
LOD0/1/2 and simple proxy colliders before shipping. Author occlusion zones and lighting reflections
suited to the user's actual rendering pipeline and target platform.
''')
rows=[]
for i,n in enumerate(NAMES):
 rows.append(dict(Model=n,CollisionPlan='Simplified primitive shell; avoid mesh colliders on decorative fine features' if i in (2,12,13,28,44,56,57,60,61,82) else 'Simple convex/box/cylinder interaction proxy',
  LOD0='Current original modular source art',LOD1='Production TODO remove small fasteners/panels approximately 50 percent',LOD2='Production TODO silhouette and combined low-detail material',
  StaticBatching='NO moving visual pivot asset' if any(p['Index']==i and p['Kind'] in ('rotation','translation') for p in PIVOTS) else 'Possible if genuinely static',
  Status='PLANNED_NOT_YET_GENERATED'))
with (ROOT/'Collision_LOD_Production_Plan.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
colors={k:{'baseColorRGBA':v,'preview_metallic':.85 if k in ('titanium','gold','copper','structural') else .12,'preview_roughness':.26 if k in ('gold','glass','titanium') else .50} for k,v in core.COLORS.items()}
(ROOT/'Material_Palette.json').write_text(json.dumps({'source': 'Original procedural Level 88 art palette','materials':colors,'uv_authored':False,'production_bakes_included':False,'shader_targets':['URP Lit','HDRP Lit','Standard']},indent=2),encoding='utf8')
process_links=[
('Habitat Compartment A','Main Airflow Duct','Gas flow / loop return','Mass balance and fan pressure'),
('Habitat Compartment B','Main Airflow Duct','Gas flow / loop return','Mass balance and damper position'),
('Habitat Compartment C','Main Airflow Duct','Gas flow / loop return','Mass balance and damper position'),
('Habitat Compartment D','Main Airflow Duct','Gas flow / loop return','Mass balance and damper position'),
('Air Duct','Adsorber Tower A','Mixed cabin gas','Bed capacity and actual inlet valve'),
('Air Duct','Adsorber Tower B','Mixed cabin gas','Bed capacity and actual inlet valve'),
('Adsorber Towers','Scrubbed Airflow Return','CO2-depleted gas','Regeneration cycle and capture efficiency'),
('Water Recovery','Finite Clean Water Reservoir','Condensed water','Condensation and water quality'),
('Water Reservoir','Oxygen Electrolyzer','Clean water flow','Resource consumption and pump response'),
('Oxygen Electrolyzer','Oxygen Buffer Tank','Separated oxygen','Finite power, water, and process qualification'),
('Oxygen Buffer Tank','Cabin Gas Regulation','Regulated oxygen','Actual valve, fresh pressure and safety authorization'),
('Habitat Compartments','Environment / Micro-Leak','Gas inventory loss','Actual leak state and compartment pressure'),
('Safety Relief Valve','Environment','Protected pressure release','Independent trip and actual valve dynamics'),
]
with (ROOT/'Physical_Process_Flow_Concept.csv').open('w',newline='',encoding='utf8') as f:
 w=csv.writer(f);w.writerow(['From','To','PhysicalFlowType','RequiredAuthority']);w.writerows(process_links)
# Distinguish process flows from the actual electrical player connection plan.
out('Visual_Rig_Integration_Notes.md','''# SparkLevel88VisualRig — verified supplied art-only API

`ProjectSpark.Visuals.SparkLevel88VisualRig` public methods:

```csharp
BindVisuals();
SetActualCO2Processing(float actualBedASaturation01,float actualBedBSaturation01,float actualValveAngleDegrees);
SetActualOxygenProcess(float actualDeliveryFlow01,float actualRegulatorAngleDegrees);
SetActualWaterAndHumidity(float actualWaterPumpSpeed01,float actualCondenserFlow01,float actualFilterDrawerOpen01);
SetActualVentilation(float actualMainFanSpeed01,float actualBackupFanSpeed01,
    float actualDamperAOpen01,float actualDamperBOpen01,float actualDamperCOpen01,float actualDamperDOpen01);
SetActualSafety(bool actualW145TrustedAndFresh,bool actualCriticalFaultLatched,bool actualAtmosphereQualified,
    bool actualLowOxygen,bool actualAirlockOpen,float actualPressureRegulatorDegrees);
```

This component never computes physics, electrical continuity or certification; pass measured real gameplay authority values after separate simulation. It uses 72 authored pivot names. Indicator pivots may remain static and need separately authored screen shader/VFX during AAA finish.
''')
print('SUPPORT DOCS GENERATED',len(rows),'collision/LOD plans',len(process_links),'physical flow paths')
