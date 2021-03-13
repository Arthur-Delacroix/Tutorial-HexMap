﻿using UnityEngine;

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

    //地图中每个高度等级之间相差的实际距离
    public const float elevationStep = 5f;

    //每个连接部分阶梯的数量
    public const int terracesPerSlope = 2;

    //横向步长个数
    //根据阶梯数量，计算出连接区域会被拆分成几个横向步长
    //参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-6-1.png
    public const int terraceSteps = terracesPerSlope * 2 + 1;

    //横向每个步长占整体长度的比例
    public const float horizontalTerraceStepSize = 1f / terraceSteps;

    //纵向每个步长占整体长度的比例
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

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
        //return (corners[(int)direction] + corners[(int)direction + 1]) * 0.5f * blendFactor;

        //这里对颜色混合区域进行优化
        //之前的 * 0.5f 的作用是：设两个cell的颜色混合区域宽度为1
        //那每个cell的颜色混合区域宽度都是 自身颜色到两者颜色相加的一半
        //也就是两个相邻的cell各自混合了一半，所以该区域宽度要 *.05f
        //在这里要将两个0.5宽度的颜色混合区域合并为一个整体，所以不在需要*.05f了
        return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
    }

    /// <summary>
    /// 通过插值计算出阶梯状连接区域，每个顶点的坐标位置
    /// </summary>
    /// <param name="a">起始顶点的位置</param>
    /// <param name="b">结束顶点的位置</param>
    /// <param name="step">第几个步长</param>
    /// <returns>根据步长插值计算得出的顶点位置</returns>
    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        //单个步长的比例 与 步长的个数，计算出现在顶点所在的比例
        float h = step * horizontalTerraceStepSize;

        //水平位置的X和Z，分别乘以 现在顶点所在的比例，得出该步长顶点的实际坐标
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;

        //参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-6-1.png
        //根据参考图可以知道，实时位置点位步长0，那么只有在奇数步长时候才会改变Y坐标
        //(step + 1) / 2取商的整数部分，其实就是将步长1 2 3 4，变成了1 1 2 2这种形式
        //也就是只在奇数步长的时候对Y值进行改变
        float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;

        //计算出Y的插值之后，计算出实际Y的坐标值
        a.y += (b.y - a.y) * v;

        return a;
    }

    /// <summary>
    /// 通过插值计算出集体装连接区域，每个顶点的颜色值
    /// 计算颜色值不需要考虑垂直方向坐标变化的问题，只需要根据水平插值便可以得出计算结果
    /// </summary>
    /// <param name="a">起始顶点的位置</param>
    /// <param name="b">结束顶点的位置</param>
    /// <param name="step">第几个步长</param>
    /// <returns></returns>
    public static Color TerraceLerp(Color a, Color b, int step)
    {
        //单个步长的比例 与 步长的个数，计算出现在顶点所在的比例
        float h = step * horizontalTerraceStepSize;

        //通过Color.Lerp方法，计算出现在顶点的颜色值
        return Color.Lerp(a, b, h);
    }
}