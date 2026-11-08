using System.Collections.Generic;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    internal enum BoneWeightTransferRuleBlockReason
    {
        None,
        MissingDestination,
        NoSourceBones,
        DestinationNotBound,
        NoSourceBound,
    }

    internal sealed class BoneWeightTransferSourceBoneInfo
    {
        internal Transform Bone;
        internal List<int> BoneIndices = new List<int>();
        internal float MovedWeight;
        internal int VertexCount;
        internal string RowLabel;
    }

    internal sealed class BoneWeightTransferResolvedRule
    {
        internal int Number;
        internal Transform Destination;
        internal List<Transform> SourceBones = new List<Transform>();
        internal float BlendRatio;
        internal BoneWeightTransferRuleBlockReason BlockReason;
        internal string BlockedLabel;
        internal string HeaderLabel;
    }

    internal sealed class BoneWeightTransferTargetRule
    {
        internal BoneWeightTransferResolvedRule Rule;
        internal int DestinationBoneIndex = -1;
        internal List<int> SourceBoneIndices = new List<int>();
        internal List<BoneWeightTransferSourceBoneInfo> SourceBones =
            new List<BoneWeightTransferSourceBoneInfo>();
        internal Dictionary<int, int> SourceInfoByBoneIndex =
            new Dictionary<int, int>();
        internal BoneWeightTransferRuleBlockReason BlockReason;
        internal string BlockedLabel;
        internal int AffectedVertexCount;
        internal float MovedWeight;
        internal string HeaderLabel;
        internal bool IsApplicable => BlockReason == BoneWeightTransferRuleBlockReason.None;
    }

    internal struct BoneWeightTransferStats
    {
        internal int AffectedVertexCount;
        internal float MovedWeightTotal;
    }

    internal sealed class BoneWeightTransferResult
    {
        internal SkinnedMeshRenderer Renderer;
        internal Mesh SourceMesh;
        internal Mesh BakedMesh;
        internal string ErrorKey;
        internal List<BoneWeightTransferTargetRule> Rules = new List<BoneWeightTransferTargetRule>();
        internal BoneWeightTransferStats Stats;
        internal int VertexCount;
        internal float[] MovedPerVertex;
        internal List<string> RuleWarnings = new List<string>();
        internal List<string> Lines = new List<string>();
    }
}
