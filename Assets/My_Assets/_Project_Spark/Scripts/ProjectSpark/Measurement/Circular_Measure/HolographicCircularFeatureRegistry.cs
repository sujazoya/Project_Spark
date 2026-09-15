using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.HolographicViewer
{
    [DisallowMultipleComponent]
    public sealed class HolographicCircularFeatureRegistry
        : MonoBehaviour
    {
        [Header("Features")]

        [SerializeField]
        private HolographicCircularFeature[] features;

        [SerializeField]
        private bool autoDiscover = true;

        [SerializeField]
        private bool includeInactiveChildren = true;

        private readonly List<HolographicCircularFeature>
            activeFeatures =
                new List<HolographicCircularFeature>();

        public int Count =>
            activeFeatures.Count;

        public IReadOnlyList<HolographicCircularFeature>
            Features =>
            activeFeatures;


        private void Awake()
        {
            Rebuild();
        }


        // ============================================================
        // REBUILD
        // ============================================================

        public void Rebuild()
        {
            activeFeatures.Clear();

            if (autoDiscover)
            {
                DiscoverFeatures();
            }
            else if (features != null)
            {
                for (int i = 0;
                     i < features.Length;
                     i++)
                {
                    Register(
                        features[i]
                    );
                }
            }
        }


        // ============================================================
        // DISCOVER
        // ============================================================

        private void DiscoverFeatures()
        {
            HolographicCircularFeature[] discovered =
                GetComponentsInChildren<
                    HolographicCircularFeature>(
                        includeInactiveChildren
                    );

            for (int i = 0;
                 i < discovered.Length;
                 i++)
            {
                Register(
                    discovered[i]
                );
            }
        }


        // ============================================================
        // REGISTER
        // ============================================================

        public void Register(
            HolographicCircularFeature feature)
        {
            if (feature == null)
                return;

            if (!feature.EnabledForMeasurement)
                return;

            if (activeFeatures.Contains(feature))
                return;

            activeFeatures.Add(
                feature
            );
        }


        // ============================================================
        // UNREGISTER
        // ============================================================

        public void Unregister(
            HolographicCircularFeature feature)
        {
            if (feature == null)
                return;

            activeFeatures.Remove(
                feature
            );
        }


        // ============================================================
        // TYPE QUERY
        // ============================================================

        public int GetFeaturesOfType(
            HolographicCircularFeatureType type,
            List<HolographicCircularFeature> results)
        {
            if (results == null)
                return 0;

            results.Clear();

            for (int i = 0;
                 i < activeFeatures.Count;
                 i++)
            {
                HolographicCircularFeature feature =
                    activeFeatures[i];

                if (feature == null)
                    continue;

                if (feature.FeatureType != type)
                    continue;

                results.Add(
                    feature
                );
            }

            return results.Count;
        }


        // ============================================================
        // CAPABILITY QUERY
        // ============================================================

        public int GetFeaturesSupporting(
            HolographicCircularMeasurementCapabilities
                capability,
            List<HolographicCircularFeature> results)
        {
            if (results == null)
                return 0;

            results.Clear();

            for (int i = 0;
                 i < activeFeatures.Count;
                 i++)
            {
                HolographicCircularFeature feature =
                    activeFeatures[i];

                if (feature == null)
                    continue;

                if (!feature.Supports(
                        capability))
                {
                    continue;
                }

                results.Add(
                    feature
                );
            }

            return results.Count;
        }
    }
}