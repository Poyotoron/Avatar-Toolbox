using System.Collections.Generic;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    internal static class BoneWeightTransferResolver
    {
        private const string DestinationRemovedWarningFormat =
            "ルール {0}: 転移先ボーンが転移元に含まれていたため、転移元から除きました。";
        private const string MissingDestinationLabel =
            "転移先ボーンが指定されていません。";
        private const string NoSourceBonesLabel =
            "転移元ボーンが指定されていません。";
        private const string DestinationNotBoundLabel =
            "転移先ボーンがこのメッシュのボーン一覧にありません。";
        private const string NoSourceBoundLabel =
            "転移元ボーンがこのメッシュのボーン一覧にありません。";

        internal static List<BoneWeightTransferResolvedRule> ResolveRules(
            IList<PATBoneWeightTransferRule> rules,
            List<string> warnings)
        {
            var result = new List<BoneWeightTransferResolvedRule>();
            if (rules == null)
            {
                return result;
            }

            for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
            {
                PATBoneWeightTransferRule input = rules[ruleIndex];
                var resolved = new BoneWeightTransferResolvedRule
                {
                    Number = ruleIndex + 1,
                    Destination = input == null ? null : input.destination,
                    BlendRatio = input == null ? 1f : input.ratio,
                };
                ResolveRuleSourceBones(input, resolved, warnings);
                if (resolved.Destination == null)
                {
                    resolved.BlockReason = BoneWeightTransferRuleBlockReason.MissingDestination;
                    resolved.BlockedLabel = MissingDestinationLabel;
                }
                else if (resolved.SourceBones.Count == 0)
                {
                    resolved.BlockReason = BoneWeightTransferRuleBlockReason.NoSourceBones;
                    resolved.BlockedLabel = NoSourceBonesLabel;
                }

                if (resolved.BlockReason != BoneWeightTransferRuleBlockReason.None)
                {
                    resolved.HeaderLabel = string.Format(
                        "ルール {0}  無効: {1}",
                        resolved.Number,
                        resolved.BlockedLabel);
                }

                result.Add(resolved);
            }

            return result;
        }

        private static void ResolveRuleSourceBones(
            PATBoneWeightTransferRule input,
            BoneWeightTransferResolvedRule resolved,
            List<string> warnings)
        {
            var seen = new HashSet<Transform>();
            bool removedDestination = false;
            if (input != null && input.sourceMode == PATBoneWeightTransferSourceMode.Direct)
            {
                if (input.sources != null)
                {
                    for (int boneIndex = 0;
                         boneIndex < input.sources.Count;
                         boneIndex++)
                    {
                        AddSourceBone(
                            input.sources[boneIndex],
                            resolved.Destination,
                            seen,
                            resolved.SourceBones,
                            ref removedDestination);
                    }
                }
            }
            else if (input != null && input.sourceRoot != null)
            {
                Transform[] descendants =
                    input.sourceRoot.GetComponentsInChildren<Transform>(true);
                for (int boneIndex = 0; boneIndex < descendants.Length; boneIndex++)
                {
                    Transform bone = descendants[boneIndex];
                    if (!input.includeSourceRoot &&
                        bone == input.sourceRoot)
                    {
                        continue;
                    }

                    AddSourceBone(
                        bone,
                        resolved.Destination,
                        seen,
                        resolved.SourceBones,
                        ref removedDestination);
                }
            }

            if (removedDestination && warnings != null)
            {
                warnings.Add(string.Format(
                    DestinationRemovedWarningFormat, resolved.Number));
            }
        }

        private static void AddSourceBone(
            Transform bone,
            Transform destination,
            HashSet<Transform> seen,
            List<Transform> result,
            ref bool removedDestination)
        {
            if (bone == null)
            {
                return;
            }

            if (bone == destination)
            {
                removedDestination = true;
                return;
            }

            if (seen.Add(bone))
            {
                result.Add(bone);
            }
        }

        internal static void BuildTargetRules(
            BoneWeightTransferResult target,
            IList<BoneWeightTransferResolvedRule> rules,
            Transform[] rendererBones)
        {
            if (rules == null)
            {
                return;
            }

            for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
            {
                BoneWeightTransferResolvedRule resolved = rules[ruleIndex];
                var targetRule = new BoneWeightTransferTargetRule
                {
                    Rule = resolved,
                    BlockReason = resolved.BlockReason,
                    BlockedLabel = resolved.BlockedLabel,
                };
                target.Rules.Add(targetRule);
                if (targetRule.BlockReason != BoneWeightTransferRuleBlockReason.None)
                {
                    continue;
                }

                targetRule.DestinationBoneIndex = FindFirstBoneIndex(
                    rendererBones, resolved.Destination);
                if (targetRule.DestinationBoneIndex < 0)
                {
                    SetRuleBlocked(
                        targetRule,
                        BoneWeightTransferRuleBlockReason.DestinationNotBound,
                        DestinationNotBoundLabel);
                    continue;
                }

                BuildSourceBoneInformation(targetRule, resolved.SourceBones, rendererBones);
                if (targetRule.SourceBoneIndices.Count == 0)
                {
                    SetRuleBlocked(
                        targetRule,
                        BoneWeightTransferRuleBlockReason.NoSourceBound,
                        NoSourceBoundLabel);
                }
            }
        }

        private static int FindFirstBoneIndex(Transform[] bones, Transform bone)
        {
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                if (bones[boneIndex] != null && bones[boneIndex] == bone)
                {
                    return boneIndex;
                }
            }

            return -1;
        }

        private static void BuildSourceBoneInformation(
            BoneWeightTransferTargetRule targetRule,
            IList<Transform> sourceBones,
            Transform[] rendererBones)
        {
            for (int sourceIndex = 0; sourceIndex < sourceBones.Count; sourceIndex++)
            {
                Transform sourceBone = sourceBones[sourceIndex];
                var sourceInfo = new BoneWeightTransferSourceBoneInfo { Bone = sourceBone };
                targetRule.SourceBones.Add(sourceInfo);
                for (int rendererBoneIndex = 0;
                     rendererBoneIndex < rendererBones.Length;
                     rendererBoneIndex++)
                {
                    if (rendererBones[rendererBoneIndex] == null ||
                        rendererBones[rendererBoneIndex] != sourceBone)
                    {
                        continue;
                    }

                    sourceInfo.BoneIndices.Add(rendererBoneIndex);
                    targetRule.SourceBoneIndices.Add(rendererBoneIndex);
                    targetRule.SourceInfoByBoneIndex.Add(
                        rendererBoneIndex, sourceIndex);
                }
            }
        }

        internal static void BuildRuleRows(BoneWeightTransferResult target)
        {
            for (int ruleIndex = 0; ruleIndex < target.Rules.Count; ruleIndex++)
            {
                BoneWeightTransferTargetRule targetRule = target.Rules[ruleIndex];
                if (!targetRule.IsApplicable)
                {
                    targetRule.HeaderLabel = string.Format(
                        "ルール {0}  無効: {1}",
                        targetRule.Rule.Number,
                        targetRule.BlockedLabel);
                    continue;
                }

                targetRule.HeaderLabel = targetRule.AffectedVertexCount > 0
                    ? string.Format(
                        "ルール {0}  影響 {1} 頂点 / ウェイト {2:0.00}",
                        targetRule.Rule.Number,
                        targetRule.AffectedVertexCount,
                        targetRule.MovedWeight)
                    : string.Format(
                        "ルール {0}  影響なし", targetRule.Rule.Number);
                for (int sourceIndex = 0;
                     sourceIndex < targetRule.SourceBones.Count;
                     sourceIndex++)
                {
                    BoneWeightTransferSourceBoneInfo source =
                        targetRule.SourceBones[sourceIndex];
                    string boneName = source.Bone == null
                        ? "(Missing)"
                        : source.Bone.name;
                    source.RowLabel = source.VertexCount > 0
                        ? string.Format(
                            "{0}  頂点 {1} / ウェイト {2:0.00}",
                            boneName,
                            source.VertexCount,
                            source.MovedWeight)
                        : boneName + "  該当なし";
                }
            }
        }

        private static void SetRuleBlocked(
            BoneWeightTransferTargetRule targetRule,
            BoneWeightTransferRuleBlockReason reason,
            string label)
        {
            targetRule.BlockReason = reason;
            targetRule.BlockedLabel = label;
        }

    }
}
