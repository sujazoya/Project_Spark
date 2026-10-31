using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Saves a bounded study NOTEBOOK from public events, without modifying the
    /// circuit, changing official progress, replaying lessons or restoring solver data.
    /// Stored answers and bookmarks are historical context, NEVER live evidence.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAITeachingSessionSaveResume : MonoBehaviour
    {
        [Serializable]
        private sealed class StudyEvent
        {
            public string utc;
            public string description;
        }

        [Serializable]
        private sealed class StudySession
        {
            public string sessionId;
            public string startedUtc;
            public string updatedUtc;
            public string levelId;
            public string levelName;
            public string circuitId;
            public string lastQuestion;
            public string lastAnswer;
            public int lastStepIndex = -1;
            public int lastStepCount;
            public string lessonView;
            public string lastObservedEvaluatorProgress;
            public List<StudyEvent> events = new List<StudyEvent>();
        }

        [Serializable]
        private sealed class StudyStore
        {
            public int schemaVersion = 1;
            public StudySession active;
            public StudySession previous;
            public List<StudySession> archive = new List<StudySession>();
        }

        [Header("Existing Project Spark Systems")]
        // Optional companion. No compile-time dependency on the earlier package.
        [SerializeField, Tooltip("Optional: AI Teaching Session Controller. Notebook works without it.")]
        private MonoBehaviour sessionController;
        [SerializeField] private SparkAIUnifiedAssistant assistant;
        [SerializeField] private SparkVisualTeachingManager teaching;
        [SerializeField] private SparkVerifiedElectricalPublisher publisher;
        [SerializeField] private LevelGamePlayManager levelManager;

        [Header("Editor-Built Study Notebook")]
        [SerializeField] private GameObject card;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text contextText;
        [SerializeField] private TMP_Text answerText;
        [SerializeField] private TMP_Text bookmarkText;
        [SerializeField] private TMP_Text timelineText;
        [SerializeField] private TMP_Text savedSessionsText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button openSessionButton;

        [Header("Privacy and Storage")]
        [Tooltip("Optional. Turn off to save question titles only (never the text of answers).")]
        [SerializeField] private bool storeAnswerText = true;
        [SerializeField, Range(5, 50)] private int eventsPerSession = 24;
        [SerializeField, Range(1, 15)] private int archivedSessionLimit = 8;
        [SerializeField, Min(10f)] private float autoSaveSeconds = 25f;

        private const int FileVersion = 1;
        private const int MaxFileBytes = 524288;
        private const int MaxQuestionChars = 600;
        private const int MaxAnswerChars = 5000;
        private const int MaxEventChars = 180;

        private enum DisplayMode { Live, Saved, History }
        private DisplayMode displayMode = DisplayMode.Live;

        private float nextOptionalSessionSearch;
        private SparkAIUnifiedAssistant listenedAssistant;
        private SparkVisualTeachingManager listenedTeaching;
        private SparkVerifiedElectricalPublisher listenedPublisher;
        private LevelGamePlayManager listenedLevel;

        private readonly List<StudySession> archived = new List<StudySession>();
        private StudySession live;
        private StudySession previousSaved;
        private bool dirty;
        private bool storageBlocked;
        private float nextAutoSave;
        private float nextRefresh;
        private float deleteConfirmUntil;
        private string feedback = "Study notebook is ready. Old answers are never live circuit evidence.";

        private string SaveDirectory => Path.Combine(Application.persistentDataPath, "ProjectSpark", "StudySessions");
        private string SaveFile => Path.Combine(SaveDirectory, "study_sessions_v1.json");

        private void Awake()
        {
            LoadFromDisk();
            live = NewSession();
            Resolve();
            if (card != null) card.SetActive(false);
        }

        private void OnEnable()
        {
            Resolve();
            Subscribe();
            Render();
        }

        private void Start()
        {
            Resolve();
            Subscribe();
            CaptureLevelIdentity();
            nextAutoSave = Time.unscaledTime + autoSaveSeconds;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 1f;
            Resolve();
            Subscribe();
            if (dirty && Time.unscaledTime >= nextAutoSave)
                Persist();
            if (card != null && card.activeSelf)
                Render();
        }

        private void OnDisable()
        {
            Persist();
            Unsubscribe();
        }

        private void OnDestroy() => Unsubscribe();
        private void OnApplicationPause(bool paused) { if (paused) Persist(); }
        private void OnApplicationQuit() => Persist();

        private static string UtcNow() => DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        private static string Trim(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Length > max ? s.Substring(0, max) : s;
        }

        private static StudySession NewSession()
        {
            return new StudySession
            {
                sessionId = Guid.NewGuid().ToString("N"),
                startedUtc = UtcNow(),
                updatedUtc = UtcNow(),
                lastStepIndex = -1
            };
        }

        private void Resolve()
        {
            // Avoid expensive scene-wide searches every refresh when optional
            // session module is not installed.
            if (sessionController == null &&
                Time.unscaledTime >= nextOptionalSessionSearch)
            {
                nextOptionalSessionSearch = Time.unscaledTime + 5f;
                sessionController = FindOptionalSessionController();
            }
            if (assistant == null)
                assistant = FindFirstObjectByType<SparkAIUnifiedAssistant>(FindObjectsInactive.Include);
            if (teaching == null)
                teaching = FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
            if (publisher == null)
                publisher = FindFirstObjectByType<SparkVerifiedElectricalPublisher>(FindObjectsInactive.Include);
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelGamePlayManager>(FindObjectsInactive.Include);
        }

        // Resolve by exact full type name so the optional AI Session package
        // does not have to be installed for study notes to compile and work.
        private static MonoBehaviour FindOptionalSessionController()
        {
            MonoBehaviour[] components = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (MonoBehaviour component in components)
            {
                if (component != null &&
                    component.GetType().FullName ==
                    "ProjectSpark.UI.VisualTeaching.SparkAITeachingSessionController")
                {
                    return component;
                }
            }

            return null;
        }

        private void Subscribe()
        {
            if (listenedAssistant != assistant)
            {
                if (listenedAssistant != null) listenedAssistant.AnswerReady -= RecordAnswer;
                listenedAssistant = assistant;
                if (listenedAssistant != null) listenedAssistant.AnswerReady += RecordAnswer;
            }
            if (listenedTeaching != teaching)
            {
                if (listenedTeaching != null) listenedTeaching.StepChanged -= RecordStep;
                listenedTeaching = teaching;
                if (listenedTeaching != null) listenedTeaching.StepChanged += RecordStep;
            }
            if (listenedPublisher != publisher)
            {
                if (listenedPublisher != null) listenedPublisher.FramePublished -= ObserveFrame;
                listenedPublisher = publisher;
                if (listenedPublisher != null) listenedPublisher.FramePublished += ObserveFrame;
            }
            if (listenedLevel != levelManager)
            {
                if (listenedLevel != null) UnsubscribeLevel();
                listenedLevel = levelManager;
                if (listenedLevel != null)
                {
                    listenedLevel.LevelStarted += OnLevelStartOrReset;
                    listenedLevel.LevelReset += OnLevelStartOrReset;
                    listenedLevel.LevelEvaluationChanged += ObserveEvaluation;
                    listenedLevel.LevelCompleted += ObserveEvaluation;
                    listenedLevel.LevelFailed += ObserveFailure;
                }
            }
        }

        private void UnsubscribeLevel()
        {
            if (listenedLevel == null) return;
            listenedLevel.LevelStarted -= OnLevelStartOrReset;
            listenedLevel.LevelReset -= OnLevelStartOrReset;
            listenedLevel.LevelEvaluationChanged -= ObserveEvaluation;
            listenedLevel.LevelCompleted -= ObserveEvaluation;
            listenedLevel.LevelFailed -= ObserveFailure;
            listenedLevel = null;
        }

        private void Unsubscribe()
        {
            if (listenedAssistant != null) listenedAssistant.AnswerReady -= RecordAnswer;
            if (listenedTeaching != null) listenedTeaching.StepChanged -= RecordStep;
            if (listenedPublisher != null) listenedPublisher.FramePublished -= ObserveFrame;
            listenedAssistant = null;
            listenedTeaching = null;
            listenedPublisher = null;
            UnsubscribeLevel();
        }

        private void CaptureLevelIdentity()
        {
            if (live == null || levelManager == null || levelManager.ActiveLevel == null) return;
            string id = levelManager.ActiveLevel.LevelId ?? string.Empty;
            if (!string.IsNullOrEmpty(live.levelId) && !string.Equals(live.levelId, id, StringComparison.Ordinal))
            {
                ArchiveLive();
                live = NewSession();
            }
            if (live.levelId != id || live.levelName != levelManager.ActiveLevel.DisplayName)
            {
                live.levelId = id;
                live.levelName = levelManager.ActiveLevel.DisplayName;
                Touch();
            }
        }

        private void ArchiveLive()
        {
            if (live == null || live.events == null || live.events.Count == 0) return;
            archived.Insert(0, live);
            while (archived.Count > archivedSessionLimit)
                archived.RemoveAt(archived.Count - 1);
        }

        private void OnLevelStartOrReset(SparkLevelDefinition _) 
        {
            ArchiveLive();
            live = NewSession();
            CaptureLevelIdentity();
            AddEvent("Level started/reset; new study attempt");
            displayMode = DisplayMode.Live;
            Render();
        }

        private void ObserveEvaluation(SparkLevelDefinition _)
        {
            CaptureLevelIdentity();
            if (levelManager == null || levelManager.ActiveLevel == null) return;
            string value = levelManager.SatisfiedTargetCount + "/" + levelManager.ActiveTargetCount;
            if (live.lastObservedEvaluatorProgress == value) return;
            live.lastObservedEvaluatorProgress = value;
            AddEvent("Evaluator progress observed: " + value + " (not a training award)");
        }

        private void ObserveFailure(SparkLevelDefinition _, string reason)
        {
            CaptureLevelIdentity();
            AddEvent("Evaluator reported level failure" +
                     (string.IsNullOrEmpty(reason) ? "" : ": " + Trim(reason, 70)));
        }

        private void RecordAnswer(string question, string answer)
        {
            if (string.IsNullOrWhiteSpace(question)) return;
            CaptureLevelIdentity();
            live.lastQuestion = Trim(question, MaxQuestionChars);
            live.lastAnswer = storeAnswerText ? Trim(answer, MaxAnswerChars) : string.Empty;
            AddEvent("Spark AI replied to: " + Trim(question.Replace('\n', ' '), 72));
        }

        private void RecordStep(int step, int total)
        {
            CaptureLevelIdentity();
            live.lastStepIndex = step;
            live.lastStepCount = total;
            live.lessonView = teaching != null ? teaching.View.ToString() : string.Empty;
            AddEvent("Visual teaching step " + (step + 1) + "/" + total);
        }

        private void ObserveFrame(SparkVerifiedElectricalFrame frame)
        {
            if (frame == null) return;
            CaptureLevelIdentity();
            string id = frame.circuitId ?? string.Empty;
            if (!string.Equals(live.circuitId, id, StringComparison.Ordinal))
            {
                live.circuitId = Trim(id, 80);
                AddEvent("Verified circuit identity observed (no readings saved)");
            }
        }

        private void Touch()
        {
            if (live == null) return;
            live.updatedUtc = UtcNow();
            dirty = true;
        }

        private void AddEvent(string description)
        {
            if (live == null) live = NewSession();
            if (live.events == null) live.events = new List<StudyEvent>();
            live.events.Insert(0, new StudyEvent { utc = UtcNow(), description = Trim(description, MaxEventChars) });
            while (live.events.Count > eventsPerSession)
                live.events.RemoveAt(live.events.Count - 1);
            Touch();
            if (card != null && card.activeSelf) Render();
        }

        private bool ValidSession(StudySession s) =>
            s != null && !string.IsNullOrWhiteSpace(s.sessionId) &&
            s.events != null && s.events.Count <= 50;

        private StudyStore TryRead(string path)
        {
            if (!File.Exists(path)) return null;
            FileInfo info = new FileInfo(path);
            if (info.Length <= 0 || info.Length > MaxFileBytes) return null;
            StudyStore store = JsonUtility.FromJson<StudyStore>(File.ReadAllText(path, Encoding.UTF8));
            if (store == null || store.schemaVersion != FileVersion || store.archive == null)
                return null;
            return store;
        }

        private void LoadFromDisk()
        {
            try
            {
                string file = SaveFile;
                bool exists = File.Exists(file);
                StudyStore store = TryRead(file);
                if (store == null && (exists || File.Exists(file + ".bak")))
                {
                    store = TryRead(file + ".bak");
                    if (store == null)
                    {
                        storageBlocked = true; // never overwrite an unreadable / future-version save
                        feedback = "Save could not be read. Existing file preserved; saving disabled.";
                        Debug.LogWarning("[Spark AI Notebook] Existing save unreadable; no overwrite: " + file, this);
                        return;
                    }
                    feedback = "Recovered previous study snapshot from backup.";
                }
                if (store == null) return;

                // Prefer a meaningful active session over an empty auto-created one.
                bool activeHasStudy = ValidSession(store.active) &&
                    ((store.active.events != null && store.active.events.Count > 0) ||
                     !string.IsNullOrEmpty(store.active.lastQuestion) ||
                     store.active.lastStepCount > 0);
                previousSaved = activeHasStudy ? store.active :
                                (ValidSession(store.previous) ? store.previous :
                                 (ValidSession(store.active) ? store.active : null));
                if (store.archive != null)
                {
                    foreach (StudySession s in store.archive)
                    {
                        if (ValidSession(s) && archived.Count < archivedSessionLimit)
                            archived.Add(s);
                    }
                }
                if (ValidSession(store.previous) && (previousSaved == null ||
                    store.previous.sessionId != previousSaved.sessionId))
                {
                    archived.Insert(0, store.previous);
                    if (archived.Count > archivedSessionLimit)
                        archived.RemoveAt(archived.Count - 1);
                }
                if (ValidSession(store.active) && (previousSaved == null ||
                    store.active.sessionId != previousSaved.sessionId))
                {
                    archived.Insert(0, store.active);
                    if (archived.Count > archivedSessionLimit)
                        archived.RemoveAt(archived.Count - 1);
                }
                feedback = "Previous study notes found. Resume Notes opens a historical snapshot.";
            }
            catch (Exception ex)
            {
                storageBlocked = true;
                feedback = "Could not read saved notes; existing file preserved.";
                Debug.LogWarning("[Spark AI Notebook] Load failed: " + ex.Message, this);
            }
        }

        private void Persist()
        {
            if (!dirty || storageBlocked || live == null) return;
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                StudyStore store = new StudyStore
                {
                    active = live,
                    previous = previousSaved,
                    archive = new List<StudySession>(archived)
                };
                string json = JsonUtility.ToJson(store, true);
                if (Encoding.UTF8.GetByteCount(json) > MaxFileBytes)
                {
                    feedback = "Notebook reached size limit; changes were not saved.";
                    return;
                }
                string path = SaveFile;
                string temp = path + ".tmp";
                File.WriteAllText(temp, json, Encoding.UTF8);
                if (File.Exists(path))
                {
                    // Keep the prior valid snapshot before replacing the active file.
                    // If the active file was corrupt but its backup loaded, do not
                    // overwrite that last valid backup with corrupt bytes.
                    if (TryRead(path) != null)
                        File.Copy(path, path + ".bak", true);
                    File.Delete(path);
                }
                File.Move(temp, path);
                dirty = false;
                nextAutoSave = Time.unscaledTime + autoSaveSeconds;
                feedback = "Study notes saved locally. Playback and circuit are unchanged.";
            }
            catch (Exception ex)
            {
                feedback = "Saving failed: " + ex.Message;
                Debug.LogWarning("[Spark AI Notebook] " + feedback, this);
            }
        }

        [ContextMenu("Spark AI Notebook / Save Now")]
        public void SaveNow()
        {
            CaptureLevelIdentity();
            Touch();
            Persist();
            Render();
        }

        public void ResumeNotes()
        {
            if (previousSaved == null)
            {
                feedback = "No saved notes from a previous launch were found.";
                Render();
                return;
            }
            displayMode = DisplayMode.Saved;
            feedback = "Historical notes loaded for review only. No old solver values, tutorial step or game progress restored.";
            Render();
        }

        public void LiveNotes()
        {
            displayMode = DisplayMode.Live;
            feedback = "Showing this launch's live study notebook.";
            Render();
        }

        public void ShowHistory()
        {
            displayMode = DisplayMode.History;
            feedback = "Prior recorded sessions are displayed read-only.";
            Render();
        }

        [ContextMenu("Spark AI Notebook / Delete Saved Notes")]
        public void DeleteSavedNotes()
        {
            // Two deliberate clicks prevent accidental loss of local study notes.
            if (deleteConfirmUntil <= 0f || Time.unscaledTime > deleteConfirmUntil)
            {
                deleteConfirmUntil = Time.unscaledTime + 6f;
                feedback = "Press DELETE NOTES again within 6 seconds to confirm permanent removal.";
                Render();
                return;
            }
            deleteConfirmUntil = 0f;
            try
            {
                if (File.Exists(SaveFile)) File.Delete(SaveFile);
                if (File.Exists(SaveFile + ".bak")) File.Delete(SaveFile + ".bak");
                previousSaved = null;
                archived.Clear();
                live = NewSession();
                CaptureLevelIdentity();
                dirty = false;
                storageBlocked = false;
                displayMode = DisplayMode.Live;
                feedback = "Saved study notes deleted. Future new activity may create new notes.";
            }
            catch (Exception ex)
            {
                feedback = "Delete failed: " + ex.Message;
                Debug.LogWarning("[Spark AI Notebook] " + feedback, this);
            }
            Render();
        }

        public void OpenSession()
        {
            Resolve();
            if (sessionController == null)
            {
                Debug.LogWarning(
                    "[Spark AI Notebook] Optional Teaching Session Controller is not installed. " +
                    "Saved notes remain available.", this);
                return;
            }

            // Optional module call. Only invoke the public, parameterless Show
            // method on the resolved session component (not on other scripts).
            var show = sessionController.GetType().GetMethod(
                "Show", System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance,
                null, Type.EmptyTypes, null);

            if (show == null || show.ReturnType != typeof(void))
            {
                Debug.LogWarning(
                    "[Spark AI Notebook] Optional session component has no public Show().", this);
                return;
            }

            try
            {
                show.Invoke(sessionController, null);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, sessionController);
            }
        }

        public void Show()
        {
            Resolve();
            Subscribe();
            if (card == null) { Debug.LogWarning("[Spark AI Notebook] UI not assigned.", this); return; }
            card.SetActive(true);
            Render();
        }

        public void Hide() { if (card != null) card.SetActive(false); }
        public void Toggle() { if (card != null && card.activeSelf) Hide(); else Show(); }

        private void Render()
        {
            if (live == null) return;
            StudySession data = displayMode == DisplayMode.Live ? live : previousSaved;
            if (data == null) data = live;
            bool historical = displayMode != DisplayMode.Live;
            bool sameLevel = levelManager != null && levelManager.ActiveLevel != null &&
                !string.IsNullOrWhiteSpace(data.levelId) &&
                string.Equals(levelManager.ActiveLevel.LevelId, data.levelId, StringComparison.Ordinal);

            if (titleText != null)
                titleText.text = displayMode == DisplayMode.Live ? "LIVE STUDY NOTEBOOK" :
                    displayMode == DisplayMode.Saved ? "PREVIOUS SESSION / HISTORICAL" : "STUDY HISTORY";
            if (contextText != null)
                contextText.text = "RECORDED LEVEL  " + (string.IsNullOrWhiteSpace(data.levelName) ? "NOT SET" : Trim(data.levelName, 72)) +
                    "\nLEVEL ID  " + (data.levelId ?? "") +
                    "\nRECORDED CIRCUIT ID  " + (string.IsNullOrWhiteSpace(data.circuitId) ? "NONE" : data.circuitId) +
                    "\n" + (historical ? (sameLevel ? "LEVEL ID MATCHES; FACTS NOT REVALIDATED" : "HISTORICAL — LEVEL NOT CONFIRMED") : "CURRENT PLAY SESSION");
            if (answerText != null)
                answerText.text = string.IsNullOrWhiteSpace(data.lastQuestion)
                    ? "No Spark AI answer recorded in this session."
                    : "Q: " + Trim(data.lastQuestion, 220) + "\n\n" +
                      (string.IsNullOrEmpty(data.lastAnswer) ? "(Answer text not stored.)" : Trim(data.lastAnswer, 600)) +
                      "\n\n[" + (historical ? "HISTORICAL" : "ADVISORY") + " AI TEXT — NOT CURRENT ELECTRICAL EVIDENCE]";
            if (bookmarkText != null)
                bookmarkText.text = "LAST OBSERVED STEP  " + (data.lastStepCount > 0 ?
                    (data.lastStepIndex + 1) + "/" + data.lastStepCount : "NOT RECORDED") +
                    "  •  VIEW " + (string.IsNullOrWhiteSpace(data.lessonView) ? "UNKNOWN" : data.lessonView) +
                    "\nEVALUATOR PROGRESS AT OBSERVATION  " +
                    (string.IsNullOrWhiteSpace(data.lastObservedEvaluatorProgress) ? "NOT RECORDED" : data.lastObservedEvaluatorProgress) +
                    "  (historic only; current evaluator controls gameplay)";
            if (timelineText != null)
            {
                StringBuilder sb = new StringBuilder();
                if (displayMode == DisplayMode.History)
                {
                    if (previousSaved != null)
                        sb.AppendLine("LAST LAUNCH  " + Trim(previousSaved.levelName, 42) +
                            "  " + Trim(previousSaved.updatedUtc, 10));
                    int historyCount = Math.Min(5, archived.Count);
                    for (int i = 0; i < historyCount; i++)
                    {
                        StudySession h = archived[i];
                        if (h != null)
                            sb.AppendLine("ARCHIVED  " + Trim(h.levelName, 42) +
                                "  " + Trim(h.updatedUtc, 10));
                    }
                    if (sb.Length == 0) sb.Append("No earlier study attempts recorded.");
                }
                else if (data.events != null)
                {
                    int count = Math.Min(5, data.events.Count);
                    for (int i = 0; i < count; i++)
                    {
                        StudyEvent item = data.events[i];
                        if (item == null) continue;
                        sb.AppendLine(Trim(item.utc, 19) + "  " + Trim(item.description, 85));
                    }
                }
                timelineText.text = sb.Length == 0 ? "No recorded events." : sb.ToString().TrimEnd();
            }
            if (savedSessionsText != null)
            {
                string savedName = previousSaved != null
                    ? Trim(previousSaved.levelName, 66) : "NONE";
                savedSessionsText.text = "PREVIOUS LAUNCH  " + savedName +
                    "\nARCHIVED ATTEMPTS  " + archived.Count +
                    "  •  HISTORY lists them without modifying gameplay";
            }
            if (statusText != null)
                statusText.text = (storageBlocked ? "READ-ONLY SAVE ERROR  •  " : "") + feedback;
            if (resumeButton != null) resumeButton.interactable = previousSaved != null;
            if (openSessionButton != null) openSessionButton.interactable = sessionController != null;
        }
    }
}
