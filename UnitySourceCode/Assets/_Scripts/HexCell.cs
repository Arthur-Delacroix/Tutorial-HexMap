using UnityEngine;

public class HexCell : MonoBehaviour
{
    //在实例化每个cell的时候会调用该实例
    //针对每个cell，重新计算它的坐标值
    public HexCoordinates coordinates;

    //存储cell自身的颜色
    public Color color;

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