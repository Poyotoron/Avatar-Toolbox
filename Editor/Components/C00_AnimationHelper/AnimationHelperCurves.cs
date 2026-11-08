using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    internal enum AnimationHelperValueKind { Start, End, Min, Max, Initial }

    internal static class AnimationHelperCurves
    {
        private static readonly string[] Axes = { ".x", ".y", ".z" };

        internal static void Collect(PATAnimationHelperEntry entry, Transform avatarRoot,
            List<EditorCurveBinding> bindings)
        {
            string path = AnimationUtility.CalculateTransformPath(entry.target, avatarRoot);
            if (entry.property == PATAnimationHelperProperty.BlendShape)
            {
                bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(SkinnedMeshRenderer),
                    "blendShape." + entry.blendShapeName));
                return;
            }
            string property;
            switch (entry.property)
            {
                case PATAnimationHelperProperty.Position:
                    property = "m_LocalPosition";
                    break;
                case PATAnimationHelperProperty.Rotation:
                    // NOTE: 四元数に変えると 180 度を超える回転が逆回りになるため、オイラー角で書く。
                    property = "localEulerAnglesRaw";
                    break;
                default:
                    property = "m_LocalScale";
                    break;
            }
            // NOTE: WD OFF でも他のアニメーションに軸の値を残させないため、変わらない軸も書く。
            foreach (string axis in Axes)
            {
                bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), property + axis));
            }
        }

        internal static void ReadValues(PATAnimationHelperEntry entry, AnimationHelperValueKind kind,
            List<float> values)
        {
            if (entry.property == PATAnimationHelperProperty.BlendShape)
            {
                switch (kind)
                {
                    case AnimationHelperValueKind.Start:
                        values.Add(entry.startWeight);
                        break;
                    case AnimationHelperValueKind.End:
                        values.Add(entry.endWeight);
                        break;
                    case AnimationHelperValueKind.Min:
                        values.Add(entry.minWeight);
                        break;
                    case AnimationHelperValueKind.Max:
                        values.Add(entry.maxWeight);
                        break;
                    default:
                        values.Add(entry.initialWeight);
                        break;
                }
                return;
            }
            Vector3 vector;
            switch (kind)
            {
                case AnimationHelperValueKind.Start:
                    vector = entry.startVector;
                    break;
                case AnimationHelperValueKind.End:
                    vector = entry.endVector;
                    break;
                case AnimationHelperValueKind.Min:
                    vector = entry.minVector;
                    break;
                case AnimationHelperValueKind.Max:
                    vector = entry.maxVector;
                    break;
                default:
                    vector = entry.initialVector;
                    break;
            }
            values.Add(vector.x);
            values.Add(vector.y);
            values.Add(vector.z);
        }

        internal static AnimationClip CreateConstantClip(string name,
            List<EditorCurveBinding> bindings, List<float> values)
        {
            AnimationClip clip = CreateClip(name);
            for (int i = 0; i < bindings.Count; i++)
            {
                AnimationUtility.SetEditorCurve(clip, bindings[i],
                    new AnimationCurve(new Keyframe(0f, values[i])));
            }
            return clip;
        }

        internal static AnimationClip CreateMovingClip(string name, List<EditorCurveBinding> bindings,
            List<float> from, List<float> to, float duration, PATAnimationHelperEasing easing, bool reversed)
        {
            AnimationClip clip = CreateClip(name);
            float time = Mathf.Clamp(duration, 0.01f, 10f);
            for (int i = 0; i < bindings.Count; i++)
            {
                float a = from[i];
                float b = to[i];
                AnimationCurve curve;
                if (easing == PATAnimationHelperEasing.EaseInOut)
                {
                    curve = AnimationCurve.EaseInOut(0f, a, time, b);
                }
                else if (easing == PATAnimationHelperEasing.EaseOut)
                {
                    float delta = (b - a) / time;
                    curve = new AnimationCurve(new Keyframe(0f, a, 0f, reversed ? 0f : 2f * delta),
                        new Keyframe(time, b, reversed ? 2f * delta : 0f, 0f));
                    // NOTE: 自動タンジェントで減速の形に戻されないよう、明示した接線を保つ。
                    for (int key = 0; key < curve.length; key++)
                    {
                        AnimationUtility.SetKeyBroken(curve, key, true);
                        AnimationUtility.SetKeyLeftTangentMode(curve, key, AnimationUtility.TangentMode.Free);
                        AnimationUtility.SetKeyRightTangentMode(curve, key, AnimationUtility.TangentMode.Free);
                    }
                }
                else
                {
                    curve = AnimationCurve.Linear(0f, a, time, b);
                }
                AnimationUtility.SetEditorCurve(clip, bindings[i], curve);
            }
            return clip;
        }

        private static AnimationClip CreateClip(string name)
        {
            // NOTE: 保存はビルドの最後に NDMF が行うため、メモリ上にだけ作る。
            var clip = new AnimationClip { name = name };
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }
    }
}
