using System;
using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper
{
    /// <summary>対象と、トグルおよび Radial で動かす値を保存します。</summary>
    [Serializable]
    public sealed class PATAnimationHelperEntry
    {
        public Transform target;
        public PATAnimationHelperProperty property = PATAnimationHelperProperty.Scale;
        public string blendShapeName = "";
        public Vector3 startVector = Vector3.zero;
        public Vector3 endVector = Vector3.zero;
        public float startWeight;
        public float endWeight;
        public Vector3 minVector = Vector3.zero;
        public Vector3 maxVector = Vector3.zero;
        public Vector3 initialVector = Vector3.zero;
        public float minWeight;
        public float maxWeight;
        public float initialWeight;
    }
}
