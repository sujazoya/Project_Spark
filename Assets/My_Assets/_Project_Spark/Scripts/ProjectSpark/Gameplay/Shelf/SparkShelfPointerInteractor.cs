using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSpark.Gameplay.Shelf
{
    /// <summary>Optional desktop/mobile pointer adapter. Existing interaction systems can call shelf.Touch(item) directly.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkShelfPointerInteractor : MonoBehaviour
    {
        [SerializeField] private SparkObjectShelf shelf;
        [SerializeField] private Camera interactionCamera;
        [SerializeField] private LayerMask itemLayers = ~0;
        [SerializeField, Min(0.1f)] private float maximumDistance = 25f;
        [SerializeField] private bool ignorePointerOverUI = true;

        private void Reset() { interactionCamera = Camera.main; }
        private void Awake() { if (interactionCamera == null) interactionCamera = Camera.main; }
        private void Update()
        {
            if (shelf == null || interactionCamera == null) return;
            if (Input.GetMouseButtonDown(0)) TryTouch(Input.mousePosition, -1);
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began) TryTouch(touch.position, touch.fingerId);
            }
        }
        private void TryTouch(Vector2 screenPoint, int pointerId)
        {
            if (ignorePointerOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId)) return;
            Ray ray = interactionCamera.ScreenPointToRay(screenPoint);
            if (!Physics.Raycast(ray, out RaycastHit hit, maximumDistance, itemLayers, QueryTriggerInteraction.Collide)) return;
            SparkShelfItem item = hit.collider.GetComponentInParent<SparkShelfItem>();
            if (item != null) shelf.Touch(item);
        }
    }
}
