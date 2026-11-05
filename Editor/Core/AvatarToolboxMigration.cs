using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Poyo.AvatarToolbox.Editor
{
    [InitializeOnLoad]
    internal static class AvatarToolboxMigration
    {
        private const string LegacyDefineSymbolPrefix = "CANDY_BOX_TOOL_";

        static AvatarToolboxMigration()
        {
            // NOTE: インポート中の再コンパイルや再インポートの重なりを避ける。
            EditorApplication.delayCall += Run;
        }

        private static void Run()
        {
            // NOTE: 旧版がまだ旧い名前のシンボルとフォルダを使っている。
            if (AvatarToolboxLegacy.IsInstalled)
            {
                return;
            }
            MigrateDefineSymbols();
            MigrateFolder();
        }

        private static void MigrateDefineSymbols()
        {
            bool changedAny = false;
            int standaloneCount = 0;
            foreach (NamedBuildTarget target in AvatarToolboxDefines.Targets)
            {
                PlayerSettings.GetScriptingDefineSymbols(target, out string[] defines);
                var updated = new List<string>(defines);
                int count = 0;
                for (int i = 0; i < updated.Count; i++)
                {
                    string symbol = updated[i];
                    if (!symbol.StartsWith(LegacyDefineSymbolPrefix, StringComparison.Ordinal))
                    {
                        continue;
                    }
                    string replacement = AvatarToolboxInfo.DefineSymbolPrefix +
                        symbol.Substring(LegacyDefineSymbolPrefix.Length);
                    if (updated.Contains(replacement))
                    {
                        updated.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        updated[i] = replacement;
                    }
                    count++;
                }
                if (count == 0)
                {
                    continue;
                }
                PlayerSettings.SetScriptingDefineSymbols(target, updated.ToArray());
                changedAny = true;
                if (target == NamedBuildTarget.Standalone)
                {
                    standaloneCount = count;
                }
            }
            if (changedAny)
            {
                Debug.Log($"Poyo's Avatar Toolbox: 以前の名前（Candy Box）で有効にしていたツールを引き継ぎました（{standaloneCount} 件）。");
            }
        }

        private static void MigrateFolder()
        {
            const string legacyFolder = "Assets/zzz_pytr/CandyBox";
            const string newFolder = "Assets/zzz_pytr/Avatar-Toolbox";
            if (!AssetDatabase.IsValidFolder(legacyFolder))
            {
                return;
            }
            if (AssetDatabase.IsValidFolder(newFolder))
            {
                const string key = "net.maaaaa.avatar-toolbox.legacyFolderWarned";
                if (!SessionState.GetBool(key, false))
                {
                    SessionState.SetBool(key, true);
                    Debug.LogWarning("Poyo's Avatar Toolbox: 以前の名前のフォルダ Assets/zzz_pytr/CandyBox が残っています。中身を Assets/zzz_pytr/Avatar-Toolbox に移してから削除してください。");
                }
                return;
            }
            // NOTE: フォルダごと移すと、中のアセットも GUID を保てる。
            string error = AssetDatabase.MoveAsset(legacyFolder, newFolder);
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log("Poyo's Avatar Toolbox: Assets/zzz_pytr/CandyBox を Assets/zzz_pytr/Avatar-Toolbox に移しました。");
            }
            else
            {
                Debug.LogWarning($"Poyo's Avatar Toolbox: Assets/zzz_pytr/CandyBox を移せませんでした: {error}");
            }
        }
    }
}
