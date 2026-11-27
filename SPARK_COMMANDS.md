# Project Spark Live Command Reference

Enter these phrases in Spark Live. Replace placeholders such as `<number>`, `<terminal ID>`, and `<device ID>` with actual values. The tables include explicit commands, their aliases, and examples for features that accept natural-language questions.

## Auto Draw and circuit construction

For hands-free construction, load the level, enter `spark auto mode`, then enter `hey Spark draw current level`. The loaded level needs an approved construction plan and a ready workbench. Use `spark drawing status` to check progress or `spark explain current construction` to see the plan and the latest available objective results.

For step-by-step assistance, enter `spark assisted mode`, then `spark suggest next wire` and `spark approve suggested wire`. Repeat as needed. In Assisted mode, `draw current level` also suggests the next wire.

| Action | Command phrases |
| --- | --- |
| Select Manual mode | `spark manual mode`, `manual mode` |
| Select Assisted mode | `spark assisted mode`, `assisted mode` |
| Select Auto mode | `spark auto mode`, `auto ai mode` |
| Draw or wire the loaded level | `draw current level`, `spark draw current level`, `spark draw this level`, `wire current level`, `spark wire current level`, `spark auto draw` |
| Continue, pause, resume, or stop drawing | `spark continue drawing`, `spark pause drawing`, `spark resume drawing`, `spark stop drawing` |
| Check drawing progress | `spark drawing status`, `spark explain current construction` |
| Check engineering status | `spark engineering status`, `engineering status`, `auto wiring status` |
| Preview a plan | `spark preview practice plan`, `preview wiring plan`, `preview approved wiring plan` |
| Suggest the next connection | `spark suggest next wire`, `spark suggest next step`, `spark suggest the next wire`, `spark inspect the next connection` |
| Locate the suggested source terminal | `spark show me where the source terminal is` |
| Explain a failed connection | `spark explain why this connection failed` |
| Approve the suggested connection | `spark approve suggested wire`, `spark perform approved step`, `spark connect the approved wire` |
| Run an approved plan | `spark start approved plan`, `auto wire practice level`, `spark wire this practice level` |
| Pause or resume automation | `spark pause automation`, `pause auto wiring`, `spark resume automation`, `resume auto wiring` |
| Cancel automation | `spark cancel automation`, `cancel auto wiring`, `spark stop`, `cancel wire`, `stop wiring` |
| Reload approved engineering plans | `spark reload engineering plans` |
| Turn engineering narration on or off | `spark engineering narration on`, `spark engineering narration off` |

When the engineering hub handles `confirm wire`, `spark connect wire`, or a direct request such as `draw real wire from <source ID> to <destination ID>`, it directs you to Assisted approval or an approved Auto plan.

[Engineering command handlers](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkEngineeringControlHub.cs#L743)

## Whiteboard, workbench, and objectives

To see a teaching drawing, use `show current level on whiteboard`. For a specific level, load it first, then enter `draw level <number>`, for example `draw level 130`. Drawing availability depends on the loaded scene and its teaching setup.

| Action | Commands or example phrases |
| --- | --- |
| Show the level or circuit on the whiteboard | `show current level on whiteboard`, `show live circuit on whiteboard`, `draw objective map in 2d`, `draw level <number>` |
| Highlight the current workbench | `show current level in 3d`, `show workbench highlight`, `clear highlight` |
| Point to a selected component | `point at this`, `point at selected component`, `show me this component` |
| Find a terminal | `show me the positive terminal`, `show me the negative terminal`, `show me the other terminal` |
| Check connections | `which terminals are connected`, `show circuit connections` |
| Check inventory | `inventory`, `show my inventory`, `what is in my inventory` |
| Check the active tool | `what tool am i using`, `which tool is active` |
| Check the power supply | `power supply settings`, `what is this power supply set to` |
| Check objectives and progress | `which objective is pending`, `what should i connect next`, `show level progress`, `give me a hint` |
| Diagnose a level or objective | `diagnose level`, `explain pending objectives`, `show objective evidence`, `audit bindings` |
| Ask for measurements | `what is the voltage`, `what is the current`, `what is the resistance`, `what is the power` |
| Check or export the current mission | `spark inspect mission`, `spark mission status`, `spark export mission report` |
| Check the campaign or current chapter | `which chapter`, `current chapter`, `current campaign`, `explain the current mission`, `explain current mission` |
| Look up authored level information | `catalog coverage`, `catalog level <number>` |

Spark also accepts questions such as `what is this component`, `why is this LED dark`, `what happens if I open the switch`, `show the schematic`, `find a level about switches`, `explain level 20`, `what equipment can I use`, `which levels have I passed`, and `what should I review`. Responses depend on the current selection, workbench, and available game data.

[Spark Live routing](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkProExecutive.cs#L252) · [Question patterns](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkCortex.cs#L151)

## Robot movement and device operation

| Action | Command phrases |
| --- | --- |
| Take off | `spark take off`, `take off`, `spark fly` |
| Hover | `spark hover` |
| Land | `spark land`, `land spark` |
| Fly to the selected item | `spark fly to selected`, `fly to selected` |
| Check flight status | `spark flight status`, `flight status` |
| Check arm status | `spark arm status`, `spark arms status` |
| Retract the arms | `spark retract arms`, `spark retract arm` |
| Reach a terminal | `spark extend right arm to <terminal ID>`, `spark extend left arm to <terminal ID>`, `spark reach terminal <terminal ID>` |
| Set maximum arm reach | `spark set arm reach <metres>` |
| Find device IDs or check device status | `spark inspect devices`, `spark device status` |
| Propose a switch action | `spark propose device <device ID> on`, `spark propose device <device ID> off` |
| Approve or cancel the proposed action | `spark approve device action`, `spark cancel device action` |

To operate a switch, start with `spark inspect devices` to find its exact ID. Enter the applicable proposal, then enter `spark approve device action`. Check the response or use `spark device status` to see the result.

[Flight handler](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkV5FlightController.cs#L834) · [Arm handler](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkTelescopicArmCommands.cs#L10) · [Device handler](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkEngineeringDeviceOperator.cs#L316)

## Teaching, practice, demonstrations, and voice

| Action | Commands or example phrases |
| --- | --- |
| Start practice | `practice`, `practice <topic>`, `give me a practice question` |
| Ask for a practice hint | `practice hint`, `hint` during practice |
| Submit an answer | `answer <answer>` |
| Control a lesson or practice | `pause the lesson`, `continue our lesson`, `next lesson step`, `cancel the lesson`, `stop practice` |
| Ask Spark to teach or explain | `teach me <topic>`, `define <term>`, `explain it another way`, `give me an analogy` |
| Use the optional online explanation service | `online explain <topic>`, `model explain <topic>` |
| Open a learning report | `my learning report`, `my practice report`, `what did i learn previously` |
| List, open, or close 3D models | `teaching models`, `show teaching model <name>`, `show <name> in 3d`, `close teaching model` |
| Start a diode or LED demonstration | `demonstrate diode polarity` |
| Start a switch demonstration | `demonstrate a switch` |
| Start a multimeter demonstration | `demonstrate voltage measurement` |
| Advance a demonstration | `next demo`, `continue demo`, `advance demo`, `proceed demo` |
| Repeat a demonstration step | `repeat demo`, `retry demo`, `redo demo`, `show again demo` |
| Check demonstration status | `demo status`, `demo progress`, `where are we in the demo` |
| Stop a demonstration | `stop demo`, `end demo`, `cancel demo`, `close demo` |
| Interrupt Spark's speech | `stop`, `stop speaking`, `wait`, `wait a second`, `wait stop speaking` |
| Mute the microphone | `mute microphone` |
| Toggle optional coaching | `turn proactive coaching on`, `turn proactive coaching off` |
| Clear optional lesson observations and conversation references | `reset optional personalization` |

The demonstration controls accept both `demo` and `demonstration`. For example, `next lab demonstration step` also advances a demonstration. Online explanations depend on the service being enabled and available.

[Voice, settings, and practice routing](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkProExecutive.cs#L252) · [Demonstration phrases](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkLabDemoIntent.cs#L14)

## Lab recording and earlier agent commands

The current engineering hub takes priority when a phrase also exists in the engineering command list. These commands depend on the corresponding lab component being present.

| Action | Command phrases |
| --- | --- |
| Start wiring recording | `start wiring recording`, `record my wiring` |
| Stop wiring recording | `stop wiring recording`, `finish wiring recording` |
| Reload recorded or reviewed wiring plans | `reload wiring plans` |
| Check wire status | `spark wiring status`, `native wire status`, `real wire status` |
| Request a wire preview | `preview wire from <source ID> to <destination ID>` |
| Confirm, discard, or undo a wire | `confirm wire`, `execute wire`, `cancel wire`, `discard wire`, `undo last AI wire`, `undo last Spark wire` |
| Preview an assisted plan | `preview level solution`, `preview assisted plan`, `show level wiring plan` |
| Run an approved practice plan | `auto solve practice level`, `complete practice level`, `run approved level plan` |
| Stop an agent plan | `stop auto solve`, `cancel auto solve`, `stop spark agent` |
| Get agent help | `agent help`, `laboratory actions`, `what can you do in the laboratory` |
| Check agent status | `agent status`, `auto solve status` |
| Get 3D objective guidance | `show next objective in 3d`, `demonstrate next objective automatically`, `guide me to next objective` |

The native wire parser also accepts `draw wire from <source ID> to <destination ID>`, with `create`, `connect`, `route`, or `make` in place of `draw`. The parser accepts optional words such as `spark`, `physically`, `a`, `real`, and `native` in phrases such as `spark physically draw a native wire from <source ID> to <destination ID>`. Engineering mode still governs whether a wire action can run.

[Lab agent command parser](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkLabActionCommand.cs#L25) · [Native wire commands](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkNativeWireDirector.cs#L31) · [Wiring recorder](Assets/ProjectSpark/SparkV5/Pro/Integration/SparkNativeWireAutoEngineer.cs#L90)

## Unity Editor commands for Levels 1–130

In Unity, open **Project Spark → SUPER AI → Universal Auto Draw**.

| Menu item | Purpose |
| --- | --- |
| **Scan All 130 Levels** | Scan the campaign and report the state of its authored levels and plans. |
| **Validate All Plans** | Run the campaign plan checks. |
| **Export Campaign Report** | Write the campaign report. |
| **Generate Construction Plan Drafts** | Generate drafts from available authored wiring requirements. |
| **Import Generated Mission Draft** | Import a generated draft into Unity for review. |
| **Review Selected Plan** | Review the selected wiring plan and its authored evidence. |
| **Approve Selected Reviewed Revision** | Approve the reviewed revision of the selected plan. |

The scan, validation, and export menu items call the same report workflow. Draft generation and approval are separate operations. During gameplay, Spark builds the loaded level when that level's approved plan and runtime requirements are satisfied.

[Universal Auto Draw editor menu](Assets/ProjectSpark/SparkV5/Pro/Integration/Editor/SparkUniversalConstructionEditor.cs#L19)

## Verification

This reference was checked against the command handlers in the Project Spark master project. Commands were reviewed in source. Play Mode and voice recognition were outside this documentation check.
