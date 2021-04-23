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
}