using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectSpark.Display
{
    [DisallowMultipleComponent]
    public sealed class SparkDisplayTextAnimator : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text target;

        private Coroutine routine;

        private string currentValue =
            string.Empty;

        private SparkDisplayTextAnimation currentMode =
            SparkDisplayTextAnimation.Instant;

        private float currentSpeed;

        public void SetText(
            string value,
            SparkDisplayTextAnimation mode,
            float speed)
        {
            value ??= string.Empty;

            float clampedSpeed =
                Mathf.Max(
                    1f,
                    speed);

            if (currentValue == value &&
                currentMode == mode &&
                Mathf.Approximately(
                    currentSpeed,
                    clampedSpeed))
            {
                return;
            }

            currentValue =
                value;

            currentMode =
                mode;

            currentSpeed =
                clampedSpeed;

            StopCurrentRoutine();

            if (target == null)
                return;

            target.enabled = true;

            routine =
                StartCoroutine(
                    Animate(
                        value,
                        mode,
                        clampedSpeed));
        }

        public void SetImmediate(
            string value)
        {
            value ??= string.Empty;

            StopCurrentRoutine();

            currentValue =
                value;

            currentMode =
                SparkDisplayTextAnimation.Instant;

            currentSpeed =
                0f;

            if (target == null)
                return;

            target.enabled = true;
            target.text = value;
        }

        private IEnumerator Animate(
            string value,
            SparkDisplayTextAnimation mode,
            float speed)
        {
            if (target == null)
                yield break;

            target.enabled = true;

            switch (mode)
            {
                case SparkDisplayTextAnimation.Instant:

                    target.text =
                        value;

                    break;

                case SparkDisplayTextAnimation.Reveal:
                case SparkDisplayTextAnimation.Typewriter:
                case SparkDisplayTextAnimation.Scan:

                    yield return AnimateReveal(
                        value,
                        speed);

                    break;

                case SparkDisplayTextAnimation.Scramble:

                    yield return AnimateScramble(
                        value,
                        speed);

                    break;

                case SparkDisplayTextAnimation.Flicker:

                    yield return AnimateFlicker(
                        value);

                    break;
            }

            if (target != null)
            {
                target.enabled = true;
                target.text = value;
            }

            routine = null;
        }

        private IEnumerator AnimateReveal(
            string value,
            float speed)
        {
            if (target == null)
                yield break;

            if (value.Length == 0)
            {
                target.text =
                    string.Empty;

                yield break;
            }

            float delay =
                1f / speed;

            for (int i = 1;
                 i <= value.Length;
                 i++)
            {
                if (target == null)
                    yield break;

                target.text =
                    value.Substring(
                        0,
                        i);

                yield return
                    new WaitForSecondsRealtime(
                        delay);
            }
        }

        private IEnumerator AnimateScramble(
            string value,
            float speed)
        {
            if (target == null)
                yield break;

            const string characters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#$%@";

            float delay =
                1f / speed;

            for (int iteration = 0;
                 iteration < 6;
                 iteration++)
            {
                if (target == null)
                    yield break;

                char[] buffer =
                    new char[value.Length];

                for (int i = 0;
                     i < buffer.Length;
                     i++)
                {
                    buffer[i] =
                        characters[
                            Random.Range(
                                0,
                                characters.Length)];
                }

                target.text =
                    new string(buffer);

                yield return
                    new WaitForSecondsRealtime(
                        delay);
            }

            if (target != null)
                target.text =
                    value;
        }

        private IEnumerator AnimateFlicker(
            string value)
        {
            if (target == null)
                yield break;

            bool originalEnabled =
                target.enabled;

            target.text =
                value;

            for (int i = 0;
                 i < 4;
                 i++)
            {
                if (target == null)
                    yield break;

                target.enabled =
                    false;

                yield return
                    new WaitForSecondsRealtime(
                        0.025f);

                if (target == null)
                    yield break;

                target.enabled =
                    true;

                yield return
                    new WaitForSecondsRealtime(
                        0.04f);
            }

            if (target != null)
            {
                target.enabled =
                    originalEnabled;
            }
        }

        private void StopCurrentRoutine()
        {
            if (routine == null)
                return;

            StopCoroutine(
                routine);

            routine = null;

            if (target != null)
                target.enabled = true;
        }

        private void OnDisable()
        {
            StopCurrentRoutine();
        }
    }
}
