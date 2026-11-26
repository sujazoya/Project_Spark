using System;
using System.Collections.Generic;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// v18.4: Original Spark hand, post-rotation precision grip and nested mechanical arms.
    /// Draws multi-stage mechanical sleeves only (never duplicate hands), preserves
    /// authored imported mesh scales and retains the existing native-wire APIs.
    /// No wire, circuit, inventory or level mutation; native tools stay authoritative.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(825)]
    public sealed class SparkTelescopicArms : MonoBehaviour
    {
        public enum ManipulatorSide { Right, Left }
        private enum ArmMode { Idle, Extending, Holding, Retracting }

        [Header("Real Spark V5 rig")]
        [SerializeField] private SparkProCharacter character;
        [SerializeField] private SparkProExecutive executive;
        [SerializeField] private bool allowLeftArm = true;

        [Header("Reach configuration (meters / Unity units)")]
        [SerializeField, Min(0.4f)] private float maximumReach = 6f;
        [SerializeField, Min(0.1f)] private float extensionSpeed = 3.25f;
        [SerializeField, Min(0.1f)] private float retractionSpeed = 3.75f;
        [SerializeField, Range(0f, 0.2f)] private float connectorGripOffset = 0.045f;
        [SerializeField, Range(0f, 0.2f)] private float elbowBend = 0.06f;
        [SerializeField, Range(0.3f, 0.7f)] private float elbowFraction = 0.52f;
        [SerializeField, Range(0.01f, 0.25f)] private float arrivalTolerance = 0.07f;
        [Header("Precise original-hand terminal contact")]
        [SerializeField, Range(0.002f, 0.020f)] private float precisionContactTolerance = 0.007f;

        [Header("Physical approach safeguards")]
        [SerializeField] private bool checkArmCollision = true;
        [SerializeField] private LayerMask obstructionLayers = ~0;
        [SerializeField, Range(0.005f, 0.08f)] private float armCollisionRadius = 0.022f;
        [SerializeField, Range(0.05f, 0.4f)] private float shoulderClearance = 0.12f;
        [SerializeField, Range(0.05f, 0.4f)] private float tipClearance = 0.16f;

        [Header("Original rig geometry only — never create a second hand")]
        [SerializeField] private bool synchronizeAllRigCopies = true;
        // Legacy serialized field: kept for prefab compatibility only.
        // Imported meshes are NEVER scaled because their local axes do not necessarily
        // coincide with the shoulder-elbow and elbow-wrist segments.
        [SerializeField, HideInInspector] private bool stretchOriginalArmMeshes;
        [Header("AAA articulated telescopic mechanism — structural sleeves, NOT hands")]
        // Legacy v18.2 fields remain serialized for compatibility but no single-span rods are drawn.
        [SerializeField, HideInInspector] private bool showJointBridges;
        [SerializeField, HideInInspector] private float bridgeRadius = 0.016f;
        [SerializeField, HideInInspector] private float bridgeActivationRatio = 1.12f;
        [SerializeField, HideInInspector] private bool showTelescopicSleeves;
        [SerializeField] private bool showMechanicalStages = true;
        [SerializeField, Range(3, 7)] private int stagesPerLimb = 5;
        [SerializeField, Range(0.008f, 0.045f)] private float baseSleeveRadius = 0.026f;
        [SerializeField, Range(0.004f, 0.03f)] private float tipSleeveRadius = 0.014f;
        [SerializeField, Range(0.008f, 0.035f)] private float collarLength = 0.018f;
        [SerializeField, Range(0f, 0.45f)] private float stageOverlap = 0.30f;
        [SerializeField, Range(1f, 2.5f)] private float mechanicalThicknessMultiplier = 1.65f;
        [SerializeField, Range(0.04f, 0.30f)] private float proximalHousingLength = 0.14f;
        [SerializeField, Range(1.02f, 1.5f)] private float extensionVisualThreshold = 1.09f;

        [Header("Real original thumb + fingertip contact")]
        [SerializeField] private bool useExistingFingerTips = true;
        [SerializeField] private Vector3 fallbackGripOffsetLocal = new Vector3(0f, 0f, 0.045f);
        [SerializeField] private bool alignWristTowardContact;
        [SerializeField, Range(1f, 25f)] private float handAlignmentSpeed = 8f;
        [SerializeField] private bool allowEditorDebugPanel;
        [SerializeField] private string debugTerminalId = "SwitchOutput";

        private sealed class Arm
        {
            public Transform shoulder, elbow, wrist;
            public Transform explicitGrip, thumbTip, indexTip;
            public float gripError;
            public Vector3 elbowHomeLocal, wristHomeLocal;
            public Quaternion shoulderHomeRotation, elbowHomeRotation, wristHomeRotation;
            public float naturalLength;
            public float upperRestWorld, lowerRestWorld;
            public ArmMode mode;
            public Vector3 goal;
            public Transform trackedTarget;
            public LimbMesh[] upperMeshes, lowerMeshes;
            public Transform allowedTerminalContact;
            public bool ready;
        }

        private sealed class LimbMesh
        {
            public Transform transform;
            public Vector3 homeScale;
        }

        private readonly Arm[] arms = { new Arm(), new Arm() };
        private readonly List<Arm> additionalRightRigs = new List<Arm>(5);
        private readonly List<Arm> additionalLeftRigs = new List<Arm>(5);
        private readonly RaycastHit[] collisionHits = new RaycastHit[48];
        // Four anatomical links: R upper/lower and L upper/lower.
        // Procedural structural parts only: never hand or finger clones.
        private sealed class MechanicalPart
        {
            public Transform transform;
            public Renderer renderer;
        }
        private readonly MechanicalPart[][] stages = new MechanicalPart[4][];
        private readonly MechanicalPart[][] collars = new MechanicalPart[4][];
        private readonly MechanicalPart[] proximalHousings = new MechanicalPart[4];
        private readonly MechanicalPart[] distalCouplers = new MechanicalPart[4];
        private Mesh sharedSleeveMesh;
        private Material materialGraphite, materialTitanium, materialAccent;
        private bool mechanismsReady;
        private bool rigReady;
        private int lastAppliedFrame = -1;
        public string LastError { get; private set; } = "Not initialized";
        public float MaximumReach => maximumReach;
        public float ReachTimeoutSeconds => Mathf.Max(1.0f, maximumReach / Mathf.Max(0.1f, extensionSpeed) + 0.7f);
        public bool SetMaximumReach(float meters)
        {
            if (float.IsNaN(meters) || float.IsInfinity(meters) || meters < 0.4f || meters > 1000f)
            { LastError = "Reach must be a finite number between 0.4 and 1000 meters"; return false; }
            maximumReach = meters;
            LastError = "Configured maximum reach: " + meters.ToString("F2") + "m";
            return true;
        }
        public bool IsExtending => arms[0].mode == ArmMode.Extending || arms[1].mode == ArmMode.Extending;
        public bool IsActive => arms[0].mode == ArmMode.Extending || arms[0].mode == ArmMode.Holding ||
                                arms[1].mode == ArmMode.Extending || arms[1].mode == ArmMode.Holding;
        public bool IsBound => rigReady;
        public string Status => string.Format("AAA original-hand arms: rig={0}; right={1}; left={2}; maximumReach={3:F2}m; rigCopies(R/L)={4}/{5}; rightGripGap={6:F3}m; leftGripGap={7:F3}m; {8}",
            rigReady, arms[0].mode, arms[1].mode, maximumReach,
            additionalRightRigs.Count + 1, additionalLeftRigs.Count + 1,
            arms[0].gripError, arms[1].gripError, LastError);
        public float RightGripError => arms[0].gripError;
        public float LeftGripError => arms[1].gripError;
        public float ContactTolerance => precisionContactTolerance;
        public bool RightContactConfirmed => arms[0].mode == ArmMode.Holding && arms[0].gripError <= precisionContactTolerance;
        public bool LeftContactConfirmed => arms[1].mode == ArmMode.Holding && arms[1].gripError <= precisionContactTolerance;
        public int ActiveStageCount
        {
            get
            {
                int total = 0;
                for (int link = 0; link < 4; link++)
                {
                    var entries = stages[link];
                    if (entries == null) continue;
                    for (int i = 0; i < entries.Length; i++)
                        if (entries[i] != null && entries[i].transform && entries[i].transform.gameObject.activeSelf) total++;
                }
                return total;
            }
        }
        public bool TryGetGripPosition(ManipulatorSide side, out Vector3 position)
        {
            position = Vector3.zero;
            if (!rigReady || !arms[(int)side].ready) return false;
            position = GripWorld(arms[(int)side]);
            return IsFinite(position);
        }

        private void Awake()
        {
            if (!character) character = GetComponent<SparkProCharacter>();
            if (!executive) executive = GetComponent<SparkProExecutive>();
        }
        private void Start()
        {
            RemoveOldV18ProxyObjects();
            if (BindRig()) CreateMechanicalStages();
        }
        private void OnEnable() { SceneManager.activeSceneChanged += OnSceneChanged; }
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            RetractAll(true);
            HideMechanicalStages();
        }
        private void OnDestroy()
        {
            for (int link = 0; link < 4; link++)
            {
                DestroyParts(stages[link]);
                DestroyParts(collars[link]);
                DestroyPart(proximalHousings[link]);
                DestroyPart(distalCouplers[link]);
            }
            if (sharedSleeveMesh) Destroy(sharedSleeveMesh);
            if (materialGraphite) Destroy(materialGraphite);
            if (materialTitanium) Destroy(materialTitanium);
            if (materialAccent) Destroy(materialAccent);
        }
        private void OnSceneChanged(Scene previous, Scene next) { RetractAll(true); HideMechanicalStages(); }

        public bool BindRig()
        {
            if (rigReady) return true;
            if (!character) { LastError = "SparkProCharacter is missing"; return false; }
            if (!Setup(arms[0], "R") || !Setup(arms[1], "L"))
            {
                LastError = "Spark shoulder/elbow/wrist pivots are not bound";
                return false;
            }
            if (synchronizeAllRigCopies) DiscoverOtherOriginalRigs();
            rigReady = true;
            LastError = "Ready — original hands only, no generated proxy objects";
            return true;
        }
        private bool Setup(Arm a, string side)
        {
            return SetupActualRig(a,
                character.GetAnchor("Shoulder_" + side),
                character.GetAnchor("Elbow_" + side),
                character.GetAnchor("Wrist_" + side));
        }

        private static bool SetupActualRig(Arm a, Transform shoulder, Transform elbow, Transform wrist)
        {
            if (!shoulder || !elbow || !wrist || elbow.parent != shoulder || wrist.parent != elbow)
                return false;
            a.shoulder = shoulder;
            a.elbow = elbow;
            a.wrist = wrist;
            a.elbowHomeLocal = elbow.localPosition;
            a.wristHomeLocal = wrist.localPosition;
            a.shoulderHomeRotation = shoulder.localRotation;
            a.elbowHomeRotation = elbow.localRotation;
            a.wristHomeRotation = wrist.localRotation;
            a.upperRestWorld = Vector3.Distance(shoulder.position, elbow.position);
            a.lowerRestWorld = Vector3.Distance(elbow.position, wrist.position);
            a.naturalLength = a.upperRestWorld + a.lowerRestWorld;
            a.upperMeshes = CaptureOriginalMeshes(shoulder, elbow);
            a.lowerMeshes = CaptureOriginalMeshes(elbow, wrist);
            string side = shoulder.name.EndsWith("_R", StringComparison.Ordinal) ? "R" : "L";
            a.explicitGrip = FindDescendant(wrist, "SparkGrip_" + side) ??
                             FindDescendant(wrist, "CableGrip_" + side);
            a.thumbTip = FindDescendant(wrist, "ThumbTip_" + side) ??
                         FindDescendant(wrist, "Thumb_" + side);
            a.indexTip = FindDescendant(wrist, "FingerTip0_" + side) ??
                         FindDescendant(wrist, "Finger0_" + side);
            a.mode = ArmMode.Idle;
            a.gripError = 0f;
            a.ready = a.naturalLength > 0.04f;
            return a.ready;
        }

        private static Transform FindDescendant(Transform root, string exactName)
        {
            if (!root) return null;
            Transform[] nodes = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < nodes.Length; i++)
                if (nodes[i] && nodes[i].name == exactName) return nodes[i];
            return null;
        }

        // A dedicated original-rig grip anchor wins. Otherwise use the actual
        // thumb/finger contact midpoint; never use a surrogate hand GameObject.
        private Vector3 GripWorld(Arm a)
        {
            if (a.explicitGrip) return a.explicitGrip.position;
            if (useExistingFingerTips && a.thumbTip && a.indexTip)
                return Vector3.Lerp(a.thumbTip.position, a.indexTip.position, 0.55f);
            return a.wrist.TransformPoint(fallbackGripOffsetLocal);
        }

        private static LimbMesh[] CaptureOriginalMeshes(Transform joint, Transform nextJoint)
        {
            var result = new List<LimbMesh>(4);
            for (int i = 0; i < joint.childCount; i++)
            {
                Transform part = joint.GetChild(i);
                if (part == nextJoint) continue;
                // Only existing mesh objects directly under the limb joint. Never touch fingers or hands.
                if (!part.GetComponent<Renderer>() && !part.GetComponent<MeshFilter>()) continue;
                result.Add(new LimbMesh { transform = part, homeScale = part.localScale });
            }
            return result.ToArray();
        }

        private static Transform DirectChildNamed(Transform parent, string required)
        {
            if (!parent) return null;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == required) return child;
            }
            return null;
        }

        // Spark's prefab includes multiple LOD/model rig copies. Every existing hand must follow
        // the same target, otherwise the untouched LOD hands appear as extra disconnected hands.
        private void DiscoverOtherOriginalRigs()
        {
            additionalRightRigs.Clear();
            additionalLeftRigs.Clear();
            Transform[] all = character.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform shoulder = all[i];
                bool right = shoulder.name == "Shoulder_R";
                if (!right && shoulder.name != "Shoulder_L") continue;
                Arm primary = arms[right ? 0 : 1];
                if (shoulder == primary.shoulder) continue;
                string suffix = right ? "R" : "L";
                Transform elbow = DirectChildNamed(shoulder, "Elbow_" + suffix);
                Transform wrist = DirectChildNamed(elbow, "Wrist_" + suffix);
                Arm other = new Arm();
                if (SetupActualRig(other, shoulder, elbow, wrist))
                {
                    if (right) additionalRightRigs.Add(other);
                    else additionalLeftRigs.Add(other);
                }
            }
        }

        private bool SafetyAllowsReach()
        {
            if (executive && executive.ExamMode) { LastError = "Independent exam mode forbids AI-assisted extending"; return false; }
            if (character && character.WarningActive) { LastError = "Spark's character safety warning is active"; return false; }
            return true;
        }
        public bool CanReachFrom(Vector3 shoulderWorld, Vector3 targetWorld)
        {
            return CanReachFromInternal(shoulderWorld, targetWorld, null);
        }

        private bool CanReachFromInternal(Vector3 shoulderWorld, Vector3 targetWorld, Transform contact)
        {
            if (!IsFinite(shoulderWorld) || !IsFinite(targetWorld)) { LastError = "Non-finite target position"; return false; }
            Vector3 delta = targetWorld - shoulderWorld;
            float distance = delta.magnitude;
            if (distance > maximumReach + 0.001f) { LastError = "Target exceeds configured telescopic reach"; return false; }
            if (distance < 0.025f) { LastError = "Target intersects shoulder origin"; return false; }
            if (!SafetyAllowsReach()) return false;
            if (!checkArmCollision) { LastError = "Reach available (collision check disabled by user)"; return true; }
            float travel = distance - shoulderClearance - tipClearance;
            if (travel <= 0f) { LastError = "Reach available (near contact)"; return true; }
            Vector3 unit = delta / distance;
            Vector3 origin = shoulderWorld + unit * shoulderClearance;
            int hits = Physics.SphereCastNonAlloc(origin, armCollisionRadius, unit, collisionHits,
                travel, obstructionLayers, QueryTriggerInteraction.Ignore);
            if (hits == collisionHits.Length) { LastError = "Too many obstacles to verify arm clearance"; return false; }
            for (int i = 0; i < hits; i++)
            {
                Collider c = collisionHits[i].collider;
                if (!c || c.transform.IsChildOf(transform)) continue;
                // Only permit the actual requested terminal's own collider near the last few
                // centimeters. Never ignore a collider obstructing the middle of the arm.
                bool belongsToContact = contact &&
                    (c.transform == contact || c.transform.IsChildOf(contact) || contact.IsChildOf(c.transform));
                float fromGoal = Vector3.Distance(collisionHits[i].point, targetWorld);
                if (belongsToContact && fromGoal <= tipClearance + armCollisionRadius + 0.035f)
                    continue;
                LastError = "Extended arm blocked by " + c.name + " on layer " + LayerMask.LayerToName(c.gameObject.layer);
                return false;
            }
            LastError = "Reach available";
            return true;
        }
        private static bool IsFinite(Vector3 v)
        {
            return !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z) &&
                   !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z);
        }
        public bool CanReachPoint(Vector3 target, ManipulatorSide side = ManipulatorSide.Right)
        {
            if (!BindRig()) return false;
            Arm a = arms[(int)side];
            if (side == ManipulatorSide.Left && !allowLeftArm) return false;
            return CanReachFrom(a.shoulder.position, target);
        }

        public bool TryReach(Vector3 worldTarget, ManipulatorSide side = ManipulatorSide.Right)
        {
            if (!CanReachPoint(worldTarget, side)) return false;
            Arm a = arms[(int)side];
            a.trackedTarget = null;
            a.allowedTerminalContact = null;
            a.goal = worldTarget;
            a.mode = ArmMode.Extending;
            lastAppliedFrame = -1;
            return true;
        }
        public bool TryReachTransform(Transform target, ManipulatorSide side = ManipulatorSide.Right)
        {
            if (!target) { LastError = "No target assigned"; return false; }
            if (!TryReach(target.position, side)) return false;
            arms[(int)side].trackedTarget = target;
            return true;
        }
        public bool TryReachTerminal(string id, ManipulatorSide side = ManipulatorSide.Right)
        {
            if (!executive || !executive.ActiveLevelManager)
            { LastError = "No active level manager"; return false; }
            var context = SparkAILevelContext.Capture(executive.ActiveLevelManager);
            if (!context.Layout || !context.Layout.SceneBindings)
            { LastError = "Active level has no authored SceneBindings"; return false; }
            if (!context.Layout.SceneBindings.TryGetTerminal(id, out SparkTerminal terminal) || !terminal ||
                !terminal.gameObject.activeInHierarchy)
            { LastError = "Terminal ID not found in active level: " + id; return false; }
            if (!BindRig()) return false;
            if (side == ManipulatorSide.Left && !allowLeftArm)
            { LastError = "Left arm extension disabled"; return false; }
            Arm a = arms[(int)side];
            if (!CanReachFromInternal(a.shoulder.position, terminal.transform.position, terminal.transform))
                return false;
            a.trackedTarget = terminal.transform;
            a.allowedTerminalContact = terminal.transform;
            a.goal = terminal.transform.position;
            a.mode = ArmMode.Extending;
            lastAppliedFrame = -1;
            return true;
        }
        public bool IsAtTarget(ManipulatorSide side = ManipulatorSide.Right)
        {
            if (!rigReady) return false;
            Arm a = arms[(int)side];
            if (a.mode != ArmMode.Holding && a.mode != ArmMode.Extending) return false;
            float error = Vector3.Distance(GripWorld(a), a.goal);
            a.gripError = error;
            return error <= precisionContactTolerance;
        }
        // Called by the existing cinematic executor before it updates the NATIVE loose cable end.
        public void FollowCinematicGrip(Vector3 gripWorld)
        {
            if (!rigReady || !IsActive) return;
            Arm a = arms[0];
            if (a.mode == ArmMode.Extending || a.mode == ArmMode.Holding)
            {
                a.trackedTarget = null;
                a.goal = gripWorld;
                a.mode = ArmMode.Extending;
                lastAppliedFrame = -1;
            }
        }
        public void ApplyPoseNow()
        {
            if (!rigReady || lastAppliedFrame == Time.frameCount) return;
            lastAppliedFrame = Time.frameCount;
            float dt = Mathf.Clamp(Time.unscaledDeltaTime, 0f, 0.08f);
            Apply(arms[0], dt, "R");
            Apply(arms[1], dt, "L");
            if (synchronizeAllRigCopies)
            {
                for (int i = 0; i < additionalRightRigs.Count; i++)
                    ApplySiblingRig(additionalRightRigs[i], arms[0], dt, "R");
                for (int i = 0; i < additionalLeftRigs.Count; i++)
                    ApplySiblingRig(additionalLeftRigs[i], arms[1], dt, "L");
            }
            RefreshMechanicalStages();
        }

        private void ApplySiblingRig(Arm sibling, Arm source, float dt, string side)
        {
            if (source.mode == ArmMode.Idle)
            {
                if (sibling.mode != ArmMode.Idle) Retract(sibling, true);
                return;
            }
            sibling.goal = source.goal;
            sibling.trackedTarget = source.trackedTarget;
            sibling.allowedTerminalContact = source.allowedTerminalContact;
            // Use each LOD's actual original wrist/mesh, never spawn a replacement hand.
            if (source.mode == ArmMode.Retracting) sibling.mode = ArmMode.Retracting;
            else if (sibling.mode == ArmMode.Idle || sibling.mode == ArmMode.Retracting)
                sibling.mode = ArmMode.Extending;
            Apply(sibling, dt, side);
        }
        // The native wire system remains authoritative. This controller only
        // drives original wrist and mechanical sleeves after character animation.
        private void LateUpdate() { ApplyPoseNow(); }

        private void Apply(Arm a, float dt, string side)
        {
            if (!a.ready || a.mode == ArmMode.Idle) return;
            if (a.mode == ArmMode.Retracting)
            {
                a.elbow.localPosition = Vector3.MoveTowards(a.elbow.localPosition,
                    a.elbowHomeLocal, retractionSpeed * dt);
                a.wrist.localPosition = Vector3.MoveTowards(a.wrist.localPosition,
                    a.wristHomeLocal, retractionSpeed * dt);
                float rotationStep = Mathf.Clamp01(4f * dt);
                a.shoulder.localRotation = Quaternion.Slerp(a.shoulder.localRotation,
                    a.shoulderHomeRotation, rotationStep);
                a.elbow.localRotation = Quaternion.Slerp(a.elbow.localRotation,
                    a.elbowHomeRotation, rotationStep);
                a.wrist.localRotation = Quaternion.Slerp(a.wrist.localRotation,
                    a.wristHomeRotation, rotationStep);
                if ((a.elbow.localPosition - a.elbowHomeLocal).sqrMagnitude < 0.00001f &&
                    (a.wrist.localPosition - a.wristHomeLocal).sqrMagnitude < 0.00001f)
                {
                    a.elbow.localPosition = a.elbowHomeLocal;
                    a.wrist.localPosition = a.wristHomeLocal;
                    a.shoulder.localRotation = a.shoulderHomeRotation;
                    a.elbow.localRotation = a.elbowHomeRotation;
                    a.wrist.localRotation = a.wristHomeRotation;
                    a.mode = ArmMode.Idle;
                    a.gripError = 0f;
                    RestoreOriginalLimbMeshes(a);
                    return;
                }
                UpdateOriginalLimbMeshes(a);
                return;
            }
            if (a.trackedTarget)
            {
                a.goal = a.trackedTarget.position;
                if (!CanReachFromInternal(a.shoulder.position, a.goal, a.allowedTerminalContact))
                { Retract(a, false); return; }
            }
            if (!SafetyAllowsReach() || (checkArmCollision &&
                !CanReachFromInternal(a.shoulder.position, a.goal, a.allowedTerminalContact)))
            { Retract(a, false); return; }
            // The original hand/finger contact drives the end effector; the wrist
            // is translated by the difference between grip and target. This fixes
            // the v18.2 false success where the wrist was close but fingers were not.
            Vector3 desiredWrist = a.wrist.position + (a.goal - GripWorld(a));
            Vector3 displacement = desiredWrist - a.shoulder.position;
            float distance = displacement.magnitude;
            if (distance < 0.04f || !IsFinite(a.goal) || !IsFinite(desiredWrist))
            { Retract(a, false); return; }
            Vector3 dir = displacement / distance;
            float bend = Mathf.Min(elbowBend, distance * 0.08f);
            Vector3 pole = Vector3.ProjectOnPlane(-transform.up, dir).normalized;
            if (pole.sqrMagnitude < 0.01f)
                pole = Vector3.ProjectOnPlane(transform.right, dir).normalized;
            Vector3 elbowGoal = a.shoulder.position +
                dir * (distance * elbowFraction) + pole * bend;
            float step = Mathf.Max(0f, extensionSpeed * dt);
            Vector3 previousUpper = a.elbow.position - a.shoulder.position;
            Vector3 aimedUpper = elbowGoal - a.shoulder.position;
            if (previousUpper.sqrMagnitude > 0.00001f && aimedUpper.sqrMagnitude > 0.00001f)
                a.shoulder.rotation = Quaternion.FromToRotation(previousUpper, aimedUpper) * a.shoulder.rotation;
            a.elbow.position = Vector3.MoveTowards(a.elbow.position, elbowGoal, step);
            Vector3 previousLower = a.wrist.position - a.elbow.position;
            Vector3 aimedLower = desiredWrist - a.elbow.position;
            if (previousLower.sqrMagnitude > 0.00001f && aimedLower.sqrMagnitude > 0.00001f)
                a.elbow.rotation = Quaternion.FromToRotation(previousLower, aimedLower) * a.elbow.rotation;
            // v18.4: Shoulder and elbow rotation have changed the grip's world offset.
            // Recalculate the wrist goal *after* those joint moves. A pre-rotation
            // goal was responsible for residual gaps such as the observed 0.031m.
            Vector3 postRotationGripOffset = GripWorld(a) - a.wrist.position;
            Vector3 correctedWristGoal = a.goal - postRotationGripOffset;
            if (!IsFinite(correctedWristGoal)) { Retract(a, false); return; }
            a.wrist.position = Vector3.MoveTowards(a.wrist.position, correctedWristGoal, step);
            if (alignWristTowardContact && a.trackedTarget)
            {
                Vector3 handDirection = GripWorld(a) - a.wrist.position;
                Vector3 targetDirection = a.goal - a.wrist.position;
                if (handDirection.sqrMagnitude > 0.00001f && targetDirection.sqrMagnitude > 0.00001f)
                {
                    Quaternion desired = Quaternion.FromToRotation(handDirection, targetDirection) * a.wrist.rotation;
                    a.wrist.rotation = Quaternion.Slerp(a.wrist.rotation, desired,
                        Mathf.Clamp01(handAlignmentSpeed * dt));
                }
            }
            // If optional wrist orientation changed the fingertip offset, finish
            // with a small, speed-limited contact correction using the *real* grip.
            if (alignWristTowardContact && a.trackedTarget)
            {
                Vector3 residual = a.goal - GripWorld(a);
                if (IsFinite(residual))
                    a.wrist.position += Vector3.ClampMagnitude(residual, step * 0.25f);
            }
            a.gripError = Vector3.Distance(GripWorld(a), a.goal);
            // Only claim fingertip contact within the authored precision tolerance.
            a.mode = a.gripError <= precisionContactTolerance
                ? ArmMode.Holding : ArmMode.Extending;
            UpdateOriginalLimbMeshes(a);
        }
        private void Retract(Arm a, bool instant)
        {
            if (!a.ready) return;
            a.trackedTarget = null;
            a.allowedTerminalContact = null;
            if (instant)
            {
                a.elbow.localPosition = a.elbowHomeLocal;
                a.wrist.localPosition = a.wristHomeLocal;
                a.shoulder.localRotation = a.shoulderHomeRotation;
                a.elbow.localRotation = a.elbowHomeRotation;
                a.wrist.localRotation = a.wristHomeRotation;
                a.mode = ArmMode.Idle;
                a.gripError = 0f;
                RestoreOriginalLimbMeshes(a);
            }
            else a.mode = ArmMode.Retracting;
        }
        public void RetractAll(bool instant = false)
        {
            Retract(arms[0], instant);
            Retract(arms[1], instant);
            for (int i = 0; i < additionalRightRigs.Count; i++) Retract(additionalRightRigs[i], instant);
            for (int i = 0; i < additionalLeftRigs.Count; i++) Retract(additionalLeftRigs[i], instant);
        }
        public void RetractRight(bool instant = false)
        {
            Retract(arms[0], instant);
            for (int i = 0; i < additionalRightRigs.Count; i++) Retract(additionalRightRigs[i], instant);
        }
        public void RetractLeft(bool instant = false)
        {
            Retract(arms[1], instant);
            for (int i = 0; i < additionalLeftRigs.Count; i++) Retract(additionalLeftRigs[i], instant);
        }

        // NEVER scale authored imported model geometry (v18.1 produced spear-like meshes).
        // The original wrist, hand and fingers are the only hand geometry. The optional
        // telescopic linkage meshes are purely structural sleeves, not a duplicate hand.
        private static void UpdateOriginalLimbMeshes(Arm a)
        {
            RestoreOriginalLimbMeshes(a);
        }

        private void CreateMechanicalStages()
        {
            if (!Application.isPlaying || mechanismsReady || !showMechanicalStages || !rigReady)
                return;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) shader = Shader.Find("Standard");
            if (!shader)
            {
                Debug.LogWarning("[Spark v18.4] No supported sleeve shader; original-hand reach remains available.", this);
                return;
            }
            materialGraphite = CreateStageMaterial(shader, "Graphite", new Color(0.055f, 0.075f, 0.105f), 0.74f);
            materialTitanium = CreateStageMaterial(shader, "Titanium", new Color(0.31f, 0.37f, 0.43f), 0.85f);
            materialAccent = CreateStageMaterial(shader, "Cyan Joint Rings", new Color(0.025f, 0.49f, 0.57f), 0.62f);
            if (materialAccent.HasProperty("_EmissionColor"))
            {
                materialAccent.EnableKeyword("_EMISSION");
                materialAccent.SetColor("_EmissionColor", new Color(0.0f, 0.13f, 0.17f));
            }
            sharedSleeveMesh = CreateUnitTube(Application.isMobilePlatform ? 8 : 12);
            int count = Application.isMobilePlatform ? Mathf.Min(stagesPerLimb, 3) : stagesPerLimb;
            for (int link = 0; link < 4; link++)
            {
                stages[link] = new MechanicalPart[count];
                collars[link] = new MechanicalPart[count - 1];
                proximalHousings[link] = CreatePart(
                    "Spark AAA Telescopic " + LinkName(link) + " Proximal Housing", materialTitanium);
                distalCouplers[link] = CreatePart(
                    "Spark AAA Telescopic " + LinkName(link) + " Distal Coupler", materialAccent);
                for (int i = 0; i < count; i++)
                {
                    stages[link][i] = CreatePart(
                        "Spark AAA Telescopic " + LinkName(link) + " Stage " + (i + 1),
                        (i & 1) == 0 ? materialGraphite : materialTitanium);
                    if (i < count - 1)
                        collars[link][i] = CreatePart(
                            "Spark AAA Telescopic " + LinkName(link) + " Sliding Collar " + (i + 1),
                            materialAccent);
                }
            }
            mechanismsReady = true;
        }

        private Material CreateStageMaterial(Shader shader, string suffix, Color color, float metallic)
        {
            var result = new Material(shader) { name = "Spark v18.3 " + suffix + " (runtime)" };
            if (result.HasProperty("_BaseColor")) result.SetColor("_BaseColor", color);
            if (result.HasProperty("_Color")) result.SetColor("_Color", color);
            if (result.HasProperty("_Metallic")) result.SetFloat("_Metallic", metallic);
            if (result.HasProperty("_Smoothness")) result.SetFloat("_Smoothness", 0.72f);
            return result;
        }

        private static string LinkName(int link)
        {
            return (link < 2 ? "Right" : "Left") + (link % 2 == 0 ? " Upper" : " Lower");
        }

        // Shared lightweight tubular mesh; no mesh collider, Rigidbody or generated hand.
        // Axis = local Y, total unscaled length=1, radius=0.5.
        private static Mesh CreateUnitTube(int sides)
        {
            int ring = sides + 1;
            int centers = ring * 4;
            var vertices = new Vector3[centers + 2];
            var normals = new Vector3[centers + 2];
            var uv = new Vector2[centers + 2];
            var tris = new int[sides * 12];
            for (int i = 0; i <= sides; i++)
            {
                float u = (float)i / sides;
                float angle = u * Mathf.PI * 2f;
                float x = Mathf.Cos(angle), z = Mathf.Sin(angle);
                vertices[i] = new Vector3(x * 0.5f, -0.5f, z * 0.5f);
                vertices[ring + i] = new Vector3(x * 0.5f, 0.5f, z * 0.5f);
                vertices[2 * ring + i] = vertices[i];
                vertices[3 * ring + i] = vertices[ring + i];
                Vector3 radial = new Vector3(x, 0f, z);
                normals[i] = radial;
                normals[ring + i] = radial;
                normals[2 * ring + i] = Vector3.down;
                normals[3 * ring + i] = Vector3.up;
                uv[i] = new Vector2(u, 0f);
                uv[ring + i] = new Vector2(u, 1f);
                uv[2 * ring + i] = uv[3 * ring + i] = new Vector2(x * 0.5f + 0.5f, z * 0.5f + 0.5f);
            }
            vertices[centers] = Vector3.down * 0.5f;
            vertices[centers + 1] = Vector3.up * 0.5f;
            normals[centers] = Vector3.down;
            normals[centers + 1] = Vector3.up;
            uv[centers] = uv[centers + 1] = Vector2.one * 0.5f;
            int k = 0;
            for (int i = 0; i < sides; i++)
            {
                // Side outward-facing triangles.
                tris[k++] = i; tris[k++] = ring + i; tris[k++] = ring + i + 1;
                tris[k++] = i; tris[k++] = ring + i + 1; tris[k++] = i + 1;
                // Bottom and top caps, using separate flat normals.
                tris[k++] = centers; tris[k++] = 2 * ring + i; tris[k++] = 2 * ring + i + 1;
                tris[k++] = centers + 1; tris[k++] = 3 * ring + i + 1; tris[k++] = 3 * ring + i;
            }
            var mesh = new Mesh { name = "Spark AAA Telescopic Tube (Shared)" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            return mesh;
        }

        private MechanicalPart CreatePart(string name, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            // No physical collider: collision authority remains on the actual gameplay geometry.
            go.layer = gameObject.layer;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = sharedSleeveMesh;
            var display = go.AddComponent<MeshRenderer>();
            display.sharedMaterial = material;
            display.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            display.receiveShadows = false;
            go.SetActive(false);
            return new MechanicalPart { transform = go.transform, renderer = display };
        }

        private void HideMechanicalStages()
        {
            for (int link = 0; link < 4; link++)
            {
                ToggleParts(stages[link], false);
                ToggleParts(collars[link], false);
                TogglePart(proximalHousings[link], false);
                TogglePart(distalCouplers[link], false);
            }
        }

        private static void TogglePart(MechanicalPart part, bool active)
        {
            if (part != null && part.transform)
                part.transform.gameObject.SetActive(active);
        }
        private static void DestroyPart(MechanicalPart part)
        {
            if (part != null && part.transform)
                UnityEngine.Object.Destroy(part.transform.gameObject);
        }
        private static void ToggleParts(MechanicalPart[] parts, bool active)
        {
            if (parts == null) return;
            for (int i = 0; i < parts.Length; i++)
                if (parts[i] != null && parts[i].transform)
                    parts[i].transform.gameObject.SetActive(active);
        }

        private static void DestroyParts(MechanicalPart[] parts)
        {
            if (parts == null) return;
            for (int i = 0; i < parts.Length; i++)
                if (parts[i] != null && parts[i].transform)
                    UnityEngine.Object.Destroy(parts[i].transform.gameObject);
        }

        private void RefreshMechanicalStages()
        {
            if (!showMechanicalStages || !Application.isPlaying || !rigReady)
            {
                HideMechanicalStages();
                return;
            }
            if (!mechanismsReady) CreateMechanicalStages();
            if (!mechanismsReady) return;
            for (int side = 0; side < 2; side++)
            {
                Arm a = arms[side];
                bool active = a.ready && a.mode != ArmMode.Idle;
                int first = side * 2;
                float upperLength = Vector3.Distance(a.shoulder.position, a.elbow.position);
                float lowerLength = Vector3.Distance(a.elbow.position, a.wrist.position);
                UpdateLink(first, active && upperLength > extensionVisualThreshold * a.upperRestWorld,
                    a.shoulder.position, a.elbow.position, a.upperRestWorld);
                UpdateLink(first + 1, active && lowerLength > extensionVisualThreshold * a.lowerRestWorld,
                    a.elbow.position, a.wrist.position, a.lowerRestWorld);
            }
        }


        // v18.4: Overlapping telescopic shells, proximal housing and terminal-side
        // coupler. No duplicated hands or scaled imported limb meshes.
        private void UpdateLink(int link, bool enabled, Vector3 start, Vector3 end, float restLength)
        {
            MechanicalPart[] shaft = stages[link];
            MechanicalPart[] bands = collars[link];
            if (!enabled || !IsFinite(start) || !IsFinite(end) || shaft == null)
            {
                ToggleParts(shaft, false);
                ToggleParts(bands, false);
                TogglePart(proximalHousings[link], false);
                TogglePart(distalCouplers[link], false);
                return;
            }
            Vector3 axis = end - start;
            float length = axis.magnitude;
            if (length < 0.025f)
            {
                ToggleParts(shaft, false);
                ToggleParts(bands, false);
                TogglePart(proximalHousings[link], false);
                TogglePart(distalCouplers[link], false);
                return;
            }
            int count = shaft.Length;
            Vector3 direction = axis / length;
            float reachRatio = Mathf.Max(1f, length / Mathf.Max(0.04f, restLength));
            float thickness = mechanicalThicknessMultiplier *
                Mathf.Clamp(1f + 0.11f * Mathf.Sqrt(reachRatio - 1f), 1f, 1.35f);
            // v18.3 arranged cylinders end-to-end: a thin segmented rod. An
            // overlap fraction of 0.30 produces genuinely nesting shell spans.
            float overlap = Mathf.Clamp(stageOverlap, 0.24f, 0.45f);
            float span = 1f / (count - (count - 1) * overlap);
            float stride = span * (1f - overlap);
            for (int i = 0; i < count; i++)
            {
                float t0 = i * stride;
                float t1 = Mathf.Min(1f, t0 + span);
                float radius = Mathf.Lerp(baseSleeveRadius, tipSleeveRadius,
                    (float)i / Mathf.Max(1, count - 1)) * thickness;
                SetTube(shaft[i], start + axis * ((t0 + t1) * 0.5f), direction,
                    Mathf.Max(0.001f, t1 - t0) * length, radius);
                if (i < count - 1)
                {
                    float collarAt = (i + 1f) * stride;
                    SetTube(bands[i], start + axis * collarAt, direction,
                        Mathf.Min(collarLength, length * stride * 0.35f), radius * 1.15f);
                }
            }
            float housingLength = Mathf.Min(proximalHousingLength, length * 0.22f);
            SetTube(proximalHousings[link], start + direction * housingLength * 0.5f,
                direction, housingLength, baseSleeveRadius * thickness * 1.30f);
            float couplerLength = Mathf.Min(collarLength * 1.6f, length * 0.08f);
            SetTube(distalCouplers[link], end - direction * couplerLength * 0.5f,
                direction, couplerLength, tipSleeveRadius * thickness * 1.14f);
        }


        private static void SetTube(MechanicalPart part, Vector3 midpoint, Vector3 direction, float length, float radius)
        {
            if (part == null || !part.transform || length <= 0.0001f) return;
            Transform t = part.transform;
            t.position = midpoint;
            t.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            // Explicit world-to-parent scale conversion for non-uniform character root scale.
            Vector3 parentScale = t.parent ? t.parent.lossyScale : Vector3.one;
            t.localScale = new Vector3((radius * 2f) / Mathf.Max(0.001f, Mathf.Abs(parentScale.x)),
                length / Mathf.Max(0.001f, Mathf.Abs(parentScale.y)),
                (radius * 2f) / Mathf.Max(0.001f, Mathf.Abs(parentScale.z)));
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
        }

        private static void RestoreOriginalLimbMeshes(Arm a)
        {
            RestoreArray(a.upperMeshes);
            RestoreArray(a.lowerMeshes);
        }
        private static void RestoreArray(LimbMesh[] meshes)
        {
            if (meshes == null) return;
            for (int i = 0; i < meshes.Length; i++)
                if (meshes[i].transform) meshes[i].transform.localScale = meshes[i].homeScale;
        }

        private void RemoveOldV18ProxyObjects()
        {
            // Cleanup if old v18 runtime barrels survived a hot script reload.
            Transform[] descendants = GetComponentsInChildren<Transform>(true);
            for (int i = descendants.Length - 1; i >= 0; i--)
            {
                Transform item = descendants[i];
                if (item == transform ||
                    !(item.name.StartsWith("Spark Telescopic ", StringComparison.Ordinal) ||
                      item.name.StartsWith("Spark v18.2 Joint Bridge ", StringComparison.Ordinal))) continue;
                // Old developer-generated primitives only. Never remove rig pivots/hands.
                if (!item.GetComponent<MeshFilter>()) continue;
                if (Application.isPlaying) Destroy(item.gameObject);
            }
        }
        [ContextMenu("TEST v18.4 / Extend right arm to Level 20 terminal")]
        private void TestExtension() { Debug.Log("[Spark v18] " + (TryReachTerminal(debugTerminalId) ? "Extending to " + debugTerminalId : LastError), this); }
        [ContextMenu("TEST v18.4 / Retract both arms")]
        private void TestRetract() { RetractAll(); }
        private void OnGUI()
        {
            if (!allowEditorDebugPanel || !Application.isEditor || !Application.isPlaying) return;
            GUILayout.BeginArea(new Rect(15, Screen.height - 140, 300, 125), "SPARK AAA ORIGINAL HANDS v18.4", GUI.skin.window);
            GUILayout.Label(Status);
            debugTerminalId = GUILayout.TextField(debugTerminalId);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reach terminal")) TryReachTerminal(debugTerminalId);
            if (GUILayout.Button("Retract")) RetractAll();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
