namespace EvocationPlus.Core
{
    internal static class Log
    {
        public static void Info(string msg)
        {
            Main.Mod.Logger.Log(msg);
        }

        public static void Error(string msg)
        {
            Main.Mod.Logger.Error(msg);
        }
    }
}