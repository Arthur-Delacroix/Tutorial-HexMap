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

    //private void Start()
    //{
    //    cell实例会由HexGrid创建
    //    之后会将实例分配到各个HexGridChunk的数组中，这样再进行mesh的构建
    //    hexMesh.Triangulate(cells);
    //}

    /// <summary>
    /// 将cell实例添加到自身的数组中
    /// </summary>
    /// <param name="index">cell在自身数组的下标</param>
    /// <param name="cell">cell的实例</param>
    public void AddCell(int index, HexCell cell)
    {
        //通过下标将cell实例添加到数组中
        cells[index] = cell;

        //将chunk自身实例添加到cell中，这样cell就知道自己属于哪个chunk了
        cell.chunk = this;

        //设置cell和cell UI的父节点
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
    }

    /// <summary>
    /// 重新构建当前chunk内的所有cell
    /// </summary>
    public void Refresh()
    {
        //hexMesh.Triangulate(cells);
        
        //需要对当前Chunk刷新时，就启用当前脚本
        enabled = true;
    }

    private void LateUpdate()
    {
        //完成三角构建后，就停用当前脚本，这样就不会发生重复刷新的问题了
        hexMesh.Triangulate(cells);
        enabled = false;
    }
}