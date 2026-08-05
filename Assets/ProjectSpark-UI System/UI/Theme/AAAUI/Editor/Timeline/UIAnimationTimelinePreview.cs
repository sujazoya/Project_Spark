#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using AAAUI;

namespace AAAUI.Editor
{
    internal static class UIAnimationTimelinePreview
    {
        private static UIAnimator animator;

        private static double lastEditorTime;
        private static bool playing;
        private static bool subscribed;

        // ---------------------------------------------------------
        // BEGIN
        // ---------------------------------------------------------

        public static void Begin(
            UIAnimator target,
            UIAnimationSequenceType type)
        {
            // Always kill the previous preview first.
            Stop();

            if (target == null)
                return;

            animator = target;

            UIPlayer player = animator.EditorPlayer;

            if (player == null)
            {
                animator = null;
                return;
            }

            // -----------------------------------------------------
            // Start animation WITHOUT creating another editor loop.
            // UIAnimator's runtime coroutine does not run in edit mode.
            // -----------------------------------------------------

            switch (type)
            {
                case UIAnimationSequenceType.Open:
                    animator.PlayIn();
                    break;

                case UIAnimationSequenceType.Close:
                    animator.PlayOut();
                    break;

                case UIAnimationSequenceType.Loop:
                    animator.PlayLoop();
                    break;
            }

            player = animator.EditorPlayer;

            if (player == null || !player.IsPlaying)
            {
                animator = null;
                return;
            }

            // -----------------------------------------------------
            // IMPORTANT:
            // Take the timestamp AFTER PlayIn/PlayOut/PlayLoop.
            // -----------------------------------------------------

            lastEditorTime =
                EditorApplication.timeSinceStartup;

            playing = true;

            Subscribe();
        }

        // ---------------------------------------------------------
        // SUBSCRIBE
        // ---------------------------------------------------------

        private static void Subscribe()
        {
            if (subscribed)
                return;

            EditorApplication.update += Update;
            subscribed = true;
        }

        // ---------------------------------------------------------
        // UNSUBSCRIBE
        // ---------------------------------------------------------

        private static void Unsubscribe()
        {
            if (!subscribed)
                return;

            EditorApplication.update -= Update;
            subscribed = false;
        }

        // ---------------------------------------------------------
        // STOP
        // ---------------------------------------------------------

        public static void Stop()
        {
            Unsubscribe();

            if (animator != null)
            {
                UIPlayer player = animator.EditorPlayer;

                if (player != null)
                {
                    player.Stop();
                    player.EditorRestoreOriginal();
                }
            }

            animator = null;
            playing = false;
            lastEditorTime = 0.0;
        }

        // ---------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------

        private static void Update()
        {
            if (!playing)
            {
                Unsubscribe();
                return;
            }

            if (animator == null)
            {
                Stop();
                return;
            }

            UIPlayer player = animator.EditorPlayer;

            if (player == null)
            {
                Stop();
                return;
            }

            if (!player.IsPlaying)
            {
                Stop();
                return;
            }

            // -----------------------------------------------------
            // REAL EDITOR TIME
            // -----------------------------------------------------

            double now =
                EditorApplication.timeSinceStartup;

            double elapsed =
                now - lastEditorTime;

            lastEditorTime = now;

            // -----------------------------------------------------
            // Ignore invalid editor time.
            //
            // When Unity loses focus / recompiles / stalls, don't
            // allow one huge delta to skip the animation.
            // -----------------------------------------------------

            if (elapsed < 0.0)
                elapsed = 0.0;

            if (elapsed > 0.1)
                elapsed = 0.1;

            float deltaTime =
                (float)elapsed;

            // -----------------------------------------------------
            // THIS IS THE ONLY EDITOR CLOCK ADVANCEMENT.
            // -----------------------------------------------------

            player.Update(deltaTime);

            // -----------------------------------------------------
            // Repaint timeline + scene immediately.
            // -----------------------------------------------------

            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();

            // -----------------------------------------------------
            // Animation completed during this update.
            // -----------------------------------------------------

            if (!player.IsPlaying)
            {
                Stop();
            }
        }
    }
}

#endif