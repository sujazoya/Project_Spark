using UnityEngine;
using UnityEngine.UI;
namespace ProjectSpark.AAAArchitecture
{
    // One active clock per root canvas, not per card. Unscaled time works in pause menus.
    [DisallowMultipleComponent]
    public sealed class SparkCinematicClock : MonoBehaviour
    {
        private static readonly int ClockId = Shader.PropertyToID("_SparkCinematicClock");
        private Canvas canvas;
        public static bool ReducedMotion { get; set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => ReducedMotion = PlayerPrefs.GetInt("ProjectSpark.FinalHD.ReducedMotion",0)!=0;
        private void OnEnable() { canvas = GetComponent<Canvas>(); }
        private void Update()
        {
            if (canvas && canvas.isActiveAndEnabled)
                Shader.SetGlobalFloat(ClockId, ReducedMotion ? 0f : Mathf.Repeat(Time.unscaledTime, 600f));
        }
    }
}
