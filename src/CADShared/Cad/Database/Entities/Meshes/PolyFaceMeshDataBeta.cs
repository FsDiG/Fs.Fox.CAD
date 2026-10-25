using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

// SDK-free validation shared by the production builder and executable tests.
internal static class PolyFaceMeshDataBeta
{
    internal static List<short[]> ValidateAndCopyFaces(int vertexCount, IEnumerable<IEnumerable<int>> faces)
    {
        if (vertexCount < 3 || vertexCount > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertexCount), "顶点数必须为 3 至 32767。");
        if (faces is null)
            throw new ArgumentNullException(nameof(faces));
        var result = new List<short[]>();
        foreach (var face in faces)
        {
            if (result.Count == short.MaxValue)
                throw new ArgumentException("面数最多为 32767。", nameof(faces));
            if (face is null)
                throw new ArgumentException("面不能为 null。", nameof(faces));
            var indices = new short[4];
            var count = 0;
            foreach (var index in face)
            {
                if (count == 4)
                    throw new ArgumentException("只支持三角面和四边面。", nameof(faces));
                if (index < 0 || index >= vertexCount)
                    throw new ArgumentException("面索引必须为有效的从零开始的顶点索引。", nameof(faces));
                var converted = checked((short)(index + 1));
                for (var i = 0; i < count; i++)
                    if (indices[i] == converted)
                        throw new ArgumentException("同一面内不允许重复顶点索引。", nameof(faces));
                indices[count++] = converted;
            }
            if (count < 3)
                throw new ArgumentException("每个面至少需要三个顶点索引。", nameof(faces));
            // Native FaceRecord uses 1-based indices; a triangle's fourth index must be zero.
            result.Add(indices);
        }
        if (result.Count == 0)
            throw new ArgumentException("至少需要一个面。", nameof(faces));
        return result;
    }
}
