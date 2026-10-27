
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkShelfDragHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private SparkManualShelfPlacement manualPlacement;
        [SerializeField] private SparkObjectShelf objectShelf;

        [Header("Drag Settings")]
        [SerializeField, Min(1f)]
        private float dragThresholdPixels = 10f;

        [SerializeField, Min(1f)]
        private float interactionDistance = 20f;

        [SerializeField]
        private LayerMask interactionLayers = ~0;

        [SerializeField, Min(10f)]
        private float storageDropRadiusPixels = 85f;

        [Header("Hologram")]
        [SerializeField]
        private Color previewColor =
            new Color(0f, 0.9f, 1f, 0.35f);

        [SerializeField]
        private float previewDepth = 4f;

        [Header("Debug")]
        [SerializeField]
        private bool logDragEvents = true;

        private SparkShelfItem pressedItem;
        private Vector2 pressPosition;

        private bool dragging;
        private bool suppressClick;

        private GameObject preview;
        private Renderer previewRenderer;

        public bool IsDragging => dragging;

        // Called by SparkInteractionController each frame.
        public void Tick()
        {
            ResolveReferences();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelDrag();
                return;
            }

            if (Input.GetMouseButtonDown(0))
                BeginPress();

            if (Input.GetMouseButton(0))
                UpdateDrag();

            if (Input.GetMouseButtonUp(0))
                EndPress();
        }

        // Called on mouse-down by the interaction controller.
        // Suppresses normal clicks on already-placed items.
        public bool ConsumeInteractionClick()
        {
            if (suppressClick)
            {
                suppressClick = false;
                return true;
            }

            return false;
        }

        private void ResolveReferences()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (manualPlacement == null)
            {
                manualPlacement =
                    FindFirstObjectByType<
                        SparkManualShelfPlacement>();
            }

            if (objectShelf == null)
            {
                objectShelf =
                    FindFirstObjectByType<SparkObjectShelf>();
            }
        }

        private void BeginPress()
        {
            pressedItem = null;
            dragging = false;
            suppressClick = false;

            if (IsPointerOverUI())
                return;

            pressedItem = FindItemUnderPointer();

            if (pressedItem == null)
                return;

            if (!pressedItem.AllowTouch ||
                pressedItem.IsMoving)
            {
                pressedItem = null;
                return;
            }

            pressPosition = Input.mousePosition;

            // Placed components are moved only by dragging.
            // Their first click must not return them.
            suppressClick = pressedItem.IsAtWorkbench;
        }

        private void UpdateDrag()
        {
            if (pressedItem == null)
                return;

            if (!dragging)
            {
                float distance = Vector2.Distance(
                    pressPosition,
                    Input.mousePosition);

                if (distance < dragThresholdPixels)
                    return;

                dragging = true;

                CreatePreview();

                if (logDragEvents)
                {
                    Debug.Log(
                        "[ShelfDrag] Started: " +
                        pressedItem.ItemId,
                        pressedItem);
                }
            }

            UpdatePreviewPosition();
        }

        private void EndPress()
        {
            if (pressedItem == null)
            {
                Cleanup();
                return;
            }

            if (!dragging)
            {
                Cleanup();
                return;
            }

            SparkShelfItem item = pressedItem;

            // A placed item may be returned to its
            // original storage slot.
            if (item.IsAtWorkbench &&
                IsPointerNearStorage(item))
            {
                TryReturn(item);
                Cleanup();
                return;
            }

            // Check workstation drop destination.
            SparkShelfSlot destination =
                FindWorkstationUnderPointer();

            if (destination != null)
            {
                TryPlace(item, destination);
                Cleanup();
                return;
            }

            if (logDragEvents)
            {
                Debug.Log(
                    "[ShelfDrag] Drop cancelled: " +
                    item.ItemId,
                    item);
            }

            Cleanup();
        }

        private void TryPlace(
            SparkShelfItem item,
            SparkShelfSlot slot)
        {
            if (manualPlacement == null)
            {
                Debug.LogWarning(
                    "[ShelfDrag] Manual placement missing.",
                    this);
                return;
            }

            // Existing manual placement owns level
            // permissions and destination validation.
            if (manualPlacement.SelectedItem != item)
            {
                if (!manualPlacement.SelectItem(item))
                {
                    Debug.LogWarning(
                        "[ShelfDrag] Cannot select " +
                        item.ItemId,
                        item);
                    return;
                }
            }

            bool success =
                manualPlacement.TryPlaceAt(slot);

            if (logDragEvents)
            {
                Debug.Log(
                    "[ShelfDrag] Place " +
                    item.ItemId +
                    " -> " +
                    slot.SlotId +
                    " : " +
                    (success ? "ACCEPTED" : "REJECTED"),
                    item);
            }
        }

        private void TryReturn(SparkShelfItem item)
        {
            if (objectShelf == null)
                return;

            // Preserve electrical safety checks.
            bool success =
                objectShelf.TryReturnToShelf(item);

            if (success)
                manualPlacement?.CancelSelection();

            if (logDragEvents)
            {
                Debug.Log(
                    "[ShelfDrag] Return " +
                    item.ItemId +
                    " : " +
                    (success ? "ACCEPTED" : "REJECTED"),
                    item);
            }
        }

        private SparkShelfItem FindItemUnderPointer()
        {
            RaycastHit[] hits = GetPointerHits();

            foreach (RaycastHit hit in hits)
            {
                SparkShelfItem item =
                    hit.collider.GetComponentInParent<
                        SparkShelfItem>();

                if (item != null)
                    return item;
            }

            return null;
        }

        private SparkShelfSlot FindWorkstationUnderPointer()
        {
            RaycastHit[] hits = GetPointerHits();

            foreach (RaycastHit hit in hits)
            {
                SparkShelfSlotClickTarget clickTarget =
                    hit.collider.GetComponentInParent<
                        SparkShelfSlotClickTarget>();

                if (clickTarget == null)
                    continue;

                SparkShelfSlot slot = clickTarget.Slot;

                if (slot == null ||
                    slot.Kind !=
                        SparkShelfSlotKind.Workstation)
                {
                    continue;
                }

                return slot;
            }

            return null;
        }

        private RaycastHit[] GetPointerHits()
        {
            if (targetCamera == null)
                return new RaycastHit[0];

            Ray ray = targetCamera.ScreenPointToRay(
                Input.mousePosition);

            RaycastHit[] hits = Physics.RaycastAll(
                ray,
                interactionDistance,
                interactionLayers,
                QueryTriggerInteraction.Ignore);

            System.Array.Sort(
                hits,
                (a, b) =>
                    a.distance.CompareTo(b.distance));

            return hits;
        }

        private bool IsPointerNearStorage(
            SparkShelfItem item)
        {
            if (item == null ||
                targetCamera == null)
                return false;

            SparkShelfSlot storage =
                item.PreferredStorageSlot;

            if (storage == null)
                return false;

            Vector3 screenPoint =
                targetCamera.WorldToScreenPoint(
                    storage.Anchor.position);

            if (screenPoint.z <= 0f)
                return false;

            return Vector2.Distance(
                Input.mousePosition,
                new Vector2(
                    screenPoint.x,
                    screenPoint.y)) <=
                storageDropRadiusPixels;
        }

       
private void CreatePreview()
{
    if (pressedItem == null)
        return;

    if (preview != null)
        Destroy(preview);

    // Root object contains visual meshes only.
    preview = new GameObject(
        "Spark_Drag_Hologram");

    preview.transform.position =
        pressedItem.transform.position;

    preview.transform.rotation =
        pressedItem.transform.rotation;

    preview.transform.localScale =
        pressedItem.transform.lossyScale;

    // Find mesh renderers in the actual component.
    MeshFilter[] meshFilters =
        pressedItem.GetComponentsInChildren<MeshFilter>(true);

    foreach (MeshFilter sourceFilter in meshFilters)
    {
        if (sourceFilter == null ||
            sourceFilter.sharedMesh == null)
            continue;

        MeshRenderer sourceRenderer =
            sourceFilter.GetComponent<MeshRenderer>();

        if (sourceRenderer == null ||
            !sourceRenderer.enabled)
            continue;

        GameObject meshCopy =
            new GameObject(
                sourceFilter.gameObject.name +
                "_Preview");

        meshCopy.transform.SetParent(
            preview.transform,
            false);

        // Preserve relative position/rotation.
        meshCopy.transform.position =
            sourceFilter.transform.position;

        meshCopy.transform.rotation =
            sourceFilter.transform.rotation;

        meshCopy.transform.localScale =
            sourceFilter.transform.lossyScale;

        MeshFilter newFilter =
            meshCopy.AddComponent<MeshFilter>();

        newFilter.sharedMesh =
            sourceFilter.sharedMesh;

        MeshRenderer newRenderer =
            meshCopy.AddComponent<MeshRenderer>();

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit");

        if (shader == null)
        {
            shader = Shader.Find(
                "Sprites/Default");
        }

        Material hologramMaterial =
            new Material(shader);

        hologramMaterial.color =
            new Color(
                0f,
                0.85f,
                1f,
                0.25f);

        newRenderer.sharedMaterial =
            hologramMaterial;
    }

    Debug.Log(
        "[ShelfDrag] Created mesh hologram for " +
        pressedItem.ItemId,
        pressedItem);
}


        private void UpdatePreviewPosition()
        {
            if (preview == null ||
                targetCamera == null)
                return;

            Ray ray = targetCamera.ScreenPointToRay(
                Input.mousePosition);

            Vector3 position =
                ray.GetPoint(previewDepth);

            // Keep the hologram close to the
            // workstation when a valid slot is hovered.
            SparkShelfSlot slot =
                FindWorkstationUnderPointer();

            if (slot != null)
            {
                position = slot.Anchor.position;
            }

            preview.transform.position = position;
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null &&
                   EventSystem.current
                       .IsPointerOverGameObject();
        }

        public void CancelDrag()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            pressedItem = null;
            dragging = false;
            suppressClick = false;

            if (preview != null)
            {
                if (previewRenderer != null &&
                    previewRenderer.material != null)
                {
                    Destroy(previewRenderer.material);
                }

                Destroy(preview);
            }

            preview = null;
            previewRenderer = null;
        }

        private void OnDisable()
        {
            Cleanup();
        }
    }
}
