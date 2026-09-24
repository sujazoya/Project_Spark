using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    public sealed class SparkDisplayTextAnimator : MonoBehaviour
    {
        [SerializeField] private TMP_Text target;

        private Coroutine routine;

        public void SetText(string value, SparkDisplayTextAnimation mode, float speed)
        {
            if (routine != null)
                StopCoroutine(routine);

            if (target == null)
                return;

            routine = StartCoroutine(Animate(value ?? string.Empty, mode, Mathf.Max(1f, speed)));
        }

        public void SetImmediate(string value)
        {
            if (routine != null)
                StopCoroutine(routine);

            if (target != null)
                target.text = value ?? string.Empty;
        }

        private IEnumerator Animate(string value, SparkDisplayTextAnimation mode, float speed)
        {
            switch (mode)
            {
                case SparkDisplayTextAnimation.Instant:
                    target.text = value;
                    yield break;

                case SparkDisplayTextAnimation.Reveal:
                case SparkDisplayTextAnimation.Typewriter:
                case SparkDisplayTextAnimation.Scan:
                    for (int i = 1; i <= value.Length; i++)
                    {
                        target.text = value.Substring(0, i);
                        yield return new WaitForSecondsRealtime(1f / speed);
                    }
                    break;

                case SparkDisplayTextAnimation.Scramble:
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#$%@";
                    for (int i = 0; i < 6; i++)
                    {
                        char[] buffer = new char[value.Length];
                        for (int c = 0; c < value.Length; c++)
                            buffer[c] = chars[Random.Range(0, chars.Length)];

                        target.text = new string(buffer);
                        yield return new WaitForSecondsRealtime(1f / speed);
                    }
                    target.text = value;
                    break;

                case SparkDisplayTextAnimation.Flicker:
                    target.text = value;
                    for (int i = 0; i < 4; i++)
                    {
                        target.enabled = false;
                        yield return new WaitForSecondsRealtime(0.025f);
                        target.enabled = true;
                        yield return new WaitForSecondsRealtime(0.04f);
                    }
                    break;
            }

            target.enabled = true;
        }
    }
}
