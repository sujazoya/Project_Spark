# PROJECT SPARK — LEVEL 112: THE EMC GAUNTLET
## MASTER UNITY IMPLEMENTATION SPECIFICATION — EMC-20

**Status: NEW EDUCATIONAL DESIGN + PROCEDURAL VISUAL SOURCE; not verified Unity gameplay or real EMC compliance.**

### Mission and identity

Level 112 follows Level 111's Signal Crucible and shifts from signal-integrity repair to advanced electromagnetic compatibility (EMC): conducted emissions, near-field radiation, common-mode and differential-mode noise, filtering, shielding seams, grounding, stitching vias, spread-spectrum switching effects, and independent safety authorization. Board-only on existing Project Spark table. Reuse the world-space virtual instrument UI and electronics truth layer; do not add a second room, table, physical oscilloscope or duplicated electrical solver.

Design identity: **The EMC Gauntlet**, board **EMC-20**. PCB **360 x 250 mm**, sixteen visual copper layers; **152** separately installable components, **38** decorative/detail meshes; **208** player route planning records, **68** via planning records; **three faults**, **six objectives**, and **W209**. Source models are **190 OBJ+GLB pairs, plus one assembled reference pair**. No FBX exports are claimed. All art, models, placements and material families were generated for this design and have not been reviewed for manufacturer-accurate footprint design.

### Physical design and world-space presentation

Real physical units are metres and the board plane is Unity XZ, Y up. Preserve the 0.360 x 0.250 m dimensions in authoritative raycast and solver physics. Presentation enlargement is a SEPARATE VISUAL PROXY that does not transform actual circuit distances. Reference assembled board is disabled, nonconductive and cannot install parts or award objectives. Shield cans and ground stitching artwork are not automatically connected to any electrical net. Sixteen copper layers are visualization proposals; thickness and manufacturer-approved stackup must be separately authored before solver use.

### Six objectives (strict acceptance criteria)

1. **Precision EMC Assembly** — 152 exact BOM refs installed with valid footprint, orientation, polarity, solder contacts and verified shield hardware. No passive count-only flag.
2. **16-layer topology construction** — build all 208 reviewed route tasks and all 68 reviewed via tasks while passing actual graph + geometry DRC. W209 is a separate detachable logical signal, not a 209th route or 153rd component.
3. **Power and emissions baseline** — protected power, validated clock and switching sources, calibrated conducted and radiated noise estimates measured over a defined educational frequency range. Separate common-mode and differential-mode currents. Use bounded deterministic educational plant with declared fixture/probe assumptions; do not claim formal CISPR/FCC compliance.
4. **Suppression qualification** — verify source-dependent filter insertion loss, shield bond impedance, return-path continuity, emissions margin and independent packet/eye signal-integrity test evidence. Measurements must be fresh, linked to source revision, netlist and fault epoch.
5. **Three real repairs** — F1 fractured shield-can seam, F2 common-mode choke saturation, F3 open stitching via. Each fault must mutate the authoritative circuit/EMI plant; independent tools must diagnose it and approved rework must change correct physical state. Rerun emissions and link tests.
6. **W209 supervised fault and recovery** — after O1-O5, disconnect physically verified trusted feedback from U21 to U9, demonstrate independent safety latch and workload inhibition, reconnect approved pin mapping, acquire new spectral and suppression measurements, explicit reset, rerun complete power/EMC/data qualification. Only LevelGamePlayManager can authoritatively award 6/6.

### Source file contracts and stop-gates

Use Data/L112_ComponentBOM.csv, L112_ComponentPlacements.csv and L112_ModelManifest.csv. Source model fields are strictly art placement, not validated pins, solder pads or package-level footprints. `L112_CopperRoutes_208_SCHEMATIC_REQUIRED.csv` has **UNASSIGNED** endpoints, nets, unknown layers and dimensions. `L112_ViaTasks_68_SCHEMATIC_REQUIRED.csv` has unknown nets and spans. These must remain **NOT_CERTIFIABLE**. W209 logical source `L112_TrustedEMCMonitor_U21.EMC_QUALIFIED_OUT` and sink `L112_IndependentSafetySupervisor_U9.EMC_QUALIFIED_IN` are design labels only; exact physical pins are *not* verified. Approval requires a signed canonical pin/netlist, footprint IDs, 16-layer stackup, shield bonding/contact map, test-fixture assumptions, and actual Project Spark API integration.

Never invent SparkTerminal package pins, legal copper paths or LevelGamePlayManager methods from naming heuristics. Inspect the actual project code first and document bound signatures before enabling interactive electrical tasks.

### Independent measurement model

Provide a bounded time-and-frequency-domain educational EMC plant. Model switching-harmonic source magnitudes, return impedance and loop areas, common-mode conversion, input noise filters with frequency-dependent loss, choke saturation versus current, capacitor ESR/ESL, shield seam transfer impedance, ground via parasitics and radiated-field proxy. Model conducted voltage at an explicitly simulated measurement-port/LISN-like fixture; model radiated amplitude as an illustrative near/far field proxy with probe orientation and distance parameters. Use consistent physical units (Hz, A, V, ohms, nH, pF, dBµV, dBµV/m), calibrated sampling timestamps, finite detector bandwidth, and measurement confidence bounds. Distinguish a *fictional training threshold* from any regulatory compliance limit. Calculate FFT/spectrum results from plant or validated sampled waveforms; never pass missions from colored charts, non-functional GUI toggles or an illustrative static waveform. CPU and mobile performance: bounded allocations, pooled VFX, no per-frame full-resolution FFT unless necessary.

### Virtual repair and player workflow

Reuse existing Unity world-space tools: multimeter, oscilloscope, spectrum analyzer, near-field probe, current probe, filter analyzer, ground return scan, continuity, layer viewer, thermal scan, solder/desolder, via repair and component replace. Each click/touch resolves source-verified collider IDs/pad/net/layer, honors UI pointer capture, verifies safety and tool eligibility, performs a real authority transaction, then plays local contact VFX. VFX and visible solder are not equivalent to a graph edge. No hot repair of energized rails; allow diagnostic measurements in approved measurement mode.

### Gameplay phases

A. Inspect and locate 152 parts and shield/seam target positions. B. Install and solder correct components. C. Connect only approved 208 copper records + 68 via spans, including shield ground returns. D. Commission input power, EMI sources and monitoring. E. Measure baseline emissions with a source-locked synthetic test chamber, evaluate common-mode choke/ferrite and return paths. F. Diagnose and repair F1/F2/F3 with appropriate virtual instruments and rerun actual qualifications. G. Arm W209 supervised disconnect, trip safety, reconnect, explicitly reset and fully requalify. All progression evidence tied to one current electrical graph epoch and exact test configuration.

### Actual Unity implementation boundaries

The delivered Editor scripts create a valid art import and an **isolated metadata-only layout** with 152 uninstalled slots, 208 blocked routes, 68 blocked vias and disabled show model. The included `SparkL112EMCSpectrumSandbox` is an optional **ILLUSTRATIVE non-certifiable teaching demo**. It has deliberately no integration with SparkCircuitSystem or LevelGamePlayManager; never display it as measured live values. Before making the level playable inspect Project Spark's real `SparkTerminal`, `SparkCircuitSystem`, `SparkElectricalNetworkGraph`, existing shop/placement, solder authority, LevelGamePlayManager, world-space virtual studio, persistence and UI contracts. Only add minimal adapter classes after confirming existing APIs, then implement a single authoritative circuit + emissions state. Preserve Levels 1–111.

### Engineering verification

Use Documentation/L112_AcceptanceTests_100_NOT_RUN.csv. All 100 acceptance tests start NOT_RUN and require date, version, actual, screenshot/log evidence. Separately record art-import success, source-model validation, approved schematic, certified electrical solver, Play Mode evidence and final 6/6. Not enough to pass offline file checks. Require actual Unity compile + Play Mode + regression tests and previous-level checks before declaring production readiness.
