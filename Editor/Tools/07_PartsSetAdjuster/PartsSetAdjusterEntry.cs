using Poyo.AvatarToolbox.Editor;
using UnityEditor;

namespace Poyo.AvatarToolbox.PartsSetAdjuster.Editor
{
    internal static class PartsSetAdjusterEntry
    {
        internal const string ToolId = "07";
        [InitializeOnLoadMethod]
        private static void Register()
        {
            AvatarToolboxToolRegistry.Register(ToolId, PartsSetAdjusterWindow.Open);
        }
    }
}
