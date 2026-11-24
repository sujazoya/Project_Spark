using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.SparkV5
{
    public enum SparkV5Mood { Idle, Listening, Thinking, Speaking, Teaching, Warning, Success }
    [DisallowMultipleComponent]
    public sealed class SparkV5Face : MonoBehaviour
    {
        [SerializeField] AudioSource voice;
        [SerializeField] Transform focus;
        [SerializeField] SparkV5Mood mood;
        readonly Dictionary<string, Transform> joints = new Dictionary<string, Transform>(StringComparer.Ordinal);
        readonly Dictionary<Transform, Vector3> restScale = new Dictionary<Transform, Vector3>();
        readonly Dictionary<Transform, Quaternion> restRotation = new Dictionary<Transform, Quaternion>();
        readonly float[] audio = new float[256];
        float nextBlink, blinkStart = -10f, speech;
        public void SetMood(SparkV5Mood value) => mood = value;
        public void SetVoice(AudioSource value) => voice = value;
        public void FocusOn(Transform target) => focus = target;
        public void InterruptSpeech() { if (voice) voice.Stop(); speech = 0; mood = SparkV5Mood.Listening; }
        void Awake()
        {
            foreach (var joint in GetComponentsInChildren<Transform>(true))
            {
                if (!joints.ContainsKey(joint.name)) joints.Add(joint.name, joint);
                restScale[joint] = joint.localScale;
                restRotation[joint] = joint.localRotation;
            }
            nextBlink = Time.unscaledTime + 2f;
        }
        void Size(string name, Vector3 scale, float k)
        {
            if (joints.TryGetValue(name, out var j)) j.localScale = Vector3.Lerp(j.localScale, Vector3.Scale(restScale[j], scale), k);
        }
        void Turn(string name, Vector3 angles, float k)
        {
            if (joints.TryGetValue(name, out var j)) j.localRotation = Quaternion.Slerp(j.localRotation, restRotation[j] * Quaternion.Euler(angles), k);
        }
        void LateUpdate()
        {
            float now = Time.unscaledTime, dt = Time.unscaledDeltaTime, k = 1f - Mathf.Exp(-15f * dt);
            if (now >= nextBlink) { blinkStart = now; nextBlink = now + UnityEngine.Random.Range(2.8f, 5.1f); }
            float phase = (now - blinkStart) / .18f;
            float blink = phase >= 0 && phase <= 1 ? 1f - .94f * Mathf.Sin(Mathf.PI * phase) : 1f;
            float amp = 0;
            if (voice && voice.isPlaying)
            {
                voice.GetOutputData(audio, 0);
                for (int i = 0; i < audio.Length; ++i) amp += audio[i] * audio[i];
                amp = Mathf.Clamp01(Mathf.Sqrt(amp / audio.Length) * 9f);
            }
            speech = Mathf.Lerp(speech, amp, k);
            float eye = mood == SparkV5Mood.Thinking ? .78f : mood == SparkV5Mood.Success ? .83f : 1f;
            Size("Eye_L", new Vector3(1, blink * eye, 1), k);
            Size("Eye_R", new Vector3(1, blink * eye, 1), k);
            Size("MouthPivot", new Vector3(1f - .22f * speech, 1f + .9f * speech, 1f), k);
            float pitch = 0, yaw = 0;
            if (focus && joints.TryGetValue("Head", out var head) && head.parent)
            {
                Vector3 d = head.parent.InverseTransformDirection(focus.position - head.position).normalized;
                pitch = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(d.y, -1f, 1f)) * Mathf.Rad2Deg, -15, 15);
                yaw = Mathf.Clamp(Mathf.Atan2(d.x, Mathf.Max(.01f, d.z)) * Mathf.Rad2Deg, -30, 30);
            }
            Turn("Head", new Vector3(pitch, yaw, mood == SparkV5Mood.Listening ? 5 : mood == SparkV5Mood.Thinking ? -6 : 0), k);
            float wave = mood == SparkV5Mood.Teaching || mood == SparkV5Mood.Speaking ? 14f + 8f * Mathf.Sin(now * 3f) : 0f;
            Turn("Shoulder_L", new Vector3(-wave * .5f, 0, -wave), k);
            Turn("Shoulder_R", new Vector3(-wave * .5f, 0, wave), k);
        }
    }
}
