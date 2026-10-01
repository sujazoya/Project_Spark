
using System;
using UnityEngine;
using ProjectSpark.Gameplay;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Unified visual controller for:
    ///
    ///     SparkSwitch
    ///     SparkSwitchIndex
    ///
    /// Supports:
    /// - Binary switch positions
    /// - Indexed switch positions
    /// - Animated lever movement
    /// - Rotation / position movement
    /// - Electrical indicator material
    /// - Indicator intensity
    /// - Audio feedback
    ///
    /// Electrical state is NEVER controlled here.
    /// This component only visualizes the electrical component state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSwitchVisual : MonoBehaviour
    {
        // ============================================================
        // SWITCH REFERENCES
        // ============================================================

        [Header("Switch Source")]

        [Tooltip("Assign for normal binary SparkSwitch.")]
        [SerializeField]
        private SparkSwitch sparkSwitch;

        [Tooltip("Assign for indexed SparkSwitchIndex.")]
        [SerializeField]
        private SparkSwitchIndex sparkSwitchIndex;


        // ============================================================
        // MECHANICAL SWITCH
        // ============================================================

        [Serializable]
        public struct SwitchPosition
        {
            public string name;

            public Vector3 localEulerAngles;

            public Vector3 localPosition;
        }

        [Header("Mechanical Positions")]

        [SerializeField]
        private Transform switchTransform;

        [SerializeField]
        private SwitchPosition[] leverPositions =
        {
            new SwitchPosition
            {
                name = "OFF / OPEN",
                localEulerAngles = Vector3.zero,
                localPosition = Vector3.zero
            },

            new SwitchPosition
            {
                name = "ON / CLOSED",
                localEulerAngles = new Vector3(0f, 25f, 0f),
                localPosition = Vector3.zero
            },

            new SwitchPosition
            {
                name = "HIGH",
                localEulerAngles = new Vector3(0f, 50f, 0f),
                localPosition = Vector3.zero
            }
        };

        [Header("Movement")]

        [SerializeField]
        private float rotationSpeed = 12f;

        [SerializeField]
        private float positionSpeed = 12f;

        [SerializeField]
        private bool usePositionMovement = false;

        [SerializeField]
        private bool useRotationMovement = true;


        // ============================================================
        // INDICATOR
        // ============================================================

        [Header("Indicator")]

        [SerializeField]
        private Renderer indicatorRenderer;

        [SerializeField]
        private string colorProperty = "_BaseColor";

        [SerializeField]
        private string intensityProperty = "_Intensity";

        [Header("Indicator Colors")]

        [SerializeField]
        private Color noPowerColor = Color.gray;

        [SerializeField]
        private Color poweredOpenColor = Color.red;

        [SerializeField]
        private Color activeColor = Color.green;

        [Header("Indicator Intensity")]

        [SerializeField]
        private float maxIntensity = 1f;

        public enum IndicatorIntensityMode
        {
            Normal,
            IndexBased
        }

        [SerializeField]
        private IndicatorIntensityMode indicatorIntensityMode =
            IndicatorIntensityMode.Normal;

        [SerializeField]
        private bool useTargetPositionForIndicator = true;


        // ============================================================
        // AUDIO
        // ============================================================

        [Header("Audio")]

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip switchSound;

        [SerializeField]
        private bool playSoundOnPositionChange = true;


        // ============================================================
        // RUNTIME
        // ============================================================

        private Material indicatorMaterial;

        private int currentPosition;

        private int targetPosition;

        private Quaternion targetRotation;

        private Vector3 targetLocalPosition;

        private bool isMoving;

        private int lastIndicatorState = -1;

        private int lastVisualPosition = -1;

        private bool initialized;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Subscribe();
            SyncFromSwitch();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            if (indicatorMaterial != null)
            {
                Destroy(indicatorMaterial);
                indicatorMaterial = null;
            }
        }

        private void Update()
        {
            UpdateMechanicalMovement();
        }


        // ============================================================
        // INITIALIZE
        // ============================================================

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            if (switchTransform == null)
                switchTransform = transform;

            if (indicatorRenderer != null)
            {
                indicatorMaterial = indicatorRenderer.material;
            }

            if (leverPositions == null || leverPositions.Length == 0)
            {
                leverPositions = new SwitchPosition[]
                {
                    new SwitchPosition
                    {
                        name = "OFF",
                        localEulerAngles = Vector3.zero,
                        localPosition = Vector3.zero
                    },

                    new SwitchPosition
                    {
                        name = "ON",
                        localEulerAngles = new Vector3(0f, 25f, 0f),
                        localPosition = Vector3.zero
                    }
                };
            }
        }


        // ============================================================
        // SUBSCRIBE
        // ============================================================

        private void Subscribe()
        {
            if (sparkSwitch != null)
            {
                sparkSwitch.VisualStateChanged +=
                    HandleSparkSwitchVisualStateChanged;
            }

            if (sparkSwitchIndex != null)
            {
                sparkSwitchIndex.VisualStateChanged +=
                    HandleSparkSwitchIndexVisualStateChanged;
            }
        }

        private void Unsubscribe()
        {
            if (sparkSwitch != null)
            {
                sparkSwitch.VisualStateChanged -=
                    HandleSparkSwitchVisualStateChanged;
            }

            if (sparkSwitchIndex != null)
            {
                sparkSwitchIndex.VisualStateChanged -=
                    HandleSparkSwitchIndexVisualStateChanged;
            }
        }


        // ============================================================
        // EVENTS
        // ============================================================

        private void HandleSparkSwitchVisualStateChanged()
        {
            SyncFromSwitch();
        }

        private void HandleSparkSwitchIndexVisualStateChanged()
        {
            SyncFromSwitch();
        }


        // ============================================================
        // MAIN SYNC
        // ============================================================

        private void SyncFromSwitch()
{
    if (!initialized)
        Initialize();

    int visualPosition = GetVisualPosition();

    if (visualPosition != currentPosition ||
        visualPosition != targetPosition)
    {
        SetPosition(visualPosition);
    }

    UpdateIndicator();

    lastVisualPosition = visualPosition;
}


        // ============================================================
        // GET VISUAL POSITION
        // ============================================================

        private int GetVisualPosition()
{
    // ------------------------------------------------------------
    // INDEXED SWITCH
    // ------------------------------------------------------------

    if (sparkSwitchIndex != null)
    {
        int index =
            sparkSwitchIndex.Index;

        int maximumPosition =
            Mathf.Min(
                sparkSwitchIndex.PositionCount - 1,
                leverPositions.Length - 1);

        return Mathf.Clamp(
            index,
            0,
            Mathf.Max(0, maximumPosition));
    }

    // ------------------------------------------------------------
    // NORMAL BINARY SWITCH
    // ------------------------------------------------------------

    if (sparkSwitch != null)
    {
        return sparkSwitch.IsClosed ? 1 : 0;
    }

    return 0;
}
        // ============================================================
        // SET POSITION
        // ============================================================

        public void SetPosition(int position)
        {
            if (leverPositions == null ||
                leverPositions.Length == 0)
                return;

            position = Mathf.Clamp(
                position,
                0,
                leverPositions.Length - 1);

            targetPosition = position;

            SwitchPosition data = leverPositions[position];

            targetRotation =
                Quaternion.Euler(data.localEulerAngles);

            targetLocalPosition =
                data.localPosition;

            isMoving = true;

            if (position != currentPosition)
            {
                currentPosition = position;

                if (playSoundOnPositionChange)
                    PlaySwitchSound();
            }

            UpdateIndicator();
        }


        // ============================================================
        // IMMEDIATE POSITION
        // ============================================================

        public void SetPositionImmediate(int position)
        {
            if (leverPositions == null ||
                leverPositions.Length == 0)
                return;

            position = Mathf.Clamp(
                position,
                0,
                leverPositions.Length - 1);

            currentPosition = position;
            targetPosition = position;

            SwitchPosition data = leverPositions[position];

            targetRotation =
                Quaternion.Euler(data.localEulerAngles);

            targetLocalPosition =
                data.localPosition;

            if (switchTransform != null)
            {
                if (useRotationMovement)
                    switchTransform.localRotation = targetRotation;

                if (usePositionMovement)
                    switchTransform.localPosition =
                        targetLocalPosition;
            }

            isMoving = false;

            UpdateIndicator();
        }


        // ============================================================
        // MECHANICAL ANIMATION
        // ============================================================

        private void UpdateMechanicalMovement()
        {
            if (!isMoving || switchTransform == null)
                return;

            bool finished = true;

            if (useRotationMovement)
            {
                switchTransform.localRotation =
                    Quaternion.Slerp(
                        switchTransform.localRotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);

                if (Quaternion.Angle(
                        switchTransform.localRotation,
                        targetRotation) > 0.05f)
                {
                    finished = false;
                }
                else
                {
                    switchTransform.localRotation =
                        targetRotation;
                }
            }

            if (usePositionMovement)
            {
                switchTransform.localPosition =
                    Vector3.Lerp(
                        switchTransform.localPosition,
                        targetLocalPosition,
                        positionSpeed * Time.deltaTime);

                if (Vector3.Distance(
                        switchTransform.localPosition,
                        targetLocalPosition) > 0.001f)
                {
                    finished = false;
                }
                else
                {
                    switchTransform.localPosition =
                        targetLocalPosition;
                }
            }

            if (finished)
                isMoving = false;
        }


        // ============================================================
        // INDICATOR
        // ============================================================

        private void UpdateIndicator()
        {
            if (indicatorMaterial == null)
                return;

            int state = GetIndicatorState();

            if (state == lastIndicatorState)
                return;

            lastIndicatorState = state;

            Color color;

            switch (state)
            {
                case 2:
                    // GREEN
                    color = activeColor;
                    break;

                case 1:
                    // RED
                    color = poweredOpenColor;
                    break;

                default:
                    // GREY
                    color = noPowerColor;
                    break;
            }

            if (indicatorMaterial.HasProperty(colorProperty))
            {
                indicatorMaterial.SetColor(
                    colorProperty,
                    color);
            }

            if (indicatorMaterial.HasProperty(intensityProperty))
            {
                float intensity = GetIndicatorIntensity();

                indicatorMaterial.SetFloat(
                    intensityProperty,
                    intensity);
            }
        }


        // ============================================================
        // INDICATOR STATE
        // ============================================================

        private int GetIndicatorState()
        {
            // --------------------------------------------------------
            // INDEXED SWITCH
            // --------------------------------------------------------

            if (sparkSwitchIndex != null)
            {
                if (sparkSwitchIndex.IsOff)
                    return 0;

                if (sparkSwitchIndex.HasVoltage ||
                    sparkSwitchIndex.HasCurrent)
                    return 2;

                return 1;
            }

            // --------------------------------------------------------
            // NORMAL SWITCH
            // --------------------------------------------------------

            if (sparkSwitch != null)
            {
                if (sparkSwitch.IsClosed)
                    return 2;

                if (sparkSwitch.HasVoltage ||
                    sparkSwitch.HasCurrent)
                    return 1;

                return 0;
            }

            return 0;
        }


        // ============================================================
        // INDICATOR INTENSITY
        // ============================================================

        private float GetIndicatorIntensity()
        {
            if (indicatorIntensityMode ==
                IndicatorIntensityMode.IndexBased)
            {
                if (sparkSwitchIndex != null)
                {
                    int count =
                        Mathf.Max(
                            1,
                            sparkSwitchIndex.PositionCount);

                    if (count <= 1)
                        return maxIntensity;

                    float normalized =
                        (float)sparkSwitchIndex.Index /
                        (count - 1);

                    return Mathf.Lerp(
                        0f,
                        maxIntensity,
                        normalized);
                }
            }

            return maxIntensity;
        }


        // ============================================================
        // AUDIO
        // ============================================================

        private void PlaySwitchSound()
        {
            if (audioSource == null ||
                switchSound == null)
                return;

            audioSource.PlayOneShot(switchSound);
        }


        // ============================================================
        // PUBLIC SYNC
        // ============================================================

        public void RefreshVisual()
        {
            SyncFromSwitch();
        }


        // ============================================================
        // CONTEXT TEST
        // ============================================================

        [ContextMenu("Refresh Visual")]
        private void ContextRefreshVisual()
        {
            SyncFromSwitch();
        }

        [ContextMenu("Set Visual Position 0")]
        private void ContextPosition0()
        {
            SetPosition(0);
        }

        [ContextMenu("Set Visual Position 1")]
        private void ContextPosition1()
        {
            SetPosition(1);
        }

        [ContextMenu("Set Visual Position 2")]
        private void ContextPosition2()
        {
            SetPosition(2);
        }
    }
}
