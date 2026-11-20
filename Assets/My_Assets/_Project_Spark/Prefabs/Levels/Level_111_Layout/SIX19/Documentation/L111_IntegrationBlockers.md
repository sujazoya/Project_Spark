# L111 source and gameplay certification blockers

1. **No approved 14-layer schematic / manufacturer-confirmed board stackup**: no actual copper/net graph approved.
2. **184 route tasks UNASSIGNED**: source/destination pads, net IDs, layers, widths and clearance remain placeholders.
3. **60 via tasks UNASSIGNED**: net, span, drill, x/z locations are unknown.
4. **W185 U18/U19 physical pins unapproved**: logical roles alone cannot create authoritative graph contacts.
5. **No actual Project Spark project source is present**: Unity `LevelGamePlayManager`, shop/shelf, solver and Virtual PCB Repair UI adapters remain unbound.
6. **Simulation inputs not certified**: package models approximate SerDes and SI pedagogically, not standards compliance; real geometry, dielectric and line losses must be reviewed.
7. **Unity compilation / import / Play Mode / Android performance NOT TESTED**.
8. **AAA art final pass incomplete**: meshes/textures are source-authored procedural previews, not manually unwrapped/baked artist signoff.

Every task must remain NON_CERTIFIABLE until the verified real system passes tests.
