# Level 96 — preflight and regression checklist

1. Unity C# console compiles (no CS errors) after install and model import.
2. `L96_SMX4_PCB_140x95mm_Empty.obj` imports with MeshFilter + MeshRenderer; no blank OBJ geometry.
3. 44 unique component targets, all inactive installable meshes in the assembled preview.
4. 187 `UNASSIGNED_PENDING_SCHEMATIC` contact labels, no fake pad collider or net assignment.
5. 58 copper-route definitions and 12 vias visible as unverified integration records.
6. 14 PBR families use native URP packed maps. Inspect normals and alpha/roughness at zoom.
7. Board rendered `X width≈10.13`, `Z depth≈6.874` and rests flat at chosen table surface.
8. Reference assembled model stays `SetActive(false)` and cannot affect player progress.
9. W059 is a digital reference-valid feedback, never the 2.500 V analog rail.
10. Scene/prefab regeneration refuses to overwrite existing generated files silently.
11. Other level prefabs and scenes have unchanged GUIDs and content.
12. Play Mode electrical solver and objectives remain **BLOCKED** until approved netlist and adapters exist.
