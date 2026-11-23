PROJECT SPARK — missing level artwork recovery (001–029)
Requires the already installed V8 Neon consolidated system and its Visual Architect.
Close Unity. Back up the two target files if they exist. Copy Assets into the project root, replacing only SparkAAAVisualArchitect.cs from this package and adding SparkEarlyVisualRecoveryWindow.cs. Keep existing .meta files.
Tools > Project Spark > AAA Level Visual Architect > 05 Recover missing level visuals.
Scan range (defaults 1–29). Review exact prefab/model candidate paths and objects. Choose complete hardware layouts, never individual unrelated components. Approve checked sources. Generate missing visuals in range. Open VisualAudit.txt for results.
If no complete layout source is discoverable, assign the actual saved prefab/model. For a scene-built workbench, save its relevant mesh hierarchy as a prefab outside AAAArchitecture/Generated, then assign it. The studio copies static meshes, not behaviour scripts. No scene editing is performed by this tool.
The original renderer exports five images per approved level: Thumbnail, Banner, Portrait, LoadingScreen, Architecture. Existing PNGs are never overwritten by this recovery command; missing sprites are imported at the normal paths. The catalog is refreshed. Unapproved, blocked, invalid and orphan-metadata cases are reported rather than guessed.
Important: this only protects the recovery operation. Other original Visual Architect commands can replace generator-owned images or explicitly approved foreign images; keep automatic refresh off if you want to freeze custom artwork. Partial sets containing manual art remain marked unowned/stale rather than silently claiming ownership.
Offline source checks only; Unity compile and GPU rendering not tested here.
