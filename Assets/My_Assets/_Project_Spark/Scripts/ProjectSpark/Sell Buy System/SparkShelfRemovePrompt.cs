
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectSpark.Gameplay.Shelf;

namespace ProjectSpark.Gameplay
{
    /// <summary>
    /// Single floating recycle icon for a selected shelf item.
    ///
    /// Uses Screen Space Overlay to avoid world-canvas
    /// scale, clipping and camera-facing problems.
    ///
    /// Item selection comes from SparkInteractionController.
    /// No additional physics raycast or recycle-bin collider.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkShelfRemovePrompt : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private SparkLevelLayoutLoader layoutLoader;
        [SerializeField] private Camera viewingCamera;

        [Header("Recycle Icon")]
        [SerializeField] private Sprite recycleIcon;

        [SerializeField] private Color backgroundColor =
            new Color(.75f, .16f, .12f, .95f);

        [SerializeField] private Color iconColor = Color.white;

        [SerializeField, Min(24f)]
        private float buttonSize = 64f;

        [SerializeField, Range(0f, 24f)]
        private float iconPadding = 10f;

        [Header("Popup Position")]
        [SerializeField] private Vector3 worldOffset =
            new Vector3(0f, .3f, 0f);

        [SerializeField] private Vector2 screenOffset =
            Vector2.zero;

        [Header("Behaviour")]
        [SerializeField] private bool toggleOnSameItem = true;
        [SerializeField] private bool disableWhileMoving = true;
        [SerializeField] private bool logResults = true;

        private SparkShelfItem selectedItem;

        private GameObject canvasObject;
        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform buttonRect;
        private Button removeButton;
        private Image iconImage;

        private bool removing;
        private bool subscribed;

        public SparkShelfItem SelectedItem => selectedItem;
        public bool HasSelection => selectedItem != null;

        private void Awake()
        {
            ResolveReferences();
            CreatePrompt();
            HidePrompt();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeLoader();
        }

        private void OnDisable()
        {
            UnsubscribeLoader();
            ClearSelection();
        }

        private void OnDestroy()
        {
            UnsubscribeLoader();

            if (removeButton != null)
                removeButton.onClick.RemoveListener(
                    RemoveSelectedItem);

            if (canvasObject != null)
                Destroy(canvasObject);
        }

        private void ResolveReferences()
        {
            if (shelf == null)
                shelf = FindFirstObjectByType<SparkObjectShelf>();

            if (layoutLoader == null)
                layoutLoader =
                    FindFirstObjectByType<SparkLevelLayoutLoader>();

            if (viewingCamera == null)
                viewingCamera = Camera.main;
        }

        // =====================================================
        // LEVEL EVENTS
        // =====================================================

        private void SubscribeLoader()
        {
            if (subscribed || layoutLoader == null)
                return;

            layoutLoader.LayoutLoaded += OnLayoutLoaded;
            layoutLoader.LayoutUnloaded += OnLayoutUnloaded;

            subscribed = true;
        }

        private void UnsubscribeLoader()
        {
            if (!subscribed)
                return;

            if (layoutLoader != null)
            {
                layoutLoader.LayoutLoaded -= OnLayoutLoaded;
                layoutLoader.LayoutUnloaded -= OnLayoutUnloaded;
            }

            subscribed = false;
        }

        private void OnLayoutLoaded(SparkLevelLayout layout)
        {
            ClearSelection();
        }

        private void OnLayoutUnloaded()
        {
            ClearSelection();
        }

        // =====================================================
        // VALIDATION
        // =====================================================

        private bool IsRegistered(SparkShelfItem item)
        {
            if (item == null || shelf == null)
                return false;

            var registered = shelf.RegisteredItems;

            if (registered == null)
                return false;

            foreach (SparkShelfItem entry in registered)
            {
                if (entry == item)
                    return true;
            }

            return false;
        }

        private bool CanDisplayFor(SparkShelfItem item)
        {
            return item != null &&
                   item.isActiveAndEnabled &&
                   item.gameObject.activeInHierarchy &&
                   item.AllowTouch &&
                   IsRegistered(item);
        }

        // =====================================================
        // SELECTION
        // =====================================================

        public void SelectItem(SparkShelfItem item)
        {

            
            ResolveReferences();
            Debug.Log(
    "[ShelfRemove] SelectItem called | Item=" +
    (item != null ? item.name : "NULL") +
    " | Registered=" + IsRegistered(item) +
    " | AllowTouch=" +
    (item != null && item.AllowTouch) +
    " | Moving=" +
    (item != null && item.IsMoving),
    this);

            if (item == null)
            {
                ClearSelection();
                return;
            }

            if (!CanDisplayFor(item))
            {
                if (logResults)
                {
                    Debug.LogWarning(
                        "[ShelfRemove] Selection rejected: " +
                        item.name +
                        " | Registered=" + IsRegistered(item) +
                        " | Active=" + item.isActiveAndEnabled +
                        " | AllowTouch=" + item.AllowTouch,
                        item);
                }

                ClearSelection();
                return;
            }

            if (selectedItem == item && toggleOnSameItem)
            {
                ClearSelection();
                return;
            }

            selectedItem = item;

            if (logResults)
            {
                Debug.Log(
                    "[ShelfRemove] Selected: " + item.name,
                    item);
            }

            UpdatePromptPosition();
        }

        public void ClearSelection()
        {
            selectedItem = null;
            HidePrompt();
        }

        // =====================================================
        // SCREEN FOLLOW
        // =====================================================

        private void LateUpdate()
        {
            if (selectedItem == null)
            {
                HidePrompt();
                return;
            }

            if (!CanDisplayFor(selectedItem))
            {
                ClearSelection();
                return;
            }

            UpdatePromptPosition();

            if (removeButton != null)
            {
                removeButton.interactable =
                    !disableWhileMoving ||
                    !selectedItem.IsMoving;
            }
        }

        private void UpdatePromptPosition()
        {
            if (selectedItem == null ||
                buttonRect == null ||
                canvasRect == null)
                return;

            if (viewingCamera == null)
                viewingCamera = Camera.main;

            if (viewingCamera == null)
            {
                HidePrompt();
                return;
            }

            Vector3 worldPosition =
                selectedItem.transform.position + worldOffset;

            Vector3 screenPosition =
                viewingCamera.WorldToScreenPoint(worldPosition);

            // Item is behind the camera.
            if (screenPosition.z <= 0f)
            {
                HidePrompt();
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    new Vector2(
                        screenPosition.x,
                        screenPosition.y),
                    null,
                    out Vector2 localPosition))
            {
                HidePrompt();
                return;
            }

            // Prevent the icon from appearing off-screen.
            Vector2 half = canvasRect.rect.size * .5f;
            float margin = buttonSize * .5f;

            localPosition += screenOffset;

            localPosition.x = Mathf.Clamp(
                localPosition.x,
                -half.x + margin,
                half.x - margin);

            localPosition.y = Mathf.Clamp(
                localPosition.y,
                -half.y + margin,
                half.y - margin);

            buttonRect.anchoredPosition = localPosition;

            ShowPrompt();
        }

        // =====================================================
        // CREATE ICON BUTTON
        // =====================================================

        private void CreatePrompt()
        {
            if (canvasObject != null)
                return;

            canvasObject = new GameObject(
                "SparkShelf_RecycleCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));

            // Independent overlay canvas.
            canvasObject.transform.SetParent(null, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            canvasRect =
                canvasObject.GetComponent<RectTransform>();

            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            GameObject buttonObject = new GameObject(
                "RecycleButton",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));

            buttonObject.transform.SetParent(
                canvasObject.transform, false);

            buttonRect =
                buttonObject.GetComponent<RectTransform>();

            buttonRect.anchorMin =
                new Vector2(.5f, .5f);

            buttonRect.anchorMax =
                new Vector2(.5f, .5f);

            buttonRect.pivot =
                new Vector2(.5f, .5f);

            buttonRect.sizeDelta =
                new Vector2(buttonSize, buttonSize);

            Image background =
                buttonObject.GetComponent<Image>();

            background.color = backgroundColor;
            background.raycastTarget = true;

            removeButton =
                buttonObject.GetComponent<Button>();

            removeButton.targetGraphic = background;
            removeButton.onClick.AddListener(
                RemoveSelectedItem);

            GameObject iconObject = new GameObject(
                "RecycleIcon",
                typeof(RectTransform),
                typeof(Image));

            iconObject.transform.SetParent(
                buttonObject.transform, false);

            RectTransform iconRect =
                iconObject.GetComponent<RectTransform>();

            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;

            iconRect.offsetMin =
                new Vector2(iconPadding, iconPadding);

            iconRect.offsetMax =
                new Vector2(-iconPadding, -iconPadding);

            iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = recycleIcon;
            iconImage.color = iconColor;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            if (recycleIcon == null && logResults)
            {
                Debug.LogWarning(
                    "[ShelfRemove] Recycle Sprite not assigned. " +
                    "The button background will still appear.",
                    this);
            }
        }

        private void ShowPrompt()
        {
            if (canvasObject == null)
                CreatePrompt();

            if (canvasObject != null &&
                !canvasObject.activeSelf)
                canvasObject.SetActive(true);
        }

        private void HidePrompt()
        {
            if (canvasObject != null &&
                canvasObject.activeSelf)
                canvasObject.SetActive(false);
        }

        // =====================================================
        // REMOVE
        // =====================================================

        public void RemoveSelectedItem()
        {
            if (removing)
                return;

            ResolveReferences();

            SparkShelfItem item = selectedItem;

            if (item == null || shelf == null)
            {
                ClearSelection();
                return;
            }

            removing = true;

            try
            {
                if (!shelf.TryRecycleItem(
                        item, out string reason))
                {
                    if (logResults)
                    {
                        Debug.LogWarning(
                            "[ShelfRemove] Removal rejected: " +
                            item.ItemId + " | " + reason,
                            item);
                    }

                    return;
                }

                if (layoutLoader != null &&
                    layoutLoader.ActiveBindings != null)
                {
                    layoutLoader.ActiveBindings
                        .RetireRuntimeBindingsOwnedBy(
                            item.transform);
                }

                if (logResults)
                {
                    Debug.Log(
                        "[ShelfRemove] Removed: " +
                        item.ItemId,
                        this);
                }

                ClearSelection();

                Destroy(item.gameObject);
            }
            finally
            {
                removing = false;
            }
        }
        
    }
}
