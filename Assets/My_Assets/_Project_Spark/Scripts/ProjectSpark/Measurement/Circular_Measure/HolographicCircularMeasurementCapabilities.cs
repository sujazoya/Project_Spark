using System;

namespace ProjectSpark.HolographicViewer
{
    [Flags]
    public enum HolographicCircularMeasurementCapabilities
    {
        None = 0,

        Center = 1 << 0,

        Radius = 1 << 1,

        Diameter = 1 << 2,

        Depth = 1 << 3,

        Axis = 1 << 4,

        Rim = 1 << 5,

        Endpoint = 1 << 6,

        Normal = 1 << 7,

        All =
            Center |
            Radius |
            Diameter |
            Depth |
            Axis |
            Rim |
            Endpoint |
            Normal
    }
}