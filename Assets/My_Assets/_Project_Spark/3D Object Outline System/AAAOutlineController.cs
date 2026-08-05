using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AAAOutline
{
    [DisallowMultipleComponent]
    public sealed class AAAOutlineController : MonoBehaviour
    {
        private const string OutlineShaderName = "AAA/Simple Outline";

        private static readonly int OutlineColorId =
            Shader.PropertyToID("_OutlineColor");

        private static readonly int OutlineWidthId =
            Shader.PropertyToID("_OutlineWidth");

        private static readonly int OutlineIntensityId =
            Shader.PropertyToID("_OutlineIntensity");

        private static readonly int FresnelPowerId =
            Shader.PropertyToID("_FresnelPower");

        private static readonly int FresnelStrengthId =
            Shader.PropertyToID("_FresnelStrength");

        private static readonly int PulseSpeedId =
            Shader.PropertyToID("_PulseSpeed");

        private static readonly int PulseMinId =
            Shader.PropertyToID("_PulseMin");

        private static readonly int PulseMaxId =
            Shader.PropertyToID("_PulseMax");

        private static readonly int ThroughWallAlphaId =
            Shader.PropertyToID("_ThroughWallAlpha");


        [Header("Outline")]

        [Tooltip("Enables or disables the complete outline system.")]
        [SerializeField]
        private bool outlineEnabled = true;


        [Header("Renderer Source")]

        [Tooltip("Automatically searches this GameObject and all children for MeshRenderer and SkinnedMeshRenderer components.")]
        [SerializeField]
        private OutlineRendererSource rendererSource =
            OutlineRendererSource.AutomaticChildren;

        [Tooltip("Used when Renderer Source is set to Manual.")]
        [SerializeField]
        private Renderer[] manualRenderers;


        [Header("State")]

        [SerializeField]
        private OutlineState currentState = OutlineState.Normal;

        [Tooltip("Default duration used by SetState().")]
        [Min(0f)]
        [SerializeField]
        private float transitionDuration = 0.15f;


        [Header("State Profiles")]

        [SerializeField]
        private AAAOutlineProfile normalProfile = new AAAOutlineProfile
        {
            Color = Color.white,
            Width = 0.02f,
            Intensity = 0f,
            VisibilityMode = OutlineVisibilityMode.Occluded
        };

        [SerializeField]
        private AAAOutlineProfile hoverProfile = new AAAOutlineProfile
        {
            Color = Color.cyan,
            Width = 0.015f,
            Intensity = 1f,
            VisibilityMode = OutlineVisibilityMode.Occluded
        };

        [SerializeField]
        private AAAOutlineProfile selectedProfile = new AAAOutlineProfile
        {
            Color = Color.blue,
            Width = 0.025f,
            Intensity = 1.5f,
            VisibilityMode = OutlineVisibilityMode.Occluded
        };

        [SerializeField]
        private AAAOutlineProfile targetStateProfile = new AAAOutlineProfile
        {
            Color = new Color(1f, 0.65f, 0.05f, 1f),
            Width = 0.03f,
            Intensity = 2f,
            PulseEnabled = true,
            PulseSpeed = 2f,
            PulseMin = 0.65f,
            PulseMax = 1.2f,
            VisibilityMode = OutlineVisibilityMode.Occluded
        };

        [SerializeField]
        private AAAOutlineProfile warningProfile = new AAAOutlineProfile
        {
            Color = Color.red,
            Width = 0.04f,
            Intensity = 3f,
            PulseEnabled = true,
            PulseSpeed = 4f,
            PulseMin = 0.5f,
            PulseMax = 1.4f,
            VisibilityMode = OutlineVisibilityMode.ThroughWalls
        };


        private readonly List<Renderer> sourceRenderers =
            new List<Renderer>();

        private readonly List<Renderer> outlineRenderers =
            new List<Renderer>();

        private readonly List<Material> outlineMaterials =
            new List<Material>();


        private GameObject outlineRoot;

        private Material outlineMaterial;

        private AAAOutlineProfile currentProfile;
        private AAAOutlineProfile targetProfile;

        private float transitionTimer;
        private float activeTransitionDuration;

        private bool isTransitioning;
        private bool isPulsing;

        private bool initialized;


        public OutlineState CurrentState
        {
            get { return currentState; }
        }


        public bool IsOutlineEnabled
        {
            get { return outlineEnabled; }
        }


        private void Awake()
        {
            Initialize();
        }


        private void OnEnable()
        {
            if (!initialized)
            {
                Initialize();
            }

            ApplyVisibility(outlineEnabled && currentState != OutlineState.Normal);
        }


        private void Update()
        {
            bool needsUpdate = false;

            if (isTransitioning)
            {
                UpdateTransition();
                needsUpdate = true;
            }

            if (isPulsing && !isTransitioning)
            {
                UpdatePulse();
                needsUpdate = true;
            }

            if (!needsUpdate)
            {
                enabled = false;
            }
        }
        private void LateUpdate()
        {
            for (int i = 0; i < sourceRenderers.Count; i++)
            {
                Renderer source =
                    sourceRenderers[i];

                if (source == null ||
                    i >= outlineRenderers.Count)
                {
                    continue;
                }

                Renderer outline =
                    outlineRenderers[i];

                if (outline == null)
                {
                    continue;
                }

                Transform sourceTransform =
                    source.transform;

                Transform outlineTransform =
                    outline.transform;

                outlineTransform.SetPositionAndRotation(
                    sourceTransform.position,
                    sourceTransform.rotation);

                outlineTransform.localScale =
                    sourceTransform.localScale;
            }
        }


        private void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            FindSourceRenderers();

            CreateOutlineMaterial();

            CreateOutlineRenderers();

            currentProfile = GetProfile(currentState).Clone();
            targetProfile = currentProfile.Clone();

            currentProfile.Sanitize();
            targetProfile.Sanitize();

            ApplyProfile(currentProfile);

            bool visible =
                outlineEnabled &&
                currentState != OutlineState.Normal &&
                currentProfile.Intensity > 0f;

            ApplyVisibility(visible);

            enabled = false;
        }


        private void FindSourceRenderers()
        {
            sourceRenderers.Clear();

            if (rendererSource == OutlineRendererSource.Manual)
            {
                if (manualRenderers == null)
                {
                    return;
                }

                for (int i = 0; i < manualRenderers.Length; i++)
                {
                    Renderer renderer = manualRenderers[i];

                    if (renderer == null)
                    {
                        continue;
                    }

                    if (renderer is MeshRenderer ||
                        renderer is SkinnedMeshRenderer)
                    {
                        sourceRenderers.Add(renderer);
                    }
                }

                return;
            }

            Renderer[] renderers =
                GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                if (renderer is MeshRenderer ||
                    renderer is SkinnedMeshRenderer)
                {
                    sourceRenderers.Add(renderer);
                }
            }
        }


        private void CreateOutlineMaterial()
        {
            Shader shader = Shader.Find(OutlineShaderName);

            if (shader == null)
            {
                Debug.LogError(
                    $"[{nameof(AAAOutlineController)}] " +
                    $"Could not find shader '{OutlineShaderName}'.",
                    this);

                return;
            }

            outlineMaterial = new Material(shader)
            {
                name = $"{name}_AAAOutlineMaterial",
                hideFlags =
                    HideFlags.HideAndDontSave
            };
        }


        private void CreateOutlineRenderers()
        {
            if (outlineMaterial == null)
            {
                return;
            }

            outlineRoot = new GameObject(
                $"{name}_AAAOutline_Renderers");

            outlineRoot.transform.SetParent(
                transform,
                false);

            outlineRoot.hideFlags =
                HideFlags.HideAndDontSave;

            outlineRoot.SetActive(false);

            for (int i = 0; i < sourceRenderers.Count; i++)
            {
                Renderer source = sourceRenderers[i];

                if (source == null)
                {
                    continue;
                }

                GameObject clone =
                    new GameObject(
                        $"{source.name}_Outline");

                clone.transform.SetParent(
                    outlineRoot.transform,
                    false);

                clone.hideFlags =
                    HideFlags.HideAndDontSave;

                Renderer cloneRenderer;

                if (source is SkinnedMeshRenderer skinnedSource)
                {
                    SkinnedMeshRenderer skinnedClone =
                        clone.AddComponent<SkinnedMeshRenderer>();

                    skinnedClone.sharedMesh =
                        skinnedSource.sharedMesh;

                    skinnedClone.rootBone =
                        skinnedSource.rootBone;

                    skinnedClone.bones =
                        skinnedSource.bones;

                    skinnedClone.localBounds =
                        skinnedSource.localBounds;

                    skinnedClone.updateWhenOffscreen =
                        skinnedSource.updateWhenOffscreen;

                    cloneRenderer = skinnedClone;
                }
                else
                {
                    MeshRenderer meshSource =
                        source as MeshRenderer;

                    MeshFilter sourceFilter =
                        source.GetComponent<MeshFilter>();

                    MeshFilter cloneFilter =
                        clone.AddComponent<MeshFilter>();

                    MeshRenderer meshClone =
                        clone.AddComponent<MeshRenderer>();

                    if (sourceFilter != null)
                    {
                        cloneFilter.sharedMesh =
                            sourceFilter.sharedMesh;
                    }

                    meshClone.shadowCastingMode =
                        ShadowCastingMode.Off;

                    meshClone.receiveShadows = false;

                    cloneRenderer = meshClone;
                }

                CopyTransform(source, clone.transform);

                cloneRenderer.sharedMaterial =
                    outlineMaterial;

                cloneRenderer.shadowCastingMode =
                    ShadowCastingMode.Off;

                cloneRenderer.receiveShadows = false;

                cloneRenderer.lightProbeUsage =
                    LightProbeUsage.Off;

                cloneRenderer.reflectionProbeUsage =
                    ReflectionProbeUsage.Off;

                outlineRenderers.Add(cloneRenderer);
            }
        }


        private static void CopyTransform(
            Renderer source,
            Transform destination)
        {
            Transform sourceTransform =
                source.transform;

            destination.localPosition =
                sourceTransform.localPosition;

            destination.localRotation =
                sourceTransform.localRotation;

            destination.localScale =
                sourceTransform.localScale;
        }


        public void SetState(
            OutlineState state)
        {
            SetState(
                state,
                transitionDuration);
        }


        public void SetState(
            OutlineState state,
            float duration)
        {
            Initialize();

            AAAOutlineProfile profile =
                GetProfile(state);

            if (profile == null)
            {
                return;
            }

            profile.Sanitize();

            currentState = state;

            targetProfile =
                profile.Clone();

            activeTransitionDuration =
                Mathf.Max(0f, duration);

            transitionTimer = 0f;

            if (activeTransitionDuration <= 0f)
            {
                currentProfile =
                    targetProfile.Clone();

                ApplyProfile(currentProfile);

                isTransitioning = false;

                bool visible =
                    outlineEnabled &&
                    state != OutlineState.Normal &&
                    currentProfile.Intensity > 0f;

                ApplyVisibility(visible);

                UpdatePulseState();

                enabled =
                    isPulsing;

                return;
            }

            bool shouldBeVisible =
                outlineEnabled &&
                state != OutlineState.Normal;

            ApplyVisibility(shouldBeVisible);

            isTransitioning = true;

            enabled = true;
        }


        public void ClearState()
        {
            SetState(
                OutlineState.Normal,
                transitionDuration);
        }


        public void SetOutlineEnabled(
            bool enabledState)
        {
            Initialize();

            outlineEnabled =
                enabledState;

            bool visible =
                outlineEnabled &&
                currentState != OutlineState.Normal &&
                currentProfile != null &&
                currentProfile.Intensity > 0f;

            ApplyVisibility(visible);

            if (!visible)
            {
                isTransitioning = false;
                isPulsing = false;
                enabled = false;
            }
            else
            {
                UpdatePulseState();

                enabled =
                    isTransitioning ||
                    isPulsing;
            }
        }


        private void UpdateTransition()
        {
            transitionTimer += Time.deltaTime;

            float t =
                activeTransitionDuration <= 0f
                    ? 1f
                    : transitionTimer /
                      activeTransitionDuration;

            t = Mathf.Clamp01(t);

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            currentProfile.Color =
                Color.Lerp(
                    currentProfile.Color,
                    targetProfile.Color,
                    smoothT);

            currentProfile.Width =
                Mathf.Lerp(
                    currentProfile.Width,
                    targetProfile.Width,
                    smoothT);

            currentProfile.Intensity =
                Mathf.Lerp(
                    currentProfile.Intensity,
                    targetProfile.Intensity,
                    smoothT);

            currentProfile.FresnelPower =
                Mathf.Lerp(
                    currentProfile.FresnelPower,
                    targetProfile.FresnelPower,
                    smoothT);

            currentProfile.FresnelStrength =
                Mathf.Lerp(
                    currentProfile.FresnelStrength,
                    targetProfile.FresnelStrength,
                    smoothT);

            currentProfile.PulseSpeed =
                Mathf.Lerp(
                    currentProfile.PulseSpeed,
                    targetProfile.PulseSpeed,
                    smoothT);

            currentProfile.PulseMin =
                Mathf.Lerp(
                    currentProfile.PulseMin,
                    targetProfile.PulseMin,
                    smoothT);

            currentProfile.PulseMax =
                Mathf.Lerp(
                    currentProfile.PulseMax,
                    targetProfile.PulseMax,
                    smoothT);

            currentProfile.FresnelEnabled =
                smoothT < 0.5f
                    ? currentProfile.FresnelEnabled
                    : targetProfile.FresnelEnabled;

            currentProfile.PulseEnabled =
                smoothT < 0.5f
                    ? currentProfile.PulseEnabled
                    : targetProfile.PulseEnabled;

            currentProfile.VisibilityMode =
                smoothT < 0.5f
                    ? currentProfile.VisibilityMode
                    : targetProfile.VisibilityMode;

            ApplyProfile(currentProfile);

            if (t >= 1f)
            {
                currentProfile =
                    targetProfile.Clone();

                ApplyProfile(currentProfile);

                isTransitioning = false;

                bool visible =
                    outlineEnabled &&
                    currentState != OutlineState.Normal &&
                    currentProfile.Intensity > 0f;

                ApplyVisibility(visible);

                UpdatePulseState();
            }
        }


        private void UpdatePulse()
        {
            if (outlineMaterial == null ||
                currentProfile == null)
            {
                return;
            }

            float pulse =
                Mathf.Lerp(
                    currentProfile.PulseMin,
                    currentProfile.PulseMax,
                    (Mathf.Sin(
                        Time.time *
                        currentProfile.PulseSpeed) + 1f) *
                    0.5f);

            outlineMaterial.SetFloat(
                OutlineIntensityId,
                currentProfile.Intensity *
                pulse);
        }


        private void ApplyProfile(
            AAAOutlineProfile profile)
        {
            if (outlineMaterial == null ||
                profile == null)
            {
                return;
            }

            outlineMaterial.SetColor(
                OutlineColorId,
                profile.Color);

            outlineMaterial.SetFloat(
                OutlineWidthId,
                profile.Width);

            outlineMaterial.SetFloat(
                OutlineIntensityId,
                profile.Intensity);

            outlineMaterial.SetFloat(
                FresnelPowerId,
                profile.FresnelPower);

            outlineMaterial.SetFloat(
                FresnelStrengthId,
                profile.FresnelStrength);

            outlineMaterial.SetFloat(
                PulseSpeedId,
                profile.PulseSpeed);

            outlineMaterial.SetFloat(
                PulseMinId,
                profile.PulseMin);

            outlineMaterial.SetFloat(
                PulseMaxId,
                profile.PulseMax);

            outlineMaterial.SetFloat(
                ThroughWallAlphaId,
                profile.VisibilityMode ==
                OutlineVisibilityMode.ThroughWalls
                    ? 0.35f
                    : 1f);

            ApplyVisibilityMode(
                profile.VisibilityMode);

            UpdatePulseState();
        }


        private void ApplyVisibility(
            bool visible)
        {
            if (outlineRoot == null)
            {
                return;
            }

            outlineRoot.SetActive(
                visible);
        }


        private void ApplyVisibilityMode(
            OutlineVisibilityMode mode)
        {
            if (outlineRenderers.Count == 0)
            {
                return;
            }

            CompareFunction zTest;

            switch (mode)
            {
                case OutlineVisibilityMode.AlwaysVisible:
                    zTest =
                        CompareFunction.Always;
                    break;

                case OutlineVisibilityMode.ThroughWalls:
                    zTest =
                        CompareFunction.Always;
                    break;

                default:
                    zTest =
                        CompareFunction.LessEqual;
                    break;
            }

            outlineMaterial.SetFloat(
                "_ZTest",
                (float)zTest);
        }


        private void UpdatePulseState()
        {
            isPulsing =
                currentProfile != null &&
                currentProfile.PulseEnabled &&
                outlineEnabled &&
                currentState != OutlineState.Normal;
        }


        private AAAOutlineProfile GetProfile(
            OutlineState state)
        {
            switch (state)
            {
                case OutlineState.Hover:
                    return hoverProfile;

                case OutlineState.Selected:
                    return selectedProfile;

                case OutlineState.Target:
                    return targetProfile;

                case OutlineState.Warning:
                    return warningProfile;

                case OutlineState.Normal:
                default:
                    return normalProfile;
            }
        }


        /// <summary>
        /// Returns the highest-priority state from the supplied states.
        /// </summary>
        public static OutlineState GetHighestPriorityState(
            bool hover,
            bool selected,
            bool target,
            bool warning)
        {
            if (warning)
            {
                return OutlineState.Warning;
            }

            if (target)
            {
                return OutlineState.Target;
            }

            if (selected)
            {
                return OutlineState.Selected;
            }

            if (hover)
            {
                return OutlineState.Hover;
            }

            return OutlineState.Normal;
        }


        private void OnDestroy()
        {
            if (outlineRoot != null)
            {
                DestroyObjectSafe(
                    outlineRoot);
            }

            if (outlineMaterial != null)
            {
                DestroyObjectSafe(
                    outlineMaterial);
            }

            outlineRenderers.Clear();
            sourceRenderers.Clear();
        }


        private static void DestroyObjectSafe(
            Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}