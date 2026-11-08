using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal static class FaceKeeperMeshes
    {
        internal static void Resolve(PATFaceKeeper component, Transform avatarRoot,
            List<SkinnedMeshRenderer> result, out string error)
        {
            result.Clear();
            error = null;
            if (component.manualMeshes)
            {
                if (component.meshes != null)
                {
                    foreach (SkinnedMeshRenderer mesh in component.meshes)
                    {
                        if (mesh == null)
                        {
                            continue;
                        }
                        if (avatarRoot != null && !mesh.transform.IsChildOf(avatarRoot))
                        {
                            error = "アバターの外にあるメッシュは使えません。";
                        }
                        if (!result.Contains(mesh))
                        {
                            result.Add(mesh);
                        }
                    }
                }
                if (result.Count == 0)
                {
                    error = "顔のメッシュを 1 つ以上指定してください。";
                }
            }
            else
            {
                VRCAvatarDescriptor descriptor = avatarRoot != null
                    ? avatarRoot.GetComponent<VRCAvatarDescriptor>() : null;
                FindAutomatic(descriptor, result);
                if (result.Count == 0)
                {
                    error = "顔のメッシュが見つかりません。「自分で指定する」で指定してください。";
                }
            }
        }

        internal static void FindAutomatic(VRCAvatarDescriptor descriptor, List<SkinnedMeshRenderer> result)
        {
            result.Clear();
            if (descriptor == null)
            {
                return;
            }
            if (descriptor.lipSync == VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape &&
                descriptor.VisemeSkinnedMesh != null)
            {
                result.Add(descriptor.VisemeSkinnedMesh);
            }
            if (descriptor.enableEyeLook &&
                descriptor.customEyeLookSettings.eyelidType == VRCAvatarDescriptor.EyelidType.Blendshapes &&
                descriptor.customEyeLookSettings.eyelidsSkinnedMesh != null &&
                !result.Contains(descriptor.customEyeLookSettings.eyelidsSkinnedMesh))
            {
                result.Add(descriptor.customEyeLookSettings.eyelidsSkinnedMesh);
            }
        }
    }
}
