#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSpark.UI;

public static class ProjectSparkDualModeToolsBuilder
{
    private static readonly Color Navy = new Color(.018f,.065f,.12f,.98f);
    private static readonly Color Cyan = new Color(.07f,.75f,1f,1f);
    private static readonly Color Pale = new Color(.86f,.96f,1f,1f);
    private static readonly Color ButtonColor = new Color(.045f,.19f,.30f,1f);
    private static readonly string[] Names = {"Select", "Inspect", "Move", "Rotate", "Wire", "Measure", "Scan", "Screen Control", "Reset Scene"};
    private static readonly string[] Icons = {"➤", "⌕", "✥", "⟳", "⌁", "▤", "◎", "▣", "↻"};
    private static readonly string[] Keys = {"1", "2", "3", "4", "5", "6", "7", "8", "R"};

    [MenuItem("Tools/Project Spark/Tools/Create Dual-Mode Tools Panel")]
    public static void Build()
    {
        Transform selected = Selection.activeTransform;
        Canvas canvas = selected != null ? selected.GetComponentInParent<Canvas>() : null;
        if (canvas == null)
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in canvases) if (c.name == "Main_HUD") { canvas = c; break; }
        }
        if (canvas == null) { Debug.LogError("[Dual Tools] Select Main_HUD Canvas first."); return; }
        if (canvas.transform.Find("SparkDualModeTools") != null)
        {
            Debug.LogWarning("[Dual Tools] SparkDualModeTools already exists. Delete it manually if rebuilding.", canvas);
            return;
        }

        GameObject root = NewUI("SparkDualModeTools", canvas.transform);
        Undo.RegisterCreatedObjectUndo(root, "Create Dual-Mode Tools Panel");
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1,.5f);
        rt.pivot = new Vector2(1,.5f);
        rt.anchoredPosition = new Vector2(-18,0);
        rt.sizeDelta = new Vector2(86,790);
        root.AddComponent<Image>().color = Navy;
        Outline edge = root.AddComponent<Outline>();
        edge.effectColor = Cyan;
        edge.effectDistance = new Vector2(2,-2);

        GameObject brand = NewUI("ExpandedHeader", root.transform);
        TopLeft(brand.GetComponent<RectTransform>(),9,-12,292,58);
        AddText("Brand",brand.transform,"PROJECT SPARK",23,Pale,0,0,292,30,TextAlignmentOptions.Center);
        AddText("Caption",brand.transform,"TOOLS PANEL",14,Cyan,0,-30,292,24,TextAlignmentOptions.Center);

        Button toggle = AddButton("ModeToggle",root.transform,"<",-48, -360,46,76,new Color(.04f,.45f,.70f,1f),out TMP_Text toggleText);
        RectTransform toggleRect = toggle.GetComponent<RectTransform>();
        toggleRect.anchorMin = toggleRect.anchorMax = new Vector2(0,.5f);
        toggleRect.pivot = new Vector2(0,.5f);
        toggleRect.anchoredPosition = new Vector2(-46,0);
        toggleRect.sizeDelta = new Vector2(46,76);

        ProjectSparkDualModeToolsPanel.ToolAction[] actions = new ProjectSparkDualModeToolsPanel.ToolAction[Names.Length];
        for (int i=0; i<Names.Length; i++)
        {
            float y = -85 - i*75;
            GameObject row = NewUI(Names[i].Replace(" ","")+"Button",root.transform);
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0,1);
            rowRect.anchorMax = new Vector2(1,1);
            rowRect.pivot = new Vector2(.5f,1);
            rowRect.offsetMin = new Vector2(8, y-64);
            rowRect.offsetMax = new Vector2(-8, y);
            Image image = row.AddComponent<Image>(); image.color = ButtonColor;
            Button button = row.AddComponent<Button>(); button.targetGraphic = image;
            ColorBlock block = button.colors; block.highlightedColor = new Color(.68f,.91f,1f); block.pressedColor = new Color(.40f,.79f,1f); button.colors=block;

            // The icon occupies the fixed compact region. Labels appear to its right in expanded mode.
            AddText("Icon",row.transform,Icons[i],32,Cyan,3,-4,62,54,TextAlignmentOptions.Center);
            TMP_Text label = AddText("Label",row.transform,Names[i].ToUpperInvariant(),19,Pale,72,-4,167,54,TextAlignmentOptions.MidlineLeft);
            TMP_Text key = AddText("Shortcut",row.transform,Keys[i],15,Cyan,243,-4,35,54,TextAlignmentOptions.Center);
            label.gameObject.SetActive(false);
            key.gameObject.SetActive(false);
            actions[i] = new ProjectSparkDualModeToolsPanel.ToolAction {
                tool=(ProjectSparkDualModeToolsPanel.Tool)i,
                button=button, background=image, labelRoot=label.gameObject, shortcutRoot=key.gameObject
            };
        }
        ProjectSparkDualModeToolsPanel controller = Undo.AddComponent<ProjectSparkDualModeToolsPanel>(root);
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("drawer").objectReferenceValue=rt;
        so.FindProperty("toggleButton").objectReferenceValue=toggle;
        so.FindProperty("toggleText").objectReferenceValue=toggleText;
        so.FindProperty("expandedHeader").objectReferenceValue=brand;
        so.FindProperty("startExpanded").boolValue=false;
        so.FindProperty("compactWidth").floatValue=86f;
        so.FindProperty("expandedWidth").floatValue=310f;
        so.FindProperty("animationSeconds").floatValue=.3f;
        SerializedProperty array=so.FindProperty("actions"); array.arraySize=actions.Length;
        for(int i=0;i<actions.Length;i++)
        {
            SerializedProperty a=array.GetArrayElementAtIndex(i);
            a.FindPropertyRelative("tool").enumValueIndex=i;
            a.FindPropertyRelative("button").objectReferenceValue=actions[i].button;
            a.FindPropertyRelative("background").objectReferenceValue=actions[i].background;
            a.FindPropertyRelative("labelRoot").objectReferenceValue=actions[i].labelRoot;
            a.FindPropertyRelative("shortcutRoot").objectReferenceValue=actions[i].shortcutRoot;
        }
        so.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
        Selection.activeGameObject=root;
        Debug.Log("[Dual Tools] Created icon-only compact panel and animated expanded panel. Wire onActivated UnityEvents to your tool methods.",root);
    }

    private static GameObject NewUI(string name,Transform parent)
    {
        GameObject go = new GameObject(name,typeof(RectTransform));
        go.transform.SetParent(parent,false);
        return go;
    }
    private static void TopLeft(RectTransform rt,float x,float y,float w,float h)
    {
        rt.anchorMin=rt.anchorMax=new Vector2(0,1);
        rt.pivot=new Vector2(0,1);
        rt.anchoredPosition=new Vector2(x,y);
        rt.sizeDelta=new Vector2(w,h);
    }
    private static TMP_Text AddText(string name,Transform parent,string value,int size,Color color,float x,float y,float w,float h,TextAlignmentOptions align)
    {
        GameObject go=NewUI(name,parent); TopLeft(go.GetComponent<RectTransform>(),x,y,w,h);
        TextMeshProUGUI txt=go.AddComponent<TextMeshProUGUI>();
        txt.text=value; txt.fontSize=size; txt.color=color; txt.alignment=align; txt.raycastTarget=false;
        txt.enableAutoSizing=true; txt.fontSizeMin=11; txt.fontSizeMax=size;
        return txt;
    }
    private static Button AddButton(string name,Transform parent,string text,float x,float y,float w,float h,Color color,out TMP_Text label)
    {
        GameObject go=NewUI(name,parent); TopLeft(go.GetComponent<RectTransform>(),x,y,w,h);
        Image img=go.AddComponent<Image>(); img.color=color;
        Button button=go.AddComponent<Button>(); button.targetGraphic=img;
        label=AddText("Text",go.transform,text,32,Color.white,0,0,w,h,TextAlignmentOptions.Center);
        return button;
    }
}
#endif
