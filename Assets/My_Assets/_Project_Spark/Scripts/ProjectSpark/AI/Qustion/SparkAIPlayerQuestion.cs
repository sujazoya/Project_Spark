using System;
using UnityEngine;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Represents one question asked by the player.
    ///
    /// This is intentionally a small data object.
    /// It does not decide the answer.
    /// </summary>
    [Serializable]
    public readonly struct SparkAIPlayerQuestion
    {
        public bool IsValid { get; }
        public string Text { get; }
        public float CreatedAt { get; }

        public SparkAIPlayerQuestion(
            bool isValid,
            string text,
            float createdAt)
        {
            IsValid = isValid;
            Text = text;
            CreatedAt = createdAt;
        }

        public static SparkAIPlayerQuestion Invalid()
        {
            return new SparkAIPlayerQuestion(
                false,
                string.Empty,
                Time.time);
        }

        public static SparkAIPlayerQuestion Create(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Invalid();

            return new SparkAIPlayerQuestion(
                true,
                text.Trim(),
                Time.time);
        }
    }
}