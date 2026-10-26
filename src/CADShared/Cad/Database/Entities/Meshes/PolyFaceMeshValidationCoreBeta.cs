using System;
using System.Collections.Generic;

namespace Fs.Fox.Cad;

// 在任何原生写入之前完成面序列求值；构造器与无宿主回归共用此验证路径。
internal static class PolyFaceMeshValidationCoreBeta
{
    internal static List<short[]> ValidateAndConvertFaceIndices(int vertexCount, IEnumerable<IEnumerable<int>> faces)
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
            // 每面固定四槽：前三槽必须填写，三角面保留第四槽的零；不持有调用者的可变索引集合。
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
            // 原生 FaceRecord 使用一基索引，零仅用于三角面的第四槽；正数索引使所有边可见。
            result.Add(indices);
        }
        if (result.Count == 0)
            throw new ArgumentException("至少需要一个面。", nameof(faces));
        return result;
    }
}
