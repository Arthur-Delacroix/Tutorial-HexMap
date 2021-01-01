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
    /// 根据 相邻cell 位于 自身cell 的位置，获得 自身cell 位于 相邻cell 的位置
    /// 也就是得到相反位置
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static HexDirection Opposite(this HexDirection direction)
    {
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