using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using F=ProjectSpark.AAAArchitecture.SparkAAAUIFactory;

namespace ProjectSpark.AAAArchitecture
{
    public sealed class SparkAAAMissionPanel : MonoBehaviour
    {
        private const int PageSize=24;
        private static readonly Color[] Themes={F.C("#E4F4FF"),F.C("#F54A57"),F.C("#43D9F4"),F.C("#F66FC4"),F.C("#FF9D45"),F.C("#49E0A1")};
        private static readonly string[] ThemeNames={"WHITE","RED","BLUE","PINK","ORANGE","GREEN"};
        private static readonly string[] Filters={"ALL","AVAILABLE","IN PROGRESS","COMPLETED","CERTIFIED","LOCKED","UNVERIFIED","UNAVAILABLE"};
        [SerializeField] private SparkAAACatalog catalog;
        [SerializeField] private SparkAAALevelCard cardPrefab;
        [SerializeField] private SparkAAAMissionGateway gateway;
        [SerializeField] private RectTransform main, sidebar, details, grid, detailPreviewBox;
        [SerializeField] private GridLayoutGroup gridLayout;
        [SerializeField] private ScrollRect gridScroll, detailScroll;
        [SerializeField] private TextMeshProUGUI header, stats, chapterLabel, filterLabel, pageLabel, detailTitle, detailStatus, detailBody, actionLabel, notice, themeLabel, detailPlaceholder;
        [SerializeField] private Button chapterNext, filterNext, previousPage, nextPage, action, back, themeNext;
        [SerializeField] private TMP_InputField search;
        [SerializeField] private Image detailPreview, accent;
        [SerializeField] private Button[] chapterButtons;
        private readonly List<SparkAAALevelCard> cards=new List<SparkAAALevelCard>(PageSize);
        private readonly List<SparkAAALevel> filtered=new List<SparkAAALevel>();
        private readonly Dictionary<string,SparkAAAProgress> states=new Dictionary<string,SparkAAAProgress>(StringComparer.Ordinal);
        private int chapter, filter, page, theme;
        private SparkAAALevel selected;
        private string loadedBannerPath;
        private Sprite loadedBanner;
        private bool compact, compactDetails, ready;
        private Coroutine resize;
        private Color Accent=>Themes[theme];
        private void OnEnable()
        {
            if(catalog==null || cardPrefab==null || search==null){Debug.LogError("[Spark AAA] Panel references missing.",this); return;}
            ready=true; theme=SparkAAAPreferences.Theme; selected=catalog.Find(SparkAAAPreferences.LastKey);
            if(!Owns(selected)) selected=null;
            if(selected!=null) { int index=catalog.levels.IndexOf(selected); page=Mathf.Max(0,index/PageSize); }
            if(gateway!=null) gateway.Changed+=Refresh;
            search.onValueChanged.AddListener(OnSearch); chapterNext.onClick.AddListener(CycleChapter); filterNext.onClick.AddListener(CycleFilter);
            previousPage.onClick.AddListener(Previous); nextPage.onClick.AddListener(Next); action.onClick.AddListener(Launch); back.onClick.AddListener(Back); themeNext.onClick.AddListener(CycleTheme);
            for(int i=0;i<chapterButtons.Length;i++){int captured=i; chapterButtons[i].onClick.AddListener(()=>SetChapter(captured==0?0:catalog.chapters[captured-1].id));}
            Refresh(); QueueResize();
        }
        private bool Owns(SparkAAALevel item)=>item!=null && catalog.Find(item.key)==item;
        private void OnDisable()
        {
            if(!ready)return; ready=false;
            if(gateway!=null)gateway.Changed-=Refresh;
            search.onValueChanged.RemoveListener(OnSearch); chapterNext.onClick.RemoveListener(CycleChapter); filterNext.onClick.RemoveListener(CycleFilter);
            previousPage.onClick.RemoveListener(Previous); nextPage.onClick.RemoveListener(Next); action.onClick.RemoveListener(Launch); back.onClick.RemoveListener(Back); themeNext.onClick.RemoveListener(CycleTheme);
            // Chapter buttons belong to this generated prefab. Their runtime listeners are owned here.
            for(int i=0;i<chapterButtons.Length;i++)chapterButtons[i].onClick.RemoveAllListeners();
            if(resize!=null)StopCoroutine(resize); resize=null;
        }
        public void SetGateway(SparkAAAMissionGateway value)
        { if(ready && gateway!=null)gateway.Changed-=Refresh; gateway=value; if(ready && gateway!=null)gateway.Changed+=Refresh; if(ready)Refresh(); }
        private SparkAAAProgress State(SparkAAALevel e)
        {
            if(!e.Present || e.blocked) return new SparkAAAProgress {reason=e.issue};
            try { return gateway!=null && gateway.isActiveAndEnabled ? gateway.Read(e)??new SparkAAAProgress() : new SparkAAAProgress(); }
            catch(Exception ex){return new SparkAAAProgress{reason="Progress adapter error: "+ex.Message};}
        }
        public void Refresh()
        {
            if(!ready)return; states.Clear(); int present=0, complete=0, certified=0, known=0;
            foreach(var e in catalog.levels){var s=State(e); s.fraction=float.IsNaN(s.fraction)||float.IsInfinity(s.fraction)?0:Mathf.Clamp01(s.fraction); states[e.key]=s; if(e.Present)present++; if(s.status!=SparkAAAStatus.Unknown)known++; if(s.status==SparkAAAStatus.Completed||s.status==SparkAAAStatus.Certified||s.status==SparkAAAStatus.Requalification)complete++; if(s.status==SparkAAAStatus.Certified)certified++;}
            stats.text=$"{catalog.levels.Count} SLOTS  /  {present} SOURCES  /  {(known==0?"PROGRESS NOT CONNECTED":complete+" COMPLETED  /  "+certified+" CERTIFIED")}";
            header.color=Accent; accent.color=Accent; themeLabel.text=ThemeNames[theme];
            Refilter(false);
        }
        private void OnSearch(string _) => Refilter(true);
        private void SetChapter(int id){chapter=id; Refilter(true);}
        private void CycleChapter(){int index=-1; for(int i=0;i<catalog.chapters.Count;i++)if(catalog.chapters[i].id==chapter)index=i; SetChapter(index+1>=catalog.chapters.Count?0:catalog.chapters[index+1].id);}
        private void CycleFilter(){filter=(filter+1)%Filters.Length; Refilter(true);}
        private void CycleTheme(){theme=(theme+1)%Themes.Length; SparkAAAPreferences.Theme=theme; Refresh();}
        public void SetTheme(int index){theme=Mathf.Clamp(index,0,Themes.Length-1); SparkAAAPreferences.Theme=theme; Refresh();}
        private bool Matches(SparkAAALevel e)
        {
            if(chapter!=0&&e.chapter!=chapter)return false;
            string q=search.text.Trim(); if(q.Length!=0 && e.number.ToString().IndexOf(q,StringComparison.OrdinalIgnoreCase)<0 && e.number.ToString("000").IndexOf(q,StringComparison.OrdinalIgnoreCase)<0 && e.title.IndexOf(q,StringComparison.OrdinalIgnoreCase)<0 && catalog.ChapterName(e.chapter).IndexOf(q,StringComparison.OrdinalIgnoreCase)<0 && (e.legacyId??"").IndexOf(q,StringComparison.OrdinalIgnoreCase)<0)return false;
            bool unavailable=!e.Present||e.blocked; if(filter==0)return true; if(filter==7)return unavailable; if(unavailable)return false;
            var s=states[e.key].status;
            switch(filter){case 1:return s==SparkAAAStatus.Available;case 2:return s==SparkAAAStatus.InProgress;case 3:return s==SparkAAAStatus.Completed||s==SparkAAAStatus.Certified||s==SparkAAAStatus.Requalification;case 4:return s==SparkAAAStatus.Certified;case 5:return s==SparkAAAStatus.Locked;case 6:return s==SparkAAAStatus.Unknown;default:return false;}
        }
        private void Refilter(bool reset)
        {
            filtered.Clear(); foreach(var e in catalog.levels)if(Matches(e))filtered.Add(e);
            if(reset)page=0;
            if(selected==null||!filtered.Contains(selected)){selected=filtered.Count>0?filtered[0]:null; compactDetails=false;}
            if(reset)notice.text="";
            chapterLabel.text=chapter==0?"ALL DIVISIONS":catalog.ChapterName(chapter); filterLabel.text=Filters[filter];
            ShowPage(reset); ShowDetail(); ApplyMode();
        }
        private int Pages=>Mathf.Max(1,(filtered.Count+PageSize-1)/PageSize);
        private void Previous(){page=Mathf.Max(0,page-1);ShowPage(true);}
        private void Next(){page=Mathf.Min(Pages-1,page+1);ShowPage(true);}
        private void ShowPage(bool resetScroll)
        {
            page=Mathf.Clamp(page,0,Pages-1); int first=page*PageSize; int count=Mathf.Min(PageSize,filtered.Count-first);
            while(cards.Count<count){var card=Instantiate(cardPrefab,grid,false); cards.Add(card);}
            for(int i=0;i<cards.Count;i++)
            {
                bool active=i<count; cards[i].gameObject.SetActive(active); if(!active)continue;
                var e=filtered[first+i];cards[i].Bind(e,catalog.ChapterName(e.chapter),states[e.key],e==selected,Accent,Select);
            }
            pageLabel.text=$"{page+1} / {Pages}  •  {filtered.Count} MISSIONS"; previousPage.interactable=page>0;nextPage.interactable=page<Pages-1;
            if(resetScroll)gridScroll.verticalNormalizedPosition=1;
            QueueResize();
        }
        private void Select(SparkAAALevel e){selected=e; SparkAAAPreferences.LastKey=e.key; compactDetails=true; notice.text=""; ShowPage(false);ShowDetail();ApplyMode();detailScroll.verticalNormalizedPosition=1;}
        private void Back(){compactDetails=false;ApplyMode();QueueResize();}
        private void ShowDetail()
        {
            if(selected==null){detailTitle.text="NO MATCHING MISSIONS"; detailStatus.text="";detailBody.text="Change the filters to continue.";detailPreview.sprite=null;detailPreview.color=F.Surface;detailPlaceholder.gameObject.SetActive(true);action.interactable=false;actionLabel.text="NO SELECTION";return;}
            var e=selected;var state=states[e.key];detailTitle.text=$"{e.number:000}  /  {e.title}";
            detailStatus.color=Accent;detailStatus.text=catalog.ChapterName(e.chapter)+"  /  "+(!e.Present||e.blocked?"UNAVAILABLE":state.status.ToString().ToUpperInvariant());
            if(loadedBannerPath!=e.bannerResourcePath){loadedBannerPath=e.bannerResourcePath;loadedBanner=string.IsNullOrEmpty(loadedBannerPath)?null:Resources.Load<Sprite>(loadedBannerPath);}
            var artwork=loadedBanner!=null?loadedBanner:e.thumbnail;
            detailPreview.sprite=artwork;detailPreview.color=artwork!=null?Color.white:F.Surface;detailPlaceholder.gameObject.SetActive(artwork==null);
            var b=new System.Text.StringBuilder(512); b.AppendLine(e.description??""); b.AppendLine(); b.AppendLine("MISSION REQUIREMENTS");
            b.AppendLine($"Configured targets: {e.targets.Count}  |  Required count: {e.requiredTargets}");
            b.AppendLine("Original completion mode: "+e.rawCompletionMode); b.AppendLine("Legacy ID: "+(e.legacyId??"—"));
            if(!string.IsNullOrEmpty(e.issue))b.AppendLine("SOURCE REVIEW: "+e.issue);
            if(!string.IsNullOrEmpty(state.reason))b.AppendLine("ACCESS: "+state.reason);
            if(state.status!=SparkAAAStatus.Unknown)b.AppendLine($"Verified progress: {Mathf.RoundToInt(Mathf.Clamp01(state.fraction)*100)}%  |  Rank: {state.rank??"—"}");
            b.AppendLine();for(int i=0;i<e.targets.Count;i++){var t=e.targets[i];b.AppendLine($"{i+1:00}  {t.title}"); if(!string.IsNullOrWhiteSpace(t.description))b.AppendLine(t.description.Trim());b.AppendLine();}
            detailBody.text=b.ToString();
            bool can=e.Present&&!e.blocked&&gateway!=null&&gateway.isActiveAndEnabled&&state.canLaunch&&state.status!=SparkAAAStatus.Unknown&&state.status!=SparkAAAStatus.Locked;
            if(gateway is SparkAAAEventGateway ev && ev.LaunchPending)can=false;
            action.interactable=can;action.image.color=can?Accent:F.Outline;actionLabel.color=can?F.C("#07151C"):F.Muted;
            actionLabel.text=!e.Present||e.blocked?"SOURCE UNAVAILABLE":state.status==SparkAAAStatus.Unknown?"ACCESS NOT VERIFIED":state.status==SparkAAAStatus.Locked?"MISSION LOCKED":!can?"LAUNCH NOT READY":state.status==SparkAAAStatus.InProgress?"CONTINUE MISSION":state.status==SparkAAAStatus.Completed||state.status==SparkAAAStatus.Certified?"REPLAY MISSION":"INITIALIZE MISSION";
        }
        private void Launch()
        {
            if(selected==null||gateway==null)return;
            try{notice.text=gateway.TryLaunch(selected,out var reason)?"REQUEST SENT TO GAMEPLAY MANAGER":reason??"Launch rejected.";}catch(Exception ex){notice.text=ex.Message;}
            ShowDetail();
        }
        private void OnRectTransformDimensionsChange(){QueueResize();}
        private void QueueResize(){if(ready&&resize==null&&isActiveAndEnabled)resize=StartCoroutine(Resize());}
        private IEnumerator Resize(){yield return null; resize=null;if(!ready)yield break; float w=((RectTransform)transform).rect.width; compact=w<1130; header.fontSize=compact?24:32; header.text=compact?"SPARK / MISSION CONTROL":"PROJECT SPARK  /  MISSION CONTROL";ApplyMode();Canvas.ForceUpdateCanvases();float actual=gridScroll.viewport.rect.width; if(actual>20){int cols=Mathf.Clamp(Mathf.FloorToInt((actual-12)/260),1,5);gridLayout.constraintCount=cols;float width=Mathf.Max(80,(actual-24-(cols-1)*12)/cols);gridLayout.cellSize=new Vector2(width,SparkAAALevelCard.HeightForWidth(width));LayoutRebuilder.MarkLayoutForRebuild(grid);} }
        private void ApplyMode(){sidebar.gameObject.SetActive(!compact);main.gameObject.SetActive(!compact||!compactDetails);details.gameObject.SetActive(!compact||compactDetails);back.gameObject.SetActive(compact&&compactDetails);var el=details.GetComponent<LayoutElement>();el.preferredWidth=compact?-1:430;el.flexibleWidth=compact?1:0;}
        public static SparkAAAMissionPanel Create(SparkAAACatalog data,SparkAAALevelCard prefab)
        {
            var root=F.Rect("SparkLevelSelectionPanel",null);F.Stretch(root);F.Image(root,F.C("#080D13"));F.Vertical(root,18,10);
            var panel=root.gameObject.AddComponent<SparkAAAMissionPanel>();panel.catalog=data;panel.cardPrefab=prefab;
            var titleRow=F.Rect("Header",root);F.Size(titleRow,-1,55);F.Horizontal(titleRow,0,10);
            panel.header=F.Text("Title",titleRow,"PROJECT SPARK  /  MISSION CONTROL",32,Themes[2],-1,true);F.Size(panel.header.rectTransform,-1,-1,1);
            panel.themeNext=F.Button("Theme",titleRow,"BLUE",out panel.themeLabel,112,48);
            panel.stats=F.Text("CampaignStats",root,"CAMPAIGN DATA",19,F.Muted,32);
            panel.accent=F.Image(F.Rect("HeaderLine",root),Themes[2]);F.Size(panel.accent.rectTransform,-1,2);
            var body=F.Rect("Body",root);F.Size(body,-1,-1,1,1);F.Horizontal(body,0,12);
            panel.sidebar=F.Rect("DivisionSidebar",body);F.Size(panel.sidebar,240,-1,0,1);F.Image(panel.sidebar,F.Surface);F.Vertical(panel.sidebar,8,6);
            F.Text("DivisionLabel",panel.sidebar,"ENGINEERING DIVISIONS",17,F.Muted,30,true);
            F.Scroll("DivisionScroll",panel.sidebar,out var divisionContent);F.Vertical(divisionContent,4,5);
            panel.chapterButtons=new Button[data.chapters.Count+1];panel.chapterButtons[0]=F.Button("All",divisionContent,"ALL MISSIONS",out _, -1,43);
            for(int i=0;i<data.chapters.Count;i++)panel.chapterButtons[i+1]=F.Button("Division"+data.chapters[i].id,divisionContent,data.chapters[i].title,out _, -1,65);
            panel.main=F.Rect("MissionCollection",body);F.Size(panel.main,-1,-1,1,1);F.Vertical(panel.main,0,8);
            panel.search=F.Search(panel.main);
            var filters=F.Rect("Filters",panel.main);F.Size(filters,-1,52);F.Horizontal(filters,0,8);
            panel.chapterNext=F.Button("CycleDivision",filters,"ALL DIVISIONS",out panel.chapterLabel);panel.filterNext=F.Button("CycleStatus",filters,"ALL",out panel.filterLabel);
            panel.gridScroll=F.Scroll("LevelScroll",panel.main,out panel.grid);panel.gridLayout=panel.grid.gameObject.AddComponent<GridLayoutGroup>();panel.gridLayout.padding=new RectOffset(12,12,12,12);panel.gridLayout.spacing=new Vector2(12,12);panel.gridLayout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;panel.gridLayout.constraintCount=2;panel.gridLayout.cellSize=new Vector2(280,340);
            var pages=F.Rect("Pages",panel.main);F.Size(pages,-1,50);F.Horizontal(pages,0,8);panel.previousPage=F.Button("Previous",pages,"PREV",out _,80);panel.pageLabel=F.Text("Page",pages,"1 / 1",18,F.Muted);F.Size(panel.pageLabel.rectTransform,-1,-1,1);panel.pageLabel.alignment=TextAlignmentOptions.Center;panel.nextPage=F.Button("Next",pages,"NEXT",out _,80);
            panel.details=F.Rect("MissionDetail",body);F.Image(panel.details,F.Surface);F.Size(panel.details,430,-1,0,1);F.Vertical(panel.details,10,8);
            panel.back=F.Button("Back",panel.details,"BACK TO MISSIONS",out _,-1,44);panel.back.gameObject.SetActive(false);
            F.Text("DetailHeader",panel.details,"MISSION INTELLIGENCE",19,Themes[2],30,true);
            panel.detailScroll=F.Scroll("DetailScroll",panel.details,out var detailContent);F.Vertical(detailContent,10,9);
            panel.detailPreviewBox=F.Rect("HardwarePreview",detailContent);F.Size(panel.detailPreviewBox,-1,170);panel.detailPreview=F.Image(panel.detailPreviewBox,F.Surface);panel.detailPreview.preserveAspect=true;
            panel.detailPlaceholder=F.Text("Placeholder",panel.detailPreviewBox,"HARDWARE PREVIEW\nNo approved thumbnail",22,F.Muted);F.Stretch(panel.detailPlaceholder.rectTransform,10,10,10,10);panel.detailPlaceholder.alignment=TextAlignmentOptions.Center;
            panel.detailTitle=F.Text("MissionTitle",detailContent,"SELECT A MISSION",29,F.Ink,-1,true);panel.detailTitle.overflowMode=TextOverflowModes.Overflow;
            panel.detailStatus=F.Text("Status",detailContent,"",18,Themes[2]);panel.detailStatus.overflowMode=TextOverflowModes.Overflow;
            panel.detailBody=F.Text("Requirements",detailContent,"",21,F.Ink);panel.detailBody.alignment=TextAlignmentOptions.TopLeft;panel.detailBody.overflowMode=TextOverflowModes.Overflow;
            panel.notice=F.Text("LaunchNotice",panel.details,"",17,F.C("#E9BA78"),56);panel.action=F.Button("InitializeMission",panel.details,"ACCESS NOT VERIFIED",out panel.actionLabel,-1,56);panel.action.interactable=false;
            F.Text("Footer",root,"ENGINEERING CAMPAIGN  /  AUTHORITATIVE GAMEPLAY CONNECTION REQUIRED",16,F.Muted,26);
            return panel;
        }
    }
}
