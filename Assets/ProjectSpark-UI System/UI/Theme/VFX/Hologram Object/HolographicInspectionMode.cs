namespace ProjectSpark.HolographicViewer
{
    public enum HolographicInspectionMode
    {
        Normal = 0,
        XRay = 1,
        Internal = 2,
        Exploded = 3,
        Wireframe = 4
    }

    public static class HolographicInspectionModeInfo
    {
        public static string GetTitle(
            HolographicInspectionMode mode)
        {
            return mode switch
            {
                HolographicInspectionMode.Normal =>
                    "NORMAL VIEW",

                HolographicInspectionMode.XRay =>
                    "X-RAY VIEW",

                HolographicInspectionMode.Internal =>
                    "INTERNAL VIEW",

                HolographicInspectionMode.Exploded =>
                    "EXPLODED VIEW",

                HolographicInspectionMode.Wireframe =>
                    "WIREFRAME VIEW",

                _ =>
                    "NORMAL VIEW"
            };
        }

        public static string GetDescription(
            HolographicInspectionMode mode)
        {
            return mode switch
            {
                HolographicInspectionMode.Normal =>
                    "Standard holographic object visualization.",

                HolographicInspectionMode.XRay =>
                    "Transparent inspection view for examining internal structure.",

                HolographicInspectionMode.Internal =>
                    "Internal components are revealed while the outer structure remains visible.",

                HolographicInspectionMode.Exploded =>
                    "Major components are separated spatially for engineering inspection.",

                HolographicInspectionMode.Wireframe =>
                    "Geometric edge representation of the selected object.",

                _ =>
                    "Standard holographic object visualization."
            };
        }

        public static string GetShortName(
            HolographicInspectionMode mode)
        {
            return mode switch
            {
                HolographicInspectionMode.Normal => "NORMAL",
                HolographicInspectionMode.XRay => "X-RAY",
                HolographicInspectionMode.Internal => "INTERNAL",
                HolographicInspectionMode.Exploded => "EXPLODED",
                HolographicInspectionMode.Wireframe => "WIREFRAME",
                _ => "NORMAL"
            };
        }
    }
}