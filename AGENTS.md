# ProjectSpark agent instructions

## Purpose and learning design

ProjectSpark is an educational electrical simulation and repair game.

- Starting levels must be very easy. Increase difficulty gradually as the player learns.
- Introduce basic component recognition, simple connections, switching, and observable outcomes before complex diagnosis.
- Avoid advanced measurements or calculations in early levels. Introduce instruments, quantitative reasoning, and multi-step repairs only after their prerequisites have been taught.
- Keep objectives, instructions, and feedback appropriate to the player's current learning stage.

## Read before changing

Read `Assets/My_Assets/_Project_Spark/Scripts/README.md` for the gameplay foundation and inspect the relevant current source, callers, adapters, and serialized references before making changes.

- Inspect existing implementations before introducing replacements or parallel systems.
- Prefer extending the existing authoritative subsystem through its APIs and narrow capability interfaces.
- Similar class names can exist in different namespaces. Confirm the full type, actual references, and intended integration before choosing an implementation.
- Treat setup guides, historical reports, and architecture snapshots as reference material. Check their claims against current source; declarations alone do not prove scene integration or working runtime behavior.

## Domain ownership

Preserve the responsibilities described in the gameplay README:

- `SparkGameplayController`: world target acquisition, UI priority, input ownership, and interaction-session coordination.
- `SparkToolController` in the gameplay foundation: active tool ownership, lookup, and activation.
- `SparkTool`: tool lifecycle and user intent.
- `SparkInteractionSession`: the lifetime of one continuous interaction.
- `SparkElectronicObject`: object identity, common operational state, hover/selection state, and generic interaction validation.
- `SparkTerminal`: connection endpoint compatibility and connection-capacity validation.
- Narrow capability interfaces: movement, rotation, connection, and Wire/Measurement/Scan integration.

The existing Wire, Measurement, and Scan systems remain authoritative. Do not duplicate their topology, measurement solving, or scan implementations in tools, UI, or new controllers.

Current source also establishes these boundaries:

- `SparkCircuitSystem` owns circuit connections and topology.
- `SparkElectricalSolver` owns electrical solving through its network graph, solve context, solver registry, and device-specific solver definitions. The registry here is an existing electrical-domain mechanism, not a general gameplay registry.
- Engineering components expose device data and own their domain-specific operational state; they receive authoritative solved electrical state. The gameplay layer does not solve circuits.
- `LevelGamePlayManager` coordinates level definitions, scene bindings, layout loading, evaluation, and completion/failure. Progression tracking remains separate from circuit solving.
- UI, displays, visual effects, and teaching systems consume authoritative state. Do not infer electrical truth from visual appearance or allow presentation to become the source of truth.
- Tutorials and AI explanations must ground claims about live circuit correctness in verified gameplay or solver results. Present illustrative or offline demonstrations as such.

For new engineering objects, derive from `SparkComponent` or `SparkInstrument` where appropriate, implement only supported capabilities, and delegate topology, electrical solving, measurement, and scan truth to their existing owners. Do not add object-type branches to the gameplay controller just to support a new object.

Do not introduce an event bus, service locator, DI container, generic repository, command bus, entity framework, massive general registry, or global gameplay singleton into the gameplay foundation. This is a boundary for new foundation changes, not an instruction to remove existing domain-specific infrastructure elsewhere.

## Evidence and validation

- Distinguish verified defects from suspected issues. For a verified defect, state the concrete evidence and affected behavior. For a suspicion, describe the uncertainty and the check needed to resolve it.
- Do not equate a source declaration with a component being configured, present in a scene, or functioning at runtime.
- Choose validation appropriate to the change. Inspect serialized references and scene/prefab integration when relevant; compile, test, or use Play Mode when needed and authorized.
- Report what changed, why, and what validation was actually performed. Explicitly state any relevant checks that were not run and remaining limitations.
- Do not claim successful compilation, passing tests, working gameplay, or a successful build without having performed the corresponding validation.

## Scope discipline

Keep changes within the user's requested scope. Do not opportunistically replace systems, regenerate assets, or alter scenes, packages, settings, or unrelated files. A documentation-only task must remain documentation-only.
