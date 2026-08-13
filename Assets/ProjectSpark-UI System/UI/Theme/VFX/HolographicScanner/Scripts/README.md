# Project Spark — Holographic Scanner Runtime Foundation
Target: Unity 6.3 LTS + URP + Shader Graph + Visual Effect Graph

This package implements the scanner presentation/diagnostics layer shown in the supplied reference image.

Important:
- The electrical simulation remains the source of truth.
- This system observes a scanner data feed; it does not replace BreadboardGrid, CircuitTerminal,
  SignalPath, SignalPathMesh, SignalWireBuilder, SignalFlowVFX, or SignalPath_Manager.
- No Singleton, Service Locator, Event Bus, reflection, or FindObjectOfType is used.
- The scanner is driven by one coroutine/state machine and explicit data feeds.

Recommended scene structure:

ProjectSpark
├── Simulation
│   ├── BreadboardGrid
│   ├── CircuitTerminal ...
│   └── SignalPath_Manager ...
├── HolographicScanner
│   ├── ScannerController
│   ├── ScannerVisuals
│   └── ScannerVFX
└── ScannerCanvas
    ├── StageHeader
    ├── StatusPanel
    ├── MainViewport
    ├── CircuitFlowPanel
    ├── VoltageMapPanel
    ├── FaultPanel
    └── BottomToolbar

Integration:
1. Add ScannerFeed to a scene object.
2. Your simulation adapter calls:
   BeginCapture()
   AddComponent(...)
   AddConnection(...)
   SetNodeVoltage(...)
   EndCapture()
3. ScannerController receives the feed and analyzes it.
4. Bind the supplied UI scripts.
5. Assign VisualEffect and materials to ScannerVisuals.

See the comments in each file for exact setup.
