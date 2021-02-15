using UnityEngine;

public static class HexMetrics
{
    //正六边形的边长 同时也是正六边形外接圆的半径
    public const float outerRadius = 10f;

    //正六边形的内切圆半径，长度为外接圆的 二分之根号三倍
    public const float innerRadius = outerRadius * 0.866025404f;

    //正六边形的六个顶点位置，其姿态为角朝上，从最上面一个顶点开始计算位置
    //根据正六边形中点的位置，顺时针依次定义6个顶点的位置
    private static Vector3[] corners =
    {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        //正六边形其实只有6个顶点，但是当构建三角面片的时候，最后一个三角面片的顶点其实为：最后一个、第一个、中点，即corners[7]
        //为了减少在循环中的判断，这里添加一条数据，防止索引越界即可
        new Vector3(0f, 0f, outerRadius)
    };

    /// <summary>
    /// 获取cell的direction位置的顶点
    /// </summary>
    /// <param name="direction">顶点方位</param>
    /// <returns></returns>
    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    /// <summary>
    /// 获取cell的direction+1位置的顶点
    /// </summary>
    /// <param name="direction">顶点方位</param>
    /// <returns></returns>
    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }
}