# Level 54 Visual Art — Animation Setup

This is a **visual-only** asset package. The physics and electrical circuit must use the real Project Spark gameplay components; these art scripts do not create a playable Level 54.

## Installation

1. Import the `Models/*.fbx` files into a Unity Assets folder; keep the individual prefabs, not only `Level54_Complete_ArtKit_Showcase.fbx`.
2. Copy `SparkLevel54ArtPlacementEditor.cs` into `Assets/Editor` (or an Editor-only assembly).
3. Select the Level 54 layout GameObject. Choose **Tools > Project Spark > Levels > 54 > Place AAA Visual Models**.
4. The tool creates/updates an `L54_VisualArt` child and positions 17 models. It does **not** generate terminal GameObjects or change the electrical solver.
5. Add `SparkLevel54VisualRig.cs` to `L54_VisualArt` and use its `Find Level 54 FBX Pivots` context menu. Assign the optional lamp Renderer references if desired.
6. Connect the real Level 54 gameplay simulation to `SparkLevel54VisualRig.ApplyState(state)` using **measured** shaft angle, shaft lateral displacement, bearing motion, damper/sensor status and guard state.

## Animation pivot reference

See `Animation_Rigs.csv` for exact names. Key transforms:

| Pivot name | Required animation |
|---|---|
| `MOTOR_SHAFT_ROTATE_X` | Drives motor shaft rotation around local X |
| `TEST_ROTOR_ROTATE_X` | Drives test rotor rotation |
| `SHAFT_FLEX_SEGMENT_00` ... `_06` | Seven separate deforming shaft sections, not a skeletal physics solution |
| `LEFT_FLEXURE_BEARING_YZ` | Left bearing support displacement |
| `RIGHT_FLEXURE_BEARING_YZ` | Right bearing support displacement |
| `ENCODER_DISC_ROTATE_X` | Encoder disc follows actual shaft angle |
| `X_PROBE_ADJUST` and `Y_PROBE_ADJUST` | Probe calibration gap visualization |
| `DAMPER_X_FORCE` and `DAMPER_Y_FORCE` | Reserved visual force markers, not physical actuator state |
| `SAFETY_DOOR_HINGE` | Safety guard opening around vertical axis |

The numeric shaft bend exaggeration is **art-only** and configurable (`visualArtUnitsPerMillimeter`). It must never influence game mechanical calculations.

## Electrical terminal anchors

`Terminal_Anchors.csv` contains **36 unique** art anchor transforms, all named `SOCKET_<ID>` inside the FBXs, with both local and assembled positions. These anchor nodes are **NOT** `SparkTerminal` components. Create Project Spark's real terminal/signal bindings separately and seat interactive sockets onto the anchors.

`Exact_Wiring.csv` contains the **22 proposed player wires**, including 5 fan-out wires from +5 V and five from RETURN. These are a design manifest, not auto-connected electrical wires.

**W15:** Y sensor OUT to controller Y IN — install last if Undo Last is used for the final fault exercise. The fault must affect actual signal validity, not only the art.

## FBX / GLB and materials

- FBXs are Autodesk FBX 7.4 binary mesh files (Y-up, local positions, mesh and pivot names preserved).
- GLBs are alternate physically viewable mesh exports with per-vertex base colors. Their glTF hierarchy is flat for convenient preview; prefer FBX for animation hierarchy.
- The transparent enclosure uses art material `glass`; configure the appropriate Unity transparent/Lit shader if your render pipeline imports it opaque.
- Screen faces are blank except decorative framing: add world-space TextMeshPro to the controller/monitor for actual data.

## Gameplay and completion are not included

Do not read the visual rig to determine voltage, current, RPM, vibration or progress. Level-local gameplay systems need to be integrated with the existing real solver and `LevelGamePlayManager`. The six authoritative objective checks require separate implementation and Play Mode testing.
