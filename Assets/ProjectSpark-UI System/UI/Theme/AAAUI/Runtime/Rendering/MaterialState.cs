using UnityEngine;
using UnityEngine.UI;

namespace AAAUI
{
    public sealed class MaterialState
    {
        private readonly UIAnimationTarget target;

        private Material runtimeMaterial;
        private Material originalMaterial;

        private MaterialPropertyBlock propertyBlock;

        public Renderer Renderer =>
            target != null ? target.Renderer : null;

        public Graphic Graphic =>
            target != null ? target.Graphic : null;

        public Material Original =>
            originalMaterial;

        public Material RuntimeMaterial =>
            runtimeMaterial;

        public MaterialState(UIAnimationTarget animationTarget)
        {
            target = animationTarget;

            if (target == null)
                return;

            Graphic graphic = target.Graphic;

            if (graphic != null)
            {
                originalMaterial =
                    graphic.material;
            }

            Renderer renderer = target.Renderer;

            if (renderer != null)
            {
                if (originalMaterial == null)
                    originalMaterial =
                        renderer.sharedMaterial;

                propertyBlock =
                    new MaterialPropertyBlock();
            }
        }

        // =========================================================
        // PREPARE
        // =========================================================

        public void Prepare()
        {
            if (target == null)
                return;

            Graphic graphic = target.Graphic;

            if (graphic != null)
            {
                EnsureRuntimeMaterial();
            }
        }

        // =========================================================
        // FLOAT
        // =========================================================

        public void SetFloat(
            int propertyId,
            float value)
        {
            if (propertyId < 0)
                return;

            Graphic graphic = target != null
                ? target.Graphic
                : null;

            // -----------------------------------------------------
            // UI / TMP
            // -----------------------------------------------------

            if (graphic != null)
            {
                Material material =
                    EnsureRuntimeMaterial();

                if (material == null)
                    return;

                if (!material.HasProperty(propertyId))
                    return;

                material.SetFloat(
                    propertyId,
                    value);

                return;
            }

            // -----------------------------------------------------
            // Renderer
            // -----------------------------------------------------

            Renderer renderer =
                target != null
                    ? target.Renderer
                    : null;

            if (renderer != null)
            {
                if (propertyBlock == null)
                    propertyBlock =
                        new MaterialPropertyBlock();

                renderer.GetPropertyBlock(
                    propertyBlock);

                propertyBlock.SetFloat(
                    propertyId,
                    value);

                renderer.SetPropertyBlock(
                    propertyBlock);
            }
        }

        // =========================================================
        // COLOR
        // =========================================================

        public void SetColor(
            int propertyId,
            Color value)
        {
            if (propertyId < 0)
                return;

            Graphic graphic = target != null
                ? target.Graphic
                : null;

            // -----------------------------------------------------
            // UI / TMP
            // -----------------------------------------------------

            if (graphic != null)
            {
                Material material =
                    EnsureRuntimeMaterial();

                if (material == null)
                    return;

                if (!material.HasProperty(propertyId))
                    return;

                material.SetColor(
                    propertyId,
                    value);

                return;
            }

            // -----------------------------------------------------
            // Renderer
            // -----------------------------------------------------

            Renderer renderer =
                target != null
                    ? target.Renderer
                    : null;

            if (renderer != null)
            {
                if (propertyBlock == null)
                    propertyBlock =
                        new MaterialPropertyBlock();

                renderer.GetPropertyBlock(
                    propertyBlock);

                propertyBlock.SetColor(
                    propertyId,
                    value);

                renderer.SetPropertyBlock(
                    propertyBlock);
            }
        }

        // =========================================================
        // WRITABLE MATERIAL
        // =========================================================

        public Material GetWritableMaterial()
        {
            if (target == null)
                return null;

            Graphic graphic =
                target.Graphic;

            if (graphic != null)
                return EnsureRuntimeMaterial();

            Renderer renderer =
                target.Renderer;

            if (renderer != null)
                return renderer.sharedMaterial;

            return null;
        }

        // =========================================================
        // CREATE UI MATERIAL INSTANCE
        // =========================================================

        private Material EnsureRuntimeMaterial()
        {
            if (target == null)
                return null;

            Graphic graphic =
                target.Graphic;

            if (graphic == null)
                return null;

            if (runtimeMaterial == null)
            {
                Material source =
                    originalMaterial;

                if (source == null)
                    source = graphic.material;

                if (source == null)
                    return null;

                runtimeMaterial =
                    new Material(source);

                runtimeMaterial.name =
                    source.name +
                    " [AAAUI Runtime]";
            }

            if (graphic.material != runtimeMaterial)
            {
                graphic.material =
                    runtimeMaterial;
            }

            return runtimeMaterial;
        }

        // =========================================================
        // RESTORE
        // =========================================================

        public void Restore()
        {
            if (target == null)
                return;

            Renderer renderer =
                target.Renderer;

            if (renderer != null)
            {
                if (propertyBlock == null)
                    propertyBlock =
                        new MaterialPropertyBlock();

                propertyBlock.Clear();

                renderer.SetPropertyBlock(
                    propertyBlock);
            }

            Graphic graphic =
                target.Graphic;

            if (graphic != null &&
                originalMaterial != null)
            {
                graphic.material =
                    originalMaterial;
            }
        }

        // =========================================================
        // DISPOSE
        // =========================================================

        public void Dispose()
        {
            Graphic graphic =
                target != null
                    ? target.Graphic
                    : null;

            if (graphic != null &&
                runtimeMaterial != null &&
                graphic.material == runtimeMaterial)
            {
                graphic.material =
                    originalMaterial;
            }

            if (runtimeMaterial != null)
            {
                Object.Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }

            propertyBlock = null;
        }
    }
}