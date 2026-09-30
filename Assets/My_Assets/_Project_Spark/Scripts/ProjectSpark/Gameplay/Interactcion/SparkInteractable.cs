using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Generic Project Spark interaction target.
    ///
    /// Attach this to an object with a Collider.
    ///
    /// The interaction controller detects this component and calls
    /// Interact().
    ///
    /// Optional specialized behavior can be assigned through the
    /// interaction type.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkInteractable : MonoBehaviour
    {
        public enum InteractionType
        {
            Generic,
            HairDryerControl
        }

        [Header("Interaction")]
        [SerializeField]
        private InteractionType interactionType =
            InteractionType.Generic;

        [SerializeField]
        private bool interactable = true;

        [SerializeField]
        private bool requirePlayerLookingAtObject = true;

        [Header("Hair Dryer")]
        [SerializeField]
        private SparkHairDryerControlInteraction
            hairDryerControl;

        [Header("Visual")]
        [SerializeField]
        private bool highlightOnHover = true;

        [SerializeField]
        private Renderer highlightRenderer;

        [SerializeField]
        private Color highlightColor =
            new Color(0.15f, 0.7f, 1f, 1f);

        [SerializeField]
        [Range(0f, 1f)]
        private float highlightIntensity = 0.15f;

        private Material runtimeMaterial;
        private Color originalColor;
        private bool initialized;
        private bool highlighted;

        private static readonly int BaseColorID =
            Shader.PropertyToID("_BaseColor");

        public bool IsInteractable =>
            interactable;

        public InteractionType Type =>
            interactionType;

        public bool IsHighlighted =>
            highlighted;

        private void Awake()
        {
            InitializeHighlight();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial == null)
                return;

            if (Application.isPlaying)
                Destroy(runtimeMaterial);
            else
                DestroyImmediate(runtimeMaterial);
        }

        private void InitializeHighlight()
        {
            if (highlightRenderer == null)
                return;

            runtimeMaterial =
                highlightRenderer.material;

            if (runtimeMaterial == null)
                return;

            if (!runtimeMaterial.HasProperty(BaseColorID))
                return;

            originalColor =
                runtimeMaterial.GetColor(BaseColorID);

            initialized = true;
        }

        public bool CanInteract()
        {
            return interactable;
        }

        public void Interact()
        {
            if (!CanInteract())
                return;

            switch (interactionType)
            {
                case InteractionType.HairDryerControl:

                    if (hairDryerControl != null)
                    {
                        hairDryerControl.Activate();
                    }

                    break;

                case InteractionType.Generic:
                default:

                    break;
            }
        }

        public void SetInteractable(bool value)
        {
            interactable = value;

            if (!interactable)
                SetHighlighted(false);
        }

        public void SetHighlighted(bool value)
        {
            if (!highlightOnHover)
                return;

            if (!initialized)
                return;

            if (highlighted == value)
                return;

            highlighted = value;

            if (highlighted)
            {
                Color color =
                    originalColor +
                    highlightColor * highlightIntensity;

                runtimeMaterial.SetColor(
                    BaseColorID,
                    color);
            }
            else
            {
                runtimeMaterial.SetColor(
                    BaseColorID,
                    originalColor);
            }
        }

        public void SetHighlightRenderer(
            Renderer renderer)
        {
            highlightRenderer = renderer;

            InitializeHighlight();
        }
    }
}