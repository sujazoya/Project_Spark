using UnityEngine;

namespace AAAUI
{
    public readonly struct UIPropertyValidationResult
    {
        public readonly bool Valid;
        public readonly bool HasMaterial;
        public readonly bool HasProperty;
        public readonly string Message;

        public UIPropertyValidationResult(
            bool valid,
            bool hasMaterial,
            bool hasProperty,
            string message)
        {
            Valid = valid;
            HasMaterial = hasMaterial;
            HasProperty = hasProperty;
            Message = message;
        }
    }

    public static class UIPropertyValidation
    {
        public static UIPropertyValidationResult Validate(
            PlaybackContext context,
            int targetIndex,
            UIPropertyReference reference)
        {
            if (context == null)
            {
                return new UIPropertyValidationResult(
                    false,
                    false,
                    false,
                    "Playback context is null."
                );
            }

            if (reference == null ||
                !reference.IsValid)
            {
                return new UIPropertyValidationResult(
                    false,
                    false,
                    false,
                    "Property is empty."
                );
            }

            if ((uint)targetIndex >=
                (uint)context.Targets.Length)
            {
                return new UIPropertyValidationResult(
                    false,
                    false,
                    false,
                    "Target index is out of range."
                );
            }

            UIAnimationTarget target =
                context.Targets[targetIndex];

            if (!target.IsAssigned)
            {
                return new UIPropertyValidationResult(
                    false,
                    false,
                    false,
                    "Target is not assigned."
                );
            }

            Material material =
                UIMaterialResolver.GetMaterial(target);

            if (material == null)
            {
                return new UIPropertyValidationResult(
                    false,
                    false,
                    false,
                    "Target has no material."
                );
            }

            int propertyId =
                reference.PropertyId;

            if (!material.HasProperty(propertyId))
            {
                return new UIPropertyValidationResult(
                    false,
                    true,
                    false,
                    "Material does not contain property '" +
                    reference.Property +
                    "'."
                );
            }

            return new UIPropertyValidationResult(
                true,
                true,
                true,
                "Property is valid."
            );
        }
    }
}