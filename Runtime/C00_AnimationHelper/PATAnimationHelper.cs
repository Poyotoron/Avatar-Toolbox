using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

namespace Poyo.AvatarToolbox.AnimationHelper
{
    /// <summary>ビルド時にアニメーションとメニューを作る設定です。</summary>
    [AddComponentMenu("Poyo's Avatar Toolbox/PAT Animation Helper")]
    [HelpURL("https://ghp.maaaaa.net/Avatar-Toolbox/animation-helper/")]
    public sealed class PATAnimationHelper : MonoBehaviour, INDMFEditorOnly
    {
        public int dataVersion = 1;
        public PATAnimationHelperMode mode = PATAnimationHelperMode.Toggle;
        public string menuName = "";
        public Texture2D menuIcon;
        public GameObject menuParent;
        public string parameterName = "";
        public bool parameterSaved = true;
        public bool parameterSynced = true;
        public bool toggleDefaultOn;
        public float toggleDuration = 0.5f;
        public PATAnimationHelperEasing toggleEasing = PATAnimationHelperEasing.Linear;
        public bool toggleReverse;
        public List<PATAnimationHelperEntry> entries = new List<PATAnimationHelperEntry>();
    }
}
