using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Core
{
    /// <summary>
    /// Manages persistent and contextual UI overlays.
    /// </summary>
    public sealed class OverlayManager : MonoBehaviour
    {
        [SerializeField]
        private Transform overlayRoot;

        private readonly Dictionary<string, GameObject> overlays =
            new Dictionary<string, GameObject>();

        public void Initialize()
        {
            overlays.Clear();

            if (overlayRoot == null)
            {
                Debug.LogError(
                    "OverlayManager requires an Overlay Root.",
                    this);

                return;
            }

            Transform[] children =
                overlayRoot.GetComponentsInChildren<Transform>(
                    true);

            foreach (Transform child in children)
            {
                if (child == overlayRoot)
                    continue;

                if (overlays.ContainsKey(child.name))
                {
                    Debug.LogWarning(
                        $"Duplicate Overlay ID: {child.name}",
                        child);

                    continue;
                }

                overlays.Add(
                    child.name,
                    child.gameObject);

                child.gameObject.SetActive(false);
            }
        }

        public bool Show(string overlayId)
        {
            if (!overlays.TryGetValue(
                    overlayId,
                    out GameObject overlay))
            {
                Debug.LogError(
                    $"Overlay '{overlayId}' was not found.",
                    this);

                return false;
            }

            overlay.SetActive(true);
            return true;
        }

        public bool Hide(string overlayId)
        {
            if (!overlays.TryGetValue(
                    overlayId,
                    out GameObject overlay))
            {
                return false;
            }

            overlay.SetActive(false);
            return true;
        }

        public bool SetVisible(
            string overlayId,
            bool visible)
        {
            return visible
                ? Show(overlayId)
                : Hide(overlayId);
        }

        public bool IsVisible(string overlayId)
        {
            if (!overlays.TryGetValue(
                    overlayId,
                    out GameObject overlay))
            {
                return false;
            }

            return overlay.activeSelf;
        }
    }
}