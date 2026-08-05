#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AAAUI;

namespace AAAUI.Editor
{
    internal static class UIAnimationValidator
    {
        public static int DrawProfileValidation(
            UIAnimationProfile profile,
            UIAnimationTarget[] targets)
        {
            if (profile == null)
                return 0;

            int errors = 0;

            errors += ValidateSequence(
                "Open",
                profile.OpenSequence,
                targets
            );

            errors += ValidateSequence(
                "Close",
                profile.CloseSequence,
                targets
            );

            if (profile.LoopSequence != null)
            {
                errors += ValidateSequence(
                    "Loop",
                    profile.LoopSequence,
                    targets
                );
            }

            if (errors > 0)
            {
                EditorGUILayout.HelpBox(
                    errors + " animation validation issue(s) found.",
                    MessageType.Warning
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Profile valid.",
                    MessageType.Info
                );
            }

            return errors;
        }

        private static int ValidateSequence(
            string name,
            UIAnimationSequence sequence,
            UIAnimationTarget[] targets)
        {
            if (sequence == null)
                return 1;

            if (targets == null)
                return 1;

            int errors = 0;

            UIAnimationTrack[] tracks = sequence.Tracks;

            if (tracks == null)
                return 0;

            for (int i = 0; i < tracks.Length; i++)
            {
                UIAnimationTrack track = tracks[i];

                // -------------------------------------------------
                // NULL TRACK
                // -------------------------------------------------

                if (track == null)
                {
                    errors++;
                    continue;
                }

                // -------------------------------------------------
                // TARGET INDEX
                // -------------------------------------------------

                if (track.TargetIndex < 0 ||
                    track.TargetIndex >= targets.Length)
                {
                    errors++;
                    continue;
                }

                UIAnimationTarget target =
                    targets[track.TargetIndex];

                // -------------------------------------------------
                // TARGET ASSIGNMENT
                // -------------------------------------------------

                if (!target.IsAssigned)
                {
                    errors++;
                    continue;
                }

                // -------------------------------------------------
                // STANDARD UI TRACKS
                // -------------------------------------------------

                if (track is FadeTrack &&
                    target.CanvasGroup == null)
                {
                    errors++;
                }

                else if (
                    (track is ScaleTrack ||
                     track is SlideTrack ||
                     track is ShakeTrack) &&
                    target.Transform == null)
                {
                    errors++;
                }

                else if (
                    track is FlashTrack &&
                    target.Graphic == null)
                {
                    errors++;
                }

                // -------------------------------------------------
                // GLOW
                // -------------------------------------------------

                else if (track is GlowTrack glow)
                {
                    errors += ValidateFloatProperty(
                        target,
                        glow.Property
                    );
                }

                // -------------------------------------------------
                // DISSOLVE
                // -------------------------------------------------

                else if (track is DissolveTrack dissolve)
                {
                    errors += ValidateFloatProperty(
                        target,
                        dissolve.Property
                    );
                }

                // -------------------------------------------------
                // GLITCH
                //
                // GlitchTrack now controls multiple properties.
                // It no longer has a single Property field.
                // -------------------------------------------------

                else if (track is GlitchTrack)
                {
                    errors += ValidateFloatProperty(
                        target,
                        "_GlitchAmount"
                    );

                    errors += ValidateFloatProperty(
                        target,
                        "_GlitchSpeed"
                    );

                    errors += ValidateFloatProperty(
                        target,
                        "_GlitchBandScale"
                    );

                    errors += ValidateFloatProperty(
                        target,
                        "_GlitchFrequency"
                    );

                    errors += ValidateFloatProperty(
                        target,
                        "_GlitchBandWidth"
                    );
                }

                // -------------------------------------------------
                // MATERIAL FLOAT
                // -------------------------------------------------

                else if (track is MaterialFloatTrack materialFloat)
                {
                    errors += ValidateFloatProperty(
                        target,
                        materialFloat.Property
                    );
                }

                // -------------------------------------------------
                // MATERIAL COLOR
                // -------------------------------------------------

                else if (track is MaterialColorTrack materialColor)
                {
                    errors += ValidateColorProperty(
                        target,
                        materialColor.Property
                    );
                }

                // -------------------------------------------------
                // TRACK MUST FIT INSIDE SEQUENCE
                // -------------------------------------------------

                if (track.EndTime >
                    sequence.Duration + 0.0001f)
                {
                    errors++;
                }
            }

            return errors;
        }

        // =========================================================
        // FLOAT PROPERTY VALIDATION
        // =========================================================

        private static int ValidateFloatProperty(
            UIAnimationTarget target,
            string property)
        {
            Material material = GetMaterial(target);

            if (material == null)
                return 1;

            if (string.IsNullOrEmpty(property))
                return 1;

            if (!material.HasProperty(property))
                return 1;

            return 0;
        }

        // =========================================================
        // COLOR PROPERTY VALIDATION
        // =========================================================

        private static int ValidateColorProperty(
            UIAnimationTarget target,
            string property)
        {
            Material material = GetMaterial(target);

            if (material == null)
                return 1;

            if (string.IsNullOrEmpty(property))
                return 1;

            if (!material.HasProperty(property))
                return 1;

            return 0;
        }

        // =========================================================
        // MATERIAL RESOLUTION
        // =========================================================

        private static Material GetMaterial(
            UIAnimationTarget target)
        {
            if (target == null)
                return null;

            if (target.Renderer != null)
                return target.Renderer.sharedMaterial;

            if (target.Graphic != null)
                return target.Graphic.material;

            return null;
        }
    }
}
#endif