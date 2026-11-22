using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using F=ProjectSpark.AAAArchitecture.SparkAAAUIFactory;

namespace ProjectSpark.AAAArchitecture
{
    public sealed class SparkAAALevelCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image frame, preview, progress, accent;
        [SerializeField] private TextMeshProUGUI number, title, domain, status, count, placeholder;
        private Action<SparkAAALevel> selected;
        private SparkAAALevel item;
        private Coroutine animation;
        private void OnEnable() { button.onClick.AddListener(Click); }
        private void OnDisable() { button.onClick.RemoveListener(Click); if(animation!=null) StopCoroutine(animation); animation=null; transform.localScale=Vector3.one; }
        private void Click() { if(item!=null) selected?.Invoke(item); }
        public void Bind(SparkAAALevel level, string chapter, SparkAAAProgress state, bool active, Color theme, Action<SparkAAALevel> callback)
        {
            item=level; selected=callback; number.text=level.number.ToString("000"); title.text=level.title; domain.text=chapter;
            bool missing=!level.Present || level.blocked; status.text=missing?"UNAVAILABLE":state.status==SparkAAAStatus.Unknown?"ACCESS NOT VERIFIED":state.status.ToString().ToUpperInvariant();
            count.text=level.Present ? level.targets.Count+" TARGETS" : "SOURCE REQUIRED";
            frame.color=active?theme:F.Outline; accent.color=active?theme:F.C("#537080");
            status.color=missing?F.Muted:state.status==SparkAAAStatus.Certified?F.C("#55D4A0"):state.status==SparkAAAStatus.Requalification?F.C("#F2B167"):theme;
            preview.sprite=level.thumbnail; preview.color=level.thumbnail!=null?Color.white:F.C("#0A131B"); placeholder.gameObject.SetActive(level.thumbnail==null);
            placeholder.text=level.Present?"ENGINEERING\nMISSION":"MISSING\nDEFINITION";
            progress.color=status.color; progress.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(state.fraction),1); progress.rectTransform.offsetMin=Vector2.zero; progress.rectTransform.offsetMax=Vector2.zero; progress.gameObject.SetActive(!missing && state.status!=SparkAAAStatus.Unknown);
        }
        public void OnPointerEnter(PointerEventData e) => Animate(1.018f);
        public void OnPointerExit(PointerEventData e) => Animate(1f);
        private void Animate(float scale) { if(!isActiveAndEnabled)return; if(animation!=null)StopCoroutine(animation); animation=StartCoroutine(Scale(scale)); }
        private IEnumerator Scale(float target)
        { float t=0; Vector3 from=transform.localScale; while(t<.16f){ t+=Time.unscaledDeltaTime; transform.localScale=Vector3.Lerp(from,Vector3.one*target,Mathf.SmoothStep(0,1,t/.16f)); yield return null; } transform.localScale=Vector3.one*target; animation=null; }
        public static SparkAAALevelCard Create()
        {
            var r=F.Rect("SparkLevelButton",null); var card=r.gameObject.AddComponent<SparkAAALevelCard>(); card.frame=F.Image(r,F.Outline); card.frame.raycastTarget=true;
            card.button=r.gameObject.AddComponent<Button>(); card.button.targetGraphic=card.frame; card.button.transition=Selectable.Transition.None;
            var body=F.Rect("Surface",r); F.Stretch(body,2,2,2,2); F.Image(body,F.Surface); F.Vertical(body,10,6);
            var hero=F.Rect("HardwarePreview",body); F.Size(hero,-1,96); card.preview=F.Image(hero,F.C("#0A131B")); card.preview.preserveAspect=true;
            card.placeholder=F.Text("PreviewPlaceholder",hero,"ENGINEERING\nMISSION",16,F.Muted); F.Stretch(card.placeholder.rectTransform,6,6,6,6); card.placeholder.alignment=TextAlignmentOptions.Center;
            card.accent=F.Image(F.Rect("Accent",body),F.Outline); F.Size(card.accent.rectTransform,-1,2);
            card.number=F.Text("LevelNumber",body,"000",34,F.Ink,37,true);
            card.title=F.Text("MissionTitle",body,"Mission",21,F.Ink,49,true);
            card.domain=F.Text("Division",body,"Division",16,F.Muted,23);
            card.count=F.Text("TargetCount",body,"0 TARGETS",16,F.Muted,23);
            card.status=F.Text("State",body,"UNKNOWN",16,F.Muted,23,true);
            var track=F.Rect("ProgressTrack",body); F.Size(track,-1,4); F.Image(track,F.Outline);
            var fill=F.Rect("ProgressFill",track); F.Stretch(fill); card.progress=F.Image(fill,Color.cyan);
            // Filled needs a sprite. Instead use anchor-driven progress (see SetProgress below).
            card.progress.type=Image.Type.Simple;
            return card;
        }
    }
}
