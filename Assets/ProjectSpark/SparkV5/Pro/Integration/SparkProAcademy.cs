using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ProjectSpark.SparkV5.Pro
{
    // Formative numerical practice. Not an official level evaluator or certification authority.
    [DisallowMultipleComponent]
    public sealed class SparkProAcademy : MonoBehaviour
    {
        [Serializable] public sealed class Evidence
        {
            public string concept;
            public int independent, assisted, incorrect;
            public string lastUtc;
        }

        [Serializable] internal sealed class LessonEvidence
        {
            public string concept, category, observedUtc, dueUtc, misconception;
            public bool correct, assisted;
            public float confidence;
        }
        [Serializable] sealed class Profile
        {
            public int version = 2;
            public List<Evidence> concepts=new List<Evidence>();
            public Draft practice;
            public List<LessonEvidence> observations = new();
        }

        [Serializable] sealed class Draft
        {
            public string concept, question, hint, explanation;
            public double answer;
            public int attempts, hints;
            public bool hinted, paused;
        }
        [SerializeField] string profileId="default";
        [SerializeField] bool persistProfile=true;
        private Profile profile = new();
        private bool active, hinted, paused, storageReadOnly;
        private int attempts, hints;
        private string concept, question, explanation;
        private double answer;
        public bool IsPracticing => active;
        string SavePath => Path.Combine(Application.persistentDataPath,
            "SparkAcademy-" + Regex.Replace(profileId ?? "default", @"[^A-Za-z0-9_-]", "_") + ".json");

        private void Awake()
        {
            if (!persistProfile || !File.Exists(SavePath)) return;
            if (TryLoad(SavePath, out var saved) || TryLoad(SavePath + ".bak", out saved))
            {
                profile = saved;
                RestorePractice();
            }
            else storageReadOnly = true;
        }

        private static bool TryLoad(string path, out Profile result)
        {
            result = null;
            try { if (File.Exists(path)) result = JsonUtility.FromJson<Profile>(File.ReadAllText(path)); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException) { return false; }
            if (result == null || result.version is < 1 or > 2 || result.concepts == null) return false;
            result.version = 2;
            result.observations ??= new List<LessonEvidence>();
            return true;
        }

        private void Save()
        {
            if (!persistProfile || storageReadOnly) return;
            try
            {
                string file = SavePath, temp = file + ".tmp";
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.WriteAllText(temp, JsonUtility.ToJson(profile, true));
                if (File.Exists(file))
                    File.Replace(temp, file, TryLoad(file, out _) ? file + ".bak" : null);
                else File.Move(temp, file);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            { Debug.LogWarning("[Spark Academy] Learner data could not be saved."); }
        }

        internal void RecordLesson(string topic, string category, bool correct, bool usedHelp, string misconception)
        {
            var now = DateTime.UtcNow;
            int independent = profile.observations.Count(item => item.concept == topic && item.category == category &&
                item.correct && !item.assisted);
            profile.observations.Add(new LessonEvidence
            {
                concept = topic, category = category, correct = correct, assisted = usedHelp,
                misconception = misconception, confidence = category == "measurement" ? 1f : usedHelp ? .45f : .7f,
                observedUtc = now.ToString("O"), dueUtc = now.AddDays(ProjectSpark.UI.VisualTeaching.SparkPhase4TeachingContract.ReviewAfterDays(independent + 1, correct && !usedHelp)).ToString("O")
            });
            const int limit = 160;
            if (profile.observations.Count > limit) profile.observations.RemoveRange(0, profile.observations.Count - limit);
            Save();
        }

        internal string RevisionReport()
        {
            var latest = profile.observations.GroupBy(item => (item.concept, item.category))
                .Select(group => group.Last()).OrderBy(item => item.dueUtc, StringComparer.Ordinal).Take(3).ToArray();
            if (latest.Length == 0) return Report();
            string today = DateTime.UtcNow.ToString("O");
            return string.Join("\n", latest.Select(item =>
                $"{item.concept} · {item.category}: {(string.CompareOrdinal(item.dueUtc, today) <= 0 ? "review due" : "review scheduled")}." +
                (string.IsNullOrWhiteSpace(item.misconception) ? "" : " Revisit: " + item.misconception)));
        }

        internal void ResetOptionalLearningObservations()
        {
            profile.observations.Clear();
            Save();
        }

        static readonly string[] Topics={"ohm","power","series","parallel","divider","led resistor","energy","rc time","frequency","charge","efficiency","digital logic"};
        string hintText;
        static string F(double n)=>n.ToString("0.####",CultureInfo.InvariantCulture);
        public string SelectTopic(string request){string q=(request??"").ToLowerInvariant();if(q.Contains("adaptive")||q=="practice"||q.Contains("weakest")){string best=Topics[0];double score=double.NegativeInfinity;foreach(string t in Topics){var e=profile.concepts.Find(x=>x.concept==t);double v=e==null?10:Math.Max(0,4-e.independent)*2+e.incorrect*.3+e.assisted*.2;if(e!=null&&DateTime.TryParse(e.lastUtc,null,DateTimeStyles.RoundtripKind,out var day))v+=Math.Min(4,Math.Max(0,(DateTime.UtcNow-day.ToUniversalTime()).TotalDays));if(v>score){best=t;score=v;}}return best;}
            foreach(string t in new[]{"led resistor","digital logic","rc time","efficiency","frequency","parallel","divider","energy","charge","series","power","ohm"})if(q.Contains(t))return t;return "ohm";}
        public string StartPractice(string topic="ohm"){
            active=true;paused=hinted=false;attempts=hints=0;concept=Array.IndexOf(Topics,topic)>=0?topic:"ohm";
            int r=UnityEngine.Random.Range(1,10)*100;int ma=UnityEngine.Random.Range(1,9)*5;double v=r*ma/1000.0;
            switch(concept){
            case "power": answer=v*ma;question=$"An ideal resistor has {F(v)} volts across it and {ma} milliamps flowing. Calculate power in milliwatts.";hintText="Multiply volts by milliamps to obtain milliwatts.";explanation=$"P = V × I. {F(v)} × {ma} = {F(answer)} mW.";break;
            case "series": int r2=UnityEngine.Random.Range(1,10)*100;answer=r+r2;question=$"Two ideal resistors, {r} and {r2} ohms, are in series. What is the total resistance in ohms?";hintText="Series resistances add.";explanation=$"R = {r} + {r2} = {F(answer)} ohms.";break;
            case "parallel": answer=r/2.0;question=$"Two identical ideal {r} ohm resistors are in parallel. Find their equivalent resistance in ohms.";hintText="For two equal resistances in parallel, equivalent resistance is half one resistance.";explanation=$"1/Rtotal = 1/R + 1/R, so Rtotal = {r}/2 = {F(answer)} ohms.";break;
            case "divider": int supply=UnityEngine.Random.Range(2,13);int lower=UnityEngine.Random.Range(1,5)*100;answer=(double)supply*lower/(r+lower);question=$"An unloaded ideal divider has {supply} V input, {r} ohms on top and {lower} ohms to ground. Find the output voltage; round to two decimals.";hintText="Vout = Vin × Rbottom / (Rtop + Rbottom).";explanation=$"Vout = {supply} × {lower}/({r}+{lower}) ≈ {F(answer)} V.";break;
            case "led resistor": int current=UnityEngine.Random.Range(1,4)*5;int vs=UnityEngine.Random.Range(4,10);answer=(vs-2)*1000.0/current;question=$"Use an ideal constant 2 V LED drop, {vs} V supply and target {current} mA. Calculate the series resistance in ohms, to two decimals. This is a theoretical value, not a component recommendation.";hintText="Subtract the LED drop from the supply first, then divide by current in amperes.";explanation=$"R = ({vs}-2)/({current}/1000) ≈ {F(answer)} ohms. A real design also checks LED ratings, resistor tolerance and dissipation.";break;
            case "energy": int watts=UnityEngine.Random.Range(1,10);int sec=UnityEngine.Random.Range(1,10)*10;answer=watts*sec;question=$"A load consumes constant power of {watts} watts for {sec} seconds. Calculate energy in joules.";hintText="Energy equals power multiplied by time.";explanation=$"E = {watts} × {sec} = {F(answer)} J.";break;
            case "rc time": int kilo=UnityEngine.Random.Range(1,10);int micro=UnityEngine.Random.Range(1,10)*10;answer=kilo*micro;question=$"An ideal first-order RC circuit has {kilo} kiloohms and {micro} microfarads. Calculate its time constant in milliseconds.";hintText="Time constant tau = R × C. Kilohms multiplied by microfarads gives milliseconds.";explanation=$"Tau = {kilo} × {micro} = {F(answer)} ms.";break;
            case "frequency": int hz=UnityEngine.Random.Range(1,11)*100;answer=1000.0/hz;question=$"A periodic signal has frequency {hz} Hz. Calculate its period in milliseconds, rounded to two decimals.";hintText="Period in seconds is the reciprocal of frequency.";explanation=$"T = 1000/{hz} ≈ {F(answer)} ms.";break;
            case "charge": int cap=UnityEngine.Random.Range(1,10)*10;int volts=UnityEngine.Random.Range(1,10);answer=cap*volts;question=$"An ideal capacitor of {cap} microfarads has {volts} V across it. Calculate stored charge in microcoulombs.";hintText="Q = C × V.";explanation=$"Q = {cap} × {volts} = {F(answer)} microcoulombs.";break;
            case "efficiency": int input=UnityEngine.Random.Range(2,9)*10;int output=input-10;answer=100.0*output/input;question=$"A converter consumes {input} W and delivers {output} W. Calculate percent efficiency to two decimals.";hintText="Efficiency is output divided by input, times 100.";explanation=$"Efficiency = 100 × {output}/{input} ≈ {F(answer)} percent.";break;
            case "digital logic": int a=UnityEngine.Random.Range(0,2),b=UnityEngine.Random.Range(0,2);int gate=UnityEngine.Random.Range(0,3);string name=gate==0?"AND":gate==1?"OR":"XOR";answer=gate==0?(a&b):gate==1?(a|b):(a^b);question=$"An ideal {name} gate has inputs {a} and {b}. Give its output, 0 or 1.";hintText=gate==0?"AND is 1 only when both inputs are 1.":gate==1?"OR is 1 if at least one input is 1.":"XOR is 1 if inputs differ.";explanation=$"{hintText} The output is {F(answer)}.";break;
            default: answer=ma;question=$"An ideal {r} ohm resistor has {F(v)} volts across it. Calculate current in milliamps.";hintText="Divide voltage by resistance to get amperes. Multiply by 1000 for milliamps.";explanation=$"I = V/R × 1000 = {F(v)}/{r} × 1000 = {F(answer)} mA.";break;
            }
            SavePractice();
            return ResumePractice();
        }
        public string Hint()
        {
            if (!active) return "No numerical practice is active.";
            hinted = true;
            hints = Mathf.Min(hints + 1, 2);
            SavePractice();
            return hints == 1 ? "Name the quantity the question asks for. Check its unit before choosing an equation." : hintText;
        }

        public string ResumePractice()
        {
            if (!active) return "There is no unfinished practice. Say practice to start a question.";
            paused = false;
            SavePractice();
            return question + " Say answer followed by the number in the requested unit.";
        }

        internal string PausePractice()
        {
            if (!active) return "No practice is active.";
            paused = true;
            SavePractice();
            return "Practice paused. Say “resume the lesson” to continue.";
        }

        private void SavePractice()
        {
            profile.practice = active ? new Draft
            {
                concept = concept, question = question, hint = hintText, explanation = explanation,
                answer = answer, attempts = attempts, hints = hints, hinted = hinted, paused = paused
            } : null;
            Save();
        }

        private void RestorePractice()
        {
            Draft draft = profile.practice;
            if (draft == null || Array.IndexOf(Topics, draft.concept) < 0 ||
                string.IsNullOrWhiteSpace(draft.question) || double.IsNaN(draft.answer) || double.IsInfinity(draft.answer))
                return;
            active = true;
            concept = draft.concept;
            question = draft.question;
            hintText = draft.hint;
            explanation = draft.explanation;
            answer = draft.answer;
            attempts = Mathf.Clamp(draft.attempts, 0, 2);
            hints = Mathf.Clamp(draft.hints, 0, 2);
            hinted = draft.hinted;
            paused = draft.paused;
        }
        public static bool TryNumber(string text,out double value){value=0;string s=(text??"").Trim().ToLowerInvariant();if(Regex.IsMatch(s,@"^[+-]?\d+(?:\.\d+)?$")&&double.TryParse(s,NumberStyles.Float,CultureInfo.InvariantCulture,out value))return !double.IsNaN(value)&&!double.IsInfinity(value);
            string[] ones={"zero","one","two","three","four","five","six","seven","eight","nine","ten","eleven","twelve","thirteen","fourteen","fifteen","sixteen","seventeen","eighteen","nineteen"};string[] tens={"twenty","thirty","forty","fifty","sixty","seventy","eighty","ninety"};int total=0,segment=0;double fraction=0,place=.1;bool decimalMode=false,digit=false,whole=false,neg=false;string[] words=s.Replace('-',' ').Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
            for(int i=0;i<words.Length;i++){string w=words[i];if(w=="minus"&&i==0){neg=true;continue;}if(w=="and"&&whole&&!decimalMode)continue;if(w=="point"&&!decimalMode){decimalMode=true;continue;}int n=Array.IndexOf(ones,w);if(decimalMode){if(n<0||n>9)return false;fraction+=n*place;place*=.1;digit=true;continue;}if(n>=0){segment+=n;whole=true;continue;}int t=Array.IndexOf(tens,w);if(t>=0){segment+=(t+2)*10;whole=true;continue;}if(w=="hundred"&&segment>0&&segment<10){segment*=100;continue;}if(w=="thousand"&&segment>0&&total==0){total=segment*1000;segment=0;continue;}return false;}
            if((!whole&&!digit)||(decimalMode&&!digit))return false;value=(total+segment+fraction)*(neg?-1:1);return true;
        }
        private bool TryPracticeNumber(string text, out double value)
        {
            if (TryNumber(text, out value)) return true;
            var answerUnits = new (string pattern, string dimension, double scale)[]
            {
                (@"milliamps?|milliampere[s]?|mA", "current", .001), (@"amps?|ampere[s]?|A", "current", 1),
                (@"milliwatts?|mW", "power", .001), (@"watts?|W", "power", 1),
                (@"milliseconds?|ms", "time", .001), (@"seconds?|s", "time", 1),
                (@"millivolts?|mV", "voltage", .001), (@"volts?|V", "voltage", 1),
                (@"kiloohms?|kΩ", "resistance", 1000), (@"ohms?|Ω", "resistance", 1),
                (@"microcoulombs?|µC", "charge", 1), (@"joules?|J", "energy", 1), (@"percent|%", "ratio", 1)
            };
            string dimension = concept switch
            {
                "power" => "power", "series" or "parallel" or "led resistor" => "resistance",
                "divider" => "voltage", "rc time" or "frequency" => "time", "energy" => "energy",
                "charge" => "charge", "efficiency" => "ratio", "digital logic" => "", _ => "current"
            };
            double requestedScale = dimension is "current" or "power" or "time" ? .001 : 1;
            foreach (var unit in answerUnits)
            {
                if (unit.dimension != dimension) continue;
                var match = Regex.Match(text, @"^(.+?)\s*(?:" + unit.pattern + ")$", RegexOptions.IgnoreCase);
                if (!match.Success || !TryNumber(match.Groups[1].Value, out value)) continue;
                value *= unit.scale / requestedScale;
                return true;
            }
            return false;
        }

        public string Submit(string text){if(paused)return "Resume the practice before submitting.";if(!active)return "Start a practice question first.";string cleaned=(text??"").Trim().ToLowerInvariant();if(!TryPracticeNumber(cleaned,out double value))return "Give the value in the requested unit, or say the number and its unit.";
            attempts++;var e=profile.concepts.Find(x=>x.concept==concept);if(e==null){e=new Evidence{concept=concept};profile.concepts.Add(e);}e.lastUtc=DateTime.UtcNow.ToString("O");bool ok=Math.Abs(value-answer)<=Math.Max(.01,Math.Abs(answer)*.001);
            if(ok){active=false;if(hinted||attempts>1)e.assisted++;else e.independent++;SavePractice();return (hinted||attempts>1?"Correct with assistance. ":"Correct independently. ")+explanation+" Try another problem to strengthen retention.";}
            e.incorrect++;if(attempts>=3){active=false;SavePractice();return "Let's review the worked solution. "+explanation+" Start another problem and try independently.";}SavePractice();
            bool likelyScaleError = answer != 0 && (Math.Abs(value - answer * 1000) <= Math.Abs(answer) ||
                Math.Abs(value - answer / 1000) <= Math.Abs(answer) / 100000);
            return likelyScaleError ? "Check the conversion between base units and the unit requested in the question." :
                "Not quite. Check the formula and units, then try again. Ask for a practice hint if needed.";
        }
        public string Cancel(){active=false;SavePractice();return "Practice ended. No completion credit was awarded.";}
        public string Report(){if(profile.concepts.Count==0)return "No formative practice evidence yet.";var s=new System.Text.StringBuilder("Formative evidence, not certification. ");foreach(var e in profile.concepts)s.Append(e.concept).Append(": ").Append(e.independent).Append(" independent, ").Append(e.assisted).Append(" assisted, ").Append(e.incorrect).Append(" incorrect attempts. ");return s.ToString();}
    }
}
