using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

public class HexGrid : MonoBehaviour
{

    //表示每一行有多少个地图单元
    //public int height = 6;
    //表示每一列有多少个地图单元
    //public int width = 6;

    //这里使用新的变量来初始化cells的尺寸
    //这两个变量的值可以通过地图中有几个chunk和每个chunk的尺寸计算出来
    private int cellCountX = 6;
    private int cellCountZ = 6;

    //存放地图单元格的预置
    public HexCell cellPrefab;

    //存放所有实例化的地图单元
    private HexCell[] cells;

    //存放显示地图单元坐标的Text Prefab
    [SerializeField] private Text cellLabelPrefab;

    //Text Prefab的父级Canvas
    private Canvas gridCanvas;

    //存储Hex Mesh物体上的hexMesh脚本组件
    private HexMesh hexMesh;

    //cell的默认颜色
    public Color defaultColor = Color.white;
    //cell被点击后的颜色
    public Color touchedColor = Color.magenta;

    //彩色噪点图的实例，直接将图片拖拽至Inspector面板对应位置赋初始值
    public Texture2D noiseSource;

    //定义一个chunk是有多少个cell组成的
    public int chunkCountX = 4;
    public int chunkCountZ = 3;

    private void Awake()
    {
        //为HexMetrics的静态变量赋值
        //由于此脚本最先被调用，所以在这里赋初始值
        HexMetrics.noiseSource = noiseSource;

        //获取Hex Mesh物体上的hexMesh脚本组件实例
        hexMesh = GetComponentInChildren<HexMesh>();

        //获取Hex Grid子物体下d Canvas组件
        gridCanvas = GetComponentInChildren<Canvas>();

        //根据长度和宽度，初始化数组大小
        //cells = new HexCell[cellCountZ * cellCountX];

        //从左下角开始，依次往右，每一行为 width 个单元后，上移一行
        //for (int z = 0, i = 0; z < cellCountZ; z++)
        //{
        //    for (int x = 0; x < cellCountX; x++)
        //    {
        //        CreateCell(x, z, i++);
        //    }
        //}

        //计算出整个地图横向和纵向cell的个数，也就是二维数组的长和宽
        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        CreateCells();
    }

    /// <summary>
    /// 初始化存储cell实例的数组
    /// </summary>
    private void CreateCells()
    {
        //通过计算出来的长和宽，对数组进行初始化
        cells = new HexCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    private void OnEnable()
    {
        //Unity在Play模式中，Awake只会在脚本被实例化时调用一次，如果之后噪点图改变了，没办法重新为静态变量赋值
        //所以这里再次进行赋值，之后只要disable后在enable，静态变量就会被重新赋值
        HexMetrics.noiseSource = noiseSource;
    }

    private void Start()
    {
        //调用绘制mesh的方法
        hexMesh.Triangulate(cells);
    }

    private void Update()
    {
        //之后鼠标点击交互相关代码会移动到其他脚本中
        //检测鼠标左键是否点击
        //此方法移动到了HexMapEditor中
        //if (Input.GetMouseButtonUp(0))
        //{
        //    HandleInput();
        //}
    }

    /// <summary>
    /// 鼠标左键单击会调用此方法，以鼠标为发射点，经过主摄像机练成射线
    /// 检测射线穿过Collider的位置
    /// 此方法移动到了HexMapEditor中
    /// </summary>
    //private void HandleInput()
    //{
    //    //射线起点为鼠标位置，经过主摄像机
    //    Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

    //    //检测射线是否碰撞到了collider
    //    RaycastHit hit;
    //    if (Physics.Raycast(inputRay, out hit))
    //    {
    //        TouchCell(hit.point);
    //    }
    //}

    /// <summary>
    /// 将射线的触碰点转换到自身的坐标系中
    /// </summary>
    /// <param name="position">触碰到的collider的位置</param>
    private void TouchCell(Vector3 position)
    {
        //将触碰点的坐标系，转换到自身的坐标系
        position = transform.InverseTransformPoint(position);

        //string strtmp= "原始坐标为" + position.ToString();
        //LoggerTool.LogMessage(strtmp);

        //调用转换坐标的方法，定位具体点击到哪个cell上了
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        //Debug.Log(coordinates.ToString());

        //计算出cell位于cells[]数组中的位置
        //在四边形网格中就是X+Z乘以宽度，但在这里还需要加上一半的Z轴偏移。????
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;

        //Debug.Log(index);

        //获取这个cell的实例
        HexCell cell = cells[index];

        //为这个cell赋值颜色
        cell.color = touchedColor;

        //重新构建整个map的mesh
        hexMesh.Triangulate(cells);

        //Debug.Log("touched at " + coordinates.ToString());

        //Debug.Log("touched at " + position);
        //Debug.Log("<color=#00FF00>原始坐标为" + position + "</color>");
    }

    /// <summary>
    /// 为被点击的cell赋值对应的颜色
    /// </summary>
    /// <param name="_position">鼠标点击hexmap的位置</param>
    /// <param name="_color">选中的颜色</param>
    //public void ColorCell(Vector3 _position, Color _color)
    //{
    //    //将鼠标点击的位置，转换到Hexmap的位置上，为unity坐标
    //    _position = transform.InverseTransformPoint(_position);
    //    //将Unity坐标转换为Hexmap坐标
    //    HexCoordinates _coordinates = HexCoordinates.FromPosition(_position);
    //    //通过计算的Hexmap坐计算出cell的索引
    //    int _index = _coordinates.X + _coordinates.Z * width + _coordinates.Z / 2;
    //    //通过索引在cells数组中找到这个cell的实例
    //    HexCell _cell = cells[_index];
    //    //为这个cell的实例赋值颜色
    //    _cell.color = _color;
    //    //重新构建所有的cell
    //    //这里注意，每次进行颜色的改变，都会重新构建整个cells数组，这个遗留问题之后会修正
    //    hexMesh.Triangulate(cells);
    //}


    /// <summary>
    /// 获取背点击cell的实例
    /// </summary>
    /// <param name="_position">鼠标点击hexmap的位置</param>
    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;

        //返回被点击cell的实例
        return cells[index];
    }

    /// <summary>
    /// 重新构建整个地图
    /// </summary>
    public void Refresh()
    {
        hexMesh.Triangulate(cells);
    }

    /// <summary>
    /// 创建一个地图单元
    /// </summary>
    /// <param name="x">地图单元是 横行中的第几个</param>
    /// <param name="z">地图单元是 纵列中的第几个</param>
    /// <param name="i">地图单元在cells数组中的索引</param>
    private void CreateCell(int x, int z, int i)
    {
        //声明一个Vector3，根据这个Cell在数组中的位置，计算其在游戏场景中的实际位置
        Vector3 position;

        //position.x = x * 10f;//正方形Cell时，两个cell的水平间距
        //position.x = x * (HexMetrics.innerRadius * 2f);//两个正六边形Cell中点的水平间距
        //增加了Offset，每一行偏移量为行数*内切圆半径
        //position.x = x * (HexMetrics.innerRadius * 2f) + z * (HexMetrics.innerRadius * 2f) * 0.5f;
        //由上一个等式提取公因式得出：
        //position.x = (x + z * 0.5f) * (HexMetrics.innerRadius * 2f);
        //上一步中，生成的Cell会排列成菱形
        //要排列成正方形，需要在偶数行去掉偏移量
        //这里注意，Z/2只是取商，舍掉余数
        //所以在偶数行正好抵消了偏移量，而在奇数行，z * 0.5f - z / 2 * (HexMetrics.innerRadius * 2f)正好是一个内切圆半径长度
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);

        position.y = 0f;

        //position.z = z * 10f;////正方形Cell时，两个cell的垂直间距
        position.z = z * (HexMetrics.outerRadius * 1.5f);//两个正六边形Cell中点的垂直间距

        //在数组cells的i位置实例化地图单元
        //cell用来给这个被实例化的单元设置父级和位置
        HexCell cell;
        cells[i] = Instantiate<HexCell>(cellPrefab);
        cell = cells[i];

        //设置被实例化地图单元的父级和位置
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;

        //在不改变cell排列的情况下，重新计算每个cell的坐标位置
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        //为每个cell赋颜色初始值
        cell.color = defaultColor;

        //以下为将 周围cell与自身相链接的代码部分----------------------------------------
        //判断cell是否为每一行第一个
        //如果不是第一个，则cell会有W方位相邻的cell，就可以建立E-W链接
        if (x > 0)
        {
            //cells[i - 1]即为其左侧的cell
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }

        //因为偶数行和奇数行的链接关系不同，所以要分开进行判断
        //注意，这里行数索引是从0开始，也就是说，实际看到的第一行索引是0，也就是说起始是偶数行
        //在使用SetNeighbor方法进行cell的链接时，自身和对应cell会相互建立连接
        //所以，这里选择除了第一行，其他行都只进行SE和SW方向的链接，再加上之前的W方向，其实就完成了所有6个方向的相互链接
        //图片参考 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/2-2-3.png
        //这里还有一点，要注意cell的“索引”和“坐标”！这两个数值是计算链接的关键数值！
        if (z > 0)
        {
            //这里的&为位运算符 MSDN：https://docs.microsoft.com/zh-cn/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators
            //这里使用位运算符，判断是否为偶数行
            if ((z & 1) == 0)
            {
                //当为偶数行的时候，创建 SE-NW 方向的链接
                //cells[i - width]为SE方向的实例，也就是右下方的cell
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);

                //每行的第一个cell是没有左下角(SW)方向的链接，这里要判断cell是否为第一个
                if (x > 0)
                {
                    //cells[i - width - 1]为SW方向的实例，也就是左下方的cell，创建SW-NE方向的链接
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            //这里是奇数行建立链接的部分
            else
            {
                //i - width 为自身SW方向的实例
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);

                //判断奇数行cell是否为每行最后一个，因为奇数行最后一个cell是没有SE方向的实例
                if (x < cellCountX - 1)
                {
                    //i - width + 1 为奇数行自身SE方向的实例
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        //该变量用来存储被实例化的cellLabelPrefab预置
        Text label = Instantiate<Text>(cellLabelPrefab);

        //设置该label的父级，也就是canvas
        label.rectTransform.SetParent(gridCanvas.transform, false);

        //设置label的位置，与被实例化的cell位置相同
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);

        //设置label的文字，就是cell在数组中的位置
        //label.text = x.ToString() + "\n" + z.ToString();

        //将转换后的坐标值复制给UGUI的Text组件，将它显示出来
        label.text = cell.coordinates.ToStringOnSeparateLines();

        //获取cell对应UI的rectTransform组件实例
        cell.uiRect = label.rectTransform;

        //在地图初始状态下，每个cell的海拔高度都经过扰动
        cell.Elevation = 0;
    }
}