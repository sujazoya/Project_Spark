#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace AAAUI.Editor
{
    internal static class UIAnimationTimelineInput
    {
        // =========================================================
        // CONSTANTS
        // =========================================================

        private const float TrackHeight = 34f;
        private const float HandleWidth = 8f;

        // Must match UIAnimationTimelineGUI exactly.
        private const float RulerHeight = 24f;
        private const float EventHeight = 48f;

        private const float MinimumTrackDuration = 0.001f;
        private const float SnapGrid = 0.05f;

        private static int selectedTrack = -1;

        // =========================================================
        // STATE
        // =========================================================

        private static int draggingTrack = -1;

        private static int hoveredTrack = -1;

        private static DragMode dragMode =
            DragMode.None;

        private static DragMode hoveredHandle =
            DragMode.None;

        private static float dragStartMouseX;
        private static float dragStartTime;
        private static float dragStartDuration;

        // =========================================================
        // DRAG MODE
        // =========================================================

        private enum DragMode
        {
            None,
            Move,
            ResizeLeft,
            ResizeRight
        }

        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public static int HoveredTrack =>
            hoveredTrack;

        public static bool IsDragging =>
            draggingTrack >= 0;

        public static bool IsResizingLeft =>
            draggingTrack >= 0 &&
            dragMode == DragMode.ResizeLeft;

        public static bool IsResizingRight =>
            draggingTrack >= 0 &&
            dragMode == DragMode.ResizeRight;

        public static bool IsMoving =>
            draggingTrack >= 0 &&
            dragMode == DragMode.Move;

        // =========================================================
        // SELECTION CALLBACK
        // =========================================================

        public static Action<int> OnTrackSelected;

        // =========================================================
        // MAIN INPUT
        // =========================================================

        public static bool Handle(
            Rect timelineRect,
            UIAnimationSequence sequence,
            float duration)
        {
            if (sequence == null)
                return false;

            UIAnimationTrack[] tracks =
                sequence.Tracks;

            if (tracks == null ||
                tracks.Length == 0)
            {
                hoveredTrack = -1;
                hoveredHandle = DragMode.None;

                return false;
            }

            Event e = Event.current;

            if (e == null)
                return false;

            duration =
                Mathf.Max(
                    duration,
                    MinimumTrackDuration
                );

            // =====================================================
            // HOVER
            // =====================================================

            UpdateHover(
                timelineRect,
                tracks,
                duration,
                e.mousePosition
            );

            // =====================================================
            // MOUSE DOWN
            // =====================================================

            if (e.type == EventType.MouseDown &&
                e.button == 0)
            {
                return HandleMouseDown(
                    timelineRect,
                    sequence,
                    tracks,
                    duration,
                    e
                );
            }

            // =====================================================
            // DRAG
            // =====================================================

            if (e.type == EventType.MouseDrag &&
                e.button == 0)
            {
                return HandleMouseDrag(
                    timelineRect,
                    sequence,
                    tracks,
                    duration,
                    e
                );
            }

            // =====================================================
            // MOUSE UP
            // =====================================================

            if (e.type == EventType.MouseUp &&
                e.button == 0)
            {
                return HandleMouseUp(e);
            }

            return false;
        }

        // =========================================================
        // HOVER
        // =========================================================

        private static void UpdateHover(
            Rect timelineRect,
            UIAnimationTrack[] tracks,
            float duration,
            Vector2 mousePosition)
        {
            hoveredTrack =
                GetTrackAtPosition(
                    timelineRect,
                    tracks,
                    duration,
                    mousePosition
                );

            hoveredHandle =
                DragMode.None;

            if (hoveredTrack < 0)
                return;

            UIAnimationTrack track =
                tracks[hoveredTrack];

            if (track == null)
                return;

            // Sequence-duration tracks cannot be resized.
            if (track.UseSequenceDuration)
                return;

            Rect row =
                GetTrackRow(
                    timelineRect,
                    hoveredTrack
                );

            float startX =
                GetTimeX(
                    row,
                    track.StartTime,
                    duration
                );

            float endX =
                GetTimeX(
                    row,
                    track.EndTime,
                    duration
                );

            float mouseX =
                mousePosition.x;

            if (Mathf.Abs(
                    mouseX - startX
                ) <= HandleWidth)
            {
                hoveredHandle =
                    DragMode.ResizeLeft;
            }
            else if (
                Mathf.Abs(
                    mouseX - endX
                ) <= HandleWidth)
            {
                hoveredHandle =
                    DragMode.ResizeRight;
            }
        }

        // =========================================================
        // MOUSE DOWN
        // =========================================================

        private static bool HandleMouseDown(
            Rect timelineRect,
            UIAnimationSequence sequence,
            UIAnimationTrack[] tracks,
            float duration,
            Event e)
        {
            int hitTrack =
                GetTrackAtPosition(
                    timelineRect,
                    tracks,
                    duration,
                    e.mousePosition
                );

            // -----------------------------------------------------
            // EMPTY AREA
            // -----------------------------------------------------

            if (hitTrack < 0)
            {
                draggingTrack = -1;
                dragMode = DragMode.None;

                // IMPORTANT:
                // Do NOT clear selection here.
                //
                // UIAnimationTimelineGUI owns selectedTrack.
                // Clearing it here was one of the reasons selection
                // could disappear while adding input systems.

                GUI.changed = true;

                e.Use();

                return true;
            }

            UIAnimationTrack track =
                tracks[hitTrack];

            if (track == null)
                return false;

            // -----------------------------------------------------
            // SELECT
            // -----------------------------------------------------

            OnTrackSelected?.Invoke(
                hitTrack
            );

            draggingTrack =
                hitTrack;

            dragStartMouseX =
                e.mousePosition.x;

            dragStartTime =
                track.StartTime;

            dragStartDuration =
                track.Duration;

            // -----------------------------------------------------
            // DETERMINE DRAG MODE
            // -----------------------------------------------------

            if (track.UseSequenceDuration)
            {
                // Full sequence tracks are movable as a unit,
                // but cannot be resized.
                dragMode =
                    DragMode.Move;
            }
            else
            {
                float clipX =
                    GetTimeX(
                        timelineRect,
                        track.StartTime,
                        duration
                    );

                float clipEndX =
                    GetTimeX(
                        timelineRect,
                        track.EndTime,
                        duration
                    );

                float mouseX =
                    e.mousePosition.x;

                if (Mathf.Abs(
                        mouseX - clipX
                    ) <= HandleWidth)
                {
                    dragMode =
                        DragMode.ResizeLeft;
                }
                else if (
                    Mathf.Abs(
                        mouseX - clipEndX
                    ) <= HandleWidth)
                {
                    dragMode =
                        DragMode.ResizeRight;
                }
                else
                {
                    dragMode =
                        DragMode.Move;
                }
            }

            GUI.changed = true;

            e.Use();

            return true;
        }

        // =========================================================
        // DRAG
        // =========================================================

        private static bool HandleMouseDrag(
            Rect timelineRect,
            UIAnimationSequence sequence,
            UIAnimationTrack[] tracks,
            float duration,
            Event e)
        {
            if (draggingTrack < 0 ||
                draggingTrack >= tracks.Length)
            {
                return false;
            }

            UIAnimationTrack track =
                tracks[draggingTrack];

            if (track == null)
                return false;

            float deltaPixels =
                e.mousePosition.x -
                dragStartMouseX;

            float deltaTime =
                PixelsToTime(
                    timelineRect,
                    deltaPixels,
                    duration
                );

           UnityEngine.Object target =
                Selection.activeObject;

            if (target != null)
            {
                Undo.RecordObject(
                    target,
                    "Edit Animation Track"
                );
            }

            switch (dragMode)
            {
                // =================================================
                // MOVE
                // =================================================

                case DragMode.Move:
                    {
                        float newStart =
                            dragStartTime +
                            deltaTime;

                        newStart =
                            SnapTime(
                                newStart
                            );

                        float maxStart =
                            Mathf.Max(
                                0f,
                                duration -
                                dragStartDuration
                            );

                        newStart =
                            Mathf.Clamp(
                                newStart,
                                0f,
                                maxStart
                            );

                        track.SetTiming(
                            newStart,
                            dragStartDuration
                        );

                        break;
                    }

                // =================================================
                // RESIZE LEFT
                // =================================================

                case DragMode.ResizeLeft:
                    {
                        float originalEnd =
                            dragStartTime +
                            dragStartDuration;

                        float newStart =
                            dragStartTime +
                            deltaTime;

                        newStart =
                            SnapTime(
                                newStart
                            );

                        newStart =
                            Mathf.Clamp(
                                newStart,
                                0f,
                                originalEnd -
                                MinimumTrackDuration
                            );

                        float newDuration =
                            originalEnd -
                            newStart;

                        newDuration =
                            Mathf.Max(
                                MinimumTrackDuration,
                                newDuration
                            );

                        track.SetTiming(
                            newStart,
                            newDuration
                        );

                        break;
                    }

                // =================================================
                // RESIZE RIGHT
                // =================================================

                case DragMode.ResizeRight:
                    {
                        float newDuration =
                            dragStartDuration +
                            deltaTime;

                        newDuration =
                            SnapTime(
                                newDuration
                            );

                        newDuration =
                            Mathf.Max(
                                MinimumTrackDuration,
                                newDuration
                            );

                        float maxDuration =
                            Mathf.Max(
                                MinimumTrackDuration,
                                duration -
                                dragStartTime
                            );

                        newDuration =
                            Mathf.Min(
                                newDuration,
                                maxDuration
                            );

                        track.SetTiming(
                            dragStartTime,
                            newDuration
                        );

                        break;
                    }
            }

            if (target != null)
            {
                EditorUtility.SetDirty(
                    target
                );
            }

            GUI.changed = true;

            e.Use();

            return true;
        }

        // =========================================================
        // MOUSE UP
        // =========================================================

        private static bool HandleMouseUp(
            Event e)
        {
            if (draggingTrack < 0)
                return false;

            draggingTrack = -1;

            dragMode =
                DragMode.None;

            dragStartMouseX = 0f;
            dragStartTime = 0f;
            dragStartDuration = 0f;

            GUI.changed = true;

            e.Use();

            return true;
        }

        // =========================================================
        // DELETE TRACK
        // =========================================================

       

        // =========================================================
        // CLONE TRACK
        // =========================================================

        private static UIAnimationTrack CloneTrack(
            UIAnimationTrack source)
        {
            if (source == null)
                return null;

            string json =
                EditorJsonUtility.ToJson(
                    source
                );

            if (string.IsNullOrEmpty(json))
                return null;

            UIAnimationTrack clone =
                Activator.CreateInstance(
                    source.GetType()
                ) as UIAnimationTrack;

            if (clone == null)
                return null;

            EditorJsonUtility.FromJsonOverwrite(
                json,
                clone
            );

            return clone;
        }

        // =========================================================
        // TRACK HIT TEST
        // =========================================================

        private static int GetTrackAtPosition(
            Rect rect,
            UIAnimationTrack[] tracks,
            float duration,
            Vector2 mousePosition)
        {
            if (tracks == null)
                return -1;

            for (int i = 0;
                 i < tracks.Length;
                 i++)
            {
                UIAnimationTrack track =
                    tracks[i];

                if (track == null)
                    continue;

                Rect row =
                    GetTrackRow(
                        rect,
                        i
                    );

                if (!row.Contains(
                        mousePosition))
                {
                    continue;
                }

                float startX =
                    GetTimeX(
                        rect,
                        track.StartTime,
                        duration
                    );

                float endX =
                    GetTimeX(
                        rect,
                        track.EndTime,
                        duration
                    );

                Rect clip =
                    new Rect(
                        startX,
                        row.y + 3f,
                        Mathf.Max(
                            endX - startX,
                            6f
                        ),
                        row.height - 6f
                    );

                // Only actual clip is selectable.
                if (clip.Contains(
                        mousePosition))
                {
                    return i;
                }
            }

            return -1;
        }

        // =========================================================
        // TRACK ROW
        // =========================================================

        private static Rect GetTrackRow(
            Rect timelineRect,
            int index)
        {
            float tracksTop =
                timelineRect.y +
                RulerHeight +
                EventHeight;

            return new Rect(
                timelineRect.x,
                tracksTop +
                index * TrackHeight,
                timelineRect.width,
                TrackHeight
            );
        }

        // =========================================================
        // TIME → X
        // =========================================================

        private static float GetTimeX(
            Rect rect,
            float time,
            float duration)
        {
            if (duration <= 0f)
                return rect.x;

            float normalized =
                Mathf.Clamp01(
                    time / duration
                );

            return Mathf.Lerp(
                rect.x,
                rect.xMax,
                normalized
            );
        }

        // =========================================================
        // PIXELS → TIME
        // =========================================================

        private static float PixelsToTime(
            Rect rect,
            float pixels,
            float duration)
        {
            if (rect.width <= 0f ||
                duration <= 0f)
            {
                return 0f;
            }

            return
                pixels /
                rect.width *
                duration;
        }

        // =========================================================
        // SNAP
        // =========================================================

        private static float SnapTime(
            float value)
        {
            return Mathf.Round(
                value / SnapGrid
            ) * SnapGrid;
        }

        // =========================================================
        // UNDO
        // =========================================================

        private static void RecordUndo(
            string action)
        {
            UnityEngine.Object target =
                Selection.activeObject;

            if (target == null)
                return;

            Undo.RecordObject(
                target,
                action
            );
        }
    }
}

#endif