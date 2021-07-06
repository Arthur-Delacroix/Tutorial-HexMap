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

            EditCell(hexGrid.GetCell(_hit.point));
        }
    }

    /// <summary>
    /// 为被点击cell赋值颜色和高度，并刷新整个地图
    /// </summary>
    /// <param name="cell">被点击cell的实例</param>
    private void EditCell(HexCell cell)
    {
        //cell.Color = activeColor;

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
}