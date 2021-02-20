using UnityEngine;

public static class HexMetrics
{
    //正六边形的边长 同时也是正六边形外接圆的半径
    public const float outerRadius = 10f;

    //正六边形的内切圆半径，长度为外接圆的 二分之根号三倍
    public const float innerRadius = outerRadius * 0.866025404f;

    //cell自身颜色区域，为75%外接圆半径
    public const float solidFactor = 0.75f;

    //cell的颜色混合区域，为25%外接圆半径
    public const float blendFactor = 1f - solidFactor;

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

    /// <summary>
    /// 获取cell自身颜色区域的direction位置的顶点
    /// </summary>
    /// <param name="direction">顶点方位</param>
    /// <returns></returns>
    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int)direction] * solidFactor;
    }

    /// <summary>
    /// 获取cell自身颜色区域的direction+1位置的顶点
    /// </summary>
    /// <param name="direction">顶点方位</param>
    /// <returns></returns>
    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return corners[(int)direction + 1] * solidFactor;
    }

    /// <summary>
    /// 获取矩形混合区域中，内边缘顶点到外边缘顶点的距离
    /// </summary>
    /// <param name="direction">顶点方位</param>
    /// <returns></returns>
    public static Vector3 GetBridge(HexDirection direction)
    {
        //参考图片 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/2-8-1.png
        //(corners[(int)direction] + corners[(int)direction + 1]) * 0.5f 是得出cell相邻两个顶点所连线的中点的位置
        //其实也就是内切圆和cell相切的一个切点，也就是线段V3V4的中点，其实也是角∠(V1 center v2)的角平分线
        //具体可以看图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/1-1-3.png 来理解
        //得出V3 V4中点位置后，再乘以颜色混合区域所占比例，即25%，得出V1到V3的距离
        return (corners[(int)direction] + corners[(int)direction + 1]) * 0.5f * blendFactor;
    }
}