using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI.LipSync
{
    /// <summary>
    /// Starter text-timed viseme driver. This is an approximation, not phoneme alignment.
    /// For accurate sync, supply timestamped phonemes from a forced aligner or phoneme-timing service.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVoiceLipSync : MonoBehaviour
    {
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private SparkFacialPoseRig facialRig;
        [SerializeField, Range(-0.05f, 0.05f)] private float timingOffset;
        [SerializeField] private bool resetFaceWhenAudioStops = true;

        private struct Event
        {
            public SparkViseme viseme;
            public float endTime;
        }

        private readonly List<Event> events = new List<Event>(256);
        private bool prepared;
        private bool hasStarted;
        private AudioClip preparedClip;
        private int lastIndex = -1;

        public bool IsSpeaking => prepared && voiceSource != null && voiceSource.isPlaying;

        private void Awake()
        {
            if (voiceSource == null || facialRig == null)
            {
                Debug.LogError("SparkVoiceLipSync: assign AudioSource and SparkFacialPoseRig.", this);
                enabled = false;
            }
        }

        /// <summary>Prepare a clip before another script calls AudioSource.Play().</summary>
        public void PrepareSpeech(string exactSpokenText, AudioClip clip)
        {
            events.Clear();
            prepared = false;
            hasStarted = false;
            preparedClip = null;
            lastIndex = -1;

            if (clip == null || string.IsNullOrWhiteSpace(exactSpokenText) ||
                voiceSource == null || facialRig == null)
            {
                facialRig?.ResetFace();
                return;
            }

            BuildApproximateTimeline(exactSpokenText, clip.length);
            preparedClip = clip;
            prepared = events.Count > 0;
            facialRig.ResetFace();
        }

        /// <summary>Use this instead of manually assigning and playing the AudioSource clip.</summary>
        public void PlaySpeech(string exactSpokenText, AudioClip clip)
        {
            if (voiceSource == null || clip == null) return;

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.loop = false;
            PrepareSpeech(exactSpokenText, clip);
            voiceSource.Play();
        }

        public void StopSpeech()
        {
            prepared = false;
            hasStarted = false;
            lastIndex = -1;
            events.Clear();
            preparedClip = null;
            if (resetFaceWhenAudioStops) facialRig?.ResetFace();
        }

        private void Update()
        {
            if (!prepared || voiceSource == null || voiceSource.clip == null)
                return;

            if (voiceSource.clip != preparedClip)
            {
                StopSpeech();
                return;
            }

            if (!voiceSource.isPlaying)
            {
                if (hasStarted) StopSpeech();
                return;
            }

            hasStarted = true;
            float time = voiceSource.timeSamples / (float)Mathf.Max(1, voiceSource.clip.frequency);
            time += timingOffset;

            int index = FindEvent(Mathf.Max(0f, time));
            if (index >= 0 && index != lastIndex)
            {
                lastIndex = index;
                facialRig.SetViseme(events[index].viseme);
            }
        }

        private int FindEvent(float time)
        {
            for (int i = 0; i < events.Count; i++)
                if (time < events[i].endTime) return i;

            return events.Count - 1;
        }

        private void BuildApproximateTimeline(string text, float clipDuration)
        {
            string upper = text.ToUpperInvariant();
            var visemes = new List<SparkViseme>(upper.Length);
            var weights = new List<float>(upper.Length);
            float totalWeight = 0f;

            for (int i = 0; i < upper.Length;)
            {
                SparkViseme viseme;
                float weight;
                int consumed;
                ParseCharacter(upper, i, out viseme, out weight, out consumed);
                visemes.Add(viseme);
                weights.Add(weight);
                totalWeight += weight;
                i += consumed;
            }

            if (totalWeight <= 0f || clipDuration <= 0f) return;

            float elapsed = 0f;
            for (int i = 0; i < visemes.Count; i++)
            {
                elapsed += clipDuration * weights[i] / totalWeight;
                events.Add(new Event { viseme = visemes[i], endTime = elapsed });
            }

            if (events.Count > 0)
            {
                Event last = events[events.Count - 1];
                last.endTime = clipDuration + 0.1f;
                events[events.Count - 1] = last;
            }
        }

        private static void ParseCharacter(
            string text, int index, out SparkViseme viseme, out float weight, out int consumed)
        {
            char c = text[index];
            char next = index + 1 < text.Length ? text[index + 1] : '\0';
            consumed = 1;
            weight = 1f;

            if (char.IsWhiteSpace(c))
            {
                viseme = SparkViseme.Default;
                weight = 0.55f;
                return;
            }

            if (!char.IsLetter(c))
            {
                viseme = SparkViseme.Default;
                weight = 0.45f;
                return;
            }

            if ((c == 'A' && next == 'I') || (c == 'A' && next == 'Y'))
            {
                viseme = SparkViseme.Ai; consumed = 2; weight = 1.4f; return;
            }
            if ((c == 'S' && next == 'H') || (c == 'C' && next == 'H'))
            {
                viseme = SparkViseme.R_Sh; consumed = 2; weight = 1.2f; return;
            }
            if (c == 'T' && next == 'H')
            {
                viseme = SparkViseme.S; consumed = 2; weight = 1.2f; return;
            }
            if (c == 'E' && next == 'E')
            {
                viseme = SparkViseme.Ee; consumed = 2; weight = 1.3f; return;
            }
            if (c == 'O' && next == 'O')
            {
                viseme = SparkViseme.Oo; consumed = 2; weight = 1.3f; return;
            }
            if (c == 'O' && next == 'U')
            {
                viseme = SparkViseme.Ooh; consumed = 2; weight = 1.3f; return;
            }

            switch (c)
            {
                case 'A': viseme = SparkViseme.Ah; weight = 1.3f; break;
                case 'I': case 'Y': viseme = SparkViseme.Ee; weight = 1.2f; break;
                case 'E': viseme = SparkViseme.Eh; weight = 1.2f; break;
                case 'O': viseme = SparkViseme.Ooh; weight = 1.3f; break;
                case 'U': case 'W': viseme = SparkViseme.Oo; weight = 1.2f; break;
                case 'F': case 'V': viseme = SparkViseme.FV; break;
                case 'M': case 'B': case 'P': viseme = SparkViseme.MBP; break;
                case 'R': case 'H': viseme = SparkViseme.R_Sh; break;
                case 'S': case 'Z': case 'X': case 'C': viseme = SparkViseme.S; break;
                case 'L': case 'N': case 'D': case 'T': viseme = SparkViseme.Eh; break;
                case 'K': case 'G': case 'Q': case 'J': viseme = SparkViseme.Ah; break;
                default: viseme = SparkViseme.Default; break;
            }
        }

        private void OnDisable()
        {
            if (resetFaceWhenAudioStops) facialRig?.ResetFace();
            prepared = false;
            hasStarted = false;
            preparedClip = null;
        }
    }
}
