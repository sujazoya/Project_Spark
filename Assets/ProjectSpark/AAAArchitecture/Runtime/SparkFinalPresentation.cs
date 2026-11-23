using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.AAAArchitecture
{
    // Owned by the campaign Canvas, not a persistent global object. No idle Update.
    [DisallowMultipleComponent]
    public sealed class SparkFinalPresentation : MonoBehaviour
    {
        private const string ThemePref = "ProjectSpark.FinalHD.Theme";
        private const string MotionPref = "ProjectSpark.FinalHD.ReducedMotion";
        private static readonly Color32[] Colors = { new(225,241,250,255),new(255,92,105,255),new(78,222,249,255),new(245,124,200,255),new(255,170,72,255),new(91,227,162,255) };
        private static readonly string[] Names = {"WHITE","RED","BLUE","PINK","ORANGE","GREEN"};
        private SparkFinalPresentationSettings settings;
        private CanvasGroup shell, brief, blocker;
        private TMP_Text launchTitle, launchHint;
        private Coroutine shellFade, briefFade;
        private Sprite[] icons;
        private int theme;
        public Color Accent => Colors[theme];
        public string ThemeName => Names[theme];
        public bool ReducedMotion => SparkCinematicClock.ReducedMotion;
        public bool Blocking => blocker && blocker.gameObject.activeSelf;
        public static SparkFinalPresentation Attach(RectTransform overlay, RectTransform shellRect, RectTransform briefRect, TMP_FontAsset font)
        {
            var p=overlay.gameObject.AddComponent<SparkFinalPresentation>();
            p.settings=Resources.Load<SparkFinalPresentationSettings>(SparkFinalPresentationSettings.Root+"Settings");
            p.icons=p.settings ? p.settings.chapterIcons : null;
            SparkCinematicClock.ReducedMotion = PlayerPrefs.GetInt(MotionPref, p.settings && p.settings.reducedMotion ? 1 : 0) != 0;
            p.theme=Mathf.Clamp(PlayerPrefs.GetInt(ThemePref,2),0,Colors.Length-1);
            p.shell=Group(shellRect); p.brief=Group(briefRect);
            p.MakeBlocker(overlay,font);
            return p;
        }
        private static CanvasGroup Group(RectTransform r) => r.TryGetComponent<CanvasGroup>(out var g) ? g : r.gameObject.AddComponent<CanvasGroup>();
        public Sprite Icon(int chapter) => icons!=null && chapter>=1 && chapter<=icons.Length ? icons[chapter-1] : null;
        public void CycleTheme() { theme=(theme+1)%Colors.Length; PlayerPrefs.SetInt(ThemePref,theme); }
        public void ToggleMotion() { SparkCinematicClock.ReducedMotion = !ReducedMotion; PlayerPrefs.SetInt(MotionPref,ReducedMotion?1:0); if(ReducedMotion) ResetFades(); }
        public void Show() { if(shellFade!=null)StopCoroutine(shellFade); shellFade=null; if (shell) { shell.alpha=1; if(!ReducedMotion && settings && settings.entranceSeconds>0) { shell.alpha=0; shellFade=StartCoroutine(Fade(shell,settings.entranceSeconds)); } } }
        public void SelectionChanged() { if(briefFade!=null)StopCoroutine(briefFade); briefFade=null; if(brief) { brief.alpha=1; if(!ReducedMotion && settings && settings.briefingSeconds>0 && isActiveAndEnabled) { brief.alpha=.40f; briefFade=StartCoroutine(Fade(brief,settings.briefingSeconds)); } } }
        private IEnumerator Fade(CanvasGroup g,float duration) { float elapsed=0,from=g.alpha; while(elapsed<duration && g) { elapsed+=Time.unscaledDeltaTime; g.alpha=Mathf.Lerp(from,1,Mathf.SmoothStep(0,1,elapsed/duration)); yield return null; } if(g)g.alpha=1; }
        private void ResetFades() { if(shellFade!=null)StopCoroutine(shellFade); if(briefFade!=null)StopCoroutine(briefFade); shellFade=briefFade=null; if(shell)shell.alpha=1; if(brief)brief.alpha=1; }
        private void OnDisable() { ResetFades(); EndLaunch(); }
        public void AddBackdrop(RectTransform parent)
        {
            if(!settings || !settings.laboratoryBackdrop) return;
            // A masked envelope preserves the original artwork aspect ratio. No input is intercepted.
            AddArt(parent, "Studio Header Backdrop", settings.backdropOpacity, new Vector2(6,6), new Vector2(-6,-6));
            if (shell && shell.transform is RectTransform shellRect && shellRect != parent)
                AddArt(shellRect, "Studio Ambient Backdrop", settings.backdropOpacity * .40f, new Vector2(9,9), new Vector2(-9,-9));
        }
        private void AddArt(RectTransform parent, string label, float opacity, Vector2 lo, Vector2 hi)
        {
            var clip = SparkCinematicSkin.Rect(label,parent,Vector2.zero,Vector2.one,lo,hi);
            clip.gameObject.AddComponent<RectMask2D>();
            clip.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var art = SparkCinematicSkin.Rect("Artwork",clip,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var image = art.gameObject.AddComponent<Image>();
            image.sprite = settings.laboratoryBackdrop;
            image.color = new Color(1,1,1,Mathf.Clamp01(opacity)); image.raycastTarget = false;
            var aspect = art.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectRatio = image.sprite.rect.width / Mathf.Max(1,image.sprite.rect.height);
            aspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            clip.SetAsFirstSibling();
        }
        private void MakeBlocker(RectTransform overlay,TMP_FontAsset font)
        {
            var root=SparkCinematicSkin.Rect("Mission Initialization",overlay,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            var dim=root.gameObject.AddComponent<Image>(); dim.color=new Color32(2,8,14,238); dim.raycastTarget=true;
            blocker=Group(root); blocker.blocksRaycasts=true; blocker.interactable=true;
            var plate=SparkCinematicSkin.Rect("Launch Status",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-230,-105),new Vector2(230,105));
            var size=plate.gameObject.AddComponent<LayoutElement>(); size.ignoreLayout=true;
            var background=plate.gameObject.AddComponent<Image>(); background.color=new Color32(12,30,44,255); SparkCinematicSkin.Plate(background,"Panel");
            SparkCinematicSkin.Frame(plate,SparkCinematicSkin.Gold);
            launchTitle=Text(plate,font,"OPENING WORKBENCH",24,new Vector2(20,96),new Vector2(-20,-20),SparkCinematicSkin.Ink);
            launchHint=Text(plate,font,"Waiting for the gameplay manager",15,new Vector2(20,18),new Vector2(-20,-115),SparkCinematicSkin.Muted);
            root.gameObject.SetActive(false);
        }
        private static TMP_Text Text(RectTransform parent,TMP_FontAsset font,string value,int size,Vector2 lo,Vector2 hi,Color color)
        {
            var r=SparkCinematicSkin.Rect("Text",parent,Vector2.zero,Vector2.one,lo,hi);
            var t=r.gameObject.AddComponent<TextMeshProUGUI>(); if(font)t.font=font; t.text=value; t.fontSize=size; t.color=color; t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false; t.textWrappingMode=TextWrappingModes.Normal; SparkCinematicSkin.StyleText(t,true); return t;
        }
        public void BeginLaunch(int level,string title) { if(!blocker)return; launchTitle.text=$"OPENING LEVEL {level:000}\n{title}"; launchHint.text="Waiting for the gameplay manager"; blocker.transform.SetAsLastSibling(); blocker.gameObject.SetActive(true); if(shell){shell.interactable=false;shell.blocksRaycasts=false;} }
        public void EndLaunch() { if(blocker)blocker.gameObject.SetActive(false); if(shell){shell.interactable=true;shell.blocksRaycasts=true;} }
    }
}
