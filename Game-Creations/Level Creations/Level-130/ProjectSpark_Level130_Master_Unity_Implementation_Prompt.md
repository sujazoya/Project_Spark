# Project Spark — Level 130: The Recovery Lab
## MSI PRO H610M-E DDR5 — advanced PC fault diagnosis
This package deliberately reuses the verified Level 129 model and texture bytes for the same physical board. The Level 130 source paths, C# namespace, output paths, mission, diagnostic data and recovery exercise are independent. No manufacturer CAD, boardview, pinout or BIOS binary is supplied. Small parts, pin geometry, copper artwork and repair coordinates are approximations.

### Hardware reference
Public MSI Basic specifications: https://in.msi.com/Motherboard/PRO-H610M-E/Specification . Use the DDR5 profile: LGA1700, H610, two DDR5 DIMMs, PCIe 4.0 x16 and PCIe 3.0 x1, single M.2 Gen3 x4 / SATA slot, four SATA connectors, 24-pin ATX and 8-pin CPU power, gigabit LAN, HDMI/VGA and four debug indicators. PCB 200 x 236.16 mm; thickness 1.6 mm assumed. The source page also contains conflicting older DDR4 text; don't mix revisions. Verify relevant manuals and CPU compatibility before assigning authoritative mappings.

### Narrative
A workshop PC has arrived with successive boot and thermal faults. It looks complete but cannot pass release qualification. The player must isolate one problem at a time, preserve before/after evidence, and perform a final cold-start audit. No actual MSI defect is asserted.

### Mission objectives
- O1 — **Intake and baseline**: Inspect received motherboard; inventory service parts and record initial symptoms.
- O2 — **Power fault isolation**: Diagnose EPS seating; restore protected CPU power with fresh solver evidence.
- O3 — **Firmware recovery**: Diagnose simulated firmware read failure; recover approved virtual firmware and repeat cold initialization.
- O4 — **Memory and storage qualification**: Verify DDR5 training, PCIe and NVMe enumeration, and data integrity after restart.
- O5 — **Thermal feedback repair**: Resolve CPU fan tach feedback fault; verify cooling under bounded load.
- O6 — **Final recovery audit**: Collect fresh power, POST, storage and thermal evidence after a complete cold restart.

### Fault cases
- F1 / EPS_8: Simulated unseated EPS connector. Standby present; CPU power invalid. Evidence: Confirm virtual standby; inspect EPS seating; retest CPU rail after isolated reseat.
- F2 / U_BIOS: Simulated firmware read failure. Power valid; initialization fails before memory training. Evidence: Compare simulated flash-read integrity; restore approved virtual image or replace matching IC; retest cold POST.
- F3 / CPU_FAN1: Simulated CPU fan tach feedback open. POST may succeed; thermal qualification fails. Evidence: Check simulated fan speed and tach feedback; repair target with power isolated; repeat thermal test.

### Integration requirements
- Use SparkSelectionController for selection; cached serialized references and event-driven transitions; no global Find or per-frame searches. Use existing input and repair UI. Reuse the existing multimeter and oscilloscope interface; do not duplicate instruments.
- SparkCircuitSystem, SparkElectricalNetworkGraph, SparkTerminal and existing solver components remain the electrical authority. Inspect actual APIs before adding adapters. DiagnosticTarget transforms are only mesh-centered visual anchors, not pins or net endpoints. Validate revision-specific approved terminals and protected supply models before energizing. No fabricated continuity or voltage readings from art coordinates.
- LevelGamePlayManager owns objectives. Reject stale evidence on topology/service changes; record case ID, target ID, measurement timestamp, topology revision and pass/fail criterion. A successful repair must invalidate earlier qualification. Do not award objectives from showroom or the isolated teaching session.
- Start with factory visuals and empty optional CPU/RAM/NVMe targets. Use approved virtual compatible parts and existing PSU/cooler models; no aftermarket hardware assumed installed. Use safe power isolation for any physical service action. EPS fault uses simulated reseating; the included spare is a header visual, not a real replacement harness. Firmware case uses abstract integrity states; do not flash real hardware or distribute proprietary firmware. Fan test uses a bounded virtual thermal and tach model, not real safety certification.
- Build a six-objective progression with before/after measurement evidence, protected restart, deterministic seeded fault stages, PC/mobile input support, accessible warning text and fresh complete audit. Level 129 and older scenes remain unchanged.
- URP material maps are external 2K PNGs. Keep original hierarchy for selection and batched mesh for preview. Profile desktop and mobile; optional mip-map/texture budgets and LOD authoring remain to be tested.
- New diagnostic anchors and spare prefabs are created by the Level 130 layout builder. SparkL130RecoveryTeachingSession is a standalone non-scoring logic sample, and SparkL130PostTeachingDemo provides a non-authoritative POST abstraction. Neither is a wired gameplay UI or runtime electrical adapter.
- Unity import, compilation, actual Play Mode and all electrical objectives must be verified in the target project. Do not claim official MSI association or manufacturer-exact CAD. Review branding permissions before distribution.
