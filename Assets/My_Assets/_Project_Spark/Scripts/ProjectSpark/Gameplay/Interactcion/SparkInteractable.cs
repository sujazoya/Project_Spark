
using UnityEngine;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkInteractable : MonoBehaviour
    {
        public enum InteractionType
        {
            Generic = 0,
            SwitchLever = 1,
            ShelfItem = 2
        }

        [Header("Interaction")]
        [SerializeField] private InteractionType interactionType = InteractionType.Generic;
        [SerializeField] private bool interactable = true;
        [SerializeField] private bool requirePlayerLookingAtObject = true;

        [Header("Switch Lever")]
        [SerializeField] private SwitchLeverController switchLeverController;

        [Header("Shelf Interaction")]
        [SerializeField] private SparkShelfItem shelfItem;
        [SerializeField] private SparkObjectShelf objectShelf;
        [SerializeField] private SparkManualShelfPlacement manualPlacement;
        [SerializeField] private bool operateSwitchWhenPlaced = true;

        [Header("Visual")]
        [SerializeField] private bool highlightOnHover = true;
        [SerializeField] private Renderer highlightRenderer;
        [SerializeField] private Color highlightColor =
            new Color(.15f, .7f, 1f, 1f);
        [SerializeField, Range(0f, 1f)]
        private float highlightIntensity = .15f;

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
        public bool IsShelfItem =>
            interactionType == InteractionType.ShelfItem;

        private void Awake()
        {
            ResolveReferences();
            InitializeHighlight();
        }

        private void ResolveReferences()
        {
            if (switchLeverController == null)
                switchLeverController =
                    GetComponent<SwitchLeverController>();

            if (switchLeverController == null)
                switchLeverController =
                    GetComponentInChildren<SwitchLeverController>(true);

            if (shelfItem == null)
                shelfItem = GetComponent<SparkShelfItem>();

            if (shelfItem == null)
                shelfItem = GetComponentInParent<SparkShelfItem>();

            if (objectShelf == null)
                objectShelf = GetComponentInParent<SparkObjectShelf>();

            if (objectShelf == null && shelfItem != null)
                objectShelf =
                    shelfItem.GetComponentInParent<SparkObjectShelf>();

            if (manualPlacement == null)
                manualPlacement =
                    GetComponentInParent<SparkManualShelfPlacement>();

            if (manualPlacement == null && objectShelf != null)
                manualPlacement =
                    objectShelf.GetComponent<SparkManualShelfPlacement>();

            if (manualPlacement == null)
                manualPlacement =
                    FindFirstObjectByType<SparkManualShelfPlacement>();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial == null) return;

            if (Application.isPlaying)
                Destroy(runtimeMaterial);
            else
                DestroyImmediate(runtimeMaterial);
        }

        private void InitializeHighlight()
        {
            if (highlightRenderer == null) return;

            if (runtimeMaterial != null)
            {
                if (Application.isPlaying)
                    Destroy(runtimeMaterial);
                else
                    DestroyImmediate(runtimeMaterial);
            }

            runtimeMaterial = highlightRenderer.material;

            initialized =
                runtimeMaterial != null &&
                runtimeMaterial.HasProperty(BaseColorID);

            if (initialized)
                originalColor =
                    runtimeMaterial.GetColor(BaseColorID);
        }

        public bool CanInteract()
        {
            return interactable && isActiveAndEnabled;
        }

        public void Interact()
        {
            if (!CanInteract()) return;

            switch (interactionType)
            {
                case InteractionType.SwitchLever:
                    ActivateSwitch();
                    break;

                case InteractionType.ShelfItem:
                    InteractWithShelf();
                    break;
            }
        }

        private void InteractWithShelf()
        {
            if (shelfItem == null || manualPlacement == null)
                ResolveReferences();

            if (shelfItem == null || manualPlacement == null)
            {
                Debug.LogWarning(
                    "[SparkInteractable] Missing shelf item " +
                    "or manual placement controller.",
                    this);
                return;
            }

            if (shelfItem.IsMoving || !shelfItem.AllowTouch)
                return;

            // Placed switches remain operational.
            if (operateSwitchWhenPlaced &&
                switchLeverController != null &&
                shelfItem.IsAtWorkbench)
            {
                ActivateSwitch();
                return;
            }

            // First click: select item.
            // Clicking a placed non-switch item returns it to storage.
            manualPlacement.SelectItem(shelfItem);
        }

        private void ActivateSwitch()
        {
            if (switchLeverController == null)
                ResolveReferences();

            if (switchLeverController != null)
                switchLeverController.Activate();
            else
                Debug.LogWarning(
                    "[SparkInteractable] SwitchLeverController missing.",
                    this);
        }

        public void SetInteractable(bool value)
        {
            interactable = value;
            if (!value) SetHighlighted(false);
        }

        public void SetInteractionType(InteractionType value)
        {
            interactionType = value;
        }

        public void SetShelfReferences(
            SparkShelfItem item,
            SparkObjectShelf shelf)
        {
            shelfItem = item;
            objectShelf = shelf;
        }

        public void SetManualPlacement(
            SparkManualShelfPlacement placement)
        {
            manualPlacement = placement;
        }

        public void SetOperateSwitchWhenPlaced(bool value)
        {
            operateSwitchWhenPlaced = value;
        }

        public void SetHighlighted(bool value)
        {
            if (!initialized ||
                !highlightOnHover ||
                highlighted == value)
                return;

            highlighted = value;

            runtimeMaterial.SetColor(
                BaseColorID,
                value
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
