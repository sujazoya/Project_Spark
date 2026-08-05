using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Central registry for SparkVFXUIStateSource components.
    ///
    /// Responsibilities:
    /// - Registers state sources.
    /// - Unregisters state sources.
    /// - Provides lookup by source ID.
    /// - Tracks active sources.
    /// - Provides source enumeration.
    /// - Prevents duplicate registrations.
    /// - Supports runtime debugging.
    ///
    /// This registry does NOT:
    /// - Resolve VFX profiles.
    /// - Play VFX directly.
    /// - Modify materials.
    /// - Replace SparkVFXUIStateCoordinator.
    /// - Resolve state priority.
    ///
    /// Priority resolution remains the responsibility of:
    /// SparkVFXUIStateCoordinator.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXUIStateSourceRegistry
        : MonoBehaviour
    {
        // ============================================================
        // AUTO REGISTER
        // ============================================================

        [Header("Registration")]

        [Tooltip(
            "Automatically searches this GameObject hierarchy " +
            "and registers SparkVFXUIStateSource components."
        )]
        [SerializeField]
        private bool autoRegisterChildren = true;


        [Tooltip(
            "Automatically registers sources that are enabled."
        )]
        [SerializeField]
        private bool registerOnEnable = true;


        // ============================================================
        // DUPLICATE POLICY
        // ============================================================

        [Header("Duplicate Policy")]

        [Tooltip(
            "If enabled, a duplicate source ID replaces the " +
            "previous registered source."
        )]
        [SerializeField]
        private bool replaceDuplicateSource = false;


        // ============================================================
        // RUNTIME CACHE
        // ============================================================

        private readonly Dictionary<
            string,
            SparkVFXUIStateSource
        > sourceCache =
            new Dictionary<
                string,
                SparkVFXUIStateSource
            >(
                StringComparer.Ordinal
            );


        // ============================================================
        // LIFECYCLE
        // ============================================================

        private void Awake()
        {
            if (autoRegisterChildren)
            {
                RegisterHierarchy();
            }
        }


        private void OnEnable()
        {
            if (registerOnEnable)
            {
                RegisterHierarchy();
            }
        }


        private void OnDisable()
        {
            ClearRegistry();
        }


        // ============================================================
        // SOURCE COUNT
        // ============================================================

        public int SourceCount
        {
            get
            {
                CleanupInvalidSources();

                return sourceCache.Count;
            }
        }


        // ============================================================
        // REGISTER
        // ============================================================

        public bool Register(
            SparkVFXUIStateSource source)
        {
            if (source == null)
            {
                return false;
            }


            string sourceID =
                source.SourceID;


            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXUIStateSourceRegistry] " +
                    "Cannot register source with empty ID.",
                    this
                );

                return false;
            }


            sourceID =
                sourceID.Trim();


            SparkVFXUIStateSource existingSource;


            if (
                sourceCache.TryGetValue(
                    sourceID,
                    out existingSource
                )
            )
            {
                if (
                    existingSource ==
                    source
                )
                {
                    return true;
                }


                if (!replaceDuplicateSource)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSourceRegistry] " +
                        "Duplicate source ID detected: " +
                        sourceID +
                        ". Existing source was kept.",
                        this
                    );

                    return false;
                }


                sourceCache[
                    sourceID
                ] =
                    source;


                Debug.Log(
                    "[SparkVFXUIStateSourceRegistry] " +
                    "Duplicate source replaced: " +
                    sourceID,
                    this
                );


                return true;
            }


            sourceCache.Add(
                sourceID,
                source
            );


            return true;
        }


        // ============================================================
        // UNREGISTER
        // ============================================================

        public bool Unregister(
            SparkVFXUIStateSource source)
        {
            if (source == null)
            {
                return false;
            }


            string sourceID =
                source.SourceID;


            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                return false;
            }


            sourceID =
                sourceID.Trim();


            SparkVFXUIStateSource registeredSource;


            if (
                !sourceCache.TryGetValue(
                    sourceID,
                    out registeredSource
                )
            )
            {
                return false;
            }


            if (
                registeredSource !=
                source
            )
            {
                return false;
            }


            return sourceCache.Remove(
                sourceID
            );
        }


        // ============================================================
        // UNREGISTER BY ID
        // ============================================================

        public bool Unregister(
            string sourceID)
        {
            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                return false;
            }


            return sourceCache.Remove(
                sourceID.Trim()
            );
        }


        // ============================================================
        // GET SOURCE
        // ============================================================

        public SparkVFXUIStateSource GetSource(
            string sourceID)
        {
            CleanupInvalidSources();


            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                return null;
            }


            SparkVFXUIStateSource source;


            if (
                sourceCache.TryGetValue(
                    sourceID.Trim(),
                    out source
                )
            )
            {
                return source;
            }


            return null;
        }


        // ============================================================
        // TRY GET SOURCE
        // ============================================================

        public bool TryGetSource(
            string sourceID,
            out SparkVFXUIStateSource source)
        {
            source =
                GetSource(
                    sourceID
                );


            return source != null;
        }


        // ============================================================
        // REGISTER HIERARCHY
        // ============================================================

        public void RegisterHierarchy()
        {
            SparkVFXUIStateSource[] sources =
                GetComponentsInChildren<
                    SparkVFXUIStateSource
                >(
                    true
                );


            if (sources == null)
            {
                return;
            }


            for (
                int i = 0;
                i < sources.Length;
                i++
            )
            {
                SparkVFXUIStateSource source =
                    sources[i];


                if (source == null)
                {
                    continue;
                }


                Register(
                    source
                );
            }
        }


        // ============================================================
        // REFRESH
        // ============================================================

        public void Refresh()
        {
            ClearRegistry();

            RegisterHierarchy();
        }


        // ============================================================
        // CLEAR
        // ============================================================

        public void ClearRegistry()
        {
            sourceCache.Clear();
        }


        // ============================================================
        // CLEANUP
        // ============================================================

        private void CleanupInvalidSources()
        {
            if (
                sourceCache.Count ==
                0
            )
            {
                return;
            }


            List<string> invalidIDs =
                null;


            foreach (
                KeyValuePair<
                    string,
                    SparkVFXUIStateSource
                > pair
                in sourceCache
            )
            {
                if (
                    pair.Value == null
                )
                {
                    if (
                        invalidIDs ==
                        null
                    )
                    {
                        invalidIDs =
                            new List<string>();
                    }


                    invalidIDs.Add(
                        pair.Key
                    );
                }
            }


            if (
                invalidIDs == null
            )
            {
                return;
            }


            for (
                int i = 0;
                i < invalidIDs.Count;
                i++
            )
            {
                sourceCache.Remove(
                    invalidIDs[i]
                );
            }
        }


        // ============================================================
        // ACTIVATE SOURCE
        // ============================================================

        public bool ActivateSource(
            string sourceID)
        {
            SparkVFXUIStateSource source =
                GetSource(
                    sourceID
                );


            if (source == null)
            {
                return false;
            }


            source.Activate();

            return true;
        }


        // ============================================================
        // DEACTIVATE SOURCE
        // ============================================================

        public bool DeactivateSource(
            string sourceID)
        {
            SparkVFXUIStateSource source =
                GetSource(
                    sourceID
                );


            if (source == null)
            {
                return false;
            }


            source.Deactivate();

            return true;
        }


        // ============================================================
        // SET STATE
        // ============================================================

        public bool SetState(
            string sourceID,
            SparkVFXEventType eventType)
        {
            SparkVFXUIStateSource source =
                GetSource(
                    sourceID
                );


            if (source == null)
            {
                return false;
            }


            source.SetState(
                eventType
            );


            return true;
        }


        // ============================================================
        // SET STATE + PRIORITY
        // ============================================================

        public bool SetState(
            string sourceID,
            SparkVFXEventType eventType,
            int priority)
        {
            SparkVFXUIStateSource source =
                GetSource(
                    sourceID
                );


            if (source == null)
            {
                return false;
            }


            source.SetState(
                eventType,
                priority
            );


            return true;
        }


        // ============================================================
        // ACTIVATE STATE
        // ============================================================

        public bool ActivateState(
            string sourceID,
            SparkVFXEventType eventType,
            int priority)
        {
            SparkVFXUIStateSource source =
                GetSource(
                    sourceID
                );


            if (source == null)
            {
                return false;
            }


            source.SetState(
                eventType,
                priority
            );


            source.Activate();


            return true;
        }


        // ============================================================
        // DEACTIVATE STATE
        // ============================================================

        public bool DeactivateState(
            string sourceID)
        {
            return
                DeactivateSource(
                    sourceID
                );
        }


        // ============================================================
        // IS REGISTERED
        // ============================================================

        public bool IsRegistered(
            string sourceID)
        {
            CleanupInvalidSources();


            if (
                string.IsNullOrWhiteSpace(
                    sourceID
                )
            )
            {
                return false;
            }


            return sourceCache.ContainsKey(
                sourceID.Trim()
            );
        }


        // ============================================================
        // IS ACTIVE
        // ============================================================

        public bool IsActive(
            string sourceID)
        {
            SparkVFXUIStateSource source =
                GetSource(
                    sourceID
                );


            if (source == null)
            {
                return false;
            }


            return source.IsActive;
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarnings = true)
        {
            CleanupInvalidSources();


            bool valid =
                true;


            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


            SparkVFXUIStateSource[] sources =
                GetComponentsInChildren<
                    SparkVFXUIStateSource
                >(
                    true
                );


            if (
                sources == null ||
                sources.Length == 0
            )
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXUIStateSourceRegistry] " +
                        "No SparkVFXUIStateSource components " +
                        "were found in the hierarchy.",
                        this
                    );
                }


                return false;
            }


            for (
                int i = 0;
                i < sources.Length;
                i++
            )
            {
                SparkVFXUIStateSource source =
                    sources[i];


                if (source == null)
                {
                    continue;
                }


                string sourceID =
                    source.SourceID;


                if (
                    string.IsNullOrWhiteSpace(
                        sourceID
                    )
                )
                {
                    valid =
                        false;


                    if (logWarnings)
                    {
                        Debug.LogWarning(
                            "[SparkVFXUIStateSourceRegistry] " +
                            "Source at index " +
                            i +
                            " has an empty ID.",
                            this
                        );
                    }


                    continue;
                }


                sourceID =
                    sourceID.Trim();


                if (
                    !ids.Add(
                        sourceID
                    )
                )
                {
                    valid =
                        false;


                    if (logWarnings)
                    {
                        Debug.LogWarning(
                            "[SparkVFXUIStateSourceRegistry] " +
                            "Duplicate source ID detected: " +
                            sourceID,
                            this
                        );
                    }
                }


                if (
                    !source.Validate(
                        logWarnings
                    )
                )
                {
                    valid =
                        false;
                }
            }


            return valid;
        }


#if UNITY_EDITOR

        // ============================================================
        // EDITOR VALIDATION
        // ============================================================

        [ContextMenu(
            "Validate State Source Registry"
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
                    "[SparkVFXUIStateSourceRegistry] " +
                    "Validation successful. " +
                    "Registered Sources: " +
                    SourceCount,
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXUIStateSourceRegistry] " +
                    "Validation failed.",
                    this
                );
            }
        }


        // ============================================================
        // EDITOR REFRESH
        // ============================================================

        [ContextMenu(
            "Refresh State Source Registry"
        )]
        private void RefreshFromContextMenu()
        {
            Refresh();


            Debug.Log(
                "[SparkVFXUIStateSourceRegistry] " +
                "Registry refreshed. " +
                "Sources: " +
                SourceCount,
                this
            );
        }


        // ============================================================
        // EDITOR CLEAR
        // ============================================================

        [ContextMenu(
            "Clear State Source Registry"
        )]
        private void ClearFromContextMenu()
        {
            ClearRegistry();
        }

#endif
    }
}