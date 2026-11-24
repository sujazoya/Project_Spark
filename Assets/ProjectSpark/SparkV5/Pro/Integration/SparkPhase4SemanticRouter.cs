using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectSpark.SparkV5.Pro
{
    // Optional Phase 4 adapter. Only the player text and fixed candidate descriptions cross the loopback boundary.
    internal sealed class SparkPhase4SemanticRouter
    {
        internal const string Endpoint = "http://127.0.0.1:8767/route";
        [Serializable] private sealed class Candidate { public string id, description; }
        [Serializable] private sealed class Request
        {
            public string requestId, question;
            public Candidate[] candidates;
            public bool allowCloud, restricted;
        }
        [Serializable] private sealed class Reply { public string requestId, candidateId, provider; }

        private static readonly Candidate[] Candidates =
        {
            new() { id = "inspect", description = "Identify the selected electrical component or describe its terminals." },
            new() { id = "diagnose", description = "Explain why a simulator circuit or objective may be failing and suggest a check." },
            new() { id = "objectives", description = "Report unfinished objectives in the active game mission." },
            new() { id = "search", description = "Find an authored Project Spark lesson or mission by topic." },
            new() { id = "review", description = "Suggest a review from the learner's existing evidence. Does not start or grade a lesson." },
            new() { id = "campaign", description = "Describe the current campaign or chapter." }
        };

        private static readonly HttpClient Client = new(new HttpClientHandler
        {
            UseProxy = false, AllowAutoRedirect = false
        }) { MaxResponseContentBufferSize = 4096 };

        internal static bool TrySelect(string json, string id, bool allowCloud, out SparkIntent intent)
        {
            intent = SparkIntent.Legacy;
            if (string.IsNullOrWhiteSpace(json)) return false;
            Reply reply;
            try { reply = JsonUtility.FromJson<Reply>(json); }
            catch (ArgumentException) { return false; }
            if (reply == null || !string.Equals(reply.requestId, id, StringComparison.Ordinal) ||
                !(reply.provider == "ollama" || reply.provider == "llamacpp" || reply.provider == "openai" && allowCloud)) return false;
            intent = reply.candidateId switch
            {
                "inspect" => SparkIntent.Inspect, "diagnose" => SparkIntent.Diagnose,
                "objectives" => SparkIntent.Objectives, "search" => SparkIntent.Search,
                "review" => SparkIntent.Revision, "campaign" => SparkIntent.Campaign, _ => SparkIntent.Legacy
            };
            return intent != SparkIntent.Legacy;
        }

        internal static async Task<string> Select(string turnId, string question, bool allowCloud, CancellationToken token)
        {
            string json = JsonUtility.ToJson(new Request
            {
                requestId = turnId, question = question, candidates = Candidates, allowCloud = allowCloud, restricted = false
            });
            try
            {
                using var body = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await Client.PostAsync(Endpoint, body, token).ConfigureAwait(false);
                return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : null;
            }
            catch (Exception error) when (error is HttpRequestException or OperationCanceledException) { return null; }
        }
    }
}
