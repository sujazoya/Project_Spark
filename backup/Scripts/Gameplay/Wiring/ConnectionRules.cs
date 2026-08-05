// Assets/My_Assets/_Project_Spark/Scripts/Gameplay/Wiring/ConnectionRules.cs

namespace ProjectSpark.Gameplay.Wiring
{
    public static class ConnectionRules
    {
        public static bool CanConnect(
            WireConnector start,
            WireConnector end)
        {
            if (start == null || end == null)
                return false;

            if (start == end)
                return false;

            if (!start.CanConnect)
                return false;

            if (!end.CanConnect)
                return false;

            if (start.Type == end.Type)
                return false;

            return true;
        }
    }
}