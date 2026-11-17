# ProjectSpark — Spark UI Manager (Unity uGUI)

## Contents

- `SparkUIManager.cs` — panel registry, lifecycle, open/close/toggle, sorting, input-blocking query, focus, close policies, navigation, transitions.
- `SparkUIPanel.cs` — add-on adapter for existing panels. Original behavior scripts are not replaced.
- `SparkUIMonitorDashboard.cs` — runtime-generated, clickable master console fitted over the supplied monitor artwork.
- `SparkUISetupWizard.cs` — Unity Editor commands.
- `Resources/SparkMonitorFrame.png` — user-supplied blank monitor artwork.
- `Documentation/Dashboard_Visual_Reference.png` — art-direction image, **not** a ready-made interactive prefab.

## Compatibility

- Designed for Unity 6 / 6000.x, built-in Unity UI (uGUI): `UnityEngine.UI` and `UnityEngine.EventSystems`.
- Requires **Unity UI** package (`com.unity.ugui`) and **EventSystem** with an input module.
- Supports Screen Space Overlay monitor. Manager panel canvases use `overrideSorting`; test if your game uses Screen Space Camera or World Space canvases, because sorting behavior differs.
- No third-party assets, DOTween, or TextMeshPro required. Uses built-in legacy UI Text for drop-in compatibility.
- Uses legacy `Input.GetKeyDown` only if `ENABLE_LEGACY_INPUT_MANAGER` is active. If Input System only, add your own input action calling `Back()` and `ToggleDashboard()`.

## Installation — IMPORTANT

1. Extract ZIP. Merge its `Assets/ProjectSpark/UIManager` folder into your Unity project `Assets` folder.
2. In Unity, wait for compilation. Verify `com.unity.ugui` is installed.
3. Go to **ProjectSpark > UI Manager > Create Master UI Manager + Monitor**. This creates `Spark UI Manager [Core]` and `Spark MASTER UI MONITOR [F8]` scene objects and prepares the frame Sprite.
4. Select an **existing panel root** such as Shop or Wire Editor, then use **ProjectSpark > UI Manager > Add Panel Adapter To Selected**. Do not attach the adapter to buttons, text children, or the dashboard itself.
5. Assign a unique `Panel Id`, appropriate `Layer`, `Exclusive Group`, `Close Policy`, and `Visible On Start` on every adapter.
6. Drag all adapters into the core manager's **Registered Panels** inspector list. `Auto Discover Children` only scans descendants of the manager object; it will not discover panels elsewhere in the scene.
7. Make sure scene contains **one EventSystem**, then press Play. The generated monitor opens by default; F8 toggles with legacy input enabled. Try Open, Close, Toggle, Focus, and Minimize from the inspector.
8. Replace old direct `SetActive()` calls for the panel root **only when you have checked compatibility**. Existing panel-specific setup/cleanup logic should be invoked from its original scripts or via `SparkUIPanel.StateChanged`.

## Simple integration example

```csharp
using ProjectSpark.UIManagement;

// Existing toolbar callback:
public void OnShopButton()
{
    if (SparkUIManager.Instance != null)
        SparkUIManager.Instance.Toggle("Shop");
}

// Existing scene interaction gate:
public bool ShouldIgnoreGameplayPointer()
{
    var ui = SparkUIManager.Instance;
    return ui != null && (ui.IsGameplayInputBlocked || ui.IsPointerOverUI);
}
```

Use the exact adapter panel ID rather than the GameObject name. For tool panels that stay open during gameplay set `Blocks Gameplay Input` **false**; your wire/gameplay tools should still check whether the pointer is physically over interactive UI. Set it true for modal/fullscreen windows.

## Recommended ProjectSpark layer policy

Order from **back to front** (actual canvas sorting):

- `Background` (0): decorative backing / system background.
- `HUD` (10): passive gameplay HUD.
- `Main` (20): Shop, Inventory, Component Library, Level Select.
- `Tool` (30): Multimeter, Wire Editor, Component Inspector, Analyzer.
- `Overlay` (40): tutorial hints, selection tooltip.
- `Popup` (50): toast, notification, small popup.
- `Modal` (60): confirmations, destructive operations; blocks gameplay.
- `Fullscreen` (70): Pause, Loading, Settings takeover.
- `System` (80): top-level system UI.

**Note:** The generated art-direction image depicts an illustrative ordering. The implementation uses the above consistent back-to-front order, so modal/fullscreen appear over ordinary panels. If modal blocking of UI click-through is needed, include a full-screen raycast backdrop as part of the modal's existing prefab. `IsGameplayInputBlocked` is a QUERY, not an automatic patch to your gameplay scripts.

## Suggested panel settings

| Panel | Layer | Exclusive Group | Close Policy | Blocks Gameplay Input |
|---|---|---|---|---|
| Shop | Main | primary | SameGroup | true |
| Inventory | Main | primary | SameGroup | true |
| Component Library | Main | primary | SameGroup | true |
| Wire Editor | Tool | tool | None | false |
| Multimeter | Tool | tool | None | false |
| AI Assistant | Tool | tool | None | false |
| Delete Wire Confirmation | Modal | modal | SameGroup | true |
| Pause Menu | Fullscreen | fullscreen | SameGroup | true |

## Deliberately excluded from this first deliverable

- Draggable/resizable/dockable live windows; this package **does not** pretend Dock buttons are functional.
- Per-user saved layouts, robust history replay, modal raycast blocker injection, multi-scene persistent registration, and integration with existing ProjectSpark input sessions.
- The monitor is a functional **management console**, not a full pixel-for-pixel reproduction of the design image. Unity's uGUI text/icon spacing depends on actual font and asset settings.
- Built-in instrumentation does **not** fabricate UI performance metrics or system-health percentages.

## Integration safety

Do not delete any existing ProjectSpark scripts. Test in a copy/branch, beginning with two noncritical panels. Existing panels that must stay active while hidden can disable `Deactivate When Closed`; but their `Update()` methods will still run. Existing animations that also drive `CanvasGroup.alpha` may conflict with the manager fade: disable `Animate` for those panels.

Do not register the dashboard itself as an ordinary panel; its high sorting order keeps the management console visible. For a diegetic monitor display in 3D world-space, a RenderTexture or world-space Canvas variant should be implemented separately.
