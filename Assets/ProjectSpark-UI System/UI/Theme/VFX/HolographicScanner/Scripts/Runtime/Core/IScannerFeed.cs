namespace ProjectSpark.Scanner
{
    public interface IScannerFeed
    {
        ScannerCapture Capture { get; }
        int Version { get; }

        void BeginCapture();
        void AddComponent(ScannerComponentData component);
        void AddConnection(ScannerConnectionData connection);
        void SetNodeVoltage(ScannerNodeVoltage node);
        void EndCapture();
    }
}
