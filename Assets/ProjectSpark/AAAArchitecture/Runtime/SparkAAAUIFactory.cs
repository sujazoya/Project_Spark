using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AAAArchitecture
{
    // Used by editor generator only, public so a project editor assembly can call it.
    public static class SparkAAAUIFactory
    {
        public static Color Ink => C("#E8F5FC");
        public static Color Muted => C("#91AABA");
        public static Color Surface => C("#101C26");
        public static Color Outline => C("#304654");
        public static Color C(string value) { ColorUtility.TryParseHtmlString(value, out var color); return color; }
        public static RectTransform Rect(string name, Transform parent)
        {
            var g = new GameObject(name, typeof(RectTransform)); var r = (RectTransform)g.transform;
            if (parent != null) r.SetParent(parent, false);
            return r;
        }
        public static void Stretch(RectTransform r, float left=0, float bottom=0, float right=0, float top=0)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(left,bottom); r.offsetMax = new Vector2(-right,-top); }
        public static Image Image(RectTransform r, Color color)
        { var i = r.gameObject.AddComponent<Image>(); i.color = color; i.raycastTarget = false; return i; }
        public static LayoutElement Size(RectTransform r, float w=-1, float h=-1, float flexW=-1, float flexH=-1)
        { var x = r.GetComponent<LayoutElement>() ?? r.gameObject.AddComponent<LayoutElement>(); x.preferredWidth=w; x.preferredHeight=h; x.flexibleWidth=flexW; x.flexibleHeight=flexH; return x; }
        public static VerticalLayoutGroup Vertical(RectTransform r, int pad=0, float gap=8)
        { var g=r.gameObject.AddComponent<VerticalLayoutGroup>(); g.padding=new RectOffset(pad,pad,pad,pad); g.spacing=gap; g.childControlWidth=true; g.childControlHeight=true; g.childForceExpandWidth=true; g.childForceExpandHeight=false; return g; }
        public static HorizontalLayoutGroup Horizontal(RectTransform r, int pad=0, float gap=8)
        { var g=r.gameObject.AddComponent<HorizontalLayoutGroup>(); g.padding=new RectOffset(pad,pad,pad,pad); g.spacing=gap; g.childControlWidth=true; g.childControlHeight=true; g.childForceExpandWidth=false; g.childForceExpandHeight=true; return g; }
        public static TextMeshProUGUI Text(string name, Transform parent, string value, float size, Color color, float h=-1, bool bold=false)
        {
            var r=Rect(name,parent); var t=r.gameObject.AddComponent<TextMeshProUGUI>();
            t.font=TMP_Settings.defaultFontAsset; t.text=value; t.fontSize=size; t.color=color; t.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
            t.raycastTarget=false; t.richText=false; t.alignment=TextAlignmentOptions.MidlineLeft; t.overflowMode=TextOverflowModes.Ellipsis;
            if(h>=0) Size(r,-1,h); return t;
        }
        public static Button Button(string name, Transform parent, string label, out TextMeshProUGUI text, float w=-1, float h=48)
        {
            var r=Rect(name,parent); Size(r,w,h, w<0?1:0); var img=Image(r,Surface); img.raycastTarget=true;
            var b=r.gameObject.AddComponent<Button>(); b.targetGraphic=img;
            var c=b.colors; c.normalColor=Color.white; c.highlightedColor=new Color(1.15f,1.15f,1.15f); c.pressedColor=new Color(.8f,.8f,.8f); c.selectedColor=new Color(1.1f,1.1f,1.1f); c.disabledColor=new Color(.4f,.4f,.4f); c.fadeDuration=.12f; b.colors=c;
            text=Text("Label",r,label,20,Ink,-1,true); Stretch(text.rectTransform,8,3,8,3); text.alignment=TextAlignmentOptions.Center;
            return b;
        }
        public static TMP_InputField Search(Transform parent)
        {
            var r=Rect("Search",parent); Size(r,-1,48,1); var img=Image(r,Surface); img.raycastTarget=true;
            var field=r.gameObject.AddComponent<TMP_InputField>(); field.targetGraphic=img;
            var area=Rect("TextArea",r); Stretch(area,12,4,12,4); area.gameObject.AddComponent<RectMask2D>();
            var text=Text("Text",area,"",20,Ink); Stretch(text.rectTransform); text.overflowMode=TextOverflowModes.Overflow;
            var placeholder=Text("Placeholder",area,"Search by level, title or discipline",20,Muted); Stretch(placeholder.rectTransform);
            field.textViewport=area; field.textComponent=text; field.placeholder=placeholder; field.lineType=TMP_InputField.LineType.SingleLine; field.characterLimit=120;
            return field;
        }
        public static ScrollRect Scroll(string name, Transform parent, out RectTransform content)
        {
            var r=Rect(name,parent); Size(r,-1,-1,1,1); var bg=Image(r,C("#0B151E")); bg.raycastTarget=true;
            var scroll=r.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false; scroll.vertical=true; scroll.movementType=ScrollRect.MovementType.Clamped; scroll.scrollSensitivity=45;
            var view=Rect("Viewport",r); Stretch(view); view.gameObject.AddComponent<RectMask2D>();
            content=Rect("Content",view); content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(.5f,1); content.anchoredPosition=Vector2.zero; content.sizeDelta=Vector2.zero;
            var fit=content.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=view; scroll.content=content; return scroll;
        }
    }
}
