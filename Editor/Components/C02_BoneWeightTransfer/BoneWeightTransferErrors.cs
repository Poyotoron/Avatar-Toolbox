using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    internal static class BoneWeightTransferErrors
    {
        internal const string NoMesh = "pat.bone-weight-transfer.error.no-mesh";
        internal const string NoWeights = "pat.bone-weight-transfer.error.no-weights";
        internal const string NoRule = "pat.bone-weight-transfer.error.no-rule";
        internal const string Unreadable = "pat.bone-weight-transfer.error.unreadable";
        internal const string RuleSkipped = "pat.bone-weight-transfer.warn.rule-skipped";
        internal const string NoVertex = "pat.bone-weight-transfer.warn.no-vertex";
        internal const string NoMeshText = "メッシュがありません。";
        internal const string NoWeightsText = "このメッシュはボーンウェイトを持っていません。";
        internal const string NoRuleText = "適用できる規則がありません。転移元と転移先を確かめてください。";
        internal const string UnreadableText = "メッシュのウェイトを読み取れませんでした。";
        internal const string NoVertexText = "ウェイトが移る頂点がありませんでした。";
        private static readonly Dictionary<string, string> Messages = new Dictionary<string, string>
        {
            { NoMesh, "PAT Bone Weight Transfer: メッシュがありません" },
            { NoMesh + ":description", "{0}: " + NoMeshText },
            { NoWeights, "PAT Bone Weight Transfer: ボーンウェイトがありません" },
            { NoWeights + ":description", "{0}: " + NoWeightsText },
            { NoRule, "PAT Bone Weight Transfer: 適用できる規則がありません" },
            { NoRule + ":description", "{0}: " + NoRuleText },
            { Unreadable, "PAT Bone Weight Transfer: メッシュのウェイトを読み取れません" },
            { Unreadable + ":description", "{0}: " + UnreadableText },
            { RuleSkipped, "PAT Bone Weight Transfer: 使われない規則があります" },
            { RuleSkipped + ":description", "{0}: 規則 {1} は使われません: {2}" },
            { NoVertex, "PAT Bone Weight Transfer: ウェイトが移る頂点がありません" },
            { NoVertex + ":description", "{0}: " + NoVertexText },
        };
        private static readonly Localizer Localizer = new Localizer(
            "ja-JP", () => new List<(string, Func<string, string>)> { ("ja-JP", Lookup) });

        internal static void Report(ErrorSeverity severity, string key, params object[] args)
        {
            ErrorReport.ReportError(Localizer, severity, key, args);
        }

        internal static string InspectorText(string key)
        {
            if (key == NoMesh)
            {
                return NoMeshText;
            }
            if (key == NoWeights)
            {
                return NoWeightsText;
            }
            if (key == Unreadable)
            {
                return UnreadableText;
            }
            return key == NoRule ? NoRuleText : string.Empty;
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
