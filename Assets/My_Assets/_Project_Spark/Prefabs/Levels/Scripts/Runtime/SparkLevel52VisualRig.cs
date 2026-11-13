using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// LEVEL 52 ART ONLY: displays externally supplied measured shaft RPM,
    /// encoder RPM, correction-mass angles, guard state and fault state.
    /// Never changes SparkElectricalComponent values, shaft physics,
    /// measured vibration, circuit topology or authoritative level targets.
    ///
    /// FBX groups are named in Animation_Rigs.csv. Correction weight A/B meshes
    /// are authored at 45 and 225 degrees in the YZ plane. Both group pivots are
    /// at the central ring axis and are intentionally independent siblings.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel52VisualRig : MonoBehaviour
    {
        [Header("Motor, shaft, rotor and encoder FBX groups")]
        [SerializeField] private Transform motorShaftRotateX;
        [SerializeField] private Transform globalDrivetrainShaftRotateX;
        [SerializeField] private Transform testRotorRotateX;
        [SerializeField] private Transform balanceRingRotateX;
        [SerializeField] private Transform encoderAbzDiscRotateX;

        [Header("Independent orbit groups on the active ring")]
        [SerializeField] private Transform correctionMassAOrbitX;
        [SerializeField] private Transform correctionMassBOrbitX;
        [Tooltip("The static authored mass-A position is at 45 degrees from local +Y toward +Z.")]
        [SerializeField] private float authoredAngleA = 45f;
        [Tooltip("The static authored mass-B position is at 225 degrees.")]
        [SerializeField] private float authoredAngleB = 225f;

        [Header("Guard, weight, indicators")]
        [SerializeField] private Transform frontSafetyDoorHingeY;
        [SerializeField] private GameObject installedImbalanceTestWeight;
        [SerializeField] private Renderer readyIndicator;
        [SerializeField] private Renderer faultIndicator;
        [SerializeField] private float guardOpenAngle = 85f;
        [SerializeField, Min(0.01f)] private float guardResponse = 6f;
        [SerializeField] private Color readyColor = new Color(0.10f, 0.96f, 0.65f);
        [SerializeField] private Color warningColor = new Color(1f, 0.72f, 0.24f);
        [SerializeField] private Color faultColor = new Color(1f, 0.20f, 0.26f);

        private Quaternion motorRest, shaftRest, rotorRest, ringRest, encoderRest;
        private Quaternion massARest, massBRest, guardRest;
        private float rotorRpm, encoderRpm, shaftDegrees;
        private float correctionA = 45f, correctionB = 225f;
        private bool guardOpen, warning, fault;
        private MaterialPropertyBlock materialBlock;

        private void Awake()
        {
            motorRest = Rest(motorShaftRotateX);
            shaftRest = Rest(globalDrivetrainShaftRotateX);
            rotorRest = Rest(testRotorRotateX);
            ringRest = Rest(balanceRingRotateX);
            encoderRest = Rest(encoderAbzDiscRotateX);
            massARest = Rest(correctionMassAOrbitX);
            massBRest = Rest(correctionMassBOrbitX);
            guardRest = Rest(frontSafetyDoorHingeY);
            materialBlock = new MaterialPropertyBlock();
            RefreshIndicators();
        }

        private static Quaternion Rest(Transform item)
        {
            return item != null ? item.localRotation : Quaternion.identity;
        }

        private void Update()
        {
            float dt = Mathf.Max(0f, Time.deltaTime);
            shaftDegrees = Mathf.Repeat(shaftDegrees + rotorRpm * 6f * dt, 360f);
            float encoderDegrees = encoderRpm * 6f * dt;

            // Purely visual rotations, not gameplay calculations.
            SetShaft(motorShaftRotateX, motorRest, shaftDegrees);
            SetShaft(globalDrivetrainShaftRotateX, shaftRest, shaftDegrees);
            SetShaft(testRotorRotateX, rotorRest, shaftDegrees);
            SetShaft(balanceRingRotateX, ringRest, shaftDegrees);

            if (encoderAbzDiscRotateX != null)
                encoderAbzDiscRotateX.Rotate(Vector3.right, encoderDegrees, Space.Self);

            // The installed mass pivots are children of the rotating ring.
            SetShaft(correctionMassAOrbitX, massARest, correctionA - authoredAngleA);
            SetShaft(correctionMassBOrbitX, massBRest, correctionB - authoredAngleB);

            if (frontSafetyDoorHingeY != null)
            {
                Quaternion goal = guardRest * Quaternion.Euler(
                    0f, guardOpen ? guardOpenAngle : 0f, 0f);
                frontSafetyDoorHingeY.localRotation = Quaternion.Slerp(
                    frontSafetyDoorHingeY.localRotation, goal,
                    Mathf.Clamp01(guardResponse * dt));
            }
        }

        private static void SetShaft(Transform target, Quaternion rest, float angle)
        {
            if (target != null)
                target.localRotation = rest * Quaternion.AngleAxis(angle, Vector3.right);
        }

        /// <summary>Supply ACTUAL game-measured rotor and encoder RPM values.</summary>
        public void SetMeasuredRPM(float measuredRotorRpm, float measuredEncoderRpm)
        {
            rotorRpm = Finite(measuredRotorRpm) ? measuredRotorRpm : 0f;
            encoderRpm = Finite(measuredEncoderRpm) ? measuredEncoderRpm : 0f;
        }

        /// <summary>
        /// Supply positions ACTUALLY measured or commanded by your balance
        /// actuator state; this method does not calculate an ideal correction.
        /// </summary>
        public void SetMeasuredCorrectionAngles(float aDegrees, float bDegrees)
        {
            if (Finite(aDegrees)) correctionA = Mathf.Repeat(aDegrees, 360f);
            if (Finite(bDegrees)) correctionB = Mathf.Repeat(bDegrees, 360f);
        }

        public void SetSafetyGuardOpen(bool open) => guardOpen = open;

        public void SetImbalanceWeightInstalled(bool installed)
        {
            if (installedImbalanceTestWeight != null)
                installedImbalanceTestWeight.SetActive(installed);
        }

        public void SetCondition(bool hasWarning, bool hasFault)
        {
            warning = hasWarning;
            fault = hasFault;
            RefreshIndicators();
        }

        private void RefreshIndicators()
        {
            if (materialBlock == null) materialBlock = new MaterialPropertyBlock();
            SetColor(readyIndicator, fault ? faultColor : warning ? warningColor : readyColor);
            SetColor(faultIndicator, fault ? faultColor : Color.black);
        }

        private void SetColor(Renderer r, Color value)
        {
            if (r == null) return;
            r.GetPropertyBlock(materialBlock);
            Material mat = r.sharedMaterial;
            if (mat != null)
            {
                if (mat.HasProperty("_BaseColor")) materialBlock.SetColor("_BaseColor", value);
                if (mat.HasProperty("_Color")) materialBlock.SetColor("_Color", value);
                if (mat.HasProperty("_EmissionColor")) materialBlock.SetColor("_EmissionColor", value);
            }
            r.SetPropertyBlock(materialBlock);
            materialBlock.Clear();
        }

        private static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
    }
}
