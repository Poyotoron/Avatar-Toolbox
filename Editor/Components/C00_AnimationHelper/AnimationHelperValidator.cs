using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    internal sealed class AnimationHelperIssue
    {
        internal bool IsError;
        internal string Message;
    }

    internal static class AnimationHelperValidator
    {
        private const float Tolerance = 0.0001f;

        internal static void Validate(PATAnimationHelper component, Transform avatarRoot,
            List<AnimationHelperIssue> issues)
        {
            issues.Clear();
            if (avatarRoot == null)
            {
                Add(issues, false, "アバターの中に置いてください。");
            }
            var humanBones = new HashSet<Transform>();
            Animator animator = avatarRoot != null ? avatarRoot.GetComponent<Animator>() : null;
            if (animator != null && animator.isHuman)
            {
                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    Transform bone = animator.GetBoneTransform((HumanBodyBones)i);
                    if (bone != null)
                    {
                        humanBones.Add(bone);
                    }
                }
            }
            if (component.menuParent != null)
            {
                ModularAvatarMenuItem item = component.menuParent.GetComponent<ModularAvatarMenuItem>();
                if (item == null || item.Control.type != VRCExpressionsMenu.Control.ControlType.SubMenu ||
                    item.MenuSource != SubmenuSource.Children)
                {
                    Add(issues, true, "メニューの挿入先には、サブメニューの MA Menu Item（子のメニューを使う設定）を指定してください。");
                }
                if (avatarRoot != null && !component.menuParent.transform.IsChildOf(avatarRoot))
                {
                    Add(issues, true, "メニューの挿入先がアバターの外にあります。");
                }
            }
            if (component.entries == null || component.entries.Count == 0)
            {
                Add(issues, true, "動かすものがありません。+ で行を足してください。");
                return;
            }
            bool radial = component.mode == PATAnimationHelperMode.Radial;
            var differing = new List<string>();
            for (int i = 0; i < component.entries.Count; i++)
            {
                PATAnimationHelperEntry entry = component.entries[i];
                if (entry == null || entry.target == null)
                {
                    Add(issues, true, "対象が入っていない行があります。");
                    continue;
                }
                if (avatarRoot != null && !entry.target.IsChildOf(avatarRoot))
                {
                    Add(issues, true, "対象がアバターの外にあります。");
                }
                bool shape = entry.property == PATAnimationHelperProperty.BlendShape;
                if (shape)
                {
                    SkinnedMeshRenderer renderer = entry.target.GetComponent<SkinnedMeshRenderer>();
                    if (renderer == null || renderer.sharedMesh == null)
                    {
                        Add(issues, true, "対象にメッシュがありません。");
                        continue;
                    }
                    if (renderer.sharedMesh.GetBlendShapeIndex(entry.blendShapeName) < 0)
                    {
                        Add(issues, true, string.Format("ブレンドシェイプ「{0}」が見つかりません。", entry.blendShapeName));
                        continue;
                    }
                }
                else if (entry.property != PATAnimationHelperProperty.Scale && humanBones.Contains(entry.target))
                {
                    Add(issues, true, "Humanoid のボーンは、位置と回転を動かせません。大きさは動かせます。");
                }
                if (radial)
                {
                    bool same = shape ? entry.minWeight == entry.maxWeight : entry.minVector.Equals(entry.maxVector);
                    if (i == 0 && same)
                    {
                        Add(issues, true, "先頭の行の下限と上限が同じです。");
                    }
                    bool between = shape
                        ? Between(entry.initialWeight, entry.minWeight, entry.maxWeight)
                        : Between(entry.initialVector.x, entry.minVector.x, entry.maxVector.x) &&
                          Between(entry.initialVector.y, entry.minVector.y, entry.maxVector.y) &&
                          Between(entry.initialVector.z, entry.minVector.z, entry.maxVector.z);
                    if (!between)
                    {
                        Add(issues, true, "初期値は下限と上限の間にしてください。");
                    }
                }
                bool differs;
                if (shape)
                {
                    AnimationHelperValues.TryReadWeight(entry.target, entry.blendShapeName, out float current);
                    float initial = radial ? entry.initialWeight
                        : component.toggleDefaultOn ? entry.endWeight : entry.startWeight;
                    differs = Mathf.Abs(initial - current) > Tolerance;
                }
                else
                {
                    Vector3 current = AnimationHelperValues.ReadVector(entry.target, entry.property);
                    Vector3 initial = radial ? entry.initialVector
                        : component.toggleDefaultOn ? entry.endVector : entry.startVector;
                    differs = Mathf.Abs(initial.x - current.x) > Tolerance ||
                        Mathf.Abs(initial.y - current.y) > Tolerance ||
                        Mathf.Abs(initial.z - current.z) > Tolerance;
                }
                if (differs)
                {
                    differing.Add((i + 1).ToString());
                }
                for (int j = 0; j < i; j++)
                {
                    PATAnimationHelperEntry previous = component.entries[j];
                    if (previous != null && previous.target == entry.target && previous.property == entry.property &&
                        (!shape || previous.blendShapeName == entry.blendShapeName))
                    {
                        Add(issues, false, string.Format("行 {0} と行 {1} が同じものを動かしています。後の行の値が使われます。", j + 1, i + 1));
                    }
                }
            }
            if (differing.Count > 0)
            {
                Add(issues, false, string.Format(
                    "行 {0} の初期状態の値が、今の見た目と違います。アップロードした直後の見た目がエディタと変わります。",
                    string.Join(", ", differing)));
            }
        }

        private static bool Between(float value, float a, float b)
        {
            return value >= Mathf.Min(a, b) && value <= Mathf.Max(a, b);
        }

        private static void Add(List<AnimationHelperIssue> issues, bool error, string message)
        {
            issues.Add(new AnimationHelperIssue { IsError = error, Message = message });
        }
    }
}
