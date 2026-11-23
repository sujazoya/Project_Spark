using UnityEngine;
namespace ProjectSpark.AAAArchitecture
{
    public sealed class SparkFinalPresentationSettings : ScriptableObject
    {
        public const string Root = "SparkFinalHD/";
        public bool reducedMotion;
        [Range(0f,.5f)] public float entranceSeconds = .22f;
        [Range(0f,.5f)] public float briefingSeconds = .14f;
        public Sprite[] chapterIcons = new Sprite[13];
        // Optional original, text-free artwork. Never a screenshot of the UI.
        public Sprite laboratoryBackdrop;
        [Range(0f,1f)] public float backdropOpacity = .55f;
    }
}
