# Project Spark — Animated <> Voltage Monitor Drawer

## Installation

Requires the existing `SparkLevelVoltageMonitor.cs` and `SparkLevelVoltageMonitorEditor.cs` from the Live Voltage Monitor package, plus Unity UI and TextMeshPro. The ZIP includes all four scripts to make a standalone install possible.

1. Copy `Runtime/*.cs` into `Assets/My_Assets/_Project_Spark/Scripts/Monitoring/` (or any ordinary scripts directory).
2. Copy `Editor/*.cs` into a folder named `Editor` under Assets.
3. If you already installed the earlier voltage monitor, **overwrite** the two matching original filenames rather than keeping duplicates.
4. Wait for Unity compilation. Open the gameplay scene, outside Play Mode.
5. Select `Tools > Project Spark > Monitoring > Create Animated Voltage Drawer`.
6. Save the scene, enter Play Mode, then click the right-side `VOLTS ❮` tab to slide the panel in. Click `HIDE ❯` to slide it out.

## UI behavior

- Starts collapsed on the right side of screen, with a 48px-wide visible tab.
- 0.34-second cubic ease-out slide-in / slide-out animation.
- Uses unscaled time, so it animates when Time.timeScale is zero.
- Works with your existing Pause/Clear controls and live voltage readings.
- No electrical solves, no `MarkDirty()`, and no modification to shared electrical code.
- `SparkVoltageDrawer` Inspector: adjust `Start Open`, `Animation Duration`, or `Additional Hide Distance`.

## Hierarchy

Spark_VoltageMonitor_Canvas (SparkLevelVoltageMonitor + SparkVoltageDrawer)
  VoltageMonitor_Panel
    Header
    Pause_Button
    Clear_Button
    Summary
    Readings_Frame
    History_Frame
    VoltageDrawer_ToggleTab (Button)
      Arrow
      Label

## Important

This upgrades the **existing scene monitor**, and does not need to be put on every Level Layout prefab. If you use multiple gameplay scenes, create the drawer in the desired scene(s). The voltage monitor is read-only and tracks the active layout, not the editor's prefab stage.
