using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Runtime validation utility for the Project Spark UI VFX system.
    ///
    /// Validates the complete VFX chain:
    ///
    /// SparkVFXLayeredStateMachine
    ///          |
    ///          v
    /// SparkVFXSequencePlayer / SparkVFXLoop
    ///          |
    ///          v
    /// SparkVFXTarget
    ///          |
    ///          v
    /// ISparkVFXController
    ///          |
    ///          +-----------------------+
    ///          |                       |
    ///          v                       v
    /// SparkVFXController       SparkTMPVFXController
    ///          |                       |
    ///          v                       v
    ///      Graphic                 TMP_Text
    ///          |                       |
    ///          +-----------+-----------+
    ///                      |
    ///                      v
    ///                Runtime Material
    ///                      |
    ///                      v
    ///                    Shader
    ///
    /// This component does not modify VFX state.
    /// It only inspects and reports configuration problems.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXRuntimeValidator
        : MonoBehaviour
    {
        // ============================================================
        // VALIDATION OPTIONS
        // ============================================================

        [Header("Validation")]

        [Tooltip(
            "Validate automatically when the object is enabled."
        )]
        [SerializeField]
        private bool validateOnEnable = true;


        [Tooltip(
            "Print successful validation information."
        )]
        [SerializeField]
        private bool logSuccess = true;


        [Tooltip(
            "Print warnings for optional components."
        )]
        [SerializeField]
        private bool logWarnings = true;


        [Tooltip(
            "Search the complete hierarchy for VFX components."
        )]
        [SerializeField]
        private bool searchChildren = true;


        [Tooltip(
            "Include inactive child objects when searching."
        )]
        [SerializeField]
        private bool includeInactiveChildren = true;


        // ============================================================
        // EXPECTED SHADER PROPERTIES
        // ============================================================

        [Header("Required Shader Properties")]

        [SerializeField]
        private string[] requiredFloatProperties =
        {
            "_Glow",
            "_Scan",
            "_Sweep",
            "_Flash",
            "_Glitch",
            "_Flicker",
            "_Dissolve",
            "_Reveal",
            "_SweepPosition"
        };


        [SerializeField]
        private string[] requiredColorProperties =
        {
            "_VFXColor",
            "_GlowColor",
            "_ScanColor",
            "_SweepColor"
        };


        // ============================================================
        // RUNTIME RESULTS
        // ============================================================

        private int errorCount;

        private int warningCount;

        private int successCount;

        private bool lastValidationPassed;


        // ============================================================
        // CACHED COMPONENTS
        // ============================================================

        private SparkVFXTarget target;

        private SparkVFXController imageController;

        private SparkTMPVFXController tmpController;

        private SparkVFXLayeredStateMachine stateMachine;

        private SparkVFXSequencePlayer sequencePlayer;

        private SparkVFXLoop loop;

        private SparkVFXUIEvents uiEvents;

        private SparkVFXEditorDebugger editorDebugger;


        // ============================================================
        // UNITY LIFECYCLE
        // ============================================================

        private void Awake()
        {
            CacheComponents();
        }


        private void OnEnable()
        {
            if (validateOnEnable)
            {
                Validate();
            }
        }


        // ============================================================
        // CACHE
        // ============================================================

        private void CacheComponents()
        {
            target =
                FindComponent<
                    SparkVFXTarget
                >();


            imageController =
                FindComponent<
                    SparkVFXController
                >();


            tmpController =
                FindComponent<
                    SparkTMPVFXController
                >();


            stateMachine =
                FindComponent<
                    SparkVFXLayeredStateMachine
                >();


            sequencePlayer =
                FindComponent<
                    SparkVFXSequencePlayer
                >();


            loop =
                FindComponent<
                    SparkVFXLoop
                >();


            uiEvents =
                FindComponent<
                    SparkVFXUIEvents
                >();


            editorDebugger =
                FindComponent<
                    SparkVFXEditorDebugger
                >();
        }


        // ============================================================
        // GENERIC COMPONENT SEARCH
        // ============================================================

        private T FindComponent<T>()
            where T : Component
        {
            T component =
                GetComponent<T>();


            if (component != null)
            {
                return component;
            }


            component =
                GetComponentInParent<T>(
                    true
                );


            if (component != null)
            {
                return component;
            }


            if (searchChildren)
            {
                component =
                    GetComponentInChildren<T>(
                        includeInactiveChildren
                    );
            }


            return component;
        }


        // ============================================================
        // MAIN VALIDATION
        // ============================================================

        [ContextMenu(
            "Spark VFX / Validate Runtime"
        )]
        public bool Validate()
        {
            ResetResults();


            CacheComponents();


            LogHeader();


            ValidateTarget();


            ValidateControllers();


            ValidateStateMachine();


            ValidateSequencePlayer();


            ValidateLoop();


            ValidateUIEvents();


            ValidateEditorDebugger();


            lastValidationPassed =
                errorCount == 0;


            LogSummary();


            return lastValidationPassed;
        }


        // ============================================================
        // RESET
        // ============================================================

        private void ResetResults()
        {
            errorCount =
                0;


            warningCount =
                0;


            successCount =
                0;
        }


        // ============================================================
        // TARGET VALIDATION
        // ============================================================

        private void ValidateTarget()
        {
            if (target == null)
            {
                AddError(
                    "SparkVFXTarget was not found."
                );


                return;
            }


            AddSuccess(
                "SparkVFXTarget found."
            );


            ISparkVFXController controller =
                target.Controller;


            if (controller == null)
            {
                AddError(
                    "SparkVFXTarget exists, " +
                    "but no ISparkVFXController could be resolved."
                );


                return;
            }


            AddSuccess(
                "SparkVFXTarget resolved an " +
                "ISparkVFXController."
            );
        }


        // ============================================================
        // CONTROLLER VALIDATION
        // ============================================================

        private void ValidateControllers()
        {
            bool hasImage =
                imageController != null;


            bool hasTMP =
                tmpController != null;


            if (!hasImage && !hasTMP)
            {
                AddError(
                    "No SparkVFXController or " +
                    "SparkTMPVFXController found."
                );


                return;
            }


            if (hasImage && hasTMP)
            {
                AddWarning(
                    "Both SparkVFXController and " +
                    "SparkTMPVFXController exist in the " +
                    "resolved hierarchy. Make sure SparkVFXTarget " +
                    "is resolving the intended controller."
                );
            }


            if (hasImage)
            {
                ValidateImageController(
                    imageController
                );
            }


            if (hasTMP)
            {
                ValidateTMPController(
                    tmpController
                );
            }
        }


        // ============================================================
        // IMAGE CONTROLLER
        // ============================================================

        private void ValidateImageController(
            SparkVFXController controller)
        {
            if (controller == null)
            {
                return;
            }


            AddSuccess(
                "SparkVFXController found."
            );


            Graphic graphic =
                controller.TargetGraphic;


            if (graphic == null)
            {
                AddError(
                    "SparkVFXController has no target Graphic."
                );


                return;
            }


            AddSuccess(
                "SparkVFXController target Graphic found: " +
                graphic.GetType().Name
            );


            Material material =
                controller.RuntimeMaterial;


            if (material == null)
            {
                AddError(
                    "SparkVFXController has no runtime material. " +
                    "Check the source material and Graphic material."
                );


                return;
            }


            ValidateMaterial(
                material,
                "SparkVFXController"
            );
        }


        // ============================================================
        // TMP CONTROLLER
        // ============================================================

        private void ValidateTMPController(
            SparkTMPVFXController controller)
        {
            if (controller == null)
            {
                return;
            }


            AddSuccess(
                "SparkTMPVFXController found."
            );


            Material material =
                controller.RuntimeMaterial;


            if (material == null)
            {
                AddError(
                    "SparkTMPVFXController has no runtime material."
                );


                return;
            }


            ValidateMaterial(
                material,
                "SparkTMPVFXController"
            );
        }


        // ============================================================
        // MATERIAL
        // ============================================================

        private void ValidateMaterial(
            Material material,
            string controllerName)
        {
            if (material == null)
            {
                AddError(
                    controllerName +
                    " runtime material is null."
                );


                return;
            }


            AddSuccess(
                controllerName +
                " runtime material found: " +
                material.name
            );


            Shader shader =
                material.shader;


            if (shader == null)
            {
                AddError(
                    controllerName +
                    " material has no shader."
                );


                return;
            }


            AddSuccess(
                controllerName +
                " shader found: " +
                shader.name
            );


            ValidateShaderProperties(
                material,
                controllerName
            );
        }


        // ============================================================
        // SHADER PROPERTIES
        // ============================================================

        private void ValidateShaderProperties(
            Material material,
            string controllerName)
        {
            if (material == null)
            {
                return;
            }


            if (requiredFloatProperties != null)
            {
                for (
                    int i = 0;
                    i < requiredFloatProperties.Length;
                    i++
                )
                {
                    string propertyName =
                        requiredFloatProperties[i];


                    if (string.IsNullOrWhiteSpace(
                        propertyName
                    ))
                    {
                        continue;
                    }


                    if (!material.HasProperty(
                        propertyName
                    ))
                    {
                        AddWarning(
                            controllerName +
                            " shader is missing float property: " +
                            propertyName
                        );
                    }
                }
            }


            if (requiredColorProperties != null)
            {
                for (
                    int i = 0;
                    i < requiredColorProperties.Length;
                    i++
                )
                {
                    string propertyName =
                        requiredColorProperties[i];


                    if (string.IsNullOrWhiteSpace(
                        propertyName
                    ))
                    {
                        continue;
                    }


                    if (!material.HasProperty(
                        propertyName
                    ))
                    {
                        AddWarning(
                            controllerName +
                            " shader is missing color property: " +
                            propertyName
                        );
                    }
                }
            }
        }


        // ============================================================
        // STATE MACHINE
        // ============================================================

        private void ValidateStateMachine()
        {
            if (stateMachine == null)
            {
                AddWarning(
                    "SparkVFXLayeredStateMachine was not found."
                );


                return;
            }


            AddSuccess(
                "SparkVFXLayeredStateMachine found."
            );
        }


        // ============================================================
        // SEQUENCE PLAYER
        // ============================================================

        private void ValidateSequencePlayer()
        {
            if (sequencePlayer == null)
            {
                AddWarning(
                    "SparkVFXSequencePlayer was not found."
                );


                return;
            }


            AddSuccess(
                "SparkVFXSequencePlayer found."
            );
        }


        // ============================================================
        // LOOP
        // ============================================================

        private void ValidateLoop()
        {
            if (loop == null)
            {
                AddWarning(
                    "SparkVFXLoop was not found."
                );


                return;
            }


            AddSuccess(
                "SparkVFXLoop found."
            );
        }


        // ============================================================
        // UI EVENTS
        // ============================================================

        private void ValidateUIEvents()
        {
            if (uiEvents == null)
            {
                AddWarning(
                    "SparkVFXUIEvents was not found. " +
                    "Automatic UI pointer state switching is unavailable."
                );


                return;
            }


            AddSuccess(
                "SparkVFXUIEvents found."
            );


            if (uiEvents.stateMachine == null)
            {
                AddWarning(
                    "SparkVFXUIEvents exists but has no resolved " +
                    "SparkVFXLayeredStateMachine."
                );


                return;
            }


            AddSuccess(
                "SparkVFXUIEvents has a valid state machine."
            );
        }


        // ============================================================
        // EDITOR DEBUGGER
        // ============================================================

        private void ValidateEditorDebugger()
        {
            if (editorDebugger == null)
            {
                AddWarning(
                    "SparkVFXEditorDebugger was not found. " +
                    "Manual runtime debugging is unavailable."
                );


                return;
            }


            AddSuccess(
                "SparkVFXEditorDebugger found."
            );


            if (!editorDebugger.IsReady)
            {
                AddWarning(
                    "SparkVFXEditorDebugger has not resolved " +
                    "a SparkVFXLayeredStateMachine."
                );


                return;
            }


            AddSuccess(
                "SparkVFXEditorDebugger is ready."
            );
        }


        // ============================================================
        // HEADER
        // ============================================================

        private void LogHeader()
        {
            Debug.Log(
                "==================================================\n" +
                "[SparkVFXRuntimeValidator]\n" +
                "Starting Project Spark UI VFX validation...\n" +
                "==================================================",
                this
            );
        }


        // ============================================================
        // SUCCESS
        // ============================================================

        private void AddSuccess(
            string message)
        {
            successCount++;


            if (!logSuccess)
            {
                return;
            }


            Debug.Log(
                "[SparkVFXRuntimeValidator] OK: " +
                message,
                this
            );
        }


        // ============================================================
        // WARNING
        // ============================================================

        private void AddWarning(
            string message)
        {
            warningCount++;


            if (!logWarnings)
            {
                return;
            }


            Debug.LogWarning(
                "[SparkVFXRuntimeValidator] WARNING: " +
                message,
                this
            );
        }


        // ============================================================
        // ERROR
        // ============================================================

        private void AddError(
            string message)
        {
            errorCount++;


            Debug.LogError(
                "[SparkVFXRuntimeValidator] ERROR: " +
                message,
                this
            );
        }


        // ============================================================
        // SUMMARY
        // ============================================================

        private void LogSummary()
        {
            string result;


            if (lastValidationPassed)
            {
                result =
                    "VALIDATION PASSED";
            }
            else
            {
                result =
                    "VALIDATION FAILED";
            }


            string message =
                "\n==================================================" +
                "\n[SparkVFXRuntimeValidator]" +
                "\n" +
                result +
                "\n" +
                "Successes: " +
                successCount +
                "\n" +
                "Warnings: " +
                warningCount +
                "\n" +
                "Errors: " +
                errorCount +
                "\n" +
                "==================================================";


            if (lastValidationPassed)
            {
                Debug.Log(
                    message,
                    this
                );
            }
            else
            {
                Debug.LogError(
                    message,
                    this
                );
            }
        }


        // ============================================================
        // PUBLIC API
        // ============================================================

        public bool LastValidationPassed
        {
            get
            {
                return lastValidationPassed;
            }
        }


        public int ErrorCount
        {
            get
            {
                return errorCount;
            }
        }


        public int WarningCount
        {
            get
            {
                return warningCount;
            }
        }


        public int SuccessCount
        {
            get
            {
                return successCount;
            }
        }


        // ============================================================
        // MANUAL REFRESH
        // ============================================================

        [ContextMenu(
            "Spark VFX / Revalidate"
        )]
        private void Revalidate()
        {
            Validate();
        }
    }
}