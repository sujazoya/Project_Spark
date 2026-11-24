using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    public sealed class SparkProGameCatalog : ScriptableObject
    {
        [Serializable] public sealed class Objective
        {
            public string id, title, description, kind, componentId, terminalId, positiveId, negativeId;
        }

        [Serializable] public sealed class Record
        {
            public int levelNumber, chapterNumber, previousLevel, accessScore;
            public bool inCampaign, chapterOpen;
            public string id, title, description, conditions, sourceGuid, sourcePath, chapter, accessRule, layoutPath;
            public string[] objectives, equipment, concepts, teachingResources;
            public Objective[] targets;
        }

        [Serializable] public sealed class IndexRecord { public string kind, name, guid, path, summary; }

        public Record[] levels = Array.Empty<Record>();
        public IndexRecord[] assets = Array.Empty<IndexRecord>();
        public string[] gaps = Array.Empty<string>();
        public string generatedUtc;
        public TextAsset motherboardQualificationSource;

        [NonSerialized] private Record[] indexed;
        [NonSerialized] private Dictionary<int, Record> byNumber;
        [NonSerialized] private Dictionary<string, Dictionary<Record, int>> search;
        private static readonly HashSet<string> StopWords = new(StringComparer.Ordinal)
        {
            "the","and","for","of","in","to","a","an","on","what","which","levels","level","show","me","is","teach",
            "tell","about","how","does","where","can","find","practice","want","i","learn","using","deals","works"
        };

        public int Count => levels?.Length ?? 0;
        public bool TryLevel(int number, out Record result)
        {
            EnsureIndex();
            return byNumber.TryGetValue(number, out result);
        }

        public IReadOnlyList<Record> Search(string query, int limit = 3)
        {
            EnsureIndex();
            var scores = new Dictionary<Record, int>();
            foreach (string token in Words(query).Distinct())
            {
                if (!search.TryGetValue(token, out var matches)) continue;
                foreach (var match in matches)
                    scores[match.Key] = scores.GetValueOrDefault(match.Key) + match.Value;
            }
            return scores.OrderByDescending(hit => hit.Value).ThenBy(hit => hit.Key.levelNumber)
                .Take(Mathf.Clamp(limit, 1, 10)).Select(hit => hit.Key).ToArray();
        }

        public static IEnumerable<string> Words(string text)
        {
            foreach (Match token in Regex.Matches((text ?? "").ToLowerInvariant(), @"[\p{L}\d][\p{L}\d-]*"))
            {
                string word = token.Value;
                if (word.Length > 4 && word.EndsWith("s", StringComparison.Ordinal) && !word.EndsWith("ss", StringComparison.Ordinal))
                    word = word.Substring(0, word.Length - 1);
                if (!StopWords.Contains(word) && !StopWords.Contains(token.Value)) yield return word;
            }
        }

        private void EnsureIndex()
        {
            if (byNumber != null && indexed == levels) return;
            indexed = levels;
            byNumber = new Dictionary<int, Record>();
            search = new Dictionary<string, Dictionary<Record, int>>(StringComparer.Ordinal);
            foreach (var record in levels ?? Array.Empty<Record>())
            {
                if (record == null || !byNumber.TryAdd(record.levelNumber, record)) continue;
                Add(record, record.title, 12);
                Add(record, string.Join(" ", record.concepts ?? Array.Empty<string>()), 8);
                Add(record, record.description, 4);
                Add(record, string.Join(" ", record.objectives ?? Array.Empty<string>()), 3);
                Add(record, record.chapter, 1);
            }
        }

        private void Add(Record record, string text, int weight)
        {
            foreach (string term in Words(text).Distinct())
            {
                if (!search.TryGetValue(term, out var matches))
                    search.Add(term, matches = new Dictionary<Record, int>());
                matches[record] = matches.GetValueOrDefault(record) + weight;
            }
        }
    }
}
