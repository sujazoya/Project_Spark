using UnityEngine;

namespace ProjectSpark.Core.Bootstrap
{
    internal static class ApplicationInitializer
    {
        public static void Configure()
        {
            Application.targetFrameRate = 120;

            QualitySettings.vSyncCount = 0;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Physics.autoSimulation = true;

            Debug.unityLogger.logEnabled = true;
        }
    }
}
