using UnityEngine;

namespace AAAUI
{
    internal static class TrackPlayer
    {
        public static void Evaluate(UIAnimationTrack track, PlaybackContext context, float time, int propertyId)
        {
            if (track == null) return;
            int index = track.TargetIndex;
            if ((uint)index >= (uint)context.Targets.Length) return;

            UIAnimationTarget target = context.Targets[index];
            float w = track.EvaluateWeight(time);

            if (track is FadeTrack fade)
            {
                if (target.CanvasGroup != null)
                    target.CanvasGroup.alpha = Mathf.LerpUnclamped(fade.From, fade.To, w);
                return;
            }

            if (track is ScaleTrack scale)
            {
                if (target.Transform != null)
                    target.Transform.localScale = Vector3.LerpUnclamped(scale.From, scale.To, w);
                return;
            }

            if (track is SlideTrack slide)
            {
                if (target.Transform != null)
                    target.Transform.localPosition = context.OriginalPositions[index] +
                        Vector3.LerpUnclamped(slide.From, slide.To, w);
                return;
            }

            if (track is ShakeTrack shake)
            {
                if (target.Transform != null)
                {
                    float phase = (time - shake.StartTime) * shake.Frequency;
                    float x = Noise(phase, shake.Seed, 0) * shake.Amplitude.x;
                    float y = Noise(phase, shake.Seed, 1) * shake.Amplitude.y;
                    float z = Noise(phase, shake.Seed, 2) * shake.Amplitude.z;
                    target.Transform.localPosition = context.OriginalPositions[index] + new Vector3(x, y, z) * w;
                }
                return;
            }

            if (track is FlashTrack flash)
            {
                if (target.Graphic != null)
                    target.Graphic.color = Color.LerpUnclamped(flash.From, flash.To, w);
                return;
            }

            if (track is GlowTrack glow)
            {
                SetFloat(context, index, propertyId, Mathf.LerpUnclamped(glow.From, glow.To, w));
                return;
            }

            if (track is DissolveTrack dissolve)
            {
                SetFloat(context, index, propertyId, Mathf.LerpUnclamped(dissolve.From, dissolve.To, w));
                return;
            }

            if (track is GlitchTrack glitch)
            {
                SetFloat(context, index, propertyId, Mathf.LerpUnclamped(glitch.From, glitch.To, w));
                return;
            }

            if (track is MaterialFloatTrack materialFloat)
            {
                SetFloat(context, index, propertyId,
                    Mathf.LerpUnclamped(materialFloat.From, materialFloat.To, w));
                return;
            }

            if (track is MaterialColorTrack materialColor)
            {
                SetColor(context, index, propertyId,
                    Color.LerpUnclamped(materialColor.From, materialColor.To, w));
            }
        }

        private static void SetFloat(PlaybackContext context, int index, int id, float value)
        {
            if (id < 0) return;
            MaterialState state = context.Materials[index];
            if (state.Renderer != null)
            {
                state.SetFloat(id, value);
            }
            else
            {
                Material material = state.GetWritableMaterial();
                if (material != null && material.HasProperty(id)) material.SetFloat(id, value);
            }
        }

        private static void SetColor(PlaybackContext context, int index, int id, Color value)
        {
            if (id < 0) return;
            MaterialState state = context.Materials[index];
            if (state.Renderer != null)
            {
                state.SetColor(id, value);
            }
            else
            {
                Material material = state.GetWritableMaterial();
                if (material != null && material.HasProperty(id)) material.SetColor(id, value);
            }
        }

        private static float Noise(float t, int seed, int channel)
        {
            return Mathf.PerlinNoise(t + seed * 13.17f + channel * 31.71f, seed * 7.93f + channel * 11.11f) * 2f - 1f;
        }
    }
}