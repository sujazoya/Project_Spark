
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.AI
{
    public enum SparkAIViseme
    {
        Silence,
        Aa,
        Ee,
        Ih,
        Oh,
        Ou,
        FV,
        MBP,
        L,
        WQ
    }

    [Serializable]
    public sealed class SparkAIFacialShape
    {
        public string shapeName;
        [Min(-1)] public int blendShapeIndex = -1;
        [Range(0f, 100f)] public float maximumWeight = 100f;
        [Range(0f, 2f)] public float multiplier = 1f;
    }

    [Serializable]
    public sealed class SparkAIFacialExpression
    {
        public string emotionName = "Neutral";
        public SparkAIFacialShape[] shapes =
            Array.Empty<SparkAIFacialShape>();
    }

    [Serializable]
    public sealed class SparkAIVisemeMapping
    {
        public SparkAIViseme viseme;
        public SparkAIFacialShape[] shapes =
            Array.Empty<SparkAIFacialShape>();
    }

    [DisallowMultipleComponent]
    public sealed class SparkAIFacialAnimationController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SparkAIVoiceController voiceController;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private SkinnedMeshRenderer faceRenderer;

        [Header("BlendShape Resolution")]
        [SerializeField] private bool resolveShapeNamesAutomatically = true;
        [SerializeField] private bool logMissingShapes;

        [Header("Lip Sync")]
        [SerializeField] private bool enableLipSync = true;
        [SerializeField, Range(0f, 1f)]
        private float speechThreshold = 0.012f;
        [SerializeField, Range(0.01f, 1f)]
        private float maximumMouthOpen = 0.8f;
        [SerializeField, Range(0.001f, 0.2f)]
        private float attackTime = 0.035f;
        [SerializeField, Range(0.01f, 0.5f)]
        private float releaseTime = 0.11f;
        [SerializeField, Range(0.01f, 0.5f)]
        private float visemeSmoothing = 0.075f;
        [SerializeField, Range(0.5f, 8f)]
        private float audioGain = 3f;
        [SerializeField, Range(0f, 1f)]
        private float vowelInfluence = 0.65f;
        [SerializeField, Range(0f, 1f)]
        private float consonantInfluence = 0.2f;
        [SerializeField, Min(64)]
        private int spectrumSize = 256;

        [Header("Viseme Mapping")]
        [SerializeField] private SparkAIVisemeMapping[] visemeMappings =
        {
            new SparkAIVisemeMapping { viseme = SparkAIViseme.Aa },
            new SparkAIVisemeMapping { viseme = SparkAIViseme.Ee },
            new SparkAIVisemeMapping { viseme = SparkAIViseme.Ih },
            new SparkAIVisemeMapping { viseme = SparkAIViseme.Oh },
            new SparkAIVisemeMapping { viseme = SparkAIViseme.Ou },
            new SparkAIVisemeMapping { viseme = SparkAIViseme.FV },
            new SparkAIVisemeMapping { viseme = SparkAIViseme.MBP },
            new SparkAIVisemeMapping { viseme = SparkAIViseme.L },
            new SparkAIVisemeMapping { viseme = SparkAIViseme.WQ }
        };

        [Header("Expressions")]
        [SerializeField] private bool enableExpressions = true;
        [SerializeField] private float expressionSmoothing = 0.18f;
        [SerializeField] private SparkAIFacialExpression[] expressions =
            Array.Empty<SparkAIFacialExpression>();

        [Header("Blink")]
        [SerializeField] private bool enableBlink = true;
        [SerializeField] private SparkAIFacialShape blinkLeft;
        [SerializeField] private SparkAIFacialShape blinkRight;
        [SerializeField, Min(0.5f)] private float minimumBlinkInterval = 2f;
        [SerializeField, Min(1f)] private float maximumBlinkInterval = 5f;
        [SerializeField, Range(0.02f, 0.25f)] private float blinkDuration = 0.12f;

        [Header("Eye Gaze")]
        [SerializeField] private bool enableEyeGaze;
        [SerializeField] private Transform leftEye;
        [SerializeField] private Transform rightEye;
        [SerializeField] private Transform gazeTarget;
        [SerializeField] private float eyeYawLimit = 18f;
        [SerializeField] private float eyePitchLimit = 12f;
        [SerializeField] private float eyeSmoothing = 8f;

        [Header("Head Motion")]
        [SerializeField] private bool enableHeadMotion;
        [SerializeField] private Transform headBone;
        [SerializeField] private float headYawLimit = 3f;
        [SerializeField] private float headPitchLimit = 2f;
        [SerializeField] private float headMotionSpeed = 1.2f;
        [SerializeField] private float headSmoothing = 3f;

        [Header("Playback")]
        [SerializeField] private bool animateOnlyWhileAudioPlays = true;
        [SerializeField] private bool resetOnStop = true;
        [SerializeField] private bool logPlayback;

        private const int SampleCount = 1024;

        private float[] samples = new float[SampleCount];
        private float[] spectrum = new float[SampleCount];
        private float[] baseWeights;
        private float[] targetWeights;
        private float[] currentVisemes = new float[10];
        private float[] targetVisemes = new float[10];

        private readonly Dictionary<string, SparkAIFacialExpression>
            expressionLookup =
                new Dictionary<string, SparkAIFacialExpression>(
                    StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<Transform, Quaternion>
            originalRotations =
                new Dictionary<Transform, Quaternion>();

        private float currentExpressionWeight;
        private float targetExpressionWeight;
        private float nextBlinkTime;
        private float blinkTimer;
        private float blinkValue;
        private bool blinkClosing;
        private bool previousAudioPlaying;
        private bool initialized;
        private string currentEmotion = "Neutral";

        public bool IsAnimating { get; private set; }
        public bool IsAudioPlaying =>
            audioSource != null && audioSource.isPlaying;

        public string CurrentEmotion => currentEmotion;

        private void Awake()
        {
            spectrumSize = Mathf.Clamp(spectrumSize, 64, SampleCount);
            CaptureOriginalRotations();
            InitializeBlendShapes();
            BuildExpressionLookup();
            ScheduleNextBlink();
        }

        private void OnEnable()
        {
            if (voiceController != null)
            {
                voiceController.VoiceRequestCreated += OnVoiceRequest;
                voiceController.VoiceStopped += OnVoiceStopped;
            }
        }

        private void Start()
        {
            if (voiceController != null && voiceController.IsSpeaking)
                currentEmotion = voiceController.CurrentEmotion.ToString();
        }

        private void OnDisable()
        {
            UnsubscribeVoiceEvents();
            RestoreFace();
            RestoreOriginalRotations();
            previousAudioPlaying = false;
            IsAnimating = false;
        }

        private void OnDestroy()
        {
            UnsubscribeVoiceEvents();
        }

        private void UnsubscribeVoiceEvents()
        {
            if (voiceController == null)
                return;

            voiceController.VoiceRequestCreated -= OnVoiceRequest;
            voiceController.VoiceStopped -= OnVoiceStopped;
        }

        private void Update()
        {
            if (!initialized || faceRenderer == null ||
                faceRenderer.sharedMesh == null)
                return;

            bool playing = IsAudioPlaying;

            if (playing && !previousAudioPlaying)
            {
                IsAnimating = true;
                if (logPlayback)
                    Debug.Log("[AI FACE] Audio playback started.", this);
            }
            else if (!playing && previousAudioPlaying)
            {
                if (logPlayback)
                    Debug.Log("[AI FACE] Audio playback ended.", this);
            }

            previousAudioPlaying = playing;

            bool shouldAnimate =
                !animateOnlyWhileAudioPlays || playing;

            if (shouldAnimate && enableLipSync && playing)
                AnalyzeAudio();
            else
                ClearVisemeTargets();

            UpdateVisemeWeights();
            UpdateBlink();
            UpdateExpression();
            ComposeBlendShapeWeights();
            UpdateEyeGaze();
            UpdateHeadMotion();

            if (!playing && !blinkClosing && blinkValue <= 0.001f)
                IsAnimating = false;
        }

        private void InitializeBlendShapes()
        {
            if (faceRenderer == null || faceRenderer.sharedMesh == null)
            {
                Debug.LogWarning(
                    "[AI FACE] Assign a SkinnedMeshRenderer with a mesh.",
                    this);
                return;
            }

            int count = faceRenderer.sharedMesh.blendShapeCount;
            baseWeights = new float[count];
            targetWeights = new float[count];

            for (int i = 0; i < count; i++)
            {
                baseWeights[i] =
                    faceRenderer.GetBlendShapeWeight(i);
                targetWeights[i] = baseWeights[i];
            }

            ResolveAllMappings();
            initialized = true;
        }

        private void ResolveAllMappings()
        {
            if (visemeMappings != null)
            {
                foreach (var mapping in visemeMappings)
                    ResolveShapes(mapping?.shapes);
            }

            if (expressions != null)
            {
                foreach (var expression in expressions)
                    ResolveShapes(expression?.shapes);
            }

            ResolveShapes(new[] { blinkLeft, blinkRight });
        }

        private void ResolveShapes(SparkAIFacialShape[] shapes)
        {
            if (shapes == null)
                return;

            foreach (var shape in shapes)
                ResolveShape(shape);
        }

        private void ResolveShape(SparkAIFacialShape shape)
        {
            if (shape == null || faceRenderer == null)
                return;

            var mesh = faceRenderer.sharedMesh;

            if (shape.blendShapeIndex >= 0 &&
                shape.blendShapeIndex < mesh.blendShapeCount)
                return;

            if (!resolveShapeNamesAutomatically ||
                string.IsNullOrWhiteSpace(shape.shapeName))
                return;

            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                if (string.Equals(
                    mesh.GetBlendShapeName(i),
                    shape.shapeName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    shape.blendShapeIndex = i;
                    return;
                }
            }

            if (logMissingShapes)
            {
                Debug.LogWarning(
                    $"[AI FACE] BlendShape not found: {shape.shapeName}",
                    this);
            }
        }

        private void BuildExpressionLookup()
        {
            expressionLookup.Clear();

            if (expressions == null)
                return;

            foreach (var expression in expressions)
            {
                if (expression == null ||
                    string.IsNullOrWhiteSpace(expression.emotionName))
                    continue;

                expressionLookup[expression.emotionName.Trim()] = expression;
            }
        }

        private void AnalyzeAudio()
        {
            Array.Clear(samples, 0, samples.Length);
            Array.Clear(spectrum, 0, spectrum.Length);
            Array.Clear(targetVisemes, 0, targetVisemes.Length);

            audioSource.GetOutputData(samples, 0);
            audioSource.GetSpectrumData(
                spectrum, 0, FFTWindow.BlackmanHarris);

            double sum = 0d;
            for (int i = 0; i < samples.Length; i++)
                sum += samples[i] * samples[i];

            float rms = Mathf.Sqrt((float)(sum / samples.Length));
            float level = Mathf.Clamp01(
                (rms - speechThreshold) * audioGain);

            if (level <= 0f)
                return;

            float low = BandEnergy(1, 5);
            float lowMid = BandEnergy(5, 18);
            float mid = BandEnergy(18, 55);
            float highMid = BandEnergy(55, 140);
            float high = BandEnergy(140, 350);

            float total = low + lowMid + mid + highMid + high + 0.00001f;

            float lowRatio = low / total;
            float midRatio = mid / total;
            float highRatio = (highMid + high) / total;

            float open = Mathf.Clamp01(level * maximumMouthOpen);
            targetVisemes[(int)SparkAIViseme.Aa] = open * Mathf.Lerp(
                0.5f, 1f, lowMid / (lowMid + mid + 0.00001f));
            targetVisemes[(int)SparkAIViseme.Ee] =
                open * Mathf.Clamp01(highRatio * 2.5f);
            targetVisemes[(int)SparkAIViseme.Ih] =
                open * Mathf.Clamp01(midRatio * 2f);
            targetVisemes[(int)SparkAIViseme.Oh] =
                open * Mathf.Clamp01(lowRatio * 2.5f);
            targetVisemes[(int)SparkAIViseme.Ou] =
                open * Mathf.Clamp01(lowRatio * 3f) * 0.8f;

            targetVisemes[(int)SparkAIViseme.FV] =
                open * highRatio * consonantInfluence;
            targetVisemes[(int)SparkAIViseme.MBP] =
                open * Mathf.Clamp01(1f - level) * consonantInfluence;
            targetVisemes[(int)SparkAIViseme.L] =
                open * midRatio * consonantInfluence;
            targetVisemes[(int)SparkAIViseme.WQ] =
                open * lowRatio * consonantInfluence;

            float vowelTotal =
                targetVisemes[(int)SparkAIViseme.Aa] +
                targetVisemes[(int)SparkAIViseme.Ee] +
                targetVisemes[(int)SparkAIViseme.Ih] +
                targetVisemes[(int)SparkAIViseme.Oh] +
                targetVisemes[(int)SparkAIViseme.Ou];

            if (vowelTotal > 0.0001f)
            {
                float scale = vowelInfluence /
                    Mathf.Max(vowelInfluence, vowelTotal);
                for (int i = 1; i <= 5; i++)
                    targetVisemes[i] *= scale;
            }
        }

        private float BandEnergy(int start, int end)
        {
            float result = 0f;
            int max = Mathf.Min(end, spectrum.Length - 1);

            for (int i = start; i <= max; i++)
                result += spectrum[i];

            return result;
        }

        private void ClearVisemeTargets()
        {
            Array.Clear(targetVisemes, 0, targetVisemes.Length);
        }

        private void UpdateVisemeWeights()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < currentVisemes.Length; i++)
            {
                float destination = targetVisemes[i];
                float smoothTime = destination > currentVisemes[i]
                    ? attackTime
                    : releaseTime;

                if (i > 0 && destination > 0f)
                    smoothTime = visemeSmoothing;

                float blend = 1f - Mathf.Exp(
                    -dt / Mathf.Max(0.001f, smoothTime));

                currentVisemes[i] = Mathf.Lerp(
                    currentVisemes[i], destination, blend);
            }
        }

        private void ComposeBlendShapeWeights()
        {
            Array.Copy(baseWeights, targetWeights, baseWeights.Length);

            if (visemeMappings != null)
            {
                foreach (var mapping in visemeMappings)
                {
                    if (mapping == null || mapping.shapes == null)
                        continue;

                    int vi = (int)mapping.viseme;
                    if (vi < 0 || vi >= currentVisemes.Length)
                        continue;

                    float amount = currentVisemes[vi];

                    foreach (var shape in mapping.shapes)
                        ApplyShape(shape, amount, false);
                }
            }

            ApplyExpressionWeights();

            if (enableBlink)
            {
                ApplyShape(blinkLeft, blinkValue, true);
                ApplyShape(blinkRight, blinkValue, true);
            }

            for (int i = 0; i < targetWeights.Length; i++)
                faceRenderer.SetBlendShapeWeight(
                    i, Mathf.Clamp(targetWeights[i], 0f, 100f));
        }

        private void ApplyShape(
            SparkAIFacialShape shape,
            float amount,
            bool overrideBase)
        {
            if (shape == null)
                return;

            int index = shape.blendShapeIndex;
            if (index < 0 || index >= targetWeights.Length)
                return;

            float weight = Mathf.Clamp(
                amount * shape.maximumWeight * shape.multiplier,
                0f, 100f);

            targetWeights[index] = overrideBase
                ? Mathf.Max(baseWeights[index], weight)
                : Mathf.Max(targetWeights[index], weight);
        }

        private void ApplyExpressionWeights()
        {
            if (!enableExpressions ||
                !expressionLookup.TryGetValue(
                    currentEmotion, out var expression))
                return;

            if (expression?.shapes == null)
                return;

            foreach (var shape in expression.shapes)
                ApplyShape(shape, currentExpressionWeight, false);
        }

        private void OnVoiceRequest(SparkAIVoiceRequest request)
        {
            if (!request.IsValid)
                return;

            currentEmotion = request.Emotion.ToString();
            targetExpressionWeight = 1f;
        }

        private void OnVoiceStopped()
        {
            ClearVisemeTargets();
            Array.Clear(currentVisemes, 0, currentVisemes.Length);
            targetExpressionWeight = 0f;

            if (resetOnStop)
            {
                blinkValue = 0f;
                blinkClosing = false;
            }

            IsAnimating = false;
        }

        private void UpdateExpression()
        {
            float target = IsAudioPlaying ? targetExpressionWeight : 0f;

            currentExpressionWeight = Mathf.Lerp(
                currentExpressionWeight,
                target,
                1f - Mathf.Exp(
                    -Time.deltaTime /
                    Mathf.Max(0.001f, expressionSmoothing)));
        }

        private void UpdateBlink()
        {
            if (!enableBlink)
            {
                blinkValue = 0f;
                return;
            }

            if (!blinkClosing && Time.time >= nextBlinkTime)
                blinkClosing = true;

            if (blinkClosing)
            {
                blinkTimer += Time.deltaTime /
                    Mathf.Max(0.02f, blinkDuration);

                blinkValue = blinkTimer <= 0.5f
                    ? Mathf.Clamp01(blinkTimer * 2f)
                    : Mathf.Clamp01((1f - blinkTimer) * 2f);

                if (blinkTimer >= 1f)
                {
                    blinkTimer = 0f;
                    blinkValue = 0f;
                    blinkClosing = false;
                    ScheduleNextBlink();
                }
            }
        }

        private void ScheduleNextBlink()
        {
            float min = Mathf.Max(0.5f, minimumBlinkInterval);
            float max = Mathf.Max(min, maximumBlinkInterval);
            nextBlinkTime = Time.time + UnityEngine.Random.Range(min, max);
        }

        private void CaptureOriginalRotations()
        {
            originalRotations.Clear();

            if (leftEye != null)
                originalRotations[leftEye] = leftEye.localRotation;

            if (rightEye != null)
                originalRotations[rightEye] = rightEye.localRotation;

            if (headBone != null)
                originalRotations[headBone] = headBone.localRotation;
        }

        private void UpdateEyeGaze()
        {
            if (!enableEyeGaze || gazeTarget == null)
                return;

            UpdateEye(leftEye);
            UpdateEye(rightEye);
        }

        private void UpdateEye(Transform eye)
        {
            if (eye == null ||
                !originalRotations.TryGetValue(eye, out var original))
                return;

            Vector3 direction =
                gazeTarget.position - eye.position;

            Vector3 localDirection =
                eye.parent != null
                    ? eye.parent.InverseTransformDirection(direction)
                    : direction;

            if (localDirection.sqrMagnitude < 0.0001f)
                return;

            Quaternion look = Quaternion.LookRotation(
                localDirection.normalized, Vector3.up);

            Vector3 euler = look.eulerAngles;
            euler.x = ClampAngle(euler.x, -eyePitchLimit, eyePitchLimit);
            euler.y = ClampAngle(euler.y, -eyeYawLimit, eyeYawLimit);
            euler.z = 0f;

            Quaternion target = Quaternion.Euler(euler);

            eye.localRotation = Quaternion.Slerp(
                eye.localRotation,
                target,
                1f - Mathf.Exp(-eyeSmoothing * Time.deltaTime));
        }

        private void UpdateHeadMotion()
        {
            if (!enableHeadMotion || headBone == null ||
                !originalRotations.TryGetValue(headBone, out var original))
                return;

            float weight = IsAudioPlaying ? 1f : 0f;
            float t = Time.time * headMotionSpeed;

            Quaternion offset = Quaternion.Euler(
                Mathf.Sin(t * 0.73f) * headPitchLimit * weight,
                Mathf.Sin(t) * headYawLimit * weight,
                0f);

            Quaternion target = original * offset;

            headBone.localRotation = Quaternion.Slerp(
                headBone.localRotation,
                target,
                1f - Mathf.Exp(-headSmoothing * Time.deltaTime));
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            angle = Mathf.DeltaAngle(0f, angle);
            return Mathf.Clamp(angle, min, max);
        }

        private void RestoreFace()
        {
            if (faceRenderer == null || baseWeights == null ||
                faceRenderer.sharedMesh == null)
                return;

            int count = Mathf.Min(
                baseWeights.Length,
                faceRenderer.sharedMesh.blendShapeCount);

            for (int i = 0; i < count; i++)
                faceRenderer.SetBlendShapeWeight(i, baseWeights[i]);
        }

        private void RestoreOriginalRotations()
        {
            foreach (var pair in originalRotations)
            {
                if (pair.Key != null)
                    pair.Key.localRotation = pair.Value;
            }
        }

        public void SetGazeTarget(Transform target)
        {
            gazeTarget = target;
        }

        public void SetEmotion(string emotionName)
        {
            if (!string.IsNullOrWhiteSpace(emotionName))
            {
                currentEmotion = emotionName.Trim();
                targetExpressionWeight = 1f;
            }
        }

        public void ResetFace()
        {
            ClearVisemeTargets();
            Array.Clear(currentVisemes, 0, currentVisemes.Length);
            currentExpressionWeight = 0f;
            targetExpressionWeight = 0f;
            blinkValue = 0f;
            blinkTimer = 0f;
            blinkClosing = false;

            RestoreFace();
            RestoreOriginalRotations();
            ScheduleNextBlink();
            IsAnimating = false;
        }

        private void OnValidate()
        {
            spectrumSize = Mathf.Clamp(spectrumSize, 64, SampleCount);

            if (maximumBlinkInterval < minimumBlinkInterval)
                maximumBlinkInterval = minimumBlinkInterval;
        }
    }
}
