using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper
{
    /// <summary>動かす Transform の値やブレンドシェイプを指定します。</summary>
    public enum PATAnimationHelperProperty
    {
        [InspectorName("位置")] Position = 0,
        [InspectorName("回転")] Rotation = 1,
        [InspectorName("大きさ")] Scale = 2,
        [InspectorName("ブレンドシェイプ")] BlendShape = 3,
    }
}
