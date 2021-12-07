using UnityEngine;

public struct EdgeVertices
{
    //4个顺时针排列在六边形一条边上的顶点
    public Vector3 v1, v2, v3, v4, v5;

    /// <summary>
    /// 通过六边形一条边上，边缘的两个顶点，计算出中间的两个点的位置
    /// </summary>
    /// <param name="corner1">三角面片顺时针的第一个顶点</param>
    /// <param name="corner2">三角面片顺时针的第二个顶点</param>
    public EdgeVertices(Vector3 corner1, Vector3 corner2)
    {
        v1 = corner1;
        //v2 = Vector3.Lerp(corner1, corner2, 1f / 3f);
        //v4 = Vector3.Lerp(corner1, corner2, 2f / 3f);
        //添加了新的v3顶点，其左右相邻的顶点位置也要相应的改变
        v2 = Vector3.Lerp(corner1, corner2, 0.25f);
        v3 = Vector3.Lerp(corner1, corner2, 0.5f);
        v4 = Vector3.Lerp(corner1, corner2, 0.75f);
        v5 = corner2;
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
        //添加了新的v3顶点
        result.v3 = HexMetrics.TerraceLerp(a.v3, b.v3, step);
        result.v4 = HexMetrics.TerraceLerp(a.v4, b.v4, step);
        result.v5 = HexMetrics.TerraceLerp(a.v5, b.v5, step);
        return result;
    }

    /// <summary>
    /// 有河流经过一条边时，三角形变为梯形，提醒区域中位线的顶点分布不是平均的
    /// 参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/6-14-2.png
    /// </summary>
    /// <param name="corner1">梯形区域第一个顶点</param>
    /// <param name="corner2">提醒区域最后一个顶点</param>
    /// <param name="outerStep">两侧顶点偏移量</param>
    public EdgeVertices(Vector3 corner1, Vector3 corner2, float outerStep)
    {
        //注意，这里不再是等距分布一条直线上的5个顶点
        //而是按照左右较窄，中间宽度为四分之一外径，这样分布的
        v1 = corner1;
        v2 = Vector3.Lerp(corner1, corner2, outerStep);
        v3 = Vector3.Lerp(corner1, corner2, 0.5f);
        v4 = Vector3.Lerp(corner1, corner2, 1f - outerStep);
        v5 = corner2;
    }
}