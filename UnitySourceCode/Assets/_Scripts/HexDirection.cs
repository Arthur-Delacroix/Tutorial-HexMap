//表示相邻cell方位的枚举
//从左上角顺时针依次开始
//参考图：http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/2-1-2.png
public enum HexDirection
{
    NE,
    E,
    SE,
    SW,
    W,
    NW
}

public static class HexDirectionExtensions
{
    /// <summary>
    /// 参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/2-1-4.png
    /// 根据 相邻cell 位于自身的位置，获得自身位于 相邻cell 的位置
    /// 也就是得到相反位置
    /// 例如一个相邻cell位于E方位，那么相对于 相邻cell，自身为W方位，也就是在 相邻cell 中，自身的实例放在W这个位置上
    /// </summary>
    /// <param name="direction">相邻cell的位置</param>
    /// <returns>自身相对于相邻cell的位置</returns>
    public static HexDirection Opposite(this HexDirection direction)
    {
        //三元运算符不直观，用下面的方法替换掉
        //return (int)direction < 3 ? (direction + 3) : (direction - 3);

        //已知了 相邻cell 的位置，自身cell位置与相邻cell位置相反
        //即 W(1) - E(4)这样的对应关系，之间正好相差3
        if ((int)direction < 3)
        {
            return (direction + 3);
        }
        else
        {
            return (direction - 3);
        }
    }

    /// <summary>
    /// 获取当前相邻cell 之前的一个cell的方位
    /// </summary>
    /// <param name="direction">当前相邻cell的方位</param>
    /// <returns></returns>
    public static HexDirection Previous(this HexDirection direction)
    {
        //如果当前cell位置为NE，即索引为0，其之前的一个cell方位应该为NE，即索引为6
        //除此之外的情况，只要当前索引值减1即可
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    /// <summary>
    /// 获取当前相邻cell 之后的一个cell的方位
    /// </summary>
    /// <param name="direction">当前相邻cell的方位</param>
    /// <returns></returns>
    public static HexDirection Next(this HexDirection direction)
    {
        //与Previous方法中类似，方位索引为6时，回到0，其余情况则方位索引加1
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}