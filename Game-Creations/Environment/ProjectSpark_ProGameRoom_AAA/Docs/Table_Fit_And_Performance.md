# Professional Level Presentation Table — Design and Integration

## Dimensional plan

| Part | Dimensions / location |
|---|---|
| Room bounding envelope | 18.20 m W x 14.40 m D x 6.08 m H |
| Center presentation table | 9.90 m W x 5.40 m D x approx. 1.30 m tabletop |
| Full-table preview fit region | 8.55 m W x 4.06 m D x 2.30 m H |
| Left / middle / right preview bays | 2.60 m W x 3.75 m D x 1.75 m H each |
| Full-stage center | (0, 1.34, 0.30) relative to room root |
| Bay centers | (-2.91, 1.34, 0.30), (0, 1.34, 0.30), (+2.91, 1.34, 0.30) |
| Chairs | (-6.06, 0, 3.86), (+6.10, 0, 3.90), (0, 0, -4.35) |
| Dustbin | (+7.85, 0, -3.50) |
| Planter | (-7.85, 0, -4.65) |

## Safe scaling algorithm

1. Read the selected source GameObject's static mesh renderers (including the original hierarchy transforms).
2. Compute the aggregate geometric bounds in the selected root's local space.
3. Fit its X, Y and Z dimensions to the requested bay while preserving the original proportions.
4. Ground the model visually at the table-top height and center it in the available bay.
5. Combine static mesh subparts by material into render batches to reduce draw calls.
6. Never clone gameplay scripts, terminals, physical colliders, level definitions or objectives.

**This is a showcase fit, not a relocation or rescaling of actual gameplay.** Complex levels remain authoritative only in their own original scenes.

## Important cases

- FBX and GLB mesh coordinates use different units. Use the Unity-imported FBX once, **not both**, and verify its imported size.
- Original level art packages can be centered at a common 0,0,0; the visual clone reads each mesh transform and computes actual bounds rather than assuming all models fit one size.
- No permanent decorative landscapes, mountains, miniature levels, or extra machinery were added to the table. The showroom is intentionally minimal.
- Separate preview holder is preserved when the room's disposable visual root is refreshed.
- For large sources, readable mesh data is needed for mesh combination. Use the level's assembled showcase art or a simplified static proxy for extreme scenes.
- Reflection probes, high-end post processing and final lightmaps must be tuned in the user's active Unity render pipeline.
