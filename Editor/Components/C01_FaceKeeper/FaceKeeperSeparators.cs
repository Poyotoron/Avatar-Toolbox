using System;
using UnityEngine;

namespace Poyo.AvatarToolbox.FaceKeeper.Editor
{
    internal static class FaceKeeperSeparators
    {
        private static readonly char[] SeparatorChars = { '-', '=', '－', '＝', '─', '━', '―' };

        internal static bool[] Resolve(Mesh mesh)
        {
            if (mesh == null)
            {
                return Array.Empty<bool>();
            }
            var result = new bool[mesh.blendShapeCount];
            Vector3[] vertices = null;
            Vector3[] normals = null;
            Vector3[] tangents = null;
            for (int i = 0; i < result.Length; i++)
            {
                string name = mesh.GetBlendShapeName(i).Trim();
                if (name.Length >= 2 &&
                    (IsSeparatorChar(name[0]) && IsSeparatorChar(name[1]) ||
                    IsSeparatorChar(name[name.Length - 2]) && IsSeparatorChar(name[name.Length - 1])))
                {
                    result[i] = true;
                    continue;
                }
                if (name.Length == 0 || !IsSymbol(name[0]) && !IsSymbol(name[name.Length - 1]))
                {
                    continue;
                }
                // NOTE: 頂点の読み取りは重いため、名前が記号で始まるか終わるものだけ調べる。
                if (vertices == null)
                {
                    vertices = new Vector3[mesh.vertexCount];
                    normals = new Vector3[mesh.vertexCount];
                    tangents = new Vector3[mesh.vertexCount];
                }
                result[i] = IsEmpty(mesh, i, vertices, normals, tangents);
            }
            return result;
        }

        private static bool IsSeparatorChar(char value)
        {
            return Array.IndexOf(SeparatorChars, value) >= 0;
        }

        private static bool IsSymbol(char value)
        {
            return !char.IsLetterOrDigit(value) && !char.IsWhiteSpace(value);
        }

        private static bool IsEmpty(Mesh mesh, int index, Vector3[] vertices, Vector3[] normals, Vector3[] tangents)
        {
            for (int frame = 0; frame < mesh.GetBlendShapeFrameCount(index); frame++)
            {
                mesh.GetBlendShapeFrameVertices(index, frame, vertices, normals, tangents);
                foreach (Vector3 vertex in vertices)
                {
                    if (vertex.sqrMagnitude > 1e-12f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
