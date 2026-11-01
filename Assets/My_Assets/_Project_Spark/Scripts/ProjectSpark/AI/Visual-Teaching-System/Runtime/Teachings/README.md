# Project Spark — Level 7/8 Advanced Teaching Whiteboard

This is a targeted update to the existing visual teaching system, based on the supplied circuit reference image. It does **not** construct wires or report verified live measurements.

## Files

- `Runtime/SparkLevel78WiringTeaching.cs` — **required replacement** for the existing class of that name. Preserves `ShowLevel7()` and `ShowLevel8()` methods and the existing `PresentValidatedPlan()` manager flow. Correct series/parallel topology and compact labels.
- `Runtime/SparkSchematicRenderer.cs` — **optional replacement** for the existing class of that name. Adds LCD rectangular symbol with A+/B- terminals, small junction dots, and compact non-overlapping labels. Replaces original renderer; **do not keep two copies**. No extra GameObjects are created beyond the original transient symbol labels. The core diagram renderer and manager APIs remain unchanged.

## Level 7

Battery+ to switch input is assumed installed. The five remaining wiring steps:
1. `SwitchOutput` → R1 220 Ω `Register_Input`
2. R1 `Register_Output` → R2 330 Ω `Register_Input` (VOUT junction)
3. R2 `Register_Output` → `Battery_negative`
4. `LCD_Terminal_A` → VOUT junction
5. `LCD_Terminal_B` → `Battery_negative`

Expected nominal: VOUT 3.00 V; divider current 9.09 mA.

## Level 8

Battery+ to switch input is assumed installed. Six remaining wiring steps:
1. `SwitchOutput` → R1 330 Ω `Register_Input`
2. `SwitchOutput` → R2 470 Ω `Register_Input`
3. R1 `Register_Output` → `Battery_negative`
4. R2 `Register_Output` → `Battery_negative`
5. `LCD_Terminal_A` → `SwitchOutput`
6. `LCD_Terminal_B` → `Battery_negative`

Expected nominal: 5.00 V across each branch; 15.15 mA and 10.64 mA; 25.79 mA total.

## Installation

1. Back up your two existing `.cs` files.
2. Replace **only** `SparkLevel78WiringTeaching.cs` first. Verify diagrams through your existing Tutorial buttons.
3. To get the advanced LCD rectangle and junction rendering, also replace the existing `SparkSchematicRenderer.cs` with the optional version, retaining its location and `.meta` file. Do not have duplicate scripts/classes.
4. Enter Play Mode. Use `ShowLevel7()` / `ShowLevel8()` as before.
5. If needed, use the existing board **Zoom +**, **Zoom -**, and **Reset View** controls.

These layouts are normalized to the existing drawing RectTransform and adapt to its width/height. No Unity compile or Play Mode test was performed here.
