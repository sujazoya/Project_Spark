
using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Universal physical switch interaction and visual controller.
    ///
    /// Supports:
    /// - SparkSwitch: binary Open / Closed.
    /// - SparkSwitchIndex: indexed positions.
    /// - Animated local rotation and position.
    /// - Optional indicator lighting and audio.
    ///
    /// Electrical simulation remains owned by the electrical components.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SwitchLeverController : MonoBehaviour
    {
        #region Mechanical Positions

        [Serializable]
        public struct LeverPosition
        {
            public string name;
            public Vector3 localEulerAngles;
            public Vector3 localPosition;
        }

        #endregion

        #region Electrical Source

        [Header("Electrical Source")]

        [Tooltip("Assign for a standard ON/OFF switch.")]
        [SerializeField]
        private SparkSwitch sparkSwitch;

        [Tooltip("Assign for a multi-position switch. Takes priority if both are assigned.")]
        [SerializeField]
        private SparkSwitchIndex sparkSwitchIndex;

        #endregion

        #region Interaction

        [Header("Interaction")]

        [SerializeField]
        private bool toggleOnClick = true;

        [SerializeField]
        private bool cycleIndexOnClick = true;

        [SerializeField]
        private bool allowDirectStateSelection = true;

        #endregion

        #region Lever

        [Header("Lever")]

        [Tooltip("The pivot Transform that moves. Assign the actual lever pivot.")]
        [SerializeField]
        private Transform leverTransform;

        [SerializeField]
        private LeverPosition[] positions =
        {
            new LeverPosition
            {
                name = "Position 0",
                localEulerAngles = Vector3.zero,
                localPosition = Vector3.zero
            },
            new LeverPosition
            {
                name = "Position 1",
                localEulerAngles = new Vector3(0f, 25f, 0f),
                localPosition = Vector3.zero
            },
            new LeverPosition
            {
                name = "Position 2",
                localEulerAngles = new Vector3(0f, 50f, 0f),
                localPosition = Vector3.zero
            }
        };

        [SerializeField]
        private bool animateRotation = true;

        [SerializeField]
        private bool animatePosition;

        [Min(0.01f)]
        [SerializeField]
        private float rotationSpeed = 12f;

        [Min(0.01f)]
        [SerializeField]
        private float positionSpeed = 12f;

        #endregion

        #region Indicator

        [Header("Optional Indicator")]

        [SerializeField]
        private Renderer indicatorRenderer;

        [Tooltip("Shader color property, commonly _BaseColor or _Color.")]
        [SerializeField]
        private string colorProperty = "_BaseColor";

        [Tooltip("Optional emission color property, commonly _EmissionColor.")]
        [SerializeField]
        private string emissionColorProperty = "_EmissionColor";

        [Tooltip("Optional custom shader intensity property.")]
        [SerializeField]
        private string intensityProperty = "_Intensity";

        [SerializeField]
        private Color inactiveColor = Color.gray;

        [SerializeField]
        private Color poweredOpenColor = Color.red;

        [SerializeField]
        private Color activeColor = Color.green;

        [Min(0f)]
        [SerializeField]
        private float maxIntensity = 1f;

        [SerializeField]
        private bool indexControlsIntensity;

        #endregion

        #region Audio

        [Header("Optional Audio")]

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip switchSound;

        [SerializeField]
        private bool playSoundOnPositionChange = true;

        #endregion

        #region Runtime

        [Header("Runtime State (Read Only)")]

        [SerializeField]
        private int currentIndex;

        [SerializeField]
        private bool isOn;

        private MaterialPropertyBlock propertyBlock;

        private int lastIndicatorState = -1;
        private int lastIndicatorIndex = -1;

        private int targetIndex;

        private Quaternion targetRotation;
        private Vector3 targetLocalPosition;

        private bool isMoving;
        private bool initialized;
        private bool subscribed;

        #endregion

        #region Public API

        public bool IsOn => isOn;

        public int Index => currentIndex;

        public int PositionCount => GetPositionCount();

        public SparkSwitch Switch => sparkSwitch;

        public SparkSwitchIndex IndexedSwitch => sparkSwitchIndex;

        public Transform LeverTransform => leverTransform;

        public event Action<int> PositionChanged;

        #endregion

        #region Unity Lifecycle

       private void Awake()
{
    Initialize();
    SyncFromElectricalComponent(true);
}

private void OnEnable()
{
    Initialize();
    Subscribe();

    // Restore the current state without animation or sound.
    SyncFromElectricalComponent(true);
}

private void OnDisable()
{
    Unsubscribe();
}        private void Update()
        {
            UpdateLeverMovement();
        }

        private void OnValidate()
        {
            rotationSpeed = Mathf.Max(0.01f, rotationSpeed);
            positionSpeed = Mathf.Max(0.01f, positionSpeed);
            maxIntensity = Mathf.Max(0f, maxIntensity);
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            if (sparkSwitchIndex == null)
                sparkSwitchIndex = GetComponent<SparkSwitchIndex>();

            if (sparkSwitch == null && sparkSwitchIndex == null)
                sparkSwitch = GetComponent<SparkSwitch>();

            if (leverTransform == null)
                leverTransform = transform;

            if (positions == null || positions.Length == 0)
            {
                positions = new[]
                {
                    new LeverPosition
                    {
                        name = "OFF",
                        localEulerAngles = Vector3.zero,
                        localPosition = Vector3.zero
                    },
                    new LeverPosition
                    {
                        name = "ON",
                        localEulerAngles = new Vector3(0f, 25f, 0f),
                        localPosition = Vector3.zero
                    }
                };
            }

            propertyBlock = new MaterialPropertyBlock();
        }

        #endregion

        #region Events

        private void Subscribe()
        {
            if (subscribed)
                return;

            if (sparkSwitch != null)
                sparkSwitch.VisualStateChanged += HandleElectricalVisualChanged;

            if (sparkSwitchIndex != null)
                sparkSwitchIndex.VisualStateChanged += HandleElectricalVisualChanged;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;

            if (sparkSwitch != null)
                sparkSwitch.VisualStateChanged -= HandleElectricalVisualChanged;

            if (sparkSwitchIndex != null)
                sparkSwitchIndex.VisualStateChanged -= HandleElectricalVisualChanged;

            subscribed = false;
        }

        private void HandleElectricalVisualChanged()
        {
            SyncFromElectricalComponent(false);
        }

        #endregion

        #region Interaction API

        /// <summary>
        /// Entry point for the Project Spark interaction system.
        /// </summary>
        public void Activate()
        {
            Initialize();

            if (!HasElectricalSource())
                return;

            if (sparkSwitchIndex != null)
            {
                if (cycleIndexOnClick)
                    NextIndex();
                else
                    ApplyCurrentIndex();

                return;
            }

            if (toggleOnClick)
                Toggle();
            else
                ApplyCurrentState();
        }

        public void Toggle()
        {
            if (sparkSwitchIndex != null)
            {
                NextIndex();
                return;
            }

            if (sparkSwitch == null)
                return;

            sparkSwitch.ToggleState();
            SyncFromElectricalComponent(false);
        }

        public void TurnOn()
        {
            SetState(true);
        }

        public void TurnOff()
        {
            if (sparkSwitchIndex != null)
            {
                sparkSwitchIndex.TurnOff();
                SyncFromElectricalComponent(false);
                return;
            }

            SetState(false);
        }

        public void SetState(bool on)
        {
            if (!allowDirectStateSelection || sparkSwitch == null)
                return;

            sparkSwitch.SetState(
                on ? SparkSwitchState.Closed : SparkSwitchState.Open);

            SyncFromElectricalComponent(false);
        }

        public void NextIndex()
        {
            if (sparkSwitchIndex == null)
                return;

            sparkSwitchIndex.NextIndex();
            SyncFromElectricalComponent(false);
        }

        public void PreviousIndex()
        {
            if (sparkSwitchIndex == null)
                return;

            sparkSwitchIndex.PreviousIndex();
            SyncFromElectricalComponent(false);
        }

        public void SetIndex(int index)
        {
            if (!allowDirectStateSelection || sparkSwitchIndex == null)
                return;

            sparkSwitchIndex.SetIndex(index);
            SyncFromElectricalComponent(false);
        }

        private void ApplyCurrentState()
        {
            SetState(isOn);
        }

        private void ApplyCurrentIndex()
        {
            SetIndex(currentIndex);
        }

        #endregion

        #region State Synchronization

       public void SyncFromElectricalComponent(bool immediate = false)
{
    Initialize();

    int actualIndex = ReadElectricalIndex();
    bool changed = actualIndex != currentIndex;

    currentIndex = actualIndex;

    isOn = sparkSwitchIndex != null
        ? !sparkSwitchIndex.IsOff
        : sparkSwitch != null && sparkSwitch.IsClosed;

    if (positions != null && positions.Length > 0)
    {
        int visualIndex = Mathf.Clamp(
            actualIndex, 0, positions.Length - 1);

        if (immediate)
            SetPositionImmediate(visualIndex);
        else
            SetPosition(visualIndex);
    }

    UpdateIndicator();

    if (changed)
        PositionChanged?.Invoke(currentIndex);
}

        private int ReadElectricalIndex()
        {
            if (sparkSwitchIndex != null)
                return Mathf.Max(0, sparkSwitchIndex.Index);

            if (sparkSwitch != null)
                return sparkSwitch.IsClosed ? 1 : 0;

            return 0;
        }

        private bool HasElectricalSource()
        {
            return sparkSwitchIndex != null || sparkSwitch != null;
        }

        private int GetPositionCount()
        {
            if (sparkSwitchIndex != null)
                return Mathf.Max(1, sparkSwitchIndex.PositionCount);

            return 2;
        }

        #endregion

        #region Lever Movement
public void SetPosition(int index)
{
    if (positions == null || positions.Length == 0)
        return;

    index = Mathf.Clamp(index, 0, positions.Length - 1);

    bool changed = index != targetIndex;
    targetIndex = index;

    LeverPosition position = positions[index];

    targetRotation = Quaternion.Euler(position.localEulerAngles);
    targetLocalPosition = position.localPosition;

    isMoving = leverTransform != null;

    // Play only when the target position genuinely changes.
    if (changed && playSoundOnPositionChange)
        PlaySwitchSound();

    UpdateIndicator();
}

        public void SetPositionImmediate(int index)
        {
            if (positions == null || positions.Length == 0)
                return;

            index = Mathf.Clamp(index, 0, positions.Length - 1);

            targetIndex = index;

            LeverPosition position = positions[index];

            targetRotation = Quaternion.Euler(position.localEulerAngles);
            targetLocalPosition = position.localPosition;

            if (leverTransform != null)
            {
                if (animateRotation)
                    leverTransform.localRotation = targetRotation;

                if (animatePosition)
                    leverTransform.localPosition = targetLocalPosition;
            }

            isMoving = false;
            UpdateIndicator();
        }

        private void UpdateLeverMovement()
        {
            if (!isMoving || leverTransform == null)
                return;

            bool rotationFinished = !animateRotation;
            bool positionFinished = !animatePosition;

            if (animateRotation)
            {
                leverTransform.localRotation = Quaternion.Slerp(
                    leverTransform.localRotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);

                rotationFinished =
                    Quaternion.Angle(
                        leverTransform.localRotation,
                        targetRotation) < 0.05f;

                if (rotationFinished)
                    leverTransform.localRotation = targetRotation;
            }

            if (animatePosition)
            {
                leverTransform.localPosition = Vector3.Lerp(
                    leverTransform.localPosition,
                    targetLocalPosition,
                    positionSpeed * Time.deltaTime);

                positionFinished =
                    Vector3.Distance(
                        leverTransform.localPosition,
                        targetLocalPosition) < 0.001f;

                if (positionFinished)
                    leverTransform.localPosition = targetLocalPosition;
            }

            isMoving = !(rotationFinished && positionFinished);
        }

        #endregion

        #region Indicator

        private int GetIndicatorState()
        {
            if (sparkSwitchIndex != null)
            {
                if (sparkSwitchIndex.IsOff)
                    return 0;

                if (sparkSwitchIndex.HasVoltage ||
                    sparkSwitchIndex.HasCurrent)
                    return 2;

                return 1;
            }

            if (sparkSwitch != null)
            {
                if (sparkSwitch.IsClosed)
                    return 2;

                if (sparkSwitch.HasVoltage ||
                    sparkSwitch.HasCurrent)
                    return 1;
            }

            return 0;
        }

        private void UpdateIndicator()
        {
            if (indicatorRenderer == null)
                return;

            int state = GetIndicatorState();

            if (state == lastIndicatorState &&
                currentIndex == lastIndicatorIndex)
                return;

            lastIndicatorState = state;
            lastIndicatorIndex = currentIndex;

            Color color = state switch
            {
                2 => activeColor,
                1 => poweredOpenColor,
                _ => inactiveColor
            };

            float intensity = maxIntensity;

            if (indexControlsIntensity && sparkSwitchIndex != null)
            {
                int count = Mathf.Max(1, sparkSwitchIndex.PositionCount);

                intensity = count <= 1
                    ? maxIntensity
                    : maxIntensity * Mathf.Clamp01(
                        (float)sparkSwitchIndex.Index / (count - 1));
            }

            indicatorRenderer.GetPropertyBlock(propertyBlock);

            Material sharedMaterial = indicatorRenderer.sharedMaterial;

            if (sharedMaterial != null &&
                sharedMaterial.HasProperty(colorProperty))
            {
                propertyBlock.SetColor(colorProperty, color);
            }

            if (sharedMaterial != null &&
                sharedMaterial.HasProperty(emissionColorProperty))
            {
                propertyBlock.SetColor(
                    emissionColorProperty, color * intensity);
            }

            if (sharedMaterial != null &&
                sharedMaterial.HasProperty(intensityProperty))
            {
                propertyBlock.SetFloat(intensityProperty, intensity);
            }

            indicatorRenderer.SetPropertyBlock(propertyBlock);
        }

        #endregion
       

       #region Audio

        [Range(0f, 1f)]
        [SerializeField]
        private float switchVolume = 1f;

        private void PlaySwitchSound()
        {
            if (audioSource == null)
            {
                Debug.LogWarning(
                    "SwitchLeverController: AudioSource is not assigned.",
                    this);
                return;
            }

            if (switchSound == null)
            {
                Debug.LogWarning(
                    "SwitchLeverController: Switch sound clip is not assigned.",
                    this);
                return;
            }

            audioSource.PlayOneShot(switchSound, switchVolume);
        }

        #endregion
       

        #region Helpers

        public bool IsSwitchClosed()
        {
            return sparkSwitch != null && sparkSwitch.IsClosed;
        }

        public bool IsIndexed()
        {
            return sparkSwitchIndex != null;
        }

        public int GetIndex()
        {
            return currentIndex;
        }

        public void RefreshVisual()
        {
            SyncFromElectricalComponent(false);
        }

        #endregion

        #region Context Tests

        [ContextMenu("Switch / Activate")]
        private void TestActivate() => Activate();

        [ContextMenu("Switch / Toggle")]
        private void TestToggle() => Toggle();

        [ContextMenu("Switch / Turn On")]
        private void TestTurnOn() => TurnOn();

        [ContextMenu("Switch / Turn Off")]
        private void TestTurnOff() => TurnOff();

        [ContextMenu("Switch / Next Position")]
        private void TestNext() => NextIndex();

        [ContextMenu("Switch / Previous Position")]
        private void TestPrevious() => PreviousIndex();

        [ContextMenu("Switch / Refresh Visual")]
        private void TestRefresh() => RefreshVisual();

        #endregion
    }
}
