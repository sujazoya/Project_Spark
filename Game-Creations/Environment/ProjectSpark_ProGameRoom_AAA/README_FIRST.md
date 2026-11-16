# PROJECT SPARK — PRO GAME ROOM / ADVANCED FUTURISTIC STUDIO

## Contents

This archive is a **genuine editable modular 3D art kit**, not an image. Its design is based on the Project Spark futuristic room direction with a single large multi-level table and restrained furnishings.

- **37 modular binary FBX 7400 files**, plus one complete assembled showcase FBX in `Models/`.
- **37 matching GLBs**, plus one complete assembled showcase GLB in `GLB/`.
- **17 different procedural PBR material families**. Each family has BaseColor, Normal, UnityMaskMap, MetalRoughAO and Emission PNG textures, for **85 total texture maps**.
- **2 original hologram shaders**, one Built-in ShaderLab/Cg, one Universal Render Pipeline/HLSL.
- **4 original Unity C# files**: the PBR material installer, room builder, asset validator and runtime visual-only level preview host.
- Room and table placement, materials, model and pivot manifests, plus production documentation.

**Art overview:** 18.2 m wide x 14.4 m deep room; 6.08 m high. Main multi-purpose table is 9.9 m x 5.4 m, with a glass presentation top, framed bays, lighting and easy access from three sides. The scene has three copies of the same chair model, a single smart waste bin, and one low-key decorative planter. There are no prefilled miniatures; level preview meshes are created **only when requested**.

### Unity setup (URP, HDRP or Built-in)

1. Extract `ProjectSpark_ProGameRoom_AAA/` into Unity's `Assets/` (for example `Assets/ProjectSpark_ProGameRoom_AAA/`). Keep Models, Textures, Shaders, Unity and Docs folders together.
2. Let Unity complete the FBX and texture import. The **FBX files store UVs, normals, material slot names and centimeter-scale geometry**; Unity imports physical dimensions using the file's unit conversion. The GLBs contain matching geometry in metres and may be retained as interchange backups. **Do not place GLB and FBX versions simultaneously.**
3. Open **Tools / Project Spark / Room Studio / 1. Create or Upgrade PBR Materials**. This creates **17 actual Unity `.mat` files** in `Assets/ProjectSpark/RoomStudio/GeneratedMaterials/` using the native project Lit shader, matching textures and transparency/emission settings.
4. Open **Tools / Project Spark / Room Studio / 2. Build or Refresh Full Room**. This instantiates the 37 FBX modules according to the saved layout CSV. The 3 chairs, 1 waste bin and 1 planter are placed at their saved positions. It also creates simple walkable colliders, soft lights and a level-preview host.
5. Optionally save the room as a prefab via **Tools / Project Spark / Room Studio / 3. Save Current Room As Prefab**.
6. To display one existing level as a full table preview, select its **assembled visual FBX or level art prefab** outside the room and run **Tools / Project Spark / Room Studio / 4. Preview Selected Level / Full Table**.
7. To display up to three separately scaled levels, use menu items **5–7** for Left, Center and Right bays.
8. Run **Tools / Project Spark / Room Studio / 8. Validate Imported 3D Kit** after material creation.

### Critical integration behavior

The table preview system copies **only static MeshFilter and MeshRenderer data** into batched render meshes and automatically fits the results to the table. It **does not clone** gameplay scripts, `SparkTerminal` components, electrical solvers, level objectives, physics components, shop items, or player wire objects. It never changes the original level prefab. Preview meshes are intentionally noninteractive, with no physics or certification authority. Actual large Project Spark levels remain playable using their original scene and prefabs; the table is a separate presentation display.

The visual room can therefore showcase art from many previously created levels without merging gameplay state or circuits. If a level includes special skinned meshes or VFX, build a static simplified art preview for those parts. Large source art can be batch-combined by material; preview creation requires readable imported meshes and may be expensive for very large sources, but only occurs when explicitly staged.

### PBR texture channel conventions

- `*_BaseColor.png` — sRGB albedo; base materials use white tint to avoid doubling the supplied color.
- `*_Normal.png` — tangent-style normal map; Unity import tool sets **Normal map** texture type.
- `*_UnityMaskMap.png` — **R metallic, G occlusion, A smoothness**; the same packed map works with the native URP Standard Lit metallic/occlusion settings, the Built-in Standard shader and the HDRP MaskMap convention.
- `*_MetalRoughAO.png` — **R metallic, G roughness, B AO** (optional engine-neutral source).
- `*_Emission.png` — emissive color source; the installer enables Unity emission keywords.

Glass and hologram materials have transparent rendering. For URP the hologram uses the included original scan/Fresnel shader; for Built-in it uses its matching version; for HDRP it falls back to the native HDRP/Unlit path. Other surfaces use the project's PBR Lit shader.

### Performance and visual polish

The kit is intentionally a restrained environment, not an entire playable level. Work with hardware MSAA/TAA appropriate for the renderer, reflection probes, appropriate baked/mixed lighting, contact shadows, and anti-aliasing. Keep preview geometry separate from gameplay scenes. Optionally add a sky/landscape outside the panorama windows using your own environment system; the archive supplies the room geometry, glass and window framing but **does not include a copyrighted landscape photo or generated backdrop**.

### Status / limitations

**Verified offline:** FBX file signatures and basic geometry layers, GLB mesh loads, material and UV presence, texture dimensions/channel presence, model manifests, scene placements and archive integrity.

**Not verified in actual Unity:** rendering-pipeline shader compilation, material importer results, FBX unit behavior, Editor scripts, visual fit for each individual historical level, scene navigation, collisions, or lighting quality. The kit is **AAA-inspired/procedural original 3D art**, not a claim of fully artist-finished AAA production or a drop-in electrical gameplay level.
