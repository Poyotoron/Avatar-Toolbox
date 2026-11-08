using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    internal static class AnimationHelperRadialBuilder
    {
        internal static BlendTree Build(PATAnimationHelper component, Transform avatarRoot,
            string parameterName, out float defaultValue)
        {
            defaultValue = AnimationHelperValues.RepresentativeRatio(component.entries[0]);
            var bindings = new List<EditorCurveBinding>();
            var min = new List<float>();
            var max = new List<float>();
            var initial = new List<float>();
            foreach (PATAnimationHelperEntry entry in component.entries)
            {
                AnimationHelperCurves.Collect(entry, avatarRoot, bindings);
                AnimationHelperCurves.ReadValues(entry, AnimationHelperValueKind.Min, min);
                AnimationHelperCurves.ReadValues(entry, AnimationHelperValueKind.Max, max);
                AnimationHelperCurves.ReadValues(entry, AnimationHelperValueKind.Initial, initial);
            }
            var tree = new BlendTree
            {
                name = "PAT Animation Helper", blendType = BlendTreeType.Simple1D,
                blendParameter = parameterName, useAutomaticThresholds = false
            };
            // NOTE: Merge Motion は定数以外の曲線に警告を出すため、各点はキーを 1 つだけ持つ。
            tree.AddChild(AnimationHelperCurves.CreateConstantClip("Min", bindings, min), 0f);
            if (defaultValue > 0.0001f && defaultValue < 0.9999f)
            {
                tree.AddChild(AnimationHelperCurves.CreateConstantClip("Initial", bindings, initial), defaultValue);
            }
            tree.AddChild(AnimationHelperCurves.CreateConstantClip("Max", bindings, max), 1f);
            return tree;
        }
    }
}
