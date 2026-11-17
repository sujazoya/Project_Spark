# Electrical & Gameplay Integration Contract

## Non-negotiable separation

The delivered world-space UI is an interaction/visual presentation layer. The existing Project Spark PCB/netlist/electrical solver remains authoritative for electrical connectivity, waveforms, faults, measurements, safe power states and `LevelGamePlayManager` objectives. DO NOT mark a net electrically repaired merely by setting the demo target's `demoDefect=None`.

## Live adapter API

Implement `ISparkPCBRepairAuthority` on a MonoBehaviour:

```
bool TryApplyRepair(SparkVirtualPCBTarget target, SparkVirtualTool tool, out string message);
bool TryReadProbe(SparkVirtualPCBTarget target, SparkVirtualTool tool, out string reading);
```

Call `workstation.BindAuthority(adapter)` to enable live mode. `BindAuthority` disables local demonstration mutations. When the adapter rejects an operation, the UI presents a rejection pulse and does not modify the target's authoritative state.

### Mapping target IDs

Map `SparkVirtualPCBTarget.TargetId` and `.NetId` to the level's **real** `SparkTerminal`, PCB pad, via, solder joint, copper trace or installed component object. Use a stable unique ID per target. Never map a decorative pad to an unrelated functional node. If a trace can be open/short, make the solver topology change when the authority commits a repair.

### Example: trace patch

1. User selects TRACE PATCH and taps target `T07`.
2. Adapter validates the target references a broken trace; checks correct layer, repair resources, solder safety and prerequisites.
3. Adapter commits a real change to PCB connectivity, updates circuit solver and reads back continuity.
4. Only if successful, `TryApplyRepair` returns true and `RepairActionAccepted` fires (for analytics or VFX subscribers).
5. The level evaluator waits for *fresh* post-repair measurements before awarding progression. The UI event itself never certifies the mission.

### Example: W035/W049/W059 feedback jumper

These are separate existing gameplay concepts and are NOT automatically placed in this generic studio. Create a `SparkVirtualPCBTarget` bound to the real jumper and direct `ReplaceComponent` / `Solder` / relevant tool to existing physical connection interactions as appropriate. A disconnected jumper must invalidate feedback; do not replace missing measurement with zero. Requires real reconnection, fresh data and explicit safety reset. Keep previous objective certificates historically persistent as the level rules require.

### Instruments

A live multimeter must obtain `Voltage`, current, resistance or continuity from the electrical model, not the target's `demoVoltage`. The oscilloscope needs a time series from the solver; the delivered graph visual is a **demo placeholder** until the UI is bound to real waveform samples. Display a clear provenance label so a player can distinguish measured physical data from estimated, filtered, invalid or illustrative values.

### Authority integration milestones

- M1: stable target IDs and physical raycast mapping.
- M2: real topology change for solder, trace, via, jumper and replace operations.
- M3: actual solver-driven instrument measurement display and UI graph.
- M4: temperature/heat and rework consumable constraints.
- M5: fault injection and evidence-based qualification through existing `LevelGamePlayManager`.

No Unity compile or play-mode verification has been performed in this environment. Run project-side checks before treating the UI as integrated gameplay.
