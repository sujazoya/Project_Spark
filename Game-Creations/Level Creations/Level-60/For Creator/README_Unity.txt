PROJECT SPARK / LEVEL 60 — THE GRID MUST SURVIVE
MODULAR PROCEDURAL 3D ART ASSETS — UNITY IMPORT GUIDE

CONTENTS
Models/ — 28 separate binary FBX 7.4 models plus one assembled art reference FBX
GLB/ — 28 individual colored GLB backups plus one assembled GLB
Previews/ — 3D render and individual model contact sheet
Animation_Rigs.csv — animation pivot transforms and coordinates
Model_Manifest.json — exact filenames, mesh part counts, triangle counts
Unity_Optional/SparkLevel60ArtPlacementEditor.cs — art-only automatic placer
Source/ — model generation source code

INSTALL
1. In Unity, create Assets/ProjectSpark/Level60/Art/Models and copy the individual FBXs there.
2. Inspect imported model size: art units represent 1 meter when import scale is 1.
3. Copy Unity_Optional/SparkLevel60ArtPlacementEditor.cs into an Editor folder.
4. Select the existing Level 60 layout root in the Hierarchy.
5. Run Tools > Project Spark > Levels > 60 > Place HD Art Under Selected Layout.
6. The tool instantiates the 28 individual models under L60_HD_VISUAL_ART using the shared origin.
7. Do not also enable the assembled showcase art, or duplicate geometry will appear.
8. Connect animation pivots to your own authoritative simulation state.
9. Author functional SparkTerminal components and exact W01-W46 wiring topology separately.

ART CONVENTIONS
X = bench width; Y = height; Z = front controls.
The animations use named FBX transform nodes. Breaker handles use the same pivot name but belong to separate model roots.
FBX materials are colored regions; final Unity URP/HDRP shaders, transparency, PBR textures, lighting and collisions need production setup.

IMPORTANT LIMITS
This package is ART ONLY. It includes NO functional 72 terminals, 46 wire manifest, AC simulation, protection, energy management, renewable physics, or level objectives.
The exact 46-wire topology and actual Unity importer/Play Mode behavior are untested.
