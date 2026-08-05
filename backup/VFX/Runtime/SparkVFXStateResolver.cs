
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Resolves logical VFX states into SparkVFXProfile instances.
    ///
    /// Responsibilities:
    /// - Stores state definitions.
    /// - Resolves states by state ID.
    /// - Resolves profiles from local state definitions.
    /// - Falls back to SparkVFXProfileLibrary when no local profile exists.
    /// - Supports priority lookup.
    /// - Validates duplicate state IDs.
    ///
    /// This class does NOT:
    /// - Apply profiles.
    /// - Control materials.
    /// - Modify shader properties.
    /// - Start or stop sequences.
    /// - Control SparkVFXController.
    /// - Control SparkTMPVFXController.
    /// - Manage state layers.
    /// - Change active VFX state.
    ///
    /// SparkVFXLayeredStateMachine is responsible for state selection
    /// and application.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXStateResolver
        : MonoBehaviour
    {
        // ============================================================
        // STATE DEFINITIONS
        // ============================================================

        [Header("State Definitions")]

        [Tooltip(
            "Logical VFX states available to this resolver."
        )]
        [SerializeField]
        private List<SparkVFXStateDefinition> states =
            new List<SparkVFXStateDefinition>();


        // ============================================================
        // PROFILE LIBRARY
        // ============================================================

        [Header("Profile Library")]

        [Tooltip(
            "Optional profile library used when a state does not " +
            "have a local profile assigned."
        )]
        [SerializeField]
        private SparkVFXProfileLibrary profileLibrary;


        // ============================================================
        // FALLBACK
        // ============================================================

        [Header("Fallback")]

        [Tooltip(
            "Optional fallback profile used when the requested state " +
            "cannot resolve a profile."
        )]
        [SerializeField]
        private SparkVFXProfile fallbackProfile;


        [Tooltip(
            "Optional fallback state ID used when a requested state " +
            "cannot be resolved."
        )]
        [SerializeField]
        private string fallbackStateID = "Normal";


        // ============================================================
        // RUNTIME CACHE
        // ============================================================

        private Dictionary<
            string,
            SparkVFXStateDefinition
        > stateCache;


        private bool cacheBuilt;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            BuildCache();
        }


        private void OnEnable()
        {
            if (!cacheBuilt)
            {
                BuildCache();
            }
        }


        private void OnValidate()
        {
            cacheBuilt =
                false;

            stateCache =
                null;
        }


        // ============================================================
        // STATES
        // ============================================================

        public IReadOnlyList<
            SparkVFXStateDefinition
        > States
        {
            get
            {
                return states;
            }
        }


        // ============================================================
        // PROFILE LIBRARY
        // ============================================================

        public SparkVFXProfileLibrary ProfileLibrary
        {
            get
            {
                return profileLibrary;
            }

            set
            {
                profileLibrary =
                    value;
            }
        }


        // ============================================================
        // FALLBACK PROFILE
        // ============================================================

        public SparkVFXProfile FallbackProfile
        {
            get
            {
                return fallbackProfile;
            }
        }


        // ============================================================
        // FALLBACK STATE ID
        // ============================================================

        public string FallbackStateID
        {
            get
            {
                return fallbackStateID;
            }
        }


        // ============================================================
        // GET STATE
        // ============================================================

        public SparkVFXStateDefinition GetState(
            string stateID)
        {
            if (
                string.IsNullOrWhiteSpace(
                    stateID
                )
            )
            {
                return null;
            }


            BuildCacheIfRequired();


            SparkVFXStateDefinition state;


            if (
                stateCache.TryGetValue(
                    NormalizeID(
                        stateID
                    ),
                    out state
                )
            )
            {
                return state;
            }


            return null;
        }


        // ============================================================
        // TRY GET STATE
        // ============================================================

        public bool TryGetState(
            string stateID,
            out SparkVFXStateDefinition state)
        {
            state =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    stateID
                )
            )
            {
                return false;
            }


            BuildCacheIfRequired();


            return stateCache.TryGetValue(
                NormalizeID(
                    stateID
                ),
                out state
            );
        }


        // ============================================================
        // HAS STATE
        // ============================================================

        public bool HasState(
            string stateID)
        {
            if (
                string.IsNullOrWhiteSpace(
                    stateID
                )
            )
            {
                return false;
            }


            BuildCacheIfRequired();


            return stateCache.ContainsKey(
                NormalizeID(
                    stateID
                )
            );
        }


        // ============================================================
        // RESOLVE PROFILE
        // ============================================================

        public SparkVFXProfile ResolveProfile(
            string stateID)
        {
            if (
                string.IsNullOrWhiteSpace(
                    stateID
                )
            )
            {
                return fallbackProfile;
            }


            SparkVFXStateDefinition state;


            if (
                !TryGetState(
                    stateID,
                    out state
                )
            )
            {
                return ResolveFallbackProfile();
            }


            // --------------------------------------------------------
            // STATE DISABLED
            // --------------------------------------------------------

            if (!state.Enabled)
            {
                return ResolveFallbackProfile();
            }


            // --------------------------------------------------------
            // LOCAL PROFILE
            // --------------------------------------------------------

            if (state.Profile != null)
            {
                return state.Profile;
            }


            // --------------------------------------------------------
            // PROFILE LIBRARY
            // --------------------------------------------------------

            if (profileLibrary != null)
            {
                SparkVFXProfile libraryProfile =
                    profileLibrary.GetProfile(
                        state.StateID
                    );


                if (libraryProfile != null)
                {
                    return libraryProfile;
                }
            }


            // --------------------------------------------------------
            // FALLBACK
            // --------------------------------------------------------

            return ResolveFallbackProfile();
        }


        // ============================================================
        // TRY RESOLVE PROFILE
        // ============================================================

        public bool TryResolveProfile(
            string stateID,
            out SparkVFXProfile profile)
        {
            profile =
                ResolveProfile(
                    stateID
                );


            return profile != null;
        }


        // ============================================================
        // RESOLVE STATE + PROFILE
        // ============================================================

        public bool TryResolve(
            string stateID,
            out SparkVFXStateDefinition state,
            out SparkVFXProfile profile)
        {
            state =
                null;


            profile =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    stateID
                )
            )
            {
                return false;
            }


            if (
                !TryGetState(
                    stateID,
                    out state
                )
            )
            {
                return false;
            }


            if (!state.Enabled)
            {
                return false;
            }


            profile =
                ResolveProfile(
                    stateID
                );


            return profile != null;
        }


        // ============================================================
        // RESOLVE FALLBACK
        // ============================================================

        public SparkVFXProfile ResolveFallbackProfile()
        {
            // --------------------------------------------------------
            // DIRECT FALLBACK PROFILE
            // --------------------------------------------------------

            if (fallbackProfile != null)
            {
                return fallbackProfile;
            }


            // --------------------------------------------------------
            // FALLBACK STATE
            // --------------------------------------------------------

            if (
                !string.IsNullOrWhiteSpace(
                    fallbackStateID
                )
            )
            {
                SparkVFXStateDefinition fallbackState;


                if (
                    TryGetState(
                        fallbackStateID,
                        out fallbackState
                    )
                )
                {
                    if (
                        fallbackState != null &&
                        fallbackState.Enabled
                    )
                    {
                        if (
                            fallbackState.Profile != null
                        )
                        {
                            return fallbackState.Profile;
                        }


                        if (
                            profileLibrary != null
                        )
                        {
                            SparkVFXProfile libraryProfile =
                                profileLibrary.GetProfile(
                                    fallbackState.StateID
                                );


                            if (
                                libraryProfile != null
                            )
                            {
                                return libraryProfile;
                            }
                        }
                    }
                }


                // ----------------------------------------------------
                // DIRECT LIBRARY LOOKUP
                // ----------------------------------------------------

                if (
                    profileLibrary != null
                )
                {
                    SparkVFXProfile libraryProfile =
                        profileLibrary.GetProfile(
                            fallbackStateID
                        );


                    if (
                        libraryProfile != null
                    )
                    {
                        return libraryProfile;
                    }
                }
            }


            return null;
        }
        // ============================================================
        // RESOLVE STATE
        // ============================================================

        /// <summary>
        /// Resolves a logical VFX state by its state ID.
        ///
        /// This method is an alias for GetState() and is provided
        /// for compatibility with systems such as
        /// SparkVFXLayeredStateMachine.
        ///
        /// This method only resolves the state definition.
        /// It does not resolve or apply a profile.
        /// </summary>
        public SparkVFXStateDefinition ResolveState(
            string stateID)
        {
            return GetState(
                stateID
            );
        }


        // ============================================================
        // RESOLVE HIGHEST PRIORITY
        // ============================================================

        public SparkVFXStateDefinition ResolveHighestPriorityState()
        {
            BuildCacheIfRequired();


            SparkVFXStateDefinition result =
                null;


            if (
                states == null
            )
            {
                return null;
            }


            for (
                int i = 0;
                i < states.Count;
                i++
            )
            {
                SparkVFXStateDefinition state =
                    states[i];


                if (
                    state == null
                )
                {
                    continue;
                }


                if (
                    !state.Enabled
                )
                {
                    continue;
                }


                if (
                    result == null ||
                    state.Priority >
                    result.Priority
                )
                {
                    result =
                        state;
                }
            }


            return result;
        }


        // ============================================================
        // RESOLVE HIGHEST PRIORITY PROFILE
        // ============================================================

        public SparkVFXProfile ResolveHighestPriorityProfile()
        {
            SparkVFXStateDefinition state =
                ResolveHighestPriorityState();


            if (
                state == null
            )
            {
                return ResolveFallbackProfile();
            }


            return ResolveProfile(
                state.StateID
            );
        }


        // ============================================================
        // CACHE
        // ============================================================

        private void BuildCacheIfRequired()
        {
            if (
                cacheBuilt &&
                stateCache != null
            )
            {
                return;
            }


            BuildCache();
        }


        // ============================================================
        // BUILD CACHE
        // ============================================================

        public void BuildCache()
        {
            stateCache =
                new Dictionary<
                    string,
                    SparkVFXStateDefinition
                >(
                    StringComparer.OrdinalIgnoreCase
                );


            if (
                states != null
            )
            {
                for (
                    int i = 0;
                    i < states.Count;
                    i++
                )
                {
                    SparkVFXStateDefinition state =
                        states[i];


                    if (
                        state == null
                    )
                    {
                        continue;
                    }


                    string id =
                        state.GetNormalizedID();


                    if (
                        string.IsNullOrWhiteSpace(
                            id
                        )
                    )
                    {
                        continue;
                    }


                    if (
                        stateCache.ContainsKey(
                            id
                        )
                    )
                    {
                        Debug.LogWarning(
                            "[SparkVFXStateResolver] " +
                            "Duplicate state ID detected: " +
                            id +
                            ". The first state will remain active.",
                            this
                        );


                        continue;
                    }


                    stateCache.Add(
                        id,
                        state
                    );
                }
            }


            cacheBuilt =
                true;
        }


        // ============================================================
        // REFRESH CACHE
        // ============================================================

        public void Refresh()
        {
            cacheBuilt =
                false;


            stateCache =
                null;


            BuildCache();
        }


        // ============================================================
        // STATE COUNT
        // ============================================================

        public int StateCount
        {
            get
            {
                BuildCacheIfRequired();


                return stateCache.Count;
            }
        }


        // ============================================================
        // NORMALIZE ID
        // ============================================================

        private string NormalizeID(
            string id)
        {
            if (
                string.IsNullOrWhiteSpace(
                    id
                )
            )
            {
                return string.Empty;
            }


            return id.Trim();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        public bool Validate(
            bool logWarnings = true)
        {
            bool valid =
                true;


            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


            if (
                states == null ||
                states.Count == 0
            )
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXStateResolver] " +
                        "No state definitions assigned.",
                        this
                    );
                }


                return false;
            }


            for (
                int i = 0;
                i < states.Count;
                i++
            )
            {
                SparkVFXStateDefinition state =
                    states[i];


                if (
                    state == null
                )
                {
                    valid =
                        false;


                    if (logWarnings)
                    {
                        Debug.LogWarning(
                            "[SparkVFXStateResolver] " +
                            "State definition at index " +
                            i +
                            " is null.",
                            this
                        );
                    }


                    continue;
                }


                if (
                    !state.Validate(
                        logWarnings
                    )
                )
                {
                    valid =
                        false;
                }


                string id =
                    state.GetNormalizedID();


                if (
                    string.IsNullOrWhiteSpace(
                        id
                    )
                )
                {
                    continue;
                }


                if (
                    !ids.Add(
                        id
                    )
                )
                {
                    valid =
                        false;


                    if (logWarnings)
                    {
                        Debug.LogWarning(
                            "[SparkVFXStateResolver] " +
                            "Duplicate state ID: " +
                            id,
                            this
                        );
                    }
                }
            }


            // --------------------------------------------------------
            // FALLBACK VALIDATION
            // --------------------------------------------------------

            if (
                fallbackProfile == null &&
                string.IsNullOrWhiteSpace(
                    fallbackStateID
                )
            )
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXStateResolver] " +
                        "No fallback profile or fallback state ID " +
                        "has been configured.",
                        this
                    );
                }
            }


            return valid;
        }


        // ============================================================
        // CONTEXT MENU
        // ============================================================

        [ContextMenu(
            "Validate State Resolver"
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
                    "[SparkVFXStateResolver] " +
                    "Validation successful. " +
                    "States: " +
                    StateCount,
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXStateResolver] " +
                    "Validation failed.",
                    this
                );
            }
        }
    }
}

