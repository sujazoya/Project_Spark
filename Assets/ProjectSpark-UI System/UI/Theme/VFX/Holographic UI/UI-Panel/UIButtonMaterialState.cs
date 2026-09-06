using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectSpark.UI
{
    [RequireComponent(typeof(Graphic))]
    public sealed class UIButtonMaterialState : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        ISelectHandler,
        IDeselectHandler
    {
        [SerializeField] private Graphic targetGraphic;

        private Material runtimeMaterial;

        private static readonly int PressedID =
            Shader.PropertyToID("_Pressed");

        private static readonly int SelectedID =
            Shader.PropertyToID("_Selected");

        private static readonly int DisabledID =
            Shader.PropertyToID("_Disabled");

        private static readonly int WarningID =
            Shader.PropertyToID("_Warning");

        private static readonly int HoverID =
            Shader.PropertyToID("_Hover");

        private void Awake()
        {
            if (targetGraphic == null)
                targetGraphic = GetComponent<Graphic>();

            if (targetGraphic.material != null)
            {
                runtimeMaterial = Instantiate(targetGraphic.material);
                targetGraphic.material = runtimeMaterial;
            }

            UpdateDisabledState();
        }

        private void OnEnable()
        {
            UpdateDisabledState();
        }

        private void Update()
        {
            UpdateDisabledState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetState(HoverID, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetState(HoverID, false);
            SetState(PressedID, false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetState(PressedID, true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetState(PressedID, false);
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetState(SelectedID, true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetState(SelectedID, false);
        }

        public void SetWarning(bool value)
        {
            SetState(WarningID, value);
        }

        private void UpdateDisabledState()
        {
            if (TryGetComponent(out Selectable selectable))
            {
                SetState(
                    DisabledID,
                    !selectable.interactable
                );
            }
        }

        private void SetState(int propertyID, bool value)
        {
            if (runtimeMaterial == null)
                return;

            runtimeMaterial.SetFloat(propertyID, value ? 1f : 0f);
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }
        }
    }
}