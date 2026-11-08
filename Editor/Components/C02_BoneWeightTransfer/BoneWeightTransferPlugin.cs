using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(Poyo.AvatarToolbox.BoneWeightTransfer.Editor.BoneWeightTransferPlugin))]

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    public sealed class BoneWeightTransferPlugin : Plugin<BoneWeightTransferPlugin>
    {
        public override string QualifiedName => "net.maaaaa.avatar-toolbox.bone-weight-transfer";
        public override string DisplayName => "PAT Bone Weight Transfer";

        protected override void Configure()
        {
            // NOTE: Merge Armature が衣装のボーンを付け替える前に、元の Transform で規則を解決する。
            InPhase(BuildPhase.Generating).Run("Transfer bone weights", Execute)
                .PreviewingWith(new BoneWeightTransferPreview());
        }

        private static void Execute(BuildContext ctx)
        {
            foreach (PATBoneWeightTransfer component in
                ctx.AvatarRootObject.GetComponentsInChildren<PATBoneWeightTransfer>(true))
            {
                try
                {
                    BoneWeightTransferResult result = BoneWeightTransferProcessor.Process(component, true, false);
                    foreach (BoneWeightTransferTargetRule rule in result.Rules)
                    {
                        if (!rule.IsApplicable)
                        {
                            BoneWeightTransferErrors.Report(ErrorSeverity.NonFatal,
                                BoneWeightTransferErrors.RuleSkipped, component.name,
                                rule.Rule.Number, rule.BlockedLabel);
                        }
                    }
                    if (result.ErrorKey != null)
                    {
                        BoneWeightTransferErrors.Report(ErrorSeverity.Error, result.ErrorKey, component.name);
                    }
                    else if (result.Stats.AffectedVertexCount == 0)
                    {
                        BoneWeightTransferErrors.Report(ErrorSeverity.NonFatal,
                            BoneWeightTransferErrors.NoVertex, component.name);
                        if (result.BakedMesh != null)
                        {
                            Object.DestroyImmediate(result.BakedMesh);
                        }
                    }
                    else
                    {
                        result.Renderer.sharedMesh = result.BakedMesh;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(component);
                }
            }
        }
    }
}
