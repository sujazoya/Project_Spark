// ============================================================================
// ConnectionAnimation.cs
// ============================================================================

using UnityEngine;

namespace ProjectSpark.Gameplay.Wiring
{
    public sealed class ConnectionAnimation : MonoBehaviour
    {
        [SerializeField]
        float duration = .12f;

        Vector3 original;

        float timer;

        bool playing;

        void Awake()
        {
            original = transform.localScale;
        }

        public void Play()
        {
            timer = 0;
            playing = true;
        }

        void Update()
        {
            if (!playing)
                return;

            timer += Time.deltaTime;

            float t = timer / duration;

            float s =
                Mathf.Lerp(
                    1.25f,
                    1f,
                    t);

            transform.localScale =
                original * s;

            if (t >= 1)
            {
                playing = false;
                transform.localScale = original;
            }
        }
    }
}