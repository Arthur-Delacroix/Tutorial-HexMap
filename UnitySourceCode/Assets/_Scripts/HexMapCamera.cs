using UnityEngine;

#pragma warning disable 649

public class HexMapCamera : MonoBehaviour
{
    //控制camera视角旋转
    [SerializeField] private Transform swivel;

    //控制camera视角远近
    [SerializeField] private Transform stick;

    //camera视距控制，0最远，1最近
    private float zoom = 1f;

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
    }

    /// <summary>
    /// 控制camera视距远近
    /// </summary>
    /// <param name="delta">鼠标滚轮输入的值</param>
    private void AdjustZoom(float delta)
    {
        //将变化值限制在0到1之间
        //zoom = Mathf.Clamp01(zoom + delta);
        zoom = Mathf.Clamp01(zoom + delta/zoomSensitivity);

        //zoom = zoom / zoomSensitivity;

        //视距远近其实就是stick.localPosition.z的值
        //设置最大值和最小值后，根据zoom取其插值即可
        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        //在视距远近变化的时候，俯仰角也会变化
        //视距最小时是斜向45度，视距最大是垂直90度
        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
}