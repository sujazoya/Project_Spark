#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.UI.VisualTeaching;

public static class SparkElectricalTelemetryBuilder
{
    private static readonly Color Deep = new Color(.025f,.082f,.14f,.97f);
    private static readonly Color Panel = new Color(.06f,.15f,.25f,1f);
    private static readonly Color Cyan = new Color(.20f,.82f,1f,1f);
    private static readonly Color Blue = new Color(.07f,.38f,.62f,1f);
    private static readonly Color White = new Color(.94f,.98f,1f,1f);
    private static readonly Color Muted = new Color(.58f,.73f,.83f,1f);
    private static readonly Color Gold = new Color(1f,.82f,.31f,1f);

    [MenuItem("Tools/Project Spark/AI/Add Live Electrical Telemetry")]
    public static void Create()
    {
        SparkVisualTeachingManager teacher = null;
        if (Selection.activeGameObject != null)
            teacher = Selection.activeGameObject.GetComponentInParent<SparkVisualTeachingManager>();
        if (teacher == null)
            teacher = Object.FindFirstObjectByType<SparkVisualTeachingManager>(FindObjectsInactive.Include);
        if (teacher == null)
        {
            Debug.LogError("[Spark Telemetry] Create the AAA Visual Teaching canvas first.");
            return;
        }
        Canvas canvas = teacher.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Spark Telemetry] Existing teacher is not under a Canvas.", teacher);
            return;
        }

        // Never delete/rebuild existing AA teaching UI or bridge.
        Transform old = canvas.transform.Find("LiveElectricalTelemetryCard");
        if (old != null)
        {
            Selection.activeGameObject = old.gameObject;
            Debug.LogWarning("[Spark Telemetry] LiveElectricalTelemetryCard already exists. Selected it.", old);
            return;
        }

        SparkWorldAnnotationRenderer renderer =
            canvas.GetComponentInChildren<SparkWorldAnnotationRenderer>(true);

        GameObject card = UI("LiveElectricalTelemetryCard", canvas.transform);
        Undo.RegisterCreatedObjectUndo(card, "Create Spark Live Electrical Telemetry");
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(376f, 350f);
        rt.anchoredPosition = new Vector2(20f, -25f);
        Image bg = card.AddComponent<Image>();
        bg.color = Deep;
        bg.raycastTarget = true;
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = Cyan;
        outline.effectDistance = new Vector2(1f,-1f);

        TMP_Text title = Text(card.transform, "Title", "LIVE ELECTRICAL TELEMETRY", 21, White, 14,-12,347,33);
        title.fontStyle = FontStyles.Bold;
        TMP_Text source = Text(card.transform, "Source", "AWAITING SIMULATOR", 12, Cyan, 14,-44,347,25);
        Box(card.transform,"Divider",Cyan,14,-72,348,2);
        TMP_Text component = Text(card.transform, "Component", "NO COMPONENT SELECTED", 19, White, 14,-85,348,34);
        Box(card.transform,"Measurements", Panel, 14,-125,348,92);
        Text(card.transform,"VoltLabel", "VOLTAGE",13,Muted,23,-133,105,24);
        TMP_Text voltage = Text(card.transform,"Voltage", "— V",22,Gold,137,-129,216,29);
        Text(card.transform,"CurrentLabel", "CURRENT",13,Muted,23,-161,105,24);
        TMP_Text current = Text(card.transform,"Current", "— A",22,Cyan,137,-156,216,29);
        Text(card.transform,"ResistanceLabel", "RESISTANCE",13,Muted,23,-190,105,24);
        TMP_Text resistance = Text(card.transform,"Resistance", "— Ω",21,White,137,-184,216,29);
        TMP_Text condition = Text(card.transform,"Condition", "WAITING",18,Gold,14,-224,348,28);
        TMP_Text info = Text(card.transform,"Information", "Connect verified values from your solver.",13,Muted,14,-252,348,31);
        info.enableWordWrapping = true;

        Button showButton = Button(card.transform,"ShowWorldButton","SHOW 3D",14,-300,80,32,Blue);
        Button hideButton = Button(card.transform,"HideWorldButton","HIDE 3D",102,-300,80,32,Panel);
        Button clearButton = Button(card.transform,"ClearButton","CLEAR",190,-300,80,32,new Color(.39f,.15f,.20f));
        Button demoButton = Button(card.transform,"DemoButton","DEMO",278,-300,84,32,Panel);

        SparkElectricalTelemetryBridge bridge =
            teacher.GetComponent<SparkElectricalTelemetryBridge>();
        if (bridge == null)
            bridge = Undo.AddComponent<SparkElectricalTelemetryBridge>(teacher.gameObject);

        var so = new SerializedObject(bridge);
        Ref(so,"teacher",teacher);
        Ref(so,"worldRenderer",renderer);
        Ref(so,"telemetryCard",card);
        Ref(so,"sourceText",source);
        Ref(so,"componentText",component);
        Ref(so,"voltageText",voltage);
        Ref(so,"currentText",current);
        Ref(so,"resistanceText",resistance);
        Ref(so,"conditionText",condition);
        Ref(so,"informationText",info);
        so.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(showButton.onClick, bridge.ShowSelectedInWorld);
        UnityEventTools.AddPersistentListener(hideButton.onClick, bridge.HideWorldCues);
        UnityEventTools.AddPersistentListener(clearButton.onClick, bridge.ClearTelemetry);
        UnityEventTools.AddPersistentListener(demoButton.onClick, bridge.RunIllustrativeDemo);
        EditorUtility.SetDirty(showButton);
        EditorUtility.SetDirty(hideButton);
        EditorUtility.SetDirty(clearButton);
        EditorUtility.SetDirty(demoButton);
        EditorUtility.SetDirty(bridge);
        Selection.activeGameObject = card;

        Debug.Log("[Spark Telemetry] HUD created and wired to the existing AAA renderer. " +
                  "Next, publish verified solver results via ReportVerifiedReading().", card);
    }

    private static GameObject UI(string name, Transform parent)
    {
        GameObject o = new GameObject(name, typeof(RectTransform));
        o.transform.SetParent(parent, false);
        return o;
    }

    private static void Position(RectTransform rect,float x,float y,float w,float h)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0f,1f);
        rect.pivot = new Vector2(0f,1f);
        rect.anchoredPosition = new Vector2(x,y);
        rect.sizeDelta = new Vector2(w,h);
    }

    private static GameObject Box(Transform parent,string name,Color color,float x,float y,float w,float h)
    {
        GameObject o = UI(name,parent);
        Position(o.GetComponent<RectTransform>(),x,y,w,h);
        var image = o.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return o;
    }

    private static TMP_Text Text(Transform parent,string name,string value,int size,Color color,float x,float y,float w,float h)
    {
        GameObject o = UI(name,parent);
        Position(o.GetComponent<RectTransform>(),x,y,w,h);
        TextMeshProUGUI text = o.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.enableAutoSizing = true;
        text.fontSizeMin = 10;
        text.fontSizeMax = size;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
        return text;
    }

    private static Button Button(Transform parent,string name,string label,float x,float y,float w,float h,Color color)
    {
        GameObject o = UI(name,parent);
        Position(o.GetComponent<RectTransform>(),x,y,w,h);
        Image image = o.AddComponent<Image>();
        image.color = color;
        Button button = o.AddComponent<Button>();
        button.targetGraphic = image;
        var text = Text(o.transform,"Text",label,11,White,0,0,w,h);
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        return button;
    }

    private static void Ref(SerializedObject so,string name,Object value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property == null)
            Debug.LogError("[Spark Telemetry] Missing runtime field: " + name);
        else
            property.objectReferenceValue = value;
    }
}
#endif
