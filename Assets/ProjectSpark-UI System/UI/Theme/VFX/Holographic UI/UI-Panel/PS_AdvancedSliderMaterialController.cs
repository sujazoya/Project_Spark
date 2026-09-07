
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ProjectSpark.UI
{
    [RequireComponent(typeof(Slider))]
    public sealed class PS_AdvancedSliderMaterialController : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private Slider slider;
        [SerializeField] private Image targetImage;

        [Header("Shader Properties")]
        [SerializeField] private string sliderValueProperty = "_SliderValue";
        [SerializeField] private string hoverProperty = "_Hover";
        [SerializeField] private string pressedProperty = "_Pressed";
        [SerializeField] private string flashProperty = "_Flash";

        [Header("Interaction")]
        [SerializeField] private bool updateShaderOnEnable = true;
        [SerializeField] private bool flashOnValueChanged = true;

        [Header("Flash")]
        [SerializeField] private float flashDuration = 0.12f;
        [SerializeField] private float flashIntensity = 1f;

        private Material runtimeMaterial;

        private int sliderValueID;
        private int hoverID;
        private int pressedID;
        private int flashID;

        private Coroutine flashCoroutine;

        private bool isPointerOver;
        private bool isPressed;

        private void Reset()
        {
            slider = GetComponent<Slider>();
            targetImage = GetComponent<Image>();
        }

        private void Awake()
        {
            if (slider == null)
                slider = GetComponent<Slider>();

            if (targetImage == null)
                targetImage = GetComponent<Image>();

            CacheShaderProperties();
            CreateRuntimeMaterial();
        }

        private void OnEnable()
        {
            if (slider == null)
                return;

            slider.onValueChanged.AddListener(OnSliderValueChanged);

            if (updateShaderOnEnable)
                UpdateSliderValue(slider.value);

            UpdateInteractionState();
        }

        private void OnDisable()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);

            StopFlash();

            isPointerOver = false;
            isPressed = false;

            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetFloat(hoverID, 0f);
                runtimeMaterial.SetFloat(pressedID, 0f);
                runtimeMaterial.SetFloat(flashID, 0f);
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }
        }

        private void CacheShaderProperties()
        {
            sliderValueID = Shader.PropertyToID(sliderValueProperty);
            hoverID = Shader.PropertyToID(hoverProperty);
            pressedID = Shader.PropertyToID(pressedProperty);
            flashID = Shader.PropertyToID(flashProperty);
        }

        private void CreateRuntimeMaterial()
        {
            if (targetImage == null)
            {
                Debug.LogWarning(
                    $"{nameof(PS_AdvancedSliderMaterialController)}: " +
                    "No target Image assigned.",
                    this);

                return;
            }

            if (targetImage.material == null)
            {
                Debug.LogWarning(
                    $"{nameof(PS_AdvancedSliderMaterialController)}: " +
                    "Target Image has no material assigned.",
                    this);

                return;
            }

            runtimeMaterial = Instantiate(targetImage.material);
            runtimeMaterial.name = targetImage.material.name + " (Runtime)";

            targetImage.material = runtimeMaterial;
        }

        private void OnSliderValueChanged(float value)
        {
            UpdateSliderValue(value);

            if (flashOnValueChanged)
                TriggerFlash();
        }

        private void UpdateSliderValue(float value)
        {
            if (runtimeMaterial == null)
                return;

            float normalizedValue = NormalizeSliderValue(value);

            runtimeMaterial.SetFloat(
                sliderValueID,
                normalizedValue);
        }

        private float NormalizeSliderValue(float value)
        {
            if (slider == null)
                return Mathf.Clamp01(value);

            float min = slider.minValue;
            float max = slider.maxValue;

            if (Mathf.Approximately(min, max))
                return 0f;

            return Mathf.Clamp01(
                Mathf.InverseLerp(min, max, value));
        }

        // ---------------------------------------------------------
        // HOVER
        // ---------------------------------------------------------

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;

            if (runtimeMaterial != null)
                runtimeMaterial.SetFloat(hoverID, 1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;

            if (!isPressed && runtimeMaterial != null)
                runtimeMaterial.SetFloat(hoverID, 0f);
        }

        // ---------------------------------------------------------
        // PRESS
        // ---------------------------------------------------------

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;

            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetFloat(pressedID, 1f);
                runtimeMaterial.SetFloat(hoverID, 1f);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;

            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetFloat(
                    pressedID,
                    0f);

                runtimeMaterial.SetFloat(
                    hoverID,
                    isPointerOver ? 1f : 0f);
            }
        }

        // ---------------------------------------------------------
        // FLASH
        // ---------------------------------------------------------

        public void TriggerFlash()
        {
            if (runtimeMaterial == null)
                return;

            StopFlash();

            flashCoroutine = StartCoroutine(
                FlashRoutine());
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            runtimeMaterial.SetFloat(
                flashID,
                flashIntensity);

            if (flashDuration <= 0f)
            {
                runtimeMaterial.SetFloat(
                    flashID,
                    0f);

                flashCoroutine = null;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float normalizedTime =
                    Mathf.Clamp01(elapsed / flashDuration);

                float intensity =
                    Mathf.Lerp(
                        flashIntensity,
                        0f,
                        normalizedTime);

                runtimeMaterial.SetFloat(
                    flashID,
                    intensity);

                yield return null;
            }

            runtimeMaterial.SetFloat(
                flashID,
                0f);

            flashCoroutine = null;
        }

        private void StopFlash()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            if (runtimeMaterial != null)
                runtimeMaterial.SetFloat(flashID, 0f);
        }

        // ---------------------------------------------------------
        // PUBLIC API
        // ---------------------------------------------------------

        public void SetValue(float value)
        {
            if (slider == null)
                return;

            slider.value = value;
        }

        public float GetValue()
        {
            if (slider == null)
                return 0f;

            return slider.value;
        }

        public float GetNormalizedValue()
        {
            if (slider == null)
                return 0f;

            return NormalizeSliderValue(
                slider.value);
        }

        public Material GetRuntimeMaterial()
        {
            return runtimeMaterial;
        }

        public Slider GetSlider()
        {
            return slider;
        }

        public Image GetTargetImage()
        {
            return targetImage;
        }

        // ---------------------------------------------------------
        // EDITOR / DEBUG
        // ---------------------------------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (flashDuration < 0f)
                flashDuration = 0f;

            if (flashIntensity < 0f)
                flashIntensity = 0f;

            if (slider == null)
                slider = GetComponent<Slider>();
        }
#endif

        private void UpdateInteractionState()
        {
            if (runtimeMaterial == null)
                return;

            runtimeMaterial.SetFloat(
                hoverID,
                isPointerOver ? 1f : 0f);

            runtimeMaterial.SetFloat(
                pressedID,
                isPressed ? 1f : 0f);
        }
    }
}
