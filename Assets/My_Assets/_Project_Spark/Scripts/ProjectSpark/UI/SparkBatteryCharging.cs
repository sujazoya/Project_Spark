using System;
using UnityEngine;
using TMPro;
using ProjectSpark.UI;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Simulates the electrical and charge state of a rechargeable battery.
    ///
    /// This component owns battery simulation only.
    /// It does not contain UI or presentation logic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkBatteryCharging : MonoBehaviour
    {
        /// <summary>
        /// Runtime battery state.
        /// </summary>
        public enum BatteryState
        {
            Empty,
            Idle,
            Live,
            Charging,
            Full,
            Discharging,
            Fault
        }

        [Header("Battery")]
        [SerializeField]
        [Min(0.001f)]
        private float capacityAh = 5f;

        [SerializeField]
        [Range(0f, 1f)]
        private float initialChargePercent = 0.68f;

        [SerializeField]
        [Min(0f)]
        private float nominalVoltage = 12.48f;

        [Header("Charging")]
        [SerializeField]
        [Min(0f)]
        private float maximumChargeCurrent = 1f;

        [SerializeField]
        [Min(0f)]
        private float chargeEfficiency = 0.90f;

        [SerializeField]
        [Min(0.001f)]
        private float minimumLiveVoltage = 0.1f;

        [SerializeField]
        [Min(0.001f)]
        private float minimumLiveCurrent = 0.01f;

        [Header("Runtime")]
        [SerializeField]
        private BatteryState state = BatteryState.Idle;

        [SerializeField]
        private float chargePercent;

        [SerializeField]
        private float voltage;

        [SerializeField]
        private float current;

        [SerializeField]
        private float power;

        [SerializeField]
        private float elapsedChargeTime;

        [SerializeField]
        private float remainingChargeTime;

        private bool electricalInputActive;
        private bool fault;

        /// <summary>
        /// Fired whenever the battery runtime state changes.
        /// </summary>
        public event Action<SparkBatteryCharging> StateChanged;

        /// <summary>
        /// Fired whenever battery electrical or charge data changes.
        /// </summary>
        public event Action<SparkBatteryCharging> BatteryChanged;

        /// <summary>
        /// Current battery state.
        /// </summary>
        public BatteryState State => state;

        /// <summary>
        /// Charge from 0 to 1.
        /// </summary>
        public float ChargePercent => chargePercent;

        /// <summary>
        /// Charge from 0 to 100.
        /// </summary>
        public float ChargePercentage =>
            chargePercent * 100f;

        /// <summary>
        /// Battery capacity in amp-hours.
        /// </summary>
        public float CapacityAh => capacityAh;

        /// <summary>
        /// Battery voltage.
        /// </summary>
        public float Voltage => voltage;

        /// <summary>
        /// Battery current.
        /// </summary>
        public float Current => current;

        /// <summary>
        /// Electrical power in watts.
        /// </summary>
        public float Power => power;

        /// <summary>
        /// Returns true when the battery has a live electrical connection.
        /// </summary>
        public bool IsLive =>
            electricalInputActive;

        /// <summary>
        /// Returns true while the battery is charging.
        /// </summary>
        public bool IsCharging =>
            state == BatteryState.Charging;

        /// <summary>
        /// Returns true when the battery is full.
        /// </summary>
        public bool IsFull =>
            chargePercent >= 0.999f;

        /// <summary>
        /// Returns true when the battery is faulty.
        /// </summary>
        public bool IsFaulted =>
            fault;

        /// <summary>
        /// Total charging session time in seconds.
        /// </summary>
        public float ElapsedChargeTime =>
            elapsedChargeTime;

        /// <summary>
        /// Estimated remaining charging time in seconds.
        /// </summary>
        public float RemainingChargeTime =>
            remainingChargeTime;

        private void Awake()
        {
            chargePercent =
                Mathf.Clamp01(
                    initialChargePercent);

            voltage =
                Mathf.Max(
                    0f,
                    nominalVoltage);

            current = 0f;
            power = 0f;

            elapsedChargeTime = 0f;
            remainingChargeTime = 0f;

            electricalInputActive = false;
            fault = false;

            EvaluateState(false);
        }

        private void Update()
        {
            if (fault)
            {
                return;
            }

            SimulateCharging();
        }

        /// <summary>
        /// Receives the electrical state produced by the circuit solver.
        ///
        /// This is the main integration point between the battery
        /// simulation and Project Spark's electrical system.
        /// </summary>
        public void ApplyElectricalState(
            float newVoltage,
            float newCurrent)
        {
            if (float.IsNaN(newVoltage) ||
                float.IsInfinity(newVoltage) ||
                float.IsNaN(newCurrent) ||
                float.IsInfinity(newCurrent))
            {
                return;
            }

            voltage =
                Mathf.Max(
                    0f,
                    newVoltage);

            current =
                Mathf.Max(
                    0f,
                    newCurrent);

            power =
                voltage *
                current;

            electricalInputActive =
                voltage >= minimumLiveVoltage ||
                current >= minimumLiveCurrent;

            EvaluateState(true);

            BatteryChanged?.Invoke(this);
        }

        /// <summary>
        /// Simulates charge accumulation using the current electrical input.
        /// </summary>
        private void SimulateCharging()
        {
            if (state != BatteryState.Charging)
            {
                return;
            }

            if (!electricalInputActive)
            {
                return;
            }

            if (current <= 0f)
            {
                return;
            }

            float effectiveCurrent =
                Mathf.Min(
                    current,
                    maximumChargeCurrent);

            float chargePerSecond =
                effectiveCurrent /
                (capacityAh * 3600f);

            chargePerSecond *=
                Mathf.Clamp01(
                    chargeEfficiency);

            float previousCharge =
                chargePercent;

            chargePercent +=
                chargePerSecond *
                Time.deltaTime;

            chargePercent =
                Mathf.Clamp01(
                    chargePercent);

            elapsedChargeTime +=
                Time.deltaTime;

            CalculateRemainingTime(
                chargePerSecond);

            if (chargePercent >= 0.999f)
            {
                chargePercent = 1f;
                remainingChargeTime = 0f;

                SetState(
                    BatteryState.Full);
            }

            if (Mathf.Abs(
                    chargePercent -
                    previousCharge) >
                0.000001f)
            {
                BatteryChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Calculates approximate remaining charging time.
        /// </summary>
        private void CalculateRemainingTime(
            float chargePerSecond)
        {
            if (chargePerSecond <= 0.0000001f)
            {
                remainingChargeTime = 0f;
                return;
            }

            float remaining =
                Mathf.Max(
                    0f,
                    1f - chargePercent);

            remainingChargeTime =
                remaining /
                chargePerSecond;
        }

        /// <summary>
        /// Determines the correct battery state.
        /// </summary>
        private void EvaluateState(
            bool notify)
        {
            BatteryState newState;

            if (fault)
            {
                newState =
                    BatteryState.Fault;
            }
            else if (chargePercent >= 0.999f)
            {
                newState =
                    BatteryState.Full;
            }
            else if (electricalInputActive &&
                     current >= minimumLiveCurrent)
            {
                newState =
                    BatteryState.Charging;
            }
            else if (electricalInputActive)
            {
                newState =
                    BatteryState.Live;
            }
            else if (chargePercent <= 0f)
            {
                newState =
                    BatteryState.Empty;
            }
            else
            {
                newState =
                    BatteryState.Idle;
            }

            SetState(
                newState,
                notify);
        }

        /// <summary>
        /// Changes the battery state.
        /// </summary>
        private void SetState(
            BatteryState newState,
            bool notify = true)
        {
            if (state == newState)
            {
                return;
            }

            state = newState;

            if (state != BatteryState.Charging)
            {
                if (state == BatteryState.Full)
                {
                    remainingChargeTime = 0f;
                }
            }

            if (notify)
            {
                StateChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Starts a new charging session.
        /// </summary>
        public void StartCharging()
        {
            if (fault ||
                IsFull)
            {
                return;
            }

            elapsedChargeTime = 0f;

            EvaluateState(true);
        }

        /// <summary>
        /// Stops active charging without resetting battery charge.
        /// </summary>
        public void StopCharging()
        {
            if (state == BatteryState.Full)
            {
                return;
            }

            electricalInputActive = false;
            current = 0f;
            power = 0f;
            remainingChargeTime = 0f;

            EvaluateState(true);

            BatteryChanged?.Invoke(this);
        }

        /// <summary>
        /// Marks the battery as faulty.
        /// </summary>
        public void SetFault()
        {
            fault = true;

            state =
                BatteryState.Fault;

            remainingChargeTime = 0f;

            StateChanged?.Invoke(this);
            BatteryChanged?.Invoke(this);
        }

        /// <summary>
        /// Clears the battery fault.
        /// </summary>
        public void ClearFault()
        {
            fault = false;

            EvaluateState(true);

            BatteryChanged?.Invoke(this);
        }

        /// <summary>
        /// Sets the battery charge directly.
        /// </summary>
        public void SetChargePercent(
            float value)
        {
            chargePercent =
                Mathf.Clamp01(value);

            if (chargePercent >= 0.999f)
            {
                chargePercent = 1f;
                remainingChargeTime = 0f;
            }

            EvaluateState(true);

            BatteryChanged?.Invoke(this);
        }

        /// <summary>
        /// Fully resets the battery.
        /// </summary>
        public void ResetBattery()
        {
            chargePercent =
                Mathf.Clamp01(
                    initialChargePercent);

            voltage =
                Mathf.Max(
                    0f,
                    nominalVoltage);

            current = 0f;
            power = 0f;

            elapsedChargeTime = 0f;
            remainingChargeTime = 0f;

            electricalInputActive = false;
            fault = false;

            EvaluateState(true);

            BatteryChanged?.Invoke(this);
        }
    }
}