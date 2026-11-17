using UnityEngine;
using UnityEngine.UI;

namespace ProjectSpark.PCBVirtualStudio
{
    public sealed class SparkPCBToolButton : MonoBehaviour
    {
        [SerializeField] private SparkVirtualTool tool;
        [SerializeField] private int group;
        [SerializeField] private SparkVirtualPCBWorkstation workstation;
        [SerializeField] private SparkPCBRoundedPanelGraphic panel;
        [SerializeField] private Text label;

        public SparkVirtualTool Tool => tool;
        public int Group => group;

        public void Configure(SparkVirtualTool t, int g, SparkVirtualPCBWorkstation owner,
            SparkPCBRoundedPanelGraphic frame, Text text)
        {
            tool = t;
            group = g;
            workstation = owner;
            panel = frame;
            label = text;
            if (label != null) label.text = SparkPCBToolCatalog.DisplayName(t);
            Refresh(false);
        }

        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(Select);
        }

        private void OnDestroy()
        {
            var button = GetComponent<Button>();
            if (button != null) button.onClick.RemoveListener(Select);
        }

        public void Select()
        {
            if (workstation != null) workstation.SelectTool(tool);
        }

        public void Refresh(bool selected)
        {
            if (panel != null)
                panel.SetStyle(selected ? new Color(.09f, .32f, .43f, .98f) : new Color(.07f,.12f,.21f,.98f),
                    selected ? new Color(.29f,.96f,1f,.95f) : new Color(.20f,.43f,.60f,.55f), 11f, selected ? 2f : 1.1f);
            if (label != null) label.color = selected ? new Color(.88f, 1f, 1f) : new Color(.69f,.85f,.91f);
        }
    }

    public sealed class SparkPCBToolTab : MonoBehaviour
    {
        [SerializeField] private int tab;
        [SerializeField] private SparkVirtualPCBWorkstation workstation;
        [SerializeField] private SparkPCBRoundedPanelGraphic frame;
        [SerializeField] private Text label;

        public void Configure(int tabId, SparkVirtualPCBWorkstation owner, SparkPCBRoundedPanelGraphic panel, Text text)
        {
            tab = tabId;
            workstation = owner;
            frame = panel;
            label = text;
        }

        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(Select);
        }

        private void OnDestroy()
        {
            var button = GetComponent<Button>();
            if (button != null) button.onClick.RemoveListener(Select);
        }

        public void Select() { if (workstation != null) workstation.SelectToolGroup(tab); }

        public void Refresh(bool selected)
        {
            if (frame != null)
                frame.SetStyle(selected ? new Color(.13f,.35f,.43f) : new Color(.06f,.11f,.19f),
                    selected ? new Color(.28f,.91f,.97f) : new Color(.20f,.37f,.49f), 9f, selected ? 2 : 1);
            if (label != null) label.color = selected ? Color.white : new Color(.56f,.72f,.79f);
        }
    }
}
