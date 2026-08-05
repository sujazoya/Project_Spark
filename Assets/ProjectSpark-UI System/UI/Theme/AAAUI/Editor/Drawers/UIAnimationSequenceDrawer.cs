#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AAAUI.Editor
{
    internal static class UIAnimationSequenceDrawer
    {
        public static void Draw(SerializedProperty sequence)
        {
            if (sequence == null)
                return;

            SerializedProperty duration =
                sequence.FindPropertyRelative("duration");

          

            SerializedProperty tracks =
                sequence.FindPropertyRelative("tracks");

            // =========================================================
            // DURATION
            // =========================================================

            EditorGUILayout.PropertyField(
                duration,
                new GUIContent("Duration"));

            DrawEvents(sequence);
            EditorGUILayout.Space(5);           

            // =========================================================
            // TRACKS
            // =========================================================

            EditorGUILayout.LabelField(
                "Tracks",
                EditorStyles.boldLabel);

            if (tracks == null)
            {
                EditorGUILayout.HelpBox(
                    "Tracks property could not be found.",
                    MessageType.Error);

                return;
            }

            for (int i = 0; i < tracks.arraySize; i++)
            {
                SerializedProperty track =
                    tracks.GetArrayElementAtIndex(i);

                DrawTrack(tracks, track, i);
            }

            EditorGUILayout.Space(4f);

            // =========================================================
            // ADD TRACK
            // =========================================================

            if (GUILayout.Button("+ Add Track"))
            {
                ShowTrackMenu(tracks, sequence);
            }
        }

        // =============================================================
        // DRAW TRACK
        // =============================================================

        private static void DrawTrack(
            SerializedProperty tracks,
            SerializedProperty track,
            int index)
        {
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            string title = GetTrackName(track);

            EditorGUILayout.LabelField(
                title,
                EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                "Remove",
                GUILayout.Width(65f)))
            {
                tracks.DeleteArrayElementAtIndex(index);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3f);

            // ---------------------------------------------------------
            // DRAW THE ACTUAL SERIALIZED REFERENCE
            // ---------------------------------------------------------

            if (track.managedReferenceValue != null)
            {
                EditorGUILayout.PropertyField(
                    track,
                    GUIContent.none,
                    true);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "This track has no type assigned.",
                    MessageType.Warning);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(3f);
        }

        // =============================================================
        // TRACK NAME
        // =============================================================

        private static string GetTrackName(
            SerializedProperty track)
        {
            if (track == null)
                return "Track";

            if (track.managedReferenceValue
                is UIAnimationTrack typed)
            {
                return typed.DisplayName;
            }

            return "Track";
        }

        // =============================================================
        // TRACK MENU
        // =============================================================

        private static void ShowTrackMenu(
            SerializedProperty tracks,
            SerializedProperty sequence)
        {
            GenericMenu menu = new GenericMenu();

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Visual/Fade",
                () => new FadeTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Visual/Flash",
                () => new FlashTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Transform/Scale",
                () => new ScaleTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Transform/Slide",
                () => new SlideTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Transform/Shake",
                () => new ShakeTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Shader/Glow",
                () => new GlowTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Shader/Dissolve",
                () => new DissolveTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Shader/Glitch",
                () => new GlitchTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Shader/Material Float",
                () => new MaterialFloatTrack());

            AddMenuItem(
                menu,
                tracks,
                sequence,
                "Shader/Material Color",
                () => new MaterialColorTrack());

            menu.ShowAsContext();
        }

        // =============================================================
        // ADD MENU ITEM
        // =============================================================

        private static void RemoveEvent(
    SerializedProperty sequence,
    int index)
        {
            SerializedProperty events =
                sequence.FindPropertyRelative("events");

            if (events == null)
                return;

            if (index < 0 ||
                index >= events.arraySize)
                return;

            events.DeleteArrayElementAtIndex(index);

            sequence.serializedObject.ApplyModifiedProperties();
        }

        private static void AddMenuItem(
            GenericMenu menu,
            SerializedProperty tracks,
            SerializedProperty sequence,
            string path,
            System.Func<UIAnimationTrack> creator)
        {
            menu.AddItem(
                new GUIContent(path),
                false,
                () =>
                {
                    int index = tracks.arraySize;

                    tracks.InsertArrayElementAtIndex(index);

                    SerializedProperty element =
                        tracks.GetArrayElementAtIndex(index);

                    element.managedReferenceValue =
                        creator();

                    sequence.serializedObject.ApplyModifiedProperties();

                    
                });
        }
        private static void DrawEvents(SerializedProperty sequence)
        {
            SerializedProperty events =
                sequence.FindPropertyRelative("events");

            if (events == null)
                return;

            EditorGUILayout.Space(8f);

            EditorGUILayout.LabelField(
                "Events",
                EditorStyles.boldLabel
            );

            for (int i = 0; i < events.arraySize; i++)
            {
                SerializedProperty element =
                    events.GetArrayElementAtIndex(i);

                if (element == null)
                    continue;

                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox
                );

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(
                    $"Event {i + 1}",
                    EditorStyles.boldLabel
                );

                if (GUILayout.Button(
                    "Remove",
                    GUILayout.Width(65f)))
                {
                    events.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(
                    element,
                    new GUIContent(),
                    true
                );

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ Add Event"))
            {
                int index = events.arraySize;

                events.InsertArrayElementAtIndex(index);

                SerializedProperty element =
                    events.GetArrayElementAtIndex(index);

                if (element != null)
                {
                    SerializedProperty time =
                        element.FindPropertyRelative("time");

                    SerializedProperty type =
                        element.FindPropertyRelative("type");

                    SerializedProperty id =
                        element.FindPropertyRelative("id");

                    if (time != null)
                        time.floatValue = 0f;

                    if (type != null)
                        type.enumValueIndex =
                            (int)UIAnimationEventType.Marker;

                    if (id != null)
                        id.stringValue = "Marker";
                }
            }
        }
    }
}

#endif