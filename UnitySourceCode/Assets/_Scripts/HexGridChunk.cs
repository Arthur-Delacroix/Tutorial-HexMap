using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

public class HexGridChunk : MonoBehaviour
{
    //用来存储属于自己范围内的cell实例
    private HexCell[] cells;

    //对其子物体中HexMesh 和 Canvas组件实例的引用
    //在Unity中，直接将其引用拖入变量对应的栏位即可
    [SerializeField] private HexMesh hexMesh;
    [SerializeField] private Canvas gridCanvas;

    private void Awake()
    {
        //设置其存储cell数组的长宽
        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
    }

    private void Start()
    {
        //cell实例会由HexGrid创建
        //之后会将实例分配到各个HexGridChunk的数组中，这样再进行mesh的构建
        hexMesh.Triangulate(cells);
    }
}