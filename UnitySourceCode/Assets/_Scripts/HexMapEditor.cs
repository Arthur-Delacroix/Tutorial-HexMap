using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HexMapEditor : MonoBehaviour
{
    //备选颜色数组
    public Color[] colors;

    //HexGrid实例，用来调用其中的ColorCell方法
    public HexGrid hexGrid;

    //已选中的高度
    private int activeElevation;

    //已选中的颜色
    private Color activeColor;

    //笔刷的尺寸
    private int brushSize;

    //表示当前是否可以修改cell的颜色
    private bool applyColor;

    //是否启用修改cell高度的功能
    private bool applyElevation = true;

    //河流编辑器的状态
    private enum OptionalToggle
    {
        Ignore, Yes, No
    }

    //当前河流编辑器选中的状态
    private OptionalToggle riverMode;

    //判断当前是否处于拖拽状态
    private bool isDrag;

    //判断鼠标在当前cell的移动方位
    private HexDirection dragDirection;

    //当鼠标到下一个cell 的时候，这里记录上一个cell
    private HexCell previousCell;

    //这里使用指定的camera代替 Camera.main方式，避免遍历场景中的Object
    [SerializeField] private Camera mainCamera = null;

    //显示当前选择高度的文本
    [SerializeField] private Text elevationText = null;
    //显示当前笔刷大小的文本
    [SerializeField] private Text brushSizeText = null;


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
    /// 当高度修改的toggle点击时，调用此方法
    /// </summary>
    /// <param name="toggle">toggle当前是否被选中</param>
    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    /// <summary>
    /// 获取UI选定的高度
    /// </summary>
    /// <param name="elevation">从UI的Slider接收的值</param>
    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;

        //在改变高度的同时，更新显示的文字信息
        elevationText.text = "Elevation: " + elevation.ToString();
    }

    /// <summary>
    /// 通过UI组件修改当前笔刷的尺寸
    /// </summary>
    /// <param name="size">Slider中设置的笔刷尺寸值</param>
    public void SetBrushSize(float size)
    {
        brushSize = (int)size;

        //在设置笔刷大小的同时，更新显示的文字信息
        brushSizeText.text = "Brush Size: " + size.ToString();
    }

    /// <summary>
    /// 设置河流编辑器的状态
    /// </summary>
    /// <param name="mode">状态枚举索引值</param>
    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    /// <summary>
    /// 通过UI组件修改坐标UI的显示/隐藏
    /// </summary>
    /// <param name="visible">UI坐标显示的状态</param>
    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    private void Awake()
    {
        //为activeColor赋初始值
        SelectColor(-1);

        //设置高度和笔刷大小默认显示数值
        elevationText.text = "Elevation: 0";
        brushSizeText.text = "Brush Size: 0";
    }

    private void Update()
    {
        //通过IsPointerOverGameObject区分点击在UI或cell上
        //IsPointerOverGameObject点击在UI上时候为true。其他为false
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
        else
        {
            //当鼠标左键没有按下的时候，记录上一个经过的cell为空
            previousCell = null;
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
            //EditCells(hexGrid.GetCell(_hit.point));

            //记录当前射线碰撞到的cell
            HexCell currentCell = hexGrid.GetCell(_hit.point);

            //判断当前cell与之前的cell是否为同一个
            //如果是，就是在拖拽
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }

            //记录当前正在编辑的cell
            EditCells(currentCell);
            //目前上一个cell与当前cell是一个
            previousCell = currentCell;
        }
        else
        {
            //当鼠标的射线未触碰到地图的时候，上一个经过的cell为空
            previousCell = null;
        }
    }

    /// <summary>
    /// 确认拖拽方向
    /// </summary>
    /// <param name="currentCell">当前射线所碰撞到的cell</param>
    private void ValidateDrag(HexCell currentCell)
    {
        //循环遍历当前cell的6个方位
        for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
        {
            //如果之前cell某个方位上的cell，与当前射线触碰到的cell相同，就证明发生了碰撞
            //可以这样理解，当真实发生拖拽了，会记录下previousCell
            //鼠标移动至新的cell后，开始检测previousCell在对应方位上是不是有新的cell
            //如果有，就证明鼠标从previousCell拖拽移动到了新的cell上
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
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

            //当移除河流勾选时，删除cell上的河流
            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            //当拖拽发生，并且是创建河流勾选时，鼠标拖拽会创建河流
            else if (isDrag && riverMode == OptionalToggle.Yes)
            {
                //检测当前cell的拖拽方位上，是否有相邻cell 的实例
                //如果有实例，就创建相应的河流信息
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    otherCell.SetOutgoingRiver(dragDirection);
                }
            }
        }
    }
}