using UnityEditor;
using UnityEngine;

namespace Poyo.AvatarToolbox.Editor
{
    internal static class AvatarToolboxLegacy
    {
        // if の条件で使うため、到達できないコードの警告を避ける。
        internal static readonly bool IsInstalled =
#if AVATAR_TOOLBOX_HAS_LEGACY
            true;
#else
            false;
#endif

#if AVATAR_TOOLBOX_HAS_LEGACY
        [InitializeOnLoadMethod]
        private static void Notify()
        {
            const string key = "net.maaaaa.avatar-toolbox.legacyNoticeShown";
            if (Application.isBatchMode || SessionState.GetBool(key, false))
            {
                return;
            }
            SessionState.SetBool(key, true);
            Debug.LogWarning("Poyo's Avatar Toolbox: 旧名の Candy Box（net.maaaaa.candy-box）が同じプロジェクトに入っています。Poyo's Avatar Toolbox は Candy Box の新しい名前です。VCC などのパッケージ管理ツールで Candy Box を削除してください。");
        }
#endif
    }
}
