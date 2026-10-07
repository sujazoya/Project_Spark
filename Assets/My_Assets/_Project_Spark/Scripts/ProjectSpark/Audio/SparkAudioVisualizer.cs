using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Audio
{
    /// <summary>
    /// Project Spark futuristic voice-wave UI visualizer.
    ///
    /// Attach this component to a UI GameObject with a RectTransform.
    /// An AudioSource is required on the same GameObject.
    ///
    /// Features:
    /// - Full-width animated voice wave
    /// - Multiple flowing wave lines
    /// - Live AudioSource amplitude analysis
    /// - Pitch-reactive movement
    /// - Smooth animation
    /// - Color gradient across the wave
    /// - Automatic idle animation
    /// - No controller
    /// - No buttons
    /// - No text
    /// - No external textures
    ///
    /// AudioSource.Play() automatically drives the visual.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(AudioSource))]
    [DisallowMultipleComponent]
    public sealed class SparkAudioVisualizer : MonoBehaviour
    {
        [Header("Wave Visual")]

        [SerializeField]
        [Range(32, 256)]
        private int points = 128;

        [SerializeField]
        [Range(1, 8)]
        private int waveLines = 4;

        [SerializeField]
        [Range(1f, 10f)]
        private float animationSpeed = 3.5f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float lineWidth = 2.5f;

        [Header("Voice Reaction")]

        [SerializeField]
        [Range(0.1f, 3f)]
        private float amplitudeStrength = 1.4f;

        [SerializeField]
        [Range(0.1f, 3f)]
        private float pitchStrength = 1f;

        [SerializeField]
        [Range(0.01f, 1f)]
        private float smoothing = 0.18f;

        [Header("Wave Shape")]

        [SerializeField]
        [Range(1f, 20f)]
        private float frequency = 8f;

        [SerializeField]
        [Range(0.1f, 5f)]
        private float frequencyVariation = 1.4f;

        [SerializeField]
        [Range(0f, 1f)]
        private float distortion = 0.35f;

        [Header("Colors")]

        [SerializeField]
        private Color startColor = new Color(0.1f, 0.8f, 1f, 1f);

        [SerializeField]
        private Color middleColor = new Color(0.9f, 0.2f, 1f, 1f);

        [SerializeField]
        private Color endColor = new Color(1f, 0.3f, 0.5f, 1f);

        [Header("Idle")]

        [SerializeField]
        private bool animateWhenIdle = true;

        [SerializeField]
        [Range(0f, 0.3f)]
        private float idleStrength = 0.025f;

        private AudioSource audioSource;
        private RectTransform rectTransform;
        private SparkVoiceWaveGraphic graphic;

        private float[] samples;

        private float currentAmplitude;
        private float targetAmplitude;

        private float currentPitch;
        private float targetPitch;

        private float animationTime;

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            rectTransform = GetComponent<RectTransform>();

            CreateGraphic();
        }

        private void OnEnable()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (graphic == null)
            {
                CreateGraphic();
            }
        }

        private void Update()
        {
            if (audioSource == null || graphic == null)
            {
                return;
            }

            animationTime +=
                Time.unscaledDeltaTime *
                animationSpeed;

            AnalyzeAudio();

            graphic.SetWaveData(
                currentAmplitude,
                currentPitch,
                animationTime,
                points,
                waveLines,
                frequency,
                frequencyVariation,
                distortion,
                amplitudeStrength,
                pitchStrength,
                lineWidth,
                startColor,
                middleColor,
                endColor);
        }

        private void AnalyzeAudio()
        {
            EnsureSamples();

            if (!audioSource.isPlaying)
            {
                targetAmplitude =
                    animateWhenIdle
                        ? idleStrength
                        : 0f;

                targetPitch = 0f;

                currentAmplitude = Mathf.Lerp(
                    currentAmplitude,
                    targetAmplitude,
                    smoothing);

                currentPitch = Mathf.Lerp(
                    currentPitch,
                    targetPitch,
                    smoothing);

                return;
            }

            audioSource.GetOutputData(
                samples,
                0);

            float rms =
                CalculateRMS(samples);

            targetAmplitude =
                Mathf.Clamp01(
                    rms * 8f);

            currentAmplitude = Mathf.Lerp(
                currentAmplitude,
                targetAmplitude,
                smoothing);

            float pitch =
                DetectPitch(
                    samples,
                    AudioSettings.outputSampleRate);

            if (pitch > 0f)
            {
                targetPitch =
                    Mathf.InverseLerp(
                        80f,
                        1000f,
                        pitch);

                targetPitch =
                    targetPitch * 2f - 1f;
            }
            else
            {
                targetPitch =
                    Mathf.Sin(
                        animationTime * 0.7f) *
                    0.12f;
            }

            currentPitch = Mathf.Lerp(
                currentPitch,
                targetPitch,
                smoothing);
        }

        private void EnsureSamples()
        {
            if (samples == null ||
                samples.Length != 1024)
            {
                samples = new float[1024];
            }
        }

        private float CalculateRMS(float[] buffer)
        {
            if (buffer == null ||
                buffer.Length == 0)
            {
                return 0f;
            }

            float sum = 0f;

            for (int i = 0; i < buffer.Length; i++)
            {
                float value = buffer[i];

                sum += value * value;
            }

            return Mathf.Sqrt(
                sum / buffer.Length);
        }

        private float DetectPitch(
            float[] buffer,
            int sampleRate)
        {
            if (buffer == null ||
                buffer.Length < 512 ||
                sampleRate <= 0)
            {
                return 0f;
            }

            float rms =
                CalculateRMS(buffer);

            if (rms < 0.008f)
            {
                return 0f;
            }

            const float minFrequency = 70f;
            const float maxFrequency = 1000f;

            int minLag =
                Mathf.FloorToInt(
                    sampleRate /
                    maxFrequency);

            int maxLag =
                Mathf.CeilToInt(
                    sampleRate /
                    minFrequency);

            maxLag =
                Mathf.Min(
                    maxLag,
                    buffer.Length - 2);

            float bestCorrelation = 0f;
            int bestLag = 0;

            for (int lag = minLag;
                 lag <= maxLag;
                 lag++)
            {
                float correlation = 0f;

                int length =
                    buffer.Length - lag;

                for (int i = 0;
                     i < length;
                     i++)
                {
                    correlation +=
                        buffer[i] *
                        buffer[i + lag];
                }

                correlation /= length;

                if (correlation > bestCorrelation)
                {
                    bestCorrelation =
                        correlation;

                    bestLag = lag;
                }
            }

            if (bestLag <= 0)
            {
                return 0f;
            }

            float normalized =
                bestCorrelation /
                Mathf.Max(
                    0.000001f,
                    rms * rms);

            if (normalized < 0.15f)
            {
                return 0f;
            }

            float frequencyValue =
                (float)sampleRate /
                bestLag;

            if (frequencyValue < minFrequency ||
                frequencyValue > maxFrequency)
            {
                return 0f;
            }

            return frequencyValue;
        }

        private void CreateGraphic()
        {
            graphic =
                GetComponent<SparkVoiceWaveGraphic>();

            if (graphic == null)
            {
                graphic =
                    gameObject.AddComponent<
                        SparkVoiceWaveGraphic>();
            }

            graphic.raycastTarget = false;
        }
    }

    /// <summary>
    /// Procedural UGUI renderer for the Spark voice wave.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkVoiceWaveGraphic : Graphic
    {
        private int points = 128;
        private int waveLines = 4;

        private float amplitude;
        private float pitch;
        private float time;

        private float frequency;
        private float frequencyVariation;
        private float distortion;

        private float amplitudeStrength;
        private float pitchStrength;
        private float lineWidth;

        private Color startColor;
        private Color middleColor;
        private Color endColor;

        public void SetWaveData(
            float amplitudeValue,
            float pitchValue,
            float animationTime,
            int pointCount,
            int lineCount,
            float waveFrequency,
            float waveFrequencyVariation,
            float waveDistortion,
            float waveAmplitudeStrength,
            float wavePitchStrength,
            float waveLineWidth,
            Color waveStartColor,
            Color waveMiddleColor,
            Color waveEndColor)
        {
            amplitude =
                Mathf.Clamp01(
                    amplitudeValue);

            pitch =
                Mathf.Clamp(
                    pitchValue,
                    -1f,
                    1f);

            time =
                animationTime;

            points =
                Mathf.Clamp(
                    pointCount,
                    32,
                    256);

            waveLines =
                Mathf.Clamp(
                    lineCount,
                    1,
                    8);

            frequency =
                Mathf.Max(
                    1f,
                    waveFrequency);

            frequencyVariation =
                Mathf.Max(
                    0.1f,
                    waveFrequencyVariation);

            distortion =
                Mathf.Clamp01(
                    waveDistortion);

            amplitudeStrength =
                Mathf.Max(
                    0.01f,
                    waveAmplitudeStrength);

            pitchStrength =
                Mathf.Max(
                    0.01f,
                    wavePitchStrength);

            lineWidth =
                Mathf.Max(
                    0.1f,
                    waveLineWidth);

            startColor =
                waveStartColor;

            middleColor =
                waveMiddleColor;

            endColor =
                waveEndColor;

            SetVerticesDirty();
        }

        protected override void Awake()
        {
            base.Awake();

            raycastTarget = false;
        }

        protected override void OnPopulateMesh(
            VertexHelper vh)
        {
            vh.Clear();

            Rect rect =
                rectTransform.rect;

            if (points < 2)
            {
                return;
            }

            for (int line = 0;
                 line < waveLines;
                 line++)
            {
                DrawWaveLine(
                    vh,
                    rect,
                    line);
            }
        }

        private void DrawWaveLine(
            VertexHelper vh,
            Rect rect,
            int lineIndex)
        {
            float lineNormalized =
                waveLines <= 1
                    ? 0.5f
                    : lineIndex /
                      (float)(waveLines - 1);

            float depth =
                lineNormalized * 2f - 1f;

            float centerY =
                rect.center.y +
                depth *
                rect.height *
                0.22f;

            float lineAmplitude =
                rect.height *
                0.12f *
                amplitude *
                amplitudeStrength;

            float frequencyOffset =
                depth *
                frequencyVariation;

            Vector2 previous =
                CalculatePoint(
                    rect,
                    centerY,
                    lineAmplitude,
                    frequencyOffset,
                    lineIndex,
                    0);

            for (int i = 1;
                 i < points;
                 i++)
            {
                Vector2 current =
                    CalculatePoint(
                        rect,
                        centerY,
                        lineAmplitude,
                        frequencyOffset,
                        lineIndex,
                        i);

                Color color =
                    CalculateColor(
                        i /
                        (float)(points - 1));

                AddLine(
                    vh,
                    previous,
                    current,
                    lineWidth,
                    color);

                previous = current;
            }
        }

        private Vector2 CalculatePoint(
            Rect rect,
            float centerY,
            float lineAmplitude,
            float frequencyOffset,
            int lineIndex,
            int index)
        {
            float normalized =
                index /
                (float)(points - 1);

            float x =
                Mathf.Lerp(
                    rect.xMin,
                    rect.xMax,
                    normalized);

            float phase =
                normalized *
                Mathf.PI *
                2f *
                (frequency +
                 frequencyOffset);

            float movement =
                time *
                (2.5f +
                 amplitude *
                 3f);

            float wave =
                Mathf.Sin(
                    phase +
                    movement);

            float harmonic =
                Mathf.Sin(
                    phase * 2.15f -
                    movement * 1.35f +
                    lineIndex) *
                0.35f;

            float detail =
                Mathf.Sin(
                    phase * 4.2f +
                    movement * 0.7f +
                    lineIndex * 1.8f) *
                0.15f;

            float distortionWave =
                Mathf.Sin(
                    normalized *
                    Mathf.PI *
                    14f +
                    time *
                    1.8f +
                    lineIndex) *
                distortion *
                0.18f;

            float voiceWave =
                wave +
                harmonic +
                detail +
                distortionWave;

            float pitchOffset =
                pitch *
                rect.height *
                0.10f *
                pitchStrength;

            float y =
                centerY +
                pitchOffset +
                voiceWave *
                lineAmplitude;

            float topLimit =
                rect.yMax -
                lineWidth;

            float bottomLimit =
                rect.yMin +
                lineWidth;

            y =
                Mathf.Clamp(
                    y,
                    bottomLimit,
                    topLimit);

            return new Vector2(
                x,
                y);
        }

        private Color CalculateColor(
            float normalized)
        {
            if (normalized < 0.5f)
            {
                return Color.Lerp(
                    startColor,
                    middleColor,
                    normalized * 2f);
            }

            return Color.Lerp(
                middleColor,
                endColor,
                (normalized - 0.5f) * 2f);
        }

        private void AddLine(
            VertexHelper vh,
            Vector2 start,
            Vector2 end,
            float thickness,
            Color lineColor)
        {
            Vector2 direction =
                end - start;

            float length =
                direction.magnitude;

            if (length < 0.0001f)
            {
                return;
            }

            direction /= length;

            Vector2 normal =
                new Vector2(
                    -direction.y,
                    direction.x) *
                (thickness * 0.5f);

            int index =
                vh.currentVertCount;

            UIVertex vertex =
                UIVertex.simpleVert;

            vertex.color =
                lineColor;

            vertex.position =
                start + normal;

            vh.AddVert(vertex);

            vertex.position =
                start - normal;

            vh.AddVert(vertex);

            vertex.position =
                end - normal;

            vh.AddVert(vertex);

            vertex.position =
                end + normal;

            vh.AddVert(vertex);

            vh.AddTriangle(
                index,
                index + 1,
                index + 2);

            vh.AddTriangle(
                index + 2,
                index + 3,
                index);
        }
    }
}