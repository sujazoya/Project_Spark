# Level 105 · Expansion Nexus

ENX13-EDU-1 is a fictional gameplay circuit for the supplied ENX-13 motherboard and ECX-4 endpoint. The source’s unassigned schematic and art manifests retain their authoring status. The U12 and U13 terminals in the training circuit implement the source’s logical safety roles with fictional contacts.

The campaign starts with 94 motherboard and six card components uninstalled. It preserves the original placement anchors and the common model scale. The board measures about 10.13 × 7.13 Unity units. The parked ECX-4 fits alongside it on the 15.79 × 8.54 Unity-unit workshop table.

| Objective | Gameplay evidence |
| --- | --- |
| Assemble | Close each original-footprint assembly contact, with separate alignment inspection for U1 and U2. |
| Route | Complete the exact native terminal pairs and layer spans in the [training route map](../Data/ENX13_EDU_1_Training_Routes.csv). Verify seven continuity branches and install W133. |
| Commission | Align and seat ECX-4, verify endpoint power and complementary REFCLK states at the card, release reset, and train four lanes at the teaching model’s 5.0 GT/s. |
| Enumerate and transfer | Read ECX-4 configuration, assign its 4 KiB resource window, and verify 128 sequence-tagged frames totaling 4,096 bytes with CRC checks. |
| Repair | Diagnose C22’s open coupling contact, U3’s missing negative clock state, and J1’s load-sensitive lane 3 contact. Confirm each repair with a fresh link, enumeration and transfer. |
| Restore W133 | Open the physical wire and verify workload inhibition with the slot powered. Reconnect, qualify a new link and heartbeat cadence, and reset U13. |

The electrical solver measures the high and low plateaus of training signals. The modeled reference clock represents 100 MHz. Sampling advances at a playable pace, and displayed lane timing margins apply a teaching model to solved receiver levels and authored length and pair skew. The model covers the selected training paths and omits manufacturer pin assignments and physical compliance testing.

Open **Expansion Nexus** in the Main Menu. The playable builder is under **Tools → Project Spark → Levels → Create Level 105**. For automated Play Mode verification, keep MainMenu active and select **Tools → Project Spark → Levels → 105 → Run Gameplay Acceptance**. The report and captures are under the project’s `Logs` directory.
