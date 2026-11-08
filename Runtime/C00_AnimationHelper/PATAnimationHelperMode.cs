using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper
{
    /// <summary>メニューの操作方法を保存します。</summary>
    public enum PATAnimationHelperMode
    {
        [InspectorName("トグル")] Toggle = 0,
        [InspectorName("Radial Puppet")] Radial = 1,
    }
}
