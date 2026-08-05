using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Pool for runtime SparkVFXUIStateSource objects.
    ///
    /// Purpose:
    /// - Avoid repeated Instantiate/Destroy operations.
    /// - Reuse dynamic UI VFX state sources.
    /// - Support temporary UI targets.
    /// - Support tutorial markers.
    /// - Support target indicators.
    /// - Support dynamically spawned UI elements.
    ///
    /// Lifecycle:
    ///
    /// Acquire
    ///     ↓
    /// Create / Reuse Source
    ///     ↓
    /// Register
    ///     ↓
    /// Use
    ///     ↓
    /// Release
    ///     ↓
    /// Unregister
    ///     ↓
    /// Return to Pool
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIStateSourcePool
        : MonoBehaviour
    {
        // ============================================================
        // REGISTRY
        // ============================================================

        [Header("Registry")]

        [Tooltip(
            "Registry used to register active state sources."
        )]
        [SerializeField]
        private SparkVFXUIStateSourceRegistry registry;


        // ============================================================
        // SOURCE PREFAB
        // ============================================================

        [Header("Source Prefab")]

        [Tooltip(
            "Optional prefab containing a SparkVFXUIStateSource. " +
            "If empty, a runtime GameObject will be created."
        )]
        [SerializeField]
        private SparkVFXUIStateSource sourcePrefab;


        // ============================================================
        // POOL PARENT
        // ============================================================

        [Header("Pool")]

        [Tooltip(
            "Parent transform used for pooled inactive sources."
        )]
        [SerializeField]
        private Transform poolParent;


        [Tooltip(
            "Initial number of sources created when the pool initializes."
        )]
        [Min(0)]
        [SerializeField]
        private int initialSize;


        [Tooltip(
            "Maximum number of inactive sources kept in the pool."
        )]
        [Min(0)]
        [SerializeField]
        private int maxPoolSize = 32;


        [Tooltip(
            "Automatically create a source when the pool is empty."
        )]
        [SerializeField]
        private bool expandable = true;


        // ============================================================
        // RUNTIME POOL
        // ============================================================

        private readonly Stack<
            SparkVFXUIStateSource
        > availableSources =
            new Stack<
                SparkVFXUIStateSource
            >();


        private readonly HashSet<
            SparkVFXUIStateSource
        > activeSources =
            new HashSet<
                SparkVFXUIStateSource
            >();


        private int generatedIndex;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            ResolveRegistry();

            ResolvePoolParent();

            Prewarm();
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
        // ACTIVE COUNT
        // ============================================================

        public int ActiveCount
        {
            get
            {
                return activeSources.Count;
            }
        }


        // ============================================================
        // AVAILABLE COUNT
        // ============================================================

        public int AvailableCount
        {
            get
            {
                return availableSources.Count;
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
        // RESOLVE POOL PARENT
        // ============================================================

        private void ResolvePoolParent()
        {
            if (poolParent != null)
            {
                return;
            }


            GameObject poolObject =
                new GameObject(
                    "Spark VFX State Source Pool"
                );


            poolObject.transform.SetParent(
                transform,
                false
            );


            poolParent =
                poolObject.transform;
        }


        // ============================================================
        // PREWARM
        // ============================================================

        public void Prewarm()
        {
            if (initialSize <= 0)
            {
                return;
            }


            for (
                int i = availableSources.Count;
                i < initialSize;
                i++
            )
            {
                SparkVFXUIStateSource source =
                    CreateSource();


                if (source == null)
                {
                    break;
                }


                ReturnToPool(
                    source
                );
            }
        }


        // ============================================================
        // ACQUIRE
        // ============================================================

        public SparkVFXUIStateSourceHandle Acquire(
            string sourceID)
        {
            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourcePool] " +
                    "Cannot acquire a source with an empty ID.",
                    this
                );

                return null;
            }


            ResolveRegistry();


            if (registry == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourcePool] " +
                    "Registry is not assigned.",
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
                    "[SparkVFXUIStateSourcePool] " +
                    "Source ID already registered: " +
                    normalizedID,
                    this
                );

                return null;
            }


            SparkVFXUIStateSource source =
                GetAvailableSource();


            if (source == null)
            {
                if (!expandable)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSourcePool] " +
                        "Pool is empty and expansion is disabled.",
                        this
                    );

                    return null;
                }


                source =
                    CreateSource();
            }


            if (source == null)
            {
                return null;
            }


            PrepareSource(
                source,
                normalizedID
            );


            if (
                !registry.Register(
                    source
                )
            )
            {
                ReturnToPool(
                    source
                );

                return null;
            }


            activeSources.Add(
                source
            );


            return new SparkVFXUIStateSourceHandle(
                source
            );
        }


        // ============================================================
        // ACQUIRE FOR TARGET
        // ============================================================

        public SparkVFXUIStateSourceHandle AcquireFor(
            Transform target,
            string sourceID)
        {
            SparkVFXUIStateSourceHandle handle =
                Acquire(
                    sourceID
                );


            if (handle == null)
            {
                return null;
            }


            SparkVFXUIStateSource source =
                handle.Source;


            if (source == null)
            {
                Release(
                    handle
                );

                return null;
            }


            if (target != null)
            {
                source.transform.SetParent(
                    target,
                    false
                );
            }


            return handle;
        }


        // ============================================================
        // GET AVAILABLE
        // ============================================================

        private SparkVFXUIStateSource
            GetAvailableSource()
        {
            while (
                availableSources.Count > 0
            )
            {
                SparkVFXUIStateSource source =
                    availableSources.Pop();


                if (source == null)
                {
                    continue;
                }


                source.gameObject.SetActive(
                    true
                );


                return source;
            }


            return null;
        }


        // ============================================================
        // CREATE SOURCE
        // ============================================================

        private SparkVFXUIStateSource
            CreateSource()
        {
            SparkVFXUIStateSource source;


            if (sourcePrefab != null)
            {
                source =
                    Instantiate(
                        sourcePrefab,
                        poolParent
                    );


                source.name =
                    "Pooled VFX State Source";
            }
            else
            {
                GameObject sourceObject =
                    new GameObject(
                        "Pooled VFX State Source"
                    );


                sourceObject.transform.SetParent(
                    poolParent,
                    false
                );


                source =
                    sourceObject.AddComponent<
                        SparkVFXUIStateSource
                    >();
            }


            if (source == null)
            {
                return null;
            }


            source.gameObject.SetActive(
                false
            );


            return source;
        }


        // ============================================================
        // PREPARE SOURCE
        // ============================================================

        private void PrepareSource(
            SparkVFXUIStateSource source,
            string sourceID)
        {
            if (source == null)
            {
                return;
            }


            generatedIndex++;


            source.name =
                "VFX State Source - " +
                sourceID +
                "_" +
                generatedIndex;


            source.gameObject.SetActive(
                true
            );
        }


        // ============================================================
        // RELEASE HANDLE
        // ============================================================

        public void Release(
            SparkVFXUIStateSourceHandle handle)
        {
            if (handle == null)
            {
                return;
            }


            SparkVFXUIStateSource source =
                handle.Source;


            Release(
                source
            );


            handle.Release();
        }


        // ============================================================
        // RELEASE SOURCE
        // ============================================================

        public void Release(
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


            activeSources.Remove(
                source
            );


            ResetSource(
                source
            );


            if (
                maxPoolSize > 0 &&
                availableSources.Count >=
                maxPoolSize
            )
            {
                DestroySource(
                    source
                );

                return;
            }


            ReturnToPool(
                source
            );
        }


        // ============================================================
        // RESET SOURCE
        // ============================================================

        private void ResetSource(
            SparkVFXUIStateSource source)
        {
            if (source == null)
            {
                return;
            }


            source.transform.SetParent(
                poolParent,
                false
            );


            source.transform.localPosition =
                Vector3.zero;


            source.transform.localRotation =
                Quaternion.identity;


            source.transform.localScale =
                Vector3.one;


            source.gameObject.SetActive(
                false
            );
        }


        // ============================================================
        // RETURN TO POOL
        // ============================================================

        private void ReturnToPool(
            SparkVFXUIStateSource source)
        {
            if (source == null)
            {
                return;
            }


            ResetSource(
                source
            );


            availableSources.Push(
                source
            );
        }


        // ============================================================
        // DESTROY SOURCE
        // ============================================================

        private void DestroySource(
            SparkVFXUIStateSource source)
        {
            if (source == null)
            {
                return;
            }


#if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                DestroyImmediate(
                    source.gameObject
                );

                return;
            }

#endif

            Destroy(
                source.gameObject
            );
        }


        // ============================================================
        // CLEAR POOL
        // ============================================================

        public void ClearPool()
        {
            while (
                availableSources.Count > 0
            )
            {
                SparkVFXUIStateSource source =
                    availableSources.Pop();


                if (source != null)
                {
                    DestroySource(
                        source
                    );
                }
            }
        }


        // ============================================================
        // RELEASE ALL
        // ============================================================

        public void ReleaseAll()
        {
            SparkVFXUIStateSource[] sources =
                new SparkVFXUIStateSource[
                    activeSources.Count
                ];


            activeSources.CopyTo(
                sources
            );


            for (
                int i = 0;
                i < sources.Length;
                i++
            )
            {
                Release(
                    sources[i]
                );
            }
        }


        // ============================================================
        // DESTROY ALL
        // ============================================================

        public void DestroyAll()
        {
            ReleaseAll();

            ClearPool();
        }


        // ============================================================
        // ON DESTROY
        // ============================================================

        private void OnDestroy()
        {
            DestroyAll();
        }


#if UNITY_EDITOR

        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

        [ContextMenu(
            "Validate State Source Pool"
        )]
        private void ValidateFromContextMenu()
        {
            ResolveRegistry();

            ResolvePoolParent();


            if (registry == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourcePool] " +
                    "Registry is not assigned.",
                    this
                );
            }
            else
            {
                Debug.Log(
                    "[SparkVFXUIStateSourcePool] " +
                    "Validation successful.\n" +
                    "Active: " +
                    ActiveCount +
                    "\nAvailable: " +
                    AvailableCount,
                    this
                );
            }
        }


        // ============================================================
        // EDITOR CLEAR
        // ============================================================

        [ContextMenu(
            "Clear Inactive Pool"
        )]
        private void ClearPoolFromContextMenu()
        {
            ClearPool();
        }

#endif
    }
}