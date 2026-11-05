using Poyo.AvatarToolbox.Editor;
using UnityEditor;

namespace Poyo.AvatarToolbox.FaceEmoSplitter.Editor
{
    internal static class FaceEmoSplitterEntry
    {
        internal const string ToolId = "08";

        [InitializeOnLoadMethod]
        private static void Register()
        {
            AvatarToolboxToolRegistry.Register(ToolId, FaceEmoSplitterWindow.Open);
        }
    }
}
