using Poyo.AvatarToolbox.Editor;
using UnityEditor;

namespace Poyo.AvatarToolbox.BlendShapeBackuper.Editor
{
    internal static class BlendShapeBackuperEntry
    {
        internal const string ToolId = "06";

        [InitializeOnLoadMethod]
        private static void Register()
        {
            AvatarToolboxToolRegistry.Register(ToolId, BlendShapeBackuperWindow.Open);
        }
    }
}
