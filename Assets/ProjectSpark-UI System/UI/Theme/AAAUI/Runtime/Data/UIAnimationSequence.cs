using System;
using UnityEngine;

namespace AAAUI
{
    /// <summary>
    /// Defines a complete UI animation sequence.
    ///
    /// A sequence contains:
    /// - Duration
    /// - Animation tracks
    /// - Timeline events
    ///
    /// Designed for the AAAUI animation framework.
    /// </summary>
    [Serializable]
    public sealed class UIAnimationSequence
    {
        // =========================================================
        // SERIALIZED DATA
        // =========================================================

        [SerializeField, Min(0f)]
        private float duration = 0.25f;

        [SerializeReference]
        private UIAnimationTrack[] tracks =
            Array.Empty<UIAnimationTrack>();

        [SerializeField]
        private UIAnimationEvent[] events =
            Array.Empty<UIAnimationEvent>();


        // =========================================================
        // PUBLIC ACCESS
        // =========================================================

        /// <summary>
        /// Total animation duration in seconds.
        /// </summary>
        public float Duration => duration;

        /// <summary>
        /// All animation tracks contained in this sequence.
        /// </summary>
        public UIAnimationTrack[] Tracks => tracks;

        /// <summary>
        /// All timeline events contained in this sequence.
        /// </summary>
        public UIAnimationEvent[] Events => events;


        // =========================================================
        // CONSTRUCTION
        // =========================================================

        public UIAnimationSequence()
        {
            duration = 0.25f;

            tracks = Array.Empty<UIAnimationTrack>();
            events = Array.Empty<UIAnimationEvent>();
        }


        // =========================================================
        // DURATION
        // =========================================================

        /// <summary>
        /// Changes the duration of the sequence.
        /// </summary>
        public void SetDuration(float value)
        {
            duration = Mathf.Max(0f, value);
        }
        /// <summary>
        /// Replaces all animation tracks in this sequence.
        /// </summary>
        public void SetTracks(UIAnimationTrack[] value)
        {
            tracks = value ?? Array.Empty<UIAnimationTrack>();
        }

        /// <summary>
        /// Returns the normalized position of a time value.
        /// </summary>
        public float NormalizeTime(float time)
        {
            if (duration <= 0f)
                return 0f;

            return Mathf.Clamp01(time / duration);
        }

        /// <summary>
        /// Converts normalized timeline position to seconds.
        /// </summary>
        public float DenormalizeTime(float normalizedTime)
        {
            return Mathf.Clamp01(normalizedTime) * duration;
        }


        // =========================================================
        // TRACK MANAGEMENT
        // =========================================================

        /// <summary>
        /// Adds a track to this sequence.
        /// </summary>
        public void AddTrack(UIAnimationTrack track)
        {
            if (track == null)
                return;

            int oldLength = tracks?.Length ?? 0;

            UIAnimationTrack[] newTracks =
                new UIAnimationTrack[oldLength + 1];

            if (oldLength > 0)
                Array.Copy(tracks, newTracks, oldLength);

            newTracks[oldLength] = track;

            tracks = newTracks;
        }

        /// <summary>
        /// Removes a specific track.
        /// </summary>
        public bool RemoveTrack(UIAnimationTrack track)
        {
            if (track == null || tracks == null)
                return false;

            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] != track)
                    continue;

                RemoveTrackAt(i);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a track at the specified index.
        /// </summary>
        public bool RemoveTrackAt(int index)
        {
            if (tracks == null)
                return false;

            if (index < 0 || index >= tracks.Length)
                return false;

            UIAnimationTrack[] newTracks =
                new UIAnimationTrack[tracks.Length - 1];

            if (index > 0)
            {
                Array.Copy(
                    tracks,
                    0,
                    newTracks,
                    0,
                    index);
            }

            if (index < tracks.Length - 1)
            {
                Array.Copy(
                    tracks,
                    index + 1,
                    newTracks,
                    index,
                    tracks.Length - index - 1);
            }

            tracks = newTracks;

            return true;
        }

        /// <summary>
        /// Removes every track.
        /// </summary>
        public void ClearTracks()
        {
            tracks = Array.Empty<UIAnimationTrack>();
        }

        /// <summary>
        /// Gets a track by index.
        /// </summary>
        public UIAnimationTrack GetTrack(int index)
        {
            if (tracks == null)
                return null;

            if (index < 0 || index >= tracks.Length)
                return null;

            return tracks[index];
        }

        /// <summary>
        /// Returns the number of tracks.
        /// </summary>
        public int TrackCount
        {
            get
            {
                return tracks?.Length ?? 0;
            }
        }


        // =========================================================
        // EVENT MANAGEMENT
        // =========================================================

        /// <summary>
        /// Adds an event to the sequence.
        /// </summary>
        public void AddEvent(UIAnimationEvent animationEvent)
        {
            int oldLength = events?.Length ?? 0;

            UIAnimationEvent[] newEvents =
                new UIAnimationEvent[oldLength + 1];

            if (oldLength > 0)
                Array.Copy(events, newEvents, oldLength);

            newEvents[oldLength] = animationEvent;

            events = newEvents;
        }

        /// <summary>
        /// Removes an event at the specified index.
        /// </summary>
        public bool RemoveEventAt(int index)
        {
            if (events == null)
                return false;

            if (index < 0 || index >= events.Length)
                return false;

            UIAnimationEvent[] newEvents =
                new UIAnimationEvent[events.Length - 1];

            if (index > 0)
            {
                Array.Copy(
                    events,
                    0,
                    newEvents,
                    0,
                    index);
            }

            if (index < events.Length - 1)
            {
                Array.Copy(
                    events,
                    index + 1,
                    newEvents,
                    index,
                    events.Length - index - 1);
            }

            events = newEvents;

            return true;
        }

        /// <summary>
        /// Removes the specified event.
        /// </summary>
        public bool RemoveEvent(UIAnimationEvent animationEvent)
        {
            if (events == null)
                return false;

            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].Equals(animationEvent))
                {
                    return RemoveEventAt(i);
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all events.
        /// </summary>
        public void ClearEvents()
        {
            events = Array.Empty<UIAnimationEvent>();
        }

        /// <summary>
        /// Gets an event by index.
        /// </summary>
        public UIAnimationEvent GetEvent(int index)
        {
            if (events == null)
                return default;

            if (index < 0 || index >= events.Length)
                return default;

            return events[index];
        }

        /// <summary>
        /// Returns the number of events.
        /// </summary>
        public int EventCount
        {
            get
            {
                return events?.Length ?? 0;
            }
        }


        // =========================================================
        // VALIDATION
        // =========================================================

        /// <summary>
        /// Removes invalid null track references.
        /// </summary>
        public void RemoveNullTracks()
        {
            if (tracks == null || tracks.Length == 0)
            {
                tracks = Array.Empty<UIAnimationTrack>();
                return;
            }

            int validCount = 0;

            for (int i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] != null)
                    validCount++;
            }

            if (validCount == tracks.Length)
                return;

            UIAnimationTrack[] validTracks =
                new UIAnimationTrack[validCount];

            int writeIndex = 0;

            for (int i = 0; i < tracks.Length; i++)
            {
                UIAnimationTrack track = tracks[i];

                if (track == null)
                    continue;

                validTracks[writeIndex] = track;
                writeIndex++;
            }

            tracks = validTracks;
        }

        /// <summary>
        /// Ensures the sequence contains valid internal arrays.
        /// </summary>
        public void EnsureValid()
        {
            if (tracks == null)
                tracks = Array.Empty<UIAnimationTrack>();

            if (events == null)
                events = Array.Empty<UIAnimationEvent>();

            duration = Mathf.Max(0f, duration);
        }


        // =========================================================
        // RESET
        // =========================================================

        /// <summary>
        /// Resets this sequence to its default state.
        /// </summary>
        public void Reset()
        {
            duration = 0.25f;

            tracks = Array.Empty<UIAnimationTrack>();
            events = Array.Empty<UIAnimationEvent>();
        }


        // =========================================================
        // TIMELINE HELPERS
        // =========================================================

        /// <summary>
        /// Returns true when the sequence has a usable duration.
        /// </summary>
        public bool HasDuration
        {
            get
            {
                return duration > 0f;
            }
        }

        /// <summary>
        /// Returns true when at least one track exists.
        /// </summary>
        public bool HasTracks
        {
            get
            {
                return tracks != null &&
                       tracks.Length > 0;
            }
        }

        /// <summary>
        /// Returns true when at least one event exists.
        /// </summary>
        public bool HasEvents
        {
            get
            {
                return events != null &&
                       events.Length > 0;
            }
        }

        /// <summary>
        /// Returns true when this sequence contains no animation data.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return !HasTracks && !HasEvents;
            }
        }


        // =========================================================
        // EDITOR / RUNTIME SAFE CLAMP
        // =========================================================

        /// <summary>
        /// Clamps a timeline time to the sequence duration.
        /// </summary>
        public float ClampTime(float time)
        {
            return Mathf.Clamp(time, 0f, duration);
        }

        /// <summary>
        /// Returns true when a time lies inside this sequence.
        /// </summary>
        public bool ContainsTime(float time)
        {
            return time >= 0f && time <= duration;
        }


        // =========================================================
        // DUPLICATION
        // =========================================================

        /// <summary>
        /// Creates a shallow sequence copy.
        ///
        /// Track and event objects themselves are not duplicated.
        /// </summary>
        public UIAnimationSequence ShallowCopy()
        {
            UIAnimationSequence copy =
                new UIAnimationSequence();

            copy.duration = duration;

            if (tracks != null)
            {
                copy.tracks =
                    new UIAnimationTrack[tracks.Length];

                Array.Copy(
                    tracks,
                    copy.tracks,
                    tracks.Length);
            }

            if (events != null)
            {
                copy.events =
                    new UIAnimationEvent[events.Length];

                Array.Copy(
                    events,
                    copy.events,
                    events.Length);
            }

            return copy;
        }
    }
}