using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    //存储重新计算后的X坐标值
    public int X { get; private set; }

    //存储重新计算后的Z坐标值
    public int Z { get; private set; }

    public int qaz(int a)
    {
        return Z + a;
    }

    /// <summary>
    /// 重载默认的构造函数
    /// </summary>
    /// <param name="x">为转换后的X坐标赋初始值</param>
    /// <param name="z">为转换后的Z坐标赋初始值</param>
    public HexCoordinates(int x, int z)
    {
        X = x;
        Z = z;
    }

    //计算Y的坐标值并存储下来
    public int Y
    {
        get
        {
            return -X - Z;
        }
    }

    /// <summary>
    /// 进行X与Z的坐标转换，将X方向锯齿状的排列，改为斜向的排列
    /// 这个方法将mesh和坐标值分开处理了
    /// 这里的入参只是处理X和Z的坐标，与mesh的排列和位置无关
    /// </summary>
    /// <param name="x">原始cell的x轴坐标</param>
    /// <param name="z">原始cell的z轴坐标</param>
    /// <returns>目前返回传入的参数值</returns>
    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    /// <summary>
    /// 重载默认的ToString方法，使其返回的是转换后的X和Z的坐标值
    /// </summary>
    /// <returns>X和Z的坐标值</returns>
    public override string ToString()
    {
        //return "(" + X.ToString() + ", " + Z.ToString() + ")";

        //加入了Y坐标值的表示
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    /// <summary>
    /// 将转换后的X和Z的坐标值添加换行符，以便显示在UGUI的每个cell上
    /// </summary>
    /// <returns>添加换行符后的X和Z，符合Text组件的富文本格式</returns>
    public string ToStringOnSeparateLines()
    {
        //return X.ToString() + "\n" + Z.ToString();

        //加入了Y坐标值的输出
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }
}