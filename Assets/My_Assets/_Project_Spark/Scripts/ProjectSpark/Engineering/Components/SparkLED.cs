using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLED :
        SparkElectricalComponent,
        ISparkConductiveDevice
    {
        [Header("LED Visual")]
        [SerializeField]
        private GameObject ledHelix;

        
        [Header("Terminals")]
        [SerializeField]
        private SparkTerminal anodeTerminal;

        [SerializeField]
        private SparkTerminal cathodeTerminal;

        [Header("LED")]
        [SerializeField, Min(0f)]
        private float forwardVoltage = 2f;

        [SerializeField, Min(0.000001f)]
        private float maximumForwardCurrent = 0.02f;

        [SerializeField, Min(0f)]
        private float reverseVoltageLimit = 5f;

        [SerializeField, Min(0.01f)]
        private float onResistance = 1f;

        [SerializeField, Min(0.01f)]
        private float conductionHysteresis = 0.01f;

        public SparkTerminal AnodeTerminal =>
            anodeTerminal;

        public SparkTerminal CathodeTerminal =>
            cathodeTerminal;

        public float ForwardVoltage =>
            forwardVoltage;

        public float MaximumForwardCurrent =>
            maximumForwardCurrent;

        public float ReverseVoltageLimit =>
            reverseVoltageLimit;

            public float OnResistance =>
                onResistance;
                public bool HasVisualReference =>
        ledHelix != null;

        public bool IsVisualActiveInHierarchy =>
            ledHelix != null &&
            ledHelix.activeInHierarchy; 

            public bool HasVisualRenderer =>
            ledHelix != null &&
            ledHelix.TryGetComponent<MeshRenderer>(out _);

        public bool IsVisualRendererEnabled
        {
            get
            {
                if (ledHelix == null)
                    return false;

                if (!ledHelix.TryGetComponent<MeshRenderer>(
                        out MeshRenderer renderer))
                {
                    return false;
                }

                return renderer.enabled &&
                    renderer.gameObject.activeInHierarchy;
            }
        }         
            

        public bool IsOn =>
    ElectricalEnabled &&
    ElectricalState.Conduction ==
        SparkConductionState.Conducting &&
    ElectricalState.Current > 0.000001f;

        public bool IsOverCurrent =>
            Mathf.Abs(
                ElectricalState.Current) >
            maximumForwardCurrent;

        public bool IsReverseVoltageExceeded =>
            ElectricalState.Voltage <
            -reverseVoltageLimit;


            public float ConductionHysteresis =>
    conductionHysteresis;

    private void OnEnable()
{
    RefreshLEDVisual();
}

        public bool CanConductBetween(
            SparkTerminal from,
            SparkTerminal to)
        {
            if (!ElectricalEnabled ||
                from == null ||
                to == null)
            {
                return false;
            }

            if (anodeTerminal == null ||
                cathodeTerminal == null)
            {
                return false;
            }

            if (from != anodeTerminal ||
                to != cathodeTerminal)
            {
                return false;
            }

            return
                ElectricalState.Conduction ==
                SparkConductionState.Conducting;
        }
        [ContextMenu("Debug LED Electrical State")]
private void DebugElectricalState()
{
    Debug.Log(
        $"[LED ELECTRICAL STATE] " +
        $"LED={name} | " +
        $"Enabled={ElectricalEnabled} | " +
        $"Anode={(anodeTerminal != null ? anodeTerminal.name : "NULL")} | " +
        $"Cathode={(cathodeTerminal != null ? cathodeTerminal.name : "NULL")} | " +
        $"Voltage={ElectricalState.Voltage:F3} V | " +
        $"Current={ElectricalState.Current:F6} A | " +
        $"Conduction={ElectricalState.Conduction} | " +
        $"IsOn={IsOn}",
        this);
}

        private void OnValidate()
        {
            forwardVoltage =
                Mathf.Max(
                    0f,
                    forwardVoltage);

            maximumForwardCurrent =
                Mathf.Max(
                    0.000001f,
                    maximumForwardCurrent);

            reverseVoltageLimit =
                Mathf.Max(
                    0f,
                    reverseVoltageLimit);

            onResistance =
                Mathf.Max(
                    0.01f,
                    onResistance);

            conductionHysteresis =
                Mathf.Max(
                    0.01f,
                    conductionHysteresis);       }

        private void RefreshLEDVisual()
        {
            // A missing visual must never interrupt the electrical solver.
            if (ledHelix == null)
                return;

            bool shouldBeVisible = IsOn;

            if (ledHelix.activeSelf != shouldBeVisible)
            {
                ledHelix.SetActive(shouldBeVisible);
            }
        }

        
        
        public override void ApplyElectricalState(
                in SparkElectricalState state)
            {
                base.ApplyElectricalState(in state);
                RefreshLEDVisual();
            }

            [ContextMenu("Debug LED State")]
private void DebugLEDState()
{
    Debug.Log(
        $"[LED DEBUG] {name} | " +
        $"Enabled={ElectricalEnabled} | " +
        $"Voltage={ElectricalState.Voltage:F3} V | " +
        $"Current={ElectricalState.Current:F6} A | " +
        $"Conduction={ElectricalState.Conduction} | " +
        $"IsOn={IsOn} | " +
        $"HelixAssigned={ledHelix != null} | " +
        $"HelixActive={(ledHelix != null && ledHelix.activeSelf)}",
        this);
}
            
    }
}