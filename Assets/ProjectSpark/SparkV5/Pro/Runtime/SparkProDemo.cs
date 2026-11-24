using UnityEngine;
namespace ProjectSpark.SparkV5.Pro
{
    // Test scene only. No dependence on input-system packages or game UI.
    public sealed class SparkProDemo : MonoBehaviour
    {
        [SerializeField] SparkProCharacter character;public Transform target;
        public void Configure(SparkProCharacter value){character=value;}
        void OnGUI(){if(!character)return;GUILayout.BeginArea(new Rect(12,12,245,Mathf.Min(Screen.height-24,600)),GUI.skin.box);GUILayout.Label("SPARK V5 PRO / CHARACTER LAB");GUILayout.Label("Actual state: "+character.State);if(GUILayout.Button("Clear warning"))character.SetWarning(false);
            foreach(SparkState s in System.Enum.GetValues(typeof(SparkState))){if(GUILayout.Button(s.ToString()))character.SetState(s);}
            if(GUILayout.Button("Point at test target"))character.PointAt(target);if(GUILayout.Button("Wave"))character.PlayGesture(SparkGesture.Wave);if(GUILayout.Button("Grasp pose"))character.PlayGesture(SparkGesture.Grasp);if(GUILayout.Button("Interrupt"))character.Interrupt();GUILayout.EndArea();}
    }
}
