using System;
using System.Collections;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Visual and locomotion executor for the *existing* native-wire director.
    /// No fake cable and no direct circuit graph mutation. The player's wire port
    /// is the sole owner of the physical cable and the electrical link.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(850)] // Sample the original hand after character animation and the 825 arm solve.
    public sealed class SparkNativeWireCinematic : MonoBehaviour
    {
        [Header("Authoritative existing services")]
        [SerializeField] private SparkProCharacter character;
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private NavMeshAgent navigation;
        [SerializeField] private SparkV5FlightController flight;
        [SerializeField] private bool autoFlyWhenWalkingCannotReach = true;
        [SerializeField] private bool allowReviewedPointerFallback;

        [Header("Navigation: no teleport or unverified floor sliding")]
        [SerializeField] private bool enableExistingAgentWhenNavMeshValid = true;
        [SerializeField, Range(0.1f, 1.0f)] private float maximumAgentStartOffset = 0.25f;
        [SerializeField, Range(0.15f, 1.5f)] private float desiredWorkbenchReach = 0.52f;
        [SerializeField, Range(2f, 40f)] private float walkTimeoutSeconds = 20f;
        [SerializeField, Range(0.2f, 3f)] private float walkingSpeed = 1.0f;

        [Header("Physical hand performance")]
        [SerializeField, Range(0.15f, 2.0f)] private float reachSeconds = 0.65f;
        [SerializeField, Range(0.05f, 1f)] private float gripPauseSeconds = 0.2f;
        [SerializeField, Range(0.05f, 1f)] private float releasePauseSeconds = 0.28f;
        [SerializeField, Range(1f, 25f)] private float ikBlendSpeed = 14f;
        [SerializeField, Range(0.01f, 0.2f)] private float handTipOffset = 0.045f;
        [SerializeField] private bool useLeftHandForSupport = true;

        [Header("Teaching narrative")]
        [SerializeField] private bool announceEachWire = true;

        [Header("Physical cable")]
        [Tooltip("Zero selects a lead with the confirmed span plus the handling allowance.")]
        [SerializeField, Min(0)] private float cableRestLength;
        [SerializeField, Range(.25f, 2f)] private float handlingAllowance = .9f;
        [SerializeField, Range(8, 48)] private int cableSegments = 24;
        [SerializeField, Range(2, 16)] private int cableIterations = 7;
        [SerializeField, Range(0f, 2f)] private float cableGravity = 1f;
        [SerializeField, Range(.01f, .4f)] private float cableDamping = .09f;
        [SerializeField, Range(0f, 1f)] private float cableBendStiffness = .12f;
        [SerializeField, Range(.004f, .06f)] private float cableCollisionRadius = .02f;
        [SerializeField, Range(0f, .1f)] private float maximumCableExtension = .025f;
        [SerializeField] private LayerMask cableCollisionLayers = ~0;
        [SerializeField, Range(.015f, .1f)] private float insertionDistance = .035f;
        [SerializeField, Range(.05f, .6f)] private float insertionSeconds = .22f;
        [SerializeField, Range(.0005f, .015f)] private float settlingTolerance = .003f;

        public enum CableStage { Idle, Prepare, SourceAttached, Grip, Carrying, Aligning, Inserting, Attached, Settling, Complete, Cancelled, Retracting,
            PreGrip, GripClosure, GripVerified, Lift, PreInsert, Release }
        private CableStage stage;
        public event Action StageChanged;
        public CableStage Stage
        {
            get => stage;
            private set
            {
                if (stage == value) return;
                stage = value;
                StageChanged?.Invoke();
            }
        }
        public bool IsPhysicalCable => physicalPort && !physicalPort.UsesPlayerPointer;
        private readonly SparkCableSimulation cable = new SparkCableSimulation();
        private readonly Collider[] socketOverlaps = new Collider[16];
        private static readonly float[] ApproachStandoffs = { .38f, .55f, .85f, 1.2f, 1.6f, 2.1f };
        private SparkAINativeSignalWirePort physicalPort;
        private Transform attachedSource;
        private bool usingPlayerPointer;
        private Vector3 pointerStart, pointerEnd;
        private float pointerStarted;
        private float insertionProgress;

        private struct HandRig
        {
            public Transform Shoulder, Elbow, Wrist, Thumb, Index, Middle;
            public Transform LeftShoulder, LeftElbow, LeftWrist;
            public bool IsReady => Shoulder && Elbow && Wrist && Thumb && Index && Middle;
            public Vector3 GripPoint => Vector3.Lerp(Thumb.position, (Index.position + Middle.position) * .5f, .65f);
        }

        private HandRig[] handRigs;
        private SparkAAANarrationDirector narration;
        private SparkWorkbenchNavigation surfaceNavigation;
        private Transform shoulderR => handRigs[0].Shoulder;
        private Transform elbowR => handRigs[0].Elbow;
        private Transform wristR => handRigs[0].Wrist;
        private SparkAINativeWirePort livePort;
        private bool usingCable, trackingHands, stageActive, reaching;
        private SparkTelescopicNativeWireBridge telescopicBridge;
        private SparkTelescopicArms pressingArm;
        private Vector3 gripTarget;
        private const float CarryReach = .40f;
        private const float WalkingCarrySideClearance = .10f;
        private bool trackingCarryPose;
        private Vector3 appliedCableGrip;
        private Vector3 previewRootPosition;
        private float ikWeight;
        private bool activatedAgent;
        private float previousAgentSpeed;
        private float previousStoppingDistance;
        private static readonly float[] LaunchStandoffs = { .8f, 1.2f, 1.6f };
        private string failure;
        private bool succeeded;

        public string LastError => failure;
        public bool Succeeded => succeeded;
        public bool IsPerforming => stageActive;
        public float SuggestedWireTimeoutSeconds =>
            walkTimeoutSeconds * 2f + reachSeconds * 2f + gripPauseSeconds + releasePauseSeconds + 4f +
            (flight ? flight.WireFlightTimeBudgetSeconds : 0f);
        public bool HasRig => handRigs != null && handRigs.Length > 0 && handRigs[0].IsReady;
        public bool HasNavMesh => navigation && navigation.isActiveAndEnabled && navigation.isOnNavMesh;
        public bool TryMeasureHeldCable(out Vector3 end, out float restLength, out float routeLength, out float extension)
        {
            end = default;
            restLength = routeLength = extension = 0f;
            var points = cable.Points;
            if (!usingCable || points == null || points.Length < 2) return false;
            end = points[points.Length - 1];
            restLength = cable.Length;
            extension = cable.Extension;
            for (int i = 1; i < points.Length; i++)
                routeLength += Vector3.Distance(points[i - 1], points[i]);
            return true;
        }

        public bool TryGetHeldCableGrip(out Vector3 position)
        {
            if (usingCable && HasRig)
            {
                position = appliedCableGrip;
                return true;
            }
            position = default;
            return false;
        }

        public bool TryMeasureNativeMotion(out NativeMotionSample sample)
        {
            sample = default;
            var arm = GetComponent<SparkTelescopicArms>();
            if (!arm || !arm.IsBound) return false;
            arm.SampleNativeGrip(out sample.Aperture, out sample.EndDiameter, out sample.WristAxisError,
                out sample.ExtensionRatio, out sample.IkCorrection, out sample.RestOffset);
            sample.HandPositionError = arm.RightGripError;
            sample.FrameMilliseconds = Time.unscaledDeltaTime * 1000f;
            if (!usingCable || !attachedSource || cable.Points == null) return true;
            sample.CableContacts = cable.Contacts;
            sample.KnotMotion = cable.LastKnotMotion;
            sample.CableStretch = cable.Extension;
            sample.PreviewGap = Vector3.Distance(cable.Points[cable.Points.Length - 1], appliedCableGrip);
            SparkAINativeSignalWirePort.TryValidateCableGeometry(cable.Points, attachedSource.position, cable.Length,
                maximumCableExtension, out sample.PathLength, out sample.PeakSegmentRatio, out _);
            return true;
        }

        public struct NativeMotionSample
        {
            public float Aperture, EndDiameter, WristAxisError, ExtensionRatio, IkCorrection, RestOffset;
            public float HandPositionError, FrameMilliseconds, KnotMotion, CableStretch, PreviewGap, PathLength, PeakSegmentRatio;
            public int CableContacts;
        }

        private void Awake()
        {
            if (!character) character = GetComponent<SparkProCharacter>();
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!navigation) navigation = GetComponent<NavMeshAgent>();
            if (!flight) flight = GetComponent<SparkV5FlightController>();
            narration = GetComponent<SparkAAANarrationDirector>();
        }

        private void OnDisable() { CancelPerformance(); }

        public void CancelPerformance()
        {
            if (pressingArm && !pressingArm.IsRightArmResting) pressingArm.RetractRight();
            pressingArm = null;
            if (telescopicBridge) telescopicBridge.ReleaseArm();
            if (flight && stageActive) flight.StopMotionAndHover();
            usingCable = false;
            trackingCarryPose = false;
            usingPlayerPointer = false;
            physicalPort = null;
            attachedSource = null;
            if (stageActive && !succeeded) Stage = CableStage.Cancelled;
            livePort = null;
            trackingHands = false;
            surfaceNavigation = null;
            ikWeight = 0f;
            stageActive = false;
            if (character) { character.StopNavigation(); character.ClearGesture(); }
            RestoreAgent();
        }

        private bool BindRig()
        {
            if (!character) return Fail("SparkProCharacter is missing; cannot animate real hands.");
            if (HasRig) return true;
            handRigs = new HandRig[character.RigCount];
            for (int i = 0; i < handRigs.Length; i++)
            {
                var index = character.GetAnchor("FingerTip0_R", i);
                var middle = character.GetAnchor("FingerTip1_R", i);
                handRigs[i] = new HandRig
                {
                    Shoulder = character.GetAnchor("Shoulder_R", i),
                    Elbow = character.GetAnchor("Elbow_R", i),
                    Wrist = character.GetAnchor("Wrist_R", i),
                    Thumb = character.GetAnchor("Thumb_R", i),
                    Index = index ? index : character.GetAnchor("Finger0_R", i),
                    Middle = middle ? middle : character.GetAnchor("Finger1_R", i),
                    LeftShoulder = character.GetAnchor("Shoulder_L", i),
                    LeftElbow = character.GetAnchor("Elbow_L", i),
                    LeftWrist = character.GetAnchor("Wrist_L", i)
                };
            }
            if (!HasRig) return Fail("Spark's real arm or grip pivots are missing.");
            return true;
        }

        private bool Fail(string why)
        {
            failure = why;
            Debug.LogWarning("[Spark Wire Performance] " + why, this);
            return false;
        }

        private void Narrate(string message)
        {
            if (announceEachWire && executive && !narration) executive.PublishWirePerformanceNarration(message);
            Debug.Log("[Spark Wire Performance] " + message, this);
        }

        private bool PrepareNavigation(SparkWorkbenchNavigation workbench, Transform layout)
        {
            if (flight && flight.ManualFlightIsBusy)
                return Fail("Wait for Spark's manual flight to finish before wiring.");
            if (flight && flight.IsFlying) return true; // Airborne: flight safely owns root movement.
            if (!navigation)
            {
                if (autoFlyWhenWalkingCannotReach && flight && flight.AllowAutoWiringFlight) return true;
                return Fail("Spark has no NavMeshAgent. Bind its existing navigation component.");
            }
            if (character && character.NavigationAgent != navigation)
                return Fail("SparkProCharacter's navigation reference does not match the on-character NavMeshAgent. Run the v12.2 binder.");
            if (!navigation.enabled && !enableExistingAgentWhenNavMeshValid)
                return Fail("Navigation is disabled on Spark's character prefab.");
            if (workbench && (!navigation.enabled || workbench.NeedsRefresh(navigation, layout)))
                workbench.TryPrepare(character, layout, out _, out _, out _, Mathf.Min(maximumAgentStartOffset, .06f));
            if (!navigation.enabled)
            {
                var filter = new NavMeshQueryFilter { agentTypeID = navigation.agentTypeID, areaMask = navigation.areaMask };
                if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                        maximumAgentStartOffset, filter) ||
                    Vector3.Distance(hit.position, transform.position) > maximumAgentStartOffset)
                {
                    if (autoFlyWhenWalkingCannotReach && flight && flight.AllowAutoWiringFlight)
                        return true; // Controlled flight does not require a floor NavMesh.
                    return Fail("No baked NavMesh under Spark. Bake a walkable AI path on the laboratory workbench; the robot will NOT slide or teleport.");
                }
                navigation.enabled = true;
                activatedAgent = true;
            }
            if (!navigation.isOnNavMesh)
            {
                if (autoFlyWhenWalkingCannotReach && flight && flight.AllowAutoWiringFlight) return true;
                return Fail("Spark's NavMeshAgent is not standing on a baked walkable surface.");
            }
            ConfigureWalkingAgent();
            return true;
        }

        private void ConfigureWalkingAgent()
        {
            if (previousAgentSpeed > 0f) return;
            previousAgentSpeed = navigation.speed;
            previousStoppingDistance = navigation.stoppingDistance;
            navigation.speed = walkingSpeed;
            navigation.stoppingDistance = .02f;
        }

        private void RestoreAgent()
        {
            if (!navigation) return;
            if (navigation.isActiveAndEnabled && navigation.isOnNavMesh)
            {
                navigation.isStopped = true;
                navigation.ResetPath();
            }
            if (previousAgentSpeed > 0)
            {
                navigation.speed = previousAgentSpeed;
                navigation.stoppingDistance = previousStoppingDistance;
                previousAgentSpeed = 0f;
            }
            if (activatedAgent && (!flight || !flight.IsFlying)) navigation.enabled = false;
            activatedAgent = false;
        }

        private float ArmLength()
        {
            return Vector3.Distance(shoulderR.position, elbowR.position) +
                   Vector3.Distance(elbowR.position, wristR.position);
        }

        private bool CanPhysicallyReach(Vector3 target, Vector3 destination, float arrivalAllowance = 0f)
        {
            Quaternion facing = GroundFacing(target, destination);
            Vector3 localShoulder = Quaternion.Inverse(transform.rotation) * (shoulderR.position - transform.position);
            Vector3 displacedShoulder = destination + facing * localShoulder;
            float reach = ArmLength() - handTipOffset - .025f - arrivalAllowance;
            return reach > 0.1f && Vector3.Distance(displacedShoulder, target) <= reach;
        }

        // Route decision remains evidence-based: a full NavMesh path and real arm reach
        // are mandatory. No teleportation, remote cable attachment or graph fabrication.
        private bool SelectWalkingDestination(Vector3 target, Vector3 groundStart,
            NavMeshQueryFilter filter, out Vector3 spot, out string diagnosis)
        {
            spot = groundStart;
            diagnosis = null;
            float fullArm = ArmLength();
            float reach = fullArm - handTipOffset - 0.025f;
            if (reach <= 0.1f)
            {
                diagnosis = "Right arm is too short or its joints are incorrectly positioned: measured arm=" +
                    fullArm.ToString("F3") + "m, usable=" + reach.ToString("F3") + "m.";
                return false;
            }

            // The target's HEIGHT is a hard constraint. Baking more triangles will not
            // help if the right shoulder cannot physically reach a terminal at this Y.
            float shoulderOffset = (Quaternion.Inverse(transform.rotation) * (shoulderR.position - transform.position)).y;
            float shoulderHeightGap = Mathf.Abs(target.y - groundStart.y - shoulderOffset);
            float planarReach = Mathf.Sqrt(Mathf.Max(0f, reach * reach -
                shoulderHeightGap * shoulderHeightGap));
            if (shoulderHeightGap >= reach || planarReach < 0.035f)
            {
                diagnosis = "Terminal vertical height is beyond Spark's actual right-arm reach " +
                    "(height gap=" + shoulderHeightGap.ToString("F3") + "m, arm=" +
                    reach.ToString("F3") + "m). Adjust avatar height/scale or give Spark " +
                    "a reachable workstation stance; do not bypass the reach check.";
                return false;
            }

            // Previous version sampled only two fixed rings at ~0.42/0.52m, which
            // could exclude every possible location for a small Spark character.
            // Search near-to-far rings relative to the *real measured* planar reach.
            float[] fractions = { 0.18f, 0.28f, 0.38f, 0.48f, 0.58f,
                                  0.68f, 0.78f, 0.88f, 0.96f };
            const int directions = 32;
            const float maxSnapDistance = 0.24f;
            int sampled = 0, withinReach = 0, completePaths = 0;
            float shortestGap = float.MaxValue;
            float bestCost = float.MaxValue;
            NavMeshPath path = new NavMeshPath();

            for (int r = 0; r < fractions.Length; r++)
            {
                float radius = Mathf.Max(0.025f, (planarReach + .15f) * fractions[r]);
                for (int i = 0; i < directions; i++)
                {
                    float angle = (i + (r % 2 == 0 ? 0f : 0.5f)) *
                        Mathf.PI * 2f / directions;
                    Vector3 radial = new Vector3(Mathf.Cos(angle), 0f,
                        Mathf.Sin(angle)) * radius;
                    Vector3 requested = new Vector3(target.x + radial.x,
                        groundStart.y, target.z + radial.z);
                    if (!NavMesh.SamplePosition(requested, out NavMeshHit hit,
                        maxSnapDistance, filter)) continue;
                    // Don't count a distant snapped point as the requested work stance.
                    if (Vector3.Distance(hit.position, requested) > maxSnapDistance)
                        continue;
                    sampled++;
                    float required = Mathf.Abs(target.y - hit.position.y - shoulderOffset);
                    shortestGap = Mathf.Min(shortestGap, required);
                    if (!CanPhysicallyReach(target, hit.position, .055f)) continue;
                    withinReach++;
                    if (!NavMesh.CalculatePath(groundStart, hit.position, filter, path) ||
                        path.status != NavMeshPathStatus.PathComplete) continue;
                    completePaths++;
                    float length = 0f;
                    for (int p = 1; p < path.corners.Length; p++)
                        length += Vector3.Distance(path.corners[p - 1], path.corners[p]);
                    float cost = length + Vector3.Distance(hit.position, requested) * 3f;
                    if (cost >= bestCost || !GroundRouteIsClear(path)) continue;
                    bestCost = cost;
                    spot = hit.position;
                }
            }

            if (!float.IsPositiveInfinity(bestCost)) return true;
            if (sampled == 0)
                diagnosis = "No NavMesh surface near the source terminal within Spark's " +
                    "physical working radius (" + planarReach.ToString("F3") +
                    "m). The NavMesh may exist under Spark but not near this terminal.";
            else if (withinReach == 0)
                diagnosis = "NavMesh samples exist (" + sampled + ") but all are outside " +
                    "Spark's measured arm reach. Closest shoulder-to-terminal gap=" +
                    shortestGap.ToString("F3") + "m, usable arm=" + reach.ToString("F3") +
                    "m. Move or scale the character's working stance or check terminal height.";
            else
                diagnosis = "Found " + withinReach + " physically reachable NavMesh stance(s), " +
                    "but none has a COMPLETE path from Spark's current position. Check obstacles, " +
                    "disconnected NavMesh islands and agent-type bake settings.";
            return false;
        }

        // Rotate the body on its current NavMesh location, without moving or teleporting.
        private void FaceWorkingTerminal(Transform terminal)
        {
            if (terminal) transform.rotation = GroundFacing(terminal.position, transform.position);
        }

        private Quaternion GroundFacing(Vector3 target, Vector3 position)
        {
            Vector3 forward = Vector3.ProjectOnPlane(target - position, Vector3.up);
            // Turn the working shoulder toward the terminal while staying upright.
            return forward.sqrMagnitude > .0025f
                ? Quaternion.LookRotation(forward, Vector3.up) * Quaternion.AngleAxis(-40f, Vector3.up)
                : transform.rotation;
        }

        private float StandingShoulderY(SparkWorkbenchNavigation workbench)
        {
            float groundY = HasNavMesh ? navigation.nextPosition.y : transform.position.y;
            if (workbench && workbench.TryGetTabletop(out var table)) groundY = table.bounds.max.y;
            return groundY + (Quaternion.Inverse(transform.rotation) * (shoulderR.position - transform.position)).y;
        }

        private bool SocketIsClear(Transform terminal, Transform layout)
        {
            int count = Physics.OverlapSphereNonAlloc(terminal.position, .015f, socketOverlaps,
                cableCollisionLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var obstacle = socketOverlaps[i];
                if (obstacle.transform.IsChildOf(layout) || obstacle.transform.IsChildOf(transform) ||
                    surfaceNavigation && surfaceNavigation.TryGetTabletop(out var table) && obstacle == table)
                    continue;
                // A board under its socket may overlap the contact point. Only
                // external geometry that encloses the socket from above blocks use.
                if (obstacle.bounds.max.y < terminal.position.y + .12f) continue;
                if (obstacle is MeshCollider mesh && !mesh.convex) continue;
                if ((obstacle.ClosestPoint(terminal.position) - terminal.position).sqrMagnitude < .000001f)
                    return Fail(obstacle.name + " physically blocks " + terminal.name + ".");
            }
            return true;
        }

        private IEnumerator ApproachWireTerminal(Transform terminal, SparkWorkbenchNavigation workbench,
            Transform layout, bool pointer, Func<bool> stillValid, bool allowOriginalHand = false)
        {
            if (!stillValid() || !terminal || !SocketIsClear(terminal, layout)) yield break;
            float upperReach = StandingShoulderY(workbench) + ArmLength() - handTipOffset - .025f;
            if (terminal.position.y > upperReach)
            {
                if (!autoFlyWhenWalkingCannotReach)
                { Fail("The socket is above standing reach and automatic flight is disabled."); yield break; }
                narration?.Announce(SparkAAANarrationDirector.Milestone.Flight);
                yield return WalkNear(terminal, workbench, layout, stillValid);
                yield break;
            }

            if (flight && flight.IsFlying)
            {
                if (!workbench || !workbench.TryEnsureSurface(character, layout, out var landingFilter, out _))
                { Fail("Spark cannot find a safe tabletop landing for the low socket."); yield break; }
                Narrate("I'm landing before approaching " + terminal.name + " on foot.");
                Vector3 hovering = transform.position;
                Vector3 previous = new Vector3(float.PositiveInfinity, 0f, 0f);
                for (int candidate = 0; candidate < 18 && stillValid() && flight.IsFlying; candidate++)
                {
                    float angle = (candidate - 2) * Mathf.PI / 4f;
                    float distance = candidate < 10 ? .85f : 1.6f;
                    Vector3 center = candidate == 0 ? flight.LastGroundPosition : candidate == 1 ? hovering
                        : hovering + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
                    if (!workbench.TryGetLanding(center, landingFilter, out var landing) ||
                        (landing - previous).sqrMagnitude < .0036f) continue;
                    previous = landing;
                    yield return flight.LandForWalking(landing, terminal.position, stillValid);
                }
                if (!flight.LastOperationSucceeded || !HasNavMesh)
                { Fail("Spark could not land on a walkable surface for " + terminal.name + ". " + flight.LastError); yield break; }
                ConfigureWalkingAgent();
            }

            if (!HasNavMesh)
            { Fail("There is no usable ground NavMesh for " + terminal.name + "."); yield break; }
            Vector3 start = navigation.nextPosition;
            Vector3 target = terminal.position;
            Vector3 horizontal = Vector3.ProjectOnPlane(target - start, Vector3.up);
            bool CanUseOriginalHand() => allowOriginalHand && telescopicBridge &&
                telescopicBridge.CanReachWithOriginalHand(terminal);
            bool ready = CanUseOriginalHand() ||
                (pointer ? horizontal.sqrMagnitude <= .85f * .85f : CanPhysicallyReach(target, start));
            if (!ready)
            {
                var filter = new NavMeshQueryFilter { agentTypeID = navigation.agentTypeID, areaMask = navigation.areaMask };
                bool route = pointer
                    ? TryFindPointerStance(target, start, filter, out var destination)
                    : SelectWalkingDestination(target, start, filter, out destination, out _);
                if (!route)
                { Fail("There is no safe walking approach to " + terminal.name + "."); yield break; }
                Narrate("I'm walking to " + terminal.name + ".");
                if (!allowOriginalHand) yield return FollowGroundRoute(destination, stillValid);
                else
                {
                    string obstruction = null;
                    yield return FollowGroundRoute(destination, stillValid, reason => obstruction = reason);
                    if (failure != null || !stillValid()) yield break;
                    if (obstruction != null && !CanUseOriginalHand())
                    {
                        Vector3 blockedStance = destination;
                        if (!TryFindHandStance(terminal, out destination,
                            stance => (stance - blockedStance).sqrMagnitude > .09f))
                        { Fail("Walking pickup was blocked by " + obstruction + ". No clear original-hand stance remains."); yield break; }
                        yield return FollowGroundRoute(destination, stillValid);
                    }
                }
            }
            if (failure == null && stillValid()) FaceWorkingTerminal(terminal);
        }

        private bool TryFindPointerStance(Vector3 target, Vector3 start, NavMeshQueryFilter filter,
            out Vector3 destination, Func<Vector3, bool> acceptStance = null, float lateralPadding = 0f)
        {
            destination = default;
            target.y = start.y;
            float best = float.PositiveInfinity;
            var path = new NavMeshPath();
            var corners = new Vector3[64];
            foreach (float radius in ApproachStandoffs)
                for (int i = 0; i < 16; i++)
                {
                    float angle = i * Mathf.PI / 8f;
                    Vector3 point = target + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                    if (!NavMesh.SamplePosition(point, out var sample, .18f, filter) ||
                        Mathf.Abs(sample.position.y - start.y) > .12f ||
                        !NavMesh.CalculatePath(start, sample.position, filter, path) ||
                        path.status != NavMeshPathStatus.PathComplete) continue;
                    int count = path.GetCornersNonAlloc(corners);
                    if (count == corners.Length) continue;
                    float travel = 0f;
                    for (int c = 1; c < count; c++) travel += Vector3.Distance(corners[c - 1], corners[c]);
                    float score = Vector3.Distance(sample.position, target) + travel * .35f;
                    if (score >= best || travel > walkingSpeed * (walkTimeoutSeconds - 2f) ||
                        (acceptStance != null && !acceptStance(sample.position)) ||
                        !GroundRouteIsClear(path, lateralPadding)) continue;
                    best = score;
                    destination = sample.position;
                }
            return !float.IsPositiveInfinity(best);
        }

        private IEnumerator WalkNear(Transform terminal, SparkWorkbenchNavigation workbench,
            Transform layout, Func<bool> stillValid)
        {
            if (!stillValid() || !terminal) { Fail("Target or level changed before walking."); yield break; }
            Vector3 point = terminal.position;
            Vector3 destination = default;
            string routeError = null;
            bool canWalk = false;
            if (flight && flight.IsFlying)
            {
                bool standingReach = TryPlanGroundApproach(workbench, layout, point, out var landing, out destination);
                bool closeByAir = Vector3.Distance(shoulderR.position, point) <= ArmLength() - handTipOffset - .025f;
                bool groundTransfer = !standingReach && !closeByAir &&
                    TryPlanGroundTransfer(workbench, layout, point, out landing, out destination);
                if (standingReach || groundTransfer)
                {
                    Narrate("There is a safe walking route to " + terminal.name + ". I'm landing before using it.");
                    yield return flight.LandForWalking(landing, point, stillValid);
                    if (!stillValid()) { Fail("The wiring context changed while landing."); yield break; }
                    canWalk = flight.LastOperationSucceeded && HasNavMesh;
                    if (canWalk) ConfigureWalkingAgent();
                    if (canWalk && groundTransfer)
                    {
                        yield return FollowGroundRoute(destination, stillValid);
                        if (failure == null)
                            yield return FlyNear(terminal, stillValid, "I'm at the nearest safe walking position. Taking flight to reach the socket.");
                        yield break;
                    }
                }
                if (!canWalk)
                {
                    if (Vector3.Distance(shoulderR.position, point) <= ArmLength() - handTipOffset - .025f)
                        yield break;
                    yield return FlyNear(terminal, stillValid, "There is no safe walking route within reach. Continuing by flight.");
                    yield break;
                }
            }
            if (CanPhysicallyReach(point, transform.position))
            {
                FaceWorkingTerminal(terminal);
                Narrate("I'm already within reach of " + terminal.name + ". Moving my hand into position.");
                yield break;
            }
            // Ground walking remains the first choice while Spark is grounded.
            if (!canWalk && HasNavMesh)
            {
                var filter = new NavMeshQueryFilter { agentTypeID = navigation.agentTypeID, areaMask = navigation.areaMask };
                canWalk = SelectWalkingDestination(point, navigation.nextPosition, filter, out destination, out routeError);
            }
            if (!canWalk)
            {
                if (autoFlyWhenWalkingCannotReach && flight && flight.AllowAutoWiringFlight)
                {
                    var filter = navigation
                        ? new NavMeshQueryFilter { agentTypeID = navigation.agentTypeID, areaMask = navigation.areaMask }
                        : default;
                    if (HasNavMesh && TryFindLaunchPosition(point, navigation.nextPosition, filter, out var launch))
                    {
                        Narrate("I'm walking as close as I safely can before the short flight to " + terminal.name + ".");
                        yield return FollowGroundRoute(launch, stillValid);
                        if (failure != null) yield break;
                    }
                    yield return FlyNear(terminal, stillValid, "The terminal is beyond my standing reach. Taking flight for the connection.");
                    yield break;
                }
                Fail("Cannot physically approach " + terminal.name + ": " +
                    (HasNavMesh ? routeError : "No usable ground NavMesh."));
                yield break;
            }
            Narrate("I'm walking to " + terminal.name + " so I can connect the real cable by hand.");
            yield return FollowGroundRoute(destination, stillValid);
            if (failure != null) yield break;
            FaceWorkingTerminal(terminal);
            if (!CanPhysicallyReach(terminal.position, transform.position))
            {
                if (autoFlyWhenWalkingCannotReach && flight && flight.AllowAutoWiringFlight)
                    yield return FlyNear(terminal, stillValid, "Taking flight to finish the physical approach.");
                else Fail("Spark arrived outside real arm reach.");
            }
        }

        private bool TryFindLaunchPosition(Vector3 terminal, Vector3 start,
            NavMeshQueryFilter filter, out Vector3 position)
        {
            position = default;
            Vector3 projected = new Vector3(terminal.x, start.y, terminal.z);
            float currentGap = Vector3.Distance(start, projected);
            if (currentGap < 1.5f) return false;

            var path = new NavMeshPath();
            float best = float.PositiveInfinity;
            foreach (float radius in LaunchStandoffs)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = i * Mathf.PI / 6f;
                    Vector3 requested = projected + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                    if (!NavMesh.SamplePosition(requested, out var hit, .22f, filter) ||
                        Mathf.Abs(hit.position.y - start.y) > .12f ||
                        Vector3.Distance(hit.position, projected) >= currentGap - .75f ||
                        !NavMesh.CalculatePath(start, hit.position, filter, path) || path.status != NavMeshPathStatus.PathComplete)
                        continue;
                    var corners = path.corners;
                    float travel = 0f;
                    for (int c = 1; c < corners.Length; c++)
                        travel += Vector3.Distance(corners[c - 1], corners[c]);
                    if (travel > walkingSpeed * (walkTimeoutSeconds - 2f)) continue;
                    float score = Vector3.Distance(hit.position, projected) + travel * .2f;
                    if (score >= best || !GroundRouteIsClear(path)) continue;
                    best = score;
                    position = hit.position;
                }
            }
            return !float.IsPositiveInfinity(best);
        }

        private IEnumerator FollowGroundRoute(Vector3 destination, Func<bool> stillValid,
            Action<string> onGroundObstruction = null)
        {
            if (!character.WalkTo(destination, .015f))
            {
                Fail("Spark could not follow the validated walking path.");
                yield break;
            }
            float started = Time.realtimeSinceStartup;
            float nextClearanceCheck = started;
            while (Time.realtimeSinceStartup - started < walkTimeoutSeconds)
            {
                if (!stillValid() || character.WarningActive)
                {
                    Fail("Scene, target or safety state changed while Spark was walking.");
                    character.StopNavigation();
                    yield break;
                }
                if (!string.IsNullOrEmpty(failure))
                {
                    character.StopNavigation();
                    yield break;
                }
                if (!navigation.isOnNavMesh) { Fail("Spark left the navigation mesh."); yield break; }
                if (!navigation.pathPending &&
                    Vector3.Distance(transform.position, destination) <= navigation.stoppingDistance + .025f)
                {
                    character.StopNavigation();
                    yield break;
                }
                if (surfaceNavigation && flight && !navigation.pathPending && Time.realtimeSinceStartup >= nextClearanceCheck)
                {
                    nextClearanceCheck = Time.realtimeSinceStartup + .08f;
                    Vector3 next = Vector3.MoveTowards(transform.position, navigation.steeringTarget, .24f);
                    if (!flight.IsNextGroundStepClear(next, out string obstruction))
                    {
                        character.StopNavigation();
                        if (onGroundObstruction != null) onGroundObstruction(obstruction);
                        else Fail("Walking route blocked by " + obstruction + ".");
                        yield break;
                    }
                }
                yield return null;
            }
            character.StopNavigation();
            Fail("The walking route timed out before Spark reached its work position.");
        }

        private bool GroundRouteIsClear(NavMeshPath path, float lateralPadding = 0f) =>
            !surfaceNavigation || !flight || flight.IsGroundRouteClear(path.corners, lateralPadding);

        private bool TryPlanGroundApproach(SparkWorkbenchNavigation workbench, Transform layout,
            Vector3 terminal, out Vector3 landing, out Vector3 destination)
        {
            landing = destination = default;
            if (!workbench || !workbench.TryEnsureSurface(character, layout, out var filter, out _))
                return false;
            if (!workbench.TryGetTabletop(out var table)) return false;
            float shoulderOffset = (Quaternion.Inverse(transform.rotation) * (shoulderR.position - transform.position)).y;
            if (Mathf.Abs(terminal.y - table.bounds.max.y - shoulderOffset) >
                ArmLength() - handTipOffset + .10f) return false;
            if (workbench.TryGetLanding(transform.position, filter, out landing) &&
                SelectWalkingDestination(terminal, landing, filter, out destination, out _)) return true;
            if (workbench.TryGetLanding(flight.LastGroundPosition, filter, out landing) &&
                SelectWalkingDestination(terminal, landing, filter, out destination, out _)) return true;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                Vector3 nearby = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.4f;
                if (workbench.TryGetLanding(nearby, filter, out landing) &&
                    SelectWalkingDestination(terminal, landing, filter, out destination, out _)) return true;
            }
            return false;
        }

        private bool TryPlanGroundTransfer(SparkWorkbenchNavigation workbench, Transform layout, Vector3 terminal,
            out Vector3 landing, out Vector3 launch)
        {
            landing = launch = default;
            if (!autoFlyWhenWalkingCannotReach || !flight.AllowAutoWiringFlight || !workbench ||
                !workbench.TryEnsureSurface(character, layout, out var filter, out _))
                return false;
            return workbench.TryGetLanding(transform.position, filter, out landing) &&
                TryFindLaunchPosition(terminal, landing, filter, out launch);
        }

        private IEnumerator FlyNear(Transform terminal, Func<bool> stillValid, string announcement)
        {
            if (!flight || !flight.AllowAutoWiringFlight)
            { Fail("Autonomous flight is unavailable or disabled."); yield break; }
            if (!string.IsNullOrEmpty(announcement)) Narrate(announcement);
            yield return flight.FlyToArmReach(terminal, shoulderR,
                ArmLength() - handTipOffset - 0.025f, stillValid);
            if (!flight.LastOperationSucceeded)
            { Fail("Flight approach failed: " + flight.LastError); yield break; }
            if (Vector3.Distance(shoulderR.position, terminal.position) > ArmLength() - handTipOffset - .025f)
                Fail("Flight ended, but the real wrist/arm cannot physically reach " + terminal.name + ".");
            else Narrate("I'm hovering within arm reach of " + terminal.name + ".");
        }

        private IEnumerator Reach(Transform terminal, Func<bool> stillValid, Vector3 offset = default)
        {
            if (!stillValid() || !terminal) { Fail("Safety or scene changed during reach."); yield break; }
            Vector3 position = terminal.position + offset;
            gripTarget = position;
            trackingHands = true;
            reaching = true;
            if (character)
            {
                character.SetState(SparkState.Teaching);
                character.PlayGesture(SparkGesture.Grasp, reachSeconds + 1f);
            }
            float started = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - started < reachSeconds)
            {
                if (!stillValid() || !terminal || Vector3.Distance(terminal.position + offset, position) > .015f)
                { Fail("The wiring context or terminal moved while positioning the hand."); yield break; }
                yield return null;
            }
            reaching = false;
            float handError = Vector3.Distance(handRigs[0].GripPoint, position);
            if (handError > .028f || Vector3.Distance(shoulderR.position, position) > ArmLength() - handTipOffset)
                Fail($"Hand could not reach {terminal.name}. Connector gap {handError:F3} world units. " +
                    $"Shoulder {shoulderR.position:F3}, elbow {elbowR.position:F3}, wrist {wristR.position:F3}, " +
                    $"grip {handRigs[0].GripPoint:F3}, target {position:F3}.");
        }

        private IEnumerator PerformNativeCable(SparkTerminal a, SparkTerminal b, SparkAINativeSignalWirePort port,
            Transform layout, Func<bool> stillValid, Func<bool> canHover)
        {
            float armReach = ArmLength() - handTipOffset - .025f;
            float lowerReach = StandingShoulderY(surfaceNavigation) - armReach + .035f;
            bool belowFixedReach = a.transform.position.y < lowerReach || b.transform.position.y < lowerReach;
            float span = Vector3.Distance(a.transform.position, b.transform.position);
            float rest = cableRestLength > 0f ? cableRestLength : span + handlingAllowance;
            if (span > rest)
            { Fail("The selected lead is too short for these sockets."); yield break; }
            Stage = CableStage.Prepare;
            character.FocusOn(a.transform);
            // For low sockets, a validated nearby stance can be useful for the
            // telescopic hand even when the fixed-length IK cannot reach them.
            yield return ApproachWireTerminal(a.transform, surfaceNavigation, layout, belowFixedReach, stillValid,
                allowOriginalHand: true);
            if (failure != null || !stillValid()) yield break;

            if (telescopicBridge && telescopicBridge.CanReachWithOriginalHand(a.transform) &&
                telescopicBridge.CanDrawTo(b.transform))
            {
                yield return PerformTelescopicNativeCable(a, b, port, layout, stillValid, canHover);
                yield break;
            }
            if (belowFixedReach && HasNavMesh && telescopicBridge && telescopicBridge.IsRightArmResting)
            {
                Vector3 localShoulder = Quaternion.Inverse(transform.rotation) * (shoulderR.position - transform.position);
                bool BothContactsReachable(Vector3 stance)
                {
                    Vector3 predictedShoulder = stance + GroundFacing(a.transform.position, stance) * localShoulder;
                    return telescopicBridge.CanReachFrom(predictedShoulder, a.transform) &&
                           telescopicBridge.CanReachFrom(predictedShoulder, b.transform);
                }
                var filter = new NavMeshQueryFilter { agentTypeID = navigation.agentTypeID, areaMask = navigation.areaMask };
                if (TryFindPointerStance(a.transform.position, navigation.nextPosition, filter, out var alternate, BothContactsReachable))
                {
                    Narrate("I'm moving to a position where my original hand can reach both sockets.");
                    yield return FollowGroundRoute(alternate, stillValid, _ => { });
                    if (failure != null || !stillValid()) yield break;
                    FaceWorkingTerminal(a.transform);
                    if (telescopicBridge.CanReachWithOriginalHand(a.transform) && telescopicBridge.CanDrawTo(b.transform))
                    {
                        yield return PerformTelescopicNativeCable(a, b, port, layout, stillValid, canHover);
                        yield break;
                    }
                }
            }
            if (HasNavMesh && telescopicBridge && telescopicBridge.IsRightArmResting)
            {
                if (!telescopicBridge.CanReachWithOriginalHand(a.transform) &&
                    TryFindHandStance(a.transform, out var sourceStance))
                {
                    yield return FollowGroundRoute(sourceStance, stillValid, _ => { });
                    if (failure != null || !stillValid()) yield break;
                    FaceWorkingTerminal(a.transform);
                }
                if (telescopicBridge.CanReachWithOriginalHand(a.transform) &&
                    TryFindHandStance(b.transform, out var destinationStance, carrying: true))
                {
                    yield return PerformTelescopicNativeCable(a, b, port, layout, stillValid, canHover, destinationStance);
                    yield break;
                }
                if (TryFindCarryStances(a.transform, b.transform, out var pickup, out var delivery))
                {
                    yield return FollowGroundRoute(pickup, stillValid, _ => { });
                    if (failure != null || !stillValid()) yield break;
                    FaceWorkingTerminal(a.transform);
                    if (telescopicBridge.CanReachWithOriginalHand(a.transform) &&
                        TryMeasureCarryRoute(a.transform.position, delivery, out _))
                    {
                        yield return PerformTelescopicNativeCable(a, b, port, layout, stillValid, canHover, delivery);
                        yield break;
                    }
                }
                if (telescopicBridge.CanReachWithOriginalHand(a.transform) &&
                    TryFindHandStance(b.transform, out var liveCheckedStance))
                {
                    yield return PerformTelescopicNativeCable(a, b, port, layout, stillValid, canHover, liveCheckedStance);
                    yield break;
                }
            }
            if (belowFixedReach && autoFlyWhenWalkingCannotReach && flight && flight.AllowAutoWiringFlight &&
                telescopicBridge && telescopicBridge.IsRightArmResting)
            {
                yield return FlyNear(a.transform, stillValid, "The ground approach cannot reach both sockets safely. Checking a hovering stance.");
                if (failure != null || !stillValid()) yield break;
                if (telescopicBridge.CanReachWithOriginalHand(a.transform) && telescopicBridge.CanDrawTo(b.transform))
                {
                    yield return PerformTelescopicNativeCable(a, b, port, layout, stillValid, canHover);
                    yield break;
                }
            }
            bool pointer = belowFixedReach && allowReviewedPointerFallback;
            if (belowFixedReach && !pointer)
            {
                string diagnostic = telescopicBridge ? telescopicBridge.ReachDiagnostic : "Original-hand bridge unavailable";
                Fail("Low sockets are outside safe original-hand reach. " + diagnostic + ".");
                yield break;
            }

            if (pointer)
            {
                character.PointAt(a.transform, false, 1f);
                narration?.Announce(SparkAAANarrationDirector.Milestone.Pointer);
                Narrate("I'll use the player Wire Tool from this walking position. A low socket is below my arm's working height.");
            }
            else
            {
                Narrate("I'm positioning my hand at " + a.name + " to pick up the connector.");
                character.PlayGesture(SparkGesture.Offer, .2f);
                yield return new WaitForSecondsRealtime(.18f);
                yield return Reach(a.transform, stillValid);
                if (failure != null || !stillValid()) yield break;
            }
            if (!port.Begin(a, b, pointer, out var error) || !port.AttachSource(a, out error))
            { Fail("The player Wire Tool refused the source: " + error); yield break; }
            Stage = CableStage.SourceAttached;
            physicalPort = port;
            attachedSource = a.transform;

            if (pointer)
            {
                pointerStart = a.transform.position;
                pointerEnd = b.transform.position;
                pointerStarted = Time.realtimeSinceStartup;
                usingPlayerPointer = true;
            }
            else
            {
                Stage = CableStage.Grip;
                int segments = Application.isMobilePlatform ? Mathf.Min(16, cableSegments) : cableSegments;
                int iterations = Application.isMobilePlatform ? Mathf.Min(4, cableIterations) : cableIterations;
                cable.Reset(a.transform.position, handRigs[0].GripPoint, segments, rest, iterations);
                usingCable = true;
                yield return new WaitForSecondsRealtime(gripPauseSeconds);
                if (failure != null || !stillValid()) yield break;
                Narrate("The source end is attached. I'm carrying the loose connector to " + b.name + ".");
                narration?.Announce(SparkAAANarrationDirector.Milestone.Carry, b.name);
            }

            Stage = CableStage.Carrying;
            character.FocusOn(b.transform);
            yield return ApproachWireTerminal(b.transform, surfaceNavigation, layout, pointer, stillValid);
            if (failure != null || !stillValid()) yield break;
            Stage = CableStage.Aligning;
            if (pointer)
            {
                character.PointAt(b.transform, false, 1f);
                while (Time.realtimeSinceStartup - pointerStarted < .85f)
                {
                    if (failure != null || !stillValid()) yield break;
                    yield return null;
                }
                usingPlayerPointer = false;
                if (!port.MovePlayerPointer(b.transform.position))
                { Fail("The player Wire Tool could not project the last drag point."); yield break; }
            }
            else
            {
                Narrate("I'm aligning the connector with " + b.name + ".");
                narration?.Announce(SparkAAANarrationDirector.Milestone.Align, b.name);
                Vector3 offset = b.transform.up * insertionDistance;
                // A steep socket may only be reachable from its closest approach.
                if (Vector3.Distance(shoulderR.position, b.transform.position + offset) > armReach)
                    offset = (shoulderR.position - b.transform.position).normalized * insertionDistance;
                yield return Reach(b.transform, stillValid, offset);
                if (failure != null || !stillValid()) yield break;
                Stage = CableStage.Inserting;
                reaching = true;
                float started = Time.realtimeSinceStartup;
                do
                {
                    if (failure != null || !stillValid()) yield break;
                    insertionProgress = Mathf.Clamp01((Time.realtimeSinceStartup - started) / insertionSeconds);
                    gripTarget = b.transform.position + offset * (1f - Mathf.SmoothStep(0f, 1f, insertionProgress));
                    yield return null;
                } while (insertionProgress < 1f);
                if (Vector3.Distance(handRigs[0].GripPoint, b.transform.position) > .03f)
                { Fail("The connector did not reach the socket during insertion."); yield break; }
                Stage = CableStage.Settling;
                float settleUntil = Time.realtimeSinceStartup + .4f;
                while (!cable.Settled(settlingTolerance) && Time.realtimeSinceStartup < settleUntil)
                {
                    if (failure != null || !stillValid()) yield break;
                    yield return null;
                }
                // Preserve the existing settled interior and hand-to-terminal continuity.
                cable.Points[cable.Points.Length - 1] = b.transform.position;
                if (!port.ShowCableShape(cable, maximumCableExtension))
                { Fail(port.LastError); yield break; }
                usingCable = false;
            }

            if (failure != null || !stillValid() || !SocketIsClear(b.transform, layout)) yield break;
            if (!port.AttachDestination(b, out error))
            { Fail("The player Wire Tool refused the destination: " + error); yield break; }
            Stage = CableStage.Attached;
            if (!pointer) character.PlayGesture(SparkGesture.Offer, releasePauseSeconds);
            Narrate("The native Wire Tool confirmed the connection at " + b.name + ".");
            yield return new WaitForSecondsRealtime(releasePauseSeconds);
            trackingHands = false;
            if (flight && flight.IsFlying) yield return flight.RiseToSafeHover(canHover);
            Stage = CableStage.Complete;
            succeeded = true;
        }

        private bool TryFindHandStance(Transform socket, out Vector3 destination, Func<Vector3, bool> accept = null,
            bool carrying = false, Vector3? routeStart = null)
        {
            destination = default;
            if (!HasNavMesh || !telescopicBridge || !socket) return false;
            var filter = new NavMeshQueryFilter { agentTypeID = navigation.agentTypeID, areaMask = navigation.areaMask };
            Vector3 localShoulder = Quaternion.Inverse(transform.rotation) * (shoulderR.position - transform.position);
            return TryFindPointerStance(socket.position, routeStart ?? navigation.nextPosition, filter, out destination,
                stance => (accept == null || accept(stance)) &&
                    telescopicBridge.CanReachFrom(stance + GroundFacing(socket.position, stance) * localShoulder, socket),
                carrying ? WalkingCarrySideClearance : 0f);
        }

        private bool TryFindCarryStances(Transform source, Transform target, out Vector3 pickup, out Vector3 delivery)
        {
            pickup = delivery = default;
            var excluded = new Vector3[4];
            excluded[0] = navigation.nextPosition;
            for (int attempt = 1; attempt < excluded.Length; attempt++)
            {
                bool NewPickup(Vector3 candidate)
                {
                    for (int i = 0; i < attempt; i++)
                        if ((candidate - excluded[i]).sqrMagnitude < .25f) return false;
                    return true;
                }
                if (!TryFindHandStance(source, out var candidate, NewPickup)) return false;
                if (TryFindHandStance(target, out delivery, carrying: true, routeStart: candidate))
                {
                    pickup = candidate;
                    return true;
                }
                excluded[attempt] = candidate;
            }
            return false;
        }

        /// <summary>
        /// Optional straight-through native-wire drawing with the existing
        /// telescopic hand. The native signal port remains the only electrical
        /// connection authority. No pointer emulation or duplicated hands.
        /// </summary>
        private IEnumerator PerformTelescopicNativeCable(
            SparkTerminal a, SparkTerminal b, SparkAINativeSignalWirePort port,
            Transform layout, Func<bool> stillValid, Func<bool> canRelease, Vector3? carryStance = null)
        {
            if (!telescopicBridge || !telescopicBridge.IsConfigured)
            { Fail("Original-hand telescopic bridge is not bound"); yield break; }
            var contact = b.GetComponent<SparkNativeConnectorContact>();
            SparkNativeConnectorProfile profile = null;
            Vector3 insertionAxis = default;
            if (contact && !contact.TryResolve(out profile, out insertionAxis))
            { Fail("The destination contact frame is inactive or misaligned with the native terminal."); yield break; }
            bool ContactFrameUnchanged() => !profile ||
                contact && contact.TryResolve(out var current, out var axis) && current == profile &&
                Vector3.Angle(insertionAxis, axis) <= profile.AxisToleranceDegrees * .25f;
            bool AlignmentAuthorized() => stillValid() && ContactFrameUnchanged();
            float span = Vector3.Distance(a.transform.position, b.transform.position);
            float rest = cableRestLength > 0f ? cableRestLength : span + handlingAllowance;
            if (span > rest)
            { Fail("Native cable rest length is shorter than terminal span"); yield break; }
            if (carryStance.HasValue)
            {
                if (!TryMeasureCarryRoute(a.transform.position, carryStance.Value, out float carrySpan))
                { Fail("The planned cable carry no longer has a clear walking route."); yield break; }
                if (cableRestLength > 0f && carrySpan > cableRestLength)
                {
                    Fail($"The selected lead is {cableRestLength:F2} units; the walking carry needs at least {carrySpan:F2}.");
                    yield break;
                }
                if (cableRestLength <= 0f) rest = Mathf.Max(rest, carrySpan);
            }
            Stage = CableStage.PreGrip;
            character.FocusOn(a.transform);
            Narrate("Extending my original hand to the source connector.");
            yield return telescopicBridge.ReachTerminal(a.transform, stillValid, prepareGrip: true);
            if (!telescopicBridge.LastContactSucceeded || !stillValid())
            { Fail("Original hand missed source: " + telescopicBridge.LastError); yield break; }
            if (!telescopicBridge.VerifyGripNear(a.transform.position, out float sourceGap))
            { Fail($"Source pose changed before gripping {a.name}: final grip gap {sourceGap:F4}."); yield break; }
            if (!SocketIsClear(a.transform, layout)) yield break;
            if (!port.Begin(a, b, false, out string error) || !port.AttachSource(a, out error))
            { Fail("Native Wire Tool refused source: " + error); yield break; }
            physicalPort = port;
            attachedSource = a.transform;
            previewRootPosition = transform.position;
            Stage = CableStage.SourceAttached;
            if (!telescopicBridge.TryGetActiveGrip(out Vector3 sourceGrip))
            { Fail("Original fingertips unavailable after source attachment"); yield break; }
            int segments = Application.isMobilePlatform ? Mathf.Min(16, cableSegments) : cableSegments;
            int iterations = Application.isMobilePlatform ? Mathf.Min(4, cableIterations) : cableIterations;
            cable.Reset(a.transform.position, sourceGrip, segments, rest, iterations);
            usingCable = true;
            float cableDiameter = port.ActiveCableEndDiameter;
            Stage = CableStage.GripClosure;
            if (!telescopicBridge.SetGrip(1f, cableDiameter))
            { Fail("The original hand lost control during grip closure."); yield break; }
            float closeBy = Time.realtimeSinceStartup + telescopicBridge.GripTransitionSeconds + .7f;
            while (stillValid() && failure == null && telescopicBridge.IsContactActive &&
                !telescopicBridge.NativeGripSettled && Time.realtimeSinceStartup < closeBy)
                yield return null;
            if (failure != null || !stillValid()) yield break;
            if (!telescopicBridge.NativeGripSettled || !telescopicBridge.VerifyGripNear(a.transform.position, out _))
            { Fail("The original grip did not stabilize at the cable end."); yield break; }
            Stage = CableStage.GripVerified;
            yield return null;
            Stage = CableStage.Lift;
            Vector3 lift = Vector3.up * Mathf.Max(.025f, cableDiameter);
            if (!carryStance.HasValue && telescopicBridge.CanDrawTo(a.transform, lift))
            {
                yield return telescopicBridge.DrawHeldWireTo(a.transform, () => stillValid() && failure == null,
                    lift, lift.magnitude / telescopicBridge.GripTransitionSeconds);
                if (!telescopicBridge.LastContactSucceeded || failure != null || !stillValid())
                { if (failure == null) Fail("The original hand could not lift the cable end: " + telescopicBridge.LastError); yield break; }
            }
            if (carryStance.HasValue)
            {
                trackingCarryPose = true;
                float carryDeadline = Time.realtimeSinceStartup + telescopicBridge.ContactWaitSeconds;
                while (!telescopicBridge.VerifyGripNear(CarryGripPoint(), out _))
                {
                    if (!stillValid() || failure != null) yield break;
                    if (Time.realtimeSinceStartup >= carryDeadline)
                    { Fail("The original hand could not settle in a safe carry pose."); yield break; }
                    yield return null;
                }
                Stage = CableStage.Carrying;
                Narrate("Drawing the real native wire with my extendable original hand.");
                Vector3 workingStance = carryStance.Value;
                for (int attempt = 0; ; attempt++)
                {
                    string obstruction = null;
                    yield return FollowGroundRoute(workingStance, stillValid, reason => obstruction = reason);
                    if (failure != null || !stillValid()) yield break;
                    if (obstruction == null) break;
                    Vector3 rejectedStance = workingStance;
                    if (attempt >= 2 || !TryFindHandStance(b.transform, out workingStance, candidate =>
                        (candidate - rejectedStance).sqrMagnitude >= .09f &&
                        TryMeasureCarryRoute(a.transform.position, candidate, out float required) && required <= rest, carrying: true))
                    {
                        Fail("Walking carry blocked by " + obstruction + ". No alternate safe route fits the lead.");
                        yield break;
                    }
                }
                // LateUpdate still has to validate the last authorized movement frame.
                yield return null;
                trackingCarryPose = false;
                if (failure != null || !stillValid()) yield break;
                FaceWorkingTerminal(b.transform);
            }
            else
            {
                Stage = CableStage.Carrying;
                yield return null;
            }
            character.FocusOn(b.transform);
            if (profile)
            {
                Stage = CableStage.PreInsert;
                telescopicBridge.SetGrip(1f, cableDiameter, insertionAxis);
                yield return telescopicBridge.DrawHeldWireTo(b.transform, AlignmentAuthorized,
                    -insertionAxis * profile.InsertionStroke, 0f);
                if (!telescopicBridge.LastContactSucceeded || failure != null || !AlignmentAuthorized())
                { Fail("Could not reach the measured pre-insertion point: " + telescopicBridge.LastError); yield break; }
                Stage = CableStage.Aligning;
                float alignmentBy = Time.realtimeSinceStartup + telescopicBridge.ContactWaitSeconds;
                while (AlignmentAuthorized() && telescopicBridge.IsContactActive &&
                    telescopicBridge.GripAimError > profile.AxisToleranceDegrees && Time.realtimeSinceStartup < alignmentBy)
                    yield return null;
                if (!AlignmentAuthorized() || !telescopicBridge.IsContactActive ||
                    telescopicBridge.GripAimError > profile.AxisToleranceDegrees)
                { Fail("The original wrist could not align to the measured socket axis."); yield break; }
                Stage = CableStage.Inserting;
                yield return telescopicBridge.DrawHeldWireTo(b.transform, AlignmentAuthorized, Vector3.zero,
                    profile.InsertionStroke / insertionSeconds);
            }
            else
            {
                Stage = CableStage.PreInsert;
                yield return telescopicBridge.DrawHeldWireTo(b.transform, () => stillValid() && failure == null);
            }
            if (!telescopicBridge.LastContactSucceeded || failure != null || !stillValid())
            { if (failure == null) Fail("Original hand could not draw wire to destination: " + telescopicBridge.LastError); yield break; }
            if (!profile) Stage = CableStage.Aligning;
            if (!telescopicBridge.VerifyGripNear(b.transform.position, out float gap))
            { Fail("Original fingertip placement mismatch: " + gap.ToString("F4") + "m"); yield break; }
            // Allow the current native cable mesh simulation to sample the actual
            // hand at the destination; do not fabricate a circuit connection.
            yield return null;
            if (failure != null || !stillValid() || !SocketIsClear(b.transform, layout)) yield break;
            Stage = CableStage.Inserting;
            yield return null;
            if (failure != null || !stillValid() || !SocketIsClear(b.transform, layout)) yield break;
            if (!telescopicBridge.VerifyGripNear(b.transform.position, out gap))
            { Fail("Native connector moved away from the original hand"); yield break; }
            if (profile && (!AlignmentAuthorized() || gap > profile.ContactTolerance ||
                telescopicBridge.GripAimError > profile.AxisToleranceDegrees))
            { Fail("The original hand is outside the socket's measured position or angular tolerance."); yield break; }
            if (!attachedSource || !cable.Advance(attachedSource.position, b.transform.position, 0f,
                cableGravity, cableDamping, cableBendStiffness, cableCollisionRadius,
                maximumCableExtension, cableCollisionLayers))
            { Fail("The seated native lead would exceed its physical length."); yield break; }
            if (!port.ShowCableShape(cable, maximumCableExtension))
            { Fail(port.LastError); yield break; }
            if (!port.AttachDestination(b, out error))
            { Fail("Native Wire Tool refused destination: " + error); yield break; }
            usingCable = false;
            Stage = CableStage.Attached;
            yield return null;
            Stage = CableStage.Release;
            telescopicBridge.SetGrip(0f, cableDiameter);
            yield return new WaitForSecondsRealtime(Mathf.Max(releasePauseSeconds, telescopicBridge.GripTransitionSeconds));
            if (profile && b && canRelease() && ContactFrameUnchanged())
                yield return telescopicBridge.DrawHeldWireTo(b.transform, () => canRelease() && ContactFrameUnchanged(),
                    -insertionAxis * profile.ReleaseDistance, profile.ReleaseDistance / insertionSeconds);
            Stage = CableStage.Retracting;
            telescopicBridge.ReleaseArm();
            float retractDeadline = Time.realtimeSinceStartup + telescopicBridge.ContactWaitSeconds;
            while (!telescopicBridge.IsRightArmResting && Time.realtimeSinceStartup < retractDeadline)
                yield return null;
            if (!telescopicBridge.IsRightArmResting)
            { Fail("Native cable attached, but the original hand did not finish retracting."); yield break; }
            Stage = CableStage.Complete;
            succeeded = true;
        }

        internal IEnumerator OperateDevice(SwitchLeverController lever, SparkWorkbenchNavigation workbench,
            Transform layout, Func<bool> stillValid, Action dispatch)
        {
            if (stageActive) { Fail("Spark is already performing a physical action."); yield break; }
            failure = null;
            succeeded = false;
            if (!lever || !lever.TryGetPressPoint(out var localPoint) || !stillValid())
            { Fail("The switch has no usable physical press surface."); yield break; }
            var control = lever.LeverTransform;
            var arm = GetComponent<SparkTelescopicArms>();
            surfaceNavigation = workbench;
            if (!arm || !BindRig() || !PrepareNavigation(workbench, layout))
            {
                if (!arm) Fail("The original index finger rig is unavailable.");
                CancelPerformance();
                yield break;
            }
            stageActive = true;
            try
            {
                bool Valid() => stillValid() && control && control.gameObject.activeInHierarchy && !character.WarningActive;
                Vector3 hover = Vector3.up * .08f;
                Vector3 target = control.TransformPoint(localPoint);
                if (flight && flight.IsFlying)
                {
                    if (!workbench || !workbench.TryEnsureSurface(character, layout, out var landingFilter, out _))
                    { Fail("Spark cannot find a workbench landing before pressing the switch."); yield break; }
                    Vector3 airborne = transform.position;
                    for (int i = -2; i < 8 && flight.IsFlying && Valid(); i++)
                    {
                        float angle = i * Mathf.PI / 4f;
                        Vector3 candidate = i == -2 ? flight.LastGroundPosition : i == -1 ? airborne :
                            airborne + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.4f;
                        if (workbench.TryGetLanding(candidate, landingFilter, out var landing))
                            yield return flight.LandForWalking(landing, target, Valid);
                    }
                    if (!Valid()) yield break;
                    if (flight.IsFlying || !HasNavMesh)
                    { Fail("Spark could not land before pressing the switch. " + flight.LastError); yield break; }
                    ConfigureWalkingAgent();
                }
                if (!HasNavMesh)
                { Fail("A grounded walking surface is required for the switch press."); yield break; }

                Vector3 shoulderOffset = Quaternion.Inverse(transform.rotation) * (shoulderR.position - transform.position);
                bool CanPressAt(Vector3 feet)
                {
                    if (Vector3.ProjectOnPlane(target - feet, Vector3.up).sqrMagnitude > 1.25f * 1.25f)
                        return false;
                    Vector3 shoulder = feet + GroundFacing(target, feet) * shoulderOffset;
                    return arm.CanPressFrom(shoulder, control, localPoint, hover) &&
                        arm.CanPressFrom(shoulder, control, localPoint, Vector3.zero);
                }
                if (!CanPressAt(transform.position))
                {
                    var filter = new NavMeshQueryFilter { agentTypeID = navigation.agentTypeID, areaMask = navigation.areaMask };
                    if (!TryFindPointerStance(target, navigation.nextPosition, filter, out var stance, CanPressAt))
                    { Fail("No clear standing position beside the switch. " + arm.LastError); yield break; }
                    yield return FollowGroundRoute(stance, Valid);
                    if (failure != null || !Valid()) yield break;
                }
                character.StopNavigation();
                // Let the agent finish applying its last ground step before
                // recording the planted position for the press.
                yield return null;
                yield return null;
                if (!Valid() || !HasNavMesh)
                { Fail("Switch authorization or ground contact was lost before the press."); yield break; }
                Vector3 standingPosition = transform.position;
                Quaternion startRotation = transform.rotation;
                Quaternion pressRotation = GroundFacing(target, standingPosition);
                bool Standing() => Valid() && HasNavMesh && !navigation.hasPath && (!flight || !flight.IsFlying) &&
                    (transform.position - standingPosition).sqrMagnitude < .0004f;
                float turnStart = Time.unscaledTime;
                while (Time.unscaledTime - turnStart < .25f && Standing())
                {
                    transform.rotation = Quaternion.Slerp(startRotation, pressRotation,
                        Mathf.SmoothStep(0f, 1f, (Time.unscaledTime - turnStart) / .25f));
                    yield return null;
                }
                if (!Standing())
                {
                    Fail($"Spark could not hold a standing pose (authorized {Valid()}, on navigation surface {HasNavMesh}, " +
                        $"airborne {flight && flight.IsFlying}, foot displacement {Vector3.Distance(transform.position, standingPosition):F3} m).");
                    yield break;
                }
                transform.rotation = pressRotation;
                character.FocusOn(control);
                character.PlayGesture(SparkGesture.Press, 10f);

                bool Reached(Vector3 offset) => arm.RightContactConfirmed &&
                    arm.TryGetGripPosition(SparkTelescopicArms.ManipulatorSide.Right, out var tip) &&
                    Vector3.Distance(tip, control.TransformPoint(localPoint) + offset) <= Mathf.Min(.005f, arm.ContactTolerance);
                if (!arm.MoveIndexToControl(control, localPoint, hover, 1.6f))
                { Fail("Spark cannot reach the switch with its index finger. " + arm.LastError); yield break; }
                pressingArm = arm;
                IEnumerator Approach(Vector3 offset)
                {
                    float deadline = Time.unscaledTime + arm.ReachTimeoutSeconds;
                    while (Standing() && arm.IsActive && !Reached(offset) && Time.unscaledTime < deadline)
                        yield return null;
                    if (!Standing() || !Reached(offset))
                        Fail("Standing switch contact was interrupted. " + arm.LastError);
                }
                yield return Approach(hover);
                if (failure != null || !Standing()) yield break;
                const float pressStrokeSpeed = .48f;
                if (!arm.MoveIndexToControl(control, localPoint, Vector3.zero, pressStrokeSpeed))
                { Fail("The switch face is obstructed. " + arm.LastError); yield break; }
                yield return Approach(Vector3.zero);
                if (failure != null || !Standing()) yield break;
                if (!arm.MoveIndexToControl(control, localPoint, Vector3.zero, 3.25f))
                { Fail("Spark lost access to the switch before pressing it."); yield break; }
                dispatch();
                float releaseAt = Time.unscaledTime + gripPauseSeconds;
                float settleBy = releaseAt + .65f;
                while (Standing() && arm.IsActive && Time.unscaledTime < settleBy &&
                    (Time.unscaledTime < releaseAt || lever.IsLeverMoving || !Reached(Vector3.zero)))
                    yield return null;
                if (!Standing() || !arm.MoveIndexToControl(control, localPoint, hover, pressStrokeSpeed)) yield break;
                yield return Approach(hover);
                arm.RetractRight();
                float restBy = Time.unscaledTime + arm.ReachTimeoutSeconds;
                while (Standing() && !arm.IsRightArmResting && Time.unscaledTime < restBy)
                    yield return null;
                if (Standing() && arm.IsRightArmResting)
                    pressingArm = null;
            }
            finally { CancelPerformance(); }
        }

        public IEnumerator Perform(SparkTerminal a, SparkTerminal b,
            SparkAINativeWirePort port, SparkWorkbenchNavigation workbench, Func<bool> stillValid, Func<bool> canHover)
        {
            failure = null;
            succeeded = false;
            if (!telescopicBridge) telescopicBridge = GetComponent<SparkTelescopicNativeWireBridge>();
            if (stageActive) { Fail("A physical performance is already in progress."); yield break; }
            if (!a || !b || !port || stillValid == null || !stillValid())
            { Fail("Invalid native-wire task or changed game context."); yield break; }
            Transform layout = executive.ActiveLevelManager.LayoutLoader.ActiveLayout.transform;
            surfaceNavigation = workbench;
            if (!BindRig()) { CancelPerformance(); yield break; }
            // Prefer the actual native cable with a single original telescopic
            // hand when BOTH endpoints are safe from the current stance.
            bool directTelescopicWire = port is SparkAINativeSignalWirePort &&
                telescopicBridge && telescopicBridge.IsConfigured &&
                telescopicBridge.CanReachWithOriginalHand(a.transform) &&
                telescopicBridge.CanDrawTo(b.transform);
            if (!directTelescopicWire && !PrepareNavigation(workbench, layout))
            { CancelPerformance(); yield break; }

            stageActive = true;
            try
            {
                if (directTelescopicWire && port is SparkAINativeSignalWirePort extendedPort)
                {
                    narration?.BeginWire();
                    yield return PerformTelescopicNativeCable(a, b, extendedPort, layout, stillValid, canHover);
                    yield break;
                }
                if (port is SparkAINativeSignalWirePort nativePort)
                {
                    narration?.BeginWire();
                    yield return PerformNativeCable(a, b, nativePort, layout, stillValid, canHover);
                    yield break;
                }
                bool extendSource = telescopicBridge && telescopicBridge.CanExtendTo(a.transform);
                if (!extendSource)
                {
                    yield return WalkNear(a.transform, workbench, layout, stillValid);
                    if (failure != null) yield break;
                }

                Narrate("First I check the source terminal " + a.name + ", then grip the cable connector.");
                if (extendSource)
                {
                    trackingHands = false;
                    yield return telescopicBridge.ReachTerminal(a.transform, stillValid);
                    if (!telescopicBridge.LastContactSucceeded)
                    { Fail("Original hand could not reach source: " + telescopicBridge.LastError); yield break; }
                }
                else
                {
                    yield return Reach(a.transform, stillValid);
                    if (failure != null || !stillValid()) yield break;
                }

                if (!port.Begin(a, b, out string error))
                { Fail("The player wire tool refused cable pickup: " + error); yield break; }
                if (!port.AttachSource(a, out error))
                { Fail("The player wire tool refused the first plug: " + error); yield break; }
                Narrate("The first cable end is attached to " + a.name + ". I'll carry its loose end to " + b.name + ".");
                usingCable = true;
                livePort = port;
                appliedCableGrip = handRigs[0].GripPoint;
                if (extendSource && telescopicBridge &&
                    telescopicBridge.TryGetActiveGrip(out Vector3 extendedSourceGrip))
                    appliedCableGrip = extendedSourceGrip;
                if (!port.MoveLooseEnd(appliedCableGrip, out error))
                { Fail("The player wire tool refused the hand grip position: " + error); yield break; }
                if (telescopicBridge) telescopicBridge.ReleaseArm();
                yield return new WaitForSecondsRealtime(gripPauseSeconds);

                bool extendDestination = telescopicBridge && telescopicBridge.CanExtendTo(b.transform);
                if (!extendDestination)
                {
                    yield return WalkNear(b.transform, workbench, layout, stillValid);
                    if (failure != null) yield break;
                }
                Narrate("I'm aligning the loose cable end with " + b.name + ". Watch the connection point.");
                if (extendDestination)
                {
                    trackingHands = false;
                    yield return telescopicBridge.ReachTerminal(b.transform, stillValid);
                    if (!telescopicBridge.LastContactSucceeded)
                    { Fail("Original hand could not reach destination: " + telescopicBridge.LastError); yield break; }
                }
                else
                {
                    yield return Reach(b.transform, stillValid);
                    if (failure != null || !stillValid()) yield break;
                }
                Vector3 destinationGrip = handRigs[0].GripPoint;
                if (extendDestination && telescopicBridge &&
                    telescopicBridge.TryGetActiveGrip(out Vector3 extendedDestinationGrip))
                    destinationGrip = extendedDestinationGrip;
                if (!port.MoveLooseEnd(destinationGrip, out error))
                { Fail("The player wire tool refused final positioning: " + error); yield break; }
                usingCable = false;
                livePort = null;
                if (!port.AttachDestination(b, out error))
                { Fail("The player Wire Tool refused the second plug: " + error); yield break; }
                Narrate("Both physical ends are attached. Checking the electrical topology; this alone does not prove current is flowing.");
                yield return new WaitForSecondsRealtime(releasePauseSeconds);
                trackingHands = false;
                surfaceNavigation = null;
                if (flight && flight.IsFlying) yield return flight.RiseToSafeHover(canHover);
                succeeded = true; // SparkNativeWireDirector independently checks CircuitSystem.HasConnectionBetween.
            }
            finally
            {
                if (telescopicBridge) telescopicBridge.ReleaseArm();
                reaching = false;
                usingCable = false;
                trackingCarryPose = false;
                usingPlayerPointer = false;
                physicalPort = null;
                attachedSource = null;
                if (!succeeded) Stage = CableStage.Cancelled;
                livePort = null;
                trackingHands = false;
                if (character) { character.StopNavigation(); character.ClearGesture(); }
                RestoreAgent();
                stageActive = false;
            }
        }

        private Vector3 CarryGripPoint()
        {
            Vector3 shoulder = shoulderR.position;
            return shoulder + Vector3.ClampMagnitude(attachedSource.position - shoulder, CarryReach);
        }

        private bool TryMeasureCarryRoute(Vector3 source, Vector3 stance, out float requiredLength)
        {
            requiredLength = 0f;
            if (!HasNavMesh) return false;
            var filter = new NavMeshQueryFilter { agentTypeID = navigation.agentTypeID, areaMask = navigation.areaMask };
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(navigation.nextPosition, stance, filter, path) ||
                path.status != NavMeshPathStatus.PathComplete || !GroundRouteIsClear(path))
                return false;
            Vector3 shoulderOffset = shoulderR.position - transform.position;
            Vector3 vertical = Vector3.Project(shoulderOffset, Vector3.up);
            float turnMargin = (shoulderOffset - vertical).magnitude;
            foreach (Vector3 corner in path.corners)
                requiredLength = Mathf.Max(requiredLength, Vector3.Distance(source, corner + vertical) + turnMargin - CarryReach);
            // The hand settles toward its carry target while Spark starts and stops.
            requiredLength += .12f;
            return true;
        }

        private void Update()
        {
            if (trackingCarryPose && failure == null &&
                (!attachedSource || !telescopicBridge || !telescopicBridge.SetCarryTarget(CarryGripPoint())))
                Fail("The original hand lost its grip during the walking carry.");
        }

        private void LateUpdate()
        {
            if (usingPlayerPointer && physicalPort && failure == null)
            {
                float progress = Mathf.Clamp01((Time.realtimeSinceStartup - pointerStarted) / .85f);
                if (!physicalPort.MovePlayerPointer(Vector3.Lerp(pointerStart, pointerEnd, progress * .94f)))
                    Fail("The player Wire Tool could not project the external drag onto its camera.");
            }
            if (usingCable && physicalPort && telescopicBridge && telescopicBridge.IsContactActive)
            {
                float permittedStep = trackingCarryPose ? walkingSpeed * Time.unscaledDeltaTime + .08f : .08f;
                Vector3 displacement = transform.position - previewRootPosition;
                if ((!flight || !flight.IsMoving) &&
                    displacement.sqrMagnitude > permittedStep * permittedStep)
                {
                    Fail($"Spark's position changed outside the validated cable motion during {Stage} " +
                        $"({displacement.magnitude:F3} m moved, {permittedStep:F3} m permitted).");
                    return;
                }
                previewRootPosition = transform.position;
                // The original telescopic hand alone owns the wrist. Do not
                // run the fixed-length IK on top of the extended skeleton.
                if (!telescopicBridge.TryGetActiveGrip(out Vector3 originalGrip))
                { Fail("Original native-wire grip is unavailable"); return; }
                appliedCableGrip = originalGrip;
                if (!attachedSource || !cable.Advance(attachedSource.position, appliedCableGrip,
                    Time.unscaledDeltaTime, cableGravity, cableDamping, cableBendStiffness,
                    cableCollisionRadius, maximumCableExtension, cableCollisionLayers))
                    Fail($"Extended hand cannot draw cable without overstretching: " +
                        $"span {(attachedSource ? Vector3.Distance(attachedSource.position, appliedCableGrip) : 0f):F3}, " +
                        $"rest length {cable.Length:F3}.");
                else if (!physicalPort.ShowCableShape(cable, maximumCableExtension))
                    Fail(physicalPort.LastError);
                return;
            }
            if (!trackingHands || !HasRig) return;
            if (usingCable && !reaching && character)
                character.PlayGesture(SparkGesture.Grasp, .25f);
            ikWeight = Mathf.MoveTowards(ikWeight, 1f, Time.unscaledDeltaTime * ikBlendSpeed);
            bool groundMoving = navigation && navigation.enabled && navigation.isOnNavMesh &&
                navigation.hasPath && navigation.velocity.sqrMagnitude > 0.0005f;
            if (usingCable && !reaching && (groundMoving || (flight && flight.IsMoving)))
            {
                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                if (forward.sqrMagnitude < .04f) forward = Vector3.ProjectOnPlane(transform.up, Vector3.up);
                gripTarget = shoulderR.position + forward.normalized * Mathf.Max(.08f, ArmLength() * .55f)
                    + Vector3.up * .055f;
            }
            foreach (var rig in handRigs)
            {
                if (!rig.IsReady) continue;
                Vector3 direction = (gripTarget - rig.Shoulder.position).normalized;
                float gripOffset = Vector3.Distance(rig.Wrist.position, rig.GripPoint);
                SolveArm(rig.Shoulder, rig.Elbow, rig.Wrist, gripTarget - direction * gripOffset, ikWeight);
                Vector3 wristToGrip = rig.GripPoint - rig.Wrist.position;
                if (wristToGrip.sqrMagnitude > .00001f && direction.sqrMagnitude > .01f)
                {
                    Quaternion rotation = Quaternion.FromToRotation(wristToGrip, direction) * rig.Wrist.rotation;
                    rig.Wrist.rotation = Quaternion.Slerp(rig.Wrist.rotation, rotation, ikWeight);
                }
                if (useLeftHandForSupport)
                {
                    Vector3 leftTarget = gripTarget - transform.right * .11f + transform.up * .035f;
                    SolveArm(rig.LeftShoulder, rig.LeftElbow, rig.LeftWrist, leftTarget, ikWeight * .70f);
                }
            }
            if (usingCable && physicalPort && attachedSource && failure == null)
            {
                appliedCableGrip = handRigs[0].GripPoint;
                if (!cable.Advance(attachedSource.position, appliedCableGrip, Time.unscaledDeltaTime,
                    cableGravity, cableDamping, cableBendStiffness, cableCollisionRadius,
                    maximumCableExtension, cableCollisionLayers))
                    Fail("The selected lead cannot reach this carry position without overstretching.");
                else if (!physicalPort.ShowCableShape(cable, maximumCableExtension))
                    Fail(physicalPort.LastError);
            }
            else if (usingCable && livePort)
            {
                appliedCableGrip = handRigs[0].GripPoint;
                if (telescopicBridge && telescopicBridge.TryGetActiveGrip(out Vector3 activeOriginalGrip))
                    appliedCableGrip = activeOriginalGrip;
                if (!livePort.MoveLooseEnd(appliedCableGrip, out string error))
                {
                    usingCable = false;
                    failure = "Real cable movement was rejected: " + error;
                }
            }
        }

        private static void SolveArm(Transform shoulder, Transform elbow, Transform wrist,
            Vector3 target, float weight)
        {
            if (!shoulder || !elbow || !wrist || weight <= 0) return;
            Vector3 root = shoulder.position;
            Vector3 toTarget = target - root;
            float d = toTarget.magnitude;
            float upper = Vector3.Distance(root, elbow.position);
            float lower = Vector3.Distance(elbow.position, wrist.position);
            if (d < 0.001f || upper < 0.001f || lower < 0.001f) return;
            Vector3 direction = toTarget / d;
            float reach = Mathf.Clamp(d, Mathf.Abs(upper - lower) + .002f, upper + lower - .002f);
            float along = (upper * upper - lower * lower + reach * reach) / (2 * reach);
            Vector3 pole = Vector3.ProjectOnPlane(elbow.position - root, direction).normalized;
            if (pole.sqrMagnitude < 0.01f) pole = Vector3.ProjectOnPlane(-shoulder.up, direction).normalized;
            if (pole.sqrMagnitude < 0.01f) return;
            Vector3 elbowGoal = root + direction * along +
                pole * Mathf.Sqrt(Mathf.Max(0, upper * upper - along * along));
            Quaternion upperGoal = Quaternion.FromToRotation(elbow.position - root, elbowGoal - root) * shoulder.rotation;
            shoulder.rotation = Quaternion.Slerp(shoulder.rotation, upperGoal, weight);
            Vector3 wristGoal = root + direction * reach;
            Quaternion lowerGoal = Quaternion.FromToRotation(wrist.position - elbow.position, wristGoal - elbow.position) * elbow.rotation;
            elbow.rotation = Quaternion.Slerp(elbow.rotation, lowerGoal, weight);
        }
    }
}
