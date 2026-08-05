using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Defines one logical VFX state.
    ///
    /// This class only describes a state.
    /// It does not control VFX playback.
    /// It does not resolve controllers.
    /// It does not modify materials.
    ///
    /// SparkVFXStateResolver is responsible for resolving this definition.
    /// SparkVFXLayeredStateMachine is responsible for applying the resolved state.
    /// </summary>
    [System.Serializable]
    public sealed class SparkVFXStateDefinition
    {
        // ============================================================
        // IDENTITY
        // ============================================================

        [Header("Identity")]

        [Tooltip(
            "Unique logical ID of this VFX state."
        )]
        [SerializeField]
        private string stateID = "Normal";


        // ============================================================
        // ENABLED
        // ============================================================

        [Header("State")]

        [Tooltip(
            "Whether this state can be resolved and used."
        )]
        [SerializeField]
        private bool enabled = true;


        // ============================================================
        // PRIORITY
        // ============================================================

        [Tooltip(
            "Priority used when multiple states are available. " +
            "Higher values have higher priority."
        )]
        [SerializeField]
        private int priority;


        // ============================================================
        // PROFILE
        // ============================================================

        [Header("Profile")]

        [Tooltip(
            "Optional local VFX profile. " +
            "If empty, SparkVFXStateResolver may use its profile library."
        )]
        [SerializeField]
        private SparkVFXProfile profile;


        // ============================================================
        // PUBLIC ACCESS
        // ============================================================

        public string StateID
        {
            get
            {
                return stateID;
            }
        }


        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }


        public int Priority
        {
            get
            {
                return priority;
            }
        }


        public SparkVFXProfile Profile
        {
            get
            {
                return profile;
            }
        }


        // ============================================================
        // NORMALIZED ID
        // ============================================================

        public string GetNormalizedID()
        {
            if (
                string.IsNullOrWhiteSpace(
                    stateID
                )
            )
            {
                return string.Empty;
            }


            return stateID.Trim();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarnings = true)
        {
            bool valid =
                true;


            string normalizedID =
                GetNormalizedID();


            // --------------------------------------------------------
            // STATE ID
            // --------------------------------------------------------

            if (
                string.IsNullOrWhiteSpace(
                    normalizedID
                )
            )
            {
                valid =
                    false;


                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXStateDefinition] " +
                        "State ID is empty."
                    );
                }
            }


            // --------------------------------------------------------
            // PROFILE
            // --------------------------------------------------------

            if (
                profile == null
            )
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXStateDefinition] " +
                        "State '" +
                        normalizedID +
                        "' has no local profile. " +
                        "SparkVFXStateResolver will attempt " +
                        "to resolve it from SparkVFXProfileLibrary."
                    );
                }
            }


            return valid;
        }
    }
}