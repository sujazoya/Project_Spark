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
    [DefaultExecutionOrder(700)] // After SparkProCharacter's LateUpdate, so grip IK remains visible.
    public sealed class SparkNativeWireCinematic : MonoBehaviour
    {
        [Header("Authoritative existing services")]
        [SerializeField] private SparkProCharacter character;
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private NavMeshAgent navigation;
        [SerializeField] private SparkV5FlightController flight;
        [SerializeField] private bool autoFlyWhenWalkingCannotReach = true;

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

        private struct HandRig
        {
            public Transform Shoulder, Elbow, Wrist, Thumb, Index, Middle;
            public Transform LeftShoulder, LeftElbow, LeftWrist;
            public bool IsReady => Shoulder && Elbow && Wrist && Thumb && Index && Middle;
            public Vector3 GripPoint => Vector3.Lerp(Thumb.position, (Index.position + Middle.position) * .5f, .65f);
        }

        private HandRig[] handRigs;
        private SparkWorkbenchNavigation surfaceNavigation;
        private Transform shoulderR => handRigs[0].Shoulder;
        private Transform elbowR => handRigs[0].Elbow;
        private Transform wristR => handRigs[0].Wrist;
        private SparkAINativeWirePort livePort;
        private bool usingCable, trackingHands, stageActive, reaching;
        private Vector3 gripTarget;
        private Vector3 appliedCableGrip;
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

        private void Awake()
        {
            if (!character) character = GetComponent<SparkProCharacter>();
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!navigation) navigation = GetComponent<NavMeshAgent>();
            if (!flight) flight = GetComponent<SparkV5FlightController>();
        }

        private void OnDisable() { CancelPerformance(); }

        public void CancelPerformance()
        {
            if (flight && stageActive) flight.StopMotionAndHover();
            usingCable = false;
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
            if (announceEachWire && executive) executive.PublishWirePerformanceNarration(message);
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

            if (completePaths > 0) return true;
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

        private IEnumerator FollowGroundRoute(Vector3 destination, Func<bool> stillValid)
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
                if (usingCable && !string.IsNullOrEmpty(failure))
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
                    if (!flight.IsNextGroundStepClear(next))
                    {
                        character.StopNavigation();
                        Fail("A physical obstacle blocks the walking route.");
                        yield break;
                    }
                }
                yield return null;
            }
            character.StopNavigation();
            Fail("The walking route timed out before Spark reached its work position.");
        }

        private bool GroundRouteIsClear(NavMeshPath path) =>
            !surfaceNavigation || !flight || flight.IsGroundRouteClear(path.corners);

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

        private IEnumerator Reach(Transform terminal, Func<bool> stillValid)
        {
            if (!stillValid() || !terminal) { Fail("Safety or scene changed during reach."); yield break; }
            Vector3 position = terminal.position;
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
                if (!stillValid() || !terminal || Vector3.Distance(terminal.position, position) > .015f)
                { Fail("The wiring context or terminal moved while positioning the hand."); yield break; }
                yield return null;
            }
            reaching = false;
            float handError = Vector3.Distance(handRigs[0].GripPoint, position);
            if (handError > .028f || Vector3.Distance(shoulderR.position, position) > ArmLength() - handTipOffset)
                Fail($"Hand could not reach {terminal.name}. Connector gap {handError:F3} world units.");
        }

        public IEnumerator Perform(SparkTerminal a, SparkTerminal b,
            SparkAINativeWirePort port, SparkWorkbenchNavigation workbench, Func<bool> stillValid, Func<bool> canHover)
        {
            failure = null;
            succeeded = false;
            if (stageActive) { Fail("A physical performance is already in progress."); yield break; }
            if (!a || !b || !port || stillValid == null || !stillValid())
            { Fail("Invalid native-wire task or changed game context."); yield break; }
            Transform layout = executive.ActiveLevelManager.LayoutLoader.ActiveLayout.transform;
            surfaceNavigation = workbench;
            if (!BindRig() || !PrepareNavigation(workbench, layout))
            { CancelPerformance(); yield break; }

            stageActive = true;
            try
            {
                yield return WalkNear(a.transform, workbench, layout, stillValid);
                if (failure != null) yield break;

                Narrate("First I check the source terminal " + a.name + ", then grip the cable connector.");
                yield return Reach(a.transform, stillValid);
                if (failure != null || !stillValid()) yield break;

                if (!port.Begin(a, b, out string error))
                { Fail("The player wire tool refused cable pickup: " + error); yield break; }
                if (!port.AttachSource(a, out error))
                { Fail("The player wire tool refused the first plug: " + error); yield break; }
                Narrate("The first cable end is attached to " + a.name + ". I'll carry its loose end to " + b.name + ".");
                usingCable = true;
                livePort = port;
                appliedCableGrip = handRigs[0].GripPoint;
                if (!port.MoveLooseEnd(appliedCableGrip, out error))
                { Fail("The player wire tool refused the hand grip position: " + error); yield break; }
                yield return new WaitForSecondsRealtime(gripPauseSeconds);

                yield return WalkNear(b.transform, workbench, layout, stillValid);
                if (failure != null) yield break;
                Narrate("I'm aligning the loose cable end with " + b.name + ". Watch the connection point.");
                yield return Reach(b.transform, stillValid);
                if (failure != null || !stillValid()) yield break;
                if (!port.MoveLooseEnd(handRigs[0].GripPoint, out error))
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
                reaching = false;
                usingCable = false;
                livePort = null;
                trackingHands = false;
                if (character) { character.StopNavigation(); character.ClearGesture(); }
                RestoreAgent();
                stageActive = false;
            }
        }

        private void LateUpdate()
        {
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
            if (usingCable && livePort)
            {
                appliedCableGrip = handRigs[0].GripPoint;
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
