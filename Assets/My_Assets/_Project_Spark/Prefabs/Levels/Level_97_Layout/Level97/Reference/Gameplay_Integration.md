# Gameplay and virtual tools
- Reuse existing World Space Canvas virtual PCB tool system; no physical instrument meshes are included.
- Use Component_BOM_50.csv for placement IDs and expected component types.
- Treat FBX parts as art; add colliders and component targets for click/touch inspection and repair.
- 66 route tasks and 14 vias begin unmade and are validated by conductive topology, not by visible strips.
- Faults: feedback resistor mismatch R7, open via V05, MOSFET high loss Q1.
- W067 trust feedback path MainRailSupervisor_U7 -> PowerSafetyController_U8, initially missing, installed for commissioning, deliberately disconnected in Objective 6.
- Repair VFX should appear at hit location, and only the authoritative electrical component can mark the fault corrected.
- Requirements for 6/6 objectives in Objectives_6.csv. Preserve all existing levels.
