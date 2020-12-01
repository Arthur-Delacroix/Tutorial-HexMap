using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

public class HexGrid : MonoBehaviour
{
    //表示每一行有多少个地图单元
    public int width = 6;

    //表示每一列有多少个地图单元
    public int height = 6;

    //存放地图单元格的预置
    public HexCell cellPrefab;

    //存放实例化的地图单元
    private HexCell[] cells;

    //存放显示地图单元坐标的Text Prefab
    [SerializeField] private Text cellLabelPrefab;

    //Text Prefab的父级Canvas
    private Canvas gridCanvas;

    //存储Hex Mesh物体上的hexMesh脚本组件
    private HexMesh hexMesh;

    private void Awake()
    {
        //获取Hex Mesh物体上的hexMesh脚本组件实例
        hexMesh = GetComponentInChildren<HexMesh>();

        //获取Hex Grid子物体下d Canvas组件
        gridCanvas = GetComponentInChildren<Canvas>();

        //根据长度和宽度，初始化数组大小
        cells = new HexCell[height * width];

        //从左下角开始，依次往右，每一行为 width 个单元后，上移一行
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
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
        if (Input.GetMouseButtonUp(0))
        {
            HandleInput();
        }
    }

    /// <summary>
    /// 鼠标左键单击会调用此方法，以鼠标为发射点，经过主摄像机练成射线
    /// 检测射线穿过Collider的位置
    /// </summary>
    private void HandleInput()
    {
        //射线起点为鼠标位置，经过主摄像机
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        //检测射线是否碰撞到了collider
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            TouchCell(hit.point);
        }
    }

    /// <summary>
    /// 将射线的触碰点转换到自身的坐标系中
    /// </summary>
    /// <param name="position">触碰到的collider的位置</param>
    private void TouchCell(Vector3 position)
    {
        //将触碰点的坐标系，转换到自身的坐标系
        position = transform.InverseTransformPoint(position);

        string strtmp= "原始坐标为" + position.ToString();
        LoggerTool.LogMessage(strtmp);

        //调用转换坐标的方法，定位具体点击到哪个cell上了
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);


        //Debug.Log("touched at " + coordinates.ToString());

        //Debug.Log("touched at " + position);
        //Debug.Log("<color=#00FF00>原始坐标为" + position + "</color>");

    }

    /// <summary>
    /// 创建一个地图单元
    /// </summary>
    /// <param name="x">地图单元是 横行中的第几个</param>
    /// <param name="z">地图单元是 纵列中的第几个</param>
    /// <param name="i">地图单元在</param>
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
    }
}