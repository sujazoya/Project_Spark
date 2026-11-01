using System;
using System.Collections.Generic;
using System.Reflection;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;
using UnityEngine;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Bridges one *completed* SparkElectricalSolver solve to the existing
    /// SparkVerifiedElectricalPublisher and SparkMeterVerifiedNodeFeed.
    /// Never modifies solver topology, electrical states, level objectives, or UI.
    ///
    /// Mapping uses explicit scene-binding IDs; object names are never electrical IDs.
    /// Component readings are intentionally limited to resistors: the existing frame
    /// requires a meaningful resistanceOhms field and cannot represent N/A.
    /// Other components can still provide verified terminal potentials.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSolverLiveMeasurementBridge : MonoBehaviour
    {
        [Serializable]
        public sealed class ResistorReadingBinding
        {
            [Tooltip("Stable ID shared by SparkTeachingAnchor.AnchorId and meter ComponentAnchorId.")]
            public string anchorId;
            [Tooltip("ID in SparkLevelSceneBindings (runtime replacements honored).")]
            public string componentBindingId;
            [Tooltip("Optional direct reference; used only when componentBindingId is empty.")]
            public SparkResistor directResistor;
            [Tooltip("Optional scene binding for voltage + terminal; leave empty to use SparkResistor.TerminalA.")]
            public string positiveTerminalBindingId;
            [Tooltip("Optional scene binding for voltage - terminal; leave empty to use SparkResistor.TerminalB.")]
            public string negativeTerminalBindingId;
        }

        [Serializable]
        public sealed class TerminalPotentialBinding
        {
            [Tooltip("Must equal the meter marker's SparkMeterProbeTerminal.TerminalId.")]
            public string terminalId;
            [Tooltip("ID in SparkLevelSceneBindings terminal bindings.")]
            public string terminalBindingId;
            [Tooltip("Optional direct terminal; used only when terminalBindingId is empty.")]
            public SparkTerminal directTerminal;
        }

        [Header("Existing solver and scene bindings")]
        [SerializeField] private SparkElectricalSolver solver;
        [SerializeField] private SparkLevelSceneBindings sceneBindings;

        [Header("Single-scene level loading")]
        [Tooltip("Uses SparkLevelLayoutLoader.ActiveBindings for the current level. " +
                 "Never selects an arbitrary inactive level's bindings.")]
        [SerializeField] private bool followActiveLevel = true;
        [SerializeField] private SparkLevelLayoutLoader levelLoader;
        [Tooltip("Rebuild resistor and terminal mappings from each level's own bindings " +
                 "when its layout loads. Manual mapping arrays are for single-level mode.")]
        [SerializeField] private bool autoMapActiveLevel = true;
        [Tooltip("A short prefix, NOT an electrical terminal/anchor ID.")]
        [SerializeField] private string circuitIdPrefix = "Spark";

        [Header("Existing destinations")]
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private SparkMeterVerifiedNodeFeed nodeFeed;

        [Header("Circuit identity")]
        [Tooltip("REQUIRED. A stable, explicit identifier for the currently loaded level layout. " +
                 "Call SetCircuitIdentity when loading a different level/layout.")]
        [SerializeField] private string circuitId;
        [SerializeField] private bool publishOnEnableIfSolved = true;

        [Header("Explicit component readings (up to 32)")]
        [SerializeField] private ResistorReadingBinding[] resistorBindings =
            Array.Empty<ResistorReadingBinding>();

        [Header("Explicit terminal potentials (up to 128)")]
        [SerializeField] private TerminalPotentialBinding[] terminalBindings =
            Array.Empty<TerminalPotentialBinding>();

        [Header("Diagnostics")]
        [SerializeField] private bool logPublishes;
        [SerializeField] private bool warnAboutIncompleteBindings = true;

        private SparkElectricalSolver subscribedSolver;
        private SparkLevelLayoutLoader subscribedLoader;
        private SparkLevelLayout mappedLayout;
        private bool activeLayoutReady;
        private bool hasCompletedSolveForActiveLayout;
        private int loadSequence;

        private static readonly BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly BindingFlags AnyInstance =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public bool FollowingActiveLevel => followActiveLevel;
        public bool HasActiveLevel => !followActiveLevel || activeLayoutReady;
        public SparkLevelSceneBindings CurrentSceneBindings => sceneBindings;
        public int MappedResistorCount => resistorBindings != null ? resistorBindings.Length : 0;
        public int MappedTerminalCount => terminalBindings != null ? terminalBindings.Length : 0;
        private SparkElectricalSolveResult lastPublishedResult;
        private int revision;
        private int publishedFrames;
        private int publishedNodeFrames;
        private int rejectedSolves;
        private string lastIssue;

        public int LastRevision => revision;
        public int PublishedFrames => publishedFrames;
        public int PublishedNodeFrames => publishedNodeFrames;
        public int RejectedSolves => rejectedSolves;
        public string CurrentCircuitId => circuitId;
        public string LastIssue => lastIssue;

        private void Awake() => ResolveReferences();

        private void OnEnable()
        {
            ResolveReferences();
            BindLoader();
            BindSolver();
            if (followActiveLevel)
                SynchronizeActiveLayout();
        }

        private void Start()
        {
            ResolveReferences();
            BindLoader();
            BindSolver();
            if (followActiveLevel && !SynchronizeActiveLayout())
                return;
            if (publishOnEnableIfSolved)
                PublishCurrentCompletedResult();
        }

        private void OnDisable()
        {
            UnbindSolver();
            UnbindLoader();
            // Force a fresh publication if this bridge is re-enabled.
            lastPublishedResult = null;
        }

        private void OnDestroy()
        {
            UnbindSolver();
            UnbindLoader();
        }

        private void ResolveReferences()
        {
            if (solver == null)
                solver = GetComponent<SparkElectricalSolver>();
            if (solver == null)
                solver = FindFirstObjectByType<SparkElectricalSolver>(FindObjectsInactive.Include);
            if (followActiveLevel)
            {
                if (levelLoader == null)
                    levelLoader = FindFirstObjectByType<SparkLevelLayoutLoader>(FindObjectsInactive.Include);
            }
            else if (sceneBindings == null)
            {
                sceneBindings = FindFirstObjectByType<SparkLevelSceneBindings>(FindObjectsInactive.Include);
            }
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (nodeFeed == null)
                nodeFeed = FindFirstObjectByType<SparkMeterVerifiedNodeFeed>(FindObjectsInactive.Include);
        }


        // ------------------------------------------------------------------
        // ONE UNITY SCENE / MANY LEVEL LAYOUTS
        // ------------------------------------------------------------------

        private void BindLoader()
        {
            if (!followActiveLevel || subscribedLoader == levelLoader)
                return;
            UnbindLoader();
            subscribedLoader = levelLoader;
            if (subscribedLoader != null)
            {
                subscribedLoader.LayoutLoaded += HandleLayoutLoaded;
                subscribedLoader.LayoutUnloaded += HandleLayoutUnloaded;
            }
        }

        private void UnbindLoader()
        {
            if (subscribedLoader != null)
            {
                subscribedLoader.LayoutLoaded -= HandleLayoutLoaded;
                subscribedLoader.LayoutUnloaded -= HandleLayoutUnloaded;
            }
            subscribedLoader = null;
        }

        private void HandleLayoutUnloaded()
        {
            // Never leave previous-level mappings usable while a new layout loads.
            ClearActiveLayout();
            if (logPublishes)
                Debug.Log("[Spark Live Bridge] Layout unloaded; waiting for new level.", this);
        }

        private void HandleLayoutLoaded(SparkLevelLayout layout)
        {
            if (!followActiveLevel)
                return;
            SparkLevelSceneBindings bindings = layout != null ? layout.SceneBindings : null;
            if (bindings == null && levelLoader != null)
                bindings = levelLoader.ActiveBindings;
            // A LayoutLoaded event is a new load even when a layout object was reused.
            ClearActiveLayout();
            ActivateLayout(layout, bindings);
        }

        private void ClearActiveLayout()
        {
            activeLayoutReady = false;
            hasCompletedSolveForActiveLayout = false;
            mappedLayout = null;
            sceneBindings = null;
            circuitId = "";
            revision = 0;
            lastPublishedResult = null;
            lastIssue = "No loaded level layout; refusing old-level measurements.";
            if (autoMapActiveLevel)
            {
                resistorBindings = Array.Empty<ResistorReadingBinding>();
                terminalBindings = Array.Empty<TerminalPotentialBinding>();
            }
        }

        /// <summary>
        /// Safe both on LayoutLoaded and when a solve occurs during a layout switch.
        /// The loader's ActiveBindings is authoritative; inactive scene objects are
        /// never used as fallbacks in active-level mode.
        /// </summary>
        private bool SynchronizeActiveLayout()
        {
            if (!followActiveLevel)
                return true;
            if (levelLoader == null)
            {
                ClearActiveLayout();
                lastIssue = "Missing SparkLevelLayoutLoader. Assign it to the bridge.";
                return false;
            }

            SparkLevelLayout layout = levelLoader.ActiveLayout;
            SparkLevelSceneBindings bindings = levelLoader.ActiveBindings;
            if (!levelLoader.HasActiveLayout || layout == null || bindings == null ||
                !layout.gameObject.activeInHierarchy || !bindings.gameObject.activeInHierarchy)
            {
                if (activeLayoutReady)
                    ClearActiveLayout();
                return false;
            }

            if (activeLayoutReady && mappedLayout == layout && sceneBindings == bindings)
                return true;

            return ActivateLayout(layout, bindings);
        }

        private bool ActivateLayout(SparkLevelLayout layout, SparkLevelSceneBindings bindings)
        {
            if (layout == null || bindings == null || !bindings.gameObject.activeInHierarchy)
            {
                ClearActiveLayout();
                return false;
            }

            // Rebuild only when the loader changes layouts, never per frame.
            bool fresh = !activeLayoutReady || mappedLayout != layout || sceneBindings != bindings;
            if (!fresh)
                return true;

            activeLayoutReady = false;
            hasCompletedSolveForActiveLayout = false;
            mappedLayout = layout;
            sceneBindings = bindings;
            revision = 0;
            lastPublishedResult = null;
            loadSequence++;
            circuitId = MakeLoadIdentity(layout, loadSequence);

            if (autoMapActiveLevel)
                BuildMappingsFromSceneBindings(bindings, layout);

            activeLayoutReady = true;
            lastIssue = null;
            if (logPublishes)
                Debug.Log("[Spark Live Bridge] Active level: " + layout.name +
                          " / " + circuitId + ", mapped " + MappedResistorCount +
                          " resistor(s), " + MappedTerminalCount + " terminal(s).", this);
            return true;
        }

        private string MakeLoadIdentity(SparkLevelLayout layout, int generation)
        {
            string label = layout.LevelDefinition != null
                ? layout.LevelDefinition.name
                : layout.name;
            string prefix = SafeLabel(circuitIdPrefix);
            string level = SafeLabel(label);
            string suffix = "_Run" + generation.ToString();
            // Preserve the run discriminator within the publisher's 80-char limit.
            int available = Math.Max(1, 80 - suffix.Length - prefix.Length - 1);
            if (level.Length > available)
                level = level.Substring(0, available);
            return prefix + "_" + level + suffix;
        }

        private static string SafeLabel(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Level";
            var chars = input.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
                    chars[i] = '_';
            string label = new string(chars);
            return label.Length <= 24 ? label : label.Substring(0, 24);
        }

        // Serialized scene-binding arrays have no public enumeration API.
        // Reflection is read-only, performed once per level load, and respects
        // runtime component replacement via TryGetComponent during each solve.
        private void BuildMappingsFromSceneBindings(
            SparkLevelSceneBindings bindings, SparkLevelLayout layout)
        {
            var resistorMaps = new List<ResistorReadingBinding>();
            var nodeMaps = new List<TerminalPotentialBinding>();
            var usedAnchors = new HashSet<string>(StringComparer.Ordinal);
            var usedTerminals = new HashSet<string>(StringComparer.Ordinal);

            foreach (object entry in ReadBindingArray(bindings, "components"))
            {
                if (resistorMaps.Count >= 32)
                    break;
                string id = ReadBindingProperty(entry, "Id") as string;
                var component = ReadBindingProperty(entry, "Component") as SparkResistor;
                if (!ValidId(id) || component == null)
                    continue;
                string anchor = FindAnchorId(component, id);
                if (!ValidId(anchor) || !usedAnchors.Add(anchor))
                    continue;
                resistorMaps.Add(new ResistorReadingBinding
                {
                    anchorId = anchor,
                    componentBindingId = id
                });
            }

            foreach (object entry in ReadBindingArray(bindings, "terminals"))
            {
                if (nodeMaps.Count >= 128)
                    break;
                string id = ReadBindingProperty(entry, "Id") as string;
                var terminal = ReadBindingProperty(entry, "Terminal") as SparkTerminal;
                if (!ValidId(id) || terminal == null)
                    continue;
                string markerId = FindTerminalId(terminal, id);
                if (!ValidId(markerId) || !usedTerminals.Add(markerId))
                    continue;
                nodeMaps.Add(new TerminalPotentialBinding
                {
                    terminalId = markerId,
                    terminalBindingId = id
                });
            }

            // Probe markers sometimes exist on terminal colliders not listed in
            // SparkLevelSceneBindings. Match ONLY an actual SparkTerminal ancestor.
            foreach (var marker in layout.GetComponentsInChildren<SparkMeterProbeTerminal>(true))
            {
                if (nodeMaps.Count >= 128)
                    break;
                if (marker == null || !ValidId(marker.TerminalId) ||
                    usedTerminals.Contains(marker.TerminalId))
                    continue;
                SparkTerminal terminal = marker.GetComponent<SparkTerminal>();
                if (terminal == null)
                    terminal = marker.GetComponentInParent<SparkTerminal>();
                if (terminal == null)
                    continue;
                usedTerminals.Add(marker.TerminalId);
                nodeMaps.Add(new TerminalPotentialBinding
                {
                    terminalId = marker.TerminalId,
                    directTerminal = terminal
                });
            }

            resistorBindings = resistorMaps.ToArray();
            terminalBindings = nodeMaps.ToArray();
            if (warnAboutIncompleteBindings && resistorMaps.Count == 0 && nodeMaps.Count == 0)
                Debug.LogWarning("[Spark Live Bridge] Active level '" + layout.name +
                                 "' has no mapped resistor or terminal IDs. " +
                                 "Fill that level's SparkLevelSceneBindings and probe markers.", this);
        }

        private static IEnumerable<object> ReadBindingArray(SparkLevelSceneBindings bindings, string field)
        {
            FieldInfo fieldInfo = typeof(SparkLevelSceneBindings).GetField(field, PrivateInstance);
            if (fieldInfo == null)
                yield break;
            if (!(fieldInfo.GetValue(bindings) is Array entries))
                yield break;
            foreach (object entry in entries)
                if (entry != null)
                    yield return entry;
        }

        private static object ReadBindingProperty(object entry, string property)
        {
            PropertyInfo info = entry.GetType().GetProperty(property, AnyInstance);
            return info != null ? info.GetValue(entry, null) : null;
        }

        private static string FindAnchorId(SparkResistor resistor, string fallback)
        {
            SparkTeachingAnchor anchor = resistor.GetComponent<SparkTeachingAnchor>();
            if (anchor != null && ValidId(anchor.AnchorId))
                return anchor.AnchorId;
            string found = null;
            foreach (var marker in resistor.GetComponentsInChildren<SparkMeterProbeTerminal>(true))
            {
                if (marker == null || !ValidId(marker.ComponentAnchorId))
                    continue;
                if (found != null && found != marker.ComponentAnchorId)
                    return fallback; // Ambiguous; do not invent a meter ID.
                found = marker.ComponentAnchorId;
            }
            return found ?? fallback;
        }

        private static string FindTerminalId(SparkTerminal terminal, string fallback)
        {
            string found = null;
            foreach (var marker in terminal.GetComponentsInChildren<SparkMeterProbeTerminal>(true))
            {
                if (marker == null || !ValidId(marker.TerminalId))
                    continue;
                if (found != null && found != marker.TerminalId)
                    return fallback;
                found = marker.TerminalId;
            }
            return found ?? fallback;
        }

        /// <summary>Use when an external loader explicitly changes the active layout.
        /// Normally unnecessary: LayoutLoaded/LayoutUnloaded are already observed.</summary>
        public bool RefreshActiveLevelNow()
        {
            ResolveReferences();
            BindLoader();
            return SynchronizeActiveLayout();
        }

        private void BindSolver()
        {
            if (subscribedSolver == solver)
                return;
            UnbindSolver();
            subscribedSolver = solver;
            if (subscribedSolver != null)
            {
                subscribedSolver.SolveCompleted += OnSolveCompleted;
                subscribedSolver.SolveFailed += OnSolveFailed;
            }
        }

        private void UnbindSolver()
        {
            if (subscribedSolver != null)
            {
                subscribedSolver.SolveCompleted -= OnSolveCompleted;
                subscribedSolver.SolveFailed -= OnSolveFailed;
            }
            subscribedSolver = null;
        }

        private void OnSolveCompleted()
        {
            if (followActiveLevel)
            {
                // Ignore solver events during the gap between level unload/load.
                if (!SynchronizeActiveLayout())
                    return;
                hasCompletedSolveForActiveLayout = true;
            }
            PublishCurrentCompletedResult();
        }

        private void OnSolveFailed()
        {
            rejectedSolves++;
            lastIssue = "Electrical solve failed; no new verified data published.";
            // Do not publish old/zero data as though it were a new completed solve.
        }

        /// <summary>
        /// Call from level-layout loader whenever the identity of the level changes.
        /// Keep this ID constant for component/wiring experiments within that layout.
        /// </summary>
        public bool SetCircuitIdentity(string newCircuitId)
        {
            if (!ValidId(newCircuitId))
            {
                lastIssue = "Circuit ID must be nonblank and at most 80 characters.";
                Debug.LogWarning("[Spark Live Bridge] " + lastIssue, this);
                return false;
            }
            if (string.Equals(circuitId, newCircuitId, StringComparison.Ordinal))
                return true;

            circuitId = newCircuitId;
            revision = 0;
            lastPublishedResult = null;
            lastIssue = null;
            return true;
        }

        [ContextMenu("Publish Current Completed Solver Result")]
        public bool PublishCurrentCompletedResult()
        {
            ResolveReferences();
            if (followActiveLevel)
                BindLoader();
            if (followActiveLevel && !SynchronizeActiveLayout())
                return Reject("No active level layout and bindings are ready. Waiting for LayoutLoaded.");
            if (followActiveLevel && !hasCompletedSolveForActiveLayout)
                return Reject("Waiting for a fresh SolveCompleted event from this loaded level.");
            if (solver == null || publisher == null)
                return Reject("Missing SparkElectricalSolver or SparkVerifiedElectricalPublisher.");
            if (!ValidId(circuitId))
                return Reject("Set a circuitId (1-80 characters) in the bridge Inspector first.");
            if (solver.IsSolving || solver.IsDirty || solver.LatestSolveResult == null)
                return Reject("No clean, completed solver result is available.");

            SparkElectricalSolveResult result = solver.LatestSolveResult;
            if (ReferenceEquals(lastPublishedResult, result))
                return false; // No fake new frame / revision for an unchanged result.
            if (result.Context == null || result.Context.Graph == null)
                return Reject("Completed solve has no topology graph.");
            if (revision == int.MaxValue)
                return Reject("Revision exhausted. Set a new circuit identity.");
            if (resistorBindings != null && resistorBindings.Length > 32)
                return Reject("Too many resistor bindings (maximum 32). Use a smaller list.");
            if (terminalBindings != null && terminalBindings.Length > 128)
                return Reject("Too many terminal bindings (maximum 128). Use a smaller list.");

            var componentReadings = new List<SparkVerifiedElectricalReading>();
            var potentials = new List<SparkMeterVerifiedNode>();
            var anchors = new HashSet<string>(StringComparer.Ordinal);
            var terminalIds = new HashSet<string>(StringComparer.Ordinal);
            int incomplete = 0;

            if (resistorBindings != null)
            {
                foreach (ResistorReadingBinding mapping in resistorBindings)
                {
                    if (mapping == null || !ValidId(mapping.anchorId) || !anchors.Add(mapping.anchorId))
                        return Reject("Invalid or duplicate resistor anchorId in component mappings.");
                    if (!TryReadResistor(result, mapping, out SparkVerifiedElectricalReading reading))
                    {
                        incomplete++;
                        continue;
                    }
                    componentReadings.Add(reading);
                }
            }

            if (terminalBindings != null)
            {
                foreach (TerminalPotentialBinding mapping in terminalBindings)
                {
                    if (mapping == null || !ValidId(mapping.terminalId) || !terminalIds.Add(mapping.terminalId))
                        return Reject("Invalid or duplicate terminalId in node mappings.");
                    if (!TryReadNode(result, mapping, out SparkMeterVerifiedNode node))
                    {
                        incomplete++;
                        continue;
                    }
                    potentials.Add(node);
                }
            }

            if (componentReadings.Count == 0 && potentials.Count == 0)
                return Reject("No resolved readings or terminal potentials; check scene bindings.");

            int nextRevision = revision + 1;
            var frame = new SparkVerifiedElectricalFrame
            {
                circuitId = circuitId,
                revision = nextRevision,
                readings = componentReadings,
                hasVerifiedRoute = false,
                hasVerifiedFault = false
            };

            // Node feed requires a positive matching revision and one common solve reference.
            var nodeFrame = new SparkMeterVerifiedNodeFrame
            {
                circuitId = circuitId,
                revision = nextRevision,
                nodes = potentials
            };

            // Validate node payload BEFORE delivering either event. Both are built from result.
            if (nodeFeed != null && potentials.Count > 0 &&
                !ValidateNodePayload(nodeFrame))
                return Reject("Node-potential frame validation failed; nothing published.");

            // Mark the snapshot before events to avoid reentrant duplicate publication.
            revision = nextRevision;
            lastPublishedResult = result;
            lastIssue = incomplete > 0 ? incomplete + " map(s) unresolved for this solve." : null;

            // Publisher and feed copy their inputs before notifying consumers.
            // Never assert a fabricated route, fault classification or tutorial action.
            try
            {
                publisher.PublishVerifiedFrame(frame);
                publishedFrames++;
            }
            catch (Exception exception)
            {
                lastIssue = "Publisher subscriber threw: " + exception.Message;
                Debug.LogException(exception, this);
                // No replay of the same result: subscribers may already have observed it.
                return false;
            }

            if (nodeFeed != null && potentials.Count > 0)
            {
                try
                {
                    if (nodeFeed.PublishVerifiedNodes(nodeFrame))
                        publishedNodeFrames++;
                    else
                        lastIssue = "Node feed rejected the completed node frame.";
                }
                catch (Exception exception)
                {
                    lastIssue = "Node-feed subscriber threw: " + exception.Message;
                    Debug.LogException(exception, this);
                }
            }
            if (warnAboutIncompleteBindings && incomplete > 0)
                Debug.LogWarning("[Spark Live Bridge] " + lastIssue, this);
            if (logPublishes)
                Debug.Log("[Spark Live Bridge] " + circuitId + " revision " + revision +
                          ": " + componentReadings.Count + " resistor(s), " + potentials.Count +
                          " terminal(s).", this);
            return true;
        }

        private bool TryReadResistor(
            SparkElectricalSolveResult result,
            ResistorReadingBinding mapping,
            out SparkVerifiedElectricalReading reading)
        {
            reading = null;
            SparkResistor resistor = null;
            if (!string.IsNullOrWhiteSpace(mapping.componentBindingId))
            {
                if (sceneBindings == null ||
                    !sceneBindings.TryGetComponent(mapping.componentBindingId,
                        out SparkElectricalComponent boundComponent))
                    return false;
                resistor = boundComponent as SparkResistor;
            }
            else
            {
                resistor = mapping.directResistor;
            }

            if (resistor == null || !resistor.isActiveAndEnabled || !resistor.ElectricalEnabled)
                return false;
            if (solver.SolverRegistry != null && solver.SolverRegistry.IsIgnored(resistor))
                return false;

            SparkTerminal positive = resistor.TerminalA;
            SparkTerminal negative = resistor.TerminalB;
            if (!string.IsNullOrWhiteSpace(mapping.positiveTerminalBindingId))
            {
                if (sceneBindings == null ||
                    !sceneBindings.TryGetTerminal(mapping.positiveTerminalBindingId, out positive))
                    return false;
            }
            if (!string.IsNullOrWhiteSpace(mapping.negativeTerminalBindingId))
            {
                if (sceneBindings == null ||
                    !sceneBindings.TryGetTerminal(mapping.negativeTerminalBindingId, out negative))
                    return false;
            }
            if (positive == null || negative == null || positive == negative)
                return false;
            if (!result.TryGetVoltage(positive, negative, out float voltage) ||
                !solver.TryCalculateComponentCurrent(resistor, out float current))
                return false;

            // EffectiveResistance is the model's own resistance, not V/I inferred
            // by this bridge. No resistance is invented for non-resistor devices.
            float resistance = resistor.EffectiveResistance;
            if (!Finite(voltage) || !Finite(current) || !Finite(resistance) || resistance < 0f)
                return false;

            reading = new SparkVerifiedElectricalReading
            {
                anchorId = mapping.anchorId,
                voltageVolts = voltage,
                currentAmps = current,
                resistanceOhms = resistance,
                condition = resistor.IsBurned ? SparkElectricalCondition.Failed : SparkElectricalCondition.Unknown,
                explanation = "V=terminal A minus B, I=solver definition direction, " +
                              "R=component effective resistance. No inferred fault classification."
            };
            return true;
        }

        private bool TryReadNode(
            SparkElectricalSolveResult result,
            TerminalPotentialBinding mapping,
            out SparkMeterVerifiedNode node)
        {
            node = null;
            SparkTerminal terminal;
            if (!string.IsNullOrWhiteSpace(mapping.terminalBindingId))
            {
                if (sceneBindings == null ||
                    !sceneBindings.TryGetTerminal(mapping.terminalBindingId, out terminal))
                    return false;
            }
            else
            {
                terminal = mapping.directTerminal;
            }

            if (terminal == null || terminal.Owner == null ||
                !terminal.Owner.isActiveAndEnabled ||
                (terminal.Owner is SparkElectricalComponent electricalOwner &&
                    !electricalOwner.ElectricalEnabled) ||
                !result.TryGetTerminalVoltage(terminal, out float volts) || !Finite(volts))
                return false;

            node = new SparkMeterVerifiedNode
            {
                terminalId = mapping.terminalId,
                potentialVolts = volts
            };
            return true;
        }

        private static bool ValidateNodePayload(SparkMeterVerifiedNodeFrame nodes)
        {
            if (nodes == null || nodes.revision <= 0 || !ValidId(nodes.circuitId) ||
                nodes.nodes == null || nodes.nodes.Count > 128)
                return false;
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (SparkMeterVerifiedNode node in nodes.nodes)
            {
                if (node == null || !ValidId(node.terminalId) ||
                    !used.Add(node.terminalId) || !Finite(node.potentialVolts))
                    return false;
            }
            return true;
        }

        private bool Reject(string reason)
        {
            rejectedSolves++;
            lastIssue = reason;
            if (warnAboutIncompleteBindings)
                Debug.LogWarning("[Spark Live Bridge] " + reason, this);
            return false;
        }

        private static bool Finite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool ValidId(string id) =>
            !string.IsNullOrWhiteSpace(id) && id.Length <= 80;
    }
}
