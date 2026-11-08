using System.Collections.Generic;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEditor;
using UnityEngine;

[assembly: ExportsPlugin(typeof(Poyo.AvatarToolbox.FaceKeeper.Editor.FaceKeeperPlugin))]

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    public sealed class FaceKeeperPlugin : Plugin<FaceKeeperPlugin>
    {
        public override string QualifiedName => "net.maaaaa.avatar-toolbox.face-keeper";
        public override string DisplayName => "PAT Face Keeper";

        protected override void Configure()
        {
            // NOTE: MA が入れたアニメーションと Shape Changer などが変える初期値を含めるため、MA の後に読む。
            InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar")
                .WithRequiredExtension(typeof(AnimatorServicesContext), sequence =>
                {
                    sequence.Run("Keep face blend shapes", Execute);
                });
        }

        private static void Execute(BuildContext ctx)
        {
            PATFaceKeeper[] components = ctx.AvatarRootObject.GetComponentsInChildren<PATFaceKeeper>(true);
            if (components.Length == 0)
            {
                return;
            }
            try
            {
                if (components.Length > 1)
                {
                    FaceKeeperErrors.Report(ErrorSeverity.Error, FaceKeeperErrors.Multiple);
                    return;
                }
                PATFaceKeeper component = components[0];
                var meshes = new List<SkinnedMeshRenderer>();
                FaceKeeperMeshes.Resolve(component, ctx.AvatarRootTransform, meshes, out string error);
                if (error != null)
                {
                    FaceKeeperErrors.Report(ErrorSeverity.Error, FaceKeeperErrors.Meshes, error);
                    return;
                }
                var animator = ctx.Extension<AnimatorServicesContext>();
                List<FaceKeeperTarget> targets = FaceKeeperTargets.Collect(
                    meshes, component.excludedBlendShapes, animator.ObjectPathRemapper);
                // NOTE: 通常のアップロードではプレビュー用のクリップを作らず、一覧の要求時だけ記録する。
                if (FaceKeeperListingSession.IsTarget(ctx.AvatarRootObject))
                {
                    FaceKeeperListingSession.Store(FaceKeeperListingBuilder.Build(ctx, targets));
                }
                var values = new Dictionary<EditorCurveBinding, float>();
                foreach (FaceKeeperTarget target in targets)
                {
                    values[target.Binding] = target.CurrentValue;
                }
                int count = 0;
                // NOTE: NDMF の仮想化されたクリップだけを変え、元のアセットには書き込まない。
                animator.AnimationIndex.EditClipsByBinding(values.Keys, clip =>
                {
                    foreach (KeyValuePair<EditorCurveBinding, float> value in values)
                    {
                        AnimationCurve curve = clip.GetFloatCurve(value.Key);
                        if (curve == null)
                        {
                            continue;
                        }
                        int raised = FaceKeeperRule.Raise(curve, value.Value);
                        if (raised > 0)
                        {
                            clip.SetFloatCurve(value.Key, curve);
                            count += raised;
                        }
                    }
                });
                if (count == 0)
                {
                    FaceKeeperErrors.Report(ErrorSeverity.NonFatal, FaceKeeperErrors.NoKeys);
                }
            }
            finally
            {
                foreach (PATFaceKeeper component in components)
                {
                    Object.DestroyImmediate(component);
                }
            }
        }
    }
}
