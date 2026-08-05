using UnityEngine;

namespace AAAUI
{
    internal static class TrackPlayer
    {
        public static void Evaluate(
            UIAnimationTrack track,
            PlaybackContext context,
            float time,
            int propertyId)
        {
            if (track == null || context == null)
                return;

            int index = track.TargetIndex;

            if ((uint)index >= (uint)context.Targets.Length)
                return;

            UIAnimationTarget target =
                context.Targets[index];

            float w = track.EvaluateWeight(time);

            // =====================================================
            // FADE
            // =====================================================

            if (track is FadeTrack fade)
            {
                if (target.CanvasGroup != null)
                {
                    target.CanvasGroup.alpha =
                        Mathf.LerpUnclamped(
                            fade.From,
                            fade.To,
                            w
                        );
                }

                return;
            }

            // =====================================================
            // SCALE
            // =====================================================

            if (track is ScaleTrack scale)
            {
                if (target.Transform != null)
                {
                    target.Transform.localScale =
                        Vector3.LerpUnclamped(
                            scale.From,
                            scale.To,
                            w
                        );
                }

                return;
            }

            // =====================================================
            // SLIDE
            // =====================================================

            if (track is SlideTrack slide)
            {
                if (target.Transform != null)
                {
                    target.Transform.localPosition =
                        context.OriginalPositions[index] +
                        Vector3.LerpUnclamped(
                            slide.From,
                            slide.To,
                            w
                        );
                }

                return;
            }

            // =====================================================
            // SHAKE
            // =====================================================

            if (track is ShakeTrack shake)
            {
                if (target.Transform != null)
                {
                    float phase =
                        (time - shake.StartTime) *
                        shake.Frequency;

                    float x =
                        Noise(
                            phase,
                            shake.Seed,
                            0
                        ) *
                        shake.Amplitude.x;

                    float y =
                        Noise(
                            phase,
                            shake.Seed,
                            1
                        ) *
                        shake.Amplitude.y;

                    float z =
                        Noise(
                            phase,
                            shake.Seed,
                            2
                        ) *
                        shake.Amplitude.z;

                    target.Transform.localPosition =
                        context.OriginalPositions[index] +
                        new Vector3(x, y, z) * w;
                }

                return;
            }

            // =====================================================
            // FLASH
            // =====================================================

            if (track is FlashTrack flash)
            {
                if (target.Graphic != null)
                {
                    target.Graphic.color =
                        Color.LerpUnclamped(
                            flash.From,
                            flash.To,
                            w
                        );
                }

                return;
            }

            // =====================================================
            // GLOW
            // =====================================================

            if (track is GlowTrack glow)
            {
                UIPropertyWriter.SetFloat(
                    context,
                    index,
                    glow.PropertyId,
                    Mathf.LerpUnclamped(
                        glow.From,
                        glow.To,
                        w
                    )
                );

                return;
            }

            // =====================================================
            // DISSOLVE
            // =====================================================

            if (track is DissolveTrack dissolve)
            {
                UIPropertyWriter.SetFloat(
                    context,
                    index,
                    dissolve.PropertyId,
                    Mathf.LerpUnclamped(
                        dissolve.From,
                        dissolve.To,
                        w
                    )
                );

                return;
            }

            // =====================================================
            // GLITCH
            // =====================================================

            if (track is GlitchTrack glitch)
            {
                UIPropertyWriter.SetFloat(
                    context,
                    index,
                    Shader.PropertyToID("_GlitchAmount"),
                    Mathf.LerpUnclamped(
                        glitch.AmountFrom,
                        glitch.AmountTo,
                        w
                    )
                );

                UIPropertyWriter.SetFloat(
                    context,
                    index,
                    Shader.PropertyToID("_GlitchSpeed"),
                    Mathf.LerpUnclamped(
                        glitch.SpeedFrom,
                        glitch.SpeedTo,
                        w
                    )
                );

                UIPropertyWriter.SetFloat(
                    context,
                    index,
                    Shader.PropertyToID("_GlitchBandScale"),
                    Mathf.LerpUnclamped(
                        glitch.BandScaleFrom,
                        glitch.BandScaleTo,
                        w
                    )
                );

                UIPropertyWriter.SetFloat(
                    context,
                    index,
                    Shader.PropertyToID("_GlitchFrequency"),
                    Mathf.LerpUnclamped(
                        glitch.FrequencyFrom,
                        glitch.FrequencyTo,
                        w
                    )
                );

                UIPropertyWriter.SetFloat(
                    context,
                    index,
                    Shader.PropertyToID("_GlitchBandWidth"),
                    Mathf.LerpUnclamped(
                        glitch.BandWidthFrom,
                        glitch.BandWidthTo,
                        w
                    )
                );

                return;
            }

            // =====================================================
            // MATERIAL FLOAT
            // =====================================================

            if (track is MaterialFloatTrack materialFloat)
            {
                UIPropertyWriter.SetFloat(
                    context,
                    index,
                    materialFloat.PropertyId,
                    Mathf.LerpUnclamped(
                        materialFloat.From,
                        materialFloat.To,
                        w
                    )
                );

                return;
            }

            // =====================================================
            // MATERIAL COLOR
            // =====================================================

            if (track is MaterialColorTrack materialColor)
            {
                UIPropertyWriter.SetColor(
                    context,
                    index,
                    materialColor.PropertyId,
                    Color.LerpUnclamped(
                        materialColor.From,
                        materialColor.To,
                        w
                    )
                );

                return;
            }
        }

        // =========================================================
        // NOISE
        // =========================================================

        private static float Noise(
            float t,
            int seed,
            int channel)
        {
            return Mathf.PerlinNoise(
                t +
                seed * 13.17f +
                channel * 31.71f,

                seed * 7.93f +
                channel * 11.11f
            ) * 2f - 1f;
        }
    }
}