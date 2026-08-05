#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AAAUI;

namespace AAAUI.Editor
{
    internal static class UIAnimationTrackDrawer
    {
        public static void Draw(SerializedProperty track, UIAnimationSequence sequence)
        {
            if (track == null) return;
            EditorGUILayout.PropertyField(track, new GUIContent("Track"), true);
        }

        public static void AddTrack(SerializedProperty tracks, int type)
        {
            int index = tracks.arraySize;
            tracks.InsertArrayElementAtIndex(index);
            SerializedProperty element = tracks.GetArrayElementAtIndex(index);

            switch (type)
            {
                case 0: element.managedReferenceValue = new FadeTrack(); break;
                case 1: element.managedReferenceValue = new ScaleTrack(); break;
                case 2: element.managedReferenceValue = new SlideTrack(); break;
                case 3: element.managedReferenceValue = new GlowTrack(); break;
                case 4: element.managedReferenceValue = new DissolveTrack(); break;
                case 5: element.managedReferenceValue = new GlitchTrack(); break;
                case 6: element.managedReferenceValue = new ShakeTrack(); break;
                case 7: element.managedReferenceValue = new FlashTrack(); break;
                case 8: element.managedReferenceValue = new MaterialFloatTrack(); break;
                case 9: element.managedReferenceValue = new MaterialColorTrack(); break;
            }
        }
    }
}
#endif