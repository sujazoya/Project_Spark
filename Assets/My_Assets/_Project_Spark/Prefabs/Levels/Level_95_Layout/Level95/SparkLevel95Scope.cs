using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel95Scope : MonoBehaviour
    {
        private static readonly Color[] Colors = {
            new(.34f,.93f,1f), new(.28f,.8f,.58f), new(.69f,.62f,1f), new(1f,.67f,.24f)
        };
        private readonly float[] channels = new float[4];
        private float duty, high, low;
        public RectTransform DisplayRect => (RectTransform)transform;
        private RectTransform plot;
        private Image[,] segments;
        private Text caption;

        internal static SparkLevel95Scope Create(Transform panel, Font font)
        {
            var node = new GameObject("Pulse Architect scope",typeof(RectTransform),typeof(SparkLevel95Scope));
            var rect = (RectTransform)node.transform;
            rect.SetParent(panel.parent,false);
            rect.anchorMin = rect.anchorMax = new Vector2(1f,0f);
            rect.pivot = new Vector2(1f,0f);
            rect.sizeDelta = new Vector2(380f,168f);
            rect.anchoredPosition = new Vector2(-36f,349f);
            var scope = node.GetComponent<SparkLevel95Scope>();
                        var background=node.AddComponent<Image>();
            background.color=new Color(.02f,.065f,.095f,.98f);
            background.raycastTarget=false;
            scope.BuildPlot(rect);
            var label = new GameObject("Live readout",typeof(RectTransform),typeof(Text));
            label.transform.SetParent(rect,false);
            var labelRect=(RectTransform)label.transform;
            labelRect.anchorMin=new Vector2(0f,.77f);
            labelRect.anchorMax=Vector2.one;
            labelRect.offsetMin=new Vector2(14f,0);
            labelRect.offsetMax=new Vector2(-12f,0);
            scope.caption=label.GetComponent<Text>();
            scope.caption.font=font;
            scope.caption.fontSize=13;
            scope.caption.alignment=TextAnchor.MiddleLeft;
            scope.caption.color=new Color(.85f,.95f,1f);
            scope.caption.raycastTarget=false;
            return scope;
        }

        internal void UpdateDisplay(float master, float divider, float slow, float trim,
            float loadHigh, float loadLow, bool enabled)
        {
            high=loadHigh;
            low=loadLow;
            var text = $"TIMING SCOPE   /   LOAD {high:F2} V ON   {low:F2} V OFF";
            if(caption.text!=text) caption.text=text;
            float pwm=enabled ? divider : 0;
            if(channels[0]==master && channels[1]==divider && channels[2]==slow &&
               channels[3]==pwm && Mathf.Abs(duty-trim)<.001f) return;
            channels[0]=master;
            channels[1]=divider;
            channels[2]=slow;
            channels[3]=pwm;
            duty=trim;
            RebuildTraces();
        }

        private const int MaxSegments=38;

        private static Image Line(Transform parent,string name,Vector2 min,Vector2 max,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            var rect=(RectTransform)go.transform;
            rect.SetParent(parent,false);
            rect.anchorMin=min;
            rect.anchorMax=max;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            var image=go.GetComponent<Image>();
            image.color=color;
            image.raycastTarget=false;
            return image;
        }

        private void BuildPlot(RectTransform parent)
        {
            var surface=Line(parent,"Scope grid",new Vector2(.035f,.07f),new Vector2(.965f,.78f),new Color(.025f,.09f,.125f));
            plot=surface.rectTransform;
            for(int i=0;i<=12;i++)
            {
                float x=(i+.35f)/12.7f;
                Line(plot,"Time division",new Vector2(x,0),new Vector2(x+.002f,1),
                    new Color(.2f,.49f,.62f,.32f));
            }
            segments=new Image[4,MaxSegments];
            for (int channel=0;channel<4;channel++)
            {
                float y=(3-channel)*.25f+.025f;
                Line(plot,"Channel baseline",new Vector2(0,y),new Vector2(1,y+.007f),new Color(.21f,.42f,.52f,.64f));
                for (int i=0;i<MaxSegments;i++)
                {
                    var line=Line(plot,"Trace",Vector2.zero,Vector2.zero,Colors[channel]);
                    line.gameObject.SetActive(false);
                    segments[channel,i]=line;
                }
            }
        }

        private void RebuildTraces()
        {
            for(int channel=0;channel<4;channel++)
            {
                int segment=0;
                float hz=channels[channel];
                if(hz>0f)
                {
                    float window=channel==2 ? 1.1f : .008f;
                    float onTime=channel==3 ? duty : channel==0 ? .552f : .5f;
                    float cycle=1f/(hz*window);
                    float baseline=(3-channel)*.25f+.045f;
                    float raised=baseline+.143f;
                    float start=0;
                    for (int period=0; start<1f && segment+4<=MaxSegments; period++)
                    {
                        float fall=Mathf.Min(1,start+cycle*onTime);
                        float stop=Mathf.Min(1,start+cycle);
                        Show(channel,segment++,start,raised,fall,raised+.016f);
                        if(fall<1) Show(channel,segment++,fall,baseline,fall+.004f,raised);
                        if(stop>fall) Show(channel,segment++,fall,baseline,stop,baseline+.016f);
                        if(stop<1) Show(channel,segment++,stop,baseline,stop+.004f,raised);
                        start=stop;
                    }
                }
                for(int i=segment;i<MaxSegments;i++) segments[channel,i].gameObject.SetActive(false);
            }
        }

        private void Show(int channel,int index,float x0,float y0,float x1,float y1)
        {
            var line=segments[channel,index];
            line.rectTransform.anchorMin=new Vector2(x0,y0);
            line.rectTransform.anchorMax=new Vector2(Mathf.Min(1f,x1),y1);
            line.gameObject.SetActive(true);
        }
    }
}