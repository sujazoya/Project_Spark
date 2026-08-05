namespace ProjectSpark.UI.Data
{
    public sealed class DiagnosticsViewModel
    {
        public string ComponentName
        {
            get;
        }

        public string ComponentType
        {
            get;
        }

        public string Status
        {
            get;
        }

        public string FaultDescription
        {
            get;
        }

        public bool HasFault
        {
            get;
        }

        public DiagnosticsViewModel(
            string componentName,
            string componentType,
            string status,
            string faultDescription,
            bool hasFault)
        {
            ComponentName =
                componentName;

            ComponentType =
                componentType;

            Status =
                status;

            FaultDescription =
                faultDescription;

            HasFault =
                hasFault;
        }
    }
}