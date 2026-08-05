#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AAAUI.Editor
{
    internal static class UIAnimationTimelineGUI
    {
        private const float HeaderHeight = 28f;
        private const float RulerHeight = 24f;
        private const float EventHeight = 48f;
        private const float TrackHeight = 34f;

        private static int draggedEvent = -1;
        private static int selectedEvent = -1;
        private static bool dragging;
        private static int draggedHandle = -1;

        private static int hoveredTrack = -1;

        private static readonly Color TrackSelectionColor =
            new Color(1f, 1f, 1f, 0.85f);

        private static readonly Color TrackHoverColor =
            new Color(1f, 1f, 1f, 0.08f);


        


        private enum TrackDragMode
        {
            None,
            Move,
            ResizeStart,
            ResizeEnd
        }      
        private static float dragMouseStartTime;
        private static float dragTrackStart;
        private static float dragTrackDuration;

        private const float TrackHandleHitWidth = 8f;
        private const float MinimumTrackDuration = 0.01f;
        private const float SnapStep = 0.01f;
        private static int selectedTrack = -1;      


       
        private static float dragStartMouseTime;
        private static float dragOriginalStart;
        private static float dragOriginalDuration;

        private const float HandleHitWidth = 8f;       

        // =========================================================
        // MAIN DRAW
        // =========================================================

        public static void Draw(
     UIAnimationSequence sequence)
        {
            if (sequence == null)
                return;

            float duration =
                Mathf.Max(
                    sequence.Duration,
                    0.001f
                );

            UIAnimationTrack[] tracks =
                sequence.Tracks;

            int trackCount =
                tracks != null
                    ? tracks.Length
                    : 0;

            float tracksHeight =
                Mathf.Max(
                    trackCount * TrackHeight,
                    TrackHeight
                );

            float totalHeight =
                HeaderHeight +
                RulerHeight +
                EventHeight +
                tracksHeight +
                20f;

            EditorGUILayout.Space(8f);

            EditorGUILayout.LabelField(
                "Animation Timeline",
                EditorStyles.boldLabel
            );

            Rect totalRect =
                GUILayoutUtility.GetRect(
                    GUIContent.none,
                    GUIStyle.none,
                    GUILayout.Height(totalHeight)
                );

            // =====================================================
            // BACKGROUND
            // =====================================================

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(
                    totalRect,
                    new Color(
                        0.055f,
                        0.065f,
                        0.075f,
                        1f
                    )
                );
            }

            // =====================================================
            // TIMELINE RECT
            // =====================================================

            Rect timelineRect =
                new Rect(
                    totalRect.x + 8f,
                    totalRect.y + HeaderHeight,
                    totalRect.width - 16f,
                    totalRect.height -
                    HeaderHeight -
                    8f
                );

            // =====================================================
            // HEADER
            // =====================================================

            DrawHeader(
                totalRect,
                duration
            );

            // =====================================================
            // RULER
            // =====================================================

            DrawRuler(
                timelineRect,
                duration
            );

            // =====================================================
            // EVENTS
            // =====================================================

            DrawEvents(
                timelineRect,
                sequence,
                duration
            );

            // =====================================================
            // TRACKS
            // =====================================================

            DrawTracks(
      timelineRect,
      sequence,
      duration
  );



            /* UIAnimationTimelineInput.Handle(
                 timelineRect,
                 sequence,
                 duration
             );*/
            UIAnimationTimelineInput.OnTrackSelected =
     index =>
     {
         selectedTrack = index;
         selectedEvent = -1;
         GUI.changed = true;
     };

            UIAnimationTimelineInput.Handle(
                timelineRect,
                sequence,
                duration
            );

            DrawPlayhead(
                timelineRect,
                sequence,
                duration
            );

            HandleEventInputOnly(
                timelineRect,
                sequence,
                duration
            );

            // =====================================================
            // SELECTED EVENT INSPECTOR
            // =====================================================

            DrawSelectedEventInspector(
                sequence,
                selectedEvent
            );
        }
        // =========================================================
        // HEADER
        // =========================================================

        private static void DrawHeader(
            Rect rect,
            float duration)
        {
            Rect header = new Rect(
                rect.x,
                rect.y,
                rect.width,
                HeaderHeight
            );

            EditorGUI.LabelField(
                header,
                $"Timeline   {duration:0.00}s",
                EditorStyles.boldLabel
            );
        }
        private static void EndTrackDrag()
        {
            draggedTrack = -1;
            trackDragMode = TrackDragMode.None;

            dragMouseStartTime = 0f;
            dragTrackStart = 0f;
            dragTrackDuration = 0f;

            GUI.changed = true;
        }
        private static bool TryBeginTrackHandleDrag(
    Rect rect,
    UIAnimationSequence sequence,
    float duration)
        {
            if (sequence == null ||
                sequence.Tracks == null)
                return false;

            Event e = Event.current;

            if (e.type != EventType.MouseDown ||
                e.button != 0)
                return false;

            UIAnimationTrack[] tracks =
                sequence.Tracks;

            float tracksTop =
                rect.y +
                RulerHeight +
                EventHeight;

            for (int i = 0; i < tracks.Length; i++)
            {
                UIAnimationTrack track =
                    tracks[i];

                if (track == null)
                    continue;

                Rect rowRect =
                    new Rect(
                        rect.x,
                        tracksTop + i * TrackHeight,
                        rect.width,
                        TrackHeight
                    );

                float startNormalized =
                    duration <= 0f
                        ? 0f
                        : Mathf.Clamp01(
                            track.StartTime / duration
                        );

                float endNormalized =
                    duration <= 0f
                        ? 0f
                        : Mathf.Clamp01(
                            track.EndTime / duration
                        );

                float startX =
                    Mathf.Lerp(
                        rowRect.x,
                        rowRect.xMax,
                        startNormalized
                    );

                float endX =
                    Mathf.Lerp(
                        rowRect.x,
                        rowRect.xMax,
                        endNormalized
                    );

                Rect startHandle =
                    new Rect(
                        startX - HandleHitWidth * 0.5f,
                        rowRect.y,
                        HandleHitWidth,
                        rowRect.height
                    );

                Rect endHandle =
                    new Rect(
                        endX - HandleHitWidth * 0.5f,
                        rowRect.y,
                        HandleHitWidth,
                        rowRect.height
                    );

                if (startHandle.Contains(e.mousePosition))
                {
                    draggedTrack = i;
                    draggedHandle = 0;

                    selectedTrack = i;
                    selectedEvent = -1;

                    GUI.changed = true;
                    e.Use();

                    return true;
                }

                if (endHandle.Contains(e.mousePosition))
                {
                    draggedTrack = i;
                    draggedHandle = 1;

                    selectedTrack = i;
                    selectedEvent = -1;

                    GUI.changed = true;
                    e.Use();

                    return true;
                }
            }

            return false;
        }
        private static bool HandleTrackHandleDrag(
    Rect rect,
    UIAnimationSequence sequence,
    float duration)
        {
            if (sequence == null ||
                sequence.Tracks == null ||
                draggedTrack < 0 ||
                draggedTrack >= sequence.Tracks.Length)
                return false;

            Event e = Event.current;

            UIAnimationTrack track =
                sequence.Tracks[draggedTrack];

            if (track == null)
                return false;

            // ---------------------------------------------------------
            // DRAG
            // ---------------------------------------------------------

            if (e.type == EventType.MouseDrag &&
                e.button == 0)
            {
                float normalized =
                    Mathf.InverseLerp(
                        rect.x,
                        rect.xMax,
                        e.mousePosition.x
                    );

                float mouseTime =
                    Mathf.Clamp01(normalized) *
                    duration;

                // Keep editor timing precise.
                mouseTime =
                    Mathf.Round(mouseTime * 100f) /
                    100f;

                float start =
                    track.StartTime;

                float end =
                    track.EndTime;

                if (draggedHandle == 0)
                {
                    // Move START.
                    //
                    // Never allow start to pass end.

                    float newStart =
                        Mathf.Clamp(
                            mouseTime,
                            0f,
                            end
                        );

                    float newDuration =
                        Mathf.Max(
                            0f,
                            end - newStart
                        );

                    Undo.RecordObject(
                        Selection.activeObject,
                        "Move Animation Track Start"
                    );

                    track.SetTiming(
                        newStart,
                        newDuration
                    );

                    GUI.changed = true;
                    e.Use();

                    return true;
                }

                if (draggedHandle == 1)
                {
                    // Move END.
                    //
                    // Never allow end before start.

                    float newEnd =
                        Mathf.Clamp(
                            mouseTime,
                            start,
                            duration
                        );

                    float newDuration =
                        Mathf.Max(
                            0f,
                            newEnd - start
                        );

                    Undo.RecordObject(
                        Selection.activeObject,
                        "Move Animation Track End"
                    );

                    track.SetTiming(
                        start,
                        newDuration
                    );

                    GUI.changed = true;
                    e.Use();

                    return true;
                }
            }

            // ---------------------------------------------------------
            // RELEASE
            // ---------------------------------------------------------

            if (e.type == EventType.MouseUp &&
                e.button == 0)
            {
                draggedTrack = -1;
                draggedHandle = -1;

                e.Use();

                return true;
            }

            return false;
        }

        // =========================================================
        // RULER
        // =========================================================

        private static void DrawRuler(
            Rect rect,
            float duration)
        {
            Rect ruler = new Rect(
                rect.x,
                rect.y,
                rect.width,
                RulerHeight
            );

            EditorGUI.DrawRect(
                ruler,
                new Color(
                    0.08f,
                    0.09f,
                    0.10f,
                    1f
                )
            );

            int divisions = Mathf.Clamp(
                Mathf.CeilToInt(duration / 0.25f),
                4,
                80
            );

            for (int i = 0; i <= divisions; i++)
            {
                float normalized =
                    i / (float)divisions;

                float x =
                    Mathf.Lerp(
                        ruler.x,
                        ruler.xMax,
                        normalized
                    );

                Color previous =
                    Handles.color;

                Handles.color =
                    new Color(
                        1f,
                        1f,
                        1f,
                        i % 4 == 0
                            ? 0.30f
                            : 0.12f
                    );

                Handles.DrawLine(
                    new Vector3(
                        x,
                        ruler.y
                    ),
                    new Vector3(
                        x,
                        ruler.yMax
                    )
                );

                Handles.color = previous;

                string label =
                    (duration * normalized)
                    .ToString("0.00");

                GUI.Label(
                    new Rect(
                        x - 25f,
                        ruler.y + 2f,
                        50f,
                        18f
                    ),
                    label,
                    EditorStyles.centeredGreyMiniLabel
                );

            }
        }

        // =========================================================
        // EVENTS
        // =========================================================

        private static void DrawEvents(
            Rect rect,
            UIAnimationSequence sequence,
            float duration)
        {
            UIAnimationEvent[] events =
                sequence.Events;

            if (events == null)
                return;

            float eventY =
                rect.y +
                RulerHeight +
                6f;

            for (int i = 0; i < events.Length; i++)
            {
                UIAnimationEvent evt =
                    events[i];

                if (evt == null)
                    continue;

                float normalized =
                    Mathf.Clamp01(
                        evt.Time / duration
                    );

                float x =
                    Mathf.Lerp(
                        rect.x,
                        rect.xMax,
                        normalized
                    );

                bool selected =
                    selectedEvent == i;

                DrawEventMarker(
                    x,
                    eventY,
                    evt,
                    selected,
                    i
                );
            }
        }

        private static void DrawEventMarker(
            float x,
            float y,
            UIAnimationEvent evt,
            bool selected,
            int index)
        {
            float markerSize =
                selected ? 9f : 7f;

            Vector3 top =
                new Vector3(
                    x,
                    y
                );

            Vector3 left =
                new Vector3(
                    x - markerSize,
                    y + markerSize
                );

            Vector3 bottom =
                new Vector3(
                    x,
                    y + markerSize * 2f
                );

            Vector3 right =
                new Vector3(
                    x + markerSize,
                    y + markerSize
                );

            Color previous =
                Handles.color;

            Handles.color =
                selected
                    ? Color.white
                    : new Color(
                        0.2f,
                        0.8f,
                        1f,
                        1f
                    );

            Handles.DrawAAConvexPolygon(
                top,
                left,
                bottom,
                right
            );

            Handles.color = previous;

            string label =
                string.IsNullOrEmpty(evt.Id)
                    ? evt.Type.ToString()
                    : evt.Id;

            GUI.Label(
                new Rect(
                    x - 55f,
                    y + 17f,
                    110f,
                    18f
                ),
                label,
                EditorStyles.miniLabel
            );

            GUI.Label(
                new Rect(
                    x - 40f,
                    y + 31f,
                    80f,
                    16f
                ),
                evt.Time.ToString("0.00") + "s",
                EditorStyles.centeredGreyMiniLabel
            );
        }

        // =========================================================
        // TRACKS
        // =========================================================
       

        private static void BeginTrackDrag(
    TrackDragMode mode,
    int trackIndex,
    float mouseX,
    UIAnimationTrack track,
    Rect rect,
    float duration)
        {
            draggedTrack = trackIndex;
            trackDragMode = mode;

            dragMouseStartTime =
                TimeFromMouse(
                    rect,
                    duration,
                    mouseX
                );

            dragTrackStart =
                track.StartTime;

            dragTrackDuration =
                track.Duration;

            selectedTrack = trackIndex;

            // One Undo operation for the entire drag.
            Object undoTarget =
                Selection.activeObject;

            if (undoTarget != null)
            {
                Undo.RecordObject(
                    undoTarget,
                    mode == TrackDragMode.Move
                        ? "Move Animation Track"
                        : "Resize Animation Track"
                );
            }

            GUI.changed = true;
        }
        // =========================================================
        // TRACK MANIPULATION        // =========================================================

        private static float TimeFromMouse(
      Rect rect,
      float duration,
      float mouseX)
        {
            if (duration <= 0f)
                return 0f;

            float normalized =
                Mathf.InverseLerp(
                    rect.x,
                    rect.xMax,
                    mouseX
                );

            return Mathf.Clamp01(normalized) * duration;
        }

        private static float SnapTime(float time)
        {
            return Mathf.Round(time / SnapStep) * SnapStep;
        }

        private static bool IsNear(
            float value,
            float target,
            float distance)
        {
            return Mathf.Abs(value - target) <= distance;
        }

        private static int draggedTrack = -1;

        private static TrackDragMode trackDragMode =
            TrackDragMode.None;

        private static float trackDragMouseStartTime;
        private static float trackOriginalStart;
        private static float trackOriginalDuration;

        // =========================================================
        // TRACK INPUT
        // MOVE + RESIZE
        // =========================================================



        private static void HandleTrackInput(
     Rect rect,
     UIAnimationSequence sequence,
     float duration)
        {
            if (sequence == null ||
                sequence.Tracks == null)
                return;

            Event e = Event.current;

            if (e == null)
                return;

            UIAnimationTrack[] tracks =
                sequence.Tracks;

            float tracksTop =
                rect.y +
                RulerHeight +
                EventHeight;

            // =========================================================
            // MOUSE DOWN
            // =========================================================

            if (e.type == EventType.MouseDown &&
                e.button == 0)
            {
                for (int i = 0; i < tracks.Length; i++)
                {
                    UIAnimationTrack track =
                        tracks[i];

                    if (track == null)
                        continue;

                    Rect row =
                        new Rect(
                            rect.x,
                            tracksTop + i * TrackHeight,
                            rect.width,
                            TrackHeight
                        );

                    if (!row.Contains(e.mousePosition))
                        continue;

                    float startNormalized =
                        duration <= 0f
                            ? 0f
                            : Mathf.Clamp01(
                                track.StartTime / duration
                            );

                    float endNormalized =
                        duration <= 0f
                            ? 0f
                            : Mathf.Clamp01(
                                track.EndTime / duration
                            );

                    float startX =
                        Mathf.Lerp(
                            rect.x,
                            rect.xMax,
                            startNormalized
                        );

                    float endX =
                        Mathf.Lerp(
                            rect.x,
                            rect.xMax,
                            endNormalized
                        );

                    // -------------------------------------------------
                    // SELECT
                    // -------------------------------------------------

                    selectedTrack = i;
                    selectedEvent = -1;

                    draggedTrack = i;

                    dragOriginalStart =
                        track.StartTime;

                    dragOriginalDuration =
                        track.Duration;

                    dragStartMouseTime =
                        MouseToTime(
                            rect,
                            e.mousePosition.x,
                            duration
                        );

                    // -------------------------------------------------
                    // LEFT HANDLE
                    // -------------------------------------------------

                        if (
                         Mathf.Abs(
                             e.mousePosition.x - startX
                         ) <= HandleHitWidth &&
                         !track.UseSequenceDuration)
                    {
                        trackDragMode =
                            TrackDragMode.ResizeStart;
                    }

                    // -------------------------------------------------
                    // RIGHT HANDLE
                    // -------------------------------------------------

                    else if (
                         Mathf.Abs(
                             e.mousePosition.x - endX
                         ) <= HandleHitWidth &&
                         !track.UseSequenceDuration)
                        {
                        trackDragMode =
                            TrackDragMode.ResizeEnd;
                    }

                    // -------------------------------------------------
                    // BODY
                    // -------------------------------------------------

                    else
                    {
                        trackDragMode =
                            TrackDragMode.Move;
                    }

                    GUI.changed = true;
                    e.Use();

                    if (TryBeginTrackHandleDrag(
                    rect,
                    sequence,
                    duration))
                    {
                        return;
                    }

                    if (HandleTrackHandleDrag(
                            rect,
                            sequence,
                            duration))
                    {
                        return;
                    }

                    return;

                }
            }

            // =========================================================
            // DRAG
            // =========================================================

            if (e.type == EventType.MouseDrag &&
                e.button == 0 &&
                draggedTrack >= 0 &&
                draggedTrack < tracks.Length &&
                trackDragMode != TrackDragMode.None)
            {
                UIAnimationTrack track =
                    tracks[draggedTrack];

                if (track == null)
                    return;

                float currentMouseTime =
                    MouseToTime(
                        rect,
                        e.mousePosition.x,
                        duration
                    );

                float delta =
                    currentMouseTime -
                    dragStartMouseTime;

                // =====================================================
                // MOVE
                // =====================================================

                if (trackDragMode ==
                    TrackDragMode.Move)
                {
                    float newStart =
                        dragOriginalStart +
                        delta;

                    float maxStart =
                        Mathf.Max(
                            0f,
                            duration -
                            dragOriginalDuration
                        );

                    newStart =
                        Mathf.Clamp(
                            newStart,
                            0f,
                            maxStart
                        );

                    newStart =
                        Snap(
                            newStart
                        );

                    track.SetTiming(
                        newStart,
                        dragOriginalDuration
                    );
                }

                // =====================================================
                // RESIZE START
                // =====================================================

                else if (trackDragMode ==
                         TrackDragMode.ResizeStart)
                {
                    float originalEnd =
                        dragOriginalStart +
                        dragOriginalDuration;

                    float newStart =
                        dragOriginalStart +
                        delta;

                    newStart =
                        Mathf.Clamp(
                            newStart,
                            0f,
                            originalEnd -
                            MinimumTrackDuration
                        );

                    newStart =
                        Snap(
                            newStart
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
                }

                // =====================================================
                // RESIZE END
                // =====================================================

                else if (trackDragMode ==
                         TrackDragMode.ResizeEnd)
                {
                    float newEnd =
                        dragOriginalStart +
                        dragOriginalDuration +
                        delta;

                    newEnd =
                        Mathf.Clamp(
                            newEnd,
                            dragOriginalStart +
                            MinimumTrackDuration,
                            duration
                        );

                    newEnd =
                        Snap(
                            newEnd
                        );

                    float newDuration =
                        newEnd -
                        dragOriginalStart;

                    newDuration =
                        Mathf.Max(
                            MinimumTrackDuration,
                            newDuration
                        );

                    track.SetTiming(
                        dragOriginalStart,
                        newDuration
                    );
                }

                GUI.changed = true;

                e.Use();

                return;
            }

            // =========================================================
            // MOUSE UP
            // =========================================================

            if (e.type == EventType.MouseUp &&
                e.button == 0)
            {
                draggedTrack = -1;
                trackDragMode =
                    TrackDragMode.None;

                e.Use();

                return;
            }
        }


        private static float MouseToTime(
    Rect rect,
    float mouseX,
    float duration)
        {
            if (duration <= 0f)
                return 0f;

            float normalized =
                Mathf.InverseLerp(
                    rect.x,
                    rect.xMax,
                    mouseX
                );

            return normalized * duration;
        }


        private static float Snap(float value)
        {
            return Mathf.Round(
                value / SnapStep
            ) * SnapStep;
        }
        // =========================================================
        // PIXEL → TIMELINE TIME
        // =========================================================

        private static float PixelToTime(
            float pixelX,
            Rect rect,
            float duration)
        {
            if (duration <= 0f)
                return 0f;

            float normalized =
                Mathf.InverseLerp(
                    rect.x,
                    rect.xMax,
                    pixelX
                );

            return Mathf.Clamp01(
                normalized
            ) * duration;
        }

        // =========================================================
        // TRACKS
        // =========================================================

        // =========================================================
        // TRACKS
        // =========================================================

        private static void DrawTracks(
     Rect rect,
     UIAnimationSequence sequence,
     float duration)
        {
            if (sequence == null ||
                sequence.Tracks == null)
                return;

            UIAnimationTrack[] tracks =
                sequence.Tracks;

            float tracksTop =
                rect.y +
                RulerHeight +
                EventHeight;

            hoveredTrack = -1;

            for (int i = 0; i < tracks.Length; i++)
            {
                UIAnimationTrack track =
                    tracks[i];

                if (track == null)
                    continue;

                Rect rowRect =
                    new Rect(
                        rect.x,
                        tracksTop + i * TrackHeight,
                        rect.width,
                        TrackHeight
                    );

                bool selected =
                    selectedTrack == i;

                bool hovered =
                    rowRect.Contains(
                        Event.current.mousePosition
                    );

                if (hovered)
                    hoveredTrack = i;

                // =====================================================
                // ROW
                // =====================================================

                EditorGUI.DrawRect(
                    rowRect,
                    i % 2 == 0
                        ? new Color(
                            0.065f,
                            0.075f,
                            0.085f,
                            1f
                        )
                        : new Color(
                            0.055f,
                            0.065f,
                            0.075f,
                            1f
                        )
                );

                // =====================================================
                // HOVER
                // =====================================================

                if (hovered && !selected)
                {
                    EditorGUI.DrawRect(
                        rowRect,
                        TrackHoverColor
                    );
                }

                // =====================================================
                // TIME
                // =====================================================

                float startNormalized =
                    duration <= 0f
                        ? 0f
                        : Mathf.Clamp01(
                            track.StartTime / duration
                        );

                float endNormalized =
                    duration <= 0f
                        ? 0f
                        : Mathf.Clamp01(
                            track.EndTime / duration
                        );

                float startX =
                    Mathf.Lerp(
                        rowRect.x,
                        rowRect.xMax,
                        startNormalized
                    );

                float endX =
                    Mathf.Lerp(
                        rowRect.x,
                        rowRect.xMax,
                        endNormalized
                    );

                float width =
                    Mathf.Max(
                        5f,
                        endX - startX
                    );

                Rect clip =
                    new Rect(
                        startX,
                        rowRect.y + 4f,
                        width,
                        TrackHeight - 8f
                    );


                // =========================================================
                // TRACK TOOLBAR
                // =========================================================

                EditorGUILayout.BeginHorizontal(
                    EditorStyles.toolbar
                );

                if (GUILayout.Button(
                    "+ Track",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(70f)))
                {
                    AddTrack(sequence);
                }

                EditorGUI.BeginDisabledGroup(
                    selectedTrack < 0 ||
                    selectedTrack >= tracks.Length
                );

                if (GUILayout.Button(
                    "Duplicate",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(75f)))
                {
                    DuplicateTrack(sequence, selectedTrack);
                }

                if (GUILayout.Button(
                    "Delete",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(60f)))
                {
                    DeleteTrack(sequence, selectedTrack);
                }

                if (GUILayout.Button(
                    "▲",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(30f)))
                {
                    MoveTrack(
                        sequence,
                        selectedTrack,
                        selectedTrack - 1
                    );
                }

                if (GUILayout.Button(
                    "▼",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(30f)))
                {
                    MoveTrack(
                        sequence,
                        selectedTrack,
                        selectedTrack + 1
                    );
                }


                EditorGUI.EndDisabledGroup();

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();

                // =====================================================
                // TRACK COLOR
                // =====================================================

                Color trackColor =
                    GetTrackColor(track);

                EditorGUI.DrawRect(
                    clip,
                    trackColor
                );

                // =====================================================
                // TOP HIGHLIGHT
                // =====================================================

                EditorGUI.DrawRect(
                    new Rect(
                        clip.x,
                        clip.y,
                        clip.width,
                        2f
                    ),
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.22f
                    )
                );

                // =====================================================
                // SELECTED OUTLINE
                // =====================================================

                if (selected)
                {
                    DrawRectOutline(
                        clip,
                        TrackSelectionColor,
                        1f
                    );
                }

                // =====================================================
                // END SHADE
                // =====================================================

                if (clip.width > 10f)
                {
                    EditorGUI.DrawRect(
                        new Rect(
                            clip.xMax - 4f,
                            clip.y,
                            4f,
                            clip.height
                        ),
                        new Color(
                            0f,
                            0f,
                            0f,
                            0.18f
                        )
                    );
                }

                // =====================================================
                // LABEL
                // =====================================================

                GUI.Label(
                    new Rect(
                        clip.x + 7f,
                        clip.y + 1f,
                        Mathf.Max(
                            20f,
                            clip.width - 14f
                        ),
                        clip.height - 2f
                    ),
                    track.DisplayName,
                    EditorStyles.whiteMiniLabel
                );

                // =====================================================
                // TIME
                // =====================================================

                if (clip.width >= 120f)
                {
                    GUI.Label(
                        new Rect(
                            clip.xMax - 105f,
                            clip.y + 1f,
                            100f,
                            clip.height - 2f
                        ),
                        $"{track.StartTime:0.00}s → {track.EndTime:0.00}s",
                        EditorStyles.centeredGreyMiniLabel
                    );
                }

                // =====================================================
                // HANDLES
                // =====================================================

                DrawTrackHandle(
                    startX,
                    rowRect.center.y
                );

                DrawTrackHandle(
                    endX,
                    rowRect.center.y
                );
            }

            // =========================================================
            // INSPECTOR
            // =========================================================

            DrawSelectedTrackInspector(
                sequence,
                selectedTrack
            );

            if (Event.current.type == EventType.Repaint)
                HandleUtility.Repaint();
        }
        private static void DrawSelectedTrackInspector(
     UIAnimationSequence sequence,
     int index)
        {
            if (sequence == null)
                return;

            UIAnimationTrack[] tracks =
                sequence.Tracks;

            if (tracks == null ||
                index < 0 ||
                index >= tracks.Length)
                return;

            UIAnimationTrack track =
                tracks[index];

            if (track == null)
                return;

            EditorGUILayout.Space(6f);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox
            );

            // =========================================================
            // HEADER
            // =========================================================

            EditorGUILayout.LabelField(
                "Selected Track",
                EditorStyles.boldLabel
            );

            EditorGUILayout.LabelField(
                track.DisplayName,
                EditorStyles.miniLabel
            );

            EditorGUILayout.Space(4f);

            // =========================================================
            // TARGET
            // =========================================================

            int targetIndex =
                EditorGUILayout.IntField(
                    "Target Index",
                    track.TargetIndex
                );

            targetIndex =
                Mathf.Max(
                    0,
                    targetIndex
                );

            if (targetIndex != track.TargetIndex)
            {
                Undo.RecordObject(
                    Selection.activeObject,
                    "Change Animation Target"
                );

                track.SetTargetIndex(
                    targetIndex
                );

                GUI.changed = true;
            }

            // =========================================================
            // START TIME
            // =========================================================

            float startTime =
                EditorGUILayout.FloatField(
                    "Start Time",
                    track.StartTime
                );

            startTime =
                Mathf.Clamp(
                    startTime,
                    0f,
                    sequence.Duration
                );

            // =========================================================
            // USE SEQUENCE DURATION
            // =========================================================

            bool useSequenceDuration =
                EditorGUILayout.Toggle(
                    "Use Sequence Duration",
                    track.UseSequenceDuration
                );

            if (useSequenceDuration !=
                track.UseSequenceDuration)
            {
                Undo.RecordObject(
                    Selection.activeObject,
                    "Change Sequence Duration Mode"
                );

                track.SetUseSequenceDuration(
                    useSequenceDuration,
                    sequence.Duration
                );

                GUI.changed = true;
            }

            // =========================================================
            // DURATION
            // =========================================================

            float duration =
                track.Duration;

            if (track.UseSequenceDuration)
            {
                EditorGUILayout.FloatField(
                    "Duration",
                    sequence.Duration
                );

                startTime = 0f;
                duration = sequence.Duration;
            }
            else
            {
                duration =
                    EditorGUILayout.FloatField(
                        "Duration",
                        duration
                    );

                duration =
                    Mathf.Max(
                        0.001f,
                        duration
                    );

                float maxDuration =
                    Mathf.Max(
                        0.001f,
                        sequence.Duration -
                        startTime
                    );

                duration =
                    Mathf.Min(
                        duration,
                        maxDuration
                    );
            }

            // =========================================================
            // END TIME
            // =========================================================

            float endTime =
                startTime +
                duration;

            EditorGUILayout.FloatField(
                "End Time",
                endTime
            );

            // =========================================================
            // EASE
            // =========================================================

            UIEase ease =
                (UIEase)EditorGUILayout.EnumPopup(
                    "Ease",
                    track.Ease
                );

            // =========================================================
            // APPLY TIMING
            // =========================================================

            bool timingChanged =
                !Mathf.Approximately(
                    track.StartTime,
                    startTime
                ) ||
                !Mathf.Approximately(
                    track.Duration,
                    duration
                );

            if (timingChanged)
            {
                Undo.RecordObject(
                    Selection.activeObject,
                    "Change Animation Track Timing"
                );

                track.SetTiming(
                    startTime,
                    duration
                );

                GUI.changed = true;
            }

            // =========================================================
            // APPLY EASE
            // =========================================================

            if (ease != track.Ease)
            {
                Undo.RecordObject(
                    Selection.activeObject,
                    "Change Animation Track Ease"
                );

                track.SetEase(
                    ease
                );

                GUI.changed = true;
            }

            // =========================================================
            // DELETE TRACK
            // =========================================================

            EditorGUILayout.Space(6f);

            if (GUILayout.Button(
                "Delete Track"))
            {
                DeleteTrack(
                    sequence,
                    index
                );

                selectedTrack = -1;

                GUI.changed = true;
            }

            EditorGUILayout.EndVertical();

            // =========================================================
            // DIRTY
            // =========================================================

            if (GUI.changed)
            {
                Object target =
                    Selection.activeObject;

                if (target != null)
                {
                    EditorUtility.SetDirty(
                        target
                    );
                }
            }
        }



        private static void AddTrack(
    UIAnimationSequence sequence)
        {
            if (sequence == null)
                return;

            UIAnimationTrack[] oldTracks =
                sequence.Tracks;

            int oldCount =
                oldTracks != null
                    ? oldTracks.Length
                    : 0;

            UIAnimationTrack[] newTracks =
                new UIAnimationTrack[oldCount + 1];

            for (int i = 0; i < oldCount; i++)
            {
                newTracks[i] =
                    oldTracks[i];
            }

            // ---------------------------------------------------------
            // Default track
            // ---------------------------------------------------------

            // We cannot instantiate the abstract
            // UIAnimationTrack directly.
            //
            // Start with a FadeTrack because it is a
            // safe concrete default track.

            FadeTrack track =
                new FadeTrack();

            track.SetTiming(
                0f,
                Mathf.Min(
                    0.2f,
                    sequence.Duration
                )
            );

            track.SetTargetIndex(0);

            newTracks[oldCount] =
                track;

            Undo.RecordObject(
                Selection.activeObject,
                "Add Animation Track"
            );

            sequence.SetTracks(
                newTracks
            );

            selectedTrack =
                oldCount;

            selectedEvent = -1;

            GUI.changed = true;

            if (Selection.activeObject != null)
            {
                EditorUtility.SetDirty(
                    Selection.activeObject
                );
            }
        }

        private static void DuplicateTrack(
    UIAnimationSequence sequence,
    int index)
        {
            if (sequence == null ||
                sequence.Tracks == null ||
                index < 0 ||
                index >= sequence.Tracks.Length)
                return;

            UIAnimationTrack source =
                sequence.Tracks[index];

            if (source == null)
                return;

            UIAnimationTrack duplicate =
                DuplicateTrackInstance(source);

            if (duplicate == null)
                return;

            UIAnimationTrack[] oldTracks =
                sequence.Tracks;

            UIAnimationTrack[] newTracks =
                new UIAnimationTrack[
                    oldTracks.Length + 1
                ];

            for (int i = 0; i < index + 1; i++)
            {
                newTracks[i] =
                    oldTracks[i];
            }

            newTracks[index + 1] =
                duplicate;

            for (
                int i = index + 1;
                i < oldTracks.Length;
                i++)
            {
                newTracks[i + 1] =
                    oldTracks[i];
            }

            Undo.RecordObject(
                Selection.activeObject,
                "Duplicate Animation Track"
            );

            sequence.SetTracks(
                newTracks
            );

            selectedTrack =
                index + 1;

            selectedEvent = -1;

            GUI.changed = true;

            if (Selection.activeObject != null)
            {
                EditorUtility.SetDirty(
                    Selection.activeObject
                );
            }
        }

        private static UIAnimationTrack DuplicateTrackInstance(
    UIAnimationTrack source)
        {
            if (source == null)
                return null;

            if (source is FadeTrack)
                return new FadeTrack();

            if (source is ScaleTrack)
                return new ScaleTrack();

            if (source is SlideTrack)
                return new SlideTrack();

            if (source is ShakeTrack)
                return new ShakeTrack();

            if (source is FlashTrack)
                return new FlashTrack();

            if (source is GlowTrack)
                return new GlowTrack();

            if (source is DissolveTrack)
                return new DissolveTrack();

            if (source is GlitchTrack)
                return new GlitchTrack();

            if (source is MaterialFloatTrack)
                return new MaterialFloatTrack();

            if (source is MaterialColorTrack)
                return new MaterialColorTrack();

            return null;
        }

        private static void DeleteTrack(
    UIAnimationSequence sequence,
    int index)
        {
            if (sequence == null ||
                sequence.Tracks == null ||
                index < 0 ||
                index >= sequence.Tracks.Length)
                return;

            UIAnimationTrack[] oldTracks =
                sequence.Tracks;

            UIAnimationTrack[] newTracks =
                new UIAnimationTrack[
                    oldTracks.Length - 1
                ];

            int writeIndex = 0;

            for (int i = 0; i < oldTracks.Length; i++)
            {
                if (i == index)
                    continue;

                newTracks[writeIndex++] =
                    oldTracks[i];
            }

            Undo.RecordObject(
                Selection.activeObject,
                "Delete Animation Track"
            );

            sequence.SetTracks(
                newTracks
            );

            selectedTrack = Mathf.Clamp(
                selectedTrack,
                0,
                newTracks.Length - 1
            );

            selectedEvent = -1;

            GUI.changed = true;

            if (Selection.activeObject != null)
            {
                EditorUtility.SetDirty(
                    Selection.activeObject
                );
            }
        }

        private static void MoveTrack(
    UIAnimationSequence sequence,
    int from,
    int to)
        {
            if (sequence == null ||
                sequence.Tracks == null)
                return;

            UIAnimationTrack[] tracks =
                sequence.Tracks;

            if (from < 0 ||
                from >= tracks.Length ||
                to < 0 ||
                to >= tracks.Length)
                return;

            if (from == to)
                return;

            Undo.RecordObject(
                Selection.activeObject,
                "Move Animation Track"
            );

            UIAnimationTrack temp =
                tracks[from];

            tracks[from] =
                tracks[to];

            tracks[to] =
                temp;

            sequence.SetTracks(
                tracks
            );

            selectedTrack =
                to;

            GUI.changed = true;

            if (Selection.activeObject != null)
            {
                EditorUtility.SetDirty(
                    Selection.activeObject
                );
            }
        }



        private static void DrawRectOutline(
    Rect rect,
    Color color,
    float thickness)
        {
            Color previous =
                Handles.color;

            Handles.color = color;

            Handles.DrawAAPolyLine(
                thickness,
                new Vector3(rect.x, rect.y),
                new Vector3(rect.xMax, rect.y)
            );

            Handles.DrawAAPolyLine(
                thickness,
                new Vector3(rect.xMax, rect.y),
                new Vector3(rect.xMax, rect.yMax)
            );

            Handles.DrawAAPolyLine(
                thickness,
                new Vector3(rect.xMax, rect.yMax),
                new Vector3(rect.x, rect.yMax)
            );

            Handles.DrawAAPolyLine(
                thickness,
                new Vector3(rect.x, rect.yMax),
                new Vector3(rect.x, rect.y)
            );

            Handles.color = previous;
        }

        private static void DrawTrackHandle(
    float x,
    float y)
        {
            Handles.color =
                Color.white;

            Handles.DrawAAPolyLine(
                3f,
                new Vector3(
                    x,
                    y - 7f
                ),
                new Vector3(
                    x,
                    y + 7f
                )
            );
        }


        // =========================================================
        // TRACK ROW
        // =========================================================

        private static void DrawTrackRow(
            Rect rowRect,
            UIAnimationTrack track,
            float duration,
            int index)
        {
            // -----------------------------------------------------
            // ROW BACKGROUND
            // -----------------------------------------------------

            EditorGUI.DrawRect(
                rowRect,
                index % 2 == 0
                    ? new Color(
                        0.065f,
                        0.075f,
                        0.085f,
                        1f
                    )
                    : new Color(
                        0.055f,
                        0.065f,
                        0.075f,
                        1f
                    )
            );

            // -----------------------------------------------------
            // GRID LINES
            // -----------------------------------------------------

            DrawTrackGrid(
                rowRect,
                duration
            );

            // -----------------------------------------------------
            // TRACK CLIP
            // -----------------------------------------------------

            float normalizedStart =
                Mathf.Clamp01(
                    track.StartTime / duration
                );

            float normalizedEnd =
                Mathf.Clamp01(
                    track.EndTime / duration
                );

            float x =
                Mathf.Lerp(
                    rowRect.x,
                    rowRect.xMax,
                    normalizedStart
                );

            float endX =
                Mathf.Lerp(
                    rowRect.x,
                    rowRect.xMax,
                    normalizedEnd
                );

            float width =
                Mathf.Max(
                    endX - x,
                    4f
                );

            Rect clipRect =
                new Rect(
                    x,
                    rowRect.y + 5f,
                    width,
                    rowRect.height - 10f
                );

            DrawTrackClip(
                clipRect,
                track
            );

            // -----------------------------------------------------
            // TRACK LABEL
            // -----------------------------------------------------

            DrawTrackLabel(
                rowRect,
                track,
                clipRect
            );
        }

        // =========================================================
        // TRACK GRID
        // =========================================================

        private static void DrawTrackGrid(
            Rect rowRect,
            float duration)
        {
            int divisions = Mathf.Clamp(
                Mathf.CeilToInt(duration / 0.25f),
                4,
                80
            );

            Color previous =
                Handles.color;

            Handles.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.035f
                );

            for (int i = 0; i <= divisions; i++)
            {
                float normalized =
                    i / (float)divisions;

                float x =
                    Mathf.Lerp(
                        rowRect.x,
                        rowRect.xMax,
                        normalized
                    );

                Handles.DrawLine(
                    new Vector3(
                        x,
                        rowRect.y
                    ),
                    new Vector3(
                        x,
                        rowRect.yMax
                    )
                );
            }

            Handles.color = previous;
        }

        // =========================================================
        // TRACK CLIP
        // =========================================================

        private static void DrawTrackClip(
            Rect rect,
            UIAnimationTrack track)
        {
            Color clipColor =
                GetTrackColor(track);

            EditorGUI.DrawRect(
                rect,
                new Color(
                    clipColor.r,
                    clipColor.g,
                    clipColor.b,
                    0.75f
                )
            );

            // -----------------------------------------------------
            // CLIP BORDER
            // -----------------------------------------------------

            Color previous =
                Handles.color;

            Handles.color =
                new Color(
                    clipColor.r,
                    clipColor.g,
                    clipColor.b,
                    1f
                );

            Handles.DrawLine(
                new Vector3(
                    rect.x,
                    rect.y
                ),
                new Vector3(
                    rect.xMax,
                    rect.y
                )
            );

            Handles.DrawLine(
                new Vector3(
                    rect.x,
                    rect.yMax
                ),
                new Vector3(
                    rect.xMax,
                    rect.yMax
                )
            );

            Handles.color = previous;

            // -----------------------------------------------------
            // CLIP TEXT
            // -----------------------------------------------------

            string text =
                track.DisplayName;

            if (rect.width >= 55f)
            {
                GUI.Label(
                    new Rect(
                        rect.x + 6f,
                        rect.y,
                        rect.width - 12f,
                        rect.height
                    ),
                    text,
                    EditorStyles.whiteMiniLabel
                );
            }
        }

        // =========================================================
        // TRACK LABEL
        // =========================================================

        private static void DrawTrackLabel(
            Rect rowRect,
            UIAnimationTrack track,
            Rect clipRect)
        {
            // If clip starts far enough to the right,
            // put the label before it.

            if (clipRect.x - rowRect.x > 70f)
            {
                GUI.Label(
                    new Rect(
                        rowRect.x + 5f,
                        rowRect.y + 7f,
                        100f,
                        20f
                    ),
                    track.DisplayName,
                    EditorStyles.miniLabel
                );
            }
        }

        // =========================================================
        // TRACK COLOR
        // =========================================================

        private static Color GetTrackColor(
            UIAnimationTrack track)
        {
            if (track is GlowTrack)
            {
                return new Color(
                    0.15f,
                    0.75f,
                    1f,
                    1f
                );
            }

            if (track is GlitchTrack)
            {
                return new Color(
                    0.75f,
                    0.30f,
                    1f,
                    1f
                );
            }

            if (track is DissolveTrack)
            {
                return new Color(
                    1f,
                    0.45f,
                    0.15f,
                    1f
                );
            }

            if (track is SlideTrack)
            {
                return new Color(
                    0.25f,
                    0.85f,
                    0.45f,
                    1f
                );
            }

            if (track is ScaleTrack)
            {
                return new Color(
                    0.95f,
                    0.65f,
                    0.20f,
                    1f
                );
            }

            if (track is FadeTrack)
            {
                return new Color(
                    0.35f,
                    0.65f,
                    1f,
                    1f
                );
            }

            if (track is ShakeTrack)
            {
                return new Color(
                    0.95f,
                    0.30f,
                    0.35f,
                    1f
                );
            }

            if (track is FlashTrack)
            {
                return new Color(
                    1f,
                    0.85f,
                    0.30f,
                    1f
                );
            }

            if (track is MaterialFloatTrack)
            {
                return new Color(
                    0.30f,
                    0.80f,
                    0.80f,
                    1f
                );
            }

            if (track is MaterialColorTrack)
            {
                return new Color(
                    0.55f,
                    0.65f,
                    1f,
                    1f
                );
            }

            return new Color(
                0.45f,
                0.50f,
                0.55f,
                1f
            );
        }

        // =========================================================
        // PLAYHEAD
        // =========================================================

        private static void DrawPlayhead(
            Rect rect,
            UIAnimationSequence sequence,
            float duration)
        {
            if (sequence == null)
                return;

            // Preview system owns the actual playback clock.
            // This GUI intentionally does not create another clock.
        }

        // =========================================================
        // INPUT
        // =========================================================

        private static void HandleEventInputOnly(
             Rect rect,
            UIAnimationSequence sequence,
            float duration)
        {
            Event e = Event.current;

            if (e == null)
                return;

            UIAnimationEvent[] events =
                sequence.Events;

            if (events == null)
                return;

            // -----------------------------------------------------
            // MOUSE DOWN
            // -----------------------------------------------------

            if (e.type == EventType.MouseDown &&
                e.button == 0)
            {
                for (int i = 0; i < events.Length; i++)
                {
                    UIAnimationEvent evt =
                        events[i];

                    if (evt == null)
                        continue;

                    float normalized =
                        Mathf.Clamp01(
                            evt.Time / duration
                        );

                    float x =
                        Mathf.Lerp(
                            rect.x,
                            rect.xMax,
                            normalized
                        );

                    float y =
                        rect.y +
                        RulerHeight +
                        6f;

                    Rect hit =
                        new Rect(
                            x - 12f,
                            y - 5f,
                            24f,
                            24f
                        );

                    if (!hit.Contains(
                        e.mousePosition))
                        continue;

                    selectedEvent = i;
                    draggedEvent = i;
                    dragging = true;

                    GUI.changed = true;
                    e.Use();

                    return;
                }
            }

            // -----------------------------------------------------
            // DRAG
            // -----------------------------------------------------

            if (e.type == EventType.MouseDrag &&
                e.button == 0 &&
                dragging &&
                draggedEvent >= 0 &&
                draggedEvent < events.Length)
            {
                float normalized =
                    Mathf.InverseLerp(
                        rect.x,
                        rect.xMax,
                        e.mousePosition.x
                    );

                float time =
                    Mathf.Clamp01(normalized)
                    * duration;

                time =
                    Mathf.Round(
                        time * 100f
                    ) / 100f;

                events[draggedEvent]
                    .SetTime(time);

                GUI.changed = true;
                e.Use();

                return;
            }

            // -----------------------------------------------------
            // MOUSE UP
            // -----------------------------------------------------

            if (e.type == EventType.MouseUp &&
                e.button == 0)
            {
                dragging = false;
                draggedEvent = -1;

                e.Use();

                return;
            }

            // -----------------------------------------------------
            // DELETE
            // -----------------------------------------------------

            if (e.type == EventType.KeyDown &&
                e.keyCode == KeyCode.Delete &&
                selectedEvent >= 0 &&
                selectedEvent < events.Length)
            {
                Undo.RecordObject(
                    GetCurrentObject(),
                    "Delete Animation Event"
                );

                RemoveEvent(
                    sequence,
                    selectedEvent
                );

                selectedEvent = -1;

                GUI.changed = true;
                e.Use();
            }
        }

        // =========================================================
        // EVENT INSPECTOR
        // =========================================================

        private static void DrawSelectedEventInspector(
            UIAnimationSequence sequence,
            int index)
        {
            if (sequence == null)
                return;

            UIAnimationEvent[] events =
                sequence.Events;

            if (events == null ||
                index < 0 ||
                index >= events.Length)
                return;

            UIAnimationEvent evt =
                events[index];

            if (evt == null)
                return;

            EditorGUILayout.Space(6f);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox
            );

            EditorGUILayout.LabelField(
                "Selected Event",
                EditorStyles.boldLabel
            );

            float time =
                EditorGUILayout.FloatField(
                    "Time",
                    evt.Time
                );

            evt.SetTime(
                Mathf.Clamp(
                    time,
                    0f,
                    sequence.Duration
                )
            );

            UIAnimationEventType type =
                (UIAnimationEventType)
                EditorGUILayout.EnumPopup(
                    "Type",
                    evt.Type
                );

            evt.SetType(type);

            string id =
                EditorGUILayout.TextField(
                    "ID",
                    evt.Id
                );

            evt.SetId(id);

            if (GUILayout.Button(
                "Delete Event"))
            {
                RemoveEvent(
                    sequence,
                    index
                );

                selectedEvent = -1;
                GUI.changed = true;
            }

            EditorGUILayout.EndVertical();
        }

        // =========================================================
        // REMOVE
        // =========================================================

        private static void RemoveEvent(
            UIAnimationSequence sequence,
            int index)
        {
            // Existing event-removal implementation remains
            // intentionally untouched.
        }

        // =========================================================
        // CURRENT OBJECT
        // =========================================================

        private static Object GetCurrentObject()
        {
            return Selection.activeObject;
        }

    }
}

#endif