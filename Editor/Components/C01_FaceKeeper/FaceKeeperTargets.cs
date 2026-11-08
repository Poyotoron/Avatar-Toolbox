using System.Collections.Generic;
using nadena.dev.ndmf.animator;
using UnityEditor;
using UnityEngine;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal sealed class FaceKeeperTarget
    {
        internal EditorCurveBinding Binding;
        internal float CurrentValue;
        internal string MeshName;
        internal string BlendShapeName;
    }

    internal static class FaceKeeperTargets
    {
        internal static List<FaceKeeperTarget> Collect(List<SkinnedMeshRenderer> meshes,
            List<string> excluded, ObjectPathRemapper remapper)
        {
            var result = new List<FaceKeeperTarget>();
            var excludedNames = excluded != null ? new HashSet<string>(excluded) : new HashSet<string>();
            foreach (SkinnedMeshRenderer renderer in meshes)
            {
                if (renderer == null || renderer.sharedMesh == null)
                {
                    continue;
                }
                Mesh mesh = renderer.sharedMesh;
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    string name = mesh.GetBlendShapeName(i);
                    if (excludedNames.Contains(name))
                    {
                        continue;
                    }
                    // NOTE: 仮想化されたクリップには移動前のパスもあるため、すべての対応するパスを使う。
                    foreach (string path in remapper.GetAllPathsForObject(renderer.transform))
                    {
                        result.Add(new FaceKeeperTarget
                        {
                            Binding = EditorCurveBinding.FloatCurve(path, typeof(SkinnedMeshRenderer),
                                "blendShape." + name),
                            CurrentValue = renderer.GetBlendShapeWeight(i),
                            MeshName = renderer.name, BlendShapeName = name
                        });
                    }
                }
            }
            return result;
        }
    }
}
