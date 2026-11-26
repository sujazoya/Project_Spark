using System;
using System.Reflection;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.Events;
using ProjectSpark.Gameplay;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// One explicit, authored bridge to a REAL player/gameplay control.
    /// The two persistent UnityEvents must be bound in the Inspector to that control.
    /// The port never writes circuit state or uses reflection to set any field/property.
    /// Only an independent read of the authoritative component can verify success.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkEngineeringDevicePort : MonoBehaviour
    {
        public enum DeviceType { Switch, VirtualPowerSupply, OtherVerifiedDevice }
        [SerializeField] private string deviceId;
        [SerializeField] private DeviceType deviceType;
        [Tooltip("For a power supply, assign its SparkPowerSupply. For a switch, assign the live component containing its actual public state.")]
        [SerializeField] private Component authoritativeStateComponent;
        [Tooltip("Switch/other: exact public property or field name. Do not point this at visual or UI-only state.")]
        [SerializeField] private string stateMemberName = "State";
        [SerializeField] private string onStateValue = "Closed";
        [SerializeField] private string offStateValue = "Open";
        [Tooltip("Bind this to the EXISTING player/device 'on' or 'close' API; not a dummy animation.")]
        [SerializeField] private UnityEvent nativeOn = new UnityEvent();
        [Tooltip("Bind this to the EXISTING player/device 'off' or 'open' API; not a dummy animation.")]
        [SerializeField] private UnityEvent nativeOff = new UnityEvent();

        public string DeviceId => deviceId;
        public DeviceType Kind => deviceType;
        public bool HasNativeAction(bool turnOn) =>
            (turnOn ? nativeOn : nativeOff).GetPersistentEventCount() > 0;

        public bool TryRead(out bool isOn, out string evidence)
        {
            isOn = false;
            evidence = null;
            if (!gameObject.activeInHierarchy)
            { evidence = "Device GameObject is inactive."; return false; }
            Component source = authoritativeStateComponent;
            if (!source && deviceType == DeviceType.VirtualPowerSupply)
                source = GetComponent<SparkPowerSupply>();
            if (!source)
            { evidence = "No real authoritative state component bound."; return false; }
            if (deviceType == DeviceType.VirtualPowerSupply)
            {
                SparkPowerSupply supply = source as SparkPowerSupply;
                if (!supply)
                { evidence = "Power port must use a SparkPowerSupply state component."; return false; }
                isOn = supply.IsOutputActive;
                evidence = "SparkPowerSupply.IsOutputActive=" + isOn;
                return true;
            }
            if (string.IsNullOrWhiteSpace(stateMemberName))
            { evidence = "The exact real state member name was not configured."; return false; }
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            Type type = source.GetType();
            object value;
            try
            {
                PropertyInfo property = type.GetProperty(stateMemberName, flags);
                if (property != null && property.CanRead && property.GetIndexParameters().Length == 0)
                    value = property.GetValue(source, null);
                else
                {
                    FieldInfo field = type.GetField(stateMemberName, flags);
                    if (field == null)
                    { evidence = "No public state member '" + stateMemberName + "' on " + type.Name; return false; }
                    value = field.GetValue(source);
                }
            }
            catch (Exception ex)
            { evidence = "Real state read failed: " + ex.GetType().Name; return false; }
            string observed = value == null ? string.Empty : value.ToString();
            if (string.Equals(observed, onStateValue, StringComparison.OrdinalIgnoreCase))
                isOn = true;
            else if (string.Equals(observed, offStateValue, StringComparison.OrdinalIgnoreCase))
                isOn = false;
            else
            {
                evidence = "State '" + observed + "' matches neither configured ON='" + onStateValue +
                    "' nor OFF='" + offStateValue + "'.";
                return false;
            }
            evidence = type.Name + "." + stateMemberName + "=" + observed;
            return true;
        }

        public bool TryInvokeNative(bool turnOn, out string status)
        {
            status = null;
            UnityEvent action = turnOn ? nativeOn : nativeOff;
            if (action.GetPersistentEventCount() == 0)
            { status = "No Inspector-bound native player/device action. Refusing simulated operation."; return false; }
            try { action.Invoke(); status = "Native action dispatched; awaiting independent device-state confirmation."; return true; }
            catch (Exception ex)
            { status = "Native device action threw " + ex.GetType().Name + ": " + ex.Message; return false; }
        }

#if UNITY_EDITOR
        public void EditorInitializePower(SparkPowerSupply supply)
        {
            deviceType = DeviceType.VirtualPowerSupply;
            authoritativeStateComponent = supply;
            if (string.IsNullOrWhiteSpace(deviceId)) deviceId = gameObject.name;
        }
#endif
    }
}
