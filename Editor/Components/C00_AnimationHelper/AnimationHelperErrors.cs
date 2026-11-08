using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    internal static class AnimationHelperErrors
    {
        internal const string Invalid = "pat.animation-helper.error.invalid";
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
            if (key == Invalid)
            {
                return "PAT Animation Helper: 設定に誤りがあります";
            }
            return key == Invalid + ":description" ? "{0}: {1}" : null;
        }
    }
}
