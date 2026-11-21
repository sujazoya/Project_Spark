# Project Spark — Level 128: The AORUS Master
## Master AAA-style production implementation brief

**Engine:** Unity 6.3 URP; PC and mobile; uGUI / TextMeshPro / New Input System. Board only. Reference product: GIGABYTE X870E AORUS MASTER. Existing project runtime is authoritative. All proprietary electrical mappings and CAD coordinates are unknown. The supplied meshes are original approximations, not electrically approved PCB layout. All six score objectives remain blocked until integration and evidence validation.

### Architecture
- Factory motherboard assembled with 682 factory-part models and 16 separate mechanical parts; CPU, memory and NVMe service assets are separate and initially absent. Keep factory identity distinct from installed service inventory. Showcase prefab is visual-only. Do not score from object visibility, animation callbacks, generated dummy wires, mesh IDs, or inspector values.
- Use SparkSelectionController as sole selection authority; use existing move/rotate/manipulation, terminal and virtual instrument APIs, SparkCircuitSystem/SparkElectricalNetworkGraph for approved physical paths, LevelGamePlayManager for objective authority. Inspect the current project code before adding any adapters. Never invent APIs or replace them with parallel truth layers.
- Use physical meters for calculations; tabletop presentation scale is separate. Eight layer models are illustrative and inactive by default. Assign pads, reference designators and route nets using an approved educational schematic, not a purported manufacturer netlist.
- Board architecture: 24-pin ATX and dual EPS, AM5, DDR5 A1/A2/B1/B2, primary PCIe, lower full-length slots electrically x4, M.2 A/B/C CPU and D chipset. The real board's advertised CPU-controlled M.2 B/C occupation can reduce GPU width to x8 for supported Ryzen 7000/9000 configurations; CPU families and specific revision must be checked. The included teaching evaluator is illustrative and cannot award an objective.
- Art: dark PCB/silkscreen, brushed armor, physically modeled connector cavities, retention parts, textures, material-batched showcase. Refine all silhouette, markings, clearances, underside and I/O details using approved reference photos. Actual AAA quality requires artist review and lighting, not just a mesh count. For mobile, profile batching, culling, LOD, texture compression, memory, shaders and draw calls; 1,425 separate mesh groups are intentionally editable, not a recommended production render hierarchy.

### Mission flow and authority

- **O1 — Safe system assembly**: Install sample CPU, cooler, DDR5 and storage, with correct retention and de-energized service checks.

- **O2 — Standby and power sequencing**: Verify standby, simulated power-good, CPU and memory rails against approved reference data.

- **O3 — POST and memory training**: Diagnose simulated CPU/DRAM/VGA/BOOT stages and achieve verified fresh POST.

- **O4 — PCIe and NVMe qualification**: Enumerate graphics and four M.2 sockets; explain CPU-specific lane sharing.

- **O5 — Three physical repair cases**: Diagnose the simulated VRM, memory, and storage faults by trusted measurement and repeat tests.

- **O6 — Protected return to service**: Verify shutdown/restart, thermal response, protection and stable repeated boot. No fictional safety jumper on commercial motherboard.

- Start de-energized. No fabricated manufacturer-specific debug values or live-board service. Simulated POST remains off until installation and approved power conditions. Fault injection must change the simulated target, not a label. O5 and O6 require fresh trusted results for the current topology revision. On removing CPU, memory, storage or armor, invalidate all dependent results. Manual reset and safe repeated boot/restart tests must not bypass electrical protection. No fictional W jumper for this commercial board.
- Acquire separate cooler, GPU, PSU and cables as necessary for fully implemented system assembly; the provided board-only kit includes generic CPU, memory and SSD service visual assets, not a complete PC build. Add compatibility and clearance checks before gameplay approval.
- Create service target colliders from approved geometry and approved footprint/pin data; never use procedural contact indices as real pins. Integrate diagnostics with existing virtual multimeter and scope. Add power-good, VRM and memory training state machine with deterministic fault seeds; source measurements from the authoritative simulated circuit. Keep illustrative POST evaluator separate.

### Fault scenarios

- **F1 / VRM_Q03** — Simulated power-stage open or failed enable. CPU rail fails validation; investigate approved circuit before replacement. Physical binding UNASSIGNED.

- **F2 / DDR5_A2** — Simulated contaminated or intermittent contact. Memory training fails; inspect and test channel. Physical binding UNASSIGNED.

- **F3 / M2B_CPU** — Simulated M.2 connector intermittent. Storage enumerates unreliably; inspect and retest link. Physical binding UNASSIGNED.

### UX, safety, performance and acceptance
Use a scientific PC service station: de-energized state, microscope inspection, exploded armor mode, voltage-domain overlays, POST event timeline, storage device inventory, clear instrument evidence, and Spark voice hints. No fake measurement or automatic goal completion. Preserve all previous levels. Avoid per-frame object discovery or unbounded allocations, cache references, unsubscribe events, use explicit data contracts, versioned saves and reproducible tests. Add unit tests and Unity edit/play tests after reviewing the codebase. Use the acceptance matrix rather than claiming certification. Local Unity FBX import and C# compile are mandatory before enabling gameplay. All electrical objectives are BLOCKED until source approval and integration.
