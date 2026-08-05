using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.VFX;

public class MonitorMaximizeController : MonoBehaviour
{
    [Header("Monitor")]
    [Tooltip("The Monitor Parent RectTransform that will be resized.")]
    public RectTransform monitor;

    [Header("Full Screen Canvas")]
    [Tooltip("Your MonitorScreen canvas RectTransform.")]
    public RectTransform screenCanvas;

    [Header("Monitor Size Slider")]
    [Tooltip("Slider that controls maximized monitor size.")]
    public Slider sizeSlider;

    [Header("Maximize Size")]
    [Range(50f, 95f)]
    [Tooltip("Size used when clicking Maximize.")]
    public float defaultSliderValue = 95f;

    [Header("Aspect Ratio")]
    public float aspectWidth = 16f;
    public float aspectHeight = 9f;

    [Header("Animation")]
    public bool smoothAnimation = true;

    [Min(0.1f)]
    public float animationSpeed = 8f;

    [SerializeField] SparkScanOverlay scanner;


    // =========================================================
    // ORIGINAL MONITOR STATE
    // =========================================================

    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;

    private Vector2 originalPosition;
    private Vector2 originalSize;

    private Vector3 originalScale;


    // =========================================================
    // TARGET MONITOR STATE
    // =========================================================

    private Vector2 targetPosition;
    private Vector2 targetSize;

    private Vector3 targetScale;


    // =========================================================
    // STATE
    // =========================================================

    private bool isMaximized = false;

    private float monitorSizePercent = 0.95f;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (monitor == null)
        {
            Debug.LogError(
                "MonitorMaximizeController: Monitor is not assigned."
            );

            return;
        }


        if (screenCanvas == null)
        {
            Debug.LogError(
                "MonitorMaximizeController: Screen Canvas is not assigned."
            );

            return;
        }


        // Save exact original monitor state
        SaveOriginalState();


        // Initial target = original state
        targetPosition =
            originalPosition;

        targetSize =
            originalSize;

        targetScale =
            originalScale;


        // Hide slider at startup
        if (sizeSlider != null)
        {
            sizeSlider.gameObject.SetActive(false);


            // Slider range
            sizeSlider.minValue = 50f;
            sizeSlider.maxValue = 95f;


            // Default value
            sizeSlider.SetValueWithoutNotify(
                defaultSliderValue
            );


            monitorSizePercent =
                defaultSliderValue / 100f;
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (monitor == null)
            return;


        if (!smoothAnimation)
            return;


        // -----------------------------------------------------
        // Smooth Position
        // -----------------------------------------------------

        monitor.anchoredPosition =
            Vector2.Lerp(
                monitor.anchoredPosition,
                targetPosition,
                Time.unscaledDeltaTime *
                animationSpeed
            );


        // -----------------------------------------------------
        // Smooth Size
        // -----------------------------------------------------

        monitor.sizeDelta =
            Vector2.Lerp(
                monitor.sizeDelta,
                targetSize,
                Time.unscaledDeltaTime *
                animationSpeed
            );


        // -----------------------------------------------------
        // Smooth Scale
        // -----------------------------------------------------

        monitor.localScale =
            Vector3.Lerp(
                monitor.localScale,
                targetScale,
                Time.unscaledDeltaTime *
                animationSpeed
            );
    }


    // =========================================================
    // SAVE ORIGINAL STATE
    // =========================================================

    private void SaveOriginalState()
    {
        originalAnchorMin =
            monitor.anchorMin;

        originalAnchorMax =
            monitor.anchorMax;

        originalPivot =
            monitor.pivot;


        originalPosition =
            monitor.anchoredPosition;

        originalSize =
            monitor.sizeDelta;

        originalScale =
            monitor.localScale;
    }


    // =========================================================
    // MAXIMIZE MONITOR
    // =========================================================

    public void MaximizeMonitor()
    {
        if (monitor == null ||
            screenCanvas == null)
        {
            return;
        }


        isMaximized = true;


        // -----------------------------------------------------
        // CENTER ANCHORS
        // IMPORTANT:
        // DO NOT USE STRETCH / STRETCH
        // This prevents the double-size problem.
        // -----------------------------------------------------

        monitor.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        monitor.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        monitor.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        // -----------------------------------------------------
        // SHOW SLIDER
        // -----------------------------------------------------

        if (sizeSlider != null)
        {
            sizeSlider.gameObject.SetActive(true);


            // Reset slider to default maximize size
            sizeSlider.SetValueWithoutNotify(
                defaultSliderValue
            );


            // Convert 95 -> 0.95
            monitorSizePercent =
                defaultSliderValue / 100f;
        }


        // -----------------------------------------------------
        // CALCULATE MAXIMIZED SIZE
        // -----------------------------------------------------

        UpdateMaximizedMonitorSize();
    }


    // =========================================================
    // UPDATE MAXIMIZED MONITOR SIZE
    // =========================================================

    private void UpdateMaximizedMonitorSize()
    {
        if (monitor == null ||
            screenCanvas == null)
        {
            return;
        }


        // -----------------------------------------------------
        // GET CANVAS SIZE
        // -----------------------------------------------------

        float canvasWidth =
            screenCanvas.rect.width;

        float canvasHeight =
            screenCanvas.rect.height;


        if (canvasWidth <= 0 ||
            canvasHeight <= 0)
        {
            return;
        }


        // -----------------------------------------------------
        // GET ASPECT RATIO
        // Example: 16 / 9
        // -----------------------------------------------------

        float aspect =
            aspectWidth /
            aspectHeight;


        // -----------------------------------------------------
        // APPLY SLIDER PERCENTAGE
        // -----------------------------------------------------

        float availableWidth =
            canvasWidth *
            monitorSizePercent;

        float availableHeight =
            canvasHeight *
            monitorSizePercent;


        float width;
        float height;


        // -----------------------------------------------------
        // FIT 16:9 INSIDE AVAILABLE AREA
        // -----------------------------------------------------

        if (
            availableWidth /
            availableHeight
            >
            aspect
        )
        {
            // Height is limiting factor

            height =
                availableHeight;

            width =
                height *
                aspect;
        }
        else
        {
            // Width is limiting factor

            width =
                availableWidth;

            height =
                width /
                aspect;
        }


        // -----------------------------------------------------
        // SET TARGET SIZE
        // -----------------------------------------------------

        targetSize =
            new Vector2(
                width,
                height
            );

       
        // -----------------------------------------------------
        // CENTER MONITOR
        // -----------------------------------------------------

        targetPosition =
            Vector2.zero;


        // -----------------------------------------------------
        // NORMAL SCALE
        // -----------------------------------------------------

        targetScale =
            Vector3.one;
    }


    // =========================================================
    // SLIDER CONTROL
    // =========================================================

   
    public void OnSizeSliderChanged(float value)
    {
        // Do nothing if monitor is minimized
        if (!isMaximized)
            return;


        // -----------------------------------------------------
        // Convert:
        //
        // Slider 50 -> 0.50
        // Slider 75 -> 0.75
        // Slider 95 -> 0.95
        // -----------------------------------------------------

        monitorSizePercent =
            value / 100f;


        // -----------------------------------------------------
        // Recalculate monitor size
        // -----------------------------------------------------

        UpdateMaximizedMonitorSize();
    }


    // =========================================================
    // MINIMIZE MONITOR
    // =========================================================

    public void MinimizeMonitor()
    {
        if (monitor == null)
            return;


        isMaximized = false;


        // -----------------------------------------------------
        // HIDE SLIDER
        // -----------------------------------------------------

        if (sizeSlider != null)
        {
            sizeSlider.gameObject.SetActive(false);
        }


        // -----------------------------------------------------
        // RESTORE ORIGINAL ANCHORS
        // -----------------------------------------------------

        monitor.anchorMin =
            originalAnchorMin;

        monitor.anchorMax =
            originalAnchorMax;

        monitor.pivot =
            originalPivot;


        // -----------------------------------------------------
        // RESTORE ORIGINAL TARGET
        // -----------------------------------------------------

        targetPosition =
            originalPosition;

        targetSize =
            originalSize;

        targetScale =
            originalScale;
    }


    // =========================================================
    // TOGGLE MAXIMIZE / MINIMIZE
    // =========================================================

    public void ToggleMonitor()
    {
        if (isMaximized)
        {
            MinimizeMonitor();
        }
        else
        {
            MaximizeMonitor();
        }
    }


    // =========================================================
    // INSTANT MAXIMIZE
    // =========================================================

    public void MaximizeInstant()
    {
        MaximizeMonitor();


        monitor.anchoredPosition =
            targetPosition;

        monitor.sizeDelta =
            targetSize;

        monitor.localScale =
            targetScale;
    }


    // =========================================================
    // INSTANT MINIMIZE
    // =========================================================

    public void MinimizeInstant()
    {
        if (monitor == null)
            return;


        isMaximized = false;


        // Hide slider
        if (sizeSlider != null)
        {
            sizeSlider.gameObject.SetActive(false);
        }


        // Restore anchors
        monitor.anchorMin =
            originalAnchorMin;

        monitor.anchorMax =
            originalAnchorMax;

        monitor.pivot =
            originalPivot;


        // Restore exact original state
        monitor.anchoredPosition =
            originalPosition;

        monitor.sizeDelta =
            originalSize;

        monitor.localScale =
            originalScale;


        // Reset targets
        targetPosition =
            originalPosition;

        targetSize =
            originalSize;

        targetScale =
            originalScale;
    }
}