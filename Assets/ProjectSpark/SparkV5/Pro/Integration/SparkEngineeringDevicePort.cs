using System;
using System.Reflection;
using ProjectSpark.Electrical;
using UnityEngine;
using UnityEngine.Events;
using ProjectSpark.Gameplay;

namespace ProjectSpark.SparkV5.Pro
{
    /// <summary>
    /// Authored bridge to an existing gameplay control. Power and switch ports use
    /// their typed APIs. Custom devices may use authored callbacks and readback.
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
        public SparkPowerSupply Supply => deviceType == DeviceType.VirtualPowerSupply
            ? (authoritativeStateComponent ? authoritativeStateComponent as SparkPowerSupply : GetComponent<SparkPowerSupply>()) : null;
        public SparkSwitch Switch => deviceType == DeviceType.Switch
            ? (authoritativeStateComponent ? authoritativeStateComponent as SparkSwitch : GetComponent<SparkSwitch>()) : null;
        internal SwitchLeverController PhysicalLever
        {
            get
            {
                var lever = Switch ? GetComponent<SwitchLeverController>() : null;
                return lever && lever.Switch == Switch ? lever : null;
            }
        }
        public Transform PhysicalControl => PhysicalLever ? PhysicalLever.LeverTransform : null;
        public bool HasNativeAction(bool turnOn) =>
            Supply || Switch || (deviceType == DeviceType.OtherVerifiedDevice &&
                (turnOn ? nativeOn : nativeOff).GetPersistentEventCount() > 0);

        public bool CanOperate(bool turnOn, MonoBehaviour interactor, out string reason)
        {
            var device = Supply ? (SparkElectronicObject)Supply : Switch;
            if (!device)
            {
                reason = HasNativeAction(turnOn) ? null : "No verified native device action is available.";
                return reason == null;
            }
            if (Supply && (Supply.State == SparkPowerSupplyState.Fault || (turnOn && !Supply.ElectricalEnabled)))
            {
                reason = "The supply is faulted or electrically disabled.";
                return false;
            }
            return device.CanInteract(CreateContext(device, interactor), out reason);
        }

        private SparkInteractionContext CreateContext(SparkElectronicObject device, MonoBehaviour interactor) =>
            new SparkInteractionContext(interactor, device, SparkInteractionType.Toggle, SparkToolType.Select,
                PhysicalControl ? PhysicalControl.position : device.transform.position, Vector3.up, default, device.gameObject);

        public bool TryRead(out bool isOn, out string evidence)
        {
            isOn = false;
            evidence = null;
            if (!gameObject.activeInHierarchy)
            { evidence = "Device GameObject is inactive."; return false; }
            if (deviceType == DeviceType.VirtualPowerSupply)
            {
                SparkPowerSupply supply = Supply;
                if (!supply)
                { evidence = "Power port must use a SparkPowerSupply state component."; return false; }
                isOn = supply.IsOutputActive;
                evidence = "SparkPowerSupply.State=" + supply.State + ", IsOutputActive=" + isOn;
                return true;
            }
            if (deviceType == DeviceType.Switch)
            {
                var control = Switch;
                if (!control) { evidence = "Switch port requires a SparkSwitch."; return false; }
                isOn = control.IsClosed;
                evidence = "SparkSwitch.State=" + control.State;
                return true;
            }
            Component source = authoritativeStateComponent;
            if (!source)
            { evidence = "No real authoritative state component bound."; return false; }
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

        public bool TryInvokeNative(bool turnOn, MonoBehaviour interactor, out string status)
        {
            if (!CanOperate(turnOn, interactor, out status)) return false;
            if (Supply)
            {
                var result = Supply.SetOutput(turnOn, CreateContext(Supply, interactor));
                status = result.Message;
                return result.Succeeded;
            }
            if (Switch)
            {
                if (Switch.IsClosed == turnOn) return true;
                var result = Switch.Toggle(CreateContext(Switch, interactor));
                status = result.Message;
                return result.Succeeded;
            }
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
            onStateValue = nameof(SparkPowerSupplyState.Active);
            offStateValue = nameof(SparkPowerSupplyState.Standby);
            if (string.IsNullOrWhiteSpace(deviceId)) deviceId = gameObject.name;
        }

        public void EditorInitializeSwitch(SparkSwitch control)
        {
            deviceType = DeviceType.Switch;
            authoritativeStateComponent = control;
            onStateValue = nameof(SparkSwitchState.Closed);
            offStateValue = nameof(SparkSwitchState.Open);
            if (string.IsNullOrWhiteSpace(deviceId)) deviceId = gameObject.name;
        }
#endif
    }
}
