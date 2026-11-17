# AAA Virtual Workstation — UI & VFX Design

## Art direction

A clean, premium aerospace electronics look: deep midnight graphite background (`#091324`-like), metallic blue panel bevels, restrained cyan holo scanlines, amber cautions, mint/green verification, and red tool rejection. The demo board is a *digital twin*, not a replacement for real PCB geometry. UI prioritizes readable information over clutter and preserves the user's large Project Spark table.

## Layout (2,200 x 1,240 logical pixels)

| Area | Position/size | Content |
|---|---|---|
| Header | x22 y22, 2156x100 | Project Spark brand, engineering deck, demo/live label |
| Tool deck | x22 y140, 328x955 | Three tool categories, 21 commands, active tool instructions |
| Main board | x365 y140, 1257x955 | Board target digital twin and touch components |
| Instruments | x1638 y140, 540x955 | Target telemetry, 3-channel animated graph, readout and log |
| Footer | x22 y1112, 2156x105 | User guidance and active pointer target |

All labels and panels are uGUI and scale with the Canvas Transform. The generated scene uses one Canvas and GraphicRaycaster, not a prefab forest of 3D tools.

## Touch-to-repair sequence

1. Player taps the **REPAIR** tab, then the required tool (e.g. FLUX).
2. Player touches a specific component/pad/trace on the digital twin or existing physical PCB.
3. The `SparkVirtualPCBWorkstation` checks demo mode vs authoritative adapter.
4. If accepted, `SparkPCBTouchVFX` animates a contact ring and context-colored particles at the touched position. Soldering and reflow use warmer sparks and upward drifting particles.
5. The instrument readout explains result. Local demo targets change their demo status; authoritative project topology changes **only** if a real adapter approves the operation.
6. Players use inspection tools again to verify the repair.

## Inspection panel behaviors

- **DMM:** probe reading and selected net label.
- **Continuity:** open/short/expected state using demo faults or real authority.
- **Oscilloscope:** procedural multi-channel waveform with Hz label; demo signal is NOT live solver data.
- **Logic Analyzer:** multi-trace timing visualizations.
- **Thermal:** spot scan readings and red/orange target pulse.
- **Net Tracer:** net ID with localized cyan highlight.
- **Fault Scanner:** displays defect category for a valid scan.
- **Microscope:** high detail component status text in the demo; a future production version can open a magnification UI.

## Typical repair recipes

| Defect | Sequence |
|---|---|
| Cold joint | Scan → Flux → Solder / Reflow → Continuity |
| Solder bridge | Scan → Desolder → Clean → Continuity |
| Open copper trace | Scan → Trace Patch → UV Mask → Continuity |
| Open via | Scan → Via Repair → UV Mask → Continuity |
| Failed IC / wrong value | Scan → Hot Air → Replace → New measurements |
| BGA ball defect | Scan → Flux → Hot Air → BGA Reball → Scan |
| Contamination | Inspect → Clean → Inspection |
| Calibration drift | DMM / Scope → Calibrate → Fresh probe readings |

In a real level, these steps should be checked by actual engineering state, permitted rework operations, station temperature and objective evidence. The demo kit intentionally only models minimal visual workflow flags.

## Motion, UI polish and optimization

- 20 Hz waveform display, no imported animation clips.
- Contact pulse, particle burst and fading smoke via a limited number of translucent UGUI graphics.
- Rounded UI cards are generated with a custom MaskableGraphic triangle mesh, no asset textures required.
- Optional scanline shader is assigned to 7 main panels with a persistent native material created by the editor.
- `Graphic.raycastTarget=false` on noninteractive artwork avoids stray pointer interception.
- 21 tool buttons and 16 sample targets are click/touch-enabled and self-bind listeners at Awake so they survive scene serialization.

For future Polish: custom brand font (licensed by your team), HDR bloom, UI camera depth control, real probe sound cues, per-tool cursor animations, haptics on mobile, semantic UI animations, and localized tool instructions. Those enhancements are not part of this baseline kit.
