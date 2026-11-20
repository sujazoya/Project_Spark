# Level 102 — Pulse Architect

The original Level 95 kit is the source for Level 102. The Unity definition ID is `level_102`. Source model names, pad IDs and route IDs are retained.

- The Unity layout uses 54 converted modular meshes from the supplied lab kit and its 126 shared PBR maps. The existing Project Spark room supplies the main bench. The separate board-only package remains a source/reference option.
- The 36 BOM entries are 35 fixed components plus W049. Normal Mode fits the fixed components at startup. Challenge Mode starts with them hidden. Both modes require 48 copper tasks, ten vias and a separate physical W049 connection.
- W001–W048 use explicit circuit edges. Each via uses two route segments and a switched interlayer path. Artwork test-point rings are separate from the graph terminals.
- C1 is 10 nF normally. F01 connects an additional 12 nF in parallel, reproducing the 22 nF misvalue. RV1 adjusts the solved PWM reference and does not alter frequency. F02 opens V06 and removes downstream edges from the timing model. F03 adds a Q1 gate–drain connection and the supervisor inhibits the output.
- The scope and logic analyzer use a deterministic RC timing and counter model gated by the solved rail and real route state. PWM qualification checks the actual DC on/off switch states and the solved RV1 duty reference. The visual PWM traces illustrate timing reconstructed from those samples. This is an educational model rather than a SPICE simulation at 256 Hz.
- The multimeter reads the native solver at the selected test point. Bench supply, probe harness, scope and analyzer controls are available from labeled housings and the bottom control deck.
- The guided routes enforce the expected endpoints and required via. Freeform fabrication DRC, chip-level pinout validation and target-hardware performance profiling are outside this implementation.

The Unity editor probe and screenshot are written to Logs/SparkLevel102Check.txt and Logs/SparkLevel102Complete.png in the project.
