using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Central manager for Spark VFX UI state source pools.
    ///
    /// Responsibilities:
    /// - Stores multiple named pools.
    /// - Resolves pools by ID.
    /// - Acquires sources from a selected pool.
    /// - Releases sources back to their originating pool.
    /// - Provides centralized pool validation.
    ///
    /// Does NOT:
    /// - Apply VFX.
    /// - Resolve VFX profiles.
    /// - Control layered state machines.
    /// - Modify materials.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIStateSourcePoolManager
        : MonoBehaviour
    {
        // ============================================================
        // POOL ENTRY
        // ============================================================

        [Serializable]
        public sealed class PoolEntry
        {
            [SerializeField]
            private string poolID = "Default";


            [SerializeField]
            private SparkVFXUIStateSourcePool pool;


            public string PoolID
            {
                get
                {
                    return poolID;
                }
            }


            public SparkVFXUIStateSourcePool Pool
            {
                get
                {
                    return pool;
                }
            }


            public string GetNormalizedID()
            {
                if (
                    string.IsNullOrWhiteSpace(
                        poolID
                    )
                )
                {
                    return string.Empty;
                }


                return poolID.Trim();
            }
        }


        // ============================================================
        // POOLS
        // ============================================================

        [Header("Pools")]

        [Tooltip(
            "Named Spark VFX UI state source pools."
        )]
        [SerializeField]
        private List<PoolEntry> pools =
            new List<PoolEntry>();


        // ============================================================
        // DEFAULT POOL
        // ============================================================

        [Header("Default Pool")]

        [SerializeField]
        private string defaultPoolID =
            "Default";


        // ============================================================
        // CACHE
        // ============================================================

        private Dictionary<
            string,
            SparkVFXUIStateSourcePool
        > poolCache;


        private bool cacheBuilt;


        // ============================================================
        // AWAKE
        // ============================================================

        private void Awake()
        {
            BuildCache();
        }


        // ============================================================
        // ENABLE
        // ============================================================

        private void OnEnable()
        {
            BuildCache();
        }


        // ============================================================
        // BUILD CACHE
        // ============================================================

        public void BuildCache()
        {
            poolCache =
                new Dictionary<
                    string,
                    SparkVFXUIStateSourcePool
                >(
                    StringComparer.OrdinalIgnoreCase
                );


            if (pools == null)
            {
                cacheBuilt =
                    true;

                return;
            }


            for (
                int i = 0;
                i < pools.Count;
                i++
            )
            {
                PoolEntry entry =
                    pools[i];


                if (entry == null)
                {
                    continue;
                }


                string poolID =
                    entry.GetNormalizedID();


                if (
                    string.IsNullOrWhiteSpace(
                        poolID
                    )
                )
                {
                    continue;
                }


                if (entry.Pool == null)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSourcePoolManager] " +
                        "Pool '" +
                        poolID +
                        "' has no assigned pool.",
                        this
                    );

                    continue;
                }


                if (
                    poolCache.ContainsKey(
                        poolID
                    )
                )
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSourcePoolManager] " +
                        "Duplicate pool ID: " +
                        poolID +
                        ". First pool will be used.",
                        this
                    );

                    continue;
                }


                poolCache.Add(
                    poolID,
                    entry.Pool
                );
            }


            cacheBuilt =
                true;
        }


        // ============================================================
        // ENSURE CACHE
        // ============================================================

        private void EnsureCache()
        {
            if (
                cacheBuilt &&
                poolCache != null
            )
            {
                return;
            }


            BuildCache();
        }


        // ============================================================
        // RESOLVE POOL
        // ============================================================

        public SparkVFXUIStateSourcePool
            ResolvePool(
                string poolID)
        {
            if (
                string.IsNullOrWhiteSpace(
                    poolID
                )
            )
            {
                return null;
            }


            EnsureCache();


            SparkVFXUIStateSourcePool pool;


            if (
                poolCache.TryGetValue(
                    poolID.Trim(),
                    out pool
                )
            )
            {
                return pool;
            }


            return null;
        }


        // ============================================================
        // DEFAULT POOL
        // ============================================================

        public SparkVFXUIStateSourcePool
            ResolveDefaultPool()
        {
            SparkVFXUIStateSourcePool pool =
                ResolvePool(
                    defaultPoolID
                );


            if (pool != null)
            {
                return pool;
            }


            EnsureCache();


            foreach (
                KeyValuePair<
                    string,
                    SparkVFXUIStateSourcePool
                > pair
                in poolCache
            )
            {
                if (pair.Value != null)
                {
                    return pair.Value;
                }
            }


            return null;
        }


        // ============================================================
        // HAS POOL
        // ============================================================

        public bool HasPool(
            string poolID)
        {
            return
                ResolvePool(
                    poolID
                ) != null;
        }


        // ============================================================
        // POOL COUNT
        // ============================================================

        public int PoolCount
        {
            get
            {
                EnsureCache();

                return poolCache.Count;
            }
        }


        // ============================================================
        // ACQUIRE
        // ============================================================

        public SparkVFXUIStateSourceHandle
            Acquire(
                string poolID,
                string sourceID)
        {
            SparkVFXUIStateSourcePool pool =
                ResolvePool(
                    poolID
                );


            if (pool == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourcePoolManager] " +
                    "Pool not found: " +
                    poolID,
                    this
                );

                return null;
            }


            return
                pool.Acquire(
                    sourceID
                );
        }


        // ============================================================
        // ACQUIRE DEFAULT
        // ============================================================

        public SparkVFXUIStateSourceHandle
            Acquire(
                string sourceID)
        {
            SparkVFXUIStateSourcePool pool =
                ResolveDefaultPool();


            if (pool == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourcePoolManager] " +
                    "No default pool is available.",
                    this
                );

                return null;
            }


            return
                pool.Acquire(
                    sourceID
                );
        }


        // ============================================================
        // ACQUIRE FOR TARGET
        // ============================================================

        public SparkVFXUIStateSourceHandle
            AcquireFor(
                string poolID,
                Transform target,
                string sourceID)
        {
            SparkVFXUIStateSourcePool pool =
                ResolvePool(
                    poolID
                );


            if (pool == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourcePoolManager] " +
                    "Pool not found: " +
                    poolID,
                    this
                );

                return null;
            }


            return
                pool.AcquireFor(
                    target,
                    sourceID
                );
        }


        // ============================================================
        // ACQUIRE FOR TARGET - DEFAULT
        // ============================================================

        public SparkVFXUIStateSourceHandle
            AcquireFor(
                Transform target,
                string sourceID)
        {
            SparkVFXUIStateSourcePool pool =
                ResolveDefaultPool();


            if (pool == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourcePoolManager] " +
                    "No default pool is available.",
                    this
                );

                return null;
            }


            return
                pool.AcquireFor(
                    target,
                    sourceID
                );
        }


        // ============================================================
        // RELEASE
        // ============================================================

        public void Release(
            string poolID,
            SparkVFXUIStateSourceHandle handle)
        {
            if (handle == null)
            {
                return;
            }


            SparkVFXUIStateSourcePool pool =
                ResolvePool(
                    poolID
                );


            if (pool == null)
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourcePoolManager] " +
                    "Cannot release handle. " +
                    "Pool not found: " +
                    poolID,
                    this
                );

                return;
            }


            pool.Release(
                handle
            );
        }


        // ============================================================
        // RELEASE ALL FROM POOL
        // ============================================================

        public void ReleaseAll(
            string poolID)
        {
            SparkVFXUIStateSourcePool pool =
                ResolvePool(
                    poolID
                );


            if (pool == null)
            {
                return;
            }


            pool.ReleaseAll();
        }


        // ============================================================
        // RELEASE ALL POOLS
        // ============================================================

        public void ReleaseAll()
        {
            EnsureCache();


            foreach (
                KeyValuePair<
                    string,
                    SparkVFXUIStateSourcePool
                > pair
                in poolCache
            )
            {
                if (pair.Value == null)
                {
                    continue;
                }


                pair.Value.ReleaseAll();
            }
        }


        // ============================================================
        // REFRESH
        // ============================================================

        public void Refresh()
        {
            cacheBuilt =
                false;


            poolCache =
                null;


            BuildCache();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarnings = true)
        {
            bool valid =
                true;


            EnsureCache();


            if (
                pools == null ||
                pools.Count == 0
            )
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSourcePoolManager] " +
                        "No pools are configured.",
                        this
                    );
                }


                return false;
            }


            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


            for (
                int i = 0;
                i < pools.Count;
                i++
            )
            {
                PoolEntry entry =
                    pools[i];


                if (entry == null)
                {
                    valid =
                        false;


                    if (logWarnings)
                    {
                        Debug.LogWarning(
                            "[SparkVFXUIStateSourcePoolManager] " +
                            "Pool entry at index " +
                            i +
                            " is null.",
                            this
                        );
                    }


                    continue;
                }


                string poolID =
                    entry.GetNormalizedID();


                if (
                    string.IsNullOrWhiteSpace(
                        poolID
                    )
                )
                {
                    valid =
                        false;


                    if (logWarnings)
                    {
                        Debug.LogWarning(
                            "[SparkVFXUIStateSourcePoolManager] " +
                            "Pool entry at index " +
                            i +
                            " has an empty ID.",
                            this
                        );
                    }
                }


                if (
                    !ids.Add(
                        poolID
                    )
                )
                {
                    valid =
                        false;


                    if (logWarnings)
                    {
                        Debug.LogWarning(
                            "[SparkVFXUIStateSourcePoolManager] " +
                            "Duplicate pool ID: " +
                            poolID,
                            this
                        );
                    }
                }


                if (entry.Pool == null)
                {
                    valid =
                        false;


                    if (logWarnings)
                    {
                        Debug.LogWarning(
                            "[SparkVFXUIStateSourcePoolManager] " +
                            "Pool '" +
                            poolID +
                            "' has no pool reference.",
                            this
                        );
                    }
                }
            }


            if (
                !string.IsNullOrWhiteSpace(
                    defaultPoolID
                ) &&
                !ids.Contains(
                    defaultPoolID.Trim()
                )
            )
            {
                valid =
                    false;


                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSourcePoolManager] " +
                        "Default pool '" +
                        defaultPoolID +
                        "' does not exist.",
                        this
                    );
                }
            }


            return valid;
        }


#if UNITY_EDITOR

        // ============================================================
        // VALIDATE CONTEXT MENU
        // ============================================================

        [ContextMenu(
            "Validate Pool Manager"
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
                    "[SparkVFXUIStateSourcePoolManager] " +
                    "Validation successful. " +
                    "Pools: " +
                    PoolCount,
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUIStateSourcePoolManager] " +
                    "Validation failed.",
                    this
                );
            }
        }


        // ============================================================
        // REFRESH CONTEXT MENU
        // ============================================================

        [ContextMenu(
            "Refresh Pool Cache"
        )]
        private void RefreshFromContextMenu()
        {
            Refresh();


            Debug.Log(
                "[SparkVFXUIStateSourcePoolManager] " +
                "Pool cache refreshed. " +
                "Pools: " +
                PoolCount,
                this
            );
        }

#endif


        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

        private void OnValidate()
        {
            cacheBuilt =
                false;


            poolCache =
                null;
        }
    }
}