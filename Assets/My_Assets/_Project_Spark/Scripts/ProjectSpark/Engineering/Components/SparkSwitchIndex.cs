
using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Multi-position electrical switch.
    ///
    /// Example:
    ///
    /// Index 0 = OFF
    /// Index 1 = LOW
    /// Index 2 = HIGH
    ///
    /// The index determines the configured voltage percentage:
    ///
    /// 0 = 0%
    /// 1 = 50%
    /// 2 = 100%
    ///
    /// IMPORTANT:
    /// This component exposes VoltageLimit for the solver/visual systems.
    /// The existing DC solver must explicitly use VoltageLimit if the
    /// index is expected to produce different electrical voltage levels.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkSwitchIndex :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        // ============================================================
        // TERMINALS
        // ============================================================

        [Header("Terminals")]

        [SerializeField]
        private SparkTerminal inputTerminal;

        [SerializeField]
        private SparkTerminal outputTerminal;


        // ============================================================
        // INDEX
        // ============================================================

        [Header("Index")]

        [SerializeField]
        [Min(0)]
        private int currentIndex = 0;

        [SerializeField]
        [Min(1)]
        private int positionCount = 3;


        // ============================================================
        // VOLTAGE LIMIT
        // ============================================================

        [Header("Voltage Limit")]

        [SerializeField]
        [Min(0f)]
        private float maximumVoltage = 230f;

        [SerializeField]
        private float[] indexVoltagePercent =
        {
            0f,
            50f,
            100f
        };


        // ============================================================
        // ELECTRICAL
        // ============================================================

        [Header("Electrical")]

        [SerializeField]
        [Min(0.000001f)]
        private float conductingResistance = 0.01f;

        [SerializeField]
        [Min(0.000001f)]
        private float openResistance = 1000000000f;

        [SerializeField]
        [Min(0f)]
        private float voltageThreshold = 0.001f;

        [SerializeField]
        [Min(0f)]
        private float currentThreshold = 0.001f;


        // ============================================================
        // RUNTIME
        // ============================================================

        [Header("Runtime")]

        [SerializeField]
        private bool isEnabled = true;


        // ============================================================
        // PUBLIC ACCESS
        // ============================================================

        public SparkTerminal InputTerminal =>
            inputTerminal;

        public SparkTerminal OutputTerminal =>
            outputTerminal;

        public int Index =>
            currentIndex;

        public int PositionCount =>
            Mathf.Max(1, positionCount);

        public float MaximumVoltage =>
            maximumVoltage;

        public float CurrentIndexVoltagePercent =>
            GetIndexVoltagePercent(currentIndex);

        public float VoltageLimit =>
            maximumVoltage *
            CurrentIndexVoltagePercent /
            100f;

        public bool IsOff =>
            currentIndex == 0 ||
            !isEnabled;

        public bool IsActive =>
            !IsOff;

        public bool IsConducting =>
            IsActive &&
            ElectricalEnabled &&
            CurrentIndexVoltagePercent > 0f;

        public float Resistance =>
            IsConducting
                ? conductingResistance
                : openResistance;

        public float Voltage =>
            ElectricalState.Voltage;

        public float Current =>
            ElectricalState.Current;

        public float Power =>
            ElectricalState.Power;

        public bool HasVoltage =>
            Mathf.Abs(Voltage) >= voltageThreshold;

        public bool HasCurrent =>
            Mathf.Abs(Current) >= currentThreshold;


        // ============================================================
        // VISUAL STATE
        // ============================================================

        /// <summary>
        /// Fired when the visual/electrical state changes.
        /// </summary>
        public event Action VisualStateChanged;

      private int lastVisualState = -1;
private int lastNotifiedIndex = -1;


        // ============================================================
        // INDEX CONTROL
        // ============================================================

        public void SetIndex(int index)
        {
            int clampedIndex =
                Mathf.Clamp(
                    index,
                    0,
                    PositionCount - 1);

            if (currentIndex == clampedIndex)
            {
                // Even if the index did not change, refresh the
                // visual state. This is useful after solver updates.
                NotifyVisualStateChanged();
                return;
            }

            currentIndex = clampedIndex;

            NotifyElectricalConfigurationChanged();
            NotifyVisualStateChanged();
        }


        public void NextIndex()
        {
            int nextIndex =
                currentIndex + 1;

            if (nextIndex >= PositionCount)
                nextIndex = 0;

            SetIndex(nextIndex);
        }


        public void PreviousIndex()
        {
            int previousIndex =
                currentIndex - 1;

            if (previousIndex < 0)
            {
                previousIndex =
                    PositionCount - 1;
            }

            SetIndex(previousIndex);
        }


        public void TurnOff()
        {
            SetIndex(0);
        }


        // ============================================================
        // INDEX VOLTAGE
        // ============================================================

        public void SetIndexVoltagePercent(
            int index,
            float percentage)
        {
            if (index < 0)
                return;

            if (index >= indexVoltagePercent.Length)
                return;

            indexVoltagePercent[index] =
                Mathf.Clamp(
                    percentage,
                    0f,
                    100f);

            NotifyElectricalConfigurationChanged();
            NotifyVisualStateChanged();
        }


        private float GetIndexVoltagePercent(int index)
        {
            if (index < 0)
                return 0f;

            if (index >= indexVoltagePercent.Length)
                return 0f;

            return Mathf.Clamp(
                indexVoltagePercent[index],
                0f,
                100f);
        }


        // ============================================================
        // CONDUCTION
        // ============================================================

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!IsConducting)
                return false;

            if (from == null ||
                to == null)
            {
                return false;
            }

            if (inputTerminal == null ||
                outputTerminal == null)
            {
                return false;
            }

            return
                (from == inputTerminal &&
                 to == outputTerminal) ||

                (from == outputTerminal &&
                 to == inputTerminal);
        }


        // ============================================================
        // ELECTRICAL STATE
        // ============================================================

        public override void ApplyElectricalState(
            in SparkElectricalState state)
        {
            base.ApplyElectricalState(state);

            NotifyVisualStateChanged();
        }


        // ============================================================
        // VISUAL STATE
        // ============================================================

        /// <summary>
        /// 0 = OFF
        /// 1 = ACTIVE but no solved voltage/current
        /// 2 = ACTIVE with solved voltage/current
        /// </summary>
        private int GetVisualState()
        {
            if (IsOff)
                return 0;

            if (HasVoltage ||
                HasCurrent)
            {
                return 2;
            }

            return 1;
        }


       private void NotifyVisualStateChanged()
{
    int newState = GetVisualState();

    bool indexChanged =
        currentIndex != lastNotifiedIndex;

    bool visualStateChanged =
        newState != lastVisualState;

    if (!indexChanged && !visualStateChanged)
        return;

    lastNotifiedIndex = currentIndex;
    lastVisualState = newState;

    VisualStateChanged?.Invoke();
}

        // ============================================================
        // UNITY VALIDATION
        // ============================================================

        private void OnValidate()
        {
            positionCount =
                Mathf.Max(
                    1,
                    positionCount);

            currentIndex =
                Mathf.Clamp(
                    currentIndex,
                    0,
                    positionCount - 1);

            maximumVoltage =
                Mathf.Max(
                    0f,
                    maximumVoltage);

            conductingResistance =
                Mathf.Max(
                    0.000001f,
                    conductingResistance);

            openResistance =
                Mathf.Max(
                    conductingResistance,
                    openResistance);

            voltageThreshold =
                Mathf.Max(
                    0f,
                    voltageThreshold);

            currentThreshold =
                Mathf.Max(
                    0f,
                    currentThreshold);

            if (indexVoltagePercent == null ||
                indexVoltagePercent.Length != positionCount)
            {
                ResizeIndexVoltageArray();
            }

            for (int i = 0;
                 i < indexVoltagePercent.Length;
                 i++)
            {
                indexVoltagePercent[i] =
                    Mathf.Clamp(
                        indexVoltagePercent[i],
                        0f,
                        100f);
            }
        }


        private void ResizeIndexVoltageArray()
        {
            float[] old =
                indexVoltagePercent;

            indexVoltagePercent =
                new float[positionCount];

            if (old == null)
                return;

            int copyCount =
                Mathf.Min(
                    old.Length,
                    indexVoltagePercent.Length);

            for (int i = 0;
                 i < copyCount;
                 i++)
            {
                indexVoltagePercent[i] =
                    old[i];
            }
        }


        // ============================================================
        // TEST
        // ============================================================

        [ContextMenu("Test / Index 0")]
        private void TestIndex0()
        {
            SetIndex(0);
        }


        [ContextMenu("Test / Index 1")]
        private void TestIndex1()
        {
            if (PositionCount > 1)
                SetIndex(1);
        }


        [ContextMenu("Test / Index 2")]
        private void TestIndex2()
        {
            if (PositionCount > 2)
                SetIndex(2);
        }


        [ContextMenu("Test / Next Index")]
        private void TestNextIndex()
        {
            NextIndex();
        }


        [ContextMenu("Test / Previous Index")]
        private void TestPreviousIndex()
        {
            PreviousIndex();
        }
    }
}
