# LEVEL 84 UNITY INTEGRATION PLAN

- Use the 76 numbered FBXs at a single common origin and inspect imported bounds; the assembled FBX or GLB is a standalone reference, **not** additional geometry to combine with all 76 modular assets.
- Import 12 PBR procedural reference materials (36 PNG textures); original geometry has generated material colors but requires production UV authoring for reliable full texture mapping.
- Each `ANIM_*` group is a real parent node owning mesh parts. Each `SOCKET_P###` is a mesh-owning visual socket transform, **not** a functional SparkTerminal.
- Instantiate gameplay equipment separately, connect 142 real user-created wires via real SparkTerminal objects, and reconcile visual sockets with signal-domain rules before creating functional terminals.
- Use two electrical domains: educational 5 V control graph vs separate modeled spacecraft motor/torquer power.
- Model a quaternion rigid body, four wheel speeds and torque/momentum conservation, B-field-dependent magnetic torque m x B, gyro bias and uncertainty, star-tracker acquisition, solar-wing flexible modes, and two actual optical ground passes.
- A ground-test gimbal is an animation visualization, never a source of fictional free flight torque.
- W097 is the unique trusted primary star-tracker absolute-attitude feedback to the estimator, with timestamp/validity; disconnect must inhibit fine pointing, latch, and require explicit reset and new qualification.
- The final authoritative six objectives must be certified by the real Project Spark LevelGamePlayManager only. No runtime solver, working player wires, objectives or Unity scene/prefab are included in the art kit.
- Generated Unity helper scripts are optional Editor visualization tools; test in the user's actual Unity version/render pipeline before claiming compilation.
