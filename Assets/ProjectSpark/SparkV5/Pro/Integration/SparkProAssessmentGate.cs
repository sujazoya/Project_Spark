using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace ProjectSpark.UI.VisualTeaching
{
    // A new pass record is tied to exact authored bank content. Legacy score-only passes do not qualify.
    public static class SparkProAssessmentGate
    {
        public static string Fingerprint(SparkAIAssessmentBank bank)
        {
            if (!bank) return "";
            // Empty Phase 4 bindings must keep the exact v2 hash used by existing saves.
            string content = Regex.Replace(JsonUtility.ToJson(bank),
                ",\"(?:requiredLevelId|requiredObjectiveId)\":(?:\"\"|null)(?=[,}])", "");
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(content))).Replace("-", "");
        }
        public static string Key(SparkAIAssessmentBank bank,string kind)=>"ProjectSpark.Assessments.Pro.v2/"+kind+"/"+Fingerprint(bank);
        public static bool AllRequiredPracticalsPassed(SparkAIAssessmentBank bank,bool[] correct){if(!bank||bank.items==null||correct==null||bank.items.Length!=correct.Length)return false;bool hasPractical=false;for(int i=0;i<bank.items.Length;i++){var item=bank.items[i];if(item==null)return false;if(item.kind!=SparkAssessmentItemKind.VerifiedPractical)continue;hasPractical=true;if(!correct[i])return false;}return hasPractical;}
        public static void RecordQualifiedPass(SparkAIAssessmentBank bank,int score){if(!bank)return;string key=Key(bank,"score");PlayerPrefs.SetInt(key,Mathf.Max(score,PlayerPrefs.GetInt(key,-1)));PlayerPrefs.SetInt(Key(bank,"pass"),1);}
        public static int QualifiedScore(SparkAIAssessmentBank bank)=>bank?PlayerPrefs.GetInt(Key(bank,"score"),-1):-1;
        public static bool HasQualifiedPass(SparkAIAssessmentBank bank)=>bank&&PlayerPrefs.GetInt(Key(bank,"pass"),0)==1&&QualifiedScore(bank)>=bank.passingPercent;
    }
}
