using UnityEngine;

namespace ProjectSpark.Gameplay.Placement
{
    public sealed class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        public Selectable CurrentSelection { get; private set; }

        private UnityEngine.Camera _camera;

        private void Awake()
        {
            Instance = this;

            _camera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            Ray ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            var selectable = hit.collider.GetComponent<Selectable>();

            if (selectable == CurrentSelection)
                return;

            CurrentSelection?.OnDeselected();

            CurrentSelection = selectable;

            CurrentSelection?.OnSelected();
        }
    }
}
