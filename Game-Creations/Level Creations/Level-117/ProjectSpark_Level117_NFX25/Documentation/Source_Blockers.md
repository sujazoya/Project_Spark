# NFX-25 Electrical Schematic Reconciliation Register

| ID | Needed | Current state | Required evidence |
|---|---|---|---|
| B1 | Complete 26-layer copper stackup and material parameters | UNVERIFIED | Signed netlist, stackup, plane map and DRC |
| B2 | All 304 W001-W304 route source/dest/net/layer/geometry IDs | UNASSIGNED | Approved pad-level pin map, DRC and topology validation |
| B3 | All 108 V001-V108 via net/span/technology/geometry | TBD | Approved fabrication span and continuity graph |
| B4 | Every one of 220 physical component footprints/values/orientations | FALSE | Pin-one, package/footprint and solder contact inspection |
| B5 | U35/U36 roles and W305 exact contacts | UNASSIGNED | Signed functional schematic and removable jumper contacts |
| B6 | Four 10GbE-class optical port schematic/module specs | UNVERIFIED | Module datasheets, real pins and level interface model |
| B7 | TDR/eye, PTP offset, CRC traffic acceptance thresholds | UNVERIFIED | Approved educational model parameters and ranges |
| B8 | Real Project Spark integration/Play Mode | NOT_RUN | Unity compile logs, integration tests and gameplay captures |

**Do not modify previous levels or certify electrical objectives from decorative assets.**
