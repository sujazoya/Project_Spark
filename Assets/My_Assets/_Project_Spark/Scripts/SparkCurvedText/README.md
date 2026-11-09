# Spark Cylinder Text
Editable 3D TextMeshPro labels for cylindrical components. No user-created images, render textures, extra cameras, or custom shaders. Uses your existing TMP font atlas/material.

## Installation
Copy SparkCurvedText into Assets. Keep the Editor folder intact. Requires TextMeshPro (Unity 6's uGUI package) and imported TMP Essentials. No assembly definitions included: if adding to a project with custom asmdefs, reference Unity.TextMeshPro and isolate Editor code in an editor-only assembly.

## Your diode
1. Select the diode in the Hierarchy. Use GameObject > 3D Object > Spark Cylinder Label, or add SparkCylinderText to your existing 3D TextMeshPro object. Do not add it to TextMeshProUGUI.
2. Type 1N5819 in TextMeshPro. Use its normal font, color and size controls.
3. Choose Across Height. Align the label's local red X axis with the diode's leads. Its blue +Z axis must point INTO the diode; the text front faces local -Z.
4. Place the label's origin at the middle of the desired surface marking. The cyan circle shows the assumed cylinder cross-section; its center must coincide with the diode's axis.
5. Set Radius to the diode radius in TEXT LOCAL units. Example: world radius 0.05 and uniform text lossyScale 0.1 means Radius = 0.5. Use font size to resize lettering; changing transform scale also changes the cylinder fit.
6. Adjust Surface Offset slightly if needed. Leave Prevent Surface Clipping enabled. Keep each glyph below roughly 30 degrees of cylinder arc for close contact.
7. Use Angle to move around the cylinder, Layout Offset X to slide along its length, and Layout Offset Y to move around its circumference. Move/rotate the whole label for initial placement.
8. Parent it to the diode and save your scene or prefab variant. Inspector and radius-handle edits support Undo and prefab overrides.

## Controls
- Across Height: cylinder axis is local X. Suitable for text running along a diode.
- Across Width: cylinder axis is local Y. Suitable for a word running around a cylinder.
- Center Visible Text: centers visible glyph geometry independently of TMP alignment. Disable to preserve the TMP layout origin.
- Show Guides: cyan cylinder circle, axis and radius handle in Scene view.
- Disable/remove component: restores flat TMP text.
- Runtime: use SetRadius(value), SetAngle(degrees), or edit TMP.text/SetText normally. Refresh() explicitly rebuilds when needed.

## Scope and limitations
This is cylindrical vertex deformation, not automatic arbitrary-mesh snapping. The script does not inspect the diode mesh or infer its radius. It supports standard visible TMP glyphs, fallback-material glyphs and inline sprite quads. Rich-text underline/strikethrough geometry is not bent; avoid those tags. Do not combine with another TMP vertex deformation effect. Use the default unlit SDF material; lit normals are not deformed.

TMP characters remain four-vertex quads. Clipping compensation raises their chords outside the cylinder but can leave a small gap near the glyph edges. Large letters or broad wrap angles require a subdivided text renderer for exact surface conformity; the Inspector warns above 30 degrees per glyph. Keep total wrapping below one circumference to avoid overlap. Positive uniform transform scale is required for a circular fit. Curved CPU mesh bounds are refreshed by TMP's normal mesh upload.

## Validation
Checked the cylindrical mapping and chord compensation numerically, plus package structure. Unity is not installed in the build environment, so Unity compilation and scene rendering have NOT been tested. In your project, verify: editing text, font fallback, changing radius/angle, Undo, toggling the component, saving/reopening a prefab, entering/exiting Play mode, and camera visibility near screen edges.
