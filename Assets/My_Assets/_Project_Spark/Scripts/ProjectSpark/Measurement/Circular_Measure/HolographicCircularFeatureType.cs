namespace ProjectSpark.HolographicViewer
{
    /// <summary>
    /// Defines the semantic type of a circular mechanical/geometric feature.
    /// </summary>
    public enum HolographicCircularFeatureType
    {
        /// <summary>
        /// No circular feature.
        /// </summary>
        None = 0,

        /// <summary>
        /// A planar circular feature.
        /// </summary>
        Circle = 10,

        /// <summary>
        /// An external cylindrical feature.
        /// </summary>
        Cylinder = 20,

        /// <summary>
        /// An internal cylindrical feature representing a hole/bore.
        /// </summary>
        Hole = 30,

        /// <summary>
        /// An external cylindrical feature representing a shaft.
        /// </summary>
        Shaft = 40,

        /// <summary>
        /// A bearing or bearing-like circular mechanical feature.
        /// </summary>
        Bearing = 50
    }
}