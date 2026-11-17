# L95 Visual Rig — Unity binding guidance

All 48 named pivot nodes are present in the FBXs and GLBs, including 36 `ANIM_<REF>_INSTALL` component presentation transforms.
The 48 pivots are *art transforms*, not animated GameObjects or gameplay controllers. Every pivot axis is recorded in `Data/Animation_Pivots.csv`. Do not hardcode all rotation about Y.

### Installation / runtime synchronization
- Board art is enlarged 12× while virtual physical positions remain 120×80mm. Component installation must snap to physical footprint coordinates, then drive visual `ANIM_<REF>_INSTALL` without changing net coordinates.
- Initial assembled full-scene GLB and FBX are **showcase** art, not the initial gameplay state.
- The Unity art assembler hides `03_Components_36` initially. A future component placement system should show / move each selected mesh from inventory when actual component placement succeeds.
- `ANIM_W049_INSTALL` is purely a visual pivot: the actual critical connection must be changed in Spark electrical topology.
- Test instruments can animate knobs, but only measured voltage/frequency/duty/current may populate SCADA.
- A probe attachment must affect which actual net is sampled; waveform display assets do not imply an active probe.
