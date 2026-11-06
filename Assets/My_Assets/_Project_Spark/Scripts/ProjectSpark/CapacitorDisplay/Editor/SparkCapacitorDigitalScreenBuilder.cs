#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectSpark.Electrical;
using ProjectSpark.Gameplay;

namespace ProjectSpark.EditorTools
{
    /// <summary>
    /// Creates an editable world-space display on the selected capacitor scene instance.
    /// The screen is offset above the component, never affects electrical terminals or wires.
    /// </summary>
    public static class SparkCapacitorDigitalScreenBuilder
    {
        private const string RootName = "Spark_Capacitor_Digital_UI";
        private const string MenuRoot = "Tools/Project Spark/Electrical/Capacitor Display/";
        private static readonly Color Background = C("05161F");
        private static readonly Color Panel = C("082733");
        private static readonly Color Border = C("11778C");
        private static readonly Color Cyan = C("31E6F3");
        private static readonly Color Green = C("42F7A9");
        private static readonly Color White = C("DEFAFF");
        private static readonly Color Muted = C("8CB7C4");

        [MenuItem(MenuRoot + "Build on Selected Capacitor")]
        public static void Build()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || EditorUtility.IsPersistent(selected))
            { EditorUtility.DisplayDialog("Capacitor UI", "Select the capacitor instance in a scene or prefab stage hierarchy.", "OK"); return; }
            SparkCapacitor capacitor = selected.GetComponent<SparkCapacitor>();
            if (capacitor == null) capacitor = selected.GetComponentInChildren<SparkCapacitor>(true);
            if (capacitor == null)
            { EditorUtility.DisplayDialog("Capacitor UI", "Selected object must contain SparkCapacitor.", "OK"); return; }
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
            { EditorUtility.DisplayDialog("Capacitor UI", "Import TMP Essentials: Window > TextMeshPro > Import TMP Essential Resources.", "OK"); return; }
            Transform old = selected.transform.Find(RootName);
            if (old != null && !EditorUtility.DisplayDialog("Rebuild capacitor screen?", "Existing generated UI will be replaced. Your electrical components and wires will not be changed.", "Rebuild", "Cancel")) return;
            Undo.RegisterFullObjectHierarchyUndo(selected, "Build capacitor digital display");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            var source = selected.GetComponent<SparkCapacitorDigitalDisplay>();
            if (source == null) source = Undo.AddComponent<SparkCapacitorDigitalDisplay>(selected);
            SparkElectricalSolver solver = Object.FindFirstObjectByType<SparkElectricalSolver>();
            SerializedObject srcSO = new SerializedObject(source);
            Set(srcSO, "capacitor", capacitor);
            if (solver != null) Set(srcSO, "solver", solver);
            srcSO.ApplyModifiedProperties();

            RectTransform root = NewRect(selected.transform, RootName);
            root.localPosition = new Vector3(0, 0.22f, 0.055f);
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one * 0.00036f;
            root.sizeDelta = new Vector2(980, 600);
            Canvas canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 155;
            root.gameObject.AddComponent<GraphicRaycaster>();
            // No EventSystem is added: use your scene's existing EventSystem and UI input module.
            Block(root, "Back",0,0,980,600,Border);
            Block(root,"Inset",4,4,972,592,Background);
            Block(root,"Header",12,12,956,64,Panel);
            TMP_Text title = Text(root,"Title","PROJECT SPARK  /  CAPACITOR DIGITAL ANALYZER",24,17,720,54,31,White,font);
            TMP_Text status = Text(root,"Status","WAITING",750,22,200,45,24,Green,font);
            Block(root,"Main",12,87,608,185,Panel);
            TMP_Text value = Text(root,"Major Value","--.---",30,103,435,125,92,Cyan,font);
            TMP_Text unit = Text(root,"Major Unit","V",470,135,125,65,51,Cyan,font);
            TMP_Text detail = Text(root,"Detail","AWAITING ACCEPTED ELECTRICAL SOLVE",23,230,588,35,19,Muted,font);
            Block(root,"Progress",634,87,334,185,Panel);
            Text(root,"Charge Header","CHARGE / TARGET",654,104,290,34,23,Muted,font);
            TMP_Text percentage = Text(root,"Charge Percent","-- %",654,145,290,65,52,Green,font);
            RectTransform barBack = Block(root,"Bar Background",656,218,288,26,Border);
            Image progress = Block(barBack,"Charge Fill",3,3,282,20,Green).GetComponent<Image>();
            progress.type = Image.Type.Filled;
            progress.fillMethod = Image.FillMethod.Horizontal;
            progress.fillAmount = 0f;
            Block(root,"Metrics",12,284,956,147,Panel);
            Text(root,"Labels","VOLTAGE                      CURRENT                       CAPACITANCE               RATED VOLTAGE",26,302,920,32,17,Muted,font);
            TMP_Text v = Text(root,"Voltage","--",25,340,205,65,32,Cyan,font);
            TMP_Text i = Text(root,"Current","--",250,340,205,65,32,Cyan,font);
            TMP_Text cap = Text(root,"Capacitance","--",490,340,215,65,32,Cyan,font);
            TMP_Text rated = Text(root,"Rated Voltage","--",735,340,210,65,32,Cyan,font);
            Block(root,"Energy Panel",12,440,475,73,Panel);
            Block(root,"Charge Panel",494,440,474,73,Panel);
            TMP_Text energy = Text(root,"Energy","ENERGY  --",25,453,450,53,29,Green,font);
            TMP_Text charge = Text(root,"Charge","Q  --",509,453,445,53,29,Green,font);
            RectTransform graphPanel = Block(root,"Live History",12,525,610,59,Panel);
            Image[] history = new Image[32];
            for (int b = 0; b < history.Length; b++)
            {
                RectTransform rt = Block(graphPanel,"History " + b,10 + b*18.4f,53,13f,2f,Border);
                rt.pivot = new Vector2(0,0);
                rt.anchoredPosition = new Vector2(10 + b*18.4f, -53);
                history[b] = rt.GetComponent<Image>();
            }
            string[] labels = { "OVERVIEW", "VOLTAGE", "CURRENT", "ENERGY" };
            Button[] buttons = new Button[4];
            for (int b = 0; b < 4; b++)
            {
                float x = 634 + (b%2)*168;
                float y = 524 + (b/2)*33;
                var rt = Block(root,"Button " + labels[b],x,y,160,29,Border);
                var bg = rt.GetComponent<Image>(); bg.raycastTarget = true;
                buttons[b] = rt.gameObject.AddComponent<Button>();
                buttons[b].targetGraphic = bg;
                buttons[b].navigation = new Navigation { mode = Navigation.Mode.None };
                Text(rt,"Label",labels[b],5,3,150,24,15,White,font);
            }
            var skin = selected.GetComponent<SparkCapacitorDigitalScreen>();
            if (skin == null) skin = Undo.AddComponent<SparkCapacitorDigitalScreen>(selected);
            SerializedObject uiSO = new SerializedObject(skin);
            Set(uiSO,"telemetry",source);
            Set(uiSO,"titleText",title); Set(uiSO,"statusText",status);
            Set(uiSO,"valueText",value); Set(uiSO,"unitText",unit);
            Set(uiSO,"voltageText",v); Set(uiSO,"currentText",i);
            Set(uiSO,"capacitanceText",cap); Set(uiSO,"ratedText",rated);
            Set(uiSO,"percentageText",percentage); Set(uiSO,"energyText",energy);
            Set(uiSO,"chargeText",charge); Set(uiSO,"detailText",detail);
            Set(uiSO,"chargeFill",progress);
            SetArray(uiSO,"historyBars",history);
            SetArray(uiSO,"pageButtons",buttons);
            uiSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(selected);
            Selection.activeGameObject = selected;
            SceneView.RepaintAll();
            Debug.Log("[Capacitor Display] Built world-space capacitor UI. Set screen offset/rotation; assign actual solver if auto-resolution did not find one. No electrical topology was changed.", selected);
        }
        [MenuItem(MenuRoot + "Remove Generated UI from Selection")]
        public static void RemoveGenerated()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) return;
            Transform generated = selected.transform.Find(RootName);
            if (generated != null) Undo.DestroyObjectImmediate(generated.gameObject);
            Debug.Log("[Capacitor Display] Generated UI removed. Runtime components retained.",selected);
        }
        private static void Set(SerializedObject so,string key,Object value)
        { SerializedProperty p=so.FindProperty(key); if(p!=null) p.objectReferenceValue=value; }
        private static void SetArray<T>(SerializedObject so,string key,T[] values) where T : Object
        { SerializedProperty p=so.FindProperty(key); if(p==null)return; p.arraySize=values.Length; for(int j=0;j<values.Length;j++) p.GetArrayElementAtIndex(j).objectReferenceValue=values[j]; }
        private static RectTransform NewRect(Transform parent,string name)
        {
            GameObject go=new GameObject(name,typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go,"Create capacitor UI");
            go.transform.SetParent(parent,false);
            RectTransform rt=go.GetComponent<RectTransform>();
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);
            rt.pivot=new Vector2(0,1);
            return rt;
        }
        private static RectTransform Block(RectTransform parent,string name,float x,float y,float w,float h,Color color)
        {
            RectTransform rt=NewRect(parent,name);
            rt.anchoredPosition=new Vector2(x,-y);
            rt.sizeDelta=new Vector2(w,h);
            Image img=rt.gameObject.AddComponent<Image>();img.color=color;img.raycastTarget=false;
            return rt;
        }
        private static TMP_Text Text(RectTransform parent,string name,string content,float x,float y,float w,float h,float size,Color color,TMP_FontAsset font)
        {
            RectTransform rt=NewRect(parent,name);
            rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);
            var t=rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font=font;t.text=content;t.fontSize=size;t.color=color;t.fontStyle=FontStyles.Bold;
            t.enableWordWrapping=false;t.overflowMode=TextOverflowModes.Ellipsis;t.raycastTarget=false;
            return t;
        }
        private static Color C(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out Color c);return c; }
    }
}
#endif
