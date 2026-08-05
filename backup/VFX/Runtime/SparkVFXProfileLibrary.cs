using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Central reusable library for Project Spark VFX profiles.
    ///
    /// Responsibilities:
    /// - Stores reusable SparkVFXProfile references.
    /// - Resolves profiles by strongly typed state.
    /// - Resolves profiles by string ID.
    /// - Supports custom profiles.
    /// - Prevents duplicate profile IDs.
    /// - Provides safe runtime lookup.
    ///
    /// This class does NOT:
    /// - Apply profiles.
    /// - Control materials.
    /// - Change VFX state.
    /// - Call SetState().
    /// - Call RequestState().
    /// - Control SparkVFXController.
    /// - Control SparkTMPVFXController.
    ///
    /// It is a data/resolution layer only.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SparkVFXProfileLibrary",
        menuName = "Project Spark/UI VFX/Profile Library"
    )]
    public sealed class SparkVFXProfileLibrary
        : ScriptableObject
    {
        // ============================================================
        // PROFILE ENTRIES
        // ============================================================

        [Header("Built-In Profiles")]

        [SerializeField]
        private SparkVFXProfile normal;


        [SerializeField]
        private SparkVFXProfile hover;


        [SerializeField]
        private SparkVFXProfile pressed;


        [SerializeField]
        private SparkVFXProfile selected;


        [SerializeField]
        private SparkVFXProfile target;


        [SerializeField]
        private SparkVFXProfile warning;


        [SerializeField]
        private SparkVFXProfile disabled;


        [SerializeField]
        private SparkVFXProfile focused;


        // ============================================================
        // CUSTOM PROFILES
        // ============================================================

        [Header("Custom Profiles")]

        [SerializeField]
        private List<SparkVFXProfileEntry> customProfiles =
            new List<SparkVFXProfileEntry>();


        // ============================================================
        // RUNTIME CACHE
        // ============================================================

        private Dictionary<
            string,
            SparkVFXProfile
        > profileCache;


        private bool cacheBuilt;


        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void OnEnable()
        {
            cacheBuilt =
                false;

            profileCache =
                null;
        }


        // ============================================================
        // PROFILE STATE
        // ============================================================

        public enum ProfileState
        {
            Normal = 0,

            Hover = 1,

            Pressed = 2,

            Selected = 3,

            Target = 4,

            Warning = 5,

            Disabled = 6,

            Focused = 7
        }


        // ============================================================
        // BUILT-IN RESOLUTION
        // ============================================================

        public SparkVFXProfile GetProfile(
            ProfileState state)
        {
            switch (state)
            {
                case ProfileState.Normal:
                    return normal;


                case ProfileState.Hover:
                    return hover;


                case ProfileState.Pressed:
                    return pressed;


                case ProfileState.Selected:
                    return selected;


                case ProfileState.Target:
                    return target;


                case ProfileState.Warning:
                    return warning;


                case ProfileState.Disabled:
                    return disabled;


                case ProfileState.Focused:
                    return focused;


                default:
                    return null;
            }
        }
        public SparkVFXProfile GetProfile(
    SparkVFXEventType eventType)
        {
            SparkVFXProfileLibrary.ProfileState state;

            if (
                !TryConvertEventToProfileState(
                    eventType,
                    out state
                )
            )
            {
                return null;
            }

            return GetProfile(
                state
            );
        }
        private bool TryConvertEventToProfileState(
    SparkVFXEventType eventType,
    out SparkVFXProfileLibrary.ProfileState profileState)
        {
            profileState =
                SparkVFXProfileLibrary.ProfileState.Normal;


            switch (eventType)
            {
                case SparkVFXEventType.HoverEnter:
                    {
                        profileState =
                            SparkVFXProfileLibrary.ProfileState.Hover;

                        return true;
                    }


                case SparkVFXEventType.Selected:
                    {
                        profileState =
                            SparkVFXProfileLibrary.ProfileState.Selected;

                        return true;
                    }


                case SparkVFXEventType.Target:
                    {
                        profileState =
                            SparkVFXProfileLibrary.ProfileState.Target;

                        return true;
                    }


                case SparkVFXEventType.Warning:
                    {
                        profileState =
                            SparkVFXProfileLibrary.ProfileState.Warning;

                        return true;
                    }


                case SparkVFXEventType.Normal:
                    {
                        profileState =
                            SparkVFXProfileLibrary.ProfileState.Normal;

                        return true;
                    }


                default:
                    {
                        return false;
                    }
            }
        }


        // ============================================================
        // STRING RESOLUTION
        // ============================================================

        public SparkVFXProfile GetProfile(
            string profileID)
        {
            if (
                string.IsNullOrWhiteSpace(
                    profileID
                )
            )
            {
                return null;
            }


            BuildCacheIfRequired();


            SparkVFXProfile profile;


            if (
                profileCache.TryGetValue(
                    profileID,
                    out profile
                )
            )
            {
                return profile;
            }


            return null;
        }


        // ============================================================
        // TRY GET
        // ============================================================

        public bool TryGetProfile(
            string profileID,
            out SparkVFXProfile profile)
        {
            profile =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    profileID
                )
            )
            {
                return false;
            }


            BuildCacheIfRequired();


            return profileCache.TryGetValue(
                profileID,
                out profile
            );
        }


        // ============================================================
        // TRY GET STATE
        // ============================================================

        public bool TryGetProfile(
            ProfileState state,
            out SparkVFXProfile profile)
        {
            profile =
                GetProfile(
                    state
                );


            return profile != null;
        }


        // ============================================================
        // NORMAL
        // ============================================================

        public SparkVFXProfile Normal
        {
            get
            {
                return normal;
            }
        }


        // ============================================================
        // HOVER
        // ============================================================

        public SparkVFXProfile Hover
        {
            get
            {
                return hover;
            }
        }


        // ============================================================
        // PRESSED
        // ============================================================

        public SparkVFXProfile Pressed
        {
            get
            {
                return pressed;
            }
        }


        // ============================================================
        // SELECTED
        // ============================================================

        public SparkVFXProfile Selected
        {
            get
            {
                return selected;
            }
        }


        // ============================================================
        // TARGET
        // ============================================================

        public SparkVFXProfile Target
        {
            get
            {
                return target;
            }
        }


        // ============================================================
        // WARNING
        // ============================================================

        public SparkVFXProfile Warning
        {
            get
            {
                return warning;
            }
        }


        // ============================================================
        // DISABLED
        // ============================================================

        public SparkVFXProfile Disabled
        {
            get
            {
                return disabled;
            }
        }


        // ============================================================
        // FOCUSED
        // ============================================================

        public SparkVFXProfile Focused
        {
            get
            {
                return focused;
            }
        }


        // ============================================================
        // CUSTOM PROFILES
        // ============================================================

        public IReadOnlyList<SparkVFXProfileEntry>
            CustomProfiles
        {
            get
            {
                return customProfiles;
            }
        }


        // ============================================================
        // CACHE
        // ============================================================

        private void BuildCacheIfRequired()
        {
            if (
                cacheBuilt &&
                profileCache != null
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
            profileCache =
                new Dictionary<
                    string,
                    SparkVFXProfile
                >(
                    StringComparer.OrdinalIgnoreCase
                );


            // --------------------------------------------------------
            // BUILT-IN
            // --------------------------------------------------------

            AddToCache(
                normal
            );


            AddToCache(
                hover
            );


            AddToCache(
                pressed
            );


            AddToCache(
                selected
            );


            AddToCache(
                target
            );


            AddToCache(
                warning
            );


            AddToCache(
                disabled
            );


            AddToCache(
                focused
            );


            // --------------------------------------------------------
            // CUSTOM
            // --------------------------------------------------------

            if (
                customProfiles != null
            )
            {
                for (
                    int i = 0;
                    i < customProfiles.Count;
                    i++
                )
                {
                    SparkVFXProfileEntry entry =
                        customProfiles[i];


                    if (
                        entry == null
                    )
                    {
                        continue;
                    }


                    if (
                        entry.Profile == null
                    )
                    {
                        continue;
                    }


                    string id =
                        entry.ProfileID;


                    if (
                        string.IsNullOrWhiteSpace(
                            id
                        )
                    )
                    {
                        continue;
                    }


                    AddToCache(
                        id,
                        entry.Profile
                    );
                }
            }


            cacheBuilt =
                true;
        }


        // ============================================================
        // ADD PROFILE
        // ============================================================

        private void AddToCache(
            SparkVFXProfile profile)
        {
            if (
                profile == null
            )
            {
                return;
            }


            string id =
                GetProfileID(
                    profile
                );


            if (
                string.IsNullOrWhiteSpace(
                    id
                )
            )
            {
                return;
            }


            AddToCache(
                id,
                profile
            );
        }


        // ============================================================
        // ADD CUSTOM
        // ============================================================

        private void AddToCache(
            string id,
            SparkVFXProfile profile)
        {
            if (
                string.IsNullOrWhiteSpace(
                    id
                )
            )
            {
                return;
            }


            if (
                profile == null
            )
            {
                return;
            }


            if (
                profileCache.ContainsKey(
                    id
                )
            )
            {
                Debug.LogWarning(
                    "[SparkVFXProfileLibrary] " +
                    "Duplicate profile ID detected: " +
                    id +
                    ". The first profile will remain active.",
                    this
                );


                return;
            }


            profileCache.Add(
                id,
                profile
            );
        }


        // ============================================================
        // PROFILE ID
        // ============================================================

        private string GetProfileID(
            SparkVFXProfile profile)
        {
            if (
                profile == null
            )
            {
                return null;
            }


            return profile.name;
        }


        // ============================================================
        // HAS PROFILE
        // ============================================================

        public bool HasProfile(
            string profileID)
        {
            if (
                string.IsNullOrWhiteSpace(
                    profileID
                )
            )
            {
                return false;
            }


            BuildCacheIfRequired();


            return profileCache.ContainsKey(
                profileID
            );
        }


        // ============================================================
        // COUNT
        // ============================================================

        public int ProfileCount
        {
            get
            {
                BuildCacheIfRequired();


                return profileCache.Count;
            }
        }


        // ============================================================
        // CLEAR CACHE
        // ============================================================

        public void RefreshCache()
        {
            cacheBuilt =
                false;


            profileCache =
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


            HashSet<string> ids =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


            ValidateProfile(
                normal,
                "Normal",
                ids,
                ref valid,
                logWarnings
            );


            ValidateProfile(
                hover,
                "Hover",
                ids,
                ref valid,
                logWarnings
            );


            ValidateProfile(
                pressed,
                "Pressed",
                ids,
                ref valid,
                logWarnings
            );


            ValidateProfile(
                selected,
                "Selected",
                ids,
                ref valid,
                logWarnings
            );


            ValidateProfile(
                target,
                "Target",
                ids,
                ref valid,
                logWarnings
            );


            ValidateProfile(
                warning,
                "Warning",
                ids,
                ref valid,
                logWarnings
            );


            ValidateProfile(
                disabled,
                "Disabled",
                ids,
                ref valid,
                logWarnings
            );


            ValidateProfile(
                focused,
                "Focused",
                ids,
                ref valid,
                logWarnings
            );


            if (
                customProfiles != null
            )
            {
                for (
                    int i = 0;
                    i < customProfiles.Count;
                    i++
                )
                {
                    SparkVFXProfileEntry entry =
                        customProfiles[i];


                    if (
                        entry == null
                    )
                    {
                        if (logWarnings)
                        {
                            Debug.LogWarning(
                                "[SparkVFXProfileLibrary] " +
                                "Custom profile entry " +
                                i +
                                " is null.",
                                this
                            );
                        }


                        valid =
                            false;


                        continue;
                    }


                    ValidateProfile(
                        entry.Profile,
                        entry.ProfileID,
                        ids,
                        ref valid,
                        logWarnings
                    );
                }
            }


            return valid;
        }


        // ============================================================
        // VALIDATE PROFILE
        // ============================================================

        private void ValidateProfile(
            SparkVFXProfile profile,
            string label,
            HashSet<string> ids,
            ref bool valid,
            bool logWarnings)
        {
            if (
                profile == null
            )
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXProfileLibrary] " +
                        "Missing profile: " +
                        label,
                        this
                    );
                }


                valid =
                    false;


                return;
            }


            string id =
                GetProfileID(
                    profile
                );


            if (
                string.IsNullOrWhiteSpace(
                    id
                )
            )
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXProfileLibrary] " +
                        "Profile has an empty ID: " +
                        label,
                        profile
                    );
                }


                valid =
                    false;


                return;
            }


            if (
                !ids.Add(
                    id
                )
            )
            {
                if (logWarnings)
                {
                    Debug.LogWarning(
                        "[SparkVFXProfileLibrary] " +
                        "Duplicate profile ID: " +
                        id,
                        profile
                    );
                }


                valid =
                    false;
            }
        }


        // ============================================================
        // CONTEXT VALIDATION
        // ============================================================

        [ContextMenu(
            "Validate Profile Library"
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
                    "[SparkVFXProfileLibrary] " +
                    "Validation successful. " +
                    "Profiles: " +
                    ProfileCount,
                    this
                );
            }
            else
            {
                Debug.LogError(
                    "[SparkVFXProfileLibrary] " +
                    "Validation failed.",
                    this
                );
            }
        }


        // ============================================================
        // EDITOR / SERIALIZATION
        // ============================================================

        private void OnValidate()
        {
            cacheBuilt =
                false;


            profileCache =
                null;
        }
    }


    // ================================================================
    // PROFILE ENTRY
    // ================================================================

    [Serializable]
    public sealed class SparkVFXProfileEntry
    {
        [SerializeField]
        private string profileID;


        [SerializeField]
        private SparkVFXProfile profile;


        // ============================================================
        // PUBLIC
        // ============================================================

        public string ProfileID
        {
            get
            {
                return profileID;
            }
        }


        public SparkVFXProfile Profile
        {
            get
            {
                return profile;
            }
        }
    }
}