using UnityEngine;
namespace ProjectSpark.AAAArchitecture
{
    public sealed class SparkLivePreviewSettings : ScriptableObject
    {
        public const string Root = "SparkLivePreview/";
        public const string LayerName = "SparkPreview";
        public bool defaultLive = true;
        public bool autoOrbit = false;
        public bool autoRefresh = true;
        [Range(10,30)] public int orbitFps = 20;
        [Range(256,1920)] public int desktopWidth = 1280;
        [Range(256,768)] public int mobileWidth = 512;
        [Min(1)] public int desktopDrawBudget = 800;
        [Min(1)] public int mobileDrawBudget = 250;
        public long desktopIndexBudget = 6000000;
        public long mobileIndexBudget = 1500000;
        public Sprite laboratoryBackdrop;
    }
}
