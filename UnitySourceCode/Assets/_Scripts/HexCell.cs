using UnityEngine;

public class HexCell : MonoBehaviour
{
    //在实例化每个cell的时候会调用该实例
    //针对每个cell，重新计算它的坐标值
    public HexCoordinates coordinates;

    //存储cell自身的颜色
    public Color color;

    //用来存储每个cell的neighbors
    [SerializeField] private HexCell[] neighbors;

    //表示每个cell的高度等级，0即在水平面位置上
    public int elevation;

    /// <summary>
    /// 用来获取neighbors中相应方位cell的实例
    /// 这里注意，虽然HexDirection取值为0-5，neighbors长度为6，不会越界
    /// 但是不是所有cell都有6个相邻的cell_neighbor，所以可能取出为空，在取值时候会添加判断
    /// </summary>
    /// <param name="direction">相邻cell的方位 枚举</param>
    /// <returns>相应方位cell 的实例</returns>
    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    /// <summary>
    /// 将相邻的cell实例赋值到neighbors中对应的位置
    /// 这里注意neighbors的索引下标，应与HexDirection方位的int值对应
    /// </summary>
    /// <param name="direction">相邻cell的方位</param>
    /// <param name="cell">相邻cell的实例</param>
    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        //通过枚举索引，将相邻的cell放入自身数组中的相对应位置上
        neighbors[(int)direction] = cell;

        //在赋值自身的neighbors实例的同时，也将自身实例赋值到相邻cell的neighbors数组中
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    //public Vector3 v1;
    //public Vector3 v2 { get; private set; }
    //void Start()
    //{
    //    v1.Set(1, 2, 3);
    //    v1.x = 4;
    //    v2.Set(1, 2, 3);      //  (Note 2)  
    //    //v2.x = 4;           //  (Note 1)  
    //    Debug.Log(v1.ToString());
    //    Debug.Log(v2.ToString());
    //}
}