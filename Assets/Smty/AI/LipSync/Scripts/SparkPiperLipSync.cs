
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI.LipSync
{
    [DisallowMultipleComponent]
    public sealed class SparkPiperLipSync : MonoBehaviour
    {
        [Header("Existing Piper Audio")]
        [SerializeField]
        private AudioSource voiceSource;

        [Header("Spark Character")]
        [SerializeField]
        private SparkFacialPoseController face;

        [Header("Speech")]
        [SerializeField, Range(0f, 0.3f)]
        private float endSilence = 0.08f;

        private struct VisemeEvent
        {
            public float time;
            public SparkViseme viseme;

            public VisemeEvent(
                float time,
                SparkViseme viseme)
            {
                this.time = time;
                this.viseme = viseme;
            }
        }

        private readonly List<VisemeEvent> timeline =
            new List<VisemeEvent>(128);

        private AudioClip preparedClip;
        private int eventIndex;
        private bool prepared;
        private bool started;
        private SparkViseme lastViseme;

        /// <summary>
        /// Call when Piper has generated its final AudioClip.
        /// Supply the exact text that was synthesized.
        /// Call before starting AudioSource playback.
        /// </summary>
        public void PrepareSpeech(
            string spokenText,
            AudioClip generatedClip)
        {
            StopLipSync();

            if (voiceSource == null ||
                face == null ||
                generatedClip == null)
            {
                Debug.LogWarning(
                    "Spark lip-sync references are incomplete.",
                    this);
                return;
            }

            preparedClip = generatedClip;

            BuildTimeline(
                spokenText ?? string.Empty,
                generatedClip.length);

            eventIndex = 0;
            lastViseme = SparkViseme.Default;
            prepared = true;
            started = false;
        }

        /// <summary>
        /// Optional convenience method if this component
        /// should also start playback.
        /// Do not use this if your existing system
        /// already calls AudioSource.Play().
        /// </summary>
        public void PlaySpeech(
            string spokenText,
            AudioClip generatedClip)
        {
            if (voiceSource == null ||
                generatedClip == null)
                return;

            voiceSource.Stop();
            voiceSource.clip = generatedClip;

            PrepareSpeech(spokenText, generatedClip);

            voiceSource.Play();
        }

        public void StopLipSync()
        {
            timeline.Clear();

            prepared = false;
            started = false;
            preparedClip = null;
            eventIndex = 0;

            if (face != null)
                face.ResetMouth();
        }

        private void Update()
        {
            if (!prepared ||
                voiceSource == null ||
                face == null)
                return;

            if (voiceSource.clip != preparedClip)
            {
                StopLipSync();
                return;
            }

            if (!voiceSource.isPlaying)
            {
                if (started)
                    StopLipSync();

                return;
            }

            started = true;

            float time = voiceSource.time;

            float activeEnd = Mathf.Max(
                0f,
                preparedClip.length - endSilence);

            if (time >= activeEnd)
            {
                ApplyViseme(SparkViseme.Default);
                return;
            }

            // Supports seeking or restarting the same clip.
            while (eventIndex > 0 &&
                   time < timeline[eventIndex].time)
            {
                eventIndex--;
            }

            while (eventIndex + 1 < timeline.Count &&
                   time >= timeline[eventIndex + 1].time)
            {
                eventIndex++;
            }

            if (timeline.Count > 0)
                ApplyViseme(timeline[eventIndex].viseme);
        }

        private void ApplyViseme(SparkViseme viseme)
        {
            if (lastViseme == viseme)
                return;

            lastViseme = viseme;
            face.SetViseme(viseme);
        }

        private void BuildTimeline(
            string text,
            float duration)
        {
            timeline.Clear();

            timeline.Add(new VisemeEvent(
                0f,
                SparkViseme.Default));

            if (string.IsNullOrWhiteSpace(text))
                return;

            List<SparkViseme> sequence =
                new List<SparkViseme>(text.Length);

            List<float> weights =
                new List<float>(text.Length);

            string input = text.ToLowerInvariant();

            for (int i = 0; i < input.Length;)
            {
                char c = input[i];

                if (char.IsWhiteSpace(c))
                {
                    Add(sequence, weights,
                        SparkViseme.Default, 0.35f);

                    i++;
                    continue;
                }

                if (!char.IsLetter(c))
                {
                    Add(sequence, weights,
                        SparkViseme.Default, 0.25f);

                    i++;
                    continue;
                }

                if (Match(input, i, "sh") ||
                    Match(input, i, "ch") ||
                    Match(input, i, "zh"))
                {
                    Add(sequence, weights,
                        SparkViseme.R_Sh, 1.5f);

                    i += 2;
                    continue;
                }

                if (Match(input, i, "th"))
                {
                    Add(sequence, weights,
                        SparkViseme.S, 1.2f);

                    i += 2;
                    continue;
                }

                if (Match(input, i, "oo"))
                {
                    Add(sequence, weights,
                        SparkViseme.Ooh, 1.6f);

                    i += 2;
                    continue;
                }

                if (Match(input, i, "ee"))
                {
                    Add(sequence, weights,
                        SparkViseme.Ee, 1.6f);

                    i += 2;
                    continue;
                }

                if (Match(input, i, "ai") ||
                    Match(input, i, "ay"))
                {
                    Add(sequence, weights,
                        SparkViseme.Ai, 1.5f);

                    i += 2;
                    continue;
                }

                SparkViseme pose;

                switch (c)
                {
                    case 'a':
                        pose = SparkViseme.Ah;
                        break;

                    case 'e':
                        pose = SparkViseme.Eh;
                        break;

                    case 'i':
                    case 'y':
                        pose = SparkViseme.Ee;
                        break;

                    case 'o':
                        pose = SparkViseme.Oo;
                        break;

                    case 'u':
                    case 'w':
                        pose = SparkViseme.Ooh;
                        break;

                    case 'f':
                    case 'v':
                        pose = SparkViseme.FV;
                        break;

                    case 'm':
                    case 'b':
                    case 'p':
                        pose = SparkViseme.MBP;
                        break;

                    case 'r':
                        pose = SparkViseme.R_Sh;
                        break;

                    case 's':
                    case 'z':
                    case 'c':
                    case 'x':
                        pose = SparkViseme.S;
                        break;

                    default:
                        pose = SparkViseme.Default;
                        break;
                }

                Add(sequence, weights, pose, 1f);
                i++;
            }

            float totalWeight = 0f;

            for (int i = 0; i < weights.Count; i++)
                totalWeight += weights[i];

            if (totalWeight <= 0f)
                return;

            float usableDuration = Mathf.Max(
                0f,
                duration - endSilence);

            float elapsedWeight = 0f;

            for (int i = 0; i < sequence.Count; i++)
            {
                float eventTime =
                    (elapsedWeight / totalWeight) *
                    usableDuration;

                timeline.Add(new VisemeEvent(
                    eventTime,
                    sequence[i]));

                elapsedWeight += weights[i];
            }

            timeline.Add(new VisemeEvent(
                usableDuration,
                SparkViseme.Default));
        }

        private static void Add(
            List<SparkViseme> sequence,
            List<float> weights,
            SparkViseme pose,
            float weight)
        {
            sequence.Add(pose);
            weights.Add(weight);
        }

        private static bool Match(
            string text,
            int index,
            string pattern)
        {
            if (index + pattern.Length > text.Length)
                return false;

            return string.Compare(
                text,
                index,
                pattern,
                0,
                pattern.Length,
                StringComparison.Ordinal) == 0;
        }
    }
}
