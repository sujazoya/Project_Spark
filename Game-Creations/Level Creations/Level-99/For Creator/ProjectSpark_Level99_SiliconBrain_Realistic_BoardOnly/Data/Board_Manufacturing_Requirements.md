# Manufacturing and rendering constraints
- Physical design outline: **190 x 130 mm**; nominal 1.6 mm thick FR4, four conductive layers. Coordinates in metres in FBX, millimetres in placement CSV.
- PCB geometry contains **decorative pad and factory trace references**, not a validated circuit; final routing requires actual pad and net IDs, clearance rules and layer connectivity.
- U1 is a procedural 64-lead LQFP, not a manufacturer-exact land pattern; verify pin pitch, body and package footprint before true electrical hookup.
- Four-layer references are visualization only. Do not treat the plane meshes as electrically continuous until the circuit solver confirms it.
- Avoid electrical false positives from the assembled reference FBX.
- Models are procedural realism-style, not certified CAD or manufactured hardware.
