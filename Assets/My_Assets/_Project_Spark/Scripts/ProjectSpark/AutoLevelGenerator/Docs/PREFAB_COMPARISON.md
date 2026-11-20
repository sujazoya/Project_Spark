# Prefab inspection: why the previous Level 110 layout was incomplete

Inspected the three *uploaded* YAML prefabs directly (object counts, names and parent-child references).

| Source prefab | GameObjects | MonoBehaviours | Direct child groups under board physical root |
|---|---:|---:|---|
| `Level_103_Layout.prefab` | 925 | 898 | ComponentSlots (44), PhysicalPads (389), SolverTopology (295), PlayerCopper F_Cu/B_Cu, ViaSockets (12), virtual controls, board art, repair and instrumentation |
| `Level_104_Layout.prefab` | 1,577 | 1,338 | ComponentSlots (50), PhysicalPads (630), SolverTopology (479), PlayerCopper (81 entries), virtual controls (34), board art |
| `L110_PWR18_EMPTY_GAMEPLAY_AUTO_GENERATED.prefab` | 142 | 132 | 130 footprint targets, art placeholders, 1 child for wiring/vias; **no** populated solver/pad/copper/virtual-control structure |

The V2.1 upgrade creates a full `Level_110_Layout` hierarchy from the available source files. It does **not** copy the unknown custom MonoBehaviour GUID bindings of Levels 103/104 onto Level 110 objects because their serialized fields and actual Unity script contracts are unavailable in this upload.

The Source Level 110 package declares 130 installable parts, 38 supporting modular meshes, 172 routes, 56 vias, three faults and six objectives. Its route/via records are planning data with no approved net/pin/layer mapping, so functional electrical pads, wire meshes and solver graph edges are explicitly held pending verified schematic data.
