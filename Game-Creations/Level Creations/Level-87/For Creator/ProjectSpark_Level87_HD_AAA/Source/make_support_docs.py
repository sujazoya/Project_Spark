from pathlib import Path
import json,csv,hashlib
from build_level87 import ROOT,NAMES,P,PIVOTS,ANCHORS,LOGICAL,WIRES,CONFLICTS,core

def put(name,s): (ROOT/name).write_text(s,encoding='utf8')
put('README_Unity.txt',f'''PROJECT SPARK — LEVEL 87: THE GRAVITY ENGINE

ACTUAL 3D ART, not a finished playable Unity level.
86 separate real binary FBX 7400 meshes + 86 matching glTF2 GLB meshes.
1 assembled FBX and GLB alternative reference — do not instantiate with modular set.
68 named mesh-owning ANIM_* pivot parents, 236 visual SOCKET_P### geometry parent nodes.
158 planned W001..W158 conceptual wire specifications and 316 independent logical port identities.
96 reused socket anchors documented; incompatible electrical domains MUST NOT be shorted.
12 procedural material sets, 36 reference PNG maps (UV unwrap and proper shader-channel adaptation still needed).

IMPORT
1. Extract archive and copy Models/ into Assets/ProjectSpark/Level87/Models.
2. Import Textures/ into Assets/ProjectSpark/Level87/Textures. Place optional *Editor.cs in Unity Editor directory.
3. Put SparkLevel87VisualRig.cs outside Editor assemblies, under runtime scripts.
4. Select Level 87 layout root GameObject in Unity Hierarchy.
5. Tools/Project Spark/Levels/87/Place HD Art Under Selected Layout.
6. Tools/Project Spark/Levels/87/Validate Imported HD Art.
7. Optionally Tools/Project Spark/Levels/87/Create & Apply HD Materials To Selected Art.
8. Implement actual electrical, solver, rotor, bearing, safety, UI, objective and persistence systems separately.

SCALE / PIVOTS
FBX numeric geometry and named pivots in CENTIMETRES, FBX UnitScaleFactor=1, GLBs in METRES.
Convert FBX cm -> Unity m exactly ONCE. Y-up, X lateral, Z front. All modular model root transforms share origin.
Ring visual model reference radius = 5.2 m; game-design physical scenario radius = 45 m, a deliberately scaled representation.
Critical player wire W137: L87_PrimaryRadialGapSensor P019 -> L87_MagneticBearingSafetyPLC P135, analog feedback.
It is DESIGN-ONLY until game wiring and compatible SparkTerminal objects are authored.

NO UNITY PLAY MODE, COMPILATION OR REAL SUSTAINED PHYSICAL SIMULATION TESTING WAS PERFORMED.
''')
put('AAA_Art_Delivery_Notes.md', '''# Project Spark — Level 87 THE GRAVITY ENGINE

## Geometry

Original editable triangulated **binary FBX 7400** and **glTF 2 GLB** modular source meshes: curved pressure-ring habitat segments, 12-spoke central assembly, station hub, observation deck, 8 separate electromagnetic bearing coil stations, segmented motor stator/pole meshes, optical displacement instruments, rotor-balancing tools, inverter/PLC panels, safety systems and SCADA visualization. World-baked modular geometry shares one assembly origin. Each module has distinct materials, separate named pivot parent geometry where indicated and decorative gold-rimmed socket nodes.

## Quality scope

This is a **detailed AAA-inspired procedural hard-surface technical-art foundation**, not finished AAA cinematic production geometry. It includes no hand sculpting, UV atlases, baked tangent-space maps or final authored LODs/collision meshes. The 1024 px procedural PNG PBR sets are *reference maps* and may require material UV unwraps, correct normal/tangent generation, and Unity-specific metallic-smoothness/occlusion remapping.

The central visual ring has a 5.2 m radius for room composition. Its proposed physics scenario radius is 45 m; gameplay must explicitly separate model presentation scale and simulation geometry; no direct Unity world-scale orbital physics follows from this showroom mock-up. The rendered ring is not a real astronaut-rated centrifuge.

## Technical boundaries

No `SparkTerminal` components, player-created wires, Unity gravity simulation, drive inverter, magnetic-bearing controller, active safety PLC, torque integrator, six authoritative objectives or actual Play Mode testing exist in this asset kit. Real machine dynamics, contactless power coupling, current/heat, resonance and W137 recovery are implementation specifications only. Do not claim the level is playable based solely on the modular art ZIP.
''')
put('Gameplay_Integration_Plan.md', '''# Level 87 — gameplay and physics implementation plan

**Source baseline:** 86 FBX+86 GLB equipment files, 68 animation pivot nodes, 236 decorative art socket positions, 158 conceptual player wire rows, 316 independently defined logical W endpoints, 6 objective designs. Each implemented system must be inspected against actual Project Spark APIs before any C# hookup.

1. Import modular models once at common origin, under disposable `L87_HD_VISUAL_ART`; never load the assembled showcase alongside them. Verify one conversion from source FBX centimetres to Unity metres.
2. Read existing `SparkElectricalSolver`, `SparkCircuitSystem`, `SparkTerminal`, `SparkLevelDefinition`, `SparkLevelEvaluator`, `LevelGamePlayManager`, object shelf, level loading and repair APIs; adapt to existing signatures, never invent authority methods.
3. Reconcile the 236 modeled art sockets to 316 proposed independent W endpoints. Reused visual sockets are NOT electrically shared unless compatibility and true bus/junction support are proven. Produce functional port subanchors and input/output/domain allocation report.
4. Build 158 actual *player-created* wire requirements; never instantiate them as already connected cables. Some planned power feed rows omit individual returns. Add/validate returns or explicitly rework wiring budget to satisfy the actual solver; do not invent a return through chassis visuals.
5. Implement a separate educational 5 V signal domain, fictional higher-power motor/bearing/rotary plant and independent protection domain. Commands differ from measured physical state; no free energy or instant RPM.
6. Single authoritative physical state: rotor angle, angular velocity, inertia, drive torque, bearing station radial/axial offsets, coil currents, current amplifier capacity and temperature, rotor imbalance and phase, structural modal vibration, touchdown contacts, radial measurements with calibration/timestamps, station reaction torque, friction/loss energy, power transfer/coupling efficiency and essential habitat loads.
7. Fictional normalized dynamics: `I * domega/dt = tau_drive - tau_losses - tau_load`, `E_rotor = .5*I*omega^2`, radial centrifugal acceleration `a=omega^2*r`; use scenario radius=45 m for gameplay UI while visual scale is separate 5.2 m. No direct physical certification claim.
8. Eight independent bearing stations with finite force/current, finite thermal response and genuine gap sensor samples; protective touchdown bearing cannot teleport/instantaneously stop spinning rotor. Magnetic field animations purely reflect actual state.
9. Allow finite motor inverter response, precharge, overspeed protection and station countermoment budget. Active vibration control measures real vibration and rotor phase; balancing mass slides have finite speed/range.
10. Three actual disturbances: primary radial sensor drift, amplifier overheating, asymmetric payload/cargo mass distribution. Causal physical changes, observations, repair and fresh spin qualification required. No objective boolean button shortcuts.
11. W137 is critical trusted radial gap measurement: primary radial gap sensor `P019` → magnetic bearing safety PLC `P135` (art locations only), originally `ANALOG_FEEDBACK`. Actual runtime wire disconnection invalidates samples, restricts normal operation, enters safe response, latches fault. Reconnecting requires new calibration, independent checks and explicit operator reset. Preserve historical Objectives 1–5; Objective 6 requires a fresh complete run ID.
12. Add full SCADA, motion and safety diagnostics, collision/LOD and lightmap production, six authoritative objective evidence records, save/load support and regression tests for Levels 1–86. Optional packaged Unity scripts only place/validate/render art, not the gameplay builder.
13. One-click gameplay builder should be implemented at `Tools/Project Spark/Levels/Create Level 87`, creating real `Level_87.asset`, `Level_87_Layout.prefab`, real gameplay, terminals and objectives. This **does not exist yet** in the package.
''')
put('AAA_Polish_And_Lighting_Plan.md','''# Level 87 AAA-inspired art finishing plan

**Lighting**: physically based broad orbital rim highlights; gold warm coil details vs cyan bearing status; strong figure/ground separation between rotating ring and fixed stator. Keep Earth/star backdrop purely ornamental, not a lighting authority.

**Mesh pass**: production bevel normals, angle-weighted smoothing, high-poly bake on bearing stators and copper windings, material IDs per machine, unique nonrepeating fastener detail, baked trims, hand-crafted assembly decals, close-up readable terminal icons and fault labels.

**Materials**: titanium bearing race (.22 roughness), ceramic pressure hull (.33), copper windings (.24), anodized orange-gold (~.25), coated composite rotor (.61), cyan emissive magnetic status; transparent instrument glass with depth sorting and render pipeline adaptation. Procedural textures supplied are reference maps, not applied professional UV-mapped texture atlases.

**VFX**: animated cyan field arcs, amp-current status emission, rotation guide ticks, synchronized vib-spectra, severe overheat amber/red indicators, controlled touchdown warnings and W137 critical trust-loss display; only driven by authoritative simulation measurements.

**Performance**: segment main ring into static/rotating batching groups; avoid static batching moving rotor art; indirect instancing/merged mesh chunking for repeating radiator tiles and copper coils; LOD0/1/2 for 8 bearing stations and 6 habitat segment panels; occlusion zones for the room; simplified selection/collision geometry; assess triangle/draw-call/memory under target hardware.
''')
plan=[]
for i,n in enumerate(NAMES):
 major=i in [2,3,4,11,12,13,14,16,17,18,19,20,21,22,23,29,32,33]
 plan.append(dict(Model=n,CollisionPlan='Split stator/rotor collision shell with no self-collision of pure display features' if major else 'Convex proxy or simplified primitive hitbox',
 LOD0='Imported original high detail model',LOD1='Production task: remove >50% ornamental triangles',LOD2='Production task: simplified silhouette, static distance fallback',
 StaticBatching='NO - actual motion-bearing visual art' if i in [2,3,5,6,7,8,9,10,11,33,49,50] else 'Consider only for truly stationary structure',
 Status='PLANNED_NOT_GENERATED'))
with open(ROOT/'Collision_LOD_Production_Plan.csv','w',newline='',encoding='utf8') as f:
 w=csv.DictWriter(f,fieldnames=list(plan[0]));w.writeheader();w.writerows(plan)
colors={k:{'baseColorFactor':v,'metallic':k in ['structural','titanium','copper','gold','magnet'], 'type':'procedural preview'} for k,v in core.COLORS.items()}
(ROOT/'Material_Palette.json').write_text(json.dumps({'materials':colors,'shader_targets':['URP Lit','HDRP Lit','Standard'],'production_UV_ready':False},indent=2),encoding='utf8')
print('WROTE supplementary guides and material palette')
