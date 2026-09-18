#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.Gameplay.Editor
{
    [CustomEditor(typeof(SparkSwitch))]
    public sealed class SparkSwitchEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();

            SparkSwitch sparkSwitch =
                (SparkSwitch)target;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Live Electrical State",
                EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle(
                    "Input Connected",
                    sparkSwitch.HasInputConnection);

                EditorGUILayout.Toggle(
                    "Output Connected",
                    sparkSwitch.HasOutputConnection);

                EditorGUILayout.Toggle(
                    "Both Connected",
                    sparkSwitch.HasBothConnections);

                EditorGUILayout.Space(4);

                EditorGUILayout.Toggle(
                    "Switch Closed",
                    sparkSwitch.IsClosed);

                EditorGUILayout.Toggle(
                    "Conducting",
                    sparkSwitch.IsActuallyConducting);

                EditorGUILayout.Toggle(
                    "Has Voltage",
                    sparkSwitch.HasVoltage);

                EditorGUILayout.Toggle(
                    "Has Current",
                    sparkSwitch.HasCurrent);

                EditorGUILayout.Toggle(
                    "Power Flow",
                    sparkSwitch.HasPowerFlow);

                EditorGUILayout.Space(4);

                EditorGUILayout.FloatField(
                    "Voltage",
                    sparkSwitch.Voltage);

                EditorGUILayout.FloatField(
                    "Current",
                    sparkSwitch.Current);

                EditorGUILayout.FloatField(
                    "Power",
                    sparkSwitch.Power);

                EditorGUILayout.EnumPopup(
                    "Conduction",
                    sparkSwitch.Conduction);
            }

            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}

#endif