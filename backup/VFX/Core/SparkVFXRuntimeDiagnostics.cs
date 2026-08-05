using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.VFX
{
    /// <summary>
    /// Runtime diagnostics monitor for Project Spark UI VFX.
    ///
    /// This component continuously monitors the VFX runtime chain:
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
    ///          +-------------------------+
    ///          |                         |
    ///          v                         v
    /// SparkVFXController       SparkTMPVFXController
    ///          |                         |
    ///          +------------+------------+
    ///                       |
    ///                       v
    ///                Runtime Material
    ///                       |
    ///                       v
    ///                     Shader
    ///
    /// IMPORTANT:
    /// - Does not change VFX state.
    /// - Does not call RequestState().
    /// - Does not call SetState().
    /// - Does not modify shader values.
    /// - Does not initialize controllers.
    /// - Only observes the system.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVFXRuntimeDiagnostics
        : MonoBehaviour
    {
        // ============================================================
        // MONITORING
        // ============================================================

        [Header("Monitoring")]

        [Tooltip(
            "Enable continuous runtime diagnostics."
        )]
        [SerializeField]
        private bool monitoringEnabled = true;


        [Tooltip(
            "How often diagnostics are checked."
        )]
        [Min(0.05f)]
        [SerializeField]
        private float checkInterval = 0.5f;


        [Tooltip(
            "Print a successful diagnostic message."
        )]
        [SerializeField]
        private bool logHealthyState = false;


        [Tooltip(
            "Log warnings only when the problem changes."
        )]
        [SerializeField]
        private bool logOnlyOnChange = true;


        // ============================================================
        // HIERARCHY SEARCH
        // ============================================================

        [Header("Hierarchy Search")]

        [SerializeField]
        private bool searchParent = true;


        [SerializeField]
        private bool searchChildren = true;


        [SerializeField]
        private bool includeInactiveChildren = true;


        // ============================================================
        // SHADER PROPERTIES
        // ============================================================

        [Header("Shader Diagnostics")]

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
        // RUNTIME REFERENCES
        // ============================================================

        private SparkVFXTarget target;

        private SparkVFXController imageController;

        private SparkTMPVFXController tmpController;

        private SparkVFXLayeredStateMachine stateMachine;

        private SparkVFXSequencePlayer sequencePlayer;

        private SparkVFXLoop loop;


        // ============================================================
        // RUNTIME STATE
        // ============================================================

        private float nextCheckTime;

        private bool initialized;

        private bool lastHealthyState;

        private string lastDiagnosticMessage;


        // ============================================================
        // LIFECYCLE
        // ============================================================

        private void Awake()
        {
            ResolveComponents();


            initialized =
                true;
        }


        private void OnEnable()
        {
            nextCheckTime =
                0f;
        }


        private void Update()
        {
            if (!monitoringEnabled)
            {
                return;
            }


            if (
                Time.unscaledTime <
                nextCheckTime
            )
            {
                return;
            }


            nextCheckTime =
                Time.unscaledTime +
                Mathf.Max(
                    0.05f,
                    checkInterval
                );


            RunDiagnostics();
        }


        // ============================================================
        // RESOLVE
        // ============================================================

        private void ResolveComponents()
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
        }


        // ============================================================
        // GENERIC SEARCH
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


            if (searchParent)
            {
                component =
                    GetComponentInParent<T>(
                        true
                    );


                if (component != null)
                {
                    return component;
                }
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
        // MAIN DIAGNOSTICS
        // ============================================================

        [ContextMenu(
            "Spark VFX / Run Diagnostics"
        )]
        public void RunDiagnostics()
        {
            if (!initialized)
            {
                ResolveComponents();

                initialized =
                    true;
            }


            List<string> errors =
                new List<string>();


            List<string> warnings =
                new List<string>();


            List<string> healthy =
                new List<string>();


            // --------------------------------------------------------
            // TARGET
            // --------------------------------------------------------

            if (target == null)
            {
                errors.Add(
                    "SparkVFXTarget is missing."
                );
            }
            else
            {
                healthy.Add(
                    "SparkVFXTarget found."
                );
            }


            // --------------------------------------------------------
            // CONTROLLERS
            // --------------------------------------------------------

            ISparkVFXController resolvedController =
                null;


            if (target != null)
            {
                resolvedController =
                    target.Controller;
            }


            if (resolvedController == null)
            {
                errors.Add(
                    "No ISparkVFXController is currently resolved."
                );
            }
            else
            {
                healthy.Add(
                    "ISparkVFXController resolved."
                );
            }


            // --------------------------------------------------------
            // IMAGE CONTROLLER
            // --------------------------------------------------------

            if (imageController != null)
            {
                DiagnoseImageController(
                    errors,
                    warnings,
                    healthy
                );
            }


            // --------------------------------------------------------
            // TMP CONTROLLER
            // --------------------------------------------------------

            if (tmpController != null)
            {
                DiagnoseTMPController(
                    errors,
                    warnings,
                    healthy
                );
            }


            // --------------------------------------------------------
            // NO CONCRETE CONTROLLER
            // --------------------------------------------------------

            if (
                imageController == null &&
                tmpController == null
            )
            {
                errors.Add(
                    "Neither SparkVFXController nor " +
                    "SparkTMPVFXController was found."
                );
            }


            // --------------------------------------------------------
            // STATE MACHINE
            // --------------------------------------------------------

            if (stateMachine == null)
            {
                warnings.Add(
                    "SparkVFXLayeredStateMachine is missing."
                );
            }
            else
            {
                healthy.Add(
                    "SparkVFXLayeredStateMachine is present."
                );
            }


            // --------------------------------------------------------
            // SEQUENCE PLAYER
            // --------------------------------------------------------

            if (sequencePlayer == null)
            {
                warnings.Add(
                    "SparkVFXSequencePlayer is missing."
                );
            }
            else
            {
                healthy.Add(
                    "SparkVFXSequencePlayer is present."
                );
            }


            // --------------------------------------------------------
            // LOOP
            // --------------------------------------------------------

            if (loop == null)
            {
                warnings.Add(
                    "SparkVFXLoop is missing."
                );
            }
            else
            {
                healthy.Add(
                    "SparkVFXLoop is present."
                );
            }


            // --------------------------------------------------------
            // RESULT
            // --------------------------------------------------------

            bool healthyState =
                errors.Count == 0;


            string diagnosticMessage =
                BuildDiagnosticMessage(
                    errors,
                    warnings,
                    healthy
                );


            ProcessDiagnosticResult(
                healthyState,
                diagnosticMessage
            );
        }


        // ============================================================
        // IMAGE CONTROLLER DIAGNOSTICS
        // ============================================================

        private void DiagnoseImageController(
            List<string> errors,
            List<string> warnings,
            List<string> healthy)
        {
            if (imageController == null)
            {
                return;
            }


            healthy.Add(
                "SparkVFXController found."
            );


            Material material =
                imageController.RuntimeMaterial;


            if (material == null)
            {
                errors.Add(
                    "SparkVFXController runtime material is null."
                );


                return;
            }


            healthy.Add(
                "SparkVFXController runtime material is valid."
            );


            DiagnoseMaterial(
                material,
                "SparkVFXController",
                errors,
                warnings,
                healthy
            );
        }


        // ============================================================
        // TMP CONTROLLER DIAGNOSTICS
        // ============================================================

        private void DiagnoseTMPController(
            List<string> errors,
            List<string> warnings,
            List<string> healthy)
        {
            if (tmpController == null)
            {
                return;
            }


            healthy.Add(
                "SparkTMPVFXController found."
            );


            Material material =
                tmpController.RuntimeMaterial;


            if (material == null)
            {
                errors.Add(
                    "SparkTMPVFXController runtime material is null."
                );


                return;
            }


            healthy.Add(
                "SparkTMPVFXController runtime material is valid."
            );


            DiagnoseMaterial(
                material,
                "SparkTMPVFXController",
                errors,
                warnings,
                healthy
            );
        }


        // ============================================================
        // MATERIAL DIAGNOSTICS
        // ============================================================

        private void DiagnoseMaterial(
            Material material,
            string controllerName,
            List<string> errors,
            List<string> warnings,
            List<string> healthy)
        {
            if (material == null)
            {
                errors.Add(
                    controllerName +
                    " material reference is null."
                );


                return;
            }


            Shader shader =
                material.shader;


            if (shader == null)
            {
                errors.Add(
                    controllerName +
                    " material has no shader."
                );


                return;
            }


            healthy.Add(
                controllerName +
                " shader is valid: " +
                shader.name
            );


            DiagnoseShaderProperties(
                material,
                controllerName,
                warnings
            );
        }


        // ============================================================
        // SHADER PROPERTIES
        // ============================================================

        private void DiagnoseShaderProperties(
            Material material,
            string controllerName,
            List<string> warnings)
        {
            if (
                material == null ||
                material.shader == null
            )
            {
                return;
            }


            if (
                requiredFloatProperties != null
            )
            {
                for (
                    int i = 0;
                    i < requiredFloatProperties.Length;
                    i++
                )
                {
                    string property =
                        requiredFloatProperties[i];


                    if (
                        string.IsNullOrWhiteSpace(
                            property
                        )
                    )
                    {
                        continue;
                    }


                    if (
                        !material.HasProperty(
                            property
                        )
                    )
                    {
                        warnings.Add(
                            controllerName +
                            " shader is missing property: " +
                            property
                        );
                    }
                }
            }


            if (
                requiredColorProperties != null
            )
            {
                for (
                    int i = 0;
                    i < requiredColorProperties.Length;
                    i++
                )
                {
                    string property =
                        requiredColorProperties[i];


                    if (
                        string.IsNullOrWhiteSpace(
                            property
                        )
                    )
                    {
                        continue;
                    }


                    if (
                        !material.HasProperty(
                            property
                        )
                    )
                    {
                        warnings.Add(
                            controllerName +
                            " shader is missing color property: " +
                            property
                        );
                    }
                }
            }
        }


        // ============================================================
        // BUILD MESSAGE
        // ============================================================

        private string BuildDiagnosticMessage(
            List<string> errors,
            List<string> warnings,
            List<string> healthy)
        {
            System.Text.StringBuilder builder =
                new System.Text.StringBuilder();


            builder.Append(
                "[SparkVFXRuntimeDiagnostics]"
            );


            builder.Append(
                "\n----------------------------------------"
            );


            if (healthy.Count > 0)
            {
                builder.Append(
                    "\nHEALTHY:"
                );


                for (
                    int i = 0;
                    i < healthy.Count;
                    i++
                )
                {
                    builder.Append(
                        "\n  + " +
                        healthy[i]
                    );
                }
            }


            if (warnings.Count > 0)
            {
                builder.Append(
                    "\nWARNINGS:"
                );


                for (
                    int i = 0;
                    i < warnings.Count;
                    i++
                )
                {
                    builder.Append(
                        "\n  ! " +
                        warnings[i]
                    );
                }
            }


            if (errors.Count > 0)
            {
                builder.Append(
                    "\nERRORS:"
                );


                for (
                    int i = 0;
                    i < errors.Count;
                    i++
                )
                {
                    builder.Append(
                        "\n  X " +
                        errors[i]
                    );
                }
            }


            builder.Append(
                "\n----------------------------------------"
            );


            return builder.ToString();
        }


        // ============================================================
        // PROCESS RESULT
        // ============================================================

        private void ProcessDiagnosticResult(
            bool healthy,
            string message)
        {
            if (
                logOnlyOnChange &&
                initialized &&
                healthy == lastHealthyState &&
                message == lastDiagnosticMessage
            )
            {
                return;
            }


            lastHealthyState =
                healthy;


            lastDiagnosticMessage =
                message;


            if (healthy)
            {
                if (logHealthyState)
                {
                    Debug.Log(
                        message,
                        this
                    );
                }
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
        // FORCE REFRESH
        // ============================================================

        [ContextMenu(
            "Spark VFX / Refresh References"
        )]
        public void RefreshReferences()
        {
            initialized =
                false;


            target =
                null;


            imageController =
                null;


            tmpController =
                null;


            stateMachine =
                null;


            sequencePlayer =
                null;


            loop =
                null;


            ResolveComponents();


            initialized =
                true;


            RunDiagnostics();
        }


        // ============================================================
        // ENABLE / DISABLE MONITORING
        // ============================================================

        public void SetMonitoring(
            bool enabled)
        {
            monitoringEnabled =
                enabled;
        }


        // ============================================================
        // PUBLIC STATUS
        // ============================================================

        public bool IsMonitoring
        {
            get
            {
                return monitoringEnabled;
            }
        }


        public bool LastHealthyState
        {
            get
            {
                return lastHealthyState;
            }
        }


        public string LastDiagnosticMessage
        {
            get
            {
                return lastDiagnosticMessage;
            }
        }


        // ============================================================
        // VALIDATION
        // ============================================================

        private void OnValidate()
        {
            checkInterval =
                Mathf.Max(
                    0.05f,
                    checkInterval
                );
        }
    }
}