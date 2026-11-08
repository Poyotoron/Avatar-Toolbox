using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper
{
    /// <summary>トグルの動く速さの変化を保存します。</summary>
    public enum PATAnimationHelperEasing
    {
        [InspectorName("一定の速さ")] Linear = 0,
        [InspectorName("ゆっくり止まる")] EaseOut = 1,
        [InspectorName("ゆっくり始まって、ゆっくり止まる")] EaseInOut = 2,
    }
}
