using System.Collections;
using UnityEngine;

namespace AAAUI
{
    [DisallowMultipleComponent]
    public sealed class UIAnimator : MonoBehaviour
    {
        [SerializeField] private UIAnimationProfile profile;
        [SerializeField]
        private UIAnimationTarget[] targets =
            new UIAnimationTarget[0];

        private PlaybackContext context;
        private UIPlayer player;
        private Coroutine playbackRoutine;

        private void Awake()
        {
            Initialize();
        }

        private void OnDisable()
        {
            StopPlaybackRoutine();
        }

        private void OnDestroy()
        {
            StopPlaybackRoutine();

            if (context != null)
                context.Dispose();

            context = null;
            player = null;
        }

        // =========================================================
        // PUBLIC PLAY
        // =========================================================

        public void PlayIn()
        {
            EnsureInitialized();

            player.PlayIn();

            StartRuntimePlayback();
        }

        public void PlayOut()
        {
            EnsureInitialized();

            player.PlayOut();

            StartRuntimePlayback();
        }

        public void PlayLoop()
        {
            EnsureInitialized();

            player.PlayLoop();

            StartRuntimePlayback();
        }

        public void Stop()
        {
            StopPlaybackRoutine();

            if (player != null)
                player.Stop();
        }

        public void Show()
        {
            StopPlaybackRoutine();

            EnsureInitialized();

            player.InstantIn();
        }

        public void Hide()
        {
            StopPlaybackRoutine();

            EnsureInitialized();

            player.InstantOut();
        }

        public void InstantIn()
        {
            Show();
        }

        public void InstantOut()
        {
            Hide();
        }

        // =========================================================
        // EDITOR ACCESS
        // =========================================================

        internal UIPlayer EditorPlayer => player;

        internal UIAnimationTarget[] EditorTargets => targets;

        // =========================================================
        // INITIALIZATION
        // =========================================================

        private void EnsureInitialized()
        {
            if (player == null)
                Initialize();
        }

        private void Initialize()
        {
            StopPlaybackRoutine();

            if (context != null)
                context.Dispose();

            context = new PlaybackContext(targets);

            player = new UIPlayer(context);

            player.SetProfile(profile);
        }

        // =========================================================
        // RUNTIME PLAYBACK ONLY
        // =========================================================

        private void StartRuntimePlayback()
        {
            StopPlaybackRoutine();

            // VERY IMPORTANT:
            //
            // The coroutine is ONLY allowed in actual Play Mode.
            //
            // Editor preview owns UIPlayer.Update() itself.

            if (!Application.isPlaying)
                return;

            if (!isActiveAndEnabled)
                return;

            if (player == null)
                return;

            if (!player.IsPlaying)
                return;

            playbackRoutine =
                StartCoroutine(PlaybackLoop());
        }

        private void StopPlaybackRoutine()
        {
            if (playbackRoutine == null)
                return;

            StopCoroutine(playbackRoutine);

            playbackRoutine = null;
        }

        private IEnumerator PlaybackLoop()
        {
            while (
                Application.isPlaying &&
                player != null &&
                player.IsPlaying)
            {
                player.Update(Time.deltaTime);

                yield return null;
            }

            playbackRoutine = null;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (targets == null)
                    targets = new UIAnimationTarget[0];
            }
        }

#endif
    }
}