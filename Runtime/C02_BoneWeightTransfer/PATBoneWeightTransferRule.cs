using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer
{
    /// <summary>転移元から転移先へ移すウェイトの割合を保存します。</summary>
    [Serializable]
    public sealed class PATBoneWeightTransferRule
    {
        public PATBoneWeightTransferSourceMode sourceMode = PATBoneWeightTransferSourceMode.Direct;
        public List<Transform> sources = new List<Transform>();
        public Transform sourceRoot;
        public bool includeSourceRoot;
        public Transform destination;
        [Range(0f, 1f)] public float ratio = 1f;
    }
}
