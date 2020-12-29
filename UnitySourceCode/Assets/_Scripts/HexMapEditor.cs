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

    //这里使用指定的camera代替 Camera.main方式，避免遍历场景中的Object
    [SerializeField] private Camera mainCamera;

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
            hexGrid.ColorCell(_hit.point, activeColor);
        }
    }

    /// <summary>
    /// 为选中颜色activeColor 赋值
    /// </summary>
    /// <param name="_index">备选颜色数组colors 中的颜色值索引</param>
    public void SelectColor(int _index)
    {
        activeColor = colors[_index];
    }
}