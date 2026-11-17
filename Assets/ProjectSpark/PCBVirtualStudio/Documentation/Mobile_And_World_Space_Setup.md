# Mobile & World-Space Setup

1. Confirm an EventSystem exists (factory automatically creates one when absent, preferring the New Input System UI module when available).
2. If your project is configured **New Input System Only**, ensure its EventSystem has an enabled `InputSystemUIInputModule`. The editor builder intentionally does not alter an existing EventSystem.
3. Set the main Camera reference on the world-space Canvas where appropriate. Do not use the SceneView camera as a runtime camera.
4. Confirm `GraphicRaycaster` on the Canvas and target `Graphic.raycastTarget=true` on each interactive target/button. Decorative layers have raycast disabled.
5. For actual 3D PCB geometry: add colliders and `SparkPCBPhysicalClickBridge` to relevant existing components, and a `PhysicsRaycaster` to the gameplay Camera.
6. Keep world-space UI clear of physical obstacles and verify touch hit areas after root scaling. The default tool targets are large (138x58 logical pixels); adjust for device DPI if necessary.
7. The generated studio measures roughly 4.84 x 2.73 Unity units at scale 0.0022. Its parent/root can be moved and scaled to match the Project Spark game room. The physical PCB manufacturing size is unaffected.
8. Use single tap for all demonstration tools; no long-press gestures or precise drag paths are required in this kit.
9. UI VFX play in an overlay Canvas RectTransform and do not use HDRP/URP particle prefabs.
10. For accessible input, add alternate text-only action buttons and contrast modes when advancing to production.

## Compatibility caution

This is a source-code delivery targeting Unity 2022.3 / Unity 6 uGUI conventions. It has not been compiled in a running Unity Editor here. The optional ShaderLab hologram effect must be checked on your project's exact pipeline before shipping; default UI materials remain usable if the shader is incompatible.
