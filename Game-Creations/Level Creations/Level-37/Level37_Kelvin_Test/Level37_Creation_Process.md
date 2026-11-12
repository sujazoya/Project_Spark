# Level 37 — Kelvin Test

Build the FORCE loop, add the Kelvin SENSE wires, and use both readings to tell a fixture fault from a damaged connector.

1. Install the runtime and editor scripts into the matching folders under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Scripts/` if needed.
2. With Play Mode stopped, run **Tools → Project Spark → Level 37 → Create Level Assets** if the assets are missing. Unity saves `Level_37.asset`, `Level_37_Kelvin_Test_Layout.prefab`, `Level_37_Drawing.asset`, and materials under `Assets/My_Assets/_Project_Spark/Prefabs/Levels/Level_37_Layout/`.
3. In the active **Level Layout Catalog**, pair the definition with the layout prefab. In **LevelGamePlayManager → Levels**, put the definition after Level 36 and keep **Auto Evaluate** enabled. Add the drawing to **Spark Draw Current Level → Drawings**.
4. Open Level 37 in Play Mode. Use the Wire tool. Connect W01–W05 and press **READ LOOP** before connecting W06–W07. Each connector socket accepts one FORCE wire and one SENSE wire.

| Wire | From | To |
|---|---|---|
| W01 | `Battery_positive` | `LIMIT_IN` |
| W02 | `LIMIT_OUT` | `LEAD_PLUS_IN` |
| W03 | `LEAD_PLUS_OUT` | `CONNECTOR_PLUS` |
| W04 | `CONNECTOR_MINUS` | `LEAD_RETURN_IN` |
| W05 | `LEAD_RETURN_OUT` | `Battery_negative` |
| W06 | `SENSE_PLUS` | `CONNECTOR_PLUS` |
| W07 | `SENSE_MINUS` | `CONNECTOR_MINUS` |

The left socket is IN or +. The right socket is OUT or −. The sense inputs are on the teal module below the connector. Connect them to the connector sockets already used by W03 and W04.

| Progress | Action | Test current | Two-wire | Kelvin |
|---|---|---:|---:|---:|
| **1/6** | Wire W01–W05. Press **READ LOOP**. | 40.0 mA | 25 Ω | — |
| **2/6** | Add W06 and W07. Press **READ KELVIN**. | 40.0 mA | 25 Ω | 5 Ω |
| **3/6** | Press **LOAD CASE**. Choose **LEAD FAULT**. | 37.0 mA | 35 Ω | 5 Ω |
| **4/6** | Press **LOAD CASE** again. Choose **CONNECTOR FAULT**. | 35.2 mA | 42 Ω | 12 Ω |
| **5/6** | Press **FIT CONNECTOR**. The lead fault remains. | 37.0 mA | 35 Ω | 5 Ω |
| **6/6** | Press **REPLACE LEAD**. Verify both readings and the manager's completion state. | 40.0 mA | 25 Ω | 5 Ω |

The two-wire voltage is measured from LIMIT OUT to Battery −, so it includes the two leads and the connector. The Kelvin voltage is measured through a 1 MΩ input across the connector. Both readings use the current through the 100 Ω limiter. The 1 MΩ input produces negligible loading at these resistance values. The level uses the existing supply, wire, and resistor solvers.

To test wiring validation, reverse the two SENSE wires at 1/6. The two-wire reading should remain available and the Kelvin reading should stay blank. Correct the polarity and verify that the first objective is still recorded. Try a wrong diagnosis and confirm progress holds. **RESTART TEST** restores the nominal parts and clears objectives. Rewiring during fault testing restarts the sequence.
