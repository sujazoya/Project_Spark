# Spark Cortex implementation and verification

## Open Cortex

In MainMenu Play Mode, open **Spark Live** from the upper-right button or the Master Panel. Voice, text, and the existing Ask Spark entry point share the Spark V5 Executive.

- “Why isn't my LED working?” inspects the identified component, counts its direct links, checks for a current solver reading, and suggests the next observation. If several LEDs match, select the one you mean.
- “Is this voltage correct?” reads a solved two-terminal component and checks the current objective context. “What is the power?” uses voltage and current from the same solver revision.
- “What should I connect next?” uses the native objective results. “Explain level one hundred thirteen” uses the authored campaign and prerequisite records.
- “Teach me about closed paths” checks prior knowledge, introduces a diagram, asks for a prediction, and offers an isolated experiment before independent practice. Say “show me what you mean” for the teaching model, “pause the lesson” to pause, or “what should I review?” for scheduled revision.
- “Turn proactive coaching on” enables optional completion coaching. Its startup default is off. “Reset optional personalization” clears the formative lesson observations. Academy arithmetic practice and campaign progression have separate stores.

## Integration and authority

The existing character prefab, four LODs, facial controls, Spark Live prefab and MainMenu references are reused. The Executive owns conversation and in-flight requests. The verified local assistant supplies specialist drawings and existing lesson answers.

Cortex has 16 typed tools. A request can use each tool once and can use at most five tools in total. Results include time, session, request, entity, source, topology and solve revisions, and unavailable-data notes. The circuit, solver and level evaluator provide live observations. Curated teaching examples use idealized assumptions. The isolated teaching circuit creates its wires and controls its switch through the native APIs, then deactivates and destroys the temporary object.

The catalog links campaign order, chapter access rules, objective bindings, available layout references and instruments, concept tags, and related authored learning assets. Teaching asset links use matching authored terms as suggestions. Coverage is 130 unique numbered game definitions. Level 113 is The Redundancy Citadel, between 112 and 114, with the fictional RDX21-EDU-1 game circuit. The learning record keeps a bounded history of recognition, understanding, measurement and application evidence and revision dates alongside migrated Academy practice.

Level 120, The Axis Forge, remains the motherboard qualification mission. Its six game objectives and authored approval record are indexed. The certification source disables issuance pending approval of the RAX-28 schematic and six-axis mechanical model. A dedicated theory assessment and mandatory practical, diagnostic and safety records are still needed for actual certificate issuance. Positive and negative policy fixtures exercise the qualification conditions.

## Optional providers

Cortex runs offline by default. Enable the existing explanation adapter in the Executive Inspector for the explicit “model explain <topic>” route. The Ollama option expects a configured local model and loopback /api/chat endpoint. The teaching gateway uses the existing /ask contract and accepts loopback HTTP or a configured HTTPS gateway. Set OPENAI_API_KEY and SPARK_MODEL on the server. The provider receives the general concept request and its curated reference. Provider output appears as supplementary explanation. The deterministic Executive controls tools and game claims.

Provider contracts were checked against [Ollama chat](https://docs.ollama.com/api/chat) and the [OpenAI Responses API](https://developers.openai.com/api/reference/resources/responses/methods/create).

## Windows voice deployment

Spark Live reuses a ready loopback Vosk service or starts an owned Windows process, checks readiness and recovery, and shuts down its own process when disabled. The Windows Editor can use the sibling SpeechServer environment. A Windows player loads the packaged speech service from StreamingAssets.

Build the service with Python/SpeechServer/package_windows.py, requirements-windows.txt and an extracted Vosk model. Set SPARK_WINDOWS_SPEECH_PACKAGE in the Unity build process to stage that package into the Windows player output. The package smoke test uses Piper-generated audio, checks transcription and confidence, and stops the owned process. Voice uses turn-based recording and playback. Android still requires a reachable HTTPS recognition endpoint and compatible speech output.

## Validation

Open **Project Spark → Spark V5 Pro → Cortex** for the architecture audit, live diagnostic window, isolated lab builder or **Run Cortex Tests**. The diagnostic window shows provider, intent, active lesson, evidence source and revisions, response latency and character state. Ordinary game logs omit dialogue and private learner records.

- Unity 6000.3.19f1 Play Mode used the saved MainMenu. Tests exercised Levels 1, 24 and 113, native evidence, stale rejection, conversation routing, adaptive lessons, the isolated switch experiment, profile migration, exam restrictions, provider timeout and cancellation, hologram isolation, Vosk readiness, Piper playback and interruption.
- The packaged Windows speech service recognized the synthesized “please show me the voltage across the resistor” fixture at 0.99 confidence and shut down.
- Actual Ollama and cloud inference, physical microphone capture, native Android speech, full-duplex audio, a packaged Unity Windows build and Android performance remain unverified. Player tools retain ownership of live assembly, wiring and instrument changes.

See Logs/SparkCortexVerification.txt, Logs/SparkSpeechPackageVerification.txt, Logs/Level113Acceptance.txt and Generated/KnowledgeAudit.txt.
