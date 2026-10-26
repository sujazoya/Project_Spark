using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectSpark.AI
{
    /// <summary>
    /// Add-on only. Does not modify Piper, the Animator, or body bones.
    /// In the Editor, use the component's "Bake Facial Curves" context menu once
    /// after assigning the 11 imported clips. Baked curves work in builds.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class SparkLipSyncAddon : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private Transform animationRoot;
        [SerializeField] private Transform faceArmatureRoot;
        [Header("11 imported animation clips")]
        [SerializeField] private AnimationClip ah, ai, ee, eh, fv, mbp, oo, ooh, rSn, s, defaultPose;
        [Header("Existing voice system (read-only observation)")]
        [SerializeField] private SparkAIVoiceController voiceController;
        [SerializeField] private AudioSource speechAudioSource;
        [SerializeField, Range(0.02f, 0.25f)] private float transitionSeconds = 0.07f;
        [SerializeField] private bool debugLog;
        [Header("Manual test (Play mode, when not speaking)")]
        [SerializeField] private bool manualTest;
        [SerializeField] private SparkVisemeScheduler.Pose testPose = SparkVisemeScheduler.Pose.Ah;

        [Serializable]
        private class CurveChannel
        {
            public string path;
            public string property;
            public AnimationCurve curve;
        }
        [Serializable]
        private class PoseData
        {
            public List<CurveChannel> channels = new List<CurveChannel>();
        }
        [SerializeField, HideInInspector] private PoseData[] bakedPoses;

        private readonly Dictionary<string, Transform> facialPaths = new Dictionary<string, Transform>();
        private readonly List<SparkVisemeScheduler.Cue> cues = new List<SparkVisemeScheduler.Cue>();
        private readonly Dictionary<string, float> smoothed = new Dictionary<string, float>();
        private string pendingText = "";
        private AudioClip activeClip;
        private bool initialized;

        private AnimationClip[] Clips => new[] { ah, ai, ee, eh, fv, mbp, oo, ooh, rSn, s, defaultPose };

#if UNITY_EDITOR
        [ContextMenu("Bake Facial Curves (after assigning clips)")]
        private void BakeFacialCurves()
        {
            if (animationRoot == null || faceArmatureRoot == null || !faceArmatureRoot.IsChildOf(animationRoot))
            {
                Debug.LogError("[SPARK LIPSYNC] Set Animation Root and nested Face Armature Root first.", this);
                return;
            }
            var clips = Clips;
            var result = new PoseData[clips.Length];
            int accepted = 0, ignored = 0;
            for (int i = 0; i < clips.Length; i++)
            {
                result[i] = new PoseData();
                if (clips[i] == null) { Debug.LogError("[SPARK LIPSYNC] Missing clip at index " + i, this); return; }
                foreach (var binding in AnimationUtility.GetCurveBindings(clips[i]))
                {
                    // Only direct transform position, rotation, scale channels.
                    if (binding.type != typeof(Transform) || !IsSupported(binding.propertyName)) { ignored++; continue; }
                    Transform target = string.IsNullOrEmpty(binding.path) ? animationRoot : animationRoot.Find(binding.path);
                    // Never write to the attachment root or anything outside the facial subtree.
                    if (target == null || target == faceArmatureRoot || !target.IsChildOf(faceArmatureRoot)) { ignored++; continue; }
                    var curve = AnimationUtility.GetEditorCurve(clips[i], binding);
                    if (curve == null) continue;
                    result[i].channels.Add(new CurveChannel { path = binding.path, property = binding.propertyName, curve = new AnimationCurve(curve.keys) });
                    accepted++;
                }
                Debug.Log("[SPARK LIPSYNC] " + clips[i].name + ": " + result[i].channels.Count + " facial transform curves", this);
            }
            bakedPoses = result;
            EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            Debug.Log("[SPARK LIPSYNC] Baked " + accepted + " facial channels; ignored " + ignored + " unrelated/unsupported channels. No animation was applied to the live character.", this);
        }
#endif
        private static bool IsSupported(string property)
        {
            return property.StartsWith("m_LocalPosition.", StringComparison.Ordinal) ||
                   property.StartsWith("m_LocalRotation.", StringComparison.Ordinal) ||
                   property.StartsWith("m_LocalScale.", StringComparison.Ordinal);
        }
        private void Awake()
        {
            if (animationRoot == null || faceArmatureRoot == null || !faceArmatureRoot.IsChildOf(animationRoot))
            {
                Debug.LogError("[SPARK LIPSYNC] Invalid hierarchy references.", this); return;
            }
            if (bakedPoses == null || bakedPoses.Length != 11)
            {
                Debug.LogError("[SPARK LIPSYNC] No baked data. In Edit Mode, open component menu (three dots) > Bake Facial Curves.", this); return;
            }
            foreach (var pose in bakedPoses)
            {
                if (pose == null) continue;
                foreach (var ch in pose.channels)
                    if (!facialPaths.ContainsKey(ch.path)) facialPaths[ch.path] = animationRoot.Find(ch.path);
            }
            initialized = true;
        }
        private void OnEnable()
        {
            if (voiceController != null) voiceController.VoiceRequestCreated += OnVoiceRequest;
        }
        private void OnDisable()
        {
            if (voiceController != null) voiceController.VoiceRequestCreated -= OnVoiceRequest;
            activeClip = null;
            smoothed.Clear();
        }
        private void OnVoiceRequest(SparkAIVoiceRequest request)
        {
            if (request.IsValid) pendingText = request.Text;
        }
        private void Update()
        {
            if (!initialized || speechAudioSource == null) return;
            var now = speechAudioSource.isPlaying ? speechAudioSource.clip : null;
            if (now == activeClip) return;
            activeClip = now;
            cues.Clear();
            smoothed.Clear();
            if (now == null) return;
            string text = !string.IsNullOrWhiteSpace(pendingText) ? pendingText : (voiceController != null ? voiceController.CurrentText : "");
            cues.AddRange(SparkVisemeScheduler.Build(text, now.length));
            if (debugLog) Debug.Log("[SPARK LIPSYNC] Playback started, cues: " + cues.Count, this);
        }
        private void LateUpdate()
        {
            if (!initialized) return;
            bool speaking = speechAudioSource != null && speechAudioSource.isPlaying && activeClip != null && speechAudioSource.clip == activeClip;
            if (!speaking && !manualTest) { smoothed.Clear(); return; }
            var pose = speaking
                ? SparkVisemeScheduler.At(cues, speechAudioSource.timeSamples / (float)Mathf.Max(1, activeClip.frequency))
                : testPose;
            int index = (int)pose;
            if (index < 0 || index >= bakedPoses.Length || bakedPoses[index] == null) return;
            float blend = 1f - Mathf.Exp(-Time.deltaTime * 4.60517f / Mathf.Max(0.01f, transitionSeconds));
            // Gather channels first so quaternion components are applied together and normalized.
            var values = new Dictionary<Transform, float[]>();
            var flags = new Dictionary<Transform, bool[]>();
            foreach (var ch in bakedPoses[index].channels)
            {
                if (!facialPaths.TryGetValue(ch.path, out var bone) || bone == null || ch.curve == null) continue;
                string key = ch.path + "|" + ch.property;
                float target = ch.curve.Evaluate(0f);
                if (!smoothed.TryGetValue(key, out float current)) current = ReadComponent(bone, ch.property);
                float next = Mathf.Lerp(current, target, blend);
                smoothed[key] = next;
                if (!values.TryGetValue(bone, out var data))
                {
                    data = new float[10]; values.Add(bone, data);
                    flags.Add(bone, new bool[10]);
                }
                int slot = Slot(ch.property);
                if (slot < 0) continue;
                data[slot] = next;
                flags[bone][slot] = true;
            }
            foreach (var pair in values)
            {
                var bone = pair.Key;
                var d = pair.Value;
                var f = flags[bone];
                Vector3 p = bone.localPosition, scale = bone.localScale;
                Quaternion q = bone.localRotation;
                if (f[0]) p.x = d[0]; if (f[1]) p.y = d[1]; if (f[2]) p.z = d[2];
                if (f[3]) q.x = d[3]; if (f[4]) q.y = d[4]; if (f[5]) q.z = d[5]; if (f[6]) q.w = d[6];
                if (f[7]) scale.x = d[7]; if (f[8]) scale.y = d[8]; if (f[9]) scale.z = d[9];
                bone.localPosition = p;
                if (f[3] || f[4] || f[5] || f[6])
                {
                    float length = Mathf.Sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);
                    if (length > 0.00001f) bone.localRotation = new Quaternion(q.x/length, q.y/length, q.z/length, q.w/length);
                }
                bone.localScale = scale;
            }
        }
        private static int Slot(string property)
        {
            if (property.Length < 2) return -1;
            int axis = property[property.Length - 1] == 'x' ? 0 : property[property.Length - 1] == 'y' ? 1 : property[property.Length - 1] == 'z' ? 2 : property[property.Length - 1] == 'w' ? 3 : -1;
            if (axis < 0) return -1;
            if (property.StartsWith("m_LocalPosition.")) return axis < 3 ? axis : -1;
            if (property.StartsWith("m_LocalRotation.")) return axis + 3;
            if (property.StartsWith("m_LocalScale.")) return axis < 3 ? axis + 7 : -1;
            return -1;
        }
        private static float ReadComponent(Transform bone, string property)
        {
            int slot = Slot(property);
            Vector3 p = bone.localPosition, s = bone.localScale;
            Quaternion q = bone.localRotation;
            switch (slot)
            {
                case 0: return p.x; case 1: return p.y; case 2: return p.z;
                case 3: return q.x; case 4: return q.y; case 5: return q.z; case 6: return q.w;
                case 7: return s.x; case 8: return s.y; case 9: return s.z;
                default: return 0f;
            }
        }
    }
}
