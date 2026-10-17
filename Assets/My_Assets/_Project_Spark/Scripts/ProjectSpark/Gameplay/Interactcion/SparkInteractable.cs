
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Generic Project Spark interaction target.
    /// Attach to an object with a Collider.
    /// Routes interaction to a universal switch lever controller.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkInteractable : MonoBehaviour
    {
        public enum InteractionType
        {
            Generic,
            SwitchLever
        }

        [Header("Interaction")]
        [SerializeField]
        private InteractionType interactionType =
            InteractionType.Generic;

        [SerializeField]
        private bool interactable = true;

        [SerializeField]
        private bool requirePlayerLookingAtObject = true;

        [Header("Switch Lever")]
        [SerializeField]
        private SwitchLeverController switchLeverController;

        [Header("Visual")]
        [SerializeField]
        private bool highlightOnHover = true;

        [SerializeField]
        private Renderer highlightRenderer;

        [SerializeField]
        private Color highlightColor =
            new Color(0.15f, 0.7f, 1f, 1f);

        [SerializeField, Range(0f, 1f)]
        private float highlightIntensity = 0.15f;

        private Material runtimeMaterial;
        private Color originalColor;
        private bool initialized;
        private bool highlighted;

        private static readonly int BaseColorID =
            Shader.PropertyToID("_BaseColor");

        public bool IsInteractable => interactable;

        public InteractionType Type => interactionType;

        public bool IsHighlighted => highlighted;

        public bool RequirePlayerLookingAtObject =>
            requirePlayerLookingAtObject;

        private void Awake()
        {
            if (switchLeverController == null)
                switchLeverController =
                    GetComponent<SwitchLeverController>();

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

            if (runtimeMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(runtimeMaterial);
                else
                    DestroyImmediate(runtimeMaterial);

                runtimeMaterial = null;
            }

            runtimeMaterial = highlightRenderer.material;

            if (runtimeMaterial == null ||
                !runtimeMaterial.HasProperty(BaseColorID))
            {
                initialized = false;
                return;
            }

            originalColor =
                runtimeMaterial.GetColor(BaseColorID);

            initialized = true;
        }

        public bool CanInteract()
        {
            return interactable && isActiveAndEnabled;
        }

        /// <summary>
        /// Called by the Project Spark interaction controller.
        /// </summary>
        public void Interact()
        {
            if (!CanInteract())
                return;

            switch (interactionType)
            {
                case InteractionType.SwitchLever:
                    if (switchLeverController != null)
                    {
                        switchLeverController.Activate();
                    }
                    else
                    {
                        Debug.LogWarning(
                            "SparkInteractable: SwitchLeverController " +
                            "is not assigned.",
                            this);
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
            if (!highlightOnHover || !initialized)
                return;

            if (highlighted == value)
                return;

            highlighted = value;

            runtimeMaterial.SetColor(
                BaseColorID,
                highlighted
                    ? originalColor +
                      highlightColor * highlightIntensity
                    : originalColor);
        }

        public void SetHighlightRenderer(Renderer renderer)
        {
            highlightRenderer = renderer;
            InitializeHighlight();
        }

        public void SetSwitchLeverController(
            SwitchLeverController controller)
        {
            switchLeverController = controller;
        }
    }
}
