# Spark NEXUS native integration

| Owner | Existing path and integration surface |
| --- | --- |
| Spark Live Ask | `SparkV5/Pro/Integration/SparkProLivePanel.cs` wires the Ask button and text submit to `SubmitText`, which calls `SparkProExecutive.Ask`. Voice recognition also calls `Ask` on the executive. |
| Intent and conversation | `SparkProExecutive` routes exact engineering actions, native controllers, Cortex, and the verified local assistant. `AnswerReady` and `ConversationChanged` expose the existing conversation. |
| Wiring and Super AI | `SparkEngineeringControlHub` owns mode, review revisions, approvals and execution. It exposes `StateChanged`, mode and phase. Native connections are owned by `SparkCircuitSystem`. |
| Device controls | `SparkEngineeringDeviceOperator` resolves live ports and revalidates power, state and approvals before dispatch. Its confirmation uses the native device and a fresh solver result. |
| Level and targets | `LevelGamePlayManager` owns evaluation. `SparkLevelLayoutLoader.LayoutLoaded/LayoutUnloaded` identify same-scene layout transitions. `SparkLevelSceneBindings` resolves authored terminal and object identities. |
| Selection | `SparkSelectionController` emits selection changes. |
| Electrical | `SparkElectricalSolver` emits solve completion/failure and provides revisions. Components hold their solved electrical state. `SparkMultimeter.ReadingChanged` carries native range, mode, stability, and units. |
| Presentation | `SparkProLivePanel.Install` owns the Spark Live launch surface and workshop UI registration. NEXUS is a tab on that surface. `SparkLiveAdjustableWindow` owns the shared bounds, drag, resize, opacity, and collapse behavior. |
