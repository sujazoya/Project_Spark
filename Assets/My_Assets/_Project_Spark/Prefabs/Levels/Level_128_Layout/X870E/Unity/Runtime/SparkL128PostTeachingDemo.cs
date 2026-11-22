namespace ProjectSpark.Level128
{
    /// <summary>Illustrative POST ordering only. Not official BIOS codes or electrical authority.</summary>
    public static class SparkL128PostTeachingDemo
    {
        public enum Stage { PowerOff, StandbyFault, CpuRailFault, CpuMissing, MemoryFault, GraphicsFault, StorageFault, Ready }
        public static Stage Evaluate(bool powerRequested, bool standbyValid, bool cpuRailValid,
            bool cpuInstalled, bool memoryTrained, bool graphicsReady, bool bootStorageReady)
        {
            if (!powerRequested) return Stage.PowerOff;
            if (!standbyValid) return Stage.StandbyFault;
            if (!cpuRailValid) return Stage.CpuRailFault;
            if (!cpuInstalled) return Stage.CpuMissing;
            if (!memoryTrained) return Stage.MemoryFault;
            if (!graphicsReady) return Stage.GraphicsFault;
            if (!bootStorageReady) return Stage.StorageFault;
            return Stage.Ready;
        }
        // For the Ryzen 7000/9000 educational case only; real CPU and lane-mux support must be checked.
        public static int IllustrativeGraphicsLaneWidth(bool m2bOccupied, bool m2cOccupied)
            => m2bOccupied || m2cOccupied ? 8 : 16;
    }
}
