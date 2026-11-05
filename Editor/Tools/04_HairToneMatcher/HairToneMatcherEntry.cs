using Poyo.AvatarToolbox.Editor;
using UnityEditor;

namespace Poyo.AvatarToolbox.HairToneMatcher.Editor
{
    internal static class HairToneMatcherEntry
    {
        internal const string ToolId = "04";

        [InitializeOnLoadMethod]
        private static void Register()
        {
            AvatarToolboxToolRegistry.Register(ToolId, HairToneMatcherWindow.Open);
        }
    }
}
