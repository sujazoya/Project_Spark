using System;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [Serializable]
    public struct HolographicSnapSettings
    {
        [Header("Enable")]

        public bool vertexEnabled;

        public bool edgeEnabled;

        public bool faceCenterEnabled;

        public bool surfaceEnabled;

        [Header("Screen Distance")]

        [Min(1f)]
        public float vertexPixelDistance;

        [Min(1f)]
        public float edgePixelDistance;

        [Min(1f)]
        public float facePixelDistance;

        [Header("Priority")]

        [Range(0f, 2f)]
        public float vertexPriority;

        [Range(0f, 2f)]
        public float edgePriority;

        [Range(0f, 2f)]
        public float facePriority;

        [Header("Selection")]

        [Range(0f, 2f)]
        public float distanceWeight;

        [Range(0f, 2f)]
        public float priorityWeight;

        [Header("Depth")]

        public bool useDepthFiltering;

        [Min(0f)]
        public float maxDepthDifference;

        [Range(0f, 2f)]
        public float depthWeight;

        [Header("Stability")]

        public bool useHysteresis;

        [Min(0f)]
        public float switchMarginPixels;

        [Min(0f)]
        public float stickyDistancePixels;

        [Header("Targeting")]

        public bool restrictToHitObject;

        [Header("Visibility")]

        public bool requireFrontFacingFace;
    }
}