# Level 129 — Master Unity implementation specification
## Mission: The Silicon Workshop (MSI PRO H610M-E DDR5)
Implement the six objectives using existing Project Spark authoritative systems. Do not replace working levels or create fake electrical completion. The level visual kit is a populated factory board, not an empty bare-PCB level. No proprietary BIOS, PCB netlist or manufacturer package pinout is supplied.

### Hardware profile
Use the public DDR5 version (not PRO H610M-E DDR4): Intel H610, LGA1700, two DDR5 DIMM sockets, PCIe 4.0 x16 and PCIe 3.0 x1, one M.2 PCIe 3.0 x4 / SATA slot, four SATA ports, ATX 24-pin/EPS 8-pin, Realtek gigabit LAN and audio, HDMI/VGA, USB, fan/front-panel/TPM headers, four EZ Debug indicators. Board footprint 200 x 236.16 mm. Actual PCB stackup unknown. Use CPU support list and manual for the revision being modeled. Do not assume DDR4 compatibility or manufacturer exact traces. Optional sample models are unverified fit proxies.

### Authority and architecture
- SparkSelectionController owns selection; use existing selection, rotate, move and measurement tools. Do not duplicate input handlers or use global Find() searches.
- SparkCircuitSystem, SparkElectricalNetworkGraph, SparkTerminal and existing solver definitions own electrical truth. Add typed adapters only after inspecting the real project APIs. No invented electrical terminals from decorative geometry. Keep all unverified pin mappings blocked.
- LevelGamePlayManager owns objective state. The visual prefab and SparkL129PostTeachingDemo are non-authoritative and cannot award objectives. Use events, cache references, PC/mobile input, deterministic replay, bounded simulation and pooled telemetry. Never mutate prior level assets.
- Build a revision-specific pin/footprint/rail manifest from approved sources; use unique IDs and validate every mapping. Gate enable only when actual topology, current limits and fresh solver result are valid. Simulate motherboard behavior at subsystem level rather than transistor-level CPU, DDR or proprietary BIOS. Do not energize during component removal. Rear panel and cosmetic copper are not circuit nets.
- Create runtime service placement validation with orientation, seating and compatible part checks; sample CPU, DDR5, NVMe and retaining hardware included. Add separately approved cooler/fan and PSU profiles from existing project assets. Support touch and mouse, undo, safe removal, explicit confirm, meaningful warning states. Use repair inventory and actual diagnosed target IDs.
- Use virtual instrumentation through existing world-space repair UI; do not create standalone instruments duplicating Project Spark. POST indicators are CPU/DRAM/VGA/BOOT state machines, not invented numeric MSI codes. Drive actual debug indicators only from authoritative state. Render nearby details using URP Lit, correct sRGB/linear maps and reasonable lighting. Batch showcase only; use modular renderer hierarchy for service. Add LODs and texture budget after profiler evidence. Ensure no cross-level static references or stale completion.

### Objectives
O1 — Service assembly: Fit compatible virtual CPU, cooler, DDR5 memory and optional NVMe; verify orientation and mounting.
O2 — Power diagnostics: Qualify ATX standby, CPU power and protected rails with solver-backed measurements.
O3 — POST and memory: Validate simulated CPU and DDR5 initialization; diagnose EZ Debug indicators.
O4 — Interface qualification: Exercise PCIe, M.2, SATA, HDMI/VGA, USB and LAN using bounded device abstractions.
O5 — Component repair: Resolve three independent hardware faults; require measurement evidence and approved repair targets.
O6 — Thermal and reboot: Validate heatsink, cooling and protected restart from a fresh state.

### Fault plan
F1 / PQ04: Illustrative VRM switching path failure; CPU rail invalid; POST CPU indicator; verify Independent rail measurements before and after approved simulated replacement.
F2 / DDR5_2: Illustrative DIMM contact discontinuity; Memory initialization failure; verify Contact continuity, compatible module, fresh memory test.
F3 / M2_1: Illustrative M.2 connector intermittent connection; Boot device detection failure; verify Fresh storage enumeration and read/write integrity test.

### Acceptance and release
Require fresh before/after repair evidence, reset invalidation on topology changes, power-off during repair, invalid/missing terminal rejection, valid supply and POST sequencing, memory mismatch rejection, independent storage verification, shader/LOD/camera/input/device performance tests and safe restart. Never pass objectives by spawning the assembled visual reference or using demo bools. Distinguish offline geometry checks from executed Unity tests. No real hardware safety guarantee. Follow original manufacturer documentation for real PC servicing. Do not claim official MSI association. Compile and test in Unity 6.3 URP, build PC and mobile test targets and record actual results.
