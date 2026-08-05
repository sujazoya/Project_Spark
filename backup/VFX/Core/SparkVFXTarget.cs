using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Central resolver for Project Spark UI VFX controllers.
    ///
    /// Supports:
    /// - SparkVFXController
    /// - SparkTMPVFXController
    ///
    /// Resolution priority:
    /// 1. Explicitly assigned Image controller
    /// 2. Explicitly assigned TMP controller
    /// 3. Image controller on this GameObject
    /// 4. TMP controller on this GameObject
    /// 5. Image controller in children
    /// 6. TMP controller in children
    ///
    /// Runtime systems should use:
    ///
    ///     Target.Controller
    ///
    /// and work through ISparkVFXController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXTarget : MonoBehaviour
    {
        // ============================================================
        // CONTROLLERS
        // ============================================================

        [Header("VFX Controllers")]

        [Tooltip(
            "Optional Image / UI Graphic VFX controller."
        )]
        [SerializeField]
        private SparkVFXController imageController;


        [Tooltip(
            "Optional TextMeshPro VFX controller."
        )]
        [SerializeField]
        private SparkTMPVFXController tmpController;


        // ============================================================
        // RUNTIME
        // ============================================================

        private ISparkVFXController resolvedController;

        private bool initialized;


        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            Resolve();
        }


        private void OnEnable()
        {
            if (!initialized)
            {
                Resolve();
            }
        }


        // ============================================================
        // RESOLVE
        // ============================================================

       
// ============================================================
// RESOLVE
// ============================================================

public ISparkVFXController Resolve()
        {
            // --------------------------------------------------------
            // ALREADY RESOLVED
            // --------------------------------------------------------

            if (
                initialized &&
                resolvedController != null
            )
            {
                return resolvedController;
            }


            // --------------------------------------------------------
            // RESET
            // --------------------------------------------------------

            resolvedController =
                null;

            initialized =
                false;


            // ========================================================
            // 1. EXPLICIT IMAGE CONTROLLER
            // ========================================================

            if (imageController != null)
            {
                resolvedController =
                    imageController;

                resolvedController.Initialize();

                initialized =
                    resolvedController != null;

                return resolvedController;
            }


            // ========================================================
            // 2. EXPLICIT TMP CONTROLLER
            // ========================================================

            if (tmpController != null)
            {
                resolvedController =
                    tmpController;

                resolvedController.Initialize();

                initialized =
                    resolvedController != null;

                return resolvedController;
            }


            // ========================================================
            // 3. IMAGE CONTROLLER ON THIS OBJECT
            // ========================================================

            imageController =
                GetComponent<SparkVFXController>();


            if (imageController != null)
            {
                resolvedController =
                    imageController;

                resolvedController.Initialize();

                initialized =
                    resolvedController != null;

                return resolvedController;
            }


            // ========================================================
            // 4. TMP CONTROLLER ON THIS OBJECT
            // ========================================================

            tmpController =
                GetComponent<SparkTMPVFXController>();


            if (tmpController != null)
            {
                resolvedController =
                    tmpController;

                resolvedController.Initialize();

                initialized =
                    resolvedController != null;

                return resolvedController;
            }


            // ========================================================
            // 5. IMAGE CONTROLLER IN CHILDREN
            // ========================================================

            imageController =
                GetComponentInChildren<
                    SparkVFXController>(
                        true
                    );


            if (imageController != null)
            {
                resolvedController =
                    imageController;

                resolvedController.Initialize();

                initialized =
                    resolvedController != null;

                return resolvedController;
            }


            // ========================================================
            // 6. TMP CONTROLLER IN CHILDREN
            // ========================================================

            tmpController =
                GetComponentInChildren<
                    SparkTMPVFXController>(
                        true
                    );


            if (tmpController != null)
            {
                resolvedController =
                    tmpController;

                resolvedController.Initialize();

                initialized =
                    resolvedController != null;

                return resolvedController;
            }


            // ========================================================
            // FAILED
            // ========================================================

            Debug.LogError(
                "[SparkVFXTarget] " +
                "No compatible VFX controller found.\n" +
                "Add either SparkVFXController or " +
                "SparkTMPVFXController to this GameObject " +
                "or one of its children.",
                this
            );


            return null;
        }



        // ============================================================
        // CONTROLLER
        // ============================================================

        public ISparkVFXController Controller
        {
            get
            {
                if (
                    !initialized ||
                    resolvedController == null
                )
                {
                    Resolve();
                }

                return resolvedController;
            }
        }


        // ============================================================
        // IMAGE CONTROLLER
        // ============================================================

        public SparkVFXController ImageController
        {
            get
            {
                if (imageController == null)
                {
                    imageController =
                        GetComponentInChildren<
                            SparkVFXController>(
                                true
                            );
                }

                return imageController;
            }
        }


        // ============================================================
        // TMP CONTROLLER
        // ============================================================

        public SparkTMPVFXController TMPController
        {
            get
            {
                if (tmpController == null)
                {
                    tmpController =
                        GetComponentInChildren<
                            SparkTMPVFXController>(
                                true
                            );
                }

                return tmpController;
            }
        }


        // ============================================================
        // HAS CONTROLLER
        // ============================================================

        public bool HasController
        {
            get
            {
                return Controller != null;
            }
        }


        // ============================================================
        // REFRESH
        // ============================================================

        public void Refresh()
        {
            imageController =
                null;

            tmpController =
                null;

            resolvedController =
                null;

            initialized =
                false;

            Resolve();
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            if (
                imageController != null &&
                tmpController != null
            )
            {
                Debug.LogWarning(
                    "[SparkVFXTarget] Both Image and TMP " +
                    "controllers are assigned. " +
                    "The Image controller has priority.",
                    this
                );
            }
        }
    }
}