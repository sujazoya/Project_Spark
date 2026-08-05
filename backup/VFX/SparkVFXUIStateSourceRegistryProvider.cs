using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Provides centralized access to a
    /// SparkVFXUIStateSourceRegistry.
    ///
    /// Intended to live on the same GameObject as:
    /// - SparkVFXUIStateSourceRegistry
    /// - SparkVFXUIStateCoordinator
    ///
    /// Other systems can reference this provider instead of
    /// directly searching the scene for the registry.
    ///
    /// This component does NOT:
    /// - Resolve VFX profiles.
    /// - Play VFX.
    /// - Resolve state priority.
    /// - Replace SparkVFXUIStateCoordinator.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIStateSourceRegistryProvider
        : MonoBehaviour
    {
        // ============================================================
        // REGISTRY
        // ============================================================

        [Header("Registry")]

        [Tooltip(
            "Central Spark VFX UI state source registry."
        )]
        [SerializeField]
        private SparkVFXUIStateSourceRegistry registry;


        // ============================================================
        // AUTO FIND
        // ============================================================

        [Header("Auto Find")]

        [Tooltip(
            "Automatically finds the registry on this GameObject " +
            "or in the parent hierarchy."
        )]
        [SerializeField]
        private bool autoFindRegistry = true;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveRegistry();
        }


        // ============================================================
        // ENABLE
        // ============================================================

        private void OnEnable()
        {
            ResolveRegistry();
        }


        // ============================================================
        // REGISTRY PROPERTY
        // ============================================================

        public SparkVFXUIStateSourceRegistry Registry
        {
            get
            {
                ResolveRegistry();

                return registry;
            }
        }


        // ============================================================
        // RESOLVE REGISTRY
        // ============================================================

        private void ResolveRegistry()
        {
            if (registry != null)
            {
                return;
            }


            if (!autoFindRegistry)
            {
                return;
            }


            registry =
                GetComponent<
                    SparkVFXUIStateSourceRegistry
                >();


            if (registry != null)
            {
                return;
            }


            registry =
                GetComponentInParent<
                    SparkVFXUIStateSourceRegistry
                >();
        }


        // ============================================================
        // GET SOURCE
        // ============================================================

        public SparkVFXUIStateSource GetSource(
            string sourceID)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return null;
            }


            return registry.GetSource(
                sourceID
            );
        }


        // ============================================================
        // TRY GET SOURCE
        // ============================================================

        public bool TryGetSource(
            string sourceID,
            out SparkVFXUIStateSource source)
        {
            ResolveRegistry();


            source =
                null;


            if (registry == null)
            {
                return false;
            }


            return registry.TryGetSource(
                sourceID,
                out source
            );
        }


        // ============================================================
        // ACTIVATE
        // ============================================================

        public bool Activate(
            string sourceID)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            return registry.ActivateSource(
                sourceID
            );
        }


        // ============================================================
        // DEACTIVATE
        // ============================================================

        public bool Deactivate(
            string sourceID)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            return registry.DeactivateSource(
                sourceID
            );
        }


        // ============================================================
        // SET STATE
        // ============================================================

        public bool SetState(
            string sourceID,
            SparkVFXEventType eventType)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            return registry.SetState(
                sourceID,
                eventType
            );
        }


        // ============================================================
        // SET STATE + PRIORITY
        // ============================================================

        public bool SetState(
            string sourceID,
            SparkVFXEventType eventType,
            int priority)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            return registry.SetState(
                sourceID,
                eventType,
                priority
            );
        }


        // ============================================================
        // ACTIVATE STATE
        // ============================================================

        public bool ActivateState(
            string sourceID,
            SparkVFXEventType eventType,
            int priority)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            return registry.ActivateState(
                sourceID,
                eventType,
                priority
            );
        }


        // ============================================================
        // DEACTIVATE STATE
        // ============================================================

        public bool DeactivateState(
            string sourceID)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            return registry.DeactivateState(
                sourceID
            );
        }


        // ============================================================
        // IS REGISTERED
        // ============================================================

        public bool IsRegistered(
            string sourceID)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            return registry.IsRegistered(
                sourceID
            );
        }


        // ============================================================
        // IS ACTIVE
        // ============================================================

        public bool IsActive(
            string sourceID)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            return registry.IsActive(
                sourceID
            );
        }


        // ============================================================
        // SOURCE COUNT
        // ============================================================

        public int SourceCount
        {
            get
            {
                ResolveRegistry();


                if (registry == null)
                {
                    return 0;
                }


                return registry.SourceCount;
            }
        }


        // ============================================================
        // REFRESH
        // ============================================================

        public void Refresh()
        {
            ResolveRegistry();


            if (registry == null)
            {
                return;
            }


            registry.Refresh();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarning = true)
        {
            ResolveRegistry();


            if (registry != null)
            {
                return registry.Validate(
                    logWarning
                );
            }


            if (logWarning)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceRegistryProvider] " +
                    "SparkVFXUIStateSourceRegistry is not assigned " +
                    "or could not be found.",
                    this
                );
            }


            return false;
        }


#if UNITY_EDITOR

        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

        [ContextMenu(
            "Validate Registry Provider"
        )]
        private void ValidateFromContextMenu()
        {
            bool valid =
                Validate(
                    true
                );


            if (valid)
            {
                Debug.Log(
                    "[SparkVFXUIStateSourceRegistryProvider] " +
                    "Validation successful.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUIStateSourceRegistryProvider] " +
                    "Validation failed.",
                    this
                );
            }
        }


        // ============================================================
        // EDITOR REFRESH
        // ============================================================

        [ContextMenu(
            "Refresh Registry"
        )]
        private void RefreshFromContextMenu()
        {
            Refresh();
        }

#endif
    }
}