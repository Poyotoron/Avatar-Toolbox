using System.Collections.Generic;
using UnityEngine;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal sealed class FaceKeeperExclusionMesh
    {
        internal string Label;
        internal bool Expanded;
        internal int[] Ungrouped;
        internal FaceKeeperExclusionGroup[] Groups;
        internal string[] Names;
        internal GUIContent[] Labels;
        internal SkinnedMeshRenderer Renderer;
        internal Mesh Mesh;
        internal bool[] Separators;
    }

    internal sealed class FaceKeeperExclusionGroup
    {
        internal GUIContent Label;
        internal string Count;
        internal int[] Members;
        internal bool Expanded;
    }

    internal static class FaceKeeperExclusionView
    {
        internal static List<FaceKeeperExclusionMesh> Build(List<SkinnedMeshRenderer> meshes,
            HashSet<string> excluded, List<FaceKeeperExclusionMesh> previous)
        {
            var result = new List<FaceKeeperExclusionMesh>();
            var found = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (SkinnedMeshRenderer renderer in meshes)
            {
                if (renderer == null || renderer.sharedMesh == null)
                {
                    continue;
                }
                Mesh mesh = renderer.sharedMesh;
                FaceKeeperExclusionMesh old = previous.Find(candidate =>
                    candidate.Renderer == renderer && candidate.Mesh == mesh);
                var view = new FaceKeeperExclusionMesh
                {
                    Renderer = renderer,
                    Mesh = mesh,
                    Expanded = old != null ? old.Expanded : meshes.Count == 1,
                    // NOTE: チェックの変更では頂点を読み直さず、同じメッシュの区切り判定を再利用する。
                    Separators = old != null ? old.Separators : FaceKeeperSeparators.Resolve(mesh),
                    Names = new string[mesh.blendShapeCount],
                    Labels = new GUIContent[mesh.blendShapeCount],
                };
                var ungrouped = new List<int>();
                var groups = new List<FaceKeeperExclusionGroup>();
                List<int> members = ungrouped;
                int count = 0;
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    string name = mesh.GetBlendShapeName(i);
                    view.Names[i] = name;
                    view.Labels[i] = new GUIContent(name);
                    found.Add(name);
                    if (view.Separators[i])
                    {
                        if (groups.Count > 0)
                        {
                            FinishGroup(groups[groups.Count - 1], members, view.Names, excluded);
                        }
                        FaceKeeperExclusionGroup oldGroup = old != null
                            ? System.Array.Find(old.Groups, group => group.Label.text == name) : null;
                        groups.Add(new FaceKeeperExclusionGroup
                        {
                            Label = view.Labels[i],
                            Expanded = oldGroup != null && oldGroup.Expanded,
                        });
                        members = new List<int>();
                    }
                    else
                    {
                        members.Add(i);
                        if (excluded.Contains(name))
                        {
                            count++;
                        }
                    }
                }
                if (groups.Count > 0)
                {
                    FinishGroup(groups[groups.Count - 1], members, view.Names, excluded);
                }
                view.Ungrouped = ungrouped.ToArray();
                view.Groups = groups.ToArray();
                view.Label = string.Format("{0}（{1} / {2}）", renderer.name, count, mesh.blendShapeCount);
                result.Add(view);
            }
            var missing = new List<string>();
            foreach (string name in excluded)
            {
                if (!found.Contains(name))
                {
                    missing.Add(name);
                }
            }
            if (missing.Count > 0)
            {
                FaceKeeperExclusionMesh old = previous.Find(candidate => candidate.Renderer == null && candidate.Mesh == null);
                var view = new FaceKeeperExclusionMesh
                {
                    Label = "見つからない名前",
                    Expanded = old == null || old.Expanded,
                    Names = missing.ToArray(),
                    Labels = new GUIContent[missing.Count],
                    Ungrouped = new int[missing.Count],
                    Groups = System.Array.Empty<FaceKeeperExclusionGroup>(),
                };
                for (int i = 0; i < missing.Count; i++)
                {
                    view.Labels[i] = new GUIContent(missing[i]);
                    view.Ungrouped[i] = i;
                }
                result.Add(view);
            }
            return result;
        }

        private static void FinishGroup(FaceKeeperExclusionGroup group, List<int> members,
            string[] names, HashSet<string> excluded)
        {
            group.Members = members.ToArray();
            int count = 0;
            foreach (int index in group.Members)
            {
                if (excluded.Contains(names[index]))
                {
                    count++;
                }
            }
            group.Count = string.Format("（{0} / {1}）", count, group.Members.Length);
        }
    }
}
