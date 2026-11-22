namespace ProjectSpark.Level130
{
    /// <summary>Educational POST state model only. Not MSI firmware or electrical authority.</summary>
    public static class SparkL130PostTeachingDemo
    {
        public enum Stage { Off, StandbyFault, CpuFault, DramFault, VgaFault, BootFault, Ready }
        public static Stage Evaluate(bool request, bool standbyValid, bool cpuRailValid, bool cpuInstalled, bool dramTrained, bool videoReady, bool storageReady)
        {
            if (!request) return Stage.Off;
            if (!standbyValid) return Stage.StandbyFault;
            if (!cpuRailValid || !cpuInstalled) return Stage.CpuFault;
            if (!dramTrained) return Stage.DramFault;
            if (!videoReady) return Stage.VgaFault;
            if (!storageReady) return Stage.BootFault;
            return Stage.Ready;
        }
    }
}
