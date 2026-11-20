# L108 AutoLayout Upgrade

- Implements Level 108 DFX-16 full-hierarchy builder atop existing Universal Generator V2, without installing duplicate core classes.
- Adds normalized source-derived manifests for V2 importer compatibility; does not change original CSV/JSON.
- Fits imported bare-PCB to workshop 10.13×7.51 Unity units and compensates separately-sized completed visual reference.
- Generates source-backed placement, copper/via task, diagnostics, W157, objectives and fault scaffolds.
- Maintains hard SCHEMATIC_REQUIRED blockers on all unassigned electrical fields.
- Adds safe PowerShell installer that backs up same-name scripts/recipes and preserves earlier levels.
- No Unity project compilation or Play Mode tests have yet been run.
