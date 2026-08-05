namespace ProjectSpark.UI.Data
{
    public sealed class WorkshopViewModel
    {
        public string ActiveToolName
        {
            get;
        }

        public string SelectedComponentName
        {
            get;
        }

        public bool IsToolActive
        {
            get;
        }

        public string StatusText
        {
            get;
        }

        public WorkshopViewModel(
            string activeToolName,
            string selectedComponentName,
            bool isToolActive,
            string statusText)
        {
            ActiveToolName =
                activeToolName;

            SelectedComponentName =
                selectedComponentName;

            IsToolActive =
                isToolActive;

            StatusText =
                statusText;
        }
    }
}