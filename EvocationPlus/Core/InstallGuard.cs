using System;

namespace EvocationPlus.Core
{
    internal static class InstallGuard
    {
        private static bool _installed;

        public static void RunOnce(Action action)
        {
            if (_installed) return;
            _installed = true;
            action();
        }
    }
}