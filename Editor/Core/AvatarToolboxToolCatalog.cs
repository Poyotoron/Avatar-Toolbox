namespace Poyo.AvatarToolbox.Editor
{
    internal sealed class AvatarToolboxToolEntry
    {
        internal readonly string Id;
        internal readonly string DisplayName;
        internal readonly string Summary;
        internal readonly string Description;
        internal readonly string RequirementLabel;
        internal readonly bool IsAvailable;

        private readonly string _defineSymbol;

        internal AvatarToolboxToolEntry(string id, string displayName, string summary, string description)
        {
            Id = id;
            DisplayName = displayName;
            Summary = summary;
            Description = description;
            RequirementLabel = null;
            IsAvailable = true;
            _defineSymbol = AvatarToolboxInfo.DefineSymbolPrefix + id;
        }

        internal AvatarToolboxToolEntry(
            string id,
            string displayName,
            string summary,
            string description,
            string requirementLabel,
            bool isAvailable)
        {
            Id = id;
            DisplayName = displayName;
            Summary = summary;
            Description = description;
            RequirementLabel = requirementLabel;
            IsAvailable = isAvailable;
            _defineSymbol = AvatarToolboxInfo.DefineSymbolPrefix + id;
        }

        internal string DefineSymbol
        {
            get { return _defineSymbol; }
        }
    }

    internal static class AvatarToolboxToolCatalog
    {
        // NOTE: const bool にすると片方の分岐が到達不能になるため、読み取り専用値にする。
#if AVATAR_TOOLBOX_HAS_MA
        private static readonly bool ModularAvatarAvailable = true;
#else
        private static readonly bool ModularAvatarAvailable = false;
#endif

#if AVATAR_TOOLBOX_HAS_AAO
        private static readonly bool AvatarOptimizerAvailable = true;
#else
        private static readonly bool AvatarOptimizerAvailable = false;
#endif

#if AVATAR_TOOLBOX_HAS_VRCSDK
        private static readonly bool VrchatSdkAvailable = true;
#else
        private static readonly bool VrchatSdkAvailable = false;
#endif
        private const string VrchatSdkRequirement =
            "VRChat SDK - Base 3.5.0 以降";
#if AVATAR_TOOLBOX_HAS_FACEEMO
        private static readonly bool FaceEmoAvailable = true;
#else
        private static readonly bool FaceEmoAvailable = false;
#endif
        private const string FaceEmoRequirement = "FaceEmo 1.7.0 以降（2.x は未対応）";
        private static readonly string FaceEmoSplitterDescription =
            @"FaceEmo の「アバターに適用」は、Prefab Variant の親まで表情を書き込みます。
Variant の系統を木で表示し、ラベルを付けたまとまりごとに表情を分けて適用します。
子の Variant で作った表情が、親や兄弟の Variant に入るのを防ぎます。";
        private static readonly string PartsSetAdjusterDescription =
            @"別のアバター向けに作られた髪・帽子・しっぽなどを、体に合わせて取り付けるのを手伝います。
アバターの軸に沿ったスライダーで位置・角度・大きさを調整でき、髪は顔の形を見て自動で合わせられます。
肌を単色で表示して貫通を見やすくし、パーツの PhysBone Collider を本体のものへ移すこともできます。";

        private const string ModularAvatarRequirement =
            "Modular Avatar 1.17.0 以降";

        private const string AvatarOptimizerRequirement =
            "AAO: Avatar Optimizer 1.9.0 以降";

        private static readonly string BlendshapeKeeperDescription =
            @"メッシュに設定したブレンドシェイプの現在値と、アニメーションのキー値を比べ、
キー値のほうが小さい場合にキー値を現在値まで引き上げます。
表情を再生した瞬間に顔の改変が打ち消されるのを防ぐためのツールです。";

        private static readonly string MaBlendshapeSyncHelperDescription =
            @"衣装のブレンドシェイプを素体のブレンドシェイプへ追従させる設定を、まとめて作ります。
素体と同じ名前のブレンドシェイプを衣装側から探し、チェックを付けたものだけを
MA Blendshape Sync の設定として書き込みます。名前が違うものは手動で対応を指定できます。";

        private static readonly string AaoMergePhysBoneHelperDescription =
            @"複数の PhysBone を 1 つに統合するとき、値が違うプロパティを調べます。
統合対象それぞれの値を一覧にし、最小・最大・平均・中央・最頻から選んだ値を
AAO Merge PhysBone の override として書き込みます。カーブにも対応しています。";

        private static readonly string AaoMergeBoneHelperDescription =
            @"指定したオブジェクト配下のボーンをツリーで一覧し、
どのボーンを親へ統合するかをチェックボックスでまとめて切り替えます。
ボーンチェーンを一定間隔で間引く設定も自動で作れます。";

        private static readonly string HairToneMatcherDescription =
            @"髪を差し替えたときに生じる色味の差を埋めます。
元の髪と新しい髪のテクスチャの色を比べ、新しい髪を元の髪に寄せる補正値を提案します。
lilToon と Poiyomi のマテリアルに対応します。導入は必須ではありません。";

        private static readonly string BoneWeightCollapserDescription =
            @"指定したボーンのウェイトを、別のボーンへまとめて移します。
袖が指ボーンに引きずられて変形する、といった追従の問題を解消するためのツールです。
結果は新しいメッシュとして保存し、元のメッシュには書き込みません。";

        private static readonly string BlendShapeBackuperDescription =
            @"メッシュのブレンドシェイプの値を、キー名と対応付けて保存します。
アバターのアップデートでキーの並びが変わっても、保存した値を名前で照らし合わせて戻せます。
改変中の状態を残しておくこともできます。";

        internal static readonly AvatarToolboxToolEntry[] Tools =
        {
            new AvatarToolboxToolEntry(
                "00",
                "00_Blendshape Keeper",
                "表情アニメで改変したブレンドシェイプが戻るのを防ぎます",
                BlendshapeKeeperDescription),
            new AvatarToolboxToolEntry(
                "01",
                "01_Helper for MA Blendshape Sync",
                "衣装のブレンドシェイプを素体へ追従させる設定をまとめて作ります",
                MaBlendshapeSyncHelperDescription,
                ModularAvatarRequirement,
                ModularAvatarAvailable),
            new AvatarToolboxToolEntry(
                "02",
                "02_Helper for AAO Merge PhysBone",
                "統合する PhysBone の値を比べ、override する値を提案します",
                AaoMergePhysBoneHelperDescription,
                AvatarOptimizerRequirement,
                AvatarOptimizerAvailable),
            new AvatarToolboxToolEntry(
                "03",
                "03_Helper for AAO Merge Bone",
                "ボーンを親へ統合する設定を、ツリーからまとめて切り替えます",
                AaoMergeBoneHelperDescription,
                AvatarOptimizerRequirement,
                AvatarOptimizerAvailable),
            new AvatarToolboxToolEntry(
                "04",
                "04_Hair Tone Matcher",
                "差し替えた髪の色味を、元の髪に合わせます",
                HairToneMatcherDescription),
            new AvatarToolboxToolEntry(
                "05",
                "05_Bone Weight Collapser",
                "不要なボーンのウェイトを、別のボーンへ移します",
                BoneWeightCollapserDescription),
            new AvatarToolboxToolEntry(
                "06",
                "06_BlendShape Backuper",
                "ブレンドシェイプの値を保存し、あとから復元します",
                BlendShapeBackuperDescription),
            new AvatarToolboxToolEntry(
                "07",
                "07_Parts Set Adjuster",
                "別のアバター向けのパーツを、体に合わせて取り付けます",
                PartsSetAdjusterDescription,
                VrchatSdkRequirement,
                VrchatSdkAvailable),
            new AvatarToolboxToolEntry(
                "08",
                "08_FaceEmo Splitter",
                "FaceEmo の表情を、Prefab Variant ごとに分けて適用します",
                FaceEmoSplitterDescription,
                FaceEmoRequirement,
                FaceEmoAvailable),
        };

        internal static AvatarToolboxToolEntry Find(string id)
        {
            for (int i = 0; i < Tools.Length; i++)
            {
                if (Tools[i].Id == id)
                {
                    return Tools[i];
                }
            }

            return null;
        }
    }
}
