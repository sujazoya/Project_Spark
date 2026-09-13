#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace ProjectSpark.UI.Animation.Editor
{
    [CustomEditor(typeof(UIAnimationPreset))]
    public sealed class UIAnimationPresetEditor :
        UnityEditor.Editor
    {
        private SerializedProperty presetId;
        private SerializedProperty description;

        private SerializedProperty usePosition;
        private SerializedProperty hiddenPositionOffset;
        private SerializedProperty positionDuration;
        private SerializedProperty positionEase;

        private SerializedProperty useScale;
        private SerializedProperty hiddenScale;
        private SerializedProperty scaleDuration;
        private SerializedProperty scaleEase;

        private SerializedProperty useRotation;
        private SerializedProperty hiddenRotation;
        private SerializedProperty rotationDuration;
        private SerializedProperty rotationEase;

        private SerializedProperty useAlpha;
        private SerializedProperty hiddenAlpha;
        private SerializedProperty fadeDuration;
        private SerializedProperty fadeEase;

        private SerializedProperty delay;
        private SerializedProperty exitMultiplier;
        private SerializedProperty ignoreTimeScale;

        private SerializedProperty
            disableInteractionDuringAnimation;

        private SerializedProperty
            disableObjectAfterHide;

        private SerializedProperty usePunch;
        private SerializedProperty punchScale;
        private SerializedProperty punchDuration;
        private SerializedProperty punchVibrato;
        private SerializedProperty punchElasticity;

        private SerializedProperty useShake;
        private SerializedProperty shakeStrength;
        private SerializedProperty shakeDuration;
        private SerializedProperty shakeVibrato;
        private SerializedProperty shakeRandomness;

        private SerializedProperty loop;
        private SerializedProperty loopCount;
        private SerializedProperty loopType;

        private bool identity = true;
        private bool position = true;
        private bool scale = true;
        private bool rotation;
        private bool alpha = true;
        private bool timing = true;
        private bool interaction = true;
        private bool punch;
        private bool shake;
        private bool looping;


        private void OnEnable()
        {
            presetId =
                Find("presetId");

            description =
                Find("description");

            usePosition =
                Find("usePosition");

            hiddenPositionOffset =
                Find("hiddenPositionOffset");

            positionDuration =
                Find("positionDuration");

            positionEase =
                Find("positionEase");

            useScale =
                Find("useScale");

            hiddenScale =
                Find("hiddenScale");

            scaleDuration =
                Find("scaleDuration");

            scaleEase =
                Find("scaleEase");

            useRotation =
                Find("useRotation");

            hiddenRotation =
                Find("hiddenRotation");

            rotationDuration =
                Find("rotationDuration");

            rotationEase =
                Find("rotationEase");

            useAlpha =
                Find("useAlpha");

            hiddenAlpha =
                Find("hiddenAlpha");

            fadeDuration =
                Find("fadeDuration");

            fadeEase =
                Find("fadeEase");

            delay =
                Find("delay");

            exitMultiplier =
                Find("exitMultiplier");

            ignoreTimeScale =
                Find("ignoreTimeScale");

            disableInteractionDuringAnimation =
                Find(
                    "disableInteractionDuringAnimation");

            disableObjectAfterHide =
                Find(
                    "disableObjectAfterHide");

            usePunch =
                Find("usePunch");

            punchScale =
                Find("punchScale");

            punchDuration =
                Find("punchDuration");

            punchVibrato =
                Find("punchVibrato");

            punchElasticity =
                Find("punchElasticity");

            useShake =
                Find("useShake");

            shakeStrength =
                Find("shakeStrength");

            shakeDuration =
                Find("shakeDuration");

            shakeVibrato =
                Find("shakeVibrato");

            shakeRandomness =
                Find("shakeRandomness");

            loop =
                Find("loop");

            loopCount =
                Find("loopCount");

            loopType =
                Find("loopType");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawTitle();

            Section(
                "IDENTITY",
                ref identity);

            if (identity)
            {
                Field(presetId);
                Field(description);
            }


            Section(
                "POSITION",
                ref position);

            if (position)
            {
                Toggle(usePosition);

                if (usePosition.boolValue)
                {
                    Field(hiddenPositionOffset);
                    Field(positionDuration);
                    Field(positionEase);
                }
            }


            Section(
                "SCALE",
                ref scale);

            if (scale)
            {
                Toggle(useScale);

                if (useScale.boolValue)
                {
                    Field(hiddenScale);
                    Field(scaleDuration);
                    Field(scaleEase);
                }
            }


            Section(
                "ROTATION",
                ref rotation);

            if (rotation)
            {
                Toggle(useRotation);

                if (useRotation.boolValue)
                {
                    Field(hiddenRotation);
                    Field(rotationDuration);
                    Field(rotationEase);
                }
            }


            Section(
                "ALPHA",
                ref alpha);

            if (alpha)
            {
                Toggle(useAlpha);

                if (useAlpha.boolValue)
                {
                    Field(hiddenAlpha);
                    Field(fadeDuration);
                    Field(fadeEase);
                }
            }


            Section(
                "TIMING",
                ref timing);

            if (timing)
            {
                Field(delay);
                Field(exitMultiplier);
                Toggle(ignoreTimeScale);
            }


            Section(
                "INTERACTION",
                ref interaction);

            if (interaction)
            {
                Toggle(
                    disableInteractionDuringAnimation);

                Toggle(
                    disableObjectAfterHide);
            }


            Section(
                "PUNCH",
                ref punch);

            if (punch)
            {
                Toggle(usePunch);

                if (usePunch.boolValue)
                {
                    Field(punchScale);
                    Field(punchDuration);
                    Field(punchVibrato);
                    Field(punchElasticity);
                }
            }


            Section(
                "SHAKE",
                ref shake);

            if (shake)
            {
                Toggle(useShake);

                if (useShake.boolValue)
                {
                    Field(shakeStrength);
                    Field(shakeDuration);
                    Field(shakeVibrato);
                    Field(shakeRandomness);
                }
            }


            Section(
                "LOOP",
                ref looping);

            if (looping)
            {
                Toggle(loop);

                if (loop.boolValue)
                {
                    Field(loopCount);
                    Field(loopType);
                }
            }


            serializedObject.ApplyModifiedProperties();
        }


        private void DrawTitle()
        {
            EditorGUILayout.Space(5);

            GUIStyle title =
                new GUIStyle(
                    EditorStyles.boldLabel)
                {
                    fontSize = 16
                };

            EditorGUILayout.LabelField(
                "PROJECT SPARK",
                title);

            EditorGUILayout.LabelField(
                "UI ANIMATION PRESET",
                EditorStyles.miniLabel);

            EditorGUILayout.Space(6);
        }


        private static void Section(
            string title,
            ref bool state)
        {
            EditorGUILayout.Space(4);

            state =
                EditorGUILayout.BeginFoldoutHeaderGroup(
                    state,
                    title);

            EditorGUILayout.EndFoldoutHeaderGroup();
        }


        private static void Field(
            SerializedProperty property)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(
                    property);
            }
        }


        private static void Toggle(
            SerializedProperty property)
        {
            if (property != null)
            {
                property.boolValue =
                    EditorGUILayout.ToggleLeft(
                        "Enabled",
                        property.boolValue);
            }
        }


        private SerializedProperty Find(
            string propertyName)
        {
            return serializedObject.FindProperty(
                propertyName);
        }
    }
}

#endif