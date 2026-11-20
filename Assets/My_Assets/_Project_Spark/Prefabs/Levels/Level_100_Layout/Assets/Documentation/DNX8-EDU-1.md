# Level 100 · Digital Nexus

DNX8-EDU-1 is the fictional training circuit used by the playable Level 100 prefab. It uses the source kit’s 70 parts, 210 × 145 mm board, and original placements. The Unity workbench view scales the board to about 10.13 × 6.99 units on the 15.79 × 8.54-unit table.

The [training route map](../Data/DNX8_EDU_1_Training_Routes.csv) gives the assigned game terminals, entry layers, via spans, and XZ coordinates in millimetres. It covers W001–W092 and the removable W093. The original pending-review CSVs remain the source-kit references.

| Objective | Evidence |
| --- | --- |
| Assemble | Fit and solder all 70 parts at their designated footprints. |
| Route | Build 92 exact terminal pairs and plate 20 barrels across four layers. Close W093 and verify the six continuity branches. |
| Commission I²C and SPI | Verify the approved virtual firmware, qualify power, clock, and reset, then discover three I²C devices and exchange data with two SPI targets. |
| Qualify UART and the three buses | Decode a fresh 115200 8N1 packet in each direction and pass the I²C and SPI checks in the same run. |
| Repair | Measure slow SDA rise at R1, loss of received SCK across W035/V11, and a 9600-baud frame mismatch at U8. Verify each repair with a new boot and full bus tests. |
| Recover W093 | Prove that U9 inhibits the protected output while U1 continues running. Reconnect, observe a fresh heartbeat, reset U9, and pass a new boot and full bus run. |

The existing circuit solver drives continuity, rail measurements, logic-level checks, and the live trace. The training software models bounded register exchanges and UART sampling once the corresponding electrical paths qualify. I²C rise timing is accelerated to make the solved voltage transition observable in the game. U9 receives its communication-health signal from U1 through W093, separate from SPI, I²C, UART, and the supply.

`Tools > Project Spark > Levels > 100 > Run Gameplay Acceptance` exercises the campaign entry from MainMenu. The acceptance report and screenshots are written under the project's `Logs` directory.
