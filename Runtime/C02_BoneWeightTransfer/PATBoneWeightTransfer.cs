using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer
{
    /// <summary>ビルド時にメッシュのボーンウェイトを転移する設定です。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    [AddComponentMenu("Poyo's Avatar Toolbox/PAT Bone Weight Transfer")]
    [HelpURL("https://ghp.maaaaa.net/Avatar-Toolbox/bone-weight-transfer/")]
    public sealed class PATBoneWeightTransfer : MonoBehaviour, INDMFEditorOnly
    {
        public int dataVersion = 1;
        public List<PATBoneWeightTransferRule> rules = new List<PATBoneWeightTransferRule> { new PATBoneWeightTransferRule() };
        public bool normalize = true;
    }
}
