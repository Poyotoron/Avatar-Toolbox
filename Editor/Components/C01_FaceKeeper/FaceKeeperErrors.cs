using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal static class FaceKeeperErrors
    {
        internal const string Multiple = "pat.face-keeper.error.multiple";
        internal const string Meshes = "pat.face-keeper.error.meshes";
        internal const string NoKeys = "pat.face-keeper.warn.no-keys";
        internal const string MultipleText = "PAT Face Keeper がアバターに 2 つ以上あります。1 つにしてください。";
        private static readonly Dictionary<string, string> Messages = new Dictionary<string, string>
        {
            { Multiple, "PAT Face Keeper: 2 つ以上あります" },
            { Multiple + ":description", MultipleText },
            { Meshes, "PAT Face Keeper: 顔のメッシュを決められません" },
            { Meshes + ":description", "{0}" },
            { NoKeys, "PAT Face Keeper: 引き上げるキーがありません" },
            { NoKeys + ":description", "引き上げるキーがありませんでした。" }
        };
        private static readonly Localizer Localizer = new Localizer(
            "ja-JP", () => new List<(string, Func<string, string>)> { ("ja-JP", Lookup) });

        internal static void Report(ErrorSeverity severity, string key, params object[] args)
        {
            ErrorReport.ReportError(Localizer, severity, key, args);
        }

        private static string Lookup(string key)
        {
            if (key.EndsWith(":hint", StringComparison.Ordinal))
            {
                return string.Empty;
            }
            return Messages.TryGetValue(key, out string message) ? message : null;
        }
    }
}
