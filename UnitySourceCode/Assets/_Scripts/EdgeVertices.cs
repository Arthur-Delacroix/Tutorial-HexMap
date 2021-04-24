using UnityEngine;

public struct EdgeVertices
{
    //4个顺时针排列在六边形一条边上的顶点
    public Vector3 v1, v2, v3, v4;

    /// <summary>
    /// 通过六边形一条边上，边缘的两个顶点，计算出中间的两个点的位置
    /// </summary>
    /// <param name="corner1">三角面片顺时针的第一个顶点</param>
    /// <param name="corner2">三角面片顺时针的第二个顶点</param>
    public EdgeVertices(Vector3 corner1, Vector3 corner2)
    {
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, 1f / 3f);
        v3 = Vector3.Lerp(corner1, corner2, 2f / 3f);
        v4 = corner2;
    }

    /// <summary>
    /// 通过两个cell的边，计算出阶梯连接区域，每一段上顶点的位置
    /// </summary>
    /// <param name="a">阶梯连接区域起始边</param>
    /// <param name="b">姐弟连接区域结束边</param>
    /// <param name="step">插值</param>
    /// <returns>插值为step时，当前段落上每个顶点的位置信息</returns>
    public static EdgeVertices TerraceLerp(EdgeVertices a, EdgeVertices b, int step)
    {
        EdgeVertices result;
        result.v1 = HexMetrics.TerraceLerp(a.v1, b.v1, step);
        result.v2 = HexMetrics.TerraceLerp(a.v2, b.v2, step);
        result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step);
        result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);
        return result;
    }
}