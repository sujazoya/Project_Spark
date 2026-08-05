using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Creates and manages runtime SparkVFXUIStateSource instances.
    ///
    /// Intended for:
    /// - Dynamically spawned UI
    /// - Runtime target indicators
    /// - Quest markers
    /// - Tutorial highlights
    /// - Warning indicators
    /// - Temporary VFX state sources
    ///
    /// Architecture:
    ///
    /// Factory
    ///    ↓
    /// SparkVFXUIStateSource
    ///    ↓
    /// SparkVFXUIStateSourceRegistry
    ///    ↓
    /// SparkVFXUIStateSourceHandle
    ///
    /// The factory owns runtime-created source GameObjects.
    /// The handle provides safe runtime control.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIStateSourceFactory
        : MonoBehaviour
    {
        // ============================================================
        // REGISTRY
        // ============================================================

        [Header("Registry")]

        [Tooltip(
            "Registry used to register dynamically created state sources."
        )]
        [SerializeField]
        private SparkVFXUIStateSourceRegistry registry;


        // ============================================================
        // AUTO FIND
        // ============================================================

        [Header("Auto Find")]

        [Tooltip(
            "Automatically searches this GameObject and parents " +
            "for the state source registry."
        )]
        [SerializeField]
        private bool autoFindRegistry = true;


        // ============================================================
        // PARENT
        // ============================================================

        [Header("Runtime Parent")]

        [Tooltip(
            "Optional parent for dynamically created state sources."
        )]
        [SerializeField]
        private Transform runtimeParent;


        // ============================================================
        // DEFAULT SETTINGS
        // ============================================================

        [Header("Defaults")]

        [SerializeField]
        private SparkVFXEventType defaultEventType =
            SparkVFXEventType.Normal;


        [SerializeField]
        private int defaultPriority;


        [SerializeField]
        private bool defaultInstantPlayback;


        // ============================================================
        // RUNTIME
        // ============================================================

        private int generatedSourceIndex;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveRegistry();
        }


        // ============================================================
        // REGISTRY
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
        // RUNTIME PARENT
        // ============================================================

        public Transform RuntimeParent
        {
            get
            {
                if (runtimeParent != null)
                {
                    return runtimeParent;
                }

                return transform;
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
        // CREATE
        // ============================================================

        public SparkVFXUIStateSourceHandle Create(
            string sourceID)
        {
            return Create(
                sourceID,
                defaultEventType,
                defaultPriority,
                defaultInstantPlayback
            );
        }


        // ============================================================
        // CREATE CONFIGURED
        // ============================================================

        public SparkVFXUIStateSourceHandle Create(
            string sourceID,
            SparkVFXEventType eventType,
            int priority,
            bool instantPlayback)
        {
            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceFactory] " +
                    "Cannot create a state source with an empty ID.",
                    this
                );

                return null;
            }


            ResolveRegistry();


            if (registry == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceFactory] " +
                    "SparkVFXUIStateSourceRegistry is not assigned " +
                    "or could not be found.",
                    this
                );

                return null;
            }


            string normalizedID =
                sourceID.Trim();


            if (
                registry.IsRegistered(
                    normalizedID
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceFactory] " +
                    "A state source with ID '" +
                    normalizedID +
                    "' is already registered.",
                    this
                );

                return null;
            }


            GameObject sourceObject =
                new GameObject(
                    "VFX State Source - " +
                    normalizedID
                );


            sourceObject.transform.SetParent(
                RuntimeParent,
                false
            );


            SparkVFXUIStateSource source =
                sourceObject.AddComponent<
                    SparkVFXUIStateSource
                >();


            ConfigureSource(
                source,
                normalizedID,
                eventType,
                priority,
                instantPlayback
            );


            if (
                !registry.Register(
                    source
                )
            )
            {
                DestroyRuntimeObject(
                    sourceObject
                );

                return null;
            }


            SparkVFXUIStateSourceHandle handle =
                new SparkVFXUIStateSourceHandle(
                    source
                );


            generatedSourceIndex++;


            return handle;
        }


        // ============================================================
        // CREATE CHILD SOURCE
        // ============================================================

        public SparkVFXUIStateSourceHandle CreateFor(
            Transform target,
            string sourceID)
        {
            return CreateFor(
                target,
                sourceID,
                defaultEventType,
                defaultPriority,
                defaultInstantPlayback
            );
        }


        // ============================================================
        // CREATE CHILD SOURCE CONFIGURED
        // ============================================================

        public SparkVFXUIStateSourceHandle CreateFor(
            Transform target,
            string sourceID,
            SparkVFXEventType eventType,
            int priority,
            bool instantPlayback)
        {
            if (target == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceFactory] " +
                    "Cannot create a state source for a null target.",
                    this
                );

                return null;
            }


            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceFactory] " +
                    "Cannot create a state source with an empty ID.",
                    this
                );

                return null;
            }


            ResolveRegistry();


            if (registry == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceFactory] " +
                    "Registry is not available.",
                    this
                );

                return null;
            }


            string normalizedID =
                sourceID.Trim();


            if (
                registry.IsRegistered(
                    normalizedID
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceFactory] " +
                    "A state source with ID '" +
                    normalizedID +
                    "' is already registered.",
                    this
                );

                return null;
            }


            GameObject sourceObject =
                new GameObject(
                    "VFX State Source - " +
                    normalizedID
                );


            sourceObject.transform.SetParent(
                target,
                false
            );


            SparkVFXUIStateSource source =
                sourceObject.AddComponent<
                    SparkVFXUIStateSource
                >();


            ConfigureSource(
                source,
                normalizedID,
                eventType,
                priority,
                instantPlayback
            );


            if (
                !registry.Register(
                    source
                )
            )
            {
                DestroyRuntimeObject(
                    sourceObject
                );

                return null;
            }


            generatedSourceIndex++;


            return new SparkVFXUIStateSourceHandle(
                source
            );
        }


        // ============================================================
        // CONFIGURE
        // ============================================================

        private void ConfigureSource(
            SparkVFXUIStateSource source,
            string sourceID,
            SparkVFXEventType eventType,
            int priority,
            bool instantPlayback)
        {
            if (source == null)
            {
                return;
            }


            // --------------------------------------------------------
            // IMPORTANT
            // --------------------------------------------------------
            // SparkVFXUIStateSource currently exposes:
            //
            // SetState(eventType, priority)
            // SetInstantPlayback(bool)
            //
            // Therefore configuration is performed through
            // the public API instead of accessing private fields.
            // --------------------------------------------------------

            source.SetState(
                eventType,
                priority
            );


            source.SetInstantPlayback(
                instantPlayback
            );
        }


        // ============================================================
        // CREATE UNIQUE
        // ============================================================

        public SparkVFXUIStateSourceHandle CreateUnique(
            string prefix)
        {
            if (
                string.IsNullOrWhiteSpace(
                    prefix
                )
            )
            {
                prefix =
                    "RuntimeSource";
            }


            string sourceID;


            do
            {
                generatedSourceIndex++;


                sourceID =
                    prefix.Trim() +
                    "_" +
                    generatedSourceIndex;
            }
            while (
                registry != null &&
                registry.IsRegistered(
                    sourceID
                )
            );


            return Create(
                sourceID
            );
        }


        // ============================================================
        // DESTROY HANDLE
        // ============================================================

        public void Destroy(
            SparkVFXUIStateSourceHandle handle)
        {
            if (handle == null)
            {
                return;
            }


            SparkVFXUIStateSource source =
                handle.Source;


            if (source == null)
            {
                handle.Release();

                return;
            }


            Destroy(
                source
            );


            handle.Release();
        }


        // ============================================================
        // DESTROY SOURCE
        // ============================================================

        public void Destroy(
            SparkVFXUIStateSource source)
        {
            if (source == null)
            {
                return;
            }


            ResolveRegistry();


            if (registry != null)
            {
                registry.Unregister(
                    source
                );
            }


            GameObject sourceObject =
                source.gameObject;


            DestroyRuntimeObject(
                sourceObject
            );
        }


        // ============================================================
        // DESTROY BY ID
        // ============================================================

        public bool Destroy(
            string sourceID)
        {
            ResolveRegistry();


            if (registry == null)
            {
                return false;
            }


            SparkVFXUIStateSource source =
                registry.GetSource(
                    sourceID
                );


            if (source == null)
            {
                return false;
            }


            Destroy(
                source
            );


            return true;
        }


        // ============================================================
        // DESTROY RUNTIME OBJECT
        // ============================================================

        private void DestroyRuntimeObject(
            GameObject target)
        {
            if (target == null)
            {
                return;
            }


#if UNITY_EDITOR

            if (
                !Application.isPlaying
            )
            {
                DestroyImmediate(
                    target
                );

                return;
            }

#endif

            Destroy(
                target
            );
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarnings = true)
        {
            ResolveRegistry();


            if (registry == null)
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSourceFactory] " +
                        "SparkVFXUIStateSourceRegistry is missing.",
                        this
                    );
                }


                return false;
            }


            return registry.Validate(
                logWarnings
            );
        }


#if UNITY_EDITOR

        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

        [ContextMenu(
            "Validate State Source Factory"
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
                    "[SparkVFXUIStateSourceFactory] " +
                    "Validation successful.",
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUIStateSourceFactory] " +
                    "Validation failed.",
                    this
                );
            }
        }

#endif
    }
}