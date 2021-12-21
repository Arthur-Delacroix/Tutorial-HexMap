using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    //用来显示在Inspector上cell坐标
    [SerializeField]
    private int x, z;

    //存储重新计算后的X坐标值
    //public int X { get; private set; }
    public int X
    {
        get
        {
            return x;
        }
    }


    //存储重新计算后的Z坐标值
    //public int Z { get; private set; }
    public int Z
    {
        get
        {
            return z;
        }
    }

    /// <summary>
    /// 重载默认的构造函数
    /// </summary>
    /// <param name="x">为转换后的X坐标赋初始值</param>
    /// <param name="z">为转换后的Z坐标赋初始值</param>
    public HexCoordinates(int x, int z)
    {
        //X = x;
        //Z = z;

        this.x = x;
        this.z = z;
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

    /// <summary>
    /// 将原有Unity坐标转换成Hexmap内坐标
    /// </summary>
    /// <param name="position">点击在cell上的Unity坐标</param>
    /// <returns></returns>
    public static HexCoordinates FromPosition(Vector3 position)
    {
        //string orign_x = "X原始坐标为：" + position.x.ToString();
        //string orign_z = "Y原始坐标为：" + position.z.ToString();

        //这里有疑问
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;

        //-----------------------------------------------------------------------------
        //string s_x = "X除以2innerRadius的值为：" + x.ToString();
        //string s_y = "Y为X的相反数：" + y.ToString();
        //string s_2innerRadius = "2innerRadius的值为：" + (HexMetrics.innerRadius * 2f).ToString();

        //Debug.Log("<color=#00ffffff>" + orign_x + "</color>");
        //Debug.Log("<color=#00ffffff>" + orign_z + "</color>");

        //LoggerTool.LogWarning(s_x);
        //LoggerTool.LogWarning(s_2innerRadius);
        //LoggerTool.LogWarning(s_y);
        //-----------------------------------------------------------------------------

        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        //-----------------------------------------------------------------------------
        //string s_offset = "offset为Z除以3outerRadius：" + offset.ToString();
        //string s_3outerRadius = "3outerRadius值为：" + (HexMetrics.outerRadius * 3f).ToString();

        //string s_Xoffset = "X减去offset的值为：" + x.ToString();
        //string s_Yoffset = "Y减去offset的值为：" + y.ToString();

        //LoggerTool.LogColorMessage("#ff00ffff ", s_offset);
        //LoggerTool.LogColorMessage("#ff00ffff ", s_3outerRadius);
        //LoggerTool.LogColorMessage("#ff00ffff ", s_Xoffset);
        //LoggerTool.LogColorMessage("#ff00ffff ", s_Yoffset);
        //-----------------------------------------------------------------------------

        //对得出的坐标进行四舍五入，得到转换后的Hexmap坐标
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        //验证X+Y+Z是否为0
        //if (iX + iY + iZ != 0)
        //{
        //    Debug.LogWarning("rounding error!");
        //}
        
        //当X Y Z四舍五入后的和不为0时进行判断
        if (iX + iY + iZ != 0)
        {
            //计算出X Y Z各被舍去的值
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            //判断哪个舍去的值最多
            //利用X+Y+Z=0的特性，使用两个舍去较小的值得，求出社区较大的那个
            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        //-----------------------------------------------------------------------------
        //string s_ix = "iX的值为：" + iX.ToString();
        //string s_iy = "iY的值为：" + iY.ToString();
        //string s_iz = "iZ的值为：" + iZ.ToString();

        //LoggerTool.LogColorMessage("#0000ffff", s_ix);
        //LoggerTool.LogColorMessage("#0000ffff", s_iy);
        //LoggerTool.LogColorMessage("#0000ffff", s_iz);

        return new HexCoordinates(iX, iZ);
    }
}