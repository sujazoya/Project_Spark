#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.UI.VisualTeaching;

public static class SparkVisualTeachingBuilder
{
    private static readonly Color Deep = new Color(.022f,.075f,.13f,.98f);
    private static readonly Color Dark = new Color(.05f,.14f,.22f,1f);
    private static readonly Color Blue = new Color(.06f,.36f,.58f,1f);
    private static readonly Color Cyan = new Color(.20f,.84f,1f,1f);
    private static readonly Color White = new Color(.94f,.98f,1f,1f);
    private static readonly Color Muted = new Color(.59f,.73f,.82f,1f);
    private static readonly Color Gold = new Color(1f,.80f,.31f,1f);

    [MenuItem("Tools/Project Spark/AI/Create Visual Teaching System (AAA)")]
    public static void Create()
    {
        GameObject old = GameObject.Find("SparkAIVisualTeachingCanvas");
        if (old != null)
        {
            Selection.activeGameObject = old;
            Debug.LogWarning("[Spark Teaching] Canvas already exists. Selected existing instance.", old);
            return;
        }

        GameObject canvasGo = new GameObject("SparkAIVisualTeachingCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create Spark AI Visual Teaching System");
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 690;
        CanvasScaler scale = canvasGo.GetComponent<CanvasScaler>();
        scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scale.referenceResolution = new Vector2(1920, 1080);
        scale.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scale.matchWidthOrHeight = .5f;

        // World annotations are fully click-through.
        GameObject world = UI("WorldOverlay", canvasGo.transform);
        Undo.AddComponent<CanvasRenderer>(world);
        Stretch(world.GetComponent<RectTransform>());
        SparkTeachingVectorGraphic worldGraphic = Undo.AddComponent<SparkTeachingVectorGraphic>(world);
        worldGraphic.raycastTarget = false;
        SparkWorldAnnotationRenderer worldRenderer = Undo.AddComponent<SparkWorldAnnotationRenderer>(world);
        SerializedObject wr = new SerializedObject(worldRenderer);
        Ref(wr,"drawArea",world.GetComponent<RectTransform>());
        Ref(wr,"vectorGraphic",worldGraphic);
        Ref(wr,"canvas",canvas);
        wr.ApplyModifiedProperties();

        // Always visible launcher if the whiteboard is hidden.
        GameObject launcher = UI("OpenVisualTeacherButton",canvasGo.transform);
        RightTop(launcher.GetComponent<RectTransform>(),54,56,-14,-156);
        Button launcherButton = MakeButton(launcher,"AI",Cyan,Deep,19);
        Outline launcherOutline = launcher.AddComponent<Outline>();
        launcherOutline.effectColor=Cyan;

        GameObject board = UI("WhiteboardPanel",canvasGo.transform);
        RightTop(board.GetComponent<RectTransform>(),680,970,-22,-22);
        Image bg = board.AddComponent<Image>();
        bg.color = Deep;
        bg.raycastTarget = true;
        Outline border = board.AddComponent<Outline>();
        border.effectColor = Cyan;
        border.effectDistance = new Vector2(2,-2);
        CanvasGroup group = board.AddComponent<CanvasGroup>();

        TMP_Text title = Text(board.transform,"Title","SPARK AI  /  VISUAL TEACHER",26,White,20,-16,570,43);
        Text(board.transform,"Subtitle","SCHEMATIC • WORLD ANNOTATIONS • GUIDED LEARNING",14,Cyan,20,-58,600,24);
        Button close = Button(board.transform,"CloseButton","X",610,-17,48,47,new Color(.48f,.12f,.17f));
        Panel(board.transform,"HeaderLine",Cyan,20,-89,638,2,false);

        Button both = Button(board.transform,"BothButton","BOTH VIEWS",20,-106,206,42,Blue);
        Button white = Button(board.transform,"WhiteboardButton","WHITEBOARD",237,-106,206,42,Blue);
        Button onlyWorld = Button(board.transform,"WorldButton","3D WORLD",454,-106,204,42,Blue);

        Panel(board.transform,"GoalCard",Dark,20,-165,638,102,false);
        TMP_Text goal = Text(board.transform,"GoalText","Visual circuit explanation",21,White,32,-175,594,39);
        TMP_Text step = Text(board.transform,"StepText","STEP 1 / 3",17,Cyan,32,-219,591,34);
        Panel(board.transform,"SchematicFrame",new Color(.025f,.10f,.18f),20,-279,638,354,false);

        GameObject viewport = UI("SchematicViewport",board.transform);
        RectTransform viewRect = viewport.GetComponent<RectTransform>();
        TopLeft(viewRect,28,-287,622,338);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color=new Color(.02f,.08f,.14f,1f);
        viewportImage.raycastTarget=true;
        viewport.AddComponent<RectMask2D>();
       GameObject diagram = UI("DiagramContent", viewport.transform);
        Stretch(diagram.GetComponent<RectTransform>());
        Undo.AddComponent<CanvasRenderer>(diagram);
        SparkTeachingVectorGraphic schematicGraphic = Undo.AddComponent<SparkTeachingVectorGraphic>(diagram);
        schematicGraphic.raycastTarget=false;
        SparkSchematicRenderer schematic = Undo.AddComponent<SparkSchematicRenderer>(viewport);
        SerializedObject sr = new SerializedObject(schematic);
        Ref(sr,"viewport",viewRect);
        Ref(sr,"diagramContent",diagram.GetComponent<RectTransform>());
        Ref(sr,"lines",schematicGraphic);
        sr.ApplyModifiedProperties();

        TMP_Text caption = Text(board.transform,"DiagramCaption","No verified schematic loaded",16,Muted,26,-636,626,30);
        Ref(sr,"captionText",caption);
        sr.ApplyModifiedProperties();

        Panel(board.transform,"ProgressBackground",Dark,25,-676,628,12,false);
        GameObject fillGo = Panel(board.transform,"ProgressFill",Cyan,25,-676,628,12,false);
        Image fill = fillGo.GetComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = .0f;
        TMP_Text instruction = Text(board.transform,"InstructionText","Choose a lesson or run DEMO.",20,White,25,-705,628,57);
        TMP_Text hint = Text(board.transform,"HintText","Hint will appear here.",16,Gold,25,-771,628,39);
        TMP_Text status = Text(board.transform,"StatusText","Ready",15,Muted,25,-815,628,34);

        Button previous=Button(board.transform,"PreviousButton","PREVIOUS",20,-861,152,44,Blue);
        Button next=Button(board.transform,"NextButton","NEXT",183,-861,152,44,Blue);
        Button replay=Button(board.transform,"ReplayButton","REPLAY",346,-861,152,44,Blue);
        Button pause=Button(board.transform,"PauseButton","PAUSE",509,-861,149,44,Blue);
        TMP_Text pauseText = pause.GetComponentInChildren<TMP_Text>();
        Button clear=Button(board.transform,"ClearButton","CLEAR",20,-919,119,38,new Color(.37f,.17f,.20f));
        Button demo=Button(board.transform,"DemoButton","DEMO",151,-919,120,38,Blue);
        Button zoomIn=Button(board.transform,"ZoomInButton","ZOOM +",282,-919,120,38,Blue);
        Button zoomOut=Button(board.transform,"ZoomOutButton","ZOOM -",414,-919,120,38,Blue);
        Button zoomReset=Button(board.transform,"ZoomResetButton","RESET VIEW",546,-919,112,38,Blue);

        SparkVisualTeachingManager manager = Undo.AddComponent<SparkVisualTeachingManager>(canvasGo);
        SerializedObject mo = new SerializedObject(manager);
        Ref(mo,"worldRenderer",worldRenderer);
        Ref(mo,"schematicRenderer",schematic);
        Ref(mo,"worldOverlay",world);
        Ref(mo,"whiteboardGroup",group);
        Ref(mo,"launcher",launcher);
        Ref(mo,"titleText",title);
        Ref(mo,"goalText",goal);
        Ref(mo,"stepText",step);
        Ref(mo,"instructionText",instruction);
        Ref(mo,"hintText",hint);
        Ref(mo,"statusText",status);
        Ref(mo,"progressFill",fill);
        Ref(mo,"pauseButtonText",pauseText);
        mo.ApplyModifiedProperties();

        SparkTeachingIntegrationAdapter adapter = Undo.AddComponent<SparkTeachingIntegrationAdapter>(canvasGo);
        SerializedObject ao = new SerializedObject(adapter);
        Ref(ao,"teachingManager",manager);
        ao.ApplyModifiedProperties();

        Wire(launcherButton, manager.OpenTeachingBoard);
        Wire(close, manager.ClosePanel);
        Wire(both, manager.ViewBoth);
        Wire(white, manager.ViewWhiteboard);
        Wire(onlyWorld, manager.ViewWorld);
        Wire(previous, manager.PreviousStep);
        Wire(next, manager.NextStep);
        Wire(replay, manager.ReplayStep);
        Wire(pause, manager.TogglePause);
        Wire(clear, manager.ClearEverything);
        Wire(demo, manager.PlayDemoLesson);
        Wire(zoomIn, schematic.ZoomIn);
        Wire(zoomOut, schematic.ZoomOut);
        Wire(zoomReset, schematic.ResetView);

        Selection.activeGameObject=canvasGo;
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(adapter);
        Debug.Log("[Project Spark] Advanced Visual Teaching canvas created. Press DEMO during Play Mode.",canvasGo);
    }

    private static void Wire(Button button, UnityEngine.Events.UnityAction callback)
    {
        UnityEventTools.AddPersistentListener(button.onClick,callback);
        EditorUtility.SetDirty(button);
    }

    private static GameObject UI(string name,Transform parent)
    {
        GameObject go = new GameObject(name,typeof(RectTransform));
        go.transform.SetParent(parent,false);
        return go;
    }

    private static void Ref(SerializedObject o,string field,Object obj)
    {
        SerializedProperty p=o.FindProperty(field);
        if (p!=null) p.objectReferenceValue=obj;
        else Debug.LogError("[Spark Teaching Builder] Missing field " + field);
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin=Vector2.zero;
        rt.anchorMax=Vector2.one;
        rt.pivot=new Vector2(.5f,.5f);
        rt.offsetMin=Vector2.zero;
        rt.offsetMax=Vector2.zero;
    }

    private static void RightTop(RectTransform rt,float w,float h,float x,float y)
    {
        rt.anchorMin=rt.anchorMax=rt.pivot=Vector2.one;
        rt.sizeDelta=new Vector2(w,h);
        rt.anchoredPosition=new Vector2(x,y);
    }

    private static void TopLeft(RectTransform rt,float x,float y,float w,float h)
    {
        rt.anchorMin=rt.anchorMax=new Vector2(0,1);
        rt.pivot=new Vector2(0,1);
        rt.anchoredPosition=new Vector2(x,y);
        rt.sizeDelta=new Vector2(w,h);
    }

    private static GameObject Panel(Transform parent,string name,Color color,float x,float y,float w,float h,bool raycast)
    {
        GameObject go=UI(name,parent);
        TopLeft(go.GetComponent<RectTransform>(),x,y,w,h);
        Image image=go.AddComponent<Image>();
        image.color=color;
        image.raycastTarget=raycast;
        return go;
    }

    private static TMP_Text Text(Transform parent,string name,string value,int fontSize,Color color,
        float x,float y,float w,float h)
    {
        GameObject go=UI(name,parent);
        TopLeft(go.GetComponent<RectTransform>(),x,y,w,h);
        TextMeshProUGUI label=go.AddComponent<TextMeshProUGUI>();
        label.text=value;
        label.fontSize=fontSize;
        label.enableAutoSizing=true;
        label.fontSizeMin=11;
        label.fontSizeMax=fontSize;
        label.color=color;
        label.alignment=TextAlignmentOptions.MidlineLeft;
        label.raycastTarget=false;
        return label;
    }

    private static Button MakeButton(GameObject go,string label,Color background,Color foreground,int fontSize)
    {
        Image bg=go.AddComponent<Image>();
        bg.color=background;
        Button button=go.AddComponent<Button>();
        button.targetGraphic=bg;
        ColorBlock colors=button.colors;
        colors.highlightedColor=new Color(.75f,.94f,1f);
        colors.pressedColor=new Color(.48f,.82f,1f);
        button.colors=colors;
        GameObject labelGo=UI("Label",go.transform);
        Stretch(labelGo.GetComponent<RectTransform>());
        TextMeshProUGUI t=labelGo.AddComponent<TextMeshProUGUI>();
        t.text=label;
        t.fontSize=fontSize;
        t.fontStyle=FontStyles.Bold;
        t.color=foreground;
        t.alignment=TextAlignmentOptions.Center;
        t.raycastTarget=false;
        return button;
    }

    private static Button Button(Transform parent,string name,string label,float x,float y,float w,float h,Color color)
    {
        GameObject go=UI(name,parent);
        TopLeft(go.GetComponent<RectTransform>(),x,y,w,h);
        return MakeButton(go,label,color,White,16);
    }
}
#endif
