# PROJECT SPARK — LEVEL 111: THE SIGNAL CRUCIBLE (SIX-19)
## Advanced AAA game design, electrical integrity and Unity integration specification
**Series:** Computer Motherboard Mastery. **Entry:** Level 110 Power Integrity Specialist has been certified in the existing progression. **Exit:** Signal Integrity Analyst; unlock Level 112 via real progression, not final board-grandmaster certification (Level 120 is reserved).

### Narrative
The lab has successfully stabilized the motherboard power system in Level 110, but its new four-lane diagnostics link cannot deliver reliable data. The scope shows an eye nearly closed at one receiver, the TDR displays a reflected pulse, and clock phase noise grows with heat. The player must build and diagnose a signal-integrity motherboard rather than merely turn green status lights on. Reveal the *cause* of the corruption: board return structure, reference clock quality and active equalizer drift.

### Original board design
**SIX-19**, 330 × 240 mm, XZ/Y-up, 14-layer pedagogical PCB. 138 installable parts: 20 ICs, 32 resistors, 36 capacitors, 12 AC-coupling capacitors, eight ferrites, eight resistor networks, six connectors, four LEDs, four sensors, three switches, two oscillators, two protection diodes, one W185 jumper. 38 structural artworks: board, 14 independently addressable copper-layer visualizations, reference planes, differential lanes, via/test art, fault overlays, silkscreen, retimer cooling and impedance coupon. The completed reference geometry is not spawned in playable mode.

**Layer roles (PROPOSED, NOT FAB-APPROVED):** L01 top mount/signal; L02 ground plane; L03 TX high-speed; L04 ground; L05 clean clock; L06 power plane; L07 RX high-speed; L08 ground; L09 probe/TDR; L10 reference power; L11 equalizer control; L12 ground; L13 low-speed sensors; L14 bottom routing. No copper or via is automatically connected by a layer visual.

**Four lanes:** each lane has differential TX and RX conductor pairs, source and receiver termination, AC coupling, and a retimer/equalizer test path. Educational 10 Gb/s line rate per lane (100 ps UI), differential impedance reference 100 Ω *design goal*. This is not USB/PCIe/Ethernet standards compliance. Use real reviewed geometry and a bounded causal loss/discontinuity model; require positive time-domain and data evidence.

### Mission initial world
- Empty board on EXISTING large Project Spark table; no room, furniture, oscilloscope, solder iron or physical tool models.
- 0/138 installed/verified/soldered, 0/184 approved routes, 0/60 vias, W185 OPEN, six objectives locked, electronics power off.
- Existing shelf/shop selection and universal component terminal/solver authority own all contacts.
- Existing advanced virtual repair UI (world-space Canvas) is the only instrument interface. The new level adds modes for TDR, Eye, BER, differential probe, equalizer calibration, jitter and layer inspection.
- 14 layer inspection images show copper and annotated dielectric stacks **only**; they never grant connectivity. The showcase is disabled and noncollidable.

### Authority stack and schematic gate
Approved component pinout / footprint map → verified solder contact state → layer-aware copper graph / vias → shared solver / signal plant → independent instruments with timestamps → objective evidence → `LevelGamePlayManager` award → world-space UI/VFX.
Source tables contain 184 `UNASSIGNED` routes and 60 `UNASSIGNED` vias. Every route must ultimately resolve exact `{ref, pad, pin, net, differentialPairId, layer, geometry, width, gap, clearance, impedanceModel, sourceOfTruth}`. Every via resolves `{net, startLayer, endLayer, type, plated geometry, reference clearance, actual coordinates}`. The W185 logical U18-to-U19 contract additionally needs physical pads and safe lost-feedback behavior. Prevent all authoritative objective award until approved. Never fake end-to-end signal integrity from decorative geometry.

### Gameplay phase 1 — Precise component placement
Install 138 BOM-matched components, with snap preview, orientation, polarity, value, BGA/QFN/contact checks where applicable and individual solder verification. Hover outlines and click/touch VFX follow existing UX. 20 chips and six connectors require stricter orientation/contact validation; eight termination networks and 12 AC couplers require exact values. Wrong parts produce actual open/short/impedance consequences only after the graph is approved.

### Phase 2 — Build signal paths
Complete all 184 approved selected routes and 60 assigned vias. Use trace width/gap constraints per reviewed differential geometry, layer rules, realistic return reference, clock isolation, via discontinuity and ground stitching. Copper route IDs must remain W001–W184, W185 is a separate removable safety jumper and **must not increase copper route count**. A glowing trace or overlapping visual is not proof of continuity. Review all 14 layer-visual spaces and prevent shorts across power/ground domains.

### Phase 3 — Clock and lane commissioning
Commission verified 3.3 V / 1.8 V or project-approved rails, low-noise reference clock, PLL, retimer, TX/RX termination and genuine link-health feedback. Start with a low educational pattern rate and qualify the 10 Gb/s target under four-lane simultaneous operation. Require fresh PLL lock, jitter and skew data per lane. Built-in diagnostic simulator alone is a tutorial; only actual real approved level topology and adapted electrical plant can unlock objective 3.

### Phase 4 — Measured signal-quality certification
Use virtual TDR to locate discontinuities and approximate effective distance from the configured propagation speed. Use virtual oscilloscope/equalizer tools to measure eye height, eye width, jitter, skew, differential amplitude, equalizer convergence and counted pattern bit errors. For the educational design, proposed initial target limits (to tune/calibrate, NOT physical device specs): per-lane differential impedance 90–110 Ω; eye height ≥310 mV; combined jitter <18 ps; inter-lane skew <25 ps; TDR return-loss metric ≥14 dB; 10^9 simulated/checksummed pattern-bit trial with a bounded deterministic BER *estimator*. Real finite tests must report "0 observed errors" rather than falsely BER=0. Gate with latest accepted instrument snapshots, not green UI.

### Phase 5 — Diagnose three different faults
**F1 Lane 2 return-path microvia open:** physical via edge removed → return impedance mismatch, large reflected pulse, closed eye/BER. Player scans 14 layers, performs TDR, isolates power and executes qualified via repair. Rebuild graph, remeasure TDR and lane data.

**F2 Clock conditioner PLL jitter:** actual reference clock phase noise increases → four lanes degrade together. Virtual differential scope shows phase noise. Replace/calibrate appropriate U5/clock network, then rerun fresh jitter and all-four-lane tests. Never fix by soldering a random capacitor.

**F3 Thermal retimer equalization drift:** loss and adaptation worsen at thermal soak → intermittent eye closure despite near-good ambient measurements. Virtual thermal scanner identifies hot U8/thermal pad. Require safe rework, legitimate component or thermal path repair and full hot-load qualification.

All faults are deterministic by seed/configuration, affect authoritative electrical model and measurement cache, and are repaired through the correct real target. A glowing repair effect cannot certify F1/F2/F3. Historical milestones may remain recorded; operational health must reflect current faults. Objective 5 requires all three independent repair proofs and a fully fresh four-lane test.

### Phase 6 — W185 supervised safety feedback recovery
**Logical** `U18.TRUSTED_LINK_HEALTH_OUT → W185 → U19.TRUSTED_LINK_HEALTH_IN`, specific actual physical pins pending schematic. First install W185 for commissioning. After O1–O5, arm supervised test, physically break W185. Independent safety monitor observes lost valid fresh-frame/heartbeat, latches fault, inhibits protected data transfer. Leave low-power diagnostic telemetry intact; don't teleport rails to zero. Reconnect to approved pads; reconnect alone does not clear latch. Acquire fresh TDR/eye/BER/jitter samples and a complete new four-lane qualification, then explicit user-commanded safety reset, then official Project Spark manager awards O6. Never treat a stuck-high pin as valid dynamic health.

### User-facing telemetry and AAA style
Restrained navy/graphite technical instrument design, physically accurate metallic copper/gold/tin materials, readable tiny printed component refdes, microscope inspection, shallow/contact sparks, subtle holographic cyan lanes and amber critical warnings. UI instrument panels are reusing existing world-space Canvas and work with mouse and touch, no extra standalone 3D tools. Show selected instrument, hardware clock, four per-lane snapshots with timestamps, status of impedance/eye/jitter/BER, 184/60 construction counters and objective evidence. VFX only after accepted transactions. Avoid cartoon glow, giant unrealistic LEDs and immediate fake repairs.

### Production C# integration plan
- `SparkL111FullLayoutBuilder`: editor-only additive source importer; produces native OBJ visuals, 138 uninstalled targets, 184 blocked routes, 60 blocked vias, disabled showroom reference, and separate table preview.
- `SparkL111URPMaterialBuilder`: 22 family URP Lit materials, packed metallic-smoothness, normal, AO maps; keeps source assets intact.
- `SparkL111PlacementSlot`, `SparkL111BlockedTask`: metadata only; NEVER authoritative.
- `SparkL111SignalIntegritySandbox`: bounded reproducible tutorial-output generator, explicitly NOT CERTIFIABLE, no Update loop.
- Later adapters (only after reading actual project): `SparkL111TopologyAuthorityBridge`, `SparkL111FourLaneSignalPlant`, `SparkL111TDRProbe`, `SparkL111EqualizerCalibration`, `SparkL111FaultAuthority`, `SparkL111W185IndependentSafety`, `SparkL111ObjectiveEvidenceBridge`. These names are proposals, NOT installed existing API contracts.

### Production and QA gates
Gate 0 original art geometry/UV/material audit; gate 1 installed art/builder EditMode compilation; gate 2 approved schematic, pins and 14-layer stackup; gate 3 physical placement/route/via graph; gate 4 four-lane signal and calibrated instruments; gate 5 fault repairs; gate 6 W185 independent safety; gate 7 six actual objective awards and mobile/PC performance; gate 8 regression Levels 1–110. No test claims before Unity validation.
