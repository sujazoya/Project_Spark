using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSpark.SparkV5.Pro
{
    internal enum NexusCapability { Ask, Engineering, NativeRecorder, NativeWire, Lab, Demo, Mission, Flight, Arms, Devices, Teaching }
    internal enum NexusParameter { None, Text, Level, Terminal, Device, Metres }
    internal enum NexusEvidence { Reply, Mode, Plan, Wire, Suggestion, Device, Proposal, Pause, Resume, CancelPlan, CancelDevice, Observe }
    internal enum NexusMode { Retain, Manual, Assisted, Auto }
    internal enum NexusApproval { None, Confirm, NativeProposal }

    internal sealed class NexusCommand
    {
        public string Id { get; }
        public string Category { get; }
        public string Title { get; }
        public string Phrase { get; }
        public NexusCapability Capability { get; }
        public NexusParameter Parameter { get; }
        public NexusEvidence Evidence { get; }
        public NexusMode Mode { get; }
        public NexusApproval Approval { get; }
        public IReadOnlyList<string> Aliases { get; }
        public IReadOnlyList<string> Related { get; }

        public NexusCommand(string id, string category, string title, string phrase, NexusCapability capability,
            NexusParameter parameter = NexusParameter.None, NexusEvidence evidence = NexusEvidence.Reply,
            NexusMode mode = NexusMode.Retain, NexusApproval approval = NexusApproval.None,
            string aliases = "", string related = "")
        {
            Id = id; Category = category; Title = title; Phrase = phrase; Capability = capability;
            Parameter = parameter; Evidence = evidence; Mode = mode; Approval = approval;
            Aliases = Array.AsReadOnly(aliases.Split('|', StringSplitOptions.RemoveEmptyEntries));
            Related = Array.AsReadOnly(related.Split('|', StringSplitOptions.RemoveEmptyEntries));
        }

        public string Format(string value) => Phrase.Replace("{value}", value ?? "");
    }

    // Explicit adapters for existing handlers. Ask remains the only parser and action dispatcher.
    internal sealed class SparkNexusCatalog
    {
        private readonly Dictionary<string, NexusCommand[]> providers = new();
        public event Action Changed;
        public IEnumerable<NexusCommand> Commands => providers.Values.SelectMany(group => group);

        public void Register(string provider, IEnumerable<NexusCommand> commands)
        {
            var incoming = commands.ToArray();
            var all = providers.Where(entry => entry.Key != provider).SelectMany(entry => entry.Value).Concat(incoming).ToArray();
            var phrases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var command in all)
            {
                if (!ids.Add(command.Id)) throw new InvalidOperationException("Duplicate NEXUS command ID: " + command.Id);
                foreach (var phrase in command.Aliases.Prepend(command.Phrase))
                    if (!phrases.Add(phrase)) throw new InvalidOperationException("Duplicate NEXUS phrase: " + phrase);
            }
            providers[provider] = incoming;
            Changed?.Invoke();
        }

        public void Unregister(string provider)
        {
            if (providers.Remove(provider)) Changed?.Invoke();
        }

        public static IEnumerable<NexusCommand> Native()
        {
            const string circuit = "Construction", live = "Workbench", robot = "Robot", teach = "Teaching", session = "Session";
            const NexusCapability engineering = NexusCapability.Engineering, ask = NexusCapability.Ask;
            yield return new("mode.manual", circuit, "Manual mode", "spark manual mode", engineering, evidence: NexusEvidence.Mode, mode: NexusMode.Manual, aliases: "manual mode");
            yield return new("mode.assisted", circuit, "Assisted mode", "spark assisted mode", engineering, evidence: NexusEvidence.Mode, mode: NexusMode.Assisted, aliases: "assisted mode", related: "wire.suggest");
            yield return new("mode.auto", circuit, "Auto mode", "spark auto mode", engineering, evidence: NexusEvidence.Mode, mode: NexusMode.Auto, approval: NexusApproval.Confirm, aliases: "auto ai mode", related: "wire.preview|wire.draw");
            yield return new("wire.draw", circuit, "Build reviewed plan", "draw current level", engineering, evidence: NexusEvidence.Plan, mode: NexusMode.Auto, approval: NexusApproval.Confirm, aliases: "spark draw current level|spark draw this level|wire current level|spark wire current level|spark auto draw|hey spark draw current level", related: "wire.status|wire.pause|wire.cancel");
            yield return new("wire.continue", circuit, "Continue drawing", "spark continue drawing", engineering, evidence: NexusEvidence.Resume, mode: NexusMode.Auto, approval: NexusApproval.Confirm);
            yield return new("wire.pause", circuit, "Pause drawing", "spark pause drawing", engineering, evidence: NexusEvidence.Pause, aliases: "spark pause automation|pause auto wiring");
            yield return new("wire.resume", circuit, "Resume drawing", "spark resume drawing", engineering, evidence: NexusEvidence.Resume, aliases: "spark resume automation|resume auto wiring");
            yield return new("wire.cancel", circuit, "Stop drawing", "spark stop drawing", engineering, evidence: NexusEvidence.CancelPlan, aliases: "spark cancel automation|cancel auto wiring|spark stop|cancel wire|stop wiring");
            yield return new("wire.status", circuit, "Drawing status", "spark drawing status", engineering);
            yield return new("wire.explain", circuit, "Construction evidence", "spark explain current construction", engineering);
            yield return new("wire.engineering", circuit, "Engineering status", "spark engineering status", engineering, aliases: "engineering status|auto wiring status");
            yield return new("wire.preview", circuit, "Preview reviewed plan", "spark preview practice plan", engineering, aliases: "preview wiring plan|preview approved wiring plan", related: "wire.suggest|wire.draw");
            yield return new("wire.suggest", circuit, "Suggest next wire", "spark suggest next wire", engineering, evidence: NexusEvidence.Suggestion, mode: NexusMode.Assisted, aliases: "spark suggest next step|spark suggest the next wire|spark inspect the next connection", related: "wire.source|wire.approve|wire.failure");
            yield return new("wire.source", circuit, "Locate source terminal", "spark show me where the source terminal is", engineering);
            yield return new("wire.failure", circuit, "Explain connection failure", "spark explain why this connection failed", engineering);
            yield return new("wire.approve", circuit, "Approve one proposed wire", "spark approve suggested wire", engineering, evidence: NexusEvidence.Wire, mode: NexusMode.Assisted, approval: NexusApproval.NativeProposal, aliases: "spark perform approved step|spark connect the approved wire", related: "work.objectives|wire.suggest");
            yield return new("wire.approved", circuit, "Start approved practice plan", "spark start approved plan", engineering, evidence: NexusEvidence.Plan, mode: NexusMode.Auto, approval: NexusApproval.Confirm, aliases: "auto wire practice level|spark wire this practice level", related: "wire.status");
            yield return new("wire.reload", circuit, "Reload engineering plans", "spark reload engineering plans", engineering);
            yield return new("wire.narrate.on", circuit, "Enable engineering narration", "spark engineering narration on", engineering);
            yield return new("wire.narrate.off", circuit, "Disable engineering narration", "spark engineering narration off", engineering);
            yield return new("wire.record", circuit, "Record native wiring", "start wiring recording", NexusCapability.NativeRecorder, aliases: "record my wiring");
            yield return new("wire.record.stop", circuit, "Save wiring recording", "stop wiring recording", NexusCapability.NativeRecorder, aliases: "finish wiring recording");
            yield return new("wire.record.status", circuit, "Wiring recorder status", "spark wiring status", NexusCapability.NativeRecorder);
            yield return new("wire.record.reload", circuit, "Reload recorded plans", "reload wiring plans", NexusCapability.NativeRecorder);
            yield return new("wire.native.status", circuit, "Native wire status", "native wire status", NexusCapability.NativeWire, aliases: "real wire status");
            yield return new("wire.undo", circuit, "Undo last AI wire", "undo last AI wire", NexusCapability.NativeWire, evidence: NexusEvidence.Observe, approval: NexusApproval.Confirm, aliases: "undo last spark wire");
            yield return new("lab.help", circuit, "Laboratory actions", "agent help", NexusCapability.Lab, aliases: "laboratory actions|what can you do in the laboratory");
            yield return new("lab.status", circuit, "Laboratory agent status", "agent status", NexusCapability.Lab, aliases: "auto solve status");
            yield return new("lab.preview", circuit, "Preview assisted solution", "preview level solution", NexusCapability.Lab, aliases: "preview assisted plan|show level wiring plan");
            yield return new("lab.guide", teach, "Demonstrate next objective", "show next objective in 3d", NexusCapability.Lab, aliases: "demonstrate next objective automatically|guide me to next objective");
            yield return new("work.objectives", live, "Pending objectives", "which objective is pending", ask, related: "work.evidence|wire.preview");
            yield return new("work.evidence", live, "Objective evidence", "show objective evidence", ask, related: "work.diagnose");
            yield return new("work.diagnose", live, "Diagnose this level", "diagnose level", ask, aliases: "explain pending objectives|audit bindings");
            yield return new("work.progress", live, "Level progress", "show level progress", ask);
            yield return new("work.hint", live, "Get a workbench hint", "give me a hint", ask);
            yield return new("work.connections", live, "Circuit connections", "show circuit connections", ask, aliases: "which terminals are connected");
            yield return new("work.selected", live, "Point to selected component", "point at selected component", ask, aliases: "point at this|show me this component");
            yield return new("work.positive", live, "Find positive terminal", "show me the positive terminal", ask);
            yield return new("work.negative", live, "Find negative terminal", "show me the negative terminal", ask);
            yield return new("work.clear", live, "Clear highlight", "clear highlight", ask);
            yield return new("work.inventory", live, "Inventory", "show my inventory", ask, aliases: "what is in my inventory|inventory");
            yield return new("work.tool", live, "Active tool", "which tool is active", ask, aliases: "what tool am i using");
            yield return new("work.power", live, "Power supply settings", "power supply settings", ask, aliases: "what is this power supply set to");
            yield return new("work.voltage", live, "Ask for voltage", "what is the voltage", ask);
            yield return new("work.current", live, "Ask for current", "what is the current", ask);
            yield return new("work.resistance", live, "Ask for resistance", "what is the resistance", ask);
            yield return new("work.watts", live, "Ask for power", "what is the power", ask);
            yield return new("mission.status", live, "Inspect mission", "spark inspect mission", NexusCapability.Mission, aliases: "spark mission status");
            yield return new("mission.export", live, "Export mission report", "spark export mission report", NexusCapability.Mission);
            yield return new("device.list", robot, "Inspect devices", "spark inspect devices", NexusCapability.Devices);
            yield return new("device.status", robot, "Device status", "spark device status", NexusCapability.Devices);
            yield return new("device.on", robot, "Propose device ON", "spark propose device {value} on", NexusCapability.Devices, NexusParameter.Device, NexusEvidence.Proposal, NexusMode.Assisted, related: "device.approve|device.cancel");
            yield return new("device.off", robot, "Propose device OFF", "spark propose device {value} off", NexusCapability.Devices, NexusParameter.Device, NexusEvidence.Proposal, NexusMode.Assisted, related: "device.approve|device.cancel");
            yield return new("device.approve", robot, "Approve proposed device action", "spark approve device action", NexusCapability.Devices, evidence: NexusEvidence.Device, mode: NexusMode.Assisted, approval: NexusApproval.NativeProposal, related: "device.status");
            yield return new("device.cancel", robot, "Cancel device action", "spark cancel device action", NexusCapability.Devices, evidence: NexusEvidence.CancelDevice);
            yield return new("flight.up", robot, "Take off", "spark take off", NexusCapability.Flight, aliases: "take off|spark fly", evidence: NexusEvidence.Observe);
            yield return new("flight.down", robot, "Land", "spark land", NexusCapability.Flight, aliases: "land spark", evidence: NexusEvidence.Observe);
            yield return new("flight.hover", robot, "Hover", "spark hover", NexusCapability.Flight, evidence: NexusEvidence.Observe);
            yield return new("flight.selected", robot, "Fly to selection", "spark fly to selected", NexusCapability.Flight, aliases: "fly to selected", evidence: NexusEvidence.Observe);
            yield return new("flight.status", robot, "Flight status", "spark flight status", NexusCapability.Flight, aliases: "flight status");
            yield return new("arm.status", robot, "Arm status", "spark arm status", NexusCapability.Arms, aliases: "spark arms status");
            yield return new("arm.retract", robot, "Retract arms", "spark retract arms", NexusCapability.Arms, aliases: "spark retract arm", evidence: NexusEvidence.Observe);
            yield return new("arm.right", robot, "Reach with right arm", "spark extend right arm to {value}", NexusCapability.Arms, NexusParameter.Terminal, NexusEvidence.Observe, aliases: "spark reach terminal {value}");
            yield return new("arm.left", robot, "Reach with left arm", "spark extend left arm to {value}", NexusCapability.Arms, NexusParameter.Terminal, NexusEvidence.Observe);
            yield return new("arm.range", robot, "Set maximum arm reach", "spark set arm reach {value}", NexusCapability.Arms, NexusParameter.Metres, NexusEvidence.Observe);
            yield return new("teach.whiteboard", teach, "Open level whiteboard", "show current level on whiteboard", ask, related: "teach.wiring|teach.objectives");
            yield return new("teach.wiring", teach, "Show live circuit", "show live circuit on whiteboard", ask);
            yield return new("teach.objectives", teach, "Draw objective map", "draw objective map in 2d", ask);
            yield return new("teach.workbench", teach, "Highlight current workbench", "show current level in 3d", ask);
            yield return new("teach.draw", teach, "Draw the loaded level number", "draw level {value}", ask, NexusParameter.Level);
            yield return new("teach.models", teach, "List teaching models", "teaching models", NexusCapability.Teaching, related: "teach.model");
            yield return new("teach.model", teach, "Open teaching model", "show teaching model {value}", NexusCapability.Teaching, NexusParameter.Text, aliases: "show {value} in 3d");
            yield return new("teach.model.close", teach, "Close teaching model", "close teaching model", NexusCapability.Teaching);
            yield return new("teach.lesson", teach, "Start a lesson", "teach me {value}", ask, NexusParameter.Text);
            yield return new("teach.definition", teach, "Define a term", "define {value}", ask, NexusParameter.Text);
            yield return new("teach.online", teach, "Request an online explanation", "online explain {value}", ask, NexusParameter.Text, aliases: "model explain {value}");
            yield return new("teach.practice", teach, "Start practice", "practice", ask, aliases: "give me a practice question", related: "teach.practice.hint|teach.answer");
            yield return new("teach.practice.topic", teach, "Practice a topic", "practice {value}", ask, NexusParameter.Text);
            yield return new("teach.practice.hint", teach, "Practice hint", "practice hint", ask, aliases: "hint");
            yield return new("teach.answer", teach, "Submit an answer", "answer {value}", ask, NexusParameter.Text);
            yield return new("teach.lesson.pause", teach, "Pause lesson", "pause the lesson", ask);
            yield return new("teach.lesson.resume", teach, "Resume lesson", "continue our lesson", ask);
            yield return new("teach.lesson.next", teach, "Next lesson step", "go to the next lesson step", ask);
            yield return new("teach.lesson.stop", teach, "End lesson", "cancel the lesson", ask);
            yield return new("teach.practice.stop", teach, "Stop practice", "stop practice", ask);
            yield return new("teach.report", teach, "Learning report", "my learning report", ask, aliases: "my practice report|what did i learn previously");
            yield return new("demo.diode", teach, "Diode polarity demo", "demonstrate diode polarity", NexusCapability.Demo);
            yield return new("demo.switch", teach, "Switch demo", "demonstrate a switch", NexusCapability.Demo);
            yield return new("demo.meter", teach, "Multimeter demo", "demonstrate voltage measurement", NexusCapability.Demo);
            yield return new("demo.next", teach, "Next demo step", "next demo", NexusCapability.Demo, aliases: "continue demo|advance demo|proceed demo", related: "demo.status|demo.repeat");
            yield return new("demo.repeat", teach, "Repeat demo step", "repeat demo", NexusCapability.Demo, aliases: "retry demo|redo demo|show again demo");
            yield return new("demo.stop", teach, "Stop demo", "stop demo", NexusCapability.Demo, aliases: "end demo|cancel demo|close demo");
            yield return new("demo.status", teach, "Demo status", "demo status", NexusCapability.Demo, aliases: "demo progress|where are we in the demo");
            yield return new("session.chapter", session, "Current chapter", "current chapter", ask, aliases: "which chapter|current campaign|explain the current mission|explain current mission");
            yield return new("session.catalog", session, "Campaign catalog coverage", "catalog coverage", ask);
            yield return new("session.level", session, "Look up a level", "catalog level {value}", ask, NexusParameter.Level);
            yield return new("session.stop", session, "Interrupt speech", "stop speaking", ask, aliases: "stop|wait|wait a second|wait stop speaking");
            yield return new("session.mute", session, "Mute microphone", "mute microphone", ask);
            yield return new("session.coach.on", session, "Enable proactive coaching", "turn proactive coaching on", ask);
            yield return new("session.coach.off", session, "Disable proactive coaching", "turn proactive coaching off", ask);
            yield return new("session.reset", session, "Reset optional personalization", "reset optional personalization", ask, approval: NexusApproval.Confirm);
        }
    }
}
