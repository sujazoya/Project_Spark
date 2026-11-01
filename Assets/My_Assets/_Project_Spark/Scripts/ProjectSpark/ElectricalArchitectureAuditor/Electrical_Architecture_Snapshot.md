# Project Spark — Electrical Architecture Snapshot

**Scope:** Static inspection of the 30 C# files in the user-provided `Runtime.zip` plus 4 newer runtime files from the Circuit Experiment Lab and Advanced Virtual Multimeter add-ons. This is **not** a scan of the full Unity project. These archives primarily contain teaching, telemetry, and instrument layers, not the actual circuit solver.

## Source-grounded capability map

### Core electrical solver and node topology

| Type | Status | Source / note |
|---|---|---|
| `SparkElectricalComponent` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |
| `SparkElectricalState` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |
| `SparkCircuitSolver` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |
| `SparkResistor` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |
| `SparkLED` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |
| `SparkSwitch` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |
| `SparkPowerSupply` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |
| `SparkResistorBurn` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |
| `LevelGamePlayManager` | No declaration in supplied source | May exist elsewhere in Unity Assets or an assembly; cannot establish implementation from this archive. |

### Verified electrical data transport

| Type | Status | Source / note |
|---|---|---|
| `SparkVerifiedElectricalReading` | Declared in supplied source | `Runtime.zip/SparkVerifiedElectricalFrame.cs` |
| `SparkVerifiedElectricalFrame` | Declared in supplied source | `Runtime.zip/SparkVerifiedElectricalFrame.cs` |
| `SparkVerifiedElectricalPublisher` | Declared in supplied source | `Runtime.zip/SparkVerifiedElectricalPublisher.cs` |
| `SparkSolverTelemetrySync` | Declared in supplied source | `Runtime.zip/SparkSolverTelemetrySync.cs` |
| `SparkElectricalTelemetryBridge` | Declared in supplied source | `Runtime.zip/SparkElectricalTelemetryBridge.cs` |
| `SparkMeterVerifiedNodeFeed` | Declared in supplied source | `Supplemental/SparkMeterVerifiedNodeFeed.cs` |

### Workbench instruments

| Type | Status | Source / note |
|---|---|---|
| `SparkCircuitExperimentLab` | Declared in supplied source | `Supplemental/SparkCircuitExperimentLab.cs` |
| `SparkVirtualMultimeter` | Declared in supplied source | `Supplemental/SparkVirtualMultimeter.cs` |
| `SparkMeterProbeTerminal` | Declared in supplied source | `Supplemental/SparkMeterProbeTerminal.cs` |

### AI and teaching electrical consumers

| Type | Status | Source / note |
|---|---|---|
| `SparkAIComponentVisualLessonPlayer` | Declared in supplied source | `Runtime.zip/SparkAIComponentVisualLessonPlayer.cs` |
| `SparkAIGuidedRepairMissions` | Declared in supplied source | `Runtime.zip/SparkAIGuidedRepairMissions.cs` |
| `SparkVisualTeachingManager` | Declared in supplied source | `Runtime.zip/SparkVisualTeachingManager.cs` |
| `SparkSchematicRenderer` | Declared in supplied source | `Runtime.zip/SparkSchematicRenderer.cs` |
| `SparkWorldAnnotationRenderer` | Declared in supplied source | `Runtime.zip/SparkWorldAnnotationRenderer.cs` |

## Confirmed data contract from source

- `SparkVerifiedElectricalPublisher.PublishVerifiedFrame(frame)` emits a **copy** via `FramePublished` and increments `PublishedCount`. It does not itself run a circuit solve (`SparkVerifiedElectricalPublisher.cs`).
- `SparkVerifiedElectricalFrame` contains `circuitId`, `revision`, up to consumer-validated component readings, optional verified conventional-current route, fault, focus, and tutorial action (`SparkVerifiedElectricalFrame.cs`).
- `SparkSolverTelemetrySync` validates 0–32 readings, numerical finiteness, duplicate anchor IDs, positive route integrity, and optional fault/action IDs; it forwards accepted frames to the telemetry bridge. On positive revisions, it rejects non-increasing revisions (`SparkSolverTelemetrySync.cs`).
- `SparkElectricalTelemetryBridge` renders readings, condition labels, and an ordered anchor-arrow approximation of a solver-verified current route. Those arrows are not a reconstruction of actual wire geometry (`SparkElectricalTelemetryBridge.cs`).
- `SparkCircuitExperimentLab` records and compares two verified solver snapshots and evaluator results; it never modifies circuit state (`SparkCircuitExperimentLab.cs`).
- `SparkVirtualMultimeter` can display a signed voltage across one correctly tagged component pair, non-invasive component current, and a modeled unpowered resistance. Arbitrary terminal-to-terminal voltage requires the optional verified node feed (`SparkVirtualMultimeter.cs`).
- `SparkMeterVerifiedNodeFeed` accepts 0–128 unique node potentials, requires a positive `revision`, and expects a common voltage reference and matching component frame circuit/revision (`SparkMeterVerifiedNodeFeed.cs`).

## Critical limitations and next gates

1. **Actual electrical solver source not included.** The supplied archive cannot establish whether the solver uses nodal analysis, graph traversal, Kirchhoff equations, fixed-step simulation, thermal feedback, AC waveforms, etc. References to `SparkResistor`, `SparkLED`, `SparkPowerSupply`, and `LevelGamePlayManager` in AI code do not substitute for their implementation source.
2. **Publishing completion not demonstrated in source.** The publisher and consumers exist; actual completed-solve call sites may be in unuploaded scripts. Run the Editor audit against the full Unity `Assets` folder.
3. **Arbitrary two-node measurement remains optional.** The node-feed contract exists, but source here does not show an authoritative solver publishing actual node potentials.
4. **Oscilloscope readiness unverified.** A time-domain sample source and consistent timestamps have not been demonstrated; do not plot invented waveforms.
5. **Circuit-ID/revision integrity matters.** Change `circuitId` on circuit-layout changes; use strictly increasing positive solve revisions when combining meter node potentials with component frames.

## Exported public declarations (lexical index)

Open `Inspected_Public_API.csv` in this package for a source-path-to-declaration index. Lines are extracted lexically; multi-line signatures may be incomplete.

## Next action in Unity

1. Import only `Editor/SparkElectricalArchitectureAuditor.cs` into any Editor folder.
2. Open your main gameplay scene.
3. Run **Tools → Project Spark → Electrical → Run Architecture Audit**.
4. Share `ProjectSparkReports/ElectricalArchitecture/SparkElectricalArchitectureAudit.md` when you need source-accurate recommendations for solver improvements.
