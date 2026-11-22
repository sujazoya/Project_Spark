using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level32.B850I.Editor {
 [CustomEditor(typeof(SparkB850IVisualisation))] public sealed class SparkB850IVisualisationEditor:UnityEditor.Editor {
 float amount; public override void OnInspectorGUI(){DrawDefaultInspector();if(!Application.isPlaying){EditorGUILayout.HelpBox("Enter Play Mode to inspect exploded geometry. These are visual controls only.",MessageType.Info);return;}var v=(SparkB850IVisualisation)target;EditorGUI.BeginChangeCheck();amount=EditorGUILayout.Slider("Exploded view",amount,0,1);if(EditorGUI.EndChangeCheck())v.SetExplosion(amount);if(GUILayout.Button("Hide armor"))v.SetArmorVisible(false);if(GUILayout.Button("Show armor"))v.SetArmorVisible(true);if(GUILayout.Button("Rear view"))v.SetRearView(true);if(GUILayout.Button("Front view"))v.SetRearView(false);if(GUILayout.Button("Isolate power"))v.SetIsolation(1);if(GUILayout.Button("Isolate storage"))v.SetIsolation(2);if(GUILayout.Button("Isolate underside"))v.SetIsolation(3);if(GUILayout.Button("Show all"))v.SetIsolation(0);if(GUILayout.Button("Reset view")){amount=0;v.ResetView();}}
 }
}
