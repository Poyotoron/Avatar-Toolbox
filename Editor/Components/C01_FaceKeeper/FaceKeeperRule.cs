using UnityEngine;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal static class FaceKeeperRule
    {
        internal const float Epsilon = 0.0001f;

        internal static int Raise(AnimationCurve curve, float currentValue)
        {
            int count = 0;
            for (int i = 0; i < curve.length; i++)
            {
                Keyframe key = curve[i];
                if (key.value < currentValue - Epsilon)
                {
                    key.value = currentValue;
                    curve.MoveKey(i, key);
                    count++;
                }
            }
            return count;
        }
    }
}
