using System;
using System.Collections;
using System.Collections.Generic;
using ProjectSpark.Gameplay;
using ProjectSpark.AI.Tutorial;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Visual 3D locomotion ONLY. Never creates electrical connections or alters
    /// circuit/mission state. Cinematic performer owns wire grip and verification.
    /// Stops on an obstructed route; no teleport or reach spoofing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkV5FlightController : MonoBehaviour
    {
        [Header("Flight capability")]
        [SerializeField] private bool allowAutoWiringFlight = true;
        [SerializeField, Range(0.2f, 5f)] private float flightSpeed = 1.75f;
        [SerializeField, Range(0.1f, 1.5f)] private float takeoffRise = 0.40f;
        [SerializeField, Range(0.1f, 1.5f)] private float cruiseClearance = 0.55f;
        [SerializeField, Range(0.03f, 0.3f)] private float bodySweepRadius = 0.085f;
        [SerializeField, Range(0.05f, 1f)] private float bodySweepCenterHeight = 0.24f;
        [SerializeField, Range(5f, 60f)] private float motionTimeoutSeconds = 23f;
        [Header("Tabletop limits and manual fallback")]
        [SerializeField, Range(0f, 1f)] private float tabletopEdgeMargin = .35f;
        [SerializeField, Range(1f, 16f)] private float maximumXDistance = 8.8f;
        [SerializeField, Range(1f, 12f)] private float maximumZDistance = 5.6f;
        [SerializeField, Range(0.5f, 5f)] private float maximumHeightAboveHome = 2.5f;
        [SerializeField, Range(0f, 1.5f)] private float maximumBelowHome = 0.65f;
        [SerializeField] private LayerMask collisionLayers = ~0;
        [Header("Visual thruster exhaust (presentation only)")]
        [SerializeField] private bool createSubtleThrusterParticles = true;
        [SerializeField] private SparkProCharacter character;
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private NavMeshAgent groundAgent;

        private Vector3 home;
        private Quaternion homeRotation;
        private bool airborne, moving, agentWasEnabled, bodyWasKinematic, bodyUsedGravity;
        private Rigidbody body;
        private Coroutine manualAction;
        private ParticleSystem leftThruster, rightThruster;
        private string lastError;
        private bool lastSucceeded;
        private bool driveBlocked;
        private Transform trackedSelected;

        public bool IsFlying => airborne;
        public bool IsMoving => moving;
        public bool AllowAutoWiringFlight => allowAutoWiringFlight;
        public bool LastOperationSucceeded => lastSucceeded;
        public string LastError => lastError;
        internal Vector3 LastGroundPosition => home;
        public float WireFlightTimeBudgetSeconds => motionTimeoutSeconds * 2f + 8f;
        public string FlightStatus => "Flight " + (airborne ? (moving ? "moving" : "hovering") : "landed") +
            "; autonomous wire flight=" + allowAutoWiringFlight +
            "; last=" + (string.IsNullOrEmpty(lastError) ? (lastSucceeded ? "completed" : "ready") : lastError);

        private void Awake()
        {
            if (!character) character = GetComponent<SparkProCharacter>();
            if (!executive) executive = GetComponent<SparkProExecutive>();
            if (!groundAgent) groundAgent = GetComponent<NavMeshAgent>();
            body = GetComponent<Rigidbody>();
        }
        private void OnEnable() { SceneManager.activeSceneChanged += SceneChanged; }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= SceneChanged;
            StopMotionAndHover();
            DisableThrusters();
        }
        private void SceneChanged(Scene previous, Scene next)
        {
            StopMotionAndHover();
            // Object is often destroyed/reloaded here; never warp it onto a NavMesh.
            if (airborne) lastError = "Scene changed during flight; motion stopped safely.";
        }

        private bool Fail(string detail)
        {
            lastError = detail;
            lastSucceeded = false;
            moving = false;
            Debug.LogWarning("[Spark Flight] " + detail, this);
            return false;
        }
        private bool IsSafeToFly()
        {
            return isActiveAndEnabled && character && !character.WarningActive &&
                executive && !executive.ExamMode;
        }
        private bool BeginFlight()
        {
            if (!IsSafeToFly()) return Fail("Flight denied: character, warning state or independent exam guard.");
            if (airborne) return true;
            home = transform.position;
            homeRotation = transform.rotation;
            if (groundAgent)
            {
                agentWasEnabled = groundAgent.enabled;
                character.StopNavigation();
                if (groundAgent.enabled) groundAgent.enabled = false; // No NavMesh warp in the air.
            }
            if (body)
            {
                bodyWasKinematic = body.isKinematic;
                bodyUsedGravity = body.useGravity;
                body.isKinematic = true;
                body.useGravity = false;
            }
            airborne = true;
            EnsureThrusters();
            return true;
        }
        public void StopMotionAndHover()
        {
            if (manualAction != null)
            {
                StopCoroutine(manualAction);
                manualAction = null;
            }
            moving = false;
            // Remain hovering if airborne. Restoring NavMesh at altitude would snap/teleport.
        }
        private void RestoreGrounding()
        {
            if (groundAgent && agentWasEnabled) groundAgent.enabled = true;
            if (body)
            {
                body.isKinematic = bodyWasKinematic;
                body.useGravity = bodyUsedGravity;
            }
            airborne = false;
            moving = false;
            DisableThrusters();
        }
        private readonly struct FlightPose
        {
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;

            public FlightPose(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }

            public static FlightPose Lerp(FlightPose from, FlightPose to, float t) =>
                new FlightPose(Vector3.Lerp(from.Position, to.Position, t), Quaternion.Slerp(from.Rotation, to.Rotation, t));
        }

        private struct Workspace
        {
            public BoxCollider Table;
            public Matrix4x4 LocalToWorld;
            public Matrix4x4 WorldToLocal;
            public Bounds ColliderBounds;
            public Bounds Limits;
            public Vector3 Up;
            public Vector3 Scale;

            public bool Contains(Vector3 position) => Limits.Contains(WorldToLocal.MultiplyPoint3x4(position));

            public float Height(Vector3 position) => WorldToLocal.MultiplyPoint3x4(position).y * Scale.y;

            public bool IsCurrent => ReferenceEquals(Table, null) || (Table && Table.enabled && Table.gameObject.activeInHierarchy &&
                Table.transform.localToWorldMatrix == LocalToWorld &&
                Table.center == ColliderBounds.center && Table.size == ColliderBounds.size);

            public string Describe() => Table
                ? $"Table {Table.name}, center {LocalToWorld.MultiplyPoint3x4(ColliderBounds.center):F3}, " +
                    $"size {Vector3.Scale(ColliderBounds.size, Scale):F3} world units. " +
                    $"Bounds in tabletop local space {Limits.min:F3} to {Limits.max:F3}."
                : $"Manual flight fallback bounds in world space {Limits.min:F3} to {Limits.max:F3}.";
        }

        private enum Rejection { Outside, Reach, Occupied, Corridor, Clear }

        private struct Candidate
        {
            public Vector3 Position;
            public Rejection Result;
        }

        private struct BodyProbe
        {
            public Vector3 Offset;
            public float Radius;
        }

        private const float RotationSpeed = 155f;
        private const int CruiseWaypoints = 5;
        private const float PoseTolerance = .012f;
        private static readonly float[] ReachRings = { .16f, .30f, .45f, .60f, .76f };
        private static readonly float[] ShoulderHeights = { .025f, .085f, .145f, .205f, .265f, .315f };
        private static readonly Vector2[] ApproachAngles =
        {
            new Vector2(0f, 0f), new Vector2(65f, 0f), new Vector2(82f, 0f), new Vector2(90f, 0f),
            new Vector2(100f, 0f), new Vector2(75f, -42f), new Vector2(82f, -65f),
            new Vector2(90f, -42f), new Vector2(90f, -65f), new Vector2(90f, -82f),
            new Vector2(100f, -45f), new Vector2(100f, -70f)
        };
        private readonly BodyProbe[] bodyProbes = new BodyProbe[3];
        private readonly Collider[] overlaps = new Collider[32];
        private readonly RaycastHit[] sweeps = new RaycastHit[32];
        private readonly FlightPose[] trialRoute = new FlightPose[CruiseWaypoints];
        private readonly FlightPose[] selectedRoute = new FlightPose[CruiseWaypoints];
        private readonly FlightPose[] diagnosticRoute = new FlightPose[CruiseWaypoints];
        private readonly List<Candidate> candidates = new List<Candidate>(5760);
        private Workspace workspace;
        private FlightPose approach;
        private Vector3 diagnosticTarget, diagnosticStart;
        private int diagnosticWaypointCount;
        private bool selectedApproach;
        private string lastApproachReport;

        public string LastApproachReport => lastApproachReport ?? "No flight approach has been evaluated.";
        public int SuccessfulApproaches { get; private set; }
        internal bool ManualFlightIsBusy => manualAction != null;
        private FlightPose CurrentPose => new FlightPose(transform.position, transform.rotation);

        private bool RefreshWorkspace(bool requireTable)
        {
            var director = GetComponent<SparkNativeWireDirector>();
            var bench = director ? director.Workbench : null;
            if (bench && bench.TryGetTabletop(out var table))
            {
                var frame = table.transform;
                var matrix = frame.localToWorldMatrix;
                Vector3 scale = new Vector3(matrix.MultiplyVector(Vector3.right).magnitude,
                    matrix.MultiplyVector(Vector3.up).magnitude, matrix.MultiplyVector(Vector3.forward).magnitude);
                if (scale.x < .001f || scale.y < .001f || scale.z < .001f)
                    return Fail("The active tabletop has a degenerate transform.");

                Vector3 minimum = table.center - table.size * .5f;
                Vector3 maximum = table.center + table.size * .5f;
                minimum.y = maximum.y - .04f / scale.y;
                maximum.y += maximumHeightAboveHome / scale.y;
                minimum.x -= tabletopEdgeMargin / scale.x;
                maximum.x += tabletopEdgeMargin / scale.x;
                minimum.z -= tabletopEdgeMargin / scale.z;
                maximum.z += tabletopEdgeMargin / scale.z;
                var limits = new Bounds();
                limits.SetMinMax(minimum, maximum);
                workspace = new Workspace
                {
                    Table = table, LocalToWorld = matrix, WorldToLocal = frame.worldToLocalMatrix,
                    ColliderBounds = new Bounds(table.center, table.size), Limits = limits,
                    Up = matrix.MultiplyVector(Vector3.up).normalized, Scale = scale
                };
                return true;
            }

            if (requireTable) return Fail("Autonomous flight requires the active laboratory tabletop collider.");
            Vector3 min = home - new Vector3(maximumXDistance, maximumBelowHome, maximumZDistance);
            Vector3 max = home + new Vector3(maximumXDistance, maximumHeightAboveHome, maximumZDistance);
            var manualLimits = new Bounds();
            manualLimits.SetMinMax(min, max);
            workspace = new Workspace
            {
                LocalToWorld = Matrix4x4.identity, WorldToLocal = Matrix4x4.identity,
                Limits = manualLimits, Up = Vector3.up, Scale = Vector3.one
            };
            return true;
        }

        private void CaptureBody()
        {
            // The probe positions use the same rig that supplies the animated arm. Sweep
            // legs, chest and head while travelling or changing body pitch.
            var chest = character.GetAnchor("Body");
            var head = character.GetAnchor("Head");
            var leftAnkle = character.GetAnchor("Ankle_L");
            var rightAnkle = character.GetAnchor("Ankle_R");
            float scale = chest && head ? Mathf.Max(.1f, Vector3.Distance(chest.position, head.position) / .41f) : 1f;
            Vector3 up = transform.up;
            Vector3 legs = leftAnkle && rightAnkle
                ? (leftAnkle.position + rightAnkle.position) * .5f + up * (.09f * scale)
                : transform.position + up * bodySweepCenterHeight;
            Vector3 torso = chest ? chest.position + up * (.07f * scale) : transform.position + up * .65f;
            Vector3 crown = head ? head.position + up * (.18f * scale) : transform.position + up * 1.25f;
            Quaternion inverse = Quaternion.Inverse(transform.rotation);
            bodyProbes[0] = new BodyProbe { Offset = inverse * (legs - transform.position), Radius = Mathf.Max(bodySweepRadius, .13f * scale) };
            bodyProbes[1] = new BodyProbe { Offset = inverse * (torso - transform.position), Radius = Mathf.Max(bodySweepRadius, .25f * scale) };
            bodyProbes[2] = new BodyProbe { Offset = inverse * (crown - transform.position), Radius = Mathf.Max(bodySweepRadius, .27f * scale) };
        }

        private bool IsOwn(Collider collider) =>
            collider && (collider.transform == transform || collider.transform.IsChildOf(transform));

        private static string Obstruction(Collider collider) =>
            collider.name + " on layer " + LayerMask.LayerToName(collider.gameObject.layer) + $" ({collider.gameObject.layer})";

        private bool CanOccupy(FlightPose pose, out string obstruction)
        {
            obstruction = null;
            for (int i = 0; i < bodyProbes.Length; i++)
            {
                var probe = bodyProbes[i];
                Vector3 center = pose.Position + pose.Rotation * probe.Offset;
                int count = Physics.OverlapSphereNonAlloc(center, probe.Radius, overlaps, collisionLayers,
                    QueryTriggerInteraction.Ignore);
                for (int c = 0; c < count; c++)
                {
                    var collider = overlaps[c];
                    if (!collider || IsOwn(collider)) continue;
                    obstruction = Obstruction(collider);
                    return false;
                }
                if (count == overlaps.Length)
                {
                    obstruction = "too many colliders to verify body clearance";
                    return false;
                }
            }
            return true;
        }

        private bool CanTravel(FlightPose from, FlightPose to, out string obstruction)
        {
            if (!workspace.Contains(to.Position))
            {
                obstruction = "destination outside the tabletop flight volume";
                return false;
            }

            int samples = Mathf.Max(1, Mathf.CeilToInt(Quaternion.Angle(from.Rotation, to.Rotation) / 12f));
            FlightPose previous = from;
            for (int s = 1; s <= samples; s++)
            {
                FlightPose next = FlightPose.Lerp(from, to, (float)s / samples);
                if (!CanOccupy(next, out obstruction)) return false;
                for (int i = 0; i < bodyProbes.Length; i++)
                {
                    var probe = bodyProbes[i];
                    Vector3 start = previous.Position + previous.Rotation * probe.Offset;
                    Vector3 end = next.Position + next.Rotation * probe.Offset;
                    Vector3 delta = end - start;
                    float length = delta.magnitude;
                    if (length < .0001f) continue;
                    int count = Physics.SphereCastNonAlloc(start, probe.Radius, delta / length, sweeps,
                        length, collisionLayers, QueryTriggerInteraction.Ignore);
                    for (int h = 0; h < count; h++)
                    {
                        var collider = sweeps[h].collider;
                        if (!collider || IsOwn(collider)) continue;
                        obstruction = Obstruction(collider);
                        return false;
                    }
                    if (count == sweeps.Length)
                    {
                        obstruction = "too many colliders to verify the flight corridor";
                        return false;
                    }
                }
                previous = next;
            }
            obstruction = null;
            return true;
        }

        private bool TryCruiseRoute(FlightPose from, FlightPose target, FlightPose[] route,
            out string obstruction, float extraClearance = 0f)
        {
            float upper = workspace.Limits.max.y * workspace.Scale.y;
            float startHeight = workspace.Height(from.Position);
            float targetHeight = workspace.Height(target.Position);
            float high = Mathf.Min(upper - .025f, Mathf.Max(startHeight, targetHeight) + cruiseClearance + extraClearance);
            Vector3 highStart = from.Position + workspace.Up * (high - startHeight);
            Vector3 highGoal = target.Position + workspace.Up * (high - targetHeight);
            Vector3 heading = Vector3.ProjectOnPlane(highGoal - highStart, workspace.Up);
            if (heading.sqrMagnitude < .001f)
                heading = Vector3.ProjectOnPlane(target.Rotation * Vector3.forward, workspace.Up);
            Quaternion cruiseRotation = heading.sqrMagnitude < .001f
                ? Quaternion.LookRotation(workspace.LocalToWorld.MultiplyVector(Vector3.forward), workspace.Up)
                : Quaternion.LookRotation(heading, workspace.Up);
            route[0] = new FlightPose(highStart, from.Rotation);
            route[1] = new FlightPose(highStart, cruiseRotation);
            route[2] = new FlightPose(highGoal, cruiseRotation);
            route[3] = new FlightPose(highGoal, target.Rotation);
            route[4] = target;
            for (int i = 0; i < route.Length; i++)
            {
                if (!CanTravel(from, route[i], out obstruction)) return false;
                from = route[i];
            }
            obstruction = null;
            return true;
        }

        private bool TryApproachRoute(FlightPose from, FlightPose target, out int waypointCount, out string obstruction)
        {
            if (TryCruiseRoute(from, target, trialRoute, out obstruction) ||
                TryCruiseRoute(from, target, trialRoute, out obstruction, .45f))
            {
                waypointCount = CruiseWaypoints;
                return true;
            }
            waypointCount = 1;
            trialRoute[0] = target;
            return CanTravel(from, target, out obstruction);
        }

        internal bool IsGroundRouteClear(Vector3[] corners, float lateralPadding = 0f)
        {
            if (corners.Length == 0 || !RefreshWorkspace(true)) return false;
            CaptureBody();
            Vector3 initialHeading = corners.Length > 1 ? corners[1] - corners[0] : transform.forward;
            initialHeading = Vector3.ProjectOnPlane(initialHeading, workspace.Up);
            if (initialHeading.sqrMagnitude < .001f)
                initialHeading = workspace.LocalToWorld.MultiplyVector(Vector3.forward);
            var previous = new FlightPose(corners[0], Quaternion.LookRotation(initialHeading, workspace.Up));
            if (!CanOccupy(previous, out _)) return false;
            for (int i = 1; i < corners.Length; i++)
            {
                Vector3 heading = Vector3.ProjectOnPlane(corners[i] - previous.Position, workspace.Up);
                Quaternion rotation = heading.sqrMagnitude > .001f
                    ? Quaternion.LookRotation(heading, workspace.Up) : previous.Rotation;
                var next = new FlightPose(corners[i], rotation);
                if (!CanTravel(previous, next, out _)) return false;
                // Check both sides of a carry route for the difference between
                // ideal NavMesh corners and live steering and foot motion.
                if (lateralPadding > 0f && heading.sqrMagnitude > .001f)
                {
                    Vector3 side = Vector3.Cross(workspace.Up, heading.normalized) * lateralPadding;
                    if (!CanTravel(new FlightPose(previous.Position + side, previous.Rotation),
                            new FlightPose(next.Position + side, next.Rotation), out _) ||
                        !CanTravel(new FlightPose(previous.Position - side, previous.Rotation),
                            new FlightPose(next.Position - side, next.Rotation), out _))
                        return false;
                }
                previous = next;
            }
            return true;
        }

        internal bool IsNextGroundStepClear(Vector3 position, out string obstruction)
        {
            if (!workspace.IsCurrent)
            {
                obstruction = "the tabletop or its collider changed";
                return false;
            }
            CaptureBody();
            return CanTravel(CurrentPose, new FlightPose(position, transform.rotation), out obstruction);
        }

        private IEnumerator Follow(FlightPose[] route, int count, Func<bool> valid, Transform target = null)
        {
            float started = Time.realtimeSinceStartup;
            Vector3 targetPosition = target ? target.position : default;
            moving = true;
            for (int i = 0; i < count; i++)
            {
                var destination = route[i];
                while (Vector3.Distance(transform.position, destination.Position) > PoseTolerance ||
                    Quaternion.Angle(transform.rotation, destination.Rotation) > 1f)
                {
                    if (!IsSafeToFly() || (valid != null && !valid()))
                    { Fail("Flight interrupted by scene, exam, or safety state."); yield break; }
                    if (!workspace.IsCurrent || (target && Vector3.Distance(target.position, targetPosition) > .015f))
                    { Fail("The worktable or terminal moved during flight. Confirm a new approach."); yield break; }
                    if (Time.realtimeSinceStartup - started > motionTimeoutSeconds)
                    { Fail("Flight route timed out."); yield break; }

                    float dt = Mathf.Min(.05f, Time.unscaledDeltaTime);
                    var next = new FlightPose(Vector3.MoveTowards(transform.position, destination.Position, flightSpeed * dt),
                        Quaternion.RotateTowards(transform.rotation, destination.Rotation, RotationSpeed * dt));
                    if (!CanTravel(CurrentPose, next, out string obstruction))
                    { Fail("Physical flight corridor blocked by " + obstruction + "."); yield break; }
                    transform.SetPositionAndRotation(next.Position, next.Rotation);
                    yield return null;
                }
            }
            moving = false;
        }

        public IEnumerator FlyToArmReach(Transform terminal, Transform shoulder, float usableArmReach, Func<bool> valid)
        {
            lastSucceeded = false;
            lastError = null;
            if (!terminal || !shoulder || usableArmReach < .12f || valid == null || !valid())
            { Fail("Flight target, arm rig or active game context is invalid."); yield break; }
            if (!IsSafeToFly())
            { Fail("Flight approach denied by character or exam safety state."); yield break; }
            if (!RefreshWorkspace(true)) yield break;

            CaptureBody();
            Physics.SyncTransforms();
            candidates.Clear();
            selectedApproach = false;
            diagnosticTarget = terminal.position;
            diagnosticStart = transform.position;
            Vector3 shoulderOffset = Quaternion.Inverse(transform.rotation) * (shoulder.position - transform.position);
            Vector3 outward = Vector3.ProjectOnPlane(diagnosticStart - diagnosticTarget, workspace.Up);
            if (outward.sqrMagnitude < .01f) outward = workspace.LocalToWorld.MultiplyVector(Vector3.forward);
            outward.Normalize();
            float bestScore = float.PositiveInfinity;
            int selectedWaypoints = 0;
            var counts = new int[5];
            var blockers = new Dictionary<string, int>(StringComparer.Ordinal);
            Vector3 firstCandidate = default;
            float minimumCandidateY = float.PositiveInfinity, maximumCandidateY = float.NegativeInfinity;
            foreach (var angles in ApproachAngles)
            foreach (float ring in ReachRings)
            foreach (float height in ShoulderHeights)
            for (int direction = 0; direction < 16; direction++)
            {
                float radius = usableArmReach * ring;
                Vector3 radial = Quaternion.AngleAxis(direction * 22.5f, workspace.Up) * outward;
                // Banking around Spark's long axis lowers the working shoulder and
                // raises the center of his torso without extending the arm.
                var facing = Quaternion.LookRotation(-radial, workspace.Up) *
                    Quaternion.AngleAxis(angles.x, Vector3.right) * Quaternion.AngleAxis(angles.y, Vector3.up);
                Vector3 shoulderPosition = diagnosticTarget + radial * radius + workspace.Up * height;
                var pose = new FlightPose(shoulderPosition - facing * shoulderOffset, facing);
                if (candidates.Count == 0) firstCandidate = pose.Position;
                minimumCandidateY = Mathf.Min(minimumCandidateY, pose.Position.y);
                maximumCandidateY = Mathf.Max(maximumCandidateY, pose.Position.y);
                Rejection result;
                string obstacle = null;
                if (radius * radius + height * height > (usableArmReach - .018f) * (usableArmReach - .018f))
                    result = Rejection.Reach;
                else if (!workspace.Contains(pose.Position))
                    result = Rejection.Outside;
                else if (!CanOccupy(pose, out obstacle))
                    result = Rejection.Occupied;
                else if (!TryApproachRoute(CurrentPose, pose, out int routeCount, out obstacle))
                    result = Rejection.Corridor;
                else
                {
                    result = Rejection.Clear;
                    float score = Vector3.Distance(diagnosticStart, pose.Position) + angles.x * .002f +
                        Mathf.Abs(angles.y) * .0015f +
                        radius * .35f + height * .35f;
                    if (score < bestScore)
                    {
                        bestScore = score;
                        approach = pose;
                        selectedWaypoints = routeCount;
                        Array.Copy(trialRoute, selectedRoute, routeCount);
                        selectedApproach = true;
                    }
                }
                if (obstacle != null)
                    blockers[obstacle] = blockers.TryGetValue(obstacle, out int blocked) ? blocked + 1 : 1;
                counts[(int)result]++;
                candidates.Add(new Candidate { Position = pose.Position, Result = result });
            }

            string blocker = "none";
            int blockerCount = 0;
            foreach (var pair in blockers)
                if (pair.Value > blockerCount) { blocker = pair.Key; blockerCount = pair.Value; }
            lastApproachReport = workspace.Describe() +
                $" Spark {diagnosticStart:F3}, terminal {terminal.name} {diagnosticTarget:F3}, shoulder offset {shoulderOffset:F3}, " +
                $"sample {firstCandidate:F3}, candidate world y {minimumCandidateY:F3} to {maximumCandidateY:F3}, arm reach {usableArmReach:F3}. " +
                $"Tested {candidates.Count}, outside volume {counts[(int)Rejection.Outside]}, out of reach {counts[(int)Rejection.Reach]}, " +
                $"occupied {counts[(int)Rejection.Occupied]}, blocked route {counts[(int)Rejection.Corridor]}, clear routes {counts[(int)Rejection.Clear]}. " +
                $"Main blocker {blocker} ({blockerCount}). " +
                (selectedApproach ? $"Selected {approach.Position:F3}, rotation {approach.Rotation.eulerAngles:F1}." : "No approach selected.");
            if (!selectedApproach)
            { Fail("No safe flight approach to " + terminal.name + ". " + lastApproachReport); yield break; }

            diagnosticWaypointCount = selectedWaypoints;
            Array.Copy(selectedRoute, diagnosticRoute, selectedWaypoints);
            if (!BeginFlight()) yield break;
            Debug.Log("[Spark Flight] " + lastApproachReport, this);
            yield return Follow(selectedRoute, selectedWaypoints, valid, terminal);
            if (!string.IsNullOrEmpty(lastError)) yield break;
            if (!valid() || !terminal || !shoulder || Vector3.Distance(shoulder.position, terminal.position) > usableArmReach)
            {
                Fail("Actual shoulder-to-terminal distance exceeds the measured reach at the final flight pose.");
                yield break;
            }

            lastSucceeded = true;
            SuccessfulApproaches++;
        }

        internal IEnumerator RiseToSafeHover(Func<bool> valid)
        {
            if (!airborne || !IsSafeToFly() || valid == null || !valid()) yield break;
            CaptureBody();
            if (diagnosticWaypointCount == CruiseWaypoints && Vector3.Distance(transform.position, approach.Position) < .12f)
            {
                selectedRoute[0] = diagnosticRoute[3];
                selectedRoute[1] = diagnosticRoute[2];
                if (CanTravel(CurrentPose, selectedRoute[0], out _) &&
                    CanTravel(selectedRoute[0], selectedRoute[1], out _))
                {
                    yield return Follow(selectedRoute, 2, valid);
                    yield break;
                }
            }
            Vector3 heading = Vector3.ProjectOnPlane(transform.forward, workspace.Up);
            if (heading.sqrMagnitude < .001f)
                heading = workspace.LocalToWorld.MultiplyVector(Vector3.forward);
            float height = workspace.Height(transform.position);
            float cap = workspace.Limits.max.y * workspace.Scale.y - .025f;
            Vector3 raised = transform.position + workspace.Up * Mathf.Max(0f, Mathf.Min(.65f, cap - height));
            selectedRoute[0] = new FlightPose(raised, transform.rotation);
            selectedRoute[1] = new FlightPose(raised, Quaternion.LookRotation(heading, workspace.Up));
            if (CanTravel(CurrentPose, selectedRoute[0], out _) &&
                CanTravel(selectedRoute[0], selectedRoute[1], out _))
                yield return Follow(selectedRoute, 2, valid);
        }

        internal IEnumerator LandForWalking(Vector3 position, Vector3 terminal, Func<bool> valid)
        {
            lastSucceeded = false;
            lastError = null;
            if (!airborne || !groundAgent || !IsSafeToFly() || !valid() || !RefreshWorkspace(true))
                yield break;

            CaptureBody();
            Vector3 forward = Vector3.ProjectOnPlane(terminal - position, workspace.Up);
            if (forward.sqrMagnitude < .001f) forward = workspace.LocalToWorld.MultiplyVector(Vector3.forward);
            var landing = new FlightPose(position, Quaternion.LookRotation(forward, workspace.Up));
            selectedRoute[0] = landing;
            int waypointCount = 1;
            if (!CanTravel(CurrentPose, landing, out string obstruction))
            {
                waypointCount = CruiseWaypoints;
                if (!TryCruiseRoute(CurrentPose, landing, selectedRoute, out obstruction))
                {
                    lastError = "Landing route blocked by " + obstruction + ".";
                    yield break;
                }
            }
            yield return Follow(selectedRoute, waypointCount, valid);
            if (!string.IsNullOrEmpty(lastError) || !valid()) yield break;

            var filter = new NavMeshQueryFilter { agentTypeID = groundAgent.agentTypeID, areaMask = groundAgent.areaMask };
            if (!NavMesh.SamplePosition(transform.position, out var hit, .065f, filter) ||
                Vector3.Distance(hit.position, position) > .065f)
            {
                Fail("The planned walking surface is no longer available at the landing point.");
                yield break;
            }
            groundAgent.enabled = true;
            if (!groundAgent.isOnNavMesh)
            {
                groundAgent.enabled = false;
                Fail("Spark could not resume walking at the planned landing point.");
                yield break;
            }
            RestoreGrounding();
            lastSucceeded = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || candidates.Count == 0) return;
            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = workspace.LocalToWorld;
            Gizmos.color = new Color(.2f, .65f, 1f, .8f);
            Gizmos.DrawWireCube(workspace.Limits.center, workspace.Limits.size);
            Gizmos.matrix = previous;
            Gizmos.DrawWireSphere(diagnosticTarget, .045f);
            foreach (var candidate in candidates)
            {
                Gizmos.color = candidate.Result switch
                {
                    Rejection.Clear => new Color(.25f, 1f, .4f, .65f),
                    Rejection.Outside => new Color(.9f, .25f, .3f, .45f),
                    Rejection.Reach => new Color(.6f, .45f, 1f, .45f),
                    _ => new Color(1f, .65f, .15f, .6f)
                };
                Gizmos.DrawSphere(candidate.Position, .014f);
            }
            if (!selectedApproach) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(approach.Position, .055f);
            Gizmos.DrawLine(approach.Position, diagnosticTarget);
            Vector3 from = diagnosticStart;
            for (int i = 0; i < diagnosticWaypointCount; i++)
            {
                var waypoint = diagnosticRoute[i];
                Gizmos.DrawLine(from, waypoint.Position);
                from = waypoint.Position;
            }
        }

        public bool RequestTakeoff(out string response)
        {
            if (!isActiveAndEnabled || (manualAction != null) ||
                (GetComponent<SparkNativeWireCinematic>() is SparkNativeWireCinematic c && c.IsPerforming))
            { response = "Flight controller busy with a wire or another action."; return false; }
            if (airborne) { response = FlightStatus; return true; }
            if (!BeginFlight()) { response = lastError; return false; }
            manualAction = StartCoroutine(ManualTakeoff());
            response = "Spark is taking off and will hover above the workstation. No circuit changes.";
            return true;
        }
        private IEnumerator ManualTakeoff()
        {
            yield return null; // StartCoroutine handle assigned before possible immediate failure.
            lastError = null;
            lastSucceeded = false;
            if (!RefreshWorkspace(false)) { manualAction = null; yield break; }
            CaptureBody();
            Vector3 target = transform.position + workspace.Up * takeoffRise;
            selectedRoute[0] = new FlightPose(target, transform.rotation);
            yield return Follow(selectedRoute, 1, IsSafeToFly);
            lastSucceeded = string.IsNullOrEmpty(lastError);
            manualAction = null;
        }
        public bool RequestFlyToSelected(out string response)
        {
            if (manualAction != null || moving ||
                (GetComponent<SparkNativeWireCinematic>() is SparkNativeWireCinematic performer && performer.IsPerforming))
            { response = "Flight is busy. Wait for the current wiring or flight operation."; return false; }
            if (!IsSafeToFly() || !executive.ActiveSelectionController ||
                !executive.ActiveSelectionController.SelectedObject)
            { response = "Select an active level terminal or component before requesting flight."; return false; }
            var manager = executive.ActiveLevelManager;
            var level = manager ? SparkAILevelContext.Capture(manager) : default;
            Transform target = executive.ActiveSelectionController.SelectedObject.transform;
            if (!level.Layout || !target || !target.gameObject.activeInHierarchy ||
                !target.IsChildOf(level.Layout.transform))
            { response = "Selected target is not within the active practice level."; return false; }
            Transform shoulder = character.GetAnchor("Shoulder_R");
            Transform elbow = character.GetAnchor("Elbow_R");
            Transform wrist = character.GetAnchor("Wrist_R");
            if (!shoulder || !elbow || !wrist)
            { response = "Right arm rig is not bound. Fly-to-selected needs the runtime character rig."; return false; }
            float reach = Vector3.Distance(shoulder.position, elbow.position) +
                Vector3.Distance(elbow.position, wrist.position) - .095f;
            trackedSelected = target;
            Func<bool> stillValid = () => IsSafeToFly() && executive.ActiveLevelManager == manager &&
                target && target.gameObject.activeInHierarchy &&
                SparkAILevelContext.Capture(manager).Layout == level.Layout;
            manualAction = StartCoroutine(ManualFlyToSelected(target, shoulder, reach, stillValid));
            response = "Spark is flying toward the selected active-level object. No circuit edits.";
            return true;
        }
        private IEnumerator ManualFlyToSelected(Transform target, Transform shoulder, float reach, Func<bool> valid)
        {
            yield return null;
            yield return FlyToArmReach(target, shoulder, reach, valid);
            manualAction = null;
        }

        public bool RequestLanding(out string response)
        {
            if (!airborne) { response = "Spark is already on the ground."; return true; }
            var performer = GetComponent<SparkNativeWireCinematic>();
            if (performer && performer.IsPerforming) { response = "Cannot land while an actual cable is being attached."; return false; }
            if (manualAction != null) { response = "Flight is busy. Say 'spark hover' first."; return false; }
            manualAction = StartCoroutine(ManualLand());
            response = "Spark is returning to his previous safe ground position.";
            return true;
        }
        private IEnumerator ManualLand()
        {
            yield return null;
            lastError = null;
            lastSucceeded = false;
            if (!RefreshWorkspace(false)) { manualAction = null; yield break; }
            CaptureBody();
            var landing = new FlightPose(home, homeRotation);
            if (!TryCruiseRoute(CurrentPose, landing, selectedRoute, out string obstacle))
            {
                Fail("Landing corridor is blocked by " + obstacle + ". Spark remains hovering.");
                manualAction = null;
                yield break;
            }
            yield return Follow(selectedRoute, selectedRoute.Length, IsSafeToFly);
            if (string.IsNullOrEmpty(lastError) && groundAgent && agentWasEnabled)
            {
                var filter = new NavMeshQueryFilter { agentTypeID = groundAgent.agentTypeID, areaMask = groundAgent.areaMask };
                if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, .14f, filter) ||
                    Vector3.Distance(hit.position, transform.position) > .14f)
                    Fail("Landing site is no longer on a valid NavMesh; Spark remains hovering.");
            }
            if (string.IsNullOrEmpty(lastError))
            {
                RestoreGrounding();
                lastSucceeded = true;
            }
            manualAction = null;
        }
        private void EnsureThrusters()
        {
            if (!createSubtleThrusterParticles) return;
            if (!leftThruster) leftThruster = CreateThruster("Spark_Thruster_L", "Ankle_L");
            if (!rightThruster) rightThruster = CreateThruster("Spark_Thruster_R", "Ankle_R");
            if (leftThruster && !leftThruster.isPlaying) leftThruster.Play();
            if (rightThruster && !rightThruster.isPlaying) rightThruster.Play();
        }
        private ParticleSystem CreateThruster(string name, string bone)
        {
            Transform anchor = character ? character.GetAnchor(bone) : null;
            if (!anchor) return null;
            var obj = new GameObject(name);
            obj.transform.SetParent(anchor, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var particles = obj.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.startLifetime = .24f;
            main.startSpeed = .32f;
            main.startSize = .045f;
            main.maxParticles = 30;
            main.startColor = new Color(.12f, .72f, 1f, .75f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.rateOverTime = 15f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = .025f;
            return particles;
        }
        private void DisableThrusters()
        {
            if (leftThruster) leftThruster.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (rightThruster) rightThruster.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        public static bool TryHandleFrom(SparkProExecutive source, string command, out string answer)
        {
            answer = null;
            if (!source || string.IsNullOrWhiteSpace(command)) return false;
            string q = command.Trim().TrimEnd('?', '.', '!').ToLowerInvariant();
            if (q != "spark take off" && q != "take off" && q != "spark fly" &&
                q != "spark land" && q != "land spark" && q != "spark hover" &&
                q != "spark flight status" && q != "flight status" &&
                q != "spark fly to selected" && q != "fly to selected") return false;
            var flight = source.GetComponent<SparkV5FlightController>();
            if (!flight) { answer = "Spark's flight controller is not installed. Run AAA Laboratory > 25 - Bind Flight v13."; return true; }
            if (source.ExamMode) { answer = "Independent examination mode blocks AI flight actions."; return true; }
            if (q == "spark flight status" || q == "flight status") answer = flight.FlightStatus;
            else if (q == "spark land" || q == "land spark") flight.RequestLanding(out answer);
            else if (q == "spark fly to selected" || q == "fly to selected")
                flight.RequestFlyToSelected(out answer);
            else if (q == "spark hover")
            {
                if (source.GetComponent<SparkNativeWireCinematic>() is SparkNativeWireCinematic performer && performer.IsPerforming)
                    answer = "Cannot interrupt active native wiring with a manual hover command; use the native cancel command.";
                else { flight.StopMotionAndHover(); answer = flight.FlightStatus; }
            }
            else flight.RequestTakeoff(out answer);
            return true;
        }
    }
}
