using UnityEngine;

#pragma warning disable 649

public class HexMapCamera : MonoBehaviour
{
    //为了控制camera的移动范围，要获取地图的实例
    [SerializeField] private HexGrid grid;

    //控制camera视角旋转
    [SerializeField] private Transform swivel;

    //控制camera视角远近
    [SerializeField] private Transform stick;

    //camera视距控制，0最远，1最近
    private float zoom = 1f;

    //摄像机移动速度
    //[SerializeField] private float moveSpeed;

    //camera的两个移动值，分别对应视距最远和最近
    [SerializeField] private float moveSpeedMinZoom;
    [SerializeField] private float moveSpeedMaxZoom;

    //camera的最远和最近视距
    public float stickMinZoom;
    public float stickMaxZoom;

    //camera的俯角和仰角值
    public float swivelMinZoom;
    public float swivelMaxZoom;

    //控制鼠标滚轮灵敏度，数值越大，速度越慢
    [SerializeField] private float zoomSensitivity = 4;

    private void Awake()
    {
        //获取对应的实例
        //swivel = transform.GetChild(0);
        //stick = swivel.GetChild(0);
    }

    private void Update()
    {
        //检测鼠标滚轮是否有输入，有输入的话就调用 AdjustZoom
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        //检测 水平 和 垂直 方向的输入
        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }

    /// <summary>
    /// 控制camera视距远近
    /// </summary>
    /// <param name="delta">鼠标滚轮输入的值</param>
    private void AdjustZoom(float delta)
    {
        //将变化值限制在0到1之间
        //zoom = Mathf.Clamp01(zoom + delta);
        zoom = Mathf.Clamp01(zoom + delta / zoomSensitivity);

        //视距远近其实就是stick.localPosition.z的值
        //设置最大值和最小值后，根据zoom取其插值即可
        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        //在视距远近变化的时候，俯仰角也会变化
        //视距最小时是斜向45度，视距最大是垂直90度
        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    /// <summary>
    /// 控制摄像机移动
    /// </summary>
    /// <param name="xDelta">X轴输入增量</param>
    /// <param name="zDelta">Z轴输入增量</param>
    private void AdjustPosition(float xDelta, float zDelta)
    {
        //先获取当前位置
        Vector3 position = transform.localPosition;

        //阻尼系数，取X或Z的绝对值中最大的一个，这样避免抬起按键后还会移动，又保留了平滑感
        //这个值是慢慢减少的，所以移动距离也会是慢慢变小，最终到0。与按键的按下和抬起同步
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));

        //camera的移动方向
        Vector3 direction = new Vector3(xDelta, 0f, zDelta).normalized;

        //根据当前视距计算移动速度
        float moveSpeed = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom);

        //通过实践增量和速度，计算出位移距离
        float distance = moveSpeed * damping * Time.deltaTime;

        //当前位置加上位移增量，得出新的位移位置，此方法会受到帧率影响
        //position += new Vector3(xDelta, 0f, zDelta);

        //这里的 new Vector3是作为方向来使用的distance表示移动的距离， new Vector3表示移动的方向
        //position += new Vector3(xDelta, 0f, zDelta) * distance;
        position += direction * distance;

        //transform.localPosition = position;
        //对camera的移动范围进行限制
        transform.localPosition = ClampPosition(position);
    }

    /// <summary>
    /// 限制camera的移动范围在地图尺寸内
    /// </summary>
    /// <param name="position">当前camera的位置</param>
    /// <returns>计算是否在地图范围内后的位置</returns>
    private Vector3 ClampPosition(Vector3 position)
    {
        //获取地图的实际长度
        //float xMax = grid.chunkCountX * HexMetrics.chunkSizeX * (2f * HexMetrics.innerRadius);
        //这里为了使camera镜头中心在最右侧cell的中心，这里要减去半个cell的宽度
        float xMax = (grid.chunkCountX * HexMetrics.chunkSizeX - 0.5f) * (2f * HexMetrics.innerRadius);
        //将camera的X限制在宽度范围内
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        //获取地图的实际宽度
        //float zMax = grid.chunkCountZ * HexMetrics.chunkSizeZ * (1.5f * HexMetrics.outerRadius);
        //因为cell排列方式，这里Z方向是减去一个cell的Z
        float zMax = (grid.chunkCountZ * HexMetrics.chunkSizeZ - 1) * (1.5f * HexMetrics.outerRadius);
        //将camera的Z限制在宽度范围内
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }
}