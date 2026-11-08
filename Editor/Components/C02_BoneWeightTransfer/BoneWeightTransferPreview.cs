using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    internal sealed class BoneWeightTransferPreview : IRenderFilter
    {
        internal static readonly TogglablePreviewNode EnableNode = TogglablePreviewNode.Create(
            () => "PAT Bone Weight Transfer",
            qualifiedName: "net.maaaaa.avatar-toolbox.bone-weight-transfer/Preview", true);

        public IEnumerable<TogglablePreviewNode> GetPreviewControlNodes()
        {
            yield return EnableNode;
        }

        public bool IsEnabled(ComputeContext context)
        {
            return context.Observe(EnableNode.IsEnabled);
        }

        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var groups = ImmutableList.CreateBuilder<RenderGroup>();
            foreach (GameObject root in context.GetAvatarRoots())
            {
                foreach (PATBoneWeightTransfer component in
                    context.GetComponentsInChildren<PATBoneWeightTransfer>(root, true))
                {
                    SkinnedMeshRenderer renderer = context.GetComponent<SkinnedMeshRenderer>(component.gameObject);
                    if (renderer != null)
                    {
                        groups.Add(RenderGroup.For(renderer));
                    }
                }
            }
            return groups.ToImmutable();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group,
            IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            Mesh mesh = null;
            foreach (Renderer renderer in group.Renderers)
            {
                PATBoneWeightTransfer component = context.GetComponent<PATBoneWeightTransfer>(renderer.gameObject);
                if (component == null)
                {
                    continue;
                }
                context.Observe(component);
                context.Observe(renderer);
                BoneWeightTransferResult result = BoneWeightTransferProcessor.Process(component, true, false);
                if (result.ErrorKey == null && result.Stats.AffectedVertexCount > 0)
                {
                    mesh = result.BakedMesh;
                    mesh.hideFlags = HideFlags.HideAndDontSave;
                }
                else if (result.BakedMesh != null)
                {
                    Object.DestroyImmediate(result.BakedMesh);
                }
            }
            return Task.FromResult<IRenderFilterNode>(new Node(mesh));
        }

        private sealed class Node : IRenderFilterNode
        {
            private readonly Mesh _mesh;

            internal Node(Mesh mesh)
            {
                _mesh = mesh;
            }

            public RenderAspects WhatChanged => RenderAspects.Mesh;

            public Task<IRenderFilterNode> Refresh(IEnumerable<(Renderer, Renderer)> proxyPairs,
                ComputeContext context, RenderAspects updatedAspects)
            {
                if ((updatedAspects & RenderAspects.Mesh) != 0 || _mesh == null)
                {
                    return Task.FromResult<IRenderFilterNode>(null);
                }
                // NOTE: 入力の変更による更新でも、監視とメッシュを作り直す。
                return Task.FromResult<IRenderFilterNode>(null);
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                if (_mesh != null && proxy is SkinnedMeshRenderer renderer)
                {
                    renderer.sharedMesh = _mesh;
                }
            }

            public void Dispose()
            {
                if (_mesh != null)
                {
                    Object.DestroyImmediate(_mesh);
                }
            }
        }
    }
}
