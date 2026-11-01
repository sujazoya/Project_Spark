using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.UI.VisualTeaching
{
    /// <summary>
    /// Local, non-accredited learning certificates. Reads (never writes) Guided Paths
    /// study/practice stage keys and Assessment passed/best-score keys. Each assessment
    /// bank must contain a verified practical question. Never changes a circuit,
    /// level result, shop, training credit, or unlocked game content.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkAICourseCertification : MonoBehaviour
    {
        private const string StagePrefix = "ProjectSpark.GuidedPaths.v1/";
        private const string PassedPrefix = "ProjectSpark.Assessments.Passed.v1/";
        private const string ScorePrefix = "ProjectSpark.Assessments.Best.v1/";
        private const int MaximumRecords = 250;

        [Header("Existing course and assessment assets")]
        [SerializeField] private SparkGuidedLearningPathAsset[] courses = Array.Empty<SparkGuidedLearningPathAsset>();
        [SerializeField] private SparkAIAssessmentBank[] assessments = Array.Empty<SparkAIAssessmentBank>();
        [Header("Optional existing panels")]
        [SerializeField] private SparkAIGuidedLearningPaths pathsPanel;
        [SerializeField] private SparkAILearningPathAssessment assessmentPanel;

        [Header("Editor generated UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text courseTitle;
        [SerializeField] private TMP_Text courseDescription;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text requirementText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text certificateText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_InputField learnerNameInput;
        [SerializeField] private Image stageFill;
        [SerializeField] private Button issueButton;
        [SerializeField] private Button exportButton;

        [Serializable]
        public sealed class CertificateRecord
        {
            public string certificateId;
            public string courseId;
            public string courseTitle;
            public string assessmentId;
            public string learnerName;
            public string issuedUtc;
            public string curriculumFingerprint;
            public int completedStages;
            public int completedPracticalStages;
            public int bestAssessmentPercent;
            public int requiredAssessmentPercent;
        }

        [Serializable]
        private sealed class CertificateDatabase
        {
            public int version = 1;
            public List<CertificateRecord> records = new List<CertificateRecord>();
        }

        private sealed class Audit
        {
            public SparkGuidedLearningPathAsset course;
            public SparkAIAssessmentBank bank;
            public string fingerprint;
            public string error;
            public int total;
            public int done;
            public int practices;
            public int practicesDone;
            public int score;
            public bool assessmentPassed;
            public bool ready;
        }

        private CertificateDatabase database = new CertificateDatabase();
        private int selectedIndex;
        private string message = "Select a course and check its requirements.";
        private string savedName = "Learner";
        private bool loaded;
        private bool storageReadOnly;
        private bool restoredFromBackup;

        private static string Folder => Path.Combine(Application.persistentDataPath, "ProjectSpark", "Certificates");
        private static string SaveFile => Path.Combine(Folder, "course_certificates_v1.json");

        private void Awake()
        {
            Load();
            selectedIndex = Mathf.Clamp(PlayerPrefs.GetInt("ProjectSpark.Certificates.Selected.v1", 0),
                0, Mathf.Max(0, courses == null ? 0 : courses.Length - 1));
            savedName = PlayerPrefs.GetString("ProjectSpark.Certificates.Name.v1", "Learner");
            if (learnerNameInput != null) learnerNameInput.SetTextWithoutNotify(savedName);
            if (panel != null) panel.SetActive(false);
        }

        public void Show()
        {
            Load();
            if (panel != null) panel.SetActive(true);
            Refresh();
        }
        public void Hide() { if (panel != null) panel.SetActive(false); }
        public void Toggle() { if (panel != null && panel.activeSelf) Hide(); else Show(); }
        public void PreviousCourse() { MoveCourse(-1); }
        public void NextCourse() { MoveCourse(1); }

        private void MoveCourse(int offset)
        {
            if (courses == null || courses.Length == 0) { Refresh(); return; }
            selectedIndex = (selectedIndex + offset + courses.Length) % courses.Length;
            PlayerPrefs.SetInt("ProjectSpark.Certificates.Selected.v1", selectedIndex);
            message = "Course selected. Press REFRESH to recheck saved requirements.";
            Refresh();
        }

        public void OpenLearningPaths()
        {
            if (pathsPanel == null)
                pathsPanel = FindFirstObjectByType<SparkAIGuidedLearningPaths>(FindObjectsInactive.Include);
            if (pathsPanel != null) pathsPanel.Show();
            else { message = "Guided Learning Paths is not installed in this scene."; Refresh(); }
        }

        public void OpenAssessment()
        {
            if (assessmentPanel == null)
                assessmentPanel = FindFirstObjectByType<SparkAILearningPathAssessment>(FindObjectsInactive.Include);
            if (assessmentPanel != null) assessmentPanel.Show();
            else { message = "Learning Path Assessment is not installed in this scene."; Refresh(); }
        }

        private SparkGuidedLearningPathAsset SelectedCourse =>
            courses != null && selectedIndex >= 0 && selectedIndex < courses.Length ? courses[selectedIndex] : null;

        private static string StageKey(string courseId, string stageId)
        {
            string a = courseId ?? string.Empty;
            string b = stageId ?? string.Empty;
            return StagePrefix + a.Length + ":" + a + "/" + b.Length + ":" + b;
        }

        private static string AssessmentKey(string prefix, string bankId)
        {
            string value = bankId ?? string.Empty;
            return prefix + value.Length + ":" + value;
        }

        private SparkAIAssessmentBank FindBank(string courseId)
        {
            if (assessments == null) return null;
            SparkAIAssessmentBank found = null;
            foreach (SparkAIAssessmentBank candidate in assessments)
            {
                if (candidate == null || !string.Equals(candidate.courseId, courseId, StringComparison.Ordinal)) continue;
                if (found != null) return null; // Ambiguous mapping must be configured explicitly.
                found = candidate;
            }
            return found;
        }

        private static string Fingerprint(SparkGuidedLearningPathAsset course, SparkAIAssessmentBank bank)
        {
            var text = new StringBuilder();
            text.Append("cert-schema:1|").Append(course.pathId).Append('|').Append(bank.bankId)
                .Append('|').Append(bank.courseId).Append('|').Append(bank.passingPercent);
            foreach (SparkLearningPathStage stage in course.stages)
                text.Append('|').Append(stage.id).Append(':').Append((int)stage.kind)
                    .Append(':').Append(stage.requiredTargetType)
                    .Append(':').Append(stage.lesson != null ? stage.lesson.lessonId : "no-lesson");
            foreach (SparkAssessmentItem question in bank.items)
                text.Append('|').Append(question.itemId).Append(':').Append((int)question.kind)
                    .Append(':').Append(question.requiredTargetType);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()));
                var hex = new StringBuilder(64);
                foreach (byte b in digest) hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                return hex.ToString();
            }
        }

        private Audit Check()
        {
            var audit = new Audit { course = SelectedCourse, score = -1 };
            SparkGuidedLearningPathAsset course = audit.course;
            if (course == null || string.IsNullOrWhiteSpace(course.pathId) ||
                course.stages == null || course.stages.Length == 0)
            {
                audit.error = "A valid editable course asset is required.";
                return audit;
            }
            if (courses != null)
            {
                int duplicates = 0;
                foreach (var other in courses)
                    if (other != null && string.Equals(other.pathId, course.pathId, StringComparison.Ordinal)) duplicates++;
                if (duplicates != 1) { audit.error = "Duplicate course ID. Assign unique course assets."; return audit; }
            }

            audit.bank = FindBank(course.pathId);
            if (audit.bank == null || string.IsNullOrWhiteSpace(audit.bank.bankId) ||
                audit.bank.items == null || audit.bank.items.Length == 0)
            {
                audit.error = "Assign exactly one valid assessment bank with matching courseId.";
                return audit;
            }
            // Since the assessment keys are bank-ID scoped, do not accept a duplicate ID.
            int duplicateBanks = 0;
            foreach (SparkAIAssessmentBank bank in assessments)
                if (bank != null && string.Equals(bank.bankId, audit.bank.bankId, StringComparison.Ordinal)) duplicateBanks++;
            if (duplicateBanks != 1) { audit.error = "Duplicate assessment bank ID."; return audit; }
            if (audit.bank.passingPercent < 1 || audit.bank.passingPercent > 100)
            {
                audit.error = "Set an assessment passing threshold from 1 to 100%.";
                return audit;
            }

            HashSet<string> stageIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (SparkLearningPathStage stage in course.stages)
            {
                if (stage == null || string.IsNullOrWhiteSpace(stage.id) || !stageIds.Add(stage.id))
                {
                    audit.error = "Every course stage needs a unique, nonblank ID.";
                    return audit;
                }
                if (stage.kind == SparkLearningPathStageKind.Lesson && stage.lesson == null)
                {
                    audit.error = "A course lesson asset is missing: " + stage.title;
                    return audit;
                }
                audit.total++;
                if (stage.kind == SparkLearningPathStageKind.VerifiedPractice) audit.practices++;
                if (PlayerPrefs.GetInt(StageKey(course.pathId, stage.id), 0) == 1)
                {
                    audit.done++;
                    if (stage.kind == SparkLearningPathStageKind.VerifiedPractice) audit.practicesDone++;
                }
            }
            if (audit.practices < 1)
            {
                audit.error = "A course needs at least one evaluator-verified practice stage.";
                return audit;
            }

            int practicalQuestions = 0;
            HashSet<string> questionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (SparkAssessmentItem item in audit.bank.items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.itemId) || !questionIds.Add(item.itemId))
                {
                    audit.error = "Assessment items need unique, nonblank IDs.";
                    return audit;
                }
                if (item.kind == SparkAssessmentItemKind.VerifiedPractical) practicalQuestions++;
            }
            if (practicalQuestions == 0)
            {
                audit.error = "A certificate assessment must include an evaluator-verified practical exam.";
                return audit;
            }
            audit.score = PlayerPrefs.GetInt(AssessmentKey(ScorePrefix, audit.bank.bankId), -1);
            audit.assessmentPassed = PlayerPrefs.GetInt(AssessmentKey(PassedPrefix, audit.bank.bankId), 0) == 1 &&
                audit.score >= audit.bank.passingPercent;
            audit.fingerprint = Fingerprint(course, audit.bank);
            audit.ready = audit.done == audit.total && audit.practicesDone == audit.practices && audit.assessmentPassed;
            return audit;
        }

        private CertificateRecord FindExisting(Audit audit)
        {
            if (audit == null || audit.course == null || audit.bank == null || database == null || database.records == null) return null;
            for (int i = database.records.Count - 1; i >= 0; i--)
            {
                CertificateRecord record = database.records[i];
                if (record != null && string.Equals(record.courseId, audit.course.pathId, StringComparison.Ordinal) &&
                    string.Equals(record.assessmentId, audit.bank.bankId, StringComparison.Ordinal) &&
                    string.Equals(record.curriculumFingerprint, audit.fingerprint, StringComparison.Ordinal))
                    return record;
            }
            return null;
        }

        private static string SafeLearnerName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Learner";
            var cleaned = new StringBuilder();
            foreach (char c in raw.Trim())
            {
                if (cleaned.Length >= 64) break;
                if (!char.IsControl(c) && c != '<' && c != '>') cleaned.Append(c);
            }
            return string.IsNullOrWhiteSpace(cleaned.ToString()) ? "Learner" : cleaned.ToString().Trim();
        }

        public void IssueCertificate()
        {
            Load();
            Audit audit = Check();
            if (!string.IsNullOrEmpty(audit.error)) { message = audit.error; Refresh(); return; }
            if (storageReadOnly) { message = "Certificate database is unreadable. Back up and repair the file before issuing."; Refresh(); return; }
            CertificateRecord existing = FindExisting(audit);
            if (existing != null)
            {
                message = "This course revision already has a local certificate. Use EXPORT.";
                Refresh(); return;
            }
            if (!audit.ready)
            {
                message = "Not eligible: finish every course stage and pass the practical assessment.";
                Refresh(); return;
            }
            if (database.records.Count >= MaximumRecords)
            {
                message = "Certificate record limit reached; back up your existing certificates first.";
                Refresh(); return;
            }
            string name = SafeLearnerName(learnerNameInput != null ? learnerNameInput.text : savedName);
            string newId = "PS-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
            CertificateRecord cert = new CertificateRecord
            {
                certificateId = newId,
                courseId = audit.course.pathId,
                courseTitle = audit.course.title,
                assessmentId = audit.bank.bankId,
                learnerName = name,
                issuedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                curriculumFingerprint = audit.fingerprint,
                completedStages = audit.total,
                completedPracticalStages = audit.practicesDone,
                bestAssessmentPercent = audit.score,
                requiredAssessmentPercent = audit.bank.passingPercent
            };
            database.records.Add(cert);
            if (!SaveDatabase())
            {
                database.records.Remove(cert);
                message = "Could not save certificate. See Console and check storage permissions.";
                Refresh(); return;
            }
            savedName = name;
            PlayerPrefs.SetString("ProjectSpark.Certificates.Name.v1", savedName);
            PlayerPrefs.Save();
            message = "Local course certificate issued. Use EXPORT to save a text and JSON copy.";
            Refresh();
        }

        public void ExportCertificate()
        {
            Load();
            Audit audit = Check();
            CertificateRecord cert = FindExisting(audit);
            if (cert == null)
            {
                message = "No local certificate found for this course revision. Complete requirements and ISSUE first.";
                Refresh(); return;
            }
            try
            {
                Directory.CreateDirectory(Folder);
                string prefix = Path.Combine(Folder, cert.certificateId);
                File.WriteAllText(prefix + ".json", JsonUtility.ToJson(cert, true), new UTF8Encoding(false));
                File.WriteAllText(prefix + ".txt", FormatCertificate(cert), new UTF8Encoding(false));
                message = "Certificate exported to: " + Folder;
                Debug.Log("[Spark Certification] Exported: " + prefix + ".txt and .json", this);
            }
            catch (Exception ex)
            {
                message = "Export failed: " + ex.Message;
                Debug.LogWarning("[Spark Certification] " + message, this);
            }
            Refresh();
        }

        private static string FormatCertificate(CertificateRecord c)
        {
            return "PROJECT SPARK - LOCAL COURSE COMPLETION CERTIFICATE\n" +
                   "================================================\n" +
                   "Learner: " + c.learnerName + "\n" +
                   "Course: " + c.courseTitle + "\n" +
                   "Course ID: " + c.courseId + "\n" +
                   "Assessment ID: " + c.assessmentId + "\n" +
                   "Issued (UTC): " + c.issuedUtc + "\n" +
                   "Certificate ID: " + c.certificateId + "\n" +
                   "Completed path stages: " + c.completedStages + "\n" +
                   "Verified practice stages credited: " + c.completedPracticalStages + "\n" +
                   "Saved best assessment score: " + c.bestAssessmentPercent + "%\n" +
                   "Required passing score: " + c.requiredAssessmentPercent + "%\n" +
                   "Curriculum fingerprint: " + c.curriculumFingerprint + "\n\n" +
                   "EVIDENCE: Local Guided Paths stage records and Learning Path Assessment\n" +
                   "pass and score records. Practical credit is issued by the existing\n" +
                   "evaluator-backed training/assessment systems; lesson study is\n" +
                   "self-acknowledged. Prior results are not current circuit readings.\n\n" +
                   "This is a local, player-maintained Project Spark achievement,\n" +
                   "NOT an accredited qualification or tamper-proof credential.\n";
        }

        private static bool TryReadDatabase(string filename, out CertificateDatabase parsed)
        {
            parsed = null;
            if (!File.Exists(filename)) return false;
            try
            {
                CertificateDatabase candidate = JsonUtility.FromJson<CertificateDatabase>(
                    File.ReadAllText(filename, Encoding.UTF8));
                if (candidate == null || candidate.version != 1 || candidate.records == null ||
                    candidate.records.Count > MaximumRecords)
                    return false;
                parsed = candidate;
                return true;
            }
            catch (Exception) { return false; }
        }

        private void Load()
        {
            if (loaded) return;
            loaded = true;
            database = new CertificateDatabase();
            if (!File.Exists(SaveFile))
            {
                CertificateDatabase backupOnly;
                if (TryReadDatabase(SaveFile + ".bak", out backupOnly))
                {
                    database = backupOnly;
                    restoredFromBackup = true;
                    Debug.LogWarning("[Spark Certification] Main save is absent; restored in memory from backup.", this);
                }
                return;
            }
            CertificateDatabase current;
            if (TryReadDatabase(SaveFile, out current))
            {
                database = current;
                return;
            }
            CertificateDatabase backup;
            if (TryReadDatabase(SaveFile + ".bak", out backup))
            {
                database = backup;
                restoredFromBackup = true;
                Debug.LogWarning("[Spark Certification] Corrupt save. Loaded backup; existing files preserved until next issue.", this);
                return;
            }
            storageReadOnly = true;
            Debug.LogError("[Spark Certification] Existing certificate save is unreadable and no valid backup exists. Issuing disabled to preserve the file.", this);
        }

        private bool SaveDatabase()
        {
            if (storageReadOnly) return false;
            try
            {
                Directory.CreateDirectory(Folder);
                string temp = SaveFile + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(database, true), new UTF8Encoding(false));
                if (File.Exists(SaveFile))
                {
                    // Preserve an existing valid backup if the main save was corrupt.
                    string backup = SaveFile + ".bak";
                    if (!restoredFromBackup)
                    {
                        if (File.Exists(backup)) File.Delete(backup);
                        File.Copy(SaveFile, backup);
                    }
                    File.Delete(SaveFile);
                }
                File.Move(temp, SaveFile);
                restoredFromBackup = false;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Spark Certification] Failed to save: " + ex.Message, this);
                return false;
            }
        }

        public void Refresh()
        {
            Load();
            Audit audit = Check();
            CertificateRecord cert = FindExisting(audit);
            int total = courses == null ? 0 : courses.Length;
            if (courseTitle != null)
                courseTitle.text = audit.course != null ? audit.course.title : "NO COURSE ASSETS";
            if (courseDescription != null)
                courseDescription.text = audit.course != null ? audit.course.description :
                    "Run the Guided Learning Paths and Assessment builders first.";
            if (progressText != null)
                progressText.text = "COURSE " + (total == 0 ? 0 : selectedIndex + 1) + "/" + total +
                    "   |   PATH STAGES " + audit.done + "/" + audit.total;
            if (stageFill != null) stageFill.fillAmount = audit.total > 0 ? (float)audit.done / audit.total : 0f;
            if (requirementText != null)
            {
                if (!string.IsNullOrEmpty(audit.error)) requirementText.text = "CONFIGURATION: " + audit.error;
                else
                    requirementText.text =
                        "STUDY + PRACTICE: " + audit.done + "/" + audit.total + " credited\n" +
                        "VERIFIED PRACTICE: " + audit.practicesDone + "/" + audit.practices + " credited\n" +
                        "ASSESSMENT: " + (audit.assessmentPassed ? "PASSED" : "NOT PASSED") +
                        " (BEST " + (audit.score >= 0 ? audit.score.ToString(CultureInfo.InvariantCulture) + "%" : "—") +
                        " / NEED " + audit.bank.passingPercent + "%)\n" +
                        "CERTIFICATION: " + (cert != null ? "ISSUED" : audit.ready ? "ELIGIBLE TO ISSUE" : "NOT READY");
            }
            if (resultText != null)
                resultText.text = storageReadOnly ? "Certificate save unreadable. Protect the file and restore from backup." :
                    audit.error != null ? "Check the course/assessment asset references." :
                    audit.ready ? "All recorded prerequisites are met. This is a local achievement only." :
                    "Complete missing Guided Paths stages and the assessment. Press REFRESH afterwards.";
            if (certificateText != null)
                certificateText.text = cert != null ?
                    "CERTIFICATE " + cert.certificateId + "\nISSUED UTC " + cert.issuedUtc +
                    "\nLEARNER " + cert.learnerName :
                    "No certificate issued for this course revision.";
            if (statusText != null) statusText.text = message;
            if (issueButton != null) issueButton.interactable = !storageReadOnly && string.IsNullOrEmpty(audit.error) && audit.ready && cert == null;
            if (exportButton != null) exportButton.interactable = cert != null;
        }
    }
}
