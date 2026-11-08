using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(Poyo.AvatarToolbox.AnimationHelper.Editor.AnimationHelperPlugin))]

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    public sealed class AnimationHelperPlugin : Plugin<AnimationHelperPlugin>
    {
        public override string QualifiedName => "net.maaaaa.avatar-toolbox.animation-helper";
        public override string DisplayName => "PAT Animation Helper";

        protected override void Configure()
        {
            // NOTE: 生成した MA のコンポーネントを MA 自身に処理させるため、MA より先に走らせる。
            InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar")
                .Run("Generate animation helper menus", Execute);
        }

        private static void Execute(BuildContext ctx)
        {
            PATAnimationHelper[] components = ctx.AvatarRootObject.GetComponentsInChildren<PATAnimationHelper>(true);
            var issues = new List<AnimationHelperIssue>();
            foreach (PATAnimationHelper component in components)
            {
                try
                {
                    AnimationHelperValidator.Validate(component, ctx.AvatarRootTransform, issues);
                    bool invalid = false;
                    foreach (AnimationHelperIssue issue in issues)
                    {
                        if (issue.IsError)
                        {
                            invalid = true;
                            AnimationHelperErrors.Report(ErrorSeverity.Error, AnimationHelperErrors.Invalid,
                                component.name, issue.Message);
                        }
                    }
                    if (!invalid)
                    {
                        AnimationHelperGenerator.Generate(component, ctx.AvatarRootTransform);
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
