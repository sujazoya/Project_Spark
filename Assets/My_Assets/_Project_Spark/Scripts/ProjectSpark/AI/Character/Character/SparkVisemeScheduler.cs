using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    // Approximate grapheme-based visemes; not phoneme alignment.
    public static class SparkVisemeScheduler
    {
        public enum Pose { Ah, Ai, Ee, Eh, FV, MBP, Oo, Ooh, R_Sn, S, Default }
        public struct Cue
        {
            public Pose pose;
            public float start;
            public float end;
            public Cue(Pose p, float a, float b) { pose = p; start = a; end = b; }
        }

        public static List<Cue> Build(string text, float duration)
        {
            var result = new List<Cue>();
            if (string.IsNullOrEmpty(text) || duration <= 0f) return result;
            var poses = new List<Pose>();
            var weights = new List<float>();
            string s = text.ToLowerInvariant();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { Add(Pose.Default, 0.38f); continue; }
                if (c == ',' || c == ';' || c == ':') { Add(Pose.Default, 1.5f); continue; }
                if (c == '.' || c == '!' || c == '?') { Add(Pose.Default, 2.5f); continue; }
                if (!char.IsLetterOrDigit(c)) continue;
                if (i + 1 < s.Length)
                {
                    string pair = s.Substring(i, 2);
                    if (pair == "oo" || pair == "ou") { Add(Pose.Ooh, 1.9f); i++; continue; }
                    if (pair == "ee" || pair == "ea") { Add(Pose.Ee, 1.9f); i++; continue; }
                    if (pair == "ai" || pair == "ay") { Add(Pose.Ai, 1.9f); i++; continue; }
                    if (pair == "sh" || pair == "ch" || pair == "th") { Add(Pose.S, 1.6f); i++; continue; }
                    if (pair == "ph") { Add(Pose.FV, 1.6f); i++; continue; }
                }
                Pose p;
                switch (c)
                {
                    case 'a': p = Pose.Ah; break;
                    case 'e': p = Pose.Eh; break;
                    case 'i': case 'y': p = Pose.Ee; break;
                    case 'o': p = Pose.Oo; break;
                    case 'u': case 'w': p = Pose.Ooh; break;
                    case 'f': case 'v': p = Pose.FV; break;
                    case 'm': case 'b': case 'p': p = Pose.MBP; break;
                    case 'r': case 'n': case 'l': p = Pose.R_Sn; break;
                    case 's': case 'z': case 'x': p = Pose.S; break;
                    default: p = Pose.Default; break;
                }
                Add(p, "aeiouy".IndexOf(c) >= 0 ? 1.6f : 0.8f);
            }
            float total = 0f;
            foreach (float w in weights) total += w;
            if (total <= 0f) return result;
            float time = 0f;
            for (int i = 0; i < poses.Count; i++)
            {
                float next = i == poses.Count - 1 ? duration : time + duration * weights[i] / total;
                result.Add(new Cue(poses[i], time, next));
                time = next;
            }
            return result;

            void Add(Pose pose, float weight)
            {
                poses.Add(pose);
                weights.Add(weight);
            }
        }

        public static Pose At(List<Cue> cues, float seconds)
        {
            if (cues == null || cues.Count == 0) return Pose.Default;
            int lo = 0, hi = cues.Count - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                if (seconds < cues[mid].start) hi = mid - 1;
                else if (seconds >= cues[mid].end) lo = mid + 1;
                else return cues[mid].pose;
            }
            return Pose.Default;
        }
    }
}
