using nadena.dev.modular_avatar.core;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Poyo.AvatarToolbox.AnimationHelper.Editor
{
    internal static class AnimationHelperGenerator
    {
        internal static void Generate(PATAnimationHelper component, Transform avatarRoot)
        {
            bool radial = component.mode == PATAnimationHelperMode.Radial;
            bool automaticName = string.IsNullOrEmpty(component.parameterName);
            string parameter = automaticName ? "PAT_AnimationHelper" : component.parameterName;
            string menuName = string.IsNullOrEmpty(component.menuName) ? component.gameObject.name : component.menuName;
            var generated = new GameObject(menuName);
            generated.transform.SetParent(component.menuParent != null
                ? component.menuParent.transform : component.transform, false);
            if (component.menuParent == null)
            {
                generated.AddComponent<ModularAvatarMenuInstaller>();
            }
            ModularAvatarMenuItem item = generated.AddComponent<ModularAvatarMenuItem>();
            item.Control = new VRCExpressionsMenu.Control
            {
                name = menuName,
                type = radial ? VRCExpressionsMenu.Control.ControlType.RadialPuppet
                    : VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = radial ? "" : parameter },
                value = 1f,
                icon = component.menuIcon,
                subParameters = radial
                    ? new[] { new VRCExpressionsMenu.Control.Parameter { name = parameter } }
                    : new VRCExpressionsMenu.Control.Parameter[0]
            };
            float defaultValue;
            // NOTE: 曲線のパスはアバタールートから作るため、生成先にかかわらず Absolute を使う。
            if (radial)
            {
                BlendTree tree = AnimationHelperRadialBuilder.Build(component, avatarRoot, parameter, out defaultValue);
                ModularAvatarMergeBlendTree merge = generated.AddComponent<ModularAvatarMergeBlendTree>();
                merge.Motion = tree;
                merge.PathMode = MergeAnimatorPathMode.Absolute;
            }
            else
            {
                defaultValue = component.toggleDefaultOn ? 1f : 0f;
                ModularAvatarMergeAnimator merge = generated.AddComponent<ModularAvatarMergeAnimator>();
                merge.animator = AnimationHelperToggleBuilder.Build(component, avatarRoot, parameter);
                merge.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                merge.pathMode = MergeAnimatorPathMode.Absolute;
                merge.matchAvatarWriteDefaults = true;
                merge.layerPriority = 0;
                merge.deleteAttachedAnimator = false;
                merge.mergeAnimatorMode = MergeAnimatorMode.Append;
            }
            ModularAvatarParameters parameters = generated.AddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(new ParameterConfig
            {
                nameOrPrefix = parameter, remapTo = "", isPrefix = false,
                internalParameter = automaticName,
                syncType = radial ? ParameterSyncType.Float : ParameterSyncType.Bool,
                defaultValue = defaultValue, hasExplicitDefaultValue = true,
                saved = component.parameterSaved, localOnly = !component.parameterSynced
            });
        }
    }
}
