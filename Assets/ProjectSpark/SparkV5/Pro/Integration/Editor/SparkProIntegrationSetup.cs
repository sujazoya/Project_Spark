using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectSpark.AI;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.Circuit;
using ProjectSpark.UI.VisualTeaching;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkProIntegrationSetup
    {
        static T Unique<T>() where T:Component{var list=UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);if(list.Length==1)return list[0];Debug.LogWarning("[Spark Pro] Expected one scene "+typeof(T).Name+", found "+list.Length+". Assign manually.");return null;}
        static T Ensure<T>(GameObject go) where T:Component{var value=go.GetComponent<T>();return value?value:Undo.AddComponent<T>(go);}
        static void Assign(UnityEngine.Object item,string field,UnityEngine.Object value){if(!value)return;var so=new SerializedObject(item);var p=so.FindProperty(field);if(p==null){Debug.LogWarning("[Spark Pro] Missing field "+field+" in "+item.name);return;}if(p.objectReferenceValue)return;p.objectReferenceValue=value;so.ApplyModifiedProperties();EditorUtility.SetDirty(item);}
        static T Read<T>(UnityEngine.Object item,string field) where T:UnityEngine.Object{if(!item)return null;var p=new SerializedObject(item).FindProperty(field);return p==null?null:p.objectReferenceValue as T;}
        [MenuItem("Project Spark/Spark V5 Pro/Bind Selected Scene Character")]
        public static void Bind(){var selected=Selection.activeGameObject;if(!selected||EditorUtility.IsPersistent(selected))throw new InvalidOperationException("Select the instantiated SparkV5Pro character in your laboratory scene.");var character=selected.GetComponentInParent<SparkProCharacter>();if(!character)throw new InvalidOperationException("Selection has no SparkProCharacter.");var root=character.gameObject;
            foreach(var existing in UnityEngine.Object.FindObjectsByType<SparkProExecutive>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(existing.gameObject!=root)throw new InvalidOperationException("Another SparkProExecutive exists in the scene. Use exactly one active executive.");
            var executive=Ensure<SparkProExecutive>(root);var mic=Ensure<SparkProVoiceInput>(root);var academy=Ensure<SparkProAcademy>(root);var hologram=Ensure<SparkProHologram>(root);var cloud=Ensure<SparkProCloudExplainer>(root);
            Assign(executive,"character",character);Assign(executive,"microphone",mic);Assign(executive,"academy",academy);Assign(executive,"hologram",hologram);Assign(executive,"cloud",cloud);Assign(mic,"character",character);
            Assign(executive,"assistant",Unique<SparkAIUnifiedAssistant>());Assign(executive,"world",Unique<SparkAIBrain>());Assign(executive,"selection",Unique<SparkSelectionController>());
            var voice=Unique<SparkAIVoiceController>();var bridge=Unique<SparkAITTSBridge>();Assign(executive,"voice",voice);Assign(executive,"tts",bridge);
            var provider=Read<MonoBehaviour>(bridge,"providerComponent");var playback=Read<AudioSource>(provider,"audioSource");if(!playback)playback=Read<AudioSource>(voice,"audioSource");if(playback){Assign(executive,"playbackSource",playback);Assign(character,"voice",playback);}else Debug.LogWarning("[Spark Pro] Assign the actual TTS provider playback AudioSource to the Executive; lip sync needs that exact source.");
            var catalog=AssetDatabase.LoadAssetAtPath<SparkProGameCatalog>("Assets/ProjectSpark/SparkV5/Pro/Generated/SparkProGameCatalog.asset");Assign(executive,"catalog",catalog);if(!catalog)Debug.LogWarning("[Spark Pro] Compile Game Knowledge, then assign the catalog.");
            string[] knowledge=AssetDatabase.FindAssets("t:SparkAIKnowledge");if(knowledge.Length==1)Assign(executive,"knowledge",AssetDatabase.LoadAssetAtPath<SparkAIKnowledge>(AssetDatabase.GUIDToAssetPath(knowledge[0])));else Debug.LogWarning("[Spark Pro] Assign the intended SparkAIKnowledge asset manually.");
            if(Camera.main)Assign(executive,"playerFocus",Camera.main.transform);foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name=="TeachingProjectionAnchor"){Assign(hologram,"projectionAnchor",t);break;}
            EditorSceneManager.MarkSceneDirty(root.scene);Selection.activeGameObject=root;Debug.Log("[Spark Pro] Scene integration component references assigned where unambiguous. Review the Inspector and save the scene. Microphone and online service remain opt-in.");
        }
        [MenuItem("Project Spark/Spark V5 Pro/Run Core Contract Checks")]
        public static void Test(){int count=0;void Check(bool ok,string name){if(!ok)throw new InvalidOperationException("Spark Pro contract failed: "+name);count++;}
            var wav=SparkProVoiceInput.MakeWave(new short[]{0,100,-100},16000);Check(wav.Length==50,"WAV length");Check(System.Text.Encoding.ASCII.GetString(wav,0,4)=="RIFF","WAV signature");Check(BitConverter.ToInt32(wav,24)==16000,"WAV sample rate");
            Check(SparkProAcademy.TryNumber("twenty five",out double a)&&a==25,"spoken integer");Check(SparkProAcademy.TryNumber("one hundred and twenty three point four",out double b)&&Math.Abs(b-123.4)<.00001,"spoken decimal");Check(!SparkProAcademy.TryNumber("voltage is high",out _),"non-numeric rejection");
            var bank=ScriptableObject.CreateInstance<SparkAIAssessmentBank>();try{bank.items=new[]{new SparkAssessmentItem{kind=SparkAssessmentItemKind.MultipleChoice},new SparkAssessmentItem{kind=SparkAssessmentItemKind.VerifiedPractical}};Check(!SparkProAssessmentGate.AllRequiredPracticalsPassed(bank,new[]{true,false}),"required practical cannot be skipped");Check(SparkProAssessmentGate.AllRequiredPracticalsPassed(bank,new[]{false,true}),"practical evidence recognized");Check(!SparkProAssessmentGate.AllRequiredPracticalsPassed(bank,new[]{true}),"mismatched attempt rejected");}finally{UnityEngine.Object.DestroyImmediate(bank);}
            Debug.Log("[Spark Pro] "+count+" core contract checks passed. This is not a substitute for Play Mode and device testing.");
        }
    }
}
