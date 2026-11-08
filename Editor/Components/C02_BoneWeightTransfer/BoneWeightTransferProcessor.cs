using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    internal static class BoneWeightTransferProcessor
    {
        internal static string BasicError(PATBoneWeightTransfer component)
        {
            SkinnedMeshRenderer renderer = component.GetComponent<SkinnedMeshRenderer>();
            if (renderer == null || renderer.sharedMesh == null)
            {
                return BoneWeightTransferErrors.NoMesh;
            }
            try
            {
                if (renderer.bones.Length == 0 || renderer.sharedMesh.GetAllBoneWeights().Length == 0)
                {
                    return BoneWeightTransferErrors.NoWeights;
                }
            }
            catch (Exception)
            {
                return BoneWeightTransferErrors.Unreadable;
            }
            return null;
        }

        internal static BoneWeightTransferResult Process(PATBoneWeightTransfer component, bool buildMesh,
            bool collectVertices = false)
        {
            var result = new BoneWeightTransferResult();
            result.Renderer = component.GetComponent<SkinnedMeshRenderer>();
            result.ErrorKey = BasicError(component);
            if (result.ErrorKey != null)
            {
                return result;
            }
            result.SourceMesh = result.Renderer.sharedMesh;
            result.VertexCount = result.SourceMesh.vertexCount;
            List<BoneWeightTransferResolvedRule> resolved = BoneWeightTransferResolver.ResolveRules(
                component.rules, result.RuleWarnings);
            BoneWeightTransferResolver.BuildTargetRules(result, resolved, result.Renderer.bones);
            if (collectVertices)
            {
                result.MovedPerVertex = new float[result.VertexCount];
            }
            var applicable = new List<BoneWeightTransferTargetRule>();
            foreach (BoneWeightTransferTargetRule rule in result.Rules)
            {
                if (rule.IsApplicable)
                {
                    applicable.Add(rule);
                }
                else
                {
                    result.RuleWarnings.Add(string.Format("規則 {0} は使われません: {1}",
                        rule.Rule.Number, rule.BlockedLabel));
                }
            }
            if (applicable.Count == 0)
            {
                result.ErrorKey = BoneWeightTransferErrors.NoRule;
            }
            else if (!BoneWeightTransferBaker.Collapse(result.SourceMesh, applicable,
                component.normalize, buildMesh, out result.BakedMesh, out result.Stats, result.MovedPerVertex))
            {
                result.ErrorKey = BoneWeightTransferErrors.Unreadable;
                return result;
            }
            if (result.BakedMesh != null)
            {
                // NOTE: sharedMesh は元アセットを指すため、複製した結果だけを差し替える。
                result.BakedMesh.name = result.SourceMesh.name + " (Transferred)";
            }
            BoneWeightTransferResolver.BuildRuleRows(result);
            result.Lines.Add(string.Format("影響を受ける頂点: {0} / {1}  移すウェイトの総量: {2:0.000}",
                result.Stats.AffectedVertexCount, result.VertexCount, result.Stats.MovedWeightTotal));
            foreach (BoneWeightTransferTargetRule rule in result.Rules)
            {
                result.Lines.Add(rule.HeaderLabel);
                foreach (BoneWeightTransferSourceBoneInfo bone in rule.SourceBones)
                {
                    if (bone.RowLabel != null)
                    {
                        result.Lines.Add(bone.RowLabel);
                    }
                }
            }
            if (result.ErrorKey == null && result.Stats.AffectedVertexCount == 0)
            {
                result.Lines.Add(BoneWeightTransferErrors.NoVertexText);
            }
            return result;
        }
    }
}
