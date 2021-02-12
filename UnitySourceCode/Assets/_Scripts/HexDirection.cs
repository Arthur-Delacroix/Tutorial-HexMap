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
}