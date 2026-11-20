# W101 — BLOCKING SOURCE CONFLICT

The supplied `Data/Critical_W101.json` states:

- Source: `L101_LinkIntegrityMonitor_U10.TRUSTED_LINK_HEALTH_OUT`
- Sink: `L101_MultilayerSafetySupervisor_U11.TRUSTED_LINK_HEALTH_IN`
- Starts **disconnected**; `functional_pins_verified=false`.

But the supplied `Data/Asset_Manifest_96.csv` and `Data/Component_BOM_76.csv` list:

- `U7` = LINK_MON
- `U8` = SAFETY
- `U10` = LDO
- `U11` = INTERFACE

These source facts conflict. This upgrade DOES NOT silently rewrite the W101 source JSON or infer that U7/U8 are the approved physical contacts. The visual target W101 is listed once in the 76-component BOM and no electrical terminal objects or trusted-feedback shortcuts are created. Resolve exact physical pins, choose authoritative owner/refdes, version and approve the reconciliation, and only then connect `SparkTerminal` and `LevelGamePlayManager` in the live project.

Under no circumstance may W101 trigger Objective 6 through a visual toggle; fresh verified changing health signals, independent inhibition/reset, and a new qualified link test are required.
