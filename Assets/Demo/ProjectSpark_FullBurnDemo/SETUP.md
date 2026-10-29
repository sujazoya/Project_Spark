# ProjectSpark — Full Burn VFX visual demo

## Required scripts already in your project
- SparkResistorFailureBurstVFX.cs (existing one-shot Sparks, Flash, Debris controller)
- SparkResistorSmokeGraphDriver.cs (existing thermal smoke controller, optional for preview)
- SparkResistorBurn.cs (existing actual thermal/electrical failure)
- VFX_ResistorSmoke.vfx (with an EXPOSED float property named `SmokeRate` and connected to Constant Spawn Rate)

## Scene setup (all in Unity Editor; no runtime GameObject creation)
1. Copy `SparkFullBurnVFXDemo.cs` into a Gameplay/VFX folder (NOT Editor).
2. Select your existing `FailureVFX` child under your resistor prefab.
3. Add the `SparkFullBurnVFXDemo` component.
4. Drag `FailureVFX`'s `SparkResistorFailureBurstVFX` to **Failure Burst**.
5. Drag the existing SmokeVFX object's `VisualEffect` component to **Smoke Visual Effect**.
6. Drag `SparkResistorSmokeGraphDriver` to **Smoke Driver** if installed. This prevents its Update() from fighting the demo over `SmokeRate`.
7. Verify Spark/Flash/Debris are assigned in `SparkResistorFailureBurstVFX`, with Play On Awake disabled.
8. Verify VFX Graph exposed `SmokeRate` default = 0 and its VisualEffect override is also zero.
9. Enter Play Mode. Select the object and choose component context menu **VFX / Play Complete Burn Demo**, or press B with Game view focused.

Sequence: light smoke -> spark/flash/debris burst -> heavy smoke -> fade.

## Important
This is a **visual-only preview**: it does NOT call SparkResistorBurn.SetDissipatedPower(), ResetBurn(), or ApplyElectricalState(). It does not damage the resistor, trigger level failure, or change progress.
For real gameplay, SparkResistorBurn's Failed event triggers SparkResistorFailureBurstVFX automatically; SparkResistorSmokeGraphDriver reads actual TemperatureC and IsFailed to drive smoke. Leave the demo's Play On Start OFF and keyboard shortcut OFF in shipped builds.

If your project uses only the new Input System, the sample's `Input.GetKeyDown` may require Active Input Handling = Both/Old or replacing with your configured input action. The ContextMenu Play Mode trigger still works without keyboard input.
