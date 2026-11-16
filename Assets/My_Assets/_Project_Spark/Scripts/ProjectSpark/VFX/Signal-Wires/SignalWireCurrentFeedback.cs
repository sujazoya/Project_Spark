
using System;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Electrical;

namespace AAAUI.VFX
{
    /// <summary>
    /// Receives verified signed branch current for one
    /// SparkCircuitConnection and drives its lightning overlay.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SignalPathMesh))]
    public sealed class SignalWireCurrentFeedback : MonoBehaviour
    {
        [Header("Current-Driven Lightning")]
        [SerializeField]
        private SignalWireElectricalGlow electricalGlow;

        [Header("Current Popup")]
        [SerializeField]
        private SparkCurrentPopup popupPrefab;

        [SerializeField]
        private Transform popupParent;

        [SerializeField]
        private Color positiveColor =
            new Color(0.3f, 0.9f, 1f);

        [SerializeField]
        private Color negativeColor =
            new Color(1f, 0.65f, 0.25f);

        [SerializeField, Min(0f)]
        private float popupChangeThresholdAmps = 0.001f;

        [SerializeField, Min(0f)]
        private float minimumPopupInterval = 0.3f;

        [SerializeField]
        private Vector3 popupOffset =
            new Vector3(0f, 0.08f, 0f);

        [Header("Diagnostics")]
        [SerializeField]
        private bool logCurrentChanges;

        private SignalPathMesh wireMesh;

        private ulong connectionId;
        private bool bound;
        private bool hasReading;

        private float signedCurrentAmps;
        private float lastPopupTime = float.NegativeInfinity;

        public bool IsBound => bound;
        public ulong ConnectionId => connectionId;
        public bool HasReading => hasReading;
        public float SignedCurrentAmps => signedCurrentAmps;

        public event Action<SignalWireCurrentFeedback> ReadingChanged;

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            CacheComponents();
            UpdateLightning();
        }

        private void CacheComponents()
        {
            if (wireMesh == null)
                wireMesh = GetComponent<SignalPathMesh>();

            if (electricalGlow == null)
                electricalGlow =
                    GetComponent<SignalWireElectricalGlow>();
        }

        /// <summary>
        /// Called when SignalPath_Manager associates this
        /// runtime wire with a circuit connection.
        /// </summary>
        public void Bind(SparkCircuitConnection connection)
        {
            Unbind();

            if (connection == null)
                return;

            CacheComponents();

            connectionId = connection.Id;
            bound = true;

            UpdateLightning();

            if (logCurrentChanges)
            {
                Debug.Log(
                    $"[WIRE FEEDBACK BIND] Wire={name} | " +
                    $"ConnectionId={connectionId}",
                    this);
            }
        }

        public void Unbind()
        {
            ClearReading();

            bound = false;
            connectionId = 0UL;
            lastPopupTime = float.NegativeInfinity;
        }

        /// <summary>
        /// Accepts current only from the matching
        /// verified branch-current connection.
        /// </summary>
        public bool PublishCurrent(
            ulong sourceConnectionId,
            float amps)
        {
            if (!bound ||
                sourceConnectionId != connectionId ||
                float.IsNaN(amps) ||
                float.IsInfinity(amps))
            {
                return false;
            }

            CacheComponents();

            bool firstReading = !hasReading;

            float previousAmps = signedCurrentAmps;

            bool changed =
                firstReading ||
                Mathf.Abs(amps - previousAmps) >=
                popupChangeThresholdAmps;

            signedCurrentAmps = amps;
            hasReading = true;

            // Always update the glow, even when the
            // popup change threshold was not reached.
            UpdateLightning();

            if (logCurrentChanges &&
                (firstReading ||
                 Mathf.Abs(amps - previousAmps) > 0.0000001f))
            {
                Debug.Log(
                    $"[WIRE FEEDBACK PUBLISH] Wire={name} | " +
                    $"ConnectionId={connectionId} | " +
                    $"Current={amps:F6} A | " +
                    $"GlowFound={electricalGlow != null}",
                    this);
            }

            if (changed)
            {
                ReadingChanged?.Invoke(this);

                if (popupPrefab != null &&
                    Application.isPlaying &&
                    Time.time - lastPopupTime >=
                    minimumPopupInterval)
                {
                    ShowCurrentPopup(amps);
                }
            }

            return true;
        }

        private void UpdateLightning()
        {
            CacheComponents();

            if (electricalGlow == null)
            {
                if (logCurrentChanges)
                {
                    Debug.LogWarning(
                        "[WIRE FEEDBACK] Missing " +
                        "SignalWireElectricalGlow on " + name,
                        this);
                }

                return;
            }

            float amps = bound && hasReading
                ? signedCurrentAmps
                : 0f;

            electricalGlow.SetCurrent(amps);
        }

        private void ShowCurrentPopup(float amps)
        {
            Vector3 position = transform.position;

            if (wireMesh != null &&
                wireMesh.GeneratedMesh != null &&
                wireMesh.GeneratedMesh.vertexCount > 0 &&
                wireMesh.MeshRenderer != null)
            {
                position = wireMesh.MeshRenderer.bounds.center;
            }

            position += popupOffset;

            SparkCurrentPopup popup = Instantiate(
                popupPrefab,
                position,
                Quaternion.identity,
                popupParent);

            if (popup == null)
                return;

            popup.Play(
                amps,
                position,
                amps < 0f
                    ? negativeColor
                    : positiveColor);

            lastPopupTime = Time.time;
        }

        /// <summary>
        /// Called when a solve becomes invalid, a connection
        /// is removed, or no verified current is available.
        /// </summary>
        public void ClearReading()
        {
            bool hadReading = hasReading;

            hasReading = false;
            signedCurrentAmps = 0f;

            // Do not return early here.
            // The glow must always receive zero current.
            UpdateLightning();

            if (hadReading)
                ReadingChanged?.Invoke(this);
        }

        private void OnDisable()
        {
            ClearReading();
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
