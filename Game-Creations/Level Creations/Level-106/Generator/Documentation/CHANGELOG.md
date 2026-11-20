# Level 106 — Full Layout Generator Addon

## V1 (2026-11-19)

- Adds a dedicated Level 106 builder and Transform-anchor tabletop placement.
- Supplies 137 mesh-bearing OBJ fallbacks (136 modular + one completed reference), preserving original FBX/GLB source files.
- Normalizes existing 136 model records (source positions meters converted to mm), 140 routes, 42 vias, 3 faults, 6 objectives, and W141 safety contract for the Universal Generator V2 importer.
- Creates `Level_106_Layout` with source models, 106 placement slots, 140 route task objects, 42 via task objects, ten visual copper layers, diagnostic bindings and a disabled assembled reference.
- Reports U14 missing from original BOM and keeps all uncertified electrical data blocked.
- Uses existing Unity generator installation and avoids duplicate classes.
- Fits board to 10.13 × 7.51 units on 15.79 × 8.54 tabletop, including Transform-only anchor handling.

## Verification boundary

Offline: source counts, GLB/OBJ geometry, package checks. Not tested: Unity import, Unity compilation, Play Mode, verified electrical solver, true AAA material finishing.
