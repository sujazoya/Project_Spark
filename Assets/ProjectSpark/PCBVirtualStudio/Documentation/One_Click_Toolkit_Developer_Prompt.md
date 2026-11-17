# Unity integration prompt — Project Spark AAA Virtual Motherboard Repair UI

Integrate the supplied `Assets/ProjectSpark/PCBVirtualStudio` scripts into the existing Unity Project Spark game. DO NOT generate FBX instrument models. The tools are UI screens, world-space Canvas cards, click/touch repair actions and procedural contact-point VFX. Preserve Levels 1 through the current level, existing room table, shop and `LevelGamePlayManager`.

1. Compile and inspect the supplied UGUI Runtime/Editor source. Fix only project-specific Unity incompatibilities. Run the one-click menu and confirm the scene loads.
2. Place the world-space Canvas above the existing large table. It should look like premium sci-fi electronics software, not a cluttered array of physical instruments.
3. Bind physical existing board MeshCollider/Collider targets to `SparkPCBPhysicalClickBridge` and `SparkVirtualPCBTarget`. Assign stable target IDs from your actual board and connect the raycaster.
4. Implement a concrete `ISparkPCBRepairAuthority` against the actual Project Spark electrical graph, PCB layers, netlist, components, failure systems and measurement APIs. Do not invent interfaces that conflict with existing code; adapt carefully after inspection.
5. For each repair action, validate target type, correct tool, prerequisite steps, mechanical/thermal state, and commit a real board change only on authoritative success. Emit correct VFX at the hit position.
6. Replace demonstration waveform/voltage displays with live solver-driven instrument data. Distinguish commanded, measured, stale and estimated values.
7. Integrate tool statuses and repair evidence into objectives without granting any success merely from visuals or UI clicks.
8. Test all tools with mouse and touch. Confirm the shader material compiles in the chosen pipeline. Verify events survive prefab/scene save and domain reload.
9. In tests, deliberately break a trace, via, solder joint and removable safety jumper. Verify actual topology/measurement behavior before and after repair and explicit safety reset.
10. Keep the scene render efficient: minimal Canvas rebuilds, 20 Hz scope refresh, capped UI VFX burst count, pooled assets if the final game needs more targets.

Deliver a fully runnable World-Space virtual inspection system and project-specific electrical adapter, then run Unity editor compilation and play-mode tests before claiming production readiness.
