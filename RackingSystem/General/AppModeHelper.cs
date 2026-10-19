namespace RackingSystem.General
{
    public static class AppModeHelper
    {
        // AppMode in appsettings.json: "Testing" or "Live" (anything else is treated as Live)
        public static bool IsTesting(IConfiguration config)
        {
            return string.Equals(config["AppMode"], "Testing", StringComparison.OrdinalIgnoreCase);
        }

        // Prefix for page title / caption, e.g. "[TEST] WMS"
        public static string TitlePrefix(IConfiguration config)
        {
            return IsTesting(config) ? "[TEST] " : "";
        }
    }
}
