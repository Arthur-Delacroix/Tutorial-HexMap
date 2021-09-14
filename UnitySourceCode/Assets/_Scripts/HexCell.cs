using UnityEngine;

public class HexCell : MonoBehaviour
{
    //在实例化每个cell的时候会调用该实例
    //针对每个cell，重新计算它的坐标值
    public HexCoordinates coordinates;

    //存储cell自身的颜色
    //public Color color;

    //用来存储每个cell的neighbors
    [SerializeField] private HexCell[] neighbors = null;

    //表示每个cell的高度等级，0即在水平面位置上
    //private int elevation;
    //为高度赋初始值，这样避免了初始值为0，新输入的值也为0，不会刷新mesh的问题
    private int elevation = int.MinValue;

    //自身坐标UI的RectTransform组件实例
    public RectTransform uiRect;

    //引用当前其所在的地图块
    public HexGridChunk chunk;

    //获取cell在扰动后的实际坐标位置
    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }

    private Color color;

    //cell颜色
    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            //当新颜色与现在颜色相同时，不再进行赋值和刷新
            if (color == value)
            {
                return;
            }

            color = value;

            Refresh();
        }
    }

    //cell高度
    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            //当新的高度值赋值时，与旧的相同，直接返回，不执行之后的代码
            if (elevation == value)
            {
                return;
            }

            elevation = value;

            //在获取高度等级的时候，同时为cell的Mesh赋值相应的高度值
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;

            //这里对cell整体的海拔高度进行扰动，并乘以强度系数
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;

            transform.localPosition = position;

            //设置cell对应UI的高度
            Vector3 uiPosition = uiRect.localPosition;
            //uiPosition.z = elevation * (-HexMetrics.elevationStep);

            //同样，UI也是扰动后的高度
            uiPosition.z = -position.y;

            uiRect.localPosition = uiPosition;

            //当前有河流从cell流出，如果流出cell高度小于相邻流入cell高度，这就是河流的逆流，移除流出河流
            if (hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation)
            {
                RemoveOutgoingRiver();
            }
            //与流出判断相类似，当流入cell高度大于流出cell高度时，也是逆流的情况
            if (hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation)
            {
                RemoveIncomingRiver();
            }

            //设置高度后刷新当前chunk
            Refresh();
        }
    }

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

    /// <summary>
    /// 获取指定方位相邻cell 的连接类型(HexEdgeType)
    /// </summary>
    /// <param name="direction">指定的相邻方位</param>
    /// <returns></returns>
    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    /// <summary>
    /// 对比自身和另一个cell的高度，返回两个cell的连接类型
    /// </summary>
    /// <param name="otherCell">另一个cell的实例</param>
    /// <returns>两个cell的连接类型</returns>
    public HexEdgeType GetEdgeType(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
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

    /// <summary>
    /// 当自身状态改变时，刷新自身所在chunk的所有cell
    /// </summary>
    private void Refresh()
    {
        if (chunk != null)
        {
            chunk.Refresh();

            //遍历自身当前所有相邻的cell
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];

                //当自身与相邻cell不在同一个chunk时，刷新相邻的chunk
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    //cell的河流部分--------------------------------------------------

    //用来记录当前cell是否有河流 流入/流出
    //只流出是河流起点，只流入是河流终点
    private bool hasIncomingRiver;
    private bool hasOutgoingRiver;

    //记录河流的流入方位和流出方位
    private HexDirection incomingRiver;
    private HexDirection outgoingRiver;

    //cell中是否有河流 流入/流出 以及 流入、流出方位，都使用属性来获取
    //赋值部分会在其他的方法中进行

    //获取cell是否有河流的流入
    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
    }

    //获取cell是否有河流的流出
    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
    }

    //获取cell中河流的流入方向
    public HexDirection IncomingRiver
    {
        get
        {
            return incomingRiver;
        }
    }

    //获取cell中河流的流出方向
    public HexDirection OutgoingRiver
    {
        get
        {
            return outgoingRiver;
        }
    }

    //获取当前cell中是否有河流
    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

    //获取当前的cell是否为河流的起点或终点
    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }

    /// <summary>
    /// 检查cell中指定方位的边是否有河流经过，不论是流入还是流出
    /// </summary>
    /// <param name="direction">当前cell指定方位的边</param>
    /// <returns>true该边有河流经过，false该边没有河流经过</returns>
    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return
            //检查指定方位是否有河流的流入
            hasIncomingRiver && incomingRiver == direction
            ||
            //检查指定方位是否有河流的流出
            hasOutgoingRiver && outgoingRiver == direction;
    }

    /// <summary>
    /// 删除cell自身流出的河流
    /// </summary>
    public void RemoveOutgoingRiver()
    {
        //判断当前cell是否有流出的河流
        //没有河流留出的话就直接跳出
        if (hasOutgoingRiver == false)
        {
            return;
        }

        //有流出的河流，就将cell流出状态改为false，并刷新当前chunk
        hasOutgoingRiver = false;
        //Refresh();
        //与改变高度和颜色不同，移除河流只会影响自身，所以只需要刷新自身mesh即可
        //与其相邻的cell同理
        RefreshSelfOnly();

        //移除当前cell河流的流出部分，那就要同时移除相邻cell河流的流入部分
        HexCell neighbor = GetNeighbor(outgoingRiver);
        //将相邻cell流入的标记设置为false
        neighbor.hasIncomingRiver = false;
        //刷新相邻cell所在的chunk
        //neighbor.Refresh();
        neighbor.RefreshSelfOnly();
    }

    /// <summary>
    /// 移除cell自身流入的河流
    /// </summary>
    public void RemoveIncomingRiver()
    {
        //思路与移除流出河流基本相同

        if (hasIncomingRiver == false)
        {
            return;
        }
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    /// <summary>
    /// 刷新cell自身的mesh
    /// </summary>
    private void RefreshSelfOnly()
    {
        //当前并未完善河流编辑功能，目前先使用刷新chunk的方法
        chunk.Refresh();
    }

    /// <summary>
    /// 移除cell自身流入和流出的河流
    /// </summary>
    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    /// <summary>
    /// 创建一条流出当前cell的河流
    /// </summary>
    /// <param name="direction">流出河流的方位</param>
    public void SetOutgoingRiver(HexDirection direction)
    {
        //如果当前cell内有流出的河流，并且流出的方向与入参给出的方向相同，直接跳出
        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }

        //与移除河流思路相似，同时为相邻的cell添加流入的河流
        HexCell neighbor = GetNeighbor(direction);
        //这里注意，如果没有相邻的cell，或者相邻的cell高于当前cell，都不可以创建河流
        //河流是不会从低处流向高处的
        if (!neighbor || elevation < neighbor.elevation)
        {
            return;
        }

        //移除当前cell中，与入参方向不同的那个流出的河流
        RemoveOutgoingRiver();
        //如果入参河流的方向，与流入当前cell河流的方向相同，那么还要移除流入当前cell 的河流
        if (hasIncomingRiver && incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }

        //设置流出标记和方向，并刷新当前cell
        hasOutgoingRiver = true;
        outgoingRiver = direction;
        RefreshSelfOnly();

        //移除相邻cell中旧的流入河流，设置流入标记和新的方位，并且刷新相邻的cell
        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }
}