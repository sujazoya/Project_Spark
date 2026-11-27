# Native wiring changes

## Level 20 binding ownership

Engineering Control and the native director now share the layout binding table's strict pair resolver. It checks the live layout, duplicate declarations, runtime overrides, active state, owner and membership. The existing lookup remains available to gameplay code that needs its original semantics. The resolver reports an empty workstation by stable ID and tells the player to install its component before suggesting a wire. The approved plan and the Level 20 authored IDs were preserved.

## Original hand and cable

The cinematic probes the original hand before choosing navigation. The native physical-cable path checks again at the reached stance, so a route that was out of reach at the start can use the existing telescopic grip controller. Exact contact remains enforced by the original grip bridge. The existing player builder owns cable preview and the registered connection. The pointer setting is off by default and cannot bypass Engineering Control.

The first native hand replay committed a cable but exceeded the fixture's route-to-span limit. The cable solver now bounds the sampled geometry by both rest length and direct span, and re-solves with the exact socket endpoint before the native commit. The historical 10.54-unit figure is separate from this run.

The original hand now retracts before the director reports the physical performance complete. The director checks the native electrical link before it publishes final success. A read-only JSON export records the active layout, reviewed binding diagnostics, engine state, actual grip and cable end, cable lengths, extension and the last registered wire.

The arm pose runs after character animation, and cable sampling runs after the arm pose. Sampling no longer preempts the arm's final-pose update. The fingertip trajectory limits motion after accounting for parent joint rotations. At wire nine the trace also identified `Register_Output` as a blocker. The 470 Ω shelf prefab has a disabled selection mesh with a scanner hover target and a box collider. The arm filter treats that specific selection envelope as a picking surface while retaining collision queries for solid geometry. The native player selection collider remains available to the Wire Tool.

The hub now lends the director authority for the exact reviewed pair during dispatch. The director rechecks reference identity and contact position during the action. The executable phrase aliases stay on the same v14 route and therefore require the same pending review and approval.

## Repeat the test

Open MainMenu and enter Level 20. Install the bridge resistors in the authored shelf slots and switch virtual power off. Say `spark assisted mode`, `spark suggest next wire`, then `spark approve suggested wire`. Before installing the resistor, the suggestion should report `RUNTIME_SLOT_UNPOPULATED: R_LTOP_Input`. After installation, the reviewed pair should resolve. Use **Project Spark > Spark V5 Pro > AAA Laboratory > Test Level 20 Flight and Native Wires** for the automated regression. Use **Export Native Wiring Diagnostics** for a JSON snapshot and **42 - Diagnose Original-Hand Precision v18.4** for rig details. Keep **Allow Reviewed Pointer Fallback** cleared on the cinematic component when testing the original hand.

## Separate Bootstrap change

The Bootstrap baseline logged “MainMenu must be included in the build scenes” even though the exact scene asset was enabled. Bootstrap now resolves the full `.unity` asset path to its enabled build index and loads that index. To replay this independent check in Edit Mode, write `boot` to `Logs/SparkWorkbench/run.request` and inspect `Logs/SparkWorkbenchVerification.txt`.

## Standing switch press

The former switch action reused the wire approach and grasp, which sent Spark into a tilted hover and aimed at the lever hinge. The assisted switch now uses the walkable workbench, pauses with its feet planted, and uses the original right index finger to press the moving end of the lever. The native switch changes state only after the approved action reaches the lever. The index follows the lever as it moves, then lifts and retracts. Send `switch` to the same verification request file for the focused Level 20 replay.

## Wire nine approach

Wire nine occasionally reached a planned corner that the live body sweep rejected. The carry planner checks space to each side of the path to account for steering and foot motion. If the current pickup stance lacks that room, it looks for a second reachable pickup stance before attaching the lead. Live clearance and cable-length checks still guard motion.

## Rollback

The baseline source is Git commit `89a3bacc471075a27d9ef82b2ce2535eedaa8e57`. No scene, prefab or approved plan was edited for these changes. Compare the affected source files individually to the baseline and revert only the wiring patch. Preserve the pre-existing working-copy changes recorded in the audit. The captured baseline Play Mode report is `Logs/SparkFlightV13Repair/NativeWiringBaseline.txt`.
