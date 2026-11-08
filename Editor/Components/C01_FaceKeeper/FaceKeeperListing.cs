using System.Collections.Generic;
using UnityEngine;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal sealed class FaceKeeperListing
    {
        internal List<FaceKeeperMenuNode> MenuRoots = new List<FaceKeeperMenuNode>();
        internal List<FaceKeeperMenuNode> UnmatchedGroups = new List<FaceKeeperMenuNode>();
        internal List<string> TargetPaths = new List<string>();
        internal int ExpressionCount;
        internal string CreatedLabel;
    }

    internal sealed class FaceKeeperMenuNode
    {
        internal string Name;
        internal List<FaceKeeperMenuNode> Children = new List<FaceKeeperMenuNode>();
        internal List<FaceKeeperExpression> Expressions = new List<FaceKeeperExpression>();
        internal bool Expanded = true;
        internal string ParameterName;
        internal float Value;
        internal bool IsLeaf;
    }

    internal sealed class FaceKeeperExpression
    {
        internal string Label;
        internal string ChangeCountLabel;
        internal List<string> ChangeLines;
        internal AnimationClip BeforeClip;
        internal AnimationClip AfterClip;
        internal float[] Times;
        internal string PreviewLabel;
    }
}
