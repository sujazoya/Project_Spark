#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ProjectSpark.AI.Tutorial;
using ProjectSpark.Gameplay;
using ProjectSpark.Gameplay.RedundancyCitadel;
using ProjectSpark.UI.VisualTeaching;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectSpark.SparkV5.Pro
{
    public static class SparkPhase4RuntimeChecks
    {
        public static void Contracts(Action<string> report)
        {
            string Route(string id, string tool, string provider) =>
                $"{{\"requestId\":\"{id}\",\"candidateId\":\"{tool}\",\"provider\":\"{provider}\"}}";
            Check(SparkPhase4SemanticRouter.TrySelect(Route("turn", "inspect", "ollama"), "turn", false, out var intent) &&
                intent == SparkIntent.Inspect, "Allowed read candidate.");
            Check(!SparkPhase4SemanticRouter.TrySelect(Route("other", "inspect", "ollama"), "turn", false, out _) &&
                !SparkPhase4SemanticRouter.TrySelect(Route("turn", "inspect", "openai"), "turn", false, out _) &&
                !SparkPhase4SemanticRouter.TrySelect(Route("turn", "place", "ollama"), "turn", true, out _),
                "Stale turn, unconsented cloud, and mutation candidates rejected.");
            Check(SparkPhase4TeachingContract.CanCreditPractical(true, true, true, true, true, 8, 9, true, true, true) &&
                !SparkPhase4TeachingContract.CanCreditPractical(true, true, false, true, true, 8, 9, true, true, true) &&
                !SparkPhase4TeachingContract.CanCreditPractical(true, true, true, true, true, 8, 8, true, true, true) &&
                !SparkPhase4TeachingContract.CanCreditPractical(false, true, true, true, true, 8, 9, true, true, true),
                "Independent practical policy rejects assisted, stale and unrelated evidence.");
            Check(SparkPhase4TeachingContract.ReviewAfterDays(4, true) == 14 &&
                SparkPhase4TeachingContract.ReviewAfterDays(4, false) == 1, "Spaced review policy.");
            report("PASS Phase 4 contract checks for allowlist, session, cloud consent, practical freshness, assisted-attempt rejection and revision.");
        }

        public static async Task Campaign(LevelGamePlayManager manager, SparkProExecutive executive, Action<string> report,
            Func<Func<bool>, string, Task> wait)
        {
            int chapters = 0;
            foreach (var chapter in manager.Campaign.Chapters.Where(item => item))
            {
                int slot = chapter.Levels.Where(level => level).Select(manager.GetLevelIndex).First(index => index >= 0);
                var selected = manager.GetLevel(slot);
                var clock = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    Check(manager.StartLevel(slot), "Native level start.");
                    await wait(() =>
                    {
                        var level = SparkAILevelContext.Capture(manager);
                        return level.Definition == selected && level.HasEvaluation && manager.TargetResults.Count > 0;
                    }, "chapter " + chapter.ChapterNumber);
                    report($"PASS Chapter {chapter.ChapterNumber}, Level {selected.LevelNumber}: matching loaded layout and {manager.TargetResults.Count} evaluator rows, {clock.ElapsedMilliseconds} ms.");
                    chapters++;
                }
                catch (Exception ex) { report($"FAIL Chapter {chapter.ChapterNumber}, Level {selected.LevelNumber}: {ex.Message}"); }
            }
            report($"Chapter coverage {chapters}/{manager.Campaign.Chapters.Count(chapter => chapter)} with native loader and evaluator.");
            await Practical(manager, executive, report, wait);
        }

        private static async Task Practical(LevelGamePlayManager manager, SparkProExecutive executive, Action<string> report,
            Func<Func<bool>, string, Task> wait)
        {
            int slot = manager.GetCampaignLevelIndices().Single(index => manager.GetLevel(index).LevelNumber == 113);
            Check(manager.StartLevel(slot), "Start native RDX-21 training board.");
            SparkLevel113Process board = null;
            await wait(() =>
            {
                var context = SparkAILevelContext.Capture(manager);
                return context.HasEvaluation && context.Layout.TryGetComponent(out board) && manager.HasCurrentElectricalEvaluation &&
                    manager.TargetResults.Any(row => row.targetId == "l113_1" && !row.satisfied);
            }, "Level 113 practical baseline");
            var bank = ScriptableObject.CreateInstance<SparkAIAssessmentBank>();
            var item = new SparkAssessmentItem
            {
                itemId = "rdx-station-trial", kind = SparkAssessmentItemKind.VerifiedPractical,
                requiredLevelId = "level_114", requiredObjectiveId = "l113_1", requiredTargetType = SparkLevelTarget.TargetType.ComponentPowered
            };
            bank.items = new[] { item };
            var fixture = new GameObject("Isolated assessment UI fixture");
            fixture.SetActive(false);
            var examiner = fixture.AddComponent<SparkAILearningPathAssessment>();
            Set(examiner, "banks", new[] { bank });
            Set(examiner, "levelManager", manager);
            // This advanced board uses a continuity/placement proof. Native solve freshness is still mandatory.
            Set(examiner, "requireFreshSolverFrame", false);
            fixture.SetActive(true);
            try
            {
                examiner.BeginPractical();
                Check(!examiner.PracticalActive, "Unrelated authored level rejected.");
                item.requiredLevelId = "level_113";
                examiner.BeginPractical();
                Check(examiner.PracticalActive, "Correct pending authored target accepted.");
                examiner.VerifyPractical();
                Check(examiner.PracticalActive && !Credited(examiner), "Clicking verify cannot pass the practical.");
                executive.BindSession(manager, examiner);
                executive.Ask("demonstrate the switch");
                Check(executive.LastAnswer.Contains("Independent examination"), "Exam blocks assistance.");
                int baseline = manager.LastEvaluatedSolveRevision;
                for (int i = 0; i < board.parts.Length; i++)
                {
                    board.SelectFootprint(i);
                    board.SelectPart(i);
                    Check(board.FitPart(), $"Fit original station {i}.");
                    board.InspectContacts();
                    Check(board.Solder(), $"Secure original station {i}.");
                    if (i % 22 == 0) await Task.Yield();
                }
                await wait(() => manager.SatisfiedTargetCount >= 1 && Credited(examiner), "actual RDX assembly credited after new solve");
                Check(manager.LastEvaluatedSolveRevision > baseline, "Assessment credits a later evaluator-issued solver revision.");
                report("PASS native Level 113 practical: wrong mission and manual verification rejected; independent assembly earned evaluator credit after a fresh solve.");
            }
            finally
            {
                executive.BindSession(manager, Object.FindObjectsByType<SparkAILearningPathAssessment>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(other => other != examiner));
                fixture.SetActive(false);
                Object.Destroy(fixture);
                Object.Destroy(bank);
            }
        }

        private static bool Credited(SparkAILearningPathAssessment examiner)
        {
            var values = (Array)examiner.GetType().GetField("attempts", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(examiner);
            var attempt = values.GetValue(0);
            return ((bool[])attempt.GetType().GetField("correct").GetValue(attempt))[0];
        }

        public static async Task Semantic(LevelGamePlayManager manager, SparkProExecutive executive, Action<string> report,
            Func<Func<bool>, string, Task> wait)
        {
            const string question = "Could you give me a rundown of the tasks awaiting attention?";
            var listener = new TcpListener(IPAddress.Loopback, 8767);
            listener.Start();
            executive.SemanticEnabled = true;
            try
            {
                var response = Reply(listener, "objectives");
                executive.Ask(question);
                await wait(() => response.IsCompleted && !executive.IsProcessing, "allowlisted candidate result");
                Check(response.IsCompletedSuccessfully && executive.SemanticStatus == "Validated candidate" &&
                    executive.LastAnswer.Contains("objective", StringComparison.OrdinalIgnoreCase), "Cortex formats the model-selected objective request.");
                var unsupported = Reply(listener, "place");
                executive.Ask(question);
                await wait(() => unsupported.IsCompleted && !executive.IsProcessing, "unregistered candidate fallback");
                Check(unsupported.IsCompletedSuccessfully && executive.SemanticStatus == "Offline fallback", "Unknown candidate falls back offline.");
                var late = Reply(listener, "objectives", 500);
                executive.Ask(question);
                executive.Interrupt();
                await wait(() => !executive.IsProcessing, "interruption");
                Check(executive.SemanticStatus == "Cancelled", "Interrupt cancels late model response.");
                report("PASS loopback semantic integration with a controlled provider response, deterministic fallback, and interruption cancellation. Fixture, not model inference.");
            }
            finally { executive.SemanticEnabled = false; listener.Stop(); }
        }

        private static async Task Reply(TcpListener listener, string candidate, int delay = 0)
        {
            try
            {
                using var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);
                int length = 0;
                string line;
                while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync().ConfigureAwait(false)))
                    if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                        length = int.Parse(line.Substring(15));
                var body = new char[length];
                await reader.ReadBlockAsync(body, 0, length).ConfigureAwait(false);
                string id = Regex.Match(new string(body), @"""requestId""\s*:\s*""([a-z0-9:]+)""").Groups[1].Value;
                Check(id.Length > 0, "Expected bounded HTTP request ID.");
                if (delay > 0) await Task.Delay(delay).ConfigureAwait(false);
                string json = $"{{\"requestId\":\"{id}\",\"candidateId\":\"{candidate}\",\"provider\":\"ollama\"}}";
                byte[] payload = Encoding.UTF8.GetBytes(json);
                byte[] headers = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {payload.Length}\r\nConnection: close\r\n\r\n");
                await stream.WriteAsync(headers, 0, headers.Length).ConfigureAwait(false);
                await stream.WriteAsync(payload, 0, payload.Length).ConfigureAwait(false);
            }
            catch (Exception error) when (delay > 0 && (error is IOException or SocketException or ObjectDisposedException)) { }
        }

        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        private static void Check(bool passed, string text) { if (!passed) throw new InvalidOperationException(text); }
    }
}
#endif
