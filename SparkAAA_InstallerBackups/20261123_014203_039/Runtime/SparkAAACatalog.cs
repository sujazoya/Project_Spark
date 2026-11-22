using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AAAArchitecture
{
    [Serializable] public sealed class SparkAAATarget
    {
        public string id, title, description, componentId, terminalId, positiveId, negativeId;
        public int rawType;
        public float minimumVoltage, minimumCurrent, minimumPower;
    }
    [Serializable] public sealed class SparkAAALevel
    {
        public int number, chapter, requiredTargets, rawCompletionMode;
        public string key, guid, legacyId, title, description, sourcePath, issue;
        public bool defaultUnlocked, autoUnlock, autoAdvance, unlockNext, blocked;
        public ScriptableObject definition;
        public Sprite thumbnail;
        public List<SparkAAATarget> targets = new List<SparkAAATarget>();
        public bool Present => definition != null;
    }
    [Serializable] public sealed class SparkAAAChapter
    {
        public int id, first, last;
        public string title;
        public SparkAAAChapter(int id, int first, int last, string title)
        { this.id = id; this.first = first; this.last = last; this.title = title; }
    }
    public sealed class SparkAAACatalog : ScriptableObject
    {
        public const int CurrentSchema = 1;
        public int schemaVersion = CurrentSchema;
        public List<SparkAAALevel> levels = new List<SparkAAALevel>();
        public List<SparkAAAChapter> chapters = new List<SparkAAAChapter>();
        public string scannedUtc;
        public SparkAAALevel Find(string key)
        {
            for (int i = 0; i < levels.Count; i++) if (levels[i].key == key) return levels[i];
            return null;
        }
        public string ChapterName(int id)
        {
            for (int i = 0; i < chapters.Count; i++) if (chapters[i].id == id) return chapters[i].title;
            return "Unclassified";
        }
    }
}
