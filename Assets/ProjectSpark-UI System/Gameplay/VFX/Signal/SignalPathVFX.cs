using UnityEngine;
using UnityEngine.VFX;

namespace AAAUI.VFX
{
    [DisallowMultipleComponent]
    public sealed class SignalPathVFX : MonoBehaviour
    {
        [SerializeField]
        private SignalPath path;

        [SerializeField]
        private VisualEffect visualEffect;

        [SerializeField, Range(0f, 1f)]
        private float progress;

        private static readonly int ProgressID =
            Shader.PropertyToID("Progress");

        private void Awake()
        {
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Apply();
        }
#endif

        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            Apply();
        }

        private void Apply()
        {
            if (visualEffect == null)
                return;

            visualEffect.SetFloat(
                ProgressID,
                progress
            );
        }
    }
}