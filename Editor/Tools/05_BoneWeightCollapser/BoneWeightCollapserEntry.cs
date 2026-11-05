using Poyo.AvatarToolbox.Editor;
using UnityEditor;

namespace Poyo.AvatarToolbox.BoneWeightCollapser.Editor
{
    internal static class BoneWeightCollapserEntry
    {
        internal const string ToolId = "05";

        [InitializeOnLoadMethod]
        private static void Register()
        {
            AvatarToolboxToolRegistry.Register(ToolId, BoneWeightCollapserWindow.Open);
        }
    }
}
