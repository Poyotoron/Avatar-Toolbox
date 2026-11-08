using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    internal static class AnimationHelperValues
    {
        internal static Vector3 ReadVector(Transform target, PATAnimationHelperProperty property)
        {
            if (target == null)
            {
                return Vector3.zero;
            }
            switch (property)
            {
                case PATAnimationHelperProperty.Position:
                    return target.localPosition;
                case PATAnimationHelperProperty.Rotation:
                    return target.localEulerAngles;
                default:
                    return target.localScale;
            }
        }

        internal static bool TryReadWeight(Transform target, string name, out float weight)
        {
            weight = 0f;
            if (target == null)
            {
                return false;
            }
            SkinnedMeshRenderer renderer = target.GetComponent<SkinnedMeshRenderer>();
            if (renderer == null || renderer.sharedMesh == null)
            {
                return false;
            }
            int index = renderer.sharedMesh.GetBlendShapeIndex(name);
            if (index < 0)
            {
                return false;
            }
            weight = renderer.GetBlendShapeWeight(index);
            return true;
        }

        internal static void ApplyDefaults(PATAnimationHelperEntry entry)
        {
            Vector3 current = ReadVector(entry.target, entry.property);
            TryReadWeight(entry.target, entry.blendShapeName, out float weight);
            entry.startVector = entry.property == PATAnimationHelperProperty.Scale ? Vector3.zero : current;
            entry.endVector = current;
            entry.minVector = entry.property == PATAnimationHelperProperty.Scale ? current * 0.5f : current;
            entry.maxVector = entry.property == PATAnimationHelperProperty.Scale ? current * 1.5f : current;
            entry.initialVector = current;
            entry.startWeight = 0f;
            entry.endWeight = 100f;
            entry.minWeight = 0f;
            entry.maxWeight = 100f;
            entry.initialWeight = weight;
        }

        internal static float RepresentativeRatio(PATAnimationHelperEntry first)
        {
            if (first.property == PATAnimationHelperProperty.BlendShape)
            {
                return Mathf.InverseLerp(first.minWeight, first.maxWeight, first.initialWeight);
            }
            Vector3 delta = first.maxVector - first.minVector;
            int axis = 0;
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            {
                axis = 1;
            }
            if (Mathf.Abs(delta.z) > Mathf.Abs(delta[axis]))
            {
                axis = 2;
            }
            return Mathf.InverseLerp(first.minVector[axis], first.maxVector[axis], first.initialVector[axis]);
        }
    }
}
