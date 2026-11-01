using System;
using System.Collections.Generic;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Read-only, active-level workbench fault inspector. It NEVER solves,
    /// alters wiring, modifies device states, or assigns a global fault to an
    /// arbitrary component. Local markers require component-specific evidence.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkWorkbenchFaultInspector : MonoBehaviour
    {
        private enum Severity { Information, Warning, Failure }

        private sealed class Finding
        {
            public string key;
            public string title;
            public string explanation;
            public Transform target;
            public Severity severity;
        }

        [Header("Existing Project Spark systems")]
        [SerializeField] private SparkLevelLayoutLoader levelLoader;
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private LevelGamePlayManager levelManager;
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private SparkSolverLiveMeasurementBridge liveBridge;
        [SerializeField] private Camera worldCamera;

        [Header("Generated inspector UI")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject expandedPanel;
        [SerializeField] private GameObject collapsedTab;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text explanationText;
        [SerializeField] private RectTransform findingsContainer;
        [SerializeField] private Button findingRowTemplate;
        [SerializeField] private RectTransform markerLayer;
        [SerializeField] private RectTransform markerTemplate;
        [SerializeField] private Button tabButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Button markersButton;
        [SerializeField] private TMP_Text markersButtonText;

        [Header("Behavior")]
        [SerializeField] private bool startExpanded = false;
        [SerializeField] private bool showWorldMarkers = true;
        [SerializeField, Range(1, 8)] private int maxWorldMarkers = 5;
        [SerializeField, Min(0.1f)] private float refreshInterval = 0.5f;
        [SerializeField] private Color failureColor = new Color(1f, .35f, .32f, 1f);
        [SerializeField] private Color warningColor = new Color(1f, .75f, .28f, 1f);
        [SerializeField] private Color informationColor = new Color(.38f, .83f, 1f, 1f);

        private readonly List<Finding> findings = new List<Finding>();
        private readonly List<Button> rowInstances = new List<Button>();
        private readonly List<RectTransform> markerInstances = new List<RectTransform>();
        private SparkLevelLayoutLoader boundLoader;
        private SparkVerifiedElectricalPublisher boundPublisher;
        private SparkElectricalSolver boundSolver;
        private SparkVerifiedElectricalFrame verifiedFrame;
        private string displayedSignature;
        private string stateMessage;
        private bool expanded;
        private bool waitingForNewLayout = true;
        private int selected = -1;
        private float nextRefresh;
        private float nextReferenceSearch;

        public int FindingCount => findings.Count;
        public string Status => stateMessage ?? "Waiting for the active level";
        public bool Expanded => expanded;

        private void Awake()
        {
            ResolveReferences();
            WireButtons();
            SetExpanded(startExpanded);
        }

        private void OnEnable()
        {
            ResolveReferences();
            RebindSources();
            RefreshNow();
        }

        private void OnDisable() => UnbindSources();
        private void OnDestroy()
        {
            UnbindSources();
            ClearGeneratedUi();
        }

        private void Start()
        {
            ResolveReferences();
            RebindSources();
            RefreshNow();
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextReferenceSearch)
            {
                nextReferenceSearch = Time.unscaledTime + 2f;
                if (levelLoader == null || solver == null || publisher == null)
                {
                    ResolveReferences();
                    RebindSources();
                }
            }

            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + Mathf.Max(.1f, refreshInterval);
                RefreshNow();
            }

            PositionMarkers();
        }

        private void ResolveReferences()
        {
            if (levelLoader == null)
                levelLoader = FindFirstObjectByType<SparkLevelLayoutLoader>(FindObjectsInactive.Include);
            if (solver == null)
                solver = FindFirstObjectByType<SparkElectricalSolver>(FindObjectsInactive.Include);
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (liveBridge == null)
                liveBridge = FindFirstObjectByType<SparkSolverLiveMeasurementBridge>(FindObjectsInactive.Include);
            if (worldCamera == null)
                worldCamera = Camera.main;
        }

        private void RebindSources()
        {
            if (boundLoader != levelLoader)
            {
                if (boundLoader != null)
                {
                    boundLoader.LayoutLoaded -= OnLayoutLoaded;
                    boundLoader.LayoutUnloaded -= OnLayoutUnloaded;
                }
                boundLoader = levelLoader;
                if (boundLoader != null)
                {
                    boundLoader.LayoutLoaded += OnLayoutLoaded;
                    boundLoader.LayoutUnloaded += OnLayoutUnloaded;
                }
                verifiedFrame = null;
                waitingForNewLayout = true;
            }

            if (boundPublisher != publisher)
            {
                if (boundPublisher != null)
                    boundPublisher.FramePublished -= OnFramePublished;
                boundPublisher = publisher;
                if (boundPublisher != null)
                    boundPublisher.FramePublished += OnFramePublished;
                verifiedFrame = null;
            }

            if (boundSolver != solver)
            {
                if (boundSolver != null)
                {
                    boundSolver.SolveCompleted -= OnSolveCompleted;
                    boundSolver.SolveFailed -= OnSolveFailed;
                }
                boundSolver = solver;
                if (boundSolver != null)
                {
                    boundSolver.SolveCompleted += OnSolveCompleted;
                    boundSolver.SolveFailed += OnSolveFailed;
                }
            }
        }

        private void UnbindSources()
        {
            if (boundLoader != null)
            {
                boundLoader.LayoutLoaded -= OnLayoutLoaded;
                boundLoader.LayoutUnloaded -= OnLayoutUnloaded;
            }
            if (boundPublisher != null)
                boundPublisher.FramePublished -= OnFramePublished;
            if (boundSolver != null)
            {
                boundSolver.SolveCompleted -= OnSolveCompleted;
                boundSolver.SolveFailed -= OnSolveFailed;
            }
            boundLoader = null;
            boundPublisher = null;
            boundSolver = null;
        }

        private void OnLayoutLoaded(SparkLevelLayout layout)
        {
            verifiedFrame = null;
            waitingForNewLayout = true;
            selected = -1;
            displayedSignature = null;
            // Let the new layout's solver publish after initialization.
            nextRefresh = Time.unscaledTime;
        }

        private void OnLayoutUnloaded()
        {
            verifiedFrame = null;
            waitingForNewLayout = true;
            selected = -1;
            displayedSignature = null;
            findings.Clear();
            RebuildVisibleFindings();
            nextRefresh = Time.unscaledTime;
        }

        private void OnSolveCompleted()
        {
            if (levelLoader != null && levelLoader.HasActiveLayout &&
                solver != null && !solver.IsDirty && solver.LatestSolveResult != null)
                waitingForNewLayout = false;
            nextRefresh = Time.unscaledTime;
        }

        private void OnSolveFailed()
        {
            // A failed numerical solve is never evidence of a new valid reading.
            nextRefresh = Time.unscaledTime;
        }

        private void OnFramePublished(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null || levelLoader == null ||
                !levelLoader.HasActiveLayout || levelLoader.ActiveLayout == null)
                return;

            // Prevent a previous level's late publisher callback being shown.
            if (liveBridge != null &&
                (!liveBridge.HasActiveLevel ||
                 !string.Equals(frame.circuitId, liveBridge.CurrentCircuitId, StringComparison.Ordinal)))
                return;

            if (frame.revision <= 0 || string.IsNullOrWhiteSpace(frame.circuitId))
                return;

            if (verifiedFrame != null &&
                string.Equals(verifiedFrame.circuitId, frame.circuitId, StringComparison.Ordinal) &&
                frame.revision <= verifiedFrame.revision)
                return;

            verifiedFrame = frame.Copy();
            waitingForNewLayout = false;
            nextRefresh = Time.unscaledTime;
        }

        public void RefreshNow()
        {
            findings.Clear();
            var unique = new HashSet<string>(StringComparer.Ordinal);
            SparkLevelLayout layout = levelLoader != null ? levelLoader.ActiveLayout : null;
            SparkLevelSceneBindings bindings = levelLoader != null ? levelLoader.ActiveBindings : null;

            if (layout == null || bindings == null || !layout.gameObject.activeInHierarchy)
            {
                waitingForNewLayout = true;
                stateMessage = "WAITING FOR ACTIVE LEVEL";
                RenderStatus("No active layout or scene bindings.", "No circuit measurements available.");
                RebuildIfChanged();
                return;
            }

            if (solver == null || solver.IsSolving || solver.IsDirty || solver.LatestSolveResult == null)
            {
                stateMessage = "WAITING FOR VERIFIED SOLVE";
                RenderStatus(layout.name, "The electrical solver has no clean completed result.");
                RebuildIfChanged();
                return;
            }

            // Do not inherit the previous level's last clean result during an unload/load.
            // If the inspector attaches after a valid solve, the bridge's positive
            // active-level publication revision also proves a fresh solve occurred.
            if (waitingForNewLayout)
            {
                if (liveBridge != null && liveBridge.HasActiveLevel && liveBridge.LastRevision > 0)
                    waitingForNewLayout = false;
                else
                {
                    stateMessage = "WAITING FOR THIS LEVEL'S SOLVE";
                    RenderStatus(layout.name, "A new completed solve is required after loading this layout.");
                    RebuildIfChanged();
                    return;
                }
            }

            // An active clean solve is the gate for any displayed fault claims.
            if (verifiedFrame != null && liveBridge != null &&
                (!liveBridge.HasActiveLevel ||
                 !string.Equals(verifiedFrame.circuitId, liveBridge.CurrentCircuitId, StringComparison.Ordinal)))
                verifiedFrame = null;

            CollectVerifiedFrameFindings(layout, bindings, unique);
            CollectResistorFindings(layout, unique);
            CollectLevelFindings(bindings, unique);
            stateMessage = findings.Count > 0 ? "ELECTRICAL FINDINGS" : "NO REPORTED FAULTS";
            string note = verifiedFrame == null
                ? "Solver ready. Waiting for the latest verified publisher frame."
                : "Verified frame " + verifiedFrame.revision + " / " + verifiedFrame.circuitId;
            RenderStatus(layout.name, note);
            RebuildIfChanged();
            if (selected < 0) SetText(explanationText, note);
        }

        private void CollectVerifiedFrameFindings(
            SparkLevelLayout layout, SparkLevelSceneBindings bindings, HashSet<string> seen)
        {
            if (verifiedFrame == null) return;
            if (verifiedFrame.hasVerifiedFault)
            {
                string id = verifiedFrame.faultAnchorId;
                string description = !string.IsNullOrWhiteSpace(verifiedFrame.verifiedFaultExplanation)
                    ? verifiedFrame.verifiedFaultExplanation : "Solver-reported verified fault.";
                AddFinding(seen, "frame:fault:" + id, "Verified circuit fault",
                    description, ResolveTarget(layout, bindings, id), Severity.Failure);
            }
            if (verifiedFrame.readings == null) return;
            for (int i = 0; i < verifiedFrame.readings.Count; i++)
            {
                SparkVerifiedElectricalReading r = verifiedFrame.readings[i];
                if (r == null || string.IsNullOrWhiteSpace(r.anchorId)) continue;
                Severity severity;
                switch (r.condition)
                {
                    case SparkElectricalCondition.Failed:
                        severity = Severity.Failure; break;
                    case SparkElectricalCondition.OpenCircuit:
                    case SparkElectricalCondition.ReversePolarity:
                    case SparkElectricalCondition.OverCurrent:
                        severity = Severity.Warning; break;
                    default: continue; // Normal/Unknown aren't verified local faults.
                }
                AddFinding(seen, "frame:" + r.anchorId + ":" + r.condition,
                    r.anchorId + " — " + r.condition,
                    !string.IsNullOrWhiteSpace(r.explanation)
                        ? r.explanation : "Published verified condition: " + r.condition + ".",
                    ResolveTarget(layout, bindings, r.anchorId), severity);
            }
        }

        private void CollectResistorFindings(SparkLevelLayout layout, HashSet<string> seen)
        {
            SparkResistor[] resistors = layout.GetComponentsInChildren<SparkResistor>(true);
            for (int i = 0; i < resistors.Length; i++)
            {
                SparkResistor resistor = resistors[i];
                if (resistor == null || !resistor.isActiveAndEnabled || !resistor.ElectricalEnabled)
                    continue;

                bool failed = resistor.IsBurned ||
                    (resistor.BurnSystem != null && resistor.BurnSystem.IsFailed);
                if (failed)
                {
                    AddFinding(seen, "burn:" + resistor.GetInstanceID(),
                        resistor.name + " — failed resistor",
                        "Resistor failure is reported by this component's live burn state.",
                        resistor.transform, Severity.Failure);
                    continue;
                }
                if (resistor.IsOverPower)
                {
                    AddFinding(seen, "power:" + resistor.GetInstanceID(),
                        resistor.name + " — over rated power",
                        "The resistor reports power above its rated limit. " +
                        "Power: " + resistor.PowerWatts.ToString("0.###") + " W / rating: " +
                        resistor.MaximumPowerWatts.ToString("0.###") + " W.",
                        resistor.transform, Severity.Warning);
                }
            }
        }

        private void CollectLevelFindings(SparkLevelSceneBindings bindings, HashSet<string> seen)
        {
            // These evaluator flags describe the whole level, NOT a specific wire.
            if (levelManager == null || (!levelManager.IsPlaying && !levelManager.IsFailed) ||
                levelManager.ElectricalSolver != solver ||
                levelManager.SceneBindings != bindings)
                return;

            if (levelManager.SourceShorted)
                AddFinding(seen, "level:sourceShort", "Source short detected",
                    "The level evaluator reports a source short. Location has not been verified.",
                    null, Severity.Failure);
            if (levelManager.TargetShorted)
                AddFinding(seen, "level:targetShort", "Target short detected",
                    "The level evaluator reports a target short. Location has not been verified.",
                    null, Severity.Failure);
            if (levelManager.Overloaded)
                AddFinding(seen, "level:overload", "Circuit overload",
                    "The level evaluator reports an overload. Inspect component readings to localize it.",
                    null, Severity.Warning);
            if (levelManager.WrongConnection)
                AddFinding(seen, "level:wrong", "Connection validation failed",
                    "The level evaluator reports an incorrect connection; no individual wire is identified.",
                    null, Severity.Warning);
            if (!levelManager.ClosedReturn)
                AddFinding(seen, "level:openReturn", "No closed return path",
                    "The level evaluator does not currently confirm a closed return path. " +
                    "This may be intentional until the circuit is completed.",
                    null, Severity.Information);
        }

        private static Transform ResolveTarget(
            SparkLevelLayout layout, SparkLevelSceneBindings bindings, string id)
        {
            if (string.IsNullOrWhiteSpace(id) || layout == null) return null;
            if (SparkTeachingAnchor.TryFind(id, out SparkTeachingAnchor anchor) &&
                anchor != null && anchor.transform.IsChildOf(layout.transform))
                return anchor.transform;
            if (bindings != null && bindings.TryGetComponent(id, out SparkElectricalComponent component) &&
                component != null && component.transform.IsChildOf(layout.transform))
                return component.transform;
            if (bindings != null && bindings.TryGetTerminal(id, out SparkTerminal terminal) &&
                terminal != null && terminal.transform.IsChildOf(layout.transform))
                return terminal.transform;
            return null; // No guessed position based on object display names.
        }

        private void AddFinding(HashSet<string> seen, string key, string title,
            string explanation, Transform target, Severity severity)
        {
            if (!seen.Add(key)) return;
            findings.Add(new Finding
            {
                key = key, title = title, explanation = explanation,
                target = target, severity = severity
            });
        }

        private void RenderStatus(string level, string detail)
        {
            SetText(stateText, stateMessage);
            SetText(levelText, "LEVEL  " + level);
            SetText(countText, findings.Count + " finding" + (findings.Count == 1 ? "" : "s"));
            if (selected < 0 || selected >= findings.Count)
                SetText(explanationText, detail);
            if (markersButtonText != null)
                SetText(markersButtonText, showWorldMarkers ? "PINS ON" : "PINS OFF");
        }

        private void RebuildIfChanged()
        {
            var signature = new System.Text.StringBuilder();
            signature.Append(stateMessage).Append('|');
            for (int i = 0; i < findings.Count; i++)
                signature.Append(findings[i].key).Append(':')
                    .Append(findings[i].target != null ? findings[i].target.GetInstanceID() : 0).Append(':')
                    .Append(findings[i].title).Append(':')
                    .Append(findings[i].explanation).Append('|');
            string value = signature.ToString();
            if (string.Equals(displayedSignature, value, StringComparison.Ordinal))
                return;
            displayedSignature = value;
            selected = -1;
            RebuildVisibleFindings();
        }

        private void RebuildVisibleFindings()
        {
            ClearGeneratedUi();
            if (findingRowTemplate != null && findingsContainer != null)
            {
                for (int i = 0; i < findings.Count; i++)
                {
                    int index = i;
                    Button row = Instantiate(findingRowTemplate, findingsContainer, false);
                    row.gameObject.name = "Finding_" + i;
                    row.gameObject.SetActive(true);
                    TMP_Text text = row.GetComponentInChildren<TMP_Text>(true);
                    if (text != null)
                    {
                        text.text = (findings[i].target != null ? "◎ " : "• ") + findings[i].title;
                        text.color = ColorFor(findings[i].severity);
                    }
                    row.onClick.RemoveAllListeners();
                    row.onClick.AddListener(() => SelectFinding(index));
                    rowInstances.Add(row);
                }
            }
            if (markerTemplate != null && markerLayer != null)
            {
                int added = 0;
                for (int i = 0; i < findings.Count && added < maxWorldMarkers; i++)
                {
                    if (findings[i].target == null) continue;
                    RectTransform marker = Instantiate(markerTemplate, markerLayer, false);
                    marker.gameObject.name = "FaultMarker_" + i;
                    marker.gameObject.SetActive(true);
                    TMP_Text label = marker.GetComponentInChildren<TMP_Text>(true);
                    if (label != null)
                    {
                        label.text = findings[i].title;
                        label.color = ColorFor(findings[i].severity);
                    }
                    markerInstances.Add(marker);
                    added++;
                }
            }
            PositionMarkers();
        }

        private Color ColorFor(Severity severity)
        {
            return severity == Severity.Failure ? failureColor :
                   severity == Severity.Warning ? warningColor : informationColor;
        }

        private void ClearGeneratedUi()
        {
            for (int i = 0; i < rowInstances.Count; i++)
                if (rowInstances[i] != null) Destroy(rowInstances[i].gameObject);
            rowInstances.Clear();
            for (int i = 0; i < markerInstances.Count; i++)
                if (markerInstances[i] != null) Destroy(markerInstances[i].gameObject);
            markerInstances.Clear();
        }

        private void PositionMarkers()
        {
            if (markerLayer == null || canvas == null) return;
            if (worldCamera == null) worldCamera = Camera.main;
            int markerIndex = 0;
            for (int i = 0; i < findings.Count && markerIndex < markerInstances.Count; i++)
            {
                Finding finding = findings[i];
                if (finding.target == null) continue;
                RectTransform marker = markerInstances[markerIndex++];
                if (marker == null) continue;
                bool visible = showWorldMarkers && worldCamera != null &&
                    finding.target.gameObject.activeInHierarchy;
                if (!visible) { marker.gameObject.SetActive(false); continue; }
                Vector3 world = finding.target.position + Vector3.up * .15f;
                Vector3 point = worldCamera.WorldToScreenPoint(world);
                visible = point.z > 0f && point.x >= 0f && point.x <= Screen.width &&
                    point.y >= 0f && point.y <= Screen.height;
                if (visible)
                {
                    Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                    visible = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        markerLayer, new Vector2(point.x, point.y), uiCam, out Vector2 local);
                    if (visible)
                    {
                        marker.anchoredPosition = local + new Vector2(0f, 42f);
                        // Present a subtle attention pulse, not a simulated electrical effect.
                        marker.localScale = Vector3.one * (1f + .035f * Mathf.Sin(Time.unscaledTime * 3.5f));
                    }
                }
                if (marker.gameObject.activeSelf != visible) marker.gameObject.SetActive(visible);
            }
        }

        private static void SetText(TMP_Text label, string value)
        {
            if (label != null && !string.Equals(label.text, value, StringComparison.Ordinal))
                label.text = value;
        }

        private void SelectFinding(int index)
        {
            if (index < 0 || index >= findings.Count) return;
            selected = index;
            Finding finding = findings[index];
            SetText(explanationText, finding.explanation +
                (finding.target == null ? "  [No verified 3D location]" : "  [3D target identified]"));
        }

        private void WireButtons()
        {
            if (tabButton != null) tabButton.onClick.AddListener(() => SetExpanded(true));
            if (closeButton != null) closeButton.onClick.AddListener(() => SetExpanded(false));
            if (refreshButton != null) refreshButton.onClick.AddListener(RefreshNow);
            if (markersButton != null) markersButton.onClick.AddListener(ToggleWorldMarkers);
        }

        public void ToggleWorldMarkers()
        {
            showWorldMarkers = !showWorldMarkers;
            if (markersButtonText != null)
                SetText(markersButtonText, showWorldMarkers ? "PINS ON" : "PINS OFF");
            PositionMarkers();
        }

        public void TogglePanel() => SetExpanded(!expanded);
        public void Show() => SetExpanded(true);
        public void Hide() => SetExpanded(false);

        private void SetExpanded(bool value)
        {
            expanded = value;
            if (expandedPanel != null) expandedPanel.SetActive(expanded);
            if (collapsedTab != null) collapsedTab.SetActive(!expanded);
        }
    }
}
