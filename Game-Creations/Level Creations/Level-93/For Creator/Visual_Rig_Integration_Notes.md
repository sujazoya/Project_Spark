# Level 93 visual-pivot integration

`SparkLevel93VisualRig` is only an animation facade. It resolves `ANIM_` pivots by name and can accept measured angles or offsets from future validated gameplay code. It does not create circuits, sample dynamics, protected instruments or objectives.

The 92 pivot records in `Animation_Rigs.csv` include parent model, nominal world coordinates and intended axis. All pivots are imported as parent nodes with independent mesh children. Authoritative runtime scripts should map actual stage position, gantry revolution, glovebox latch, sample cartridge, vacuum pump rotor and analyzer status to these art transforms.

When replacing visual art, call `Rebind()`. Never drive `LevelGamePlayManager` from this component.
