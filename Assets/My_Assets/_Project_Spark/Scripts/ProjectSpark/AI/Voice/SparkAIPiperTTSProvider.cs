using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSpark.AI
{
    /// <summary>
    /// Local/offline neural TTS provider using Piper.
    ///
    /// Text flow:
    ///
    /// Spark AI
    ///     ↓
    /// SparkAIVoiceRequest
    ///     ↓
    /// Piper
    ///     ↓
    /// Generated WAV
    ///     ↓
    /// Unity AudioClip
    ///     ↓
    /// AudioSource
    ///
    /// No prerecorded dialogue is required.
    ///
    /// Piper itself must be installed locally and the selected
    /// ONNX voice model must exist on the machine.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAIPiperTTSProvider
        : MonoBehaviour,
          ISparkAITTSProvider
    {
        [Header("Piper")]
        [Tooltip(
            "Full path to the Piper executable.")]
        [SerializeField]
        private string piperExecutablePath =
            "piper.exe";

        [Tooltip(
            "Full path to the Piper .onnx voice model.")]
        [SerializeField]
        private string modelPath =
            string.Empty;

        [Header("Generated Audio")]
        [Tooltip(
            "Directory where generated speech WAV files are stored.")]
        [SerializeField]
        private string outputDirectory =
            "ProjectSparkVoice";

        [SerializeField]
        private bool cacheGeneratedAudio = true;

        [Header("Playback")]
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        [Range(0f, 1f)]
        private float volume = 1f;

        [SerializeField]
        private bool interruptCurrentSpeech = true;

        [Header("Piper")]
        [Tooltip(
            "Controls speaking rate. 1 = normal. " +
            "Higher values are slower.")]
        [SerializeField]
        [Range(0.5f, 2f)]
        private float defaultLengthScale = 1f;

        [SerializeField]
        private bool useRequestSpeed = true;

        [Header("Debug")]
        [SerializeField]
        private bool logPiperProcess;

        private Process activeProcess;

        private Coroutine playbackRoutine;

        private bool initialized;

        private string currentCacheKey =
            string.Empty;

        private AudioClip currentClip;

        public bool IsAvailable
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return initialized &&
                       File.Exists(
                           ResolvePiperExecutable()) &&
                       File.Exists(
                           ResolveModelPath());
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Stop();

            if (currentClip != null)
            {
                Destroy(currentClip);
                currentClip = null;
            }
        }

        // =========================================================
        // INITIALIZATION
        // =========================================================

        private void Initialize()
        {
            if (initialized)
                return;

            if (audioSource == null)
            {
                audioSource =
                    GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource =
                    gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume =
                Mathf.Clamp01(volume);

            CreateOutputDirectory();

            initialized = true;

            if (!IsAvailable)
            {
                UnityEngine.Debug.LogWarning(
                    "[AI PIPER] Piper is not configured correctly.",
                    this);
            }
        }

        // =========================================================
        // PUBLIC PROVIDER API
        // =========================================================

        public bool Speak(
            SparkAIVoiceRequest request)
        {
            if (!request.IsValid)
                return false;

            if (!IsAvailable)
            {
                UnityEngine.Debug.LogWarning(
                    "[AI PIPER] Provider is unavailable.",
                    this);

                return false;
            }

            if (interruptCurrentSpeech)
            {
                Stop();
            }

            if (playbackRoutine != null)
            {
                StopCoroutine(
                    playbackRoutine);

                playbackRoutine = null;
            }

            playbackRoutine =
                StartCoroutine(
                    GenerateAndPlay(
                        request));

            return true;
        }

        public void Stop()
        {
            if (playbackRoutine != null)
            {
                StopCoroutine(
                    playbackRoutine);

                playbackRoutine = null;
            }

            StopActiveProcess();

            if (audioSource != null &&
                audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        public void Shutdown()
        {
            Stop();
        }

        // =========================================================
        // GENERATION
        // =========================================================

        private IEnumerator GenerateAndPlay(
            SparkAIVoiceRequest request)
        {
            string cacheKey =
                BuildCacheKey(
                    request);

            string wavPath =
                Path.Combine(
                    GetOutputDirectory(),
                    cacheKey + ".wav");

            if (cacheGeneratedAudio &&
                File.Exists(wavPath))
            {
                if (logPiperProcess)
                {
                    UnityEngine.Debug.Log(
                        $"[AI PIPER] Using cached voice: " +
                        $"{wavPath}",
                        this);
                }
            }
            else
            {
                Task<bool> generationTask =
                    GenerateSpeechAsync(
                        request,
                        wavPath);

                while (!generationTask.IsCompleted)
                {
                    yield return null;
                }

                if (generationTask.IsFaulted)
                {
                    UnityEngine.Debug.LogError(
                        "[AI PIPER] Speech generation failed.\n" +
                        generationTask.Exception,
                        this);

                    playbackRoutine = null;
                    yield break;
                }

                if (!generationTask.Result)
                {
                    playbackRoutine = null;
                    yield break;
                }
            }

            if (!File.Exists(wavPath))
            {
                UnityEngine.Debug.LogError(
                    $"[AI PIPER] Generated WAV does not exist: " +
                    $"{wavPath}",
                    this);

                playbackRoutine = null;
                yield break;
            }

            UnityWebRequest requestAudio =
                UnityWebRequestMultimedia.GetAudioClip(
                    new Uri(wavPath).AbsoluteUri,
                    AudioType.WAV);

            yield return requestAudio.SendWebRequest();

            if (requestAudio.result !=
                UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError(
                    $"[AI PIPER] Could not load generated WAV: " +
                    $"{requestAudio.error}",
                    this);

                playbackRoutine = null;
                yield break;
            }

            AudioClip clip =
                DownloadHandlerAudioClip
                    .GetContent(
                        requestAudio);

            if (clip == null)
            {
                UnityEngine.Debug.LogError(
                    "[AI PIPER] Unity returned a null AudioClip.",
                    this);

                playbackRoutine = null;
                yield break;
            }

            if (audioSource == null)
            {
                playbackRoutine = null;
                yield break;
            }

            if (currentClip != null)
            {
                Destroy(currentClip);
            }

            currentClip =
                clip;

            audioSource.clip =
                currentClip;

            audioSource.volume =
                Mathf.Clamp01(volume);

            /*
             * Piper does not expose a universal pitch parameter
             * through the standard CLI.
             *
             * The request pitch is therefore mapped to Unity
             * playback pitch as a presentation effect.
             */
            audioSource.pitch =
                ConvertPitch(
                    request.Pitch);

            audioSource.Play();

            playbackRoutine = null;
        }

        // =========================================================
        // PIPER PROCESS
        // =========================================================

private async Task<bool> GenerateSpeechAsync(
    SparkAIVoiceRequest request,
    string outputPath)
{
    if (string.IsNullOrWhiteSpace(piperExecutablePath))
    {
        UnityEngine.Debug.LogError(
            "[AI PIPER] Piper executable path is empty.",
            this);

        return false;
    }

    if (string.IsNullOrWhiteSpace(modelPath))
    {
        UnityEngine.Debug.LogError(
            "[AI PIPER] Model path is empty.",
            this);

        return false;
    }

    string executablePath =
        ResolveStreamingAssetsPath(
            piperExecutablePath);

    string resolvedModelPath =
        ResolveStreamingAssetsPath(
            modelPath);

    if (!File.Exists(executablePath))
    {
        UnityEngine.Debug.LogError(
            $"[AI PIPER] Piper executable not found:\n{executablePath}",
            this);

        return false;
    }

    if (!File.Exists(resolvedModelPath))
    {
        UnityEngine.Debug.LogError(
            $"[AI PIPER] Piper model not found:\n{resolvedModelPath}",
            this);

        return false;
    }

    string outputDirectory =
        Path.GetDirectoryName(outputPath);

    if (string.IsNullOrEmpty(outputDirectory))
    {
       UnityEngine.Debug.LogError(
            $"[AI PIPER] Invalid output path:\n{outputPath}",
            this);

        return false;
    }

    Directory.CreateDirectory(outputDirectory);

    string executableDirectory =
        Path.GetDirectoryName(executablePath);

    if (string.IsNullOrEmpty(executableDirectory))
        executableDirectory =
            Application.streamingAssetsPath;

    float lengthScale =
        ConvertSpeedToLengthScale(
            request.Speed);

    string invariantLengthScale =
    lengthScale.ToString(
        System.Globalization.CultureInfo.InvariantCulture);

string arguments =
    $"--model \"{resolvedModelPath}\" " +
    $"--output_file \"{outputPath}\" " +
    $"--length_scale {invariantLengthScale}";

    ProcessStartInfo startInfo =
        new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = arguments,

            WorkingDirectory = executableDirectory,

            UseShellExecute = false,

            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

            CreateNoWindow = true,

            StandardInputEncoding =
                System.Text.Encoding.UTF8,

            StandardOutputEncoding =
                System.Text.Encoding.UTF8,

            StandardErrorEncoding =
                System.Text.Encoding.UTF8
        };

    Process process = null;

    try
    {
        process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = false
        };

        if (logPiperProcess)
        {
            UnityEngine.Debug.Log(
                "[AI PIPER] Starting Piper:\n" +
                $"Executable: {executablePath}\n" +
                $"Working Directory: {executableDirectory}\n" +
                $"Model: {resolvedModelPath}\n" +
                $"Output: {outputPath}",
                this);
        }

        if (!process.Start())
        {
            UnityEngine.Debug.LogError(
                "[AI PIPER] Failed to start Piper.",
                this);

            return false;
        }

        activeProcess = process;

        /*
         * IMPORTANT:
         *
         * Write the complete sentence synchronously and close stdin
         * immediately. Piper reads until stdin is closed.
         *
         * This avoids the broken-pipe problem caused by asynchronous
         * StreamWriter.WriteAsync() racing against Piper startup/exit.
         */
        try
        {
            using (StreamWriter writer = process.StandardInput)
            {
                writer.Write(request.Text);
                writer.Flush();
            }
        }
        catch (IOException ioException)
        {
            string errorOutput = string.Empty;

            try
            {
                errorOutput =
                    process.StandardError.ReadToEnd();
            }
            catch
            {
                // Ignore secondary pipe errors.
            }

            UnityEngine.Debug.LogError(
                "[AI PIPER] Piper closed stdin before Unity finished " +
                "writing the request.\n\n" +
                $"Text: {request.Text}\n" +
                $"Exit Code: {process.ExitCode}\n" +
                $"Piper Error:\n{errorOutput}\n\n" +
                $"IO Error:\n{ioException}",
                this);

            return false;
        }

        /*
         * Read Piper's output after stdin has been closed.
         */
        Task<string> standardErrorTask =
            process.StandardError.ReadToEndAsync();

        Task<string> standardOutputTask =
            process.StandardOutput.ReadToEndAsync();

        await Task.Run(
            () => process.WaitForExit());

        string standardError =
            await standardErrorTask;

        string standardOutput =
            await standardOutputTask;

        int exitCode =
            process.ExitCode;

        if (logPiperProcess)
        {
            if (!string.IsNullOrWhiteSpace(standardOutput))
            {
                UnityEngine.Debug.Log(
                    $"[AI PIPER] stdout:\n{standardOutput}",
                    this);
            }

            if (!string.IsNullOrWhiteSpace(standardError))
            {
                UnityEngine.Debug.Log(
                    $"[AI PIPER] stderr:\n{standardError}",
                    this);
            }

            UnityEngine.Debug.Log(
                $"[AI PIPER] Process exited with code {exitCode}.",
                this);
        }

        if (exitCode != 0)
        {
            UnityEngine.Debug.LogError(
                "[AI PIPER] Piper process failed.\n" +
                $"Exit Code: {exitCode}\n" +
                $"Error: {standardError}",
                this);

            return false;
        }

        if (!File.Exists(outputPath))
        {
            UnityEngine.Debug.LogError(
                "[AI PIPER] Piper finished successfully, " +
                "but the WAV file was not created.\n" +
                $"Expected: {outputPath}",
                this);

            return false;
        }

        FileInfo outputFile =
            new FileInfo(outputPath);

        if (outputFile.Length <= 44)
        {
            UnityEngine.Debug.LogError(
                "[AI PIPER] Generated WAV file appears to be empty:\n" +
                outputPath,
                this);

            return false;
        }

        return true;
    }
    catch (Exception exception)
    {
            UnityEngine.Debug.LogError(
            $"[AI PIPER] Process error:\n{exception}",
            this);

        return false;
    }
    finally
    {
        if (process != null)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill();
            }
            catch
            {
                // Process already exited.
            }

            try
            {
                process.Dispose();
            }
            catch
            {
                // Ignore disposal errors.
            }
        }

        if (ReferenceEquals(activeProcess, process))
            activeProcess = null;
    }
}

        private string ResolveStreamingAssetsPath(
    string relativePath)
{
    if (string.IsNullOrWhiteSpace(relativePath))
        return string.Empty;

    if (Path.IsPathRooted(relativePath))
        return Path.GetFullPath(relativePath);

    string normalized =
        relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

    return Path.GetFullPath(
        Path.Combine(
            Application.streamingAssetsPath,
            normalized));
}

        // =========================================================
        // PIPER ARGUMENTS
        // =========================================================

        private string BuildPiperArguments(
            string model,
            string outputPath,
            float lengthScale)
        {
            return
                "--model " +
                QuoteArgument(model) +
                " " +
                "--output_file " +
                QuoteArgument(outputPath) +
                " " +
                "--length_scale " +
                lengthScale.ToString(
                    System.Globalization.CultureInfo.InvariantCulture);
        }

        // =========================================================
        // PATHS
        // =========================================================

        private string ResolvePiperExecutable()
        {
            if (Path.IsPathRooted(
                    piperExecutablePath))
            {
                return piperExecutablePath;
            }

            return Path.Combine(
                Application.streamingAssetsPath,
                piperExecutablePath);
        }

        private string ResolveModelPath()
        {
            if (Path.IsPathRooted(
                    modelPath))
            {
                return modelPath;
            }

            return Path.Combine(
                Application.streamingAssetsPath,
                modelPath);
        }

        private string GetOutputDirectory()
        {
            string directory =
                Path.Combine(
                    Application.persistentDataPath,
                    outputDirectory);

            Directory.CreateDirectory(
                directory);

            return directory;
        }

        private void CreateOutputDirectory()
        {
            Directory.CreateDirectory(
                GetOutputDirectory());
        }

        // =========================================================
        // CACHE
        // =========================================================

        private string BuildCacheKey(
            SparkAIVoiceRequest request)
        {
            string raw =
                request.ConceptId +
                "|" +
                request.TextType +
                "|" +
                request.Text +
                "|" +
                request.Speed.ToString(
                    System.Globalization.CultureInfo.InvariantCulture) +
                "|" +
                request.Pitch.ToString(
                    System.Globalization.CultureInfo.InvariantCulture);

            return
                ComputeStableHash(raw);
        }

        private string ComputeStableHash(
            string value)
        {
            unchecked
            {
                int hash =
                    23;

                for (int i = 0;
                     i < value.Length;
                     i++)
                {
                    hash =
                        hash * 31 +
                        value[i];
                }

                return
                    hash.ToString(
                        "X8");
            }
        }

        // =========================================================
        // CONVERSION
        // =========================================================

        private float ConvertSpeedToLengthScale(
            float speed)
        {
            speed =
                Mathf.Clamp(
                    speed,
                    0.5f,
                    2f);

            /*
             * Spark AI:
             *
             * speed 0.5 = slow
             * speed 1.0 = normal
             * speed 2.0 = fast
             *
             * Piper:
             *
             * length_scale 2.0 = slower
             * length_scale 1.0 = normal
             * length_scale 0.5 = faster
             */

            return
                Mathf.Clamp(
                    1f / speed,
                    0.5f,
                    2f);
        }

        private float ConvertPitch(
            float normalizedPitch)
        {
            /*
             * 0.5 = neutral.
             *
             * Keep the adjustment subtle so the AI does not
             * sound artificially distorted.
             */

            normalizedPitch =
                Mathf.Clamp01(
                    normalizedPitch);

            return
                Mathf.Lerp(
                    0.85f,
                    1.15f,
                    normalizedPitch);
        }

        // =========================================================
        // PROCESS CLEANUP
        // =========================================================

        private void StopActiveProcess()
        {
            Process process =
                activeProcess;

            if (process == null)
                return;

            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            {
                // Process may already have exited.
            }

            activeProcess =
                null;
        }

        // =========================================================
        // STRING HELPERS
        // =========================================================

        private string QuoteArgument(
            string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            return
                "\"" +
                value.Replace(
                    "\"",
                    "\\\"") +
                "\"";
        }
    }
}