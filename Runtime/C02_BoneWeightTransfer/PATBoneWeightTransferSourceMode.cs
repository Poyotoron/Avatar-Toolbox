using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer
{
    /// <summary>転移元を直接指定するか、起点の子孫から選ぶかを保存します。</summary>
    public enum PATBoneWeightTransferSourceMode
    {
        [InspectorName("直接指定")] Direct = 0,
        [InspectorName("子孫")] Descendants = 1,
    }
}
