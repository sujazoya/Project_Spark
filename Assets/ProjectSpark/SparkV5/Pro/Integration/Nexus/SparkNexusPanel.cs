using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.SparkV5.Pro
{
    [DisallowMultipleComponent]
    public sealed class SparkNexusPanel : MonoBehaviour
    {
        private const float FullNavigationWidth = 620f;
        private static readonly Color Cyan = new Color32(111, 236, 255, 255);
        private static readonly Color Ink = new Color32(228, 243, 249, 255);
        private static readonly Color Muted = new Color32(161, 193, 206, 255);
        private static readonly Color Deep = new Color32(8, 19, 29, 252);
        private static readonly Color Surface = new Color32(17, 42, 57, 255);
        private static readonly Color Amber = new Color32(255, 196, 104, 255);
        private static readonly Color Red = new Color32(255, 122, 128, 255);
        private static readonly Color Green = new Color32(129, 239, 181, 255);
        private static readonly Color Violet = new Color32(206, 179, 251, 255);
        private static readonly Color BlueGlass = new Color32(22, 64, 78, 255);
        private static readonly string[] Domains = { "Construction", "Workbench", "Robot", "Teaching", "Session" };

        [SerializeField] private bool reducedMotion;
        [SerializeField] private Material glass;
        private SparkProExecutive executive;
        private SparkEngineeringControlHub hub;
        private SparkNexusCatalog catalog;
        private SparkNexusContext context;
        private SparkNexusWorkflow workflow;
        private GameObject window;
        private RectTransform safe;
        private RectTransform dial;
        private RectTransform radial;
        private RectTransform domainGrid;
        private LayoutElement navigationSize;
        private TMP_Text voiceSignal;
        private float nextVoiceSample;
        private readonly float[] voiceSamples = new float[256];
        private RectTransform content;
        private ScrollRect scroll;
        private TMP_Text leftTitle, header, instrument, fault, preview, timeline, transcript, approvalText;
        private TMP_InputField input;
        private Button run, approve, dismiss, pause, stop;
        private readonly List<Button> rows = new();
        private readonly List<Button> targetRows = new();
        private readonly List<Button> relatedRows = new();
        private readonly List<string> stageHistory = new();
        private RectTransform targets, related;
        private GameObject approval;
        private NexusCommand selected;
        private IReadOnlyList<NexusTarget> displayedTargets;
        private string domain = "Workbench";
        private string rowStamp;
        private string relatedStamp;
        private Vector2 displaySize;
        private bool compact;
        private bool lastApprovalVisible;

        public bool Visible => window && window.activeInHierarchy;
        public SparkProExecutive BoundExecutive => executive;

        internal static SparkNexusPanel Create(SparkProExecutive spark, Transform parent)
        {
            var prefab = Resources.Load<SparkNexusPanel>("SparkNexusPanel");
            var panel = prefab ? Instantiate(prefab, parent, false) :
                new GameObject("Spark NEXUS", typeof(RectTransform), typeof(SparkNexusPanel)).GetComponent<SparkNexusPanel>();
            if (panel.transform.parent != parent) panel.transform.SetParent(parent, false);
            panel.Bind(spark);
            return panel;
        }

        private void Bind(SparkProExecutive spark)
        {
            executive = spark;
            hub = executive.GetComponent<SparkEngineeringControlHub>();
            context = new SparkNexusContext(spark);
            workflow = new SparkNexusWorkflow(spark, context);
            catalog = new SparkNexusCatalog();
            catalog.Register("Spark Live native adapters", SparkNexusCatalog.Native());
            Build();
            window.SetActive(false);
            executive.ConversationChanged += RenderConversation;
            executive.NexusTick += Tick;
            context.Changed += RenderContext;
            catalog.Changed += RefreshRows;
            workflow.Changed += RenderWorkflow;
        }

        private void OnDestroy()
        {
            if (executive)
            {
                executive.ConversationChanged -= RenderConversation;
                executive.NexusTick -= Tick;
            }
            if (context != null) { context.Changed -= RenderContext; context.Dispose(); }
            if (workflow != null) workflow.Changed -= RenderWorkflow;
            if (catalog != null) { catalog.Changed -= RefreshRows; catalog.Unregister("Spark Live native adapters"); }
        }

        public void SetVisible(bool visible)
        {
            if (!window) return;
            window.SetActive(visible);
            if (!Visible) return;
            Resize();
            context.Tick();
            RenderContext();
            RenderConversation();
            RenderWorkflow();
            if (approval.activeSelf) dismiss.Select();
        }

        private void OnEnable()
        {
            if (window && window.activeSelf) SetVisible(true);
        }

        private void Tick()
        {
            if (context == null || (!Visible && !workflow.Busy)) return;
            context.Tick();
            workflow.Tick();
            if (!Visible) return;
            Resize();
            UpdateVoice();
            if (!reducedMotion && dial) dial.Rotate(0f, 0f, Time.unscaledDeltaTime * 7f);
        }

        private void Resize()
        {
            Vector2 size = ((RectTransform)transform).rect.size;
            if ((displaySize - size).sqrMagnitude < 0.25f || size.x <= 0 || size.y <= 0) return;
            displaySize = size;
            compact = size.x < FullNavigationWidth || size.y < 460f;
            leftTitle.text = domain.ToUpperInvariant();
            radial.gameObject.SetActive(!compact);
            domainGrid.gameObject.SetActive(compact);
            navigationSize.preferredHeight = compact ? 42 : 126;
        }

        private void UpdateVoice()
        {
            if (Time.unscaledTime < nextVoiceSample) return;
            nextVoiceSample = Time.unscaledTime + 0.1f;
            if (!executive.SpeechOutputAvailable) { voiceSignal.text = "Voice offline"; return; }
            if (!executive.PlaybackActive) { voiceSignal.text = "Voice idle"; return; }
            float level = executive.SampleVoice(voiceSamples);
            voiceSignal.text = level > 0.0001f ? $"Voice signal  {level:P0}" : "Speaking";
        }

        private void RenderContext()
        {
            if (!Visible || context.Snapshot == null) return;
            var snapshot = context.Snapshot;
            header.text = $"NEXUS    /    {(snapshot.Level ? $"Level {snapshot.Level.LevelNumber}" : "Workshop")}    ·    {(hub ? hub.Mode.ToString() : "Mode unavailable")}    ·    {snapshot.Objectives}";
            instrument.text = snapshot.Telemetry;
            fault.gameObject.SetActive(!string.IsNullOrEmpty(snapshot.Fault));
            fault.text = snapshot.Fault;
            UpdatePreview();
            run.interactable = !workflow.Busy && (selected == null || string.IsNullOrEmpty(workflow.Blocker(selected, snapshot)));
            if (!ReferenceEquals(displayedTargets, snapshot.Targets))
            {
                displayedTargets = snapshot.Targets;
                RefreshTargets();
            }
            RefreshRows();
        }

        private void RenderConversation()
        {
            if (!Visible || transcript == null) return;
            var log = executive.History;
            StringBuilder text = new();
            for (int i = Math.Max(0, log.Count - 5); i < log.Count; i++)
                text.Append(log[i].Speaker).Append("  /  ").AppendLine(log[i].Text);
            transcript.text = text.Length == 0 ? "Spark Live transcript" : text.ToString();
        }

        private void RenderWorkflow()
        {
            if (!Visible || timeline == null) return;
            if (stageHistory.Count == 0 || stageHistory[^1] != workflow.Stage.ToString())
                stageHistory.Add(workflow.Stage.ToString());
            if (stageHistory.Count > 6) stageHistory.RemoveAt(0);
            timeline.text = string.Join("  ›  ", stageHistory) + (string.IsNullOrEmpty(workflow.Detail) ? "" : "\n" + workflow.Detail);
            timeline.color = workflow.Stage switch
            {
                NexusStage.AwaitingApproval => Amber, NexusStage.Succeeded => Green,
                NexusStage.Failed or NexusStage.TimedOut => Red, _ => Muted
            };
            bool awaiting = workflow.Stage == NexusStage.AwaitingApproval;
            approval.SetActive(awaiting);
            approvalText.text = workflow.Detail;
            if (awaiting && !lastApprovalVisible) dismiss.Select();
            if (!awaiting && lastApprovalVisible) run.Select();
            lastApprovalVisible = awaiting;
            pause.interactable = hub && hub.IsExecuting;
            stop.interactable = workflow.Busy || (hub && hub.IsExecuting) ||
                (executive.TryGetComponent<SparkEngineeringDeviceOperator>(out var devices) && devices.IsBusy);
            run.interactable = !workflow.Busy && (selected == null || string.IsNullOrEmpty(workflow.Blocker(selected, context.Snapshot)));
            RefreshRows();
        }

        private void RefreshRows()
        {
            if (!Visible || content == null) return;
            var snapshot = context.Snapshot;
            var entries = catalog.Commands.Where(command => command.Category == domain).ToArray();
            var blockers = entries.Select(command => workflow.Blocker(command, snapshot)).ToArray();
            string current = domain + "|" + string.Join("|", entries.Select(entry => entry.Id)) + "|" + string.Join("|", blockers);
            if (rowStamp != current)
            {
                rowStamp = current;
                FillRows(rows, content, entries.Select((command, index) =>
                    (command.Title + (blockers[index].Length == 0 ? "" : "  ·  " + blockers[index]),
                    blockers[index].Length == 0 ? Ink : Muted, (Action)(() => Choose(command)))));
            }
            var suggestions = catalog.Commands
                .Select(command => (command, score: Score(command, snapshot)))
                .Where(entry => entry.score > 0 && string.IsNullOrEmpty(workflow.Blocker(entry.command, snapshot)))
                .OrderByDescending(entry => entry.score)
                .ThenBy(entry => entry.command.Id, StringComparer.Ordinal)
                .Take(3).Select(entry => entry.command).ToArray();
            string stamp = string.Join("|", suggestions.Select(entry => entry.Id));
            if (stamp != relatedStamp)
            {
                relatedStamp = stamp;
                FillRows(relatedRows, related, suggestions.Select(command =>
                    (command.Title, Cyan, (Action)(() => Choose(command)))));
            }
        }

        private int Score(NexusCommand command, NexusSnapshot snapshot)
        {
            if (command == selected) return 0;
            if (selected != null && selected.Related.Contains(command.Id)) return 30;
            if (hub && hub.IsExecuting && command.Id is "wire.status" or "wire.pause" or "wire.cancel") return 26;
            if (hub && hub.PendingStep != "None" && command.Id is "wire.source" or "wire.approve") return 24;
            if (!string.IsNullOrEmpty(snapshot?.Fault) && command.Id is "work.diagnose" or "work.evidence") return 22;
            var target = snapshot?.Selected;
            if (target && (target is SparkEngineeringDevicePort || target.GetComponentInParent<SparkEngineeringDevicePort>())
                && command.Id is "device.on" or "device.off" or "device.status") return 20;
            if (target && (target is SparkTerminal || target is SparkElectricalComponent)
                && command.Id is "work.voltage" or "work.current" or "work.connections") return 15;
            if (snapshot?.Level && command.Id is "work.objectives" or "teach.whiteboard" or "wire.preview") return 10;
            return 0;
        }

        private void RefreshTargets()
        {
            if (targets == null) return;
            FillRows(targetRows, targets, displayedTargets.Select(target => (target.Label, Muted, (Action)(() =>
            {
                context.Focus(target.Component);
                if (selected?.Parameter is NexusParameter.Device or NexusParameter.Terminal)
                    input.SetTextWithoutNotify(target.NativeId ?? "");
                UpdatePreview();
            }))));
        }

        private void Choose(NexusCommand command)
        {
            selected = command;
            input.SetTextWithoutNotify("");
            var snapshot = context.Snapshot;
            if (command?.Parameter == NexusParameter.Level && snapshot?.Level)
                input.SetTextWithoutNotify(snapshot.Level.LevelNumber.ToString());
            else if (command?.Parameter is NexusParameter.Terminal or NexusParameter.Device && snapshot?.Selected)
            {
                var target = snapshot.Targets.FirstOrDefault(item => item.Component == snapshot.Selected);
                if (target != null) input.SetTextWithoutNotify(target.NativeId ?? "");
            }
            input.contentType = command?.Parameter == NexusParameter.Level
                ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.Standard;
            input.gameObject.SetActive(command == null || command.Parameter != NexusParameter.None);
            ((TMP_Text)input.placeholder).text = command?.Parameter switch
            {
                NexusParameter.Level => "Level number", NexusParameter.Metres => "Reach in metres",
                NexusParameter.Terminal => "Choose an authored terminal or enter its ID",
                NexusParameter.Device => "Choose a device or enter its exact ID",
                _ => command == null ? "Ask Spark…" : "Enter a topic or answer"
            };
            UpdatePreview();
            RenderWorkflow();
        }

        private void UpdatePreview()
        {
            var blocker = selected == null ? "" : workflow.Blocker(selected, context.Snapshot);
            preview.text = (selected?.Format(input.text) ?? "Ask Spark") +
                (string.IsNullOrEmpty(blocker) ? "" : "\n" + blocker);
        }

        private void Execute()
        {
            if (workflow.Busy) return;
            stageHistory.Clear();
            workflow.Start(selected, input.text);
        }

        private void Build()
        {
            var root = (RectTransform)transform;
            Stretch(root);
            window = Rect(root, "Nexus console").gameObject;
            Stretch((RectTransform)window.transform);
            window.AddComponent<RectMask2D>();
            safe = Rect(window.transform, "Safe area");
            Stretch(safe);
            var frame = safe.gameObject.AddComponent<Image>();
            frame.color = Deep;
            frame.material = glass ? glass : Resources.Load<Material>("SparkNexusGlass");
            Vertical(safe, 5f, 7);

            var title = Row(safe, 30);
            header = Label(title, "NEXUS", 15, Cyan, true);
            header.alignment = TextAlignmentOptions.MidlineLeft;
            header.overflowMode = TextOverflowModes.Ellipsis;
            var motionButton = ButtonFor(title, "Motion", () => reducedMotion = !reducedMotion, 62);
            motionButton.GetComponent<LayoutElement>().minHeight = 30;
            var navigation = Row(safe, 126);
            navigationSize = navigation.GetComponent<LayoutElement>();
            var navigationControls = Rect(navigation, "Domain controls");
            navigationControls.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
            radial = Rect(navigationControls, "Radial navigation");
            Stretch(radial);
            dial = Rect(radial, "Holographic dial");
            dial.sizeDelta = new Vector2(132, 132);
            var segments = dial.gameObject.AddComponent<SparkNexusDial>();
            segments.color = Cyan;
            segments.raycastTarget = false;
            domainGrid = Rect(navigationControls, "Compact navigation");
            Stretch(domainGrid);
            var compactRow = Row(domainGrid, 42);
            Stretch(compactRow);
            for (int i = 0; i < Domains.Length; i++)
            {
                string item = Domains[i];
                void Browse()
                {
                    domain = item;
                    leftTitle.text = domain.ToUpperInvariant();
                    RefreshRows();
                    scroll.verticalNormalizedPosition = 1;
                }
                ButtonFor(compactRow, item, Browse);
                var card = (RectTransform)ButtonFor(radial, item, Browse).transform;
                float angle = (90f - i * 72f) * Mathf.Deg2Rad;
                card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
                card.anchoredPosition = new Vector2(192f * Mathf.Cos(angle), 49f * Mathf.Sin(angle));
                card.sizeDelta = new Vector2(112, 42);
            }
            voiceSignal = Label(radial, "Voice idle", 13, Muted);
            voiceSignal.alignment = TextAlignmentOptions.Midline;
            voiceSignal.rectTransform.anchorMin = voiceSignal.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            voiceSignal.rectTransform.sizeDelta = new Vector2(122, 32);

            // One scroll surface keeps the command and component cards reachable on narrow touch screens.
            var scrollRoot = Rect(safe, "Command browser");
            var flex = scrollRoot.gameObject.AddComponent<LayoutElement>();
            flex.flexibleHeight = 1f; flex.minHeight = 30;
            scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = 34;
            var view = Rect(scrollRoot, "Viewport");
            Stretch(view); view.gameObject.AddComponent<RectMask2D>();
            var hit = view.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0.01f);
            var stack = Rect(view, "Content");
            stack.anchorMin = new Vector2(0, 1); stack.anchorMax = Vector2.one; stack.pivot = new Vector2(0.5f, 1);
            stack.anchoredPosition = Vector2.zero; stack.sizeDelta = Vector2.zero;
            Vertical(stack, 5, 0);
            stack.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = view; scroll.content = stack;
            leftTitle = Label(stack, "COMMANDS", 12, Muted);
            instrument = Label(stack, "", 14, Cyan);
            fault = Label(stack, "", 14, Red);
            content = Rect(stack, "Command cards"); Vertical(content, 3, 0);
            Label(stack, "RELATED COMMANDS", 12, Violet);
            related = Rect(stack, "Related commands"); Vertical(related, 3, 0);
            Label(stack, "LIVE COMPONENTS AND TERMINALS", 12, Cyan);
            targets = Rect(stack, "Component cards"); Vertical(targets, 3, 0);
            transcript = Label(stack, "", 15, Muted);

            preview = Label(safe, "Ask Spark", 15, Ink);
            preview.maxVisibleLines = 2;
            preview.overflowMode = TextOverflowModes.Ellipsis;
            preview.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
            input = MakeInput(safe);
            input.onValueChanged.AddListener(_ => UpdatePreview());
            input.onSubmit.AddListener(_ => Execute());
            var actions = Row(safe, 44);
            ButtonFor(actions, "Ask", () => Choose(null));
            run = ButtonFor(actions, "Send", Execute);
            pause = ButtonFor(actions, "Pause / Resume", () => workflow.TogglePause());
            stop = ButtonFor(actions, "Cancel", () => workflow.Cancel());
            timeline = Label(safe, "", 13, Muted);
            timeline.maxVisibleLines = 3;
            timeline.overflowMode = TextOverflowModes.Ellipsis;
            var timelineSize = timeline.gameObject.AddComponent<LayoutElement>();
            timelineSize.preferredHeight = 36;
            timelineSize.flexibleHeight = 0;

            approval = Rect(window.transform, "Confirm native action").gameObject;
            var approvalRoot = (RectTransform)approval.transform;
            Stretch(approvalRoot);
            var overlay = approval.AddComponent<Image>(); overlay.color = new Color(0, 0, 0, 0.78f);
            var dialog = Rect(approvalRoot, "Approval dialog");
            dialog.anchorMin = new Vector2(0.08f, 0.28f);
            dialog.anchorMax = new Vector2(0.92f, 0.72f);
            dialog.offsetMin = dialog.offsetMax = Vector2.zero;
            dialog.gameObject.AddComponent<Image>().color = Surface;
            Vertical(dialog, 10, 20);
            Label(dialog, "CONFIRM NATIVE ACTION", 19, Amber);
            approvalText = Label(dialog, "", 16, Ink, true);
            var confirmRow = Row(dialog, 46);
            dismiss = ButtonFor(confirmRow, "Back", () => workflow.Cancel());
            approve = ButtonFor(confirmRow, "Confirm", () => workflow.Confirm());
            var backNav = dismiss.navigation; backNav.mode = Navigation.Mode.Explicit; backNav.selectOnRight = approve;
            dismiss.navigation = backNav;
            var confirmNav = approve.navigation; confirmNav.mode = Navigation.Mode.Explicit; confirmNav.selectOnLeft = dismiss;
            approve.navigation = confirmNav;
            approval.SetActive(false);
        }

        private static RectTransform Rect(Transform parent, string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return (RectTransform)child.transform;
        }

        private static void Stretch(RectTransform item)
        {
            item.anchorMin = Vector2.zero; item.anchorMax = Vector2.one;
            item.offsetMin = item.offsetMax = Vector2.zero;
        }

        private static void Vertical(RectTransform parent, float spacing, int padding)
        {
            var group = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            group.spacing = spacing; group.padding = new RectOffset(padding, padding, padding, padding);
            group.childControlWidth = true; group.childControlHeight = true;
            group.childForceExpandWidth = true; group.childForceExpandHeight = false;
        }

        private static RectTransform Row(Transform parent, float height)
        {
            var item = Rect(parent, "Controls");
            var group = item.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 5; group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = true;
            var size = item.gameObject.AddComponent<LayoutElement>();
            size.preferredHeight = height;
            size.flexibleHeight = 0;
            return item;
        }

        private static TMP_Text Label(Transform parent, string text, float size, Color color, bool flex = false)
        {
            var label = Rect(parent, "Label").gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text; label.fontSize = size; label.color = color;
            label.richText = false; label.raycastTarget = false;
            if (flex) label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            return label;
        }

        private static Button ButtonFor(Transform parent, string caption, Action pressed, float width = 0)
        {
            var item = Rect(parent, caption);
            var image = item.gameObject.AddComponent<Image>(); image.color = Surface;
            var button = item.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(1.2f, 1.45f, 1.55f);
            colors.selectedColor = new Color(1.2f, 1.45f, 1.55f); button.colors = colors;
            var dimensions = item.gameObject.AddComponent<LayoutElement>();
            dimensions.minHeight = 42;
            dimensions.flexibleWidth = width > 0 ? 0 : 1;
            if (width > 0) { dimensions.preferredWidth = width; dimensions.flexibleWidth = 0; }
            var text = Label(item, caption, 14, Ink);
            text.alignment = TextAlignmentOptions.Midline;
            text.overflowMode = TextOverflowModes.Ellipsis;
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(5, 1); text.rectTransform.offsetMax = new Vector2(-5, -1);
            if (pressed != null) button.onClick.AddListener(() => pressed());
            return button;
        }

        private static TMP_InputField MakeInput(RectTransform parent)
        {
            var item = Rect(parent, "Command parameter");
            item.gameObject.AddComponent<Image>().color = BlueGlass;
            item.gameObject.AddComponent<LayoutElement>().preferredHeight = 36;
            var field = item.gameObject.AddComponent<TMP_InputField>();
            field.targetGraphic = item.GetComponent<Image>();
            field.characterLimit = 900;
            var viewport = Rect(item, "Text viewport");
            Stretch(viewport);
            viewport.offsetMin = new Vector2(10, 3); viewport.offsetMax = new Vector2(-10, -3);
            viewport.gameObject.AddComponent<RectMask2D>();
            field.textViewport = viewport;
            var text = Label(viewport, "", 16, Ink);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            var hint = Label(viewport, "Ask Spark…", 16, Muted);
            hint.alignment = TextAlignmentOptions.MidlineLeft;
            Stretch(text.rectTransform); Stretch(hint.rectTransform);
            field.textComponent = text; field.placeholder = hint;
            return field;
        }

        private static void FillRows(List<Button> pool, RectTransform parent,
            IEnumerable<(string caption, Color color, Action handler)> values)
        {
            int index = 0;
            foreach (var entry in values)
            {
                if (pool.Count == index) pool.Add(ButtonFor(parent, "", null));
                var button = pool[index++];
                var label = button.GetComponentInChildren<TMP_Text>(true);
                label.text = entry.caption; label.color = entry.color;
                button.onClick.RemoveAllListeners();
                var handler = entry.handler;
                button.onClick.AddListener(() => handler());
                button.gameObject.SetActive(true);
            }
            for (; index < pool.Count; index++) pool[index].gameObject.SetActive(false);
        }
    }
}
