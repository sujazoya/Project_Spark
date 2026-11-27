# Spark NEXUS

## Launch

Open **Spark Live** in the workshop and select the **NEXUS** tab. Switch to **Chat** any time to return to the existing conversation. Both tabs share one window, UI manager, and event system. Drag the header to move it, drag an edge or corner to resize it, or use **Layout** for scale, opacity, snap, lock, and minimize. NEXUS switches to compact navigation as the window gets narrower.

For authoring, open **Project Spark → NEXUS → Install/Upgrade Panel**, then **Bind Existing Spark Live** and **Validate Selected Scene**. MainMenu's normal installer binds the panel automatically at runtime. The editor tools import and validate the prefab and its native route. MainMenu needs one campaign manager, the existing Spark Live installation path and its executive, and a working event system. The prefab holds the glass material and reduced-motion default. Native robot and teaching cards depend on their corresponding components being present.

## Operate

- Browse domains, choose a command or click **Ask** to send a free-form question through Spark Live. Select a live component card to inspect it. Use a native terminal or device card to fill the exact ID when a command requires one.
- For reviewed construction, select **Build reviewed plan**, inspect the preview, and confirm. NEXUS requests Auto through Spark Live and waits for the engineering owner to acknowledge it. Assisted suggestions and approvals remain separate actions.
- **Pause / Resume** requests the existing wiring control. **Cancel** uses the current wiring or device owner. Confirmed wiring may remain after cancellation. Answers without correlated native completion evidence appear as unknown in the timeline, with any reply in the shared transcript.
- The instrument strip displays available solver or instrument readings. Resistance ratings and supply setpoints are labeled separately from measured quantities. Use native player tools to change controls and take measurements.
- **Low motion** freezes the animated dial while leaving the controls and state indicators active.

## Verify

- **Project Spark → NEXUS → Test Embedded Panel and LED** runs just the shared-window and Level 1 electrical checks.
- **Project Spark → NEXUS → Export Report** exports scene validation, the command registry, and the authored campaign audit to this directory.
- **Project Spark → NEXUS → Run Tests** runs catalog and asset checks. With MainMenu open in Edit Mode, it also runs workshop Play Mode checks and returns to Edit Mode. It uses the current scene without saving it.
- The campaign audit reads authored assets through the native campaign registry. Consult the report for layout, binding, instrument, and reviewed-plan availability.
- [Native integration map](Native-Integration.md) describes where the shared runtime references originate.

## Source and rollback

The NEXUS runtime, prefab, shader, material, and Editor menu are under `Assets/ProjectSpark/SparkV5/Pro/Integration/Nexus`. Native integration touches `SparkProLivePanel`, `SparkProExecutive`, `SparkEngineeringControlHub`, `SparkEngineeringDeviceOperator`, `SparkLiveAdjustableWindow`, and `SparkLevelSceneBindings`. The Play Mode test reuses the existing Level 20 resistor placement fixture.

To disable NEXUS installation, remove the `InstallNexusTabs` call added to `SparkProLivePanel.Bind`. For a full rollback, reverse only the NEXUS changes in the native files and remove the NEXUS folder and its Unity metadata. Preserve pre-existing edits in shared scripts. The integration is runtime-installed and requires no authored scene migration to roll back.
