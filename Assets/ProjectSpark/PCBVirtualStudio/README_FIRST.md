# PROJECT SPARK — AAA VIRTUAL PCB INSPECTION & REPAIR UI

**Delivery:** actual Unity C# world-space Canvas UI, custom UI shader, built-in procedural graphics, and tap/click-driven repair VFX. **No FBX meshes, no physical test-tool models, no premade texture atlas.**

## One-click setup (Unity 2022.3 LTS / Unity 6 recommended)

1. Extract the package and copy its **Assets/ProjectSpark/PCBVirtualStudio** folder into your Unity project's **Assets/ProjectSpark/** folder (keep the Runtime, Editor, Shaders folders).
2. Unity must have the **Unity UI (uGUI)** package and an enabled UI input module. TextMeshPro, Cinemachine and third-party VFX packages are NOT required.
3. Select your existing Project Spark room/table parent GameObject if you want the studio beneath it; otherwise deselect.
4. Run **Tools > Project Spark > PCB Virtual Studio > 1. Build AAA World-Space Studio**.
5. Select the generated **ProjectSpark_AAA_VirtualPCB_Studio** root.
6. Run **Tools > Project Spark > PCB Virtual Studio > 4. Build + Apply AAA Hologram Material** for animated cyan scanlines (optional; UI remains functional with default shader).
7. Press **Play**, select a virtual tool, and **click or touch** one of the 16 demonstration PCB targets.
8. Run **2. Verify Selected Studio** to check UI structure. Optional: **3. Set Selected Studio to Live Mode** blocks demo modifications until a real authority is bound.

### What you get

- A polished 2,200 × 1,240 world-space UI canvas with dark glass-style surfaces, cyan/amber detail lines and responsive electronic readouts.
- 21 virtual inspection/repair tools across **SCAN**, **REPAIR**, **ADVANCED** tabs.
- 16 demonstrator component, trace, via and test-pad targets on a procedural motherboard digital twin.
- Procedural scope/logic/thermal wave visualization, dynamic event console and active tool guidance.
- Cursor/touch-localized VFX: glowing target pulses, solder sparks, fine smoke, scan particles, warm rework effects and diagnostic failure indicators.
- Native Unity editor generation menus, a ShaderLab scanline UI shader and an interface for connecting your existing Project Spark electrical solver.

### Tool set

**SCAN:** Inspect, DMM, Continuity, Oscilloscope, Logic Analyzer, Thermal Camera, Net Tracer, Fault Scanner, Microscope.

**REPAIR:** Flux, Solder, Desolder, Reflow, Hot Air, Trace Patch, Via Repair, Replace Component, Clean.

**ADVANCED:** BGA Reball, UV Solder Mask, Calibrate.

### Basic repair tutorial

**Cold solder joint**: SCAN > Fault Scan > tap **C18** > REPAIR > Flux > tap C18 > Solder or Reflow > tap C18. The contact point emits a warm solder repair VFX and the local demo defect changes to DEMO FIXED. The real electrical solver is **not** modified by this demo action.

**Open via**: REPAIR > Via Repair > tap V07. **Broken trace**: REPAIR > Trace Patch > tap T07. **Solder bridge**: REPAIR > Desolder > tap J4. **BGA**: Flux > Hot Air > BGA Reball > tap U1 on each step.

### Existing 3D PCB integration (without new 3D tool models)

You can keep the already-modeled Level 94+ PCB itself as the interaction surface:

1. Add a suitable Collider to each clickable component/pad/trace object.
2. Add **SparkVirtualPCBTarget** and **SparkPCBPhysicalClickBridge** to each component.
3. Set the bridge's `workstation` to the generated UI root's `SparkVirtualPCBWorkstation` and `target` to the local `SparkVirtualPCBTarget`.
4. Add **PhysicsRaycaster** to your gameplay Camera; ensure the scene has an EventSystem and active UI input module.
5. Assign real target IDs and net IDs in the inspector, matching your gameplay database. Keep demo mode ON only for experimenting; disable it for gameplay with real electrical topology.
6. The same UI VFX appear at the 3D click point (projected onto the world-space Canvas). No soldering-iron FBX is necessary.

### Wiring it to a real Project Spark level

Implement `ISparkPCBRepairAuthority` in a `MonoBehaviour` that talks to your actual `SparkTerminal`/PCB connectivity, diagnostic and fault systems. Call `SparkVirtualPCBWorkstation.BindAuthority(yourAdapter)` (or bind and disable demo in your level builder). The provided **SparkPCBAuthorityAdapterExample** rejects all operations rather than inventing successful electrical repairs.

**Important:** This kit contains a functioning virtual UI demonstration and visual feedback. It does **not** implement an actual electrical solver, component analog physics, production safety certification, or objectives. The demonstration targets and waveform numbers are illustrative. The toolkit NEVER reports a LevelGamePlayManager 6/6 or changes real electrical topology by itself.

### Render pipeline / performance

- All default UI primitives use uGUI; the optional `ProjectSpark/PCB/AAA Hologram UI` ShaderLab unlit transparent shader is render-pipeline-agnostic in design, but still requires a Unity compile check for the project's pipeline.
- No imported textures or 3D effects needed. The effects are lightweight procedural UI particles; max ~80 active at once.
- The UI is created once, not rebuilt each frame. Waveforms refresh at 20 Hz in play mode.
- If no EventSystem exists, the factory prefers `InputSystemUIInputModule` when that package is installed and otherwise creates `StandaloneInputModule`. If an EventSystem already exists it is never replaced; verify its active UI input module matches your project input settings.
- World-space canvas size: 2,200 × 1,240 UI pixels at local scale 0.0022, giving a presentation area around 4.84 × 2.73 Unity units. Reposition/scale the root to fit your table without changing PCB manufacturing dimensions.

See `Documentation/` for workflows and integration requirements.
