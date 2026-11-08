namespace Poyo.AvatarToolbox.Editor
{
    internal sealed class AvatarToolboxComponentEntry
    {
        internal readonly string Id;
        internal readonly string DisplayName;
        internal readonly string ComponentName;
        internal readonly string Summary;
        internal readonly string Description;
        internal readonly string RequirementLabel;
        internal readonly bool IsAvailable;

        internal AvatarToolboxComponentEntry(
            string id, string displayName, string componentName, string summary,
            string description, string requirementLabel, bool isAvailable)
        {
            Id = id;
            DisplayName = displayName;
            ComponentName = componentName;
            Summary = summary;
            Description = description;
            RequirementLabel = requirementLabel;
            IsAvailable = isAvailable;
        }
    }

    internal static class AvatarToolboxComponentCatalog
    {
        // NOTE: const は要件の有無で到達不能な分岐を作るため使わない。
#if AVATAR_TOOLBOX_HAS_NDMF
        private static readonly bool NdmfAvailable = true;
#else
        private static readonly bool NdmfAvailable = false;
#endif
#if AVATAR_TOOLBOX_HAS_MA
        private static readonly bool MaAvailable = true;
#else
        private static readonly bool MaAvailable = false;
#endif
#if AVATAR_TOOLBOX_HAS_VRCSDK
        private static readonly bool SdkAvailable = true;
#else
        private static readonly bool SdkAvailable = false;
#endif

        internal static readonly AvatarToolboxComponentEntry[] Components =
        {
            new AvatarToolboxComponentEntry(
                "C00", "C00_Animation Helper", "PAT Animation Helper",
                "Transform とブレンドシェイプを動かすメニューを、値を入れるだけで作ります",
                "動かす対象と値を入れるだけで、Radial Puppet とトグルのメニューを作ります。\n" +
                "トグルでは、ON にしたときに開始値から終了値へ、決めた時間をかけて動かせます（大きさを 0 から 1 にして出す、など）。\n" +
                "アップロードのときに Modular Avatar の設定に変わります。アニメーションを自分で作る必要はありません。",
                "Modular Avatar 1.17.0 以降", MaAvailable && NdmfAvailable && SdkAvailable),
            new AvatarToolboxComponentEntry(
                "C01", "C01_Face Keeper", "PAT Face Keeper",
                "表情アニメで改変したブレンドシェイプが戻るのを、アップロードのときに防ぎます",
                "00_Blendshape Keeper の後継です。アバタールートに付けます。\n" +
                "アップロードのとき、表情アニメーションのキー値が顔のブレンドシェイプの今の値より小さければ、今の値まで引き上げます。\n" +
                "FaceEmo や Modular Avatar で入れた表情にもかかります。元のアニメーションは書き換えません。",
                "NDMF 1.14.0 以降（2.x は未対応）、VRChat SDK - Base 3.5.0 以降",
                NdmfAvailable && SdkAvailable),
            new AvatarToolboxComponentEntry(
                "C02", "C02_Bone Weight Transfer", "PAT Bone Weight Transfer",
                "不要なボーンのウェイトを、アップロードのときに別のボーンへ移します",
                "05_Bone Weight Collapser の後継です。衣装の Skinned Mesh Renderer に付けます。\n" +
                "アップロードのとき、指定したボーンのウェイトを別のボーンへ移します。\n" +
                "袖が指ボーンに引きずられて変形する、といった問題を止めます。メッシュのファイルは作りません。",
                "NDMF 1.14.0 以降（2.x は未対応）", NdmfAvailable)
        };

        internal static AvatarToolboxComponentEntry Find(string id)
        {
            foreach (AvatarToolboxComponentEntry entry in Components)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }
            return null;
        }
    }
}
