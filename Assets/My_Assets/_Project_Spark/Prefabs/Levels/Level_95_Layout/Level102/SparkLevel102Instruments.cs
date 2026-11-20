using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel102Instruments : MonoBehaviour
    {
        private static readonly string[] ModelNames = {
            "L95_Protected5VBenchSupply", "L95_PrecisionDigitalMultimeter",
            "L95_FourChannelOscilloscope", "L95_DigitalLogicAnalyzer"
        };
        private static readonly string[] Names = { "5 V SUPPLY", "MULTIMETER", "OSCILLOSCOPE", "LOGIC ANALYZER" };
        private static readonly string[] ProbeNames = {
            "TP01 / U1 RAW CLOCK", "TP02 / ÷4 CLOCK", "TP03 / ÷512 CLOCK", "TP04 / PWM COMMAND",
            "TP05 / GATED OUTPUT", "TP06 / 3.3 V MONITOR", "TP07 / SAFETY STATUS", "TP08 / C1 TIMING",
            "TP09 / SWITCHED LOAD"
        };
        private readonly Transform[] anchors=new Transform[4];
        private readonly RectTransform[] callouts=new RectTransform[4];
        private SparkLevel102Process board;
        private RectTransform canvas, popup;
        private Text title, readout, actionText, leftText, rightText;
        private Button left, right;
        private SparkLevel102Scope scope;
        private int activeInstrument=-1;
        private int probe=-1;
        private bool leadsAttached;

        public int ActiveInstrument => activeInstrument;

        internal static SparkLevel102Instruments Create(SparkLevel102Process board,Transform arena,Text heading)
        {
            var deck=heading.rectTransform.parent;
            var node=new GameObject("Pulse Architect instrument station",typeof(RectTransform),typeof(SparkLevel102Instruments));
            node.transform.SetParent(deck.parent,false);
            var station=node.GetComponent<SparkLevel102Instruments>();
            station.board=board;
            station.canvas=(RectTransform)deck.parent;
            station.Build(arena,(RectTransform)deck,heading.font);
            return station;
        }

        private void Build(Transform arena,RectTransform deck,Font font)
        {
            var core=arena.Find("L102_ART/L95_FR4PrecisionBoardCore");
            var boardRenderer=core ? core.GetComponentInChildren<Renderer>() : null;
            if (boardRenderer)
            {
                foreach (string fixture in new[] {"L95_InspectionMicroscopeGantry","L95_MicroscopeFocusLens"})
                {
                    var module=arena.Find("L102_ART/"+fixture);
                    if (module) module.RotateAround(boardRenderer.bounds.center,arena.up,180f);
                }
            }

            float[] controlPositions = {-486f,-324f,324f,486f};
            for(int i=0;i<Names.Length;i++)
            {
                int slot=i;
                var footer=MakeButton(deck,Names[i],font,new Vector2(158,30),()=>Open(slot),out _);
                var footerRect=(RectTransform)footer.transform;
                footerRect.anchorMin=footerRect.anchorMax=new Vector2(.5f,0);
                footerRect.pivot=new Vector2(.5f,0);
                footerRect.anchoredPosition=new Vector2(controlPositions[i],3);

                var module=arena.Find("L102_ART/"+ModelNames[i]);
                if(!module) continue;
                var surfaces=module.GetComponentsInChildren<Renderer>();
                if(surfaces.Length==0) continue;
                var bounds=surfaces[0].bounds;
                foreach(var surface in surfaces) bounds.Encapsulate(surface.bounds);
                module.RotateAround(bounds.center,arena.up,180f);
                var anchor=new GameObject(Names[i]+" interaction point").transform;
                anchor.SetParent(module,false);
                anchor.position=bounds.center + arena.up * bounds.extents.y * 1.12f;
                anchors[i]=anchor;
                var callout=MakeButton(transform,Names[i],font,new Vector2(144,27),()=>Open(slot),out _);
                callouts[i]=(RectTransform)callout.transform;
            }

            popup=CreateRect(canvas,"Instrument display",new Vector2(425,238));
            popup.anchorMin=popup.anchorMax=new Vector2(1,0);
            popup.pivot=new Vector2(1,0);
            popup.anchoredPosition=new Vector2(-44,401);
            var popupCanvas=popup.gameObject.AddComponent<Canvas>();
            popupCanvas.overrideSorting=true;
            popupCanvas.sortingOrder=900;
            popup.gameObject.AddComponent<GraphicRaycaster>();
            var background=popup.gameObject.AddComponent<Image>();
            background.color=new Color(.024f,.066f,.097f,.98f);
            title=MakeText(popup,"Instrument name","",font,17,TextAnchor.MiddleLeft);
            Place(title.rectTransform,new Vector2(15,197),new Vector2(335,36));
            readout=MakeText(popup,"Instrument readings","",font,15,TextAnchor.MiddleLeft);
            Place(readout.rectTransform,new Vector2(15,57),new Vector2(396,128));

            var close=MakeButton(popup,"CLOSE",font,new Vector2(68,28),Close,out _);
            Place((RectTransform)close.transform,new Vector2(344,201),new Vector2(68,28));
            left=MakeButton(popup,"PREVIOUS",font,new Vector2(103,31),PreviousOrDisconnect,out leftText);
            Place((RectTransform)left.transform,new Vector2(14,12),new Vector2(104,31));
            right=MakeButton(popup,"NEXT",font,new Vector2(103,31),NextProbe,out rightText);
            Place((RectTransform)right.transform,new Vector2(122,12),new Vector2(92,31));
            var action=MakeButton(popup,"CAPTURE",font,new Vector2(175,31),Operate,out actionText);
            Place((RectTransform)action.transform,new Vector2(224,12),new Vector2(187,31));

            scope=SparkLevel102Scope.Create(title.rectTransform,font);
            Place(scope.DisplayRect,new Vector2(13,48),new Vector2(399,146));
            popup.gameObject.SetActive(false);
        }

        public void Open(int instrument)
        {
            if (instrument < 0 || instrument >= Names.Length) return;
            activeInstrument=instrument;
            title.text=Names[instrument];
            popup.gameObject.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            activeInstrument=-1;
            popup.gameObject.SetActive(false);
        }

        public void Operate()
        {
            switch(activeInstrument)
            {
                case 0: board.TogglePower(); break;
                case 1: if(probe<0) probe=5; break;
                case 2:
                case 3:
                    if(!leadsAttached) leadsAttached=true;
                    else board.Measure();
                    break;
            }
            Refresh();
        }

        private void PreviousOrDisconnect()
        {
            if(activeInstrument==1) probe=probe<0 ? ProbeNames.Length-1 : (probe+ProbeNames.Length-1)%ProbeNames.Length;
            else leadsAttached=false;
            Refresh();
        }

        private void NextProbe()
        {
            probe=(probe+1)%ProbeNames.Length;
            Refresh();
        }

        private void LateUpdate()
        {
            var camera=Camera.main;
            if(camera)
            {
                for(int i=0;i<callouts.Length;i++)
                {
                    var callout=callouts[i];
                    if(!callout || !anchors[i]) continue;
                    var point=camera.WorldToScreenPoint(anchors[i].position);
                    bool visible=point.z>0 && point.x>66 && point.x<Screen.width-66 &&
                        point.y>260 && point.y<Screen.height-158;
                    callout.gameObject.SetActive(visible);
                    if(visible && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas,point,null,out var local))
                    {
                        callout.anchorMin=callout.anchorMax=new Vector2(.5f,.5f);
                        callout.anchoredPosition=local+Vector2.up*(i==0 || i==3 ? 10f : -10f);
                    }
                }
            }
            if(activeInstrument>=0) Refresh();
        }

        private void Refresh()
        {
            if (activeInstrument<0) return;
            bool scopeVisible=activeInstrument==2 && leadsAttached;
            scope.gameObject.SetActive(scopeVisible);
            readout.gameObject.SetActive(!scopeVisible);
            left.gameObject.SetActive(activeInstrument==1 || activeInstrument>=2 && leadsAttached);
            right.gameObject.SetActive(activeInstrument==1);
            leftText.text=activeInstrument==1?"PREV POINT":"DISCONNECT";
            switch(activeInstrument)
            {
                case 0:
                    SetReadout(board.SupplyReadout());
                    actionText.text=board.supply.IsOutputActive?"DISABLE OUTPUT":"ENABLE OUTPUT";
                    break;
                case 1:
                    SetReadout(MeterText());
                    actionText.text=probe<0?"ATTACH PROBE":"READ DC VOLTAGE";
                    break;
                case 2:
                case 3:
                    if(!leadsAttached) SetReadout("PROBES DISCONNECTED\nConnect the harness to TP01–TP04.");
                    else if(activeInstrument==2) board.UpdateInstrumentScope(scope);
                    else SetReadout(board.LogicReadout());
                    actionText.text=leadsAttached ? "CAPTURE / MEASURE" : "CONNECT PROBES";
                    break;
            }
        }

        public string MeterText()
        {
            if(probe<0) return "PROBE DISCONNECTED\nSelect a test point. Black lead is at J1 return.";
            if(!board.supply.IsOutputActive) return ProbeNames[probe]+"\nBENCH OFF";
            return board.TryReadInstrumentPad($"TP{probe+1:00}.PROBE",out float voltage)
                ? $"{ProbeNames[probe]}\n{voltage:F3} V DC   /   REFERENCED TO RETURN"
                : ProbeNames[probe]+"\nNO SOLVED READING";
        }

        private void SetReadout(string value)
        {
            if(readout.text!=value) readout.text=value;
        }

        private static RectTransform CreateRect(Transform parent,string name,Vector2 size)
        {
            var child=new GameObject(name,typeof(RectTransform));
            var rect=(RectTransform)child.transform;
            rect.SetParent(parent,false);
            rect.sizeDelta=size;
            return rect;
        }

        private static void Place(RectTransform rect,Vector2 position,Vector2 size)
        {
            rect.anchorMin=rect.anchorMax=Vector2.zero;
            rect.pivot=Vector2.zero;
            rect.anchoredPosition=position;
            rect.sizeDelta=size;
        }

        private static Text MakeText(Transform parent,string name,string contents,Font font,int size,TextAnchor alignment)
        {
            var rect=CreateRect(parent,name,Vector2.zero);
            var text=rect.gameObject.AddComponent<Text>();
            text.font=font;
            text.fontSize=size;
            text.alignment=alignment;
            text.color=new Color(.86f,.96f,1f);
            text.text=contents;
            text.raycastTarget=false;
            return text;
        }

        private static Button MakeButton(Transform parent,string name,Font font,Vector2 size,
            UnityEngine.Events.UnityAction clicked,out Text label)
        {
            var rect=CreateRect(parent,name,size);
            var image=rect.gameObject.AddComponent<Image>();
            image.color=new Color(.09f,.29f,.48f,.96f);
            var button=rect.gameObject.AddComponent<Button>();
            button.targetGraphic=image;
            button.onClick.AddListener(clicked);
            label=MakeText(rect,"Label",name,font,12,TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin=Vector2.zero;
            label.rectTransform.anchorMax=Vector2.one;
            label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
            return button;
        }
    }
}
