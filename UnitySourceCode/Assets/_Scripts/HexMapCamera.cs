using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
    //控制camera视角旋转
    [SerializeField] private Transform swivel;

    //控制camera视角远近
    [SerializeField] private Transform stick;

    void Awake()
    {
        //获取对应的实例
        //swivel = transform.GetChild(0);
        //stick = swivel.GetChild(0);
    }
}