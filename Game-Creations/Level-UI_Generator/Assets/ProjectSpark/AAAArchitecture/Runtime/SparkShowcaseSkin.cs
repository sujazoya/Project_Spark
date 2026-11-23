using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AAAArchitecture
{
    // Separate asset namespace; old V5 skins and user settings are not overwritten.
    public static class SparkShowcaseSkin
    {
        public const string Root = "SparkShowcase/";
        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => sprites.Clear();
        public static Sprite Get(string role)
        {
            if (sprites.TryGetValue(role, out var s) && s) return s;
            s = Resources.Load<Sprite>(Root + "Skin/" + role);
            if (s) sprites[role] = s;
            return s;
        }
        static string Legacy(string role)
        {
            switch(role) {
                case "Shell": return "Backdrop";
                case "Action": case "ActiveTab": return "Primary";
                case "Tab": case "Index": return "Button";
                case "GalleryCard": return "Card";
                case "Chamber": return "Hero";
                case "Notice": return "Badge";
                case "Selection": return "Frame";
                default: return "Panel";
            }
        }
        public static void Apply(Image image, string role)
        {
            if (!image) return;
            var s = Get(role);
            if (!s) { SparkCinematicSkin.Plate(image, Legacy(role)); return; }
            image.material = null;
            image.sprite=s; image.type=role=="Chamber" || role=="Shell" ? Image.Type.Simple : Image.Type.Sliced;
            image.preserveAspect=false; image.color=Color.white; image.pixelsPerUnitMultiplier=1;
        }
        public static Image Frame(RectTransform parent, Color color, string label="Selection Outline", float inset=0)
        {
            var r=SparkCinematicSkin.Rect(label,parent,Vector2.zero,Vector2.one,Vector2.one*inset,-Vector2.one*inset);
            var im=r.gameObject.AddComponent<Image>(); im.raycastTarget=false;
            Apply(im,"Selection"); im.color=im.sprite?color:Color.clear; return im;
        }
        public static void StyleButton(Button button, bool primary)
        {
            Apply(button.image, primary?"Action":"Tab");
            button.transition=Selectable.Transition.ColorTint;
            var c=button.colors; c.normalColor=Color.white; c.highlightedColor=new Color(1.08f,1.08f,1.08f);
            c.selectedColor=new Color(.82f,.95f,1); c.pressedColor=new Color(.72f,.78f,.82f);
            c.disabledColor=new Color(.46f,.49f,.52f,.90f); c.colorMultiplier=1; c.fadeDuration=.12f; button.colors=c;
        }
        // Only one selected banner is requested; the existing level-card exact-level fallback remains authoritative.
        public static Sprite Banner(int number) => number > 0 ? Resources.Load<Sprite>($"SparkAAAVisuals/PS-L{number:000}/Banner") : null;
    }
}
