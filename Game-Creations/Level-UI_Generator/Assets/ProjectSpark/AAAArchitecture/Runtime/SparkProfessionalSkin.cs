using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AAAArchitecture
{
    public static class SparkProfessionalSkin
    {
        public const string Root = "SparkProfessional/";
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        private static SparkProfessionalSettings settings;
        private static bool loaded;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { cache.Clear(); settings = null; loaded = false; }
        private static SparkProfessionalSettings Settings
        {
            get { if (!loaded) { settings = Resources.Load<SparkProfessionalSettings>(Root + "Settings"); loaded = true; } return settings; }
        }
        public static bool UseShowcase => Settings && Settings.useShowcaseLayout;
        public static bool NeonEnabled => !Settings || Settings.neonEnabled;
        public static float NeonIntensity => Settings ? Mathf.Clamp(Settings.neonIntensity,.15f,1f) : .88f;
        private static readonly Color32[] NeonPalette = {
            new(35,220,255,255), new(255,75,154,255), new(150,112,255,255),
            new(255,171,47,255), new(70,255,171,255), new(88,153,255,255),
            new(35,235,218,255), new(221,101,255,255), new(255,102,110,255),
            new(112,241,95,255), new(82,196,255,255), new(255,208,79,255), new(183,125,255,255)
        };
        public static Color ChapterColor(int chapter, Color fallback)
        {
            if (!NeonEnabled || (Settings && !Settings.colorfulChapters) || chapter < 1) return fallback;
            return NeonPalette[(chapter-1)%NeonPalette.Length];
        }
        public static Color MissionColor(int level, Color fallback)
        {
            if (!NeonEnabled || (Settings && !Settings.colorfulMissions) || level < 1) return fallback;
            return NeonPalette[(level-1)%NeonPalette.Length];
        }
        public static Color Glow(Color color, float alpha)
        { color.a = Mathf.Clamp01(alpha * NeonIntensity); return color; }
        public static Color SurfaceTint(Color color, float weight)
        { return Color.Lerp(Color.white, new Color(color.r,color.g,color.b,1), Mathf.Clamp01(weight * NeonIntensity)); }
        public static Sprite Get(string role)
        {
            var s = Settings;
            if (s && role == "Shell" && s.environment) return s.environment;
            if (s && role == "Chamber" && s.inspectionBackground) return s.inspectionBackground;
            if (cache.TryGetValue(role, out var sprite) && sprite) return sprite;
            sprite = NeonEnabled ? Resources.Load<Sprite>(Root + "SkinV8Neon/" + role) : null;
            if (!sprite) sprite = Resources.Load<Sprite>(Root + "SkinV8/" + role);
            if (!sprite) sprite = Resources.Load<Sprite>(Root + "Skin/" + role);
            if (sprite) cache[role] = sprite;
            return sprite;
        }
        public static void Apply(Image image, string role)
        {
            if (!image) return;
            var sprite = Get(role);
            if (!sprite)
            {
                if (role == "Fade") { image.color = Color.clear; image.raycastTarget = false; return; }
                SparkShowcaseSkin.Apply(image, role); return;
            }
            image.material = null; image.sprite = sprite;
            image.type = role == "Shell" || role == "Chamber" || role == "Fade" || sprite.border == Vector4.zero ? Image.Type.Simple : Image.Type.Sliced;
            image.preserveAspect = false; image.color = role == "Fade" ? new Color(1,1,1,.85f) : Color.white;
            image.pixelsPerUnitMultiplier = 1f;
        }
        public static Image Frame(RectTransform parent, Color color, string label = "Selection Outline", float inset = 0)
        {
            var rect = SparkCinematicSkin.Rect(label, parent, Vector2.zero, Vector2.one, Vector2.one * inset, -Vector2.one * inset);
            var image = rect.gameObject.AddComponent<Image>(); image.raycastTarget = false;
            Apply(image, "Selection"); image.color = image.sprite ? color : Color.clear; return image;
        }
        public static void StyleButton(Button button, bool primary)
        {
            Apply(button.image, primary ? "Action" : "Tab");
            button.transition = Selectable.Transition.ColorTint;
            var c = button.colors;
            c.normalColor = Color.white; c.highlightedColor = new Color(1.12f,1.12f,1.12f);
            c.selectedColor = new Color(.85f,.95f,1f); c.pressedColor = new Color(.76f,.80f,.83f);
            c.disabledColor = new Color(.50f,.53f,.55f,.88f); c.colorMultiplier = 1; c.fadeDuration = .13f;
            button.colors = c;
        }
        public static void ApplyTypography(TMP_Text t, bool heading)
        {
            if (!t) return;
            var s = Settings;
            if (s)
            {
                var font = heading ? s.displayFont : s.bodyFont;
                if (font) t.font = font;
            }
            t.characterSpacing = heading ? .35f : .08f;
            t.extraPadding = true; t.richText = false;
        }
        public static Sprite Banner(int number) => SparkShowcaseSkin.Banner(number);
    }
}
