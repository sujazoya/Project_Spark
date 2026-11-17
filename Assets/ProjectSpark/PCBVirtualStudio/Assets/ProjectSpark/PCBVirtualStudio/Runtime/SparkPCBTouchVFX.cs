using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.PCBVirtualStudio
{
    // Built-in, touch-positioned UI VFX. No particle prefabs, FBX models, or external sprites required.
    [DisallowMultipleComponent]
    public sealed class SparkPCBTouchVFX : MonoBehaviour
    {
        [SerializeField] private RectTransform effectsRoot;
        [SerializeField] private int maxConcurrentEffects = 80;
        private int activeEffects;

        public void Initialize(RectTransform root) => effectsRoot = root;

        public void PlayAt(RectTransform target, SparkVirtualTool tool, bool accepted)
        {
            if (effectsRoot == null || target == null) return;
            Vector3 pos = effectsRoot.InverseTransformPoint(target.TransformPoint(target.rect.center));
            Color color = accepted ? GetToolColor(tool) : new Color(1f, .26f, .32f, 1f);
            SpawnRing(pos, color, .6f);
            if (!accepted) return;
            if (tool == SparkVirtualTool.Solder || tool == SparkVirtualTool.Desolder ||
                tool == SparkVirtualTool.Reflow || tool == SparkVirtualTool.HotAir ||
                tool == SparkVirtualTool.BGAReball)
                SpawnParticles(pos, 17, color, .85f, true);
            else if (tool == SparkVirtualTool.ThermalCamera || tool == SparkVirtualTool.FaultScanner ||
                     tool == SparkVirtualTool.Multimeter || tool == SparkVirtualTool.NetTracer)
                SpawnParticles(pos, 9, color, .70f, false);
            else
                SpawnParticles(pos, 12, color, .75f, false);
        }

        public void PlayAtWorld(Vector3 worldPosition, Camera camera, SparkVirtualTool tool, bool accepted)
        {
            if (effectsRoot == null) return;
            Canvas canvas = effectsRoot.GetComponentInParent<Canvas>();
            if (camera == null && canvas != null) camera = canvas.worldCamera;
            if (camera == null) camera = Camera.main;
            if (camera == null) return;
            Vector3 projected = camera.WorldToScreenPoint(worldPosition);
            if (projected.z <= 0) return;
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(effectsRoot,
                new Vector2(projected.x, projected.y), camera, out local)) return;
            Color tint = accepted ? GetToolColor(tool) : new Color(1f,.26f,.32f,1f);
            SpawnRing(new Vector3(local.x, local.y, 0), tint, .6f);
            if (accepted) SpawnParticles(new Vector3(local.x, local.y, 0),
                tool == SparkVirtualTool.Solder || tool == SparkVirtualTool.Reflow ? 18 : 10,
                tint, .75f, tool == SparkVirtualTool.Solder || tool == SparkVirtualTool.Reflow);
        }

        private static Color GetToolColor(SparkVirtualTool tool)
        {
            switch (tool)
            {
                case SparkVirtualTool.Solder:
                case SparkVirtualTool.Reflow:
                case SparkVirtualTool.HotAir:
                case SparkVirtualTool.Desolder: return new Color(1f, .56f, .21f);
                case SparkVirtualTool.ThermalCamera: return new Color(1f, .28f, .42f);
                case SparkVirtualTool.Clean:
                case SparkVirtualTool.Flux: return new Color(.21f, .95f, .72f);
                case SparkVirtualTool.BGAReball: return new Color(1f, .82f, .38f);
                default: return new Color(.22f, .86f, 1f);
            }
        }

        private void SpawnRing(Vector3 position, Color tint, float duration)
        {
            if (activeEffects >= maxConcurrentEffects) return;
            var go = new GameObject("UI_VFX_ScanRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(SparkPCBRoundedPanelGraphic));
            go.transform.SetParent(effectsRoot, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(position.x, position.y);
            rect.sizeDelta = new Vector2(14, 14);
            SparkPCBRoundedPanelGraphic img = go.GetComponent<SparkPCBRoundedPanelGraphic>();
            img.SetStyle(new Color(0, 0, 0, 0), tint, 25f, 2.5f);
            img.raycastTarget = false;
            activeEffects++;
            StartCoroutine(AnimateRing(go, rect, img, tint, duration));
        }

        private IEnumerator AnimateRing(GameObject go, RectTransform rt, SparkPCBRoundedPanelGraphic img, Color c, float duration)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / duration);
                rt.sizeDelta = Vector2.one * Mathf.Lerp(14, 85, a);
                img.SetStyle(new Color(0, 0, 0, 0), new Color(c.r, c.g, c.b, (1f - a) * .8f), Mathf.Lerp(7f, 42f, a), 2.5f);
                yield return null;
            }
            if (go != null) DestroySafe(go);
            activeEffects--;
        }

        private void SpawnParticles(Vector3 position, int count, Color color, float duration, bool smoke)
        {
            for (int i = 0; i < count; i++)
            {
                if (activeEffects >= maxConcurrentEffects) return;
                var go = new GameObject(smoke ? "UI_VFX_SolderSpark" : "UI_VFX_DiagnosticParticle",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(effectsRoot, false);
                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(position.x, position.y);
                rt.sizeDelta = Vector2.one * Random.Range(3f, smoke ? 9f : 5f);
                Image img = go.GetComponent<Image>();
                img.raycastTarget = false;
                Color p = smoke && i % 4 == 0 ? new Color(.66f, .77f, .83f, 1f) : color;
                img.color = p;
                Vector2 velocity = Random.insideUnitCircle.normalized * Random.Range(24f, smoke ? 115f : 90f);
                if (smoke) velocity += Vector2.up * 48f;
                activeEffects++;
                StartCoroutine(AnimateParticle(go, rt, img, p, velocity, duration * Random.Range(.6f, 1.15f)));
            }
        }

        private IEnumerator AnimateParticle(GameObject go, RectTransform rt, Image img, Color c,
            Vector2 velocity, float duration)
        {
            float t = 0;
            Vector2 start = rt.anchoredPosition;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / duration);
                rt.anchoredPosition = start + velocity * t + new Vector2(0, 18f * t * t);
                img.color = new Color(c.r, c.g, c.b, (1f - a) * .95f);
                yield return null;
            }
            if (go != null) DestroySafe(go);
            activeEffects--;
        }

        private static void DestroySafe(Object o)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(o);
            else Object.Destroy(o);
#else
            Object.Destroy(o);
#endif
        }
    }
}
