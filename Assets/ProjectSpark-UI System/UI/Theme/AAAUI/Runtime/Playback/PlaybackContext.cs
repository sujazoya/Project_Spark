using System;
using UnityEngine;

namespace AAAUI
{
    public sealed class PlaybackContext
    {
        public readonly UIAnimationTarget[] Targets;

        public readonly Vector3[] OriginalPositions;
        public readonly Vector3[] OriginalScales;
        public readonly Quaternion[] OriginalRotations;

        public readonly float[] OriginalAlpha;
        public readonly Color[] OriginalColors;

        public readonly MaterialState[] Materials;

        public PlaybackContext(UIAnimationTarget[] targets)
        {
            Targets =
                targets ??
                Array.Empty<UIAnimationTarget>();

            int count = Targets.Length;

            OriginalPositions =
                new Vector3[count];

            OriginalScales =
                new Vector3[count];

            OriginalRotations =
                new Quaternion[count];

            OriginalAlpha =
                new float[count];

            OriginalColors =
                new Color[count];

            Materials =
                new MaterialState[count];

            for (int i = 0; i < count; i++)
            {
                UIAnimationTarget target =
                    Targets[i];

                if (target != null &&
                    target.Transform != null)
                {
                    OriginalPositions[i] =
                        target.Transform.localPosition;

                    OriginalScales[i] =
                        target.Transform.localScale;

                    OriginalRotations[i] =
                        target.Transform.localRotation;
                }
                else
                {
                    OriginalPositions[i] =
                        Vector3.zero;

                    OriginalScales[i] =
                        Vector3.one;

                    OriginalRotations[i] =
                        Quaternion.identity;
                }

                OriginalAlpha[i] =
                    target != null &&
                    target.CanvasGroup != null
                        ? target.CanvasGroup.alpha
                        : 1f;

                OriginalColors[i] =
                    target != null &&
                    target.Graphic != null
                        ? target.Graphic.color
                        : Color.white;

                Materials[i] =
                    new MaterialState(target);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < Materials.Length; i++)
            {
                if (Materials[i] != null)
                    Materials[i].Dispose();
            }
        }
    }
}