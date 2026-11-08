using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.AvatarToolbox.BoneWeightTransfer.Editor
{
    internal sealed class BoneWeightTransferVertexOverlay : IDisposable
    {
        private readonly PATBoneWeightTransfer _component;
        private readonly Mesh _baked = new Mesh { hideFlags = HideFlags.HideAndDontSave };
        private SkinnedMeshRenderer _renderer;
        private int[] _indices = Array.Empty<int>();
        private float[] _amounts = Array.Empty<float>();
        private Vector3[] _positions = Array.Empty<Vector3>();
        internal bool Enabled { get; private set; }
        internal string CountLabel { get; private set; } = "点の数: 0";

        internal BoneWeightTransferVertexOverlay(PATBoneWeightTransfer component)
        {
            _component = component;
        }

        internal void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
            {
                return;
            }
            Enabled = enabled;
            if (enabled)
            {
                SceneView.duringSceneGui += Draw;
                Refresh();
            }
            else
            {
                SceneView.duringSceneGui -= Draw;
                SceneView.RepaintAll();
            }
        }

        internal void Refresh()
        {
            if (!Enabled || _component == null)
            {
                return;
            }
            BoneWeightTransferResult result = BoneWeightTransferProcessor.Process(_component, false, true);
            _renderer = result.Renderer;
            var indices = new List<int>();
            if (result.ErrorKey == null && result.MovedPerVertex != null)
            {
                for (int i = 0; i < result.MovedPerVertex.Length; i++)
                {
                    if (result.MovedPerVertex[i] > 0f)
                    {
                        indices.Add(i);
                    }
                }
                indices.Sort((left, right) => result.MovedPerVertex[right].CompareTo(result.MovedPerVertex[left]));
            }
            if (indices.Count > 5000)
            {
                indices.RemoveRange(5000, indices.Count - 5000);
            }
            _indices = indices.ToArray();
            _amounts = new float[_indices.Length];
            _positions = new Vector3[_indices.Length];
            for (int i = 0; i < _indices.Length; i++)
            {
                _amounts[i] = result.MovedPerVertex[_indices[i]];
            }
            CountLabel = string.Format("点の数: {0}", _indices.Length);
            RefreshPositions();
        }

        internal void RefreshPositions()
        {
            if (!Enabled || _renderer == null || _renderer.sharedMesh == null)
            {
                return;
            }
            _renderer.BakeMesh(_baked, true);
            Vector3[] vertices = _baked.vertices;
            Matrix4x4 matrix = _renderer.transform.localToWorldMatrix;
            for (int i = 0; i < _indices.Length; i++)
            {
                _positions[i] = matrix.MultiplyPoint3x4(vertices[_indices[i]]);
            }
            SceneView.RepaintAll();
        }

        private void Draw(SceneView scene)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            Color previous = Handles.color;
            for (int i = 0; i < _positions.Length; i++)
            {
                Vector3 position = _positions[i];
                Handles.color = Color.Lerp(Color.yellow, Color.red, _amounts[i]);
                Handles.DotHandleCap(0, position, Quaternion.identity,
                    HandleUtility.GetHandleSize(position) * 0.015f, EventType.Repaint);
            }
            Handles.color = previous;
        }

        public void Dispose()
        {
            SetEnabled(false);
            UnityEngine.Object.DestroyImmediate(_baked);
        }
    }
}
