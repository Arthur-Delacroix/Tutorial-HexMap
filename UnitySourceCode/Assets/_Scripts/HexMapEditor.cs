using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    //备选颜色数组
    public Color[] colors;

    //HexGrid实例，用来调用其中的ColorCell方法
    public HexGrid hexGrid;

    //已选中的颜色
    private Color activeColor;

    //已选中的高度
    private int activeElevation;

    //这里使用指定的camera代替 Camera.main方式，避免遍历场景中的Object
    [SerializeField] private Camera mainCamera = null;

    //表示当前是否可以修改cell的颜色
    private bool applyColor;

    //是否启用修改cell高度的功能
    private bool applyElevation = true;

    //笔刷的尺寸
    private int brushSize;

    private void Awake()
    {
        //为activeColor赋初始值
        SelectColor(0);
    }

    private void Update()
    {
        //通过IsPointerOverGameObject区分点击在UI或cell上
        //IsPointerOverGameObject点击在UI上时候为true。其他为false
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
    }

    /// <summary>
    /// 鼠标左键单击会调用此方法，以鼠标为发射点，经过主摄像机练成射线
    /// 检测射线穿过Collider的位置
    /// 此方法移动到了HexMapEditor中
    /// </summary>
    private void HandleInput()
    {
        //射线起点为鼠标位置，经过主摄像机
        Ray _inputRay = mainCamera.ScreenPointToRay(Input.mousePosition);

        //检测射线是否碰撞到了collider
        RaycastHit _hit;
        if (Physics.Raycast(_inputRay, out _hit))
        {
            //hexGrid.ColorCell(_hit.point, activeColor);

            //修改单个cell
            //EditCell(hexGrid.GetCell(_hit.point));

            //带笔刷 修改多个cell
            EditCells(hexGrid.GetCell(_hit.point));
        }
    }

    /// <summary>
    /// 通过被点击的cell和笔刷半径，遍历获取笔刷半径内所有的cell，并对其进行对应的改变
    /// 参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/5-12-3.png
    /// </summary>
    /// <param name="center">笔刷中心cell的实例</param>
    private void EditCells(HexCell center)
    {
        //获取笔刷中心的cell坐标
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        //这里先进行Z(横行)的遍历，而且只遍历一半
        //即从最底部一行开始，一直到中心cell所在的那行
        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            //遍历每一行中在笔刷范围内cell的X坐标
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                //通过计算后得出的坐标，获取到对应cell的实例，并对其进行高度或颜色的改变
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        //这里是通过循环补足上半部分地图单元的代码
        //出了避免重复排除了中间行外，其余的逻辑是完全对称的
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    /// <summary>
    /// 为被点击cell赋值颜色和高度，并刷新整个地图
    /// </summary>
    /// <param name="cell">被点击cell的实例</param>
    private void EditCell(HexCell cell)
    {
        //cell.Color = activeColor;

        //避免空引用异常，首先检查被编辑的cell是否存在
        if (cell)
        {
            //当applyColor为true，也就是索引值大于等于0，才会修改cell的颜色
            if (applyColor)
            {
                cell.Color = activeColor;
            }

            //cell.Elevation = activeElevation;
            //hexGrid.Refresh();

            //当toggle勾选时，才会修改cell的高度
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
        }
    }

    /// <summary>
    /// 为选中颜色activeColor 赋值
    /// </summary>
    /// <param name="_index">备选颜色数组colors 中的颜色值索引</param>
    public void SelectColor(int _index)
    {
        //activeColor = colors[_index];

        //当传入的索引值大于0的时候，才会获取当前选中的颜色
        applyColor = _index >= 0;
        if (applyColor)
        {
            activeColor = colors[_index];
        }
    }

    /// <summary>
    /// 获取UI选定的高度
    /// </summary>
    /// <param name="elevation">从UI的Slider接收的值</param>
    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    /// <summary>
    /// 当高度修改的toggle点击时，调用此方法
    /// </summary>
    /// <param name="toggle">toggle当前是否被选中</param>
    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    /// <summary>
    /// 通过UI组件修改当前笔刷的尺寸
    /// </summary>
    /// <param name="size">Slider中设置的笔刷尺寸值</param>
    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    /// <summary>
    /// 通过UI组件修改坐标UI的显示/隐藏
    /// </summary>
    /// <param name="visible">UI坐标显示的状态</param>
    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }
}