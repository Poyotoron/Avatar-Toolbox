using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

namespace Poyo.AvatarToolbox.FaceKeeper
{
    /// <summary>表情のキーを顔のブレンドシェイプの現在値まで引き上げる設定です。</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Poyo's Avatar Toolbox/PAT Face Keeper")]
    [HelpURL("https://ghp.maaaaa.net/Avatar-Toolbox/face-keeper/")]
    public sealed class PATFaceKeeper : MonoBehaviour, INDMFEditorOnly
    {
        public int dataVersion = 1;
        public bool manualMeshes;
        public List<SkinnedMeshRenderer> meshes = new List<SkinnedMeshRenderer>();
        public List<string> excludedBlendShapes = new List<string>();
    }
}
