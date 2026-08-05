using System;
using UnityEngine;



namespace AAAUI
{
    public sealed class UIPlayer
    {
        private readonly PlaybackContext context;

        private UIAnimationProfile profile;
        private UIAnimationSequence sequence;
        

        private UIPlaybackState state;
        private UIPlaybackDirection direction;

        private float time;
        private bool loop;

        private int[] propertyIds = Array.Empty<int>();
        private bool[] firedEvents = Array.Empty<bool>();
       

        public UIPlaybackState State => state;

        public UIPlaybackDirection Direction => direction;

        public float Time => time;

        public float NormalizedTime =>
            sequence == null || sequence.Duration <= 0f
                ? 1f
                : Mathf.Clamp01(time / sequence.Duration);

        public bool IsPlaying =>
            state == UIPlaybackState.Playing;

        public event Action Started;
        public event Action<string> MarkerReached;
        public event Action Completed;
        public event Action ReverseCompleted;

        private int nextEventIndex;
        private float previousTime;

        public UIPlayer(PlaybackContext playbackContext)
        {
            context = playbackContext
                ?? throw new ArgumentNullException(nameof(playbackContext));

            state = UIPlaybackState.Stopped;
            direction = UIPlaybackDirection.Forward;
        }

        // =========================================================
        // PROFILE
        // =========================================================

        public void SetProfile(UIAnimationProfile value)
        {
            profile = value;

            if (value != null)
                PrepareMaterials(value);
        }

        // =========================================================
        // PLAY COMMANDS
        // =========================================================

        public void PlayIn()
        {
            Play(
                UIAnimationSequenceType.Open,
                false,
                UIPlaybackDirection.Forward
            );
        }

        public void PlayOut()
        {
            Play(
                UIAnimationSequenceType.Close,
                false,
                UIPlaybackDirection.Forward
            );
        }

        public void PlayLoop()
        {
            Play(
                UIAnimationSequenceType.Loop,
                true,
                UIPlaybackDirection.Forward
            );
        }

        public void Play(
            UIAnimationSequenceType type,
            bool shouldLoop,
            UIPlaybackDirection playDirection)
        {
            if (profile == null)
                return;

            UIAnimationSequence next = profile.GetSequence(type);

            if (next == null)
                return;

            sequence = next;

            PreparePropertyIds(sequence);

            loop = shouldLoop;
            direction = playDirection;

            time =
                playDirection == UIPlaybackDirection.Forward
                    ? 0f
                    : next.Duration;

            state = UIPlaybackState.Playing;
            previousTime = time;
            nextEventIndex =
                playDirection == UIPlaybackDirection.Forward
                    ? 0
                    : GetLastEventIndex(next);

            Started?.Invoke();

            Evaluate();
        }
        private int GetLastEventIndex(UIAnimationSequence value)
        {
            if (value == null ||
                value.Events == null ||
                value.Events.Length == 0)
            {
                return -1;
            }

            return value.Events.Length - 1;
        }

        private void ProcessEvents(float oldTime, float newTime)
        {
            if (sequence == null)
                return;

            UIAnimationEvent[] events = sequence.Events;

            if (events == null || events.Length == 0)
                return;

            if (direction == UIPlaybackDirection.Forward)
            {
                for (int i = 0; i < events.Length; i++)
                {
                    UIAnimationEvent animationEvent = events[i];

                    if (animationEvent == null)
                        continue;

                    if (animationEvent.Time > oldTime &&
                        animationEvent.Time <= newTime)
                    {
                        FireEvent(animationEvent);
                    }
                }
            }
            else
            {
                for (int i = events.Length - 1; i >= 0; i--)
                {
                    UIAnimationEvent animationEvent = events[i];

                    if (animationEvent == null)
                        continue;

                    if (animationEvent.Time < oldTime &&
                        animationEvent.Time >= newTime)
                    {
                        FireReverseEvent(animationEvent);
                    }
                }
            }
        }
        private void FireEvent(UIAnimationEvent animationEvent)
        {
            if (animationEvent == null)
                return;

            switch (animationEvent.Type)
            {
                case UIAnimationEventType.Start:
                    Started?.Invoke();
                    break;

                case UIAnimationEventType.Marker:
                    MarkerReached?.Invoke(animationEvent.Id);
                    break;

                case UIAnimationEventType.Complete:
                    Completed?.Invoke();
                    break;
            }
        }
        private void FireReverseEvent(UIAnimationEvent animationEvent)
        {
            if (animationEvent == null)
                return;

            switch (animationEvent.Type)
            {
                case UIAnimationEventType.Marker:
                    MarkerReached?.Invoke(animationEvent.Id);
                    break;

                case UIAnimationEventType.ReverseComplete:
                    ReverseCompleted?.Invoke();
                    break;
            }
        }


        // =========================================================
        // STOP / PAUSE / RESUME
        // =========================================================

        public void Stop()
        {
            if (state == UIPlaybackState.Stopped)
                return;

            state = UIPlaybackState.Stopped;
            loop = false;
        }

        public void Pause()
        {
            if (state == UIPlaybackState.Playing)
                state = UIPlaybackState.Paused;
        }

        public void Resume()
        {
            if (state == UIPlaybackState.Paused)
                state = UIPlaybackState.Playing;
        }

        // =========================================================
        // INSTANT STATES
        // =========================================================

        public void InstantIn()
        {
            if (profile == null ||
                profile.OpenSequence == null)
                return;

            sequence = profile.OpenSequence;

            PreparePropertyIds(sequence);

            direction = UIPlaybackDirection.Forward;

            time = sequence.Duration;

            state = UIPlaybackState.Stopped;
            loop = false;

            Evaluate();
        }

        public void InstantOut()
        {
            if (profile == null ||
                profile.CloseSequence == null)
                return;

            sequence = profile.CloseSequence;

            PreparePropertyIds(sequence);

            direction = UIPlaybackDirection.Forward;

            time = sequence.Duration;

            state = UIPlaybackState.Stopped;
            loop = false;

            Evaluate();
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public void Update(float deltaTime)
        {

            if (state != UIPlaybackState.Playing ||
                sequence == null)
                return;

            float previousTime = time;

            float delta =
                Mathf.Max(0f, deltaTime);

            if (direction == UIPlaybackDirection.Forward)
                time += delta;
            else
                time -= delta;

            float duration = sequence.Duration;

            // =========================================================
            // LOOP
            // =========================================================

            if (loop && duration > 0f)
            {
                if (direction == UIPlaybackDirection.Forward)
                {
                    if (time >= duration)
                    {
                        FireEventsForward(
                            previousTime,
                            duration
                        );

                        time %= duration;

                        ResetEventsAfterLoop();
                    }
                    else
                    {
                        FireEventsForward(
                            previousTime,
                            time
                        );
                    }
                }
                else
                {
                    if (time <= 0f)
                    {
                        FireEventsBackward(
                            previousTime,
                            0f
                        );

                        time =
                            duration -
                            Mathf.Repeat(
                                -time,
                                duration
                            );

                        ResetEventsAfterLoop();
                    }
                    else
                    {
                        FireEventsBackward(
                            previousTime,
                            time
                        );
                    }
                }

                Evaluate();
                return;
            }

            // =========================================================
            // FORWARD
            // =========================================================

            if (direction == UIPlaybackDirection.Forward)
            {
                float eventEnd =
                    Mathf.Min(
                        time,
                        duration
                    );

                FireEventsForward(
                    previousTime,
                    eventEnd
                );

                if (time >= duration)
                {
                    time = duration;

                    Evaluate();

                    state = UIPlaybackState.Stopped;

                    Completed?.Invoke();

                    return;
                }
            }

            // =========================================================
            // BACKWARD
            // =========================================================

            else
            {
                float eventEnd =
                    Mathf.Max(
                        time,
                        0f
                    );

                FireEventsBackward(
                    previousTime,
                    eventEnd
                );

                if (time <= 0f)
                {
                    time = 0f;

                    Evaluate();

                    state = UIPlaybackState.Stopped;

                    Completed?.Invoke();

                    return;
                }
            }

            Evaluate();
        }
        private void FireEventsForward(
    float previousTime,
    float currentTime)
        {
            if (sequence == null ||
                sequence.Events == null)
                return;

            UIAnimationEvent[] events =
                sequence.Events;

            for (int i = 0; i < events.Length; i++)
            {
                UIAnimationEvent evt = events[i];

                if (evt == null)
                    continue;

                if (firedEvents.Length <= i)
                    continue;

                if (firedEvents[i])
                    continue;

                float eventTime = evt.Time;

                if (eventTime > previousTime &&
                    eventTime <= currentTime)
                {
                    firedEvents[i] = true;

                    if (evt.Type ==
                        UIAnimationEventType.Marker)
                    {
                        MarkerReached?.Invoke(evt.Id);
                    }
                }
            }
        }
        private void FireEventsBackward(
    float previousTime,
    float currentTime)
        {
            if (sequence == null ||
                sequence.Events == null)
                return;

            UIAnimationEvent[] events =
                sequence.Events;

            for (int i = 0; i < events.Length; i++)
            {
                UIAnimationEvent evt = events[i];

                if (evt == null)
                    continue;

                if (firedEvents.Length <= i)
                    continue;

                if (evt.Time < previousTime &&
                    evt.Time >= currentTime)
                {
                    firedEvents[i] = false;

                    if (evt.Type ==
                        UIAnimationEventType.Marker)
                    {
                        MarkerReached?.Invoke(evt.Id);
                    }
                }
            }
        }
        private void ResetEventsAfterLoop()
        {
            for (int i = 0; i < firedEvents.Length; i++)
                firedEvents[i] = false;
        }

        // =========================================================
        // DIRECT EVALUATION
        // =========================================================

        public void EvaluateAt(float absoluteTime)
        {
            if (sequence == null)
                return;

            time = Mathf.Clamp(
                absoluteTime,
                0f,
                sequence.Duration
            );

            Evaluate();
        }

        // =========================================================
        // MATERIAL PREPARATION
        // =========================================================

        private void PrepareMaterials(UIAnimationProfile value)
        {
            if (value == null)
                return;

            PrepareSequenceMaterials(value.OpenSequence);
            PrepareSequenceMaterials(value.CloseSequence);
            PrepareSequenceMaterials(value.LoopSequence);
        }

        private void PrepareSequenceMaterials(
      UIAnimationSequence value)
        {
            if (value == null ||
                value.Tracks == null)
                return;

            UIAnimationTrack[] tracks =
                value.Tracks;

            for (int i = 0; i < tracks.Length; i++)
            {
                UIAnimationTrack track =
                    tracks[i];

                if (track == null)
                    continue;

                bool isMaterialTrack =
                    track is GlowTrack ||
                    track is DissolveTrack ||
                    track is GlitchTrack ||
                    track is MaterialFloatTrack ||
                    track is MaterialColorTrack;

                if (!isMaterialTrack)
                    continue;

                int targetIndex =
                    track.TargetIndex;

                if ((uint)targetIndex <
                    (uint)context.Materials.Length)
                {
                    context.Materials[targetIndex].Prepare();
                }
            }
        }

        // =========================================================
        // PROPERTY IDS
        // =========================================================

        private void PreparePropertyIds(
            UIAnimationSequence value)
        {
            UIAnimationTrack[] tracks =
                value != null
                    ? value.Tracks
                    : null;

            int count =
                tracks != null
                    ? tracks.Length
                    : 0;

            if (propertyIds.Length != count)
                propertyIds = new int[count];

            for (int i = 0; i < count; i++)
            {
                UIAnimationTrack track = tracks[i];

                if (track == null)
                {
                    propertyIds[i] = -1;
                    continue;
                }

                // -------------------------------------------------
                // SINGLE PROPERTY TRACKS
                // -------------------------------------------------


                if (track is GlowTrack glow)
                {
                    propertyIds[i] =
                        ShaderPropertyCache.GetId(
                            glow.Property
                        );
                }

                else if (track is DissolveTrack dissolve)
                {
                    propertyIds[i] =
                        ShaderPropertyCache.GetId(
                            dissolve.Property
                        );
                }

                else if (track is DissolveTrack dissolves)
                {
                    propertyIds[i] =
                        ShaderPropertyCache.GetId(dissolves.Property);

                    Debug.Log(
                        $"[AAAUI DISSOLVE] " +
                        $"Property = {dissolves.Property} | " +
                        $"ID = {propertyIds[i]}"
                    );
                }

                // -------------------------------------------------
                // GLITCH IS MULTI-PROPERTY
                // -------------------------------------------------

                else if (track is GlitchTrack)
                {
                    // IMPORTANT:
                    //
                    // Glitch no longer has one Property.
                    //
                    // TrackPlayer handles:
                    //
                    // _GlitchAmount
                    // _GlitchSpeed
                    // _GlitchBandScale
                    // _GlitchFrequency
                    // _GlitchBandWidth
                    //
                    // Therefore there is no single property ID.

                    propertyIds[i] = -1;
                }

                // -------------------------------------------------
                // MATERIAL FLOAT
                // -------------------------------------------------

                else if (track is MaterialFloatTrack materialFloat)
                {
                    propertyIds[i] =
                        ShaderPropertyCache.GetId(
                            materialFloat.Property
                        );
                }

                // -------------------------------------------------
                // MATERIAL COLOR
                // -------------------------------------------------

                else if (track is MaterialColorTrack materialColor)
                {
                    propertyIds[i] =
                        ShaderPropertyCache.GetId(
                            materialColor.Property
                        );
                }

                // -------------------------------------------------
                // NON-MATERIAL TRACK
                // -------------------------------------------------

                else
                {
                    propertyIds[i] = -1;
                }
            }
        }

        // =========================================================
        // RESTORE ORIGINAL STATE
        // =========================================================

        internal void EditorRestoreOriginal()
        {
            for (int i = 0;
                 i < context.Targets.Length;
                 i++)
            {
                UIAnimationTarget target =
                    context.Targets[i];

                if (target.Transform != null)
                {
                    target.Transform.localPosition =
                        context.OriginalPositions[i];

                    target.Transform.localScale =
                        context.OriginalScales[i];

                    target.Transform.localRotation =
                        context.OriginalRotations[i];
                }

                if (target.CanvasGroup != null)
                {
                    target.CanvasGroup.alpha =
                        context.OriginalAlpha[i];
                }

                if (target.Graphic != null)
                {
                    target.Graphic.color =
                        context.OriginalColors[i];
                }

                context.Materials[i].Restore();
            }
        }

        // =========================================================
        // EVALUATE CURRENT FRAME
        // =========================================================

        private void Evaluate()
        {
            if (sequence == null)
                return;

            UIAnimationTrack[] tracks =
                sequence.Tracks;

            if (tracks == null)
                return;

            int count =
                Mathf.Min(
                    tracks.Length,
                    propertyIds.Length
                );

            for (int i = 0; i < count; i++)
            {              

                TrackPlayer.Evaluate(
                    tracks[i],
                    context,
                    time,
                    propertyIds[i]
                );
            }
        }
    }
}