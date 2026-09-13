using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.UI.Animation
{
    /// <summary>
    /// Production notification manager for Project Spark.
    ///
    /// Responsibilities:
    /// - Notification pooling.
    /// - Notification queueing.
    /// - Active notification management.
    /// - Notification content assignment.
    /// - Notification type assignment.
    /// - Lifetime configuration.
    /// - Automatic pool return.
    /// - Runtime notification lookup.
    /// - Pause / resume.
    /// - Dismissal.
    ///
    /// UINotificationAnimator owns the visual animation.
    /// UINotificationManager owns notification lifecycle and pooling.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UINotificationManager : MonoBehaviour
    {
        // ============================================================
        // SINGLETON
        // ============================================================

        public static UINotificationManager Instance
        {
            get;
            private set;
        }

        // ============================================================
        // SERIALIZABLE DATA
        // ============================================================

        [Serializable]
        public struct NotificationRequest
        {
            public string Id;
            public string Title;
            public string Message;

            public UINotificationAnimator.NotificationType Type;

            public float Duration;

            public bool Immediate;

            public NotificationRequest(
                string id,
                string title,
                string message,
                UINotificationAnimator.NotificationType type,
                float duration,
                bool immediate)
            {
                Id = id;
                Title = title;
                Message = message;
                Type = type;
                Duration = duration;
                Immediate = immediate;
            }
        }

        // ============================================================
        // CONFIGURATION
        // ============================================================

        [Header("POOL")]

        [SerializeField]
        private UINotificationAnimator notificationPrefab;

        [SerializeField]
        private RectTransform notificationContainer;

        [SerializeField]
        [Min(1)]
        private int poolSize = 6;

        [SerializeField]
        [Min(1)]
        private int maxActiveNotifications = 4;

        [SerializeField]
        private bool prewarmOnAwake = true;

        [SerializeField]
        private bool allowPoolExpansion = true;

        [SerializeField]
        [Min(1)]
        private int maximumPoolSize = 12;

        [Header("QUEUE")]

        [SerializeField]
        private bool useQueue = true;

        [SerializeField]
        [Min(1)]
        private int maximumQueueSize = 16;

        [SerializeField]
        private bool dropOldestWhenQueueIsFull;

        [Header("DEFAULTS")]

        [SerializeField]
        [Min(0.05f)]
        private float defaultDuration = 3f;

        [SerializeField]
        private bool defaultImmediate;

        [Header("LIFETIME")]

        [SerializeField]
        private bool useUnscaledTime = true;

        [Header("PERSISTENCE")]

        [SerializeField]
        private bool dontDestroyOnLoad;

        [Header("DEBUG")]

        [SerializeField]
        private bool logWarnings = true;

        // ============================================================
        // POOLS
        // ============================================================

        private readonly List<UINotificationAnimator> availableNotifications =
            new();

        private readonly List<UINotificationAnimator> activeNotifications =
            new();

        private readonly Queue<NotificationRequest> queuedNotifications =
            new();

        private readonly Dictionary<string, UINotificationAnimator>
            activeById = new();

        // ============================================================
        // STATE
        // ============================================================

        private int generatedNotificationId;

        private bool initialized;

        // ============================================================
        // PUBLIC PROPERTIES
        // ============================================================

        public int QueuedCount =>
            queuedNotifications.Count;

        public int ActiveCount =>
            activeNotifications.Count;

        public int AvailablePoolCount =>
            availableNotifications.Count;

        public int PoolCapacity =>
            availableNotifications.Count +
            activeNotifications.Count;

        public int MaximumPoolSize =>
            maximumPoolSize;

        public int MaximumActiveNotifications =>
            maxActiveNotifications;

        public bool IsInitialized =>
            initialized;

        // ============================================================
        // UNITY
        // ============================================================

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            ClearRuntimeState();
        }

        // ============================================================
        // INITIALIZATION
        // ============================================================

        private void Initialize()
        {
            if (initialized)
                return;

            ValidateConfiguration();

            if (prewarmOnAwake)
                Prewarm();

            initialized = true;
        }

        private void ValidateConfiguration()
        {
            if (notificationPrefab == null)
            {
                LogWarning(
                    "Notification prefab is not assigned.");

                return;
            }

            if (notificationContainer == null)
            {
                LogWarning(
                    "Notification container is not assigned.");
            }

            poolSize =
                Mathf.Max(
                    1,
                    poolSize);

            maxActiveNotifications =
                Mathf.Clamp(
                    maxActiveNotifications,
                    1,
                    maximumPoolSize);

            maximumPoolSize =
                Mathf.Max(
                    poolSize,
                    maximumPoolSize);

            maximumQueueSize =
                Mathf.Max(
                    1,
                    maximumQueueSize);

            defaultDuration =
                Mathf.Max(
                    0.05f,
                    defaultDuration);
        }

        // ============================================================
        // PREWARM
        // ============================================================

        public void Prewarm()
        {
            if (notificationPrefab == null)
                return;

            int targetCount =
                Mathf.Min(
                    poolSize,
                    maximumPoolSize);

            while (PoolCapacity < targetCount)
            {
                if (!CreatePoolInstance())
                    break;
            }
        }

        // ============================================================
        // PUBLIC SHOW API
        // ============================================================

        public UINotificationAnimator Show(
            string title,
            string message,
            UINotificationAnimator.NotificationType type)
        {
            return Show(
                title,
                message,
                type,
                defaultDuration,
                defaultImmediate);
        }

        public UINotificationAnimator Show(
            string title,
            string message,
            UINotificationAnimator.NotificationType type,
            float duration)
        {
            return Show(
                title,
                message,
                type,
                duration,
                defaultImmediate);
        }

        public UINotificationAnimator Show(
            string title,
            string message,
            UINotificationAnimator.NotificationType type,
            float duration,
            bool immediate)
        {
            NotificationRequest request =
                new NotificationRequest(
                    GenerateId(),
                    title,
                    message,
                    type,
                    duration,
                    immediate);

            return Show(request);
        }

        public UINotificationAnimator Show(
            NotificationRequest request)
        {
            EnsureInitialized();

            NormalizeRequest(
                ref request);

            if (CanDisplayImmediately())
            {
                return Display(request);
            }

            if (!useQueue)
            {
                return null;
            }

            Enqueue(request);

            return null;
        }

        // ============================================================
        // QUEUE
        // ============================================================

        private void Enqueue(
            NotificationRequest request)
        {
            if (queuedNotifications.Count >= maximumQueueSize)
            {
                if (dropOldestWhenQueueIsFull)
                {
                    queuedNotifications.Dequeue();
                }
                else
                {
                    LogWarning(
                        "Notification queue is full. " +
                        "New notification was dropped.");

                    return;
                }
            }

            queuedNotifications.Enqueue(
                request);
        }

        private void ProcessQueue()
        {
            while (
                queuedNotifications.Count > 0 &&
                CanDisplayImmediately())
            {
                NotificationRequest request =
                    queuedNotifications.Dequeue();

                Display(request);
            }
        }

        // ============================================================
        // DISPLAY
        // ============================================================

        private UINotificationAnimator Display(
            NotificationRequest request)
        {
            UINotificationAnimator notification =
                Acquire();

            if (notification == null)
            {
                LogWarning(
                    "Unable to acquire notification from pool.");

                return null;
            }

            string id =
                string.IsNullOrWhiteSpace(request.Id)
                    ? GenerateId()
                    : request.Id;

            notification.SetNotificationId(id);

            notification.SetContent(
                request.Title,
                request.Message);

            notification.SetType(
                request.Type);

            notification.SetUseUnscaledTime(
                useUnscaledTime);

            RegisterActive(
                id,
                notification);

            notification.Hidden -=
                HandleNotificationHidden;

            notification.Hidden +=
                HandleNotificationHidden;

            notification.Show(
                request.Duration,
                request.Immediate);

            return notification;
        }

        // ============================================================
        // ACQUIRE
        // ============================================================

        private UINotificationAnimator Acquire()
        {
            if (availableNotifications.Count > 0)
            {
                int lastIndex =
                    availableNotifications.Count - 1;

                UINotificationAnimator notification =
                    availableNotifications[lastIndex];

                availableNotifications.RemoveAt(
                    lastIndex);

                if (notification != null)
                {
                    notification.gameObject.SetActive(true);
                    return notification;
                }
            }

            if (!allowPoolExpansion)
                return null;

            if (PoolCapacity >= maximumPoolSize)
                return null;

            if (!CreatePoolInstance())
                return null;

            if (availableNotifications.Count == 0)
                return null;

            int index =
                availableNotifications.Count - 1;

            UINotificationAnimator created =
                availableNotifications[index];

            availableNotifications.RemoveAt(
                index);

            if (created != null)
                created.gameObject.SetActive(true);

            return created;
        }

        // ============================================================
        // CREATE
        // ============================================================

        private bool CreatePoolInstance()
        {
            if (notificationPrefab == null)
                return false;

            if (PoolCapacity >= maximumPoolSize)
                return false;

            UINotificationAnimator instance =
                Instantiate(
                    notificationPrefab,
                    notificationContainer);

            if (instance == null)
                return false;

            instance.name =
                $"{notificationPrefab.name}_Pooled";

            instance.gameObject.SetActive(false);

            availableNotifications.Add(
                instance);

            return true;
        }

        // ============================================================
        // ACTIVE REGISTRATION
        // ============================================================

        private void RegisterActive(
            string id,
            UINotificationAnimator notification)
        {
            if (notification == null)
                return;

            activeNotifications.Add(
                notification);

            if (!string.IsNullOrWhiteSpace(id))
            {
                activeById[id] =
                    notification;
            }
        }

        private void UnregisterActive(
            UINotificationAnimator notification)
        {
            if (notification == null)
                return;

            activeNotifications.Remove(
                notification);

            string id =
                notification.NotificationId;

            if (!string.IsNullOrWhiteSpace(id))
            {
                if (activeById.TryGetValue(
                        id,
                        out UINotificationAnimator registered))
                {
                    if (registered == notification)
                    {
                        activeById.Remove(id);
                    }
                }
            }
        }

        // ============================================================
        // HIDDEN CALLBACK
        // ============================================================

        private void HandleNotificationHidden(
            UINotificationAnimator notification)
        {
            if (notification == null)
                return;

            notification.Hidden -=
                HandleNotificationHidden;

            UnregisterActive(
                notification);

            ReturnToPool(
                notification);

            ProcessQueue();
        }

        // ============================================================
        // RETURN TO POOL
        // ============================================================

        private void ReturnToPool(
            UINotificationAnimator notification)
        {
            if (notification == null)
                return;

            notification.gameObject.SetActive(false);

            notification.ResetForPool();

            if (!availableNotifications.Contains(
                    notification))
            {
                availableNotifications.Add(
                    notification);
            }
        }

        // ============================================================
        // LOOKUP
        // ============================================================

        public bool TryGet(
            string id,
            out UINotificationAnimator notification)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                notification = null;
                return false;
            }

            if (!activeById.TryGetValue(
                    id,
                    out notification))
            {
                return false;
            }

            if (notification == null)
            {
                activeById.Remove(id);
                return false;
            }

            return true;
        }

        // ============================================================
        // DISMISS
        // ============================================================

        public bool Dismiss(
            string id)
        {
            if (!TryGet(
                    id,
                    out UINotificationAnimator notification))
            {
                return false;
            }

            notification.Hide();

            return true;
        }

        public bool DismissImmediate(
            string id)
        {
            if (!TryGet(
                    id,
                    out UINotificationAnimator notification))
            {
                return false;
            }

            notification.DismissImmediate();

            return true;
        }

        public void DismissAll()
        {
            for (int i = activeNotifications.Count - 1;
                 i >= 0;
                 i--)
            {
                UINotificationAnimator notification =
                    activeNotifications[i];

                if (notification == null)
                    continue;

                notification.Hide();
            }
        }

        public void DismissAllImmediate()
        {
            for (int i = activeNotifications.Count - 1;
                 i >= 0;
                 i--)
            {
                UINotificationAnimator notification =
                    activeNotifications[i];

                if (notification == null)
                    continue;

                notification.DismissImmediate();
            }

            queuedNotifications.Clear();
        }

        // ============================================================
        // PAUSE / RESUME
        // ============================================================

        public bool Pause(
            string id)
        {
            if (!TryGet(
                    id,
                    out UINotificationAnimator notification))
            {
                return false;
            }

            notification.Pause();

            return true;
        }

        public bool Resume(
            string id)
        {
            if (!TryGet(
                    id,
                    out UINotificationAnimator notification))
            {
                return false;
            }

            notification.Resume();

            return true;
        }

        public void PauseAll()
        {
            for (int i = 0;
                 i < activeNotifications.Count;
                 i++)
            {
                UINotificationAnimator notification =
                    activeNotifications[i];

                if (notification == null)
                    continue;

                notification.Pause();
            }
        }

        public void ResumeAll()
        {
            for (int i = 0;
                 i < activeNotifications.Count;
                 i++)
            {
                UINotificationAnimator notification =
                    activeNotifications[i];

                if (notification == null)
                    continue;

                notification.Resume();
            }
        }

        // ============================================================
        // QUEUE CONTROL
        // ============================================================

        public void ClearQueue()
        {
            queuedNotifications.Clear();
        }

        public void ClearQueueAndDismissAll()
        {
            queuedNotifications.Clear();

            DismissAll();
        }

        public void ClearQueueAndDismissAllImmediate()
        {
            queuedNotifications.Clear();

            DismissAllImmediate();
        }

        // ============================================================
        // ID GENERATION
        // ============================================================

        private string GenerateId()
        {
            generatedNotificationId++;

            return
                $"notification_{generatedNotificationId:000000}";
        }

        // ============================================================
        // REQUEST NORMALIZATION
        // ============================================================

        private void NormalizeRequest(
            ref NotificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                request.Id =
                    GenerateId();
            }

            request.Title ??= string.Empty;
            request.Message ??= string.Empty;

            request.Duration =
                Mathf.Max(
                    0.05f,
                    request.Duration);
        }

        // ============================================================
        // CAPACITY
        // ============================================================

        private bool CanDisplayImmediately()
        {
            return activeNotifications.Count <
                   maxActiveNotifications;
        }

        // ============================================================
        // INITIALIZATION SAFETY
        // ============================================================

        private void EnsureInitialized()
        {
            if (initialized)
                return;

            Initialize();
        }

        // ============================================================
        // CLEAR
        // ============================================================

        private void ClearRuntimeState()
        {
            for (int i = 0;
                 i < activeNotifications.Count;
                 i++)
            {
                UINotificationAnimator notification =
                    activeNotifications[i];

                if (notification == null)
                    continue;

                notification.Hidden -=
                    HandleNotificationHidden;
            }

            activeNotifications.Clear();
            availableNotifications.Clear();
            queuedNotifications.Clear();
            activeById.Clear();
        }

        // ============================================================
        // DEBUG
        // ============================================================

        private void LogWarning(
            string message)
        {
            if (!logWarnings)
                return;

            Debug.LogWarning(
                $"[Project Spark] {message}",
                this);
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            poolSize =
                Mathf.Max(
                    1,
                    poolSize);

            maximumPoolSize =
                Mathf.Max(
                    poolSize,
                    maximumPoolSize);

            maxActiveNotifications =
                Mathf.Clamp(
                    maxActiveNotifications,
                    1,
                    maximumPoolSize);

            maximumQueueSize =
                Mathf.Max(
                    1,
                    maximumQueueSize);

            defaultDuration =
                Mathf.Max(
                    0.05f,
                    defaultDuration);
        }

#endif
    }
}
