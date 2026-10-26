using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI.Tutorial
{
    public enum SparkTutorialAdvance { ContinueButton, GameplaySignal, Either }
    public enum SparkTutorialFocus { None, WorldTransform, ScreenRect }

    [Serializable]
    public sealed class SparkTutorialStep
    {
        public string id = "step";
        public string title = "Learn";
        [TextArea(3, 8)] public string explanation;
        [TextArea(2, 5)] public string hint;
        [Tooltip("Optional concept key for analytics and curriculum mapping.")]
        public string conceptId;
        public SparkTutorialAdvance advance = SparkTutorialAdvance.ContinueButton;
        [Tooltip("For GameplaySignal/Either: e.g. shelf.item_selected, shelf.item_placed, circuit.connection_created, switch.toggled")]
        public string requiredSignal;
        public SparkTutorialFocus focus = SparkTutorialFocus.None;
        [Tooltip("A scene-specific direct target. Prefer locatorKey for reusable levels.")]
        public Transform worldTarget;
        [Tooltip("A scene-specific UI target. Prefer locatorKey for reusable UI.")]
        public RectTransform screenTarget;
        [Tooltip("Resolved at runtime from SparkTutorialTarget.key")]
        public string locatorKey;
        public Vector3 worldOffset = Vector3.up * 0.05f;
        [Min(0f)] public float autoHintAfterSeconds = 14f;
        public bool speak = true;
        public bool allowSkip = true;
    }

    [CreateAssetMenu(fileName = "Tutorial_New", menuName = "ProjectSpark/AI/Tutorial Definition")]
    public sealed class SparkTutorialDefinition : ScriptableObject
    {
        public string tutorialId = "getting_started";
        public string displayName = "First Circuit";
        public bool resumeFromCheckpoint = true;
        public List<SparkTutorialStep> steps = new List<SparkTutorialStep>();
    }
}