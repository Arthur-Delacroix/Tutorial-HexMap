using UnityEngine;
using System.Collections.Generic;

//依赖MeshFilter和MeshRenderer组件
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class HexMesh : MonoBehaviour
{
    //存储通过vertices计算生成后的mesh
    private Mesh hexMesh;

    //存储所有正六边形的顶点位置信息
    private List<Vector3> vertices;

    //索引，每个三角面片顶点的渲染顺序
    private List<int> triangles;

    //为了检测射线碰撞Collider
    private MeshCollider meshCollider;

    //存储cell每个顶点的颜色信息
    private List<Color> colors;

    private void Awake()
    {
        //初始化MeshFilter组件的，实例化hexMesh，并给其命名
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex Mesh";

        //为HexMesh物体添加MeshCollider组件
        meshCollider = gameObject.AddComponent<MeshCollider>();

        //初始化vertices、triangles链表 用于存储顶点和面片信息
        vertices = new List<Vector3>();
        triangles = new List<int>();

        //初始化colors链表，用于存储顶点颜色信息
        colors = new List<Color>();
    }

    /// <summary>
    /// 根据数组长度创建cell的Mesh
    /// </summary>
    /// <param name="cells">存储所有Hex Cell实例的数组</param>
    public void Triangulate(HexCell[] cells)
    {
        //清空原有的数据
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();

        //依次读取数组中的Hex Cell实例，录入每个Hex Cell的顶点信息
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        //将所有的顶点位置信息，顶点位置信息的索引存储到链表中
        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();

        //将所有顶点的颜色信息存储在colors链表中
        hexMesh.colors = colors.ToArray();

        //重新计算法线方向，使得三角面片可以正确的显示出来
        hexMesh.RecalculateNormals();

        meshCollider.sharedMesh = hexMesh;
    }

    /// <summary>
    /// 使用HexDirection方位，为单个cell循环添加其6个顶点信息
    /// 此方法之后会进行优化合并
    /// </summary>
    /// <param name="cell">单个cell的实例</param>
    private void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    /// <summary>
    /// 通过单个Hex Cell实例，计算其6个顶点位置，并创建三角形面片
    /// </summary>
    /// <param name="cell">单个Hex Cell的实例</param>
    //private void Triangulate(HexCell cell)
    private void Triangulate(HexDirection direction, HexCell cell)
    {
        //获取单个cell的中点位置
        Vector3 center = cell.transform.localPosition;

        //根据中点位置计算出其余两个顶点的信息
        AddTriangle(
            center,
            //center + HexMetrics.GetFirstCorner(direction),
            //center + HexMetrics.GetSecondCorner(direction)

            //因为将颜色混合区域、cell自身颜色区域分开了，这里首先构建cell自身颜色区域的三角面片
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );

        //因为有了HexDirection，这里不再直接使用corners枚举来获取cell的顶点位置信息，而使用HexDirection方位来获取
        //根据中点位置计算出其余的顶点位置信息，并按照顺序构建三角面片
        //for (int i = 0; i < 6; i++)
        //{
        //    //构建三角面片
        //    AddTriangle(
        //        center,
        //        center + HexMetrics.corners[i],
        //        center + HexMetrics.corners[i + 1]
        //    );
        //}

        //获取当前相邻方位cell，索引值-1相邻cell的实例
        HexCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;

        //获取与自身当前相邻的cell的颜色值
        //每个cell会在 Triangulate(HexCell cell) 方法中将与自身相邻的cell遍历一次
        //?? 为 可空合并运算符，即cell.GetNeighbor(direction)的值为null时，使用 cell的值
        HexCell neighbor = cell.GetNeighbor(direction) ?? cell;

        //获取当前相邻方位cell，索引值+1相邻cell的实例
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;

        //将 ?? 替换为了 if/else 判断
        //HexCell neighbor = null;
        //if (cell.GetNeighbor(direction) != null)
        //{
        //    neighbor = cell.GetNeighbor(direction);
        //}
        //else
        //{
        //    neighbor = cell;
        //}

        //两个相邻的cell，其交界处的颜色应该是两个cell颜色的平均值
        //Color edgeColor = (cell.color + neighbor.color) * 0.5f;

        //为三角面片的顶点赋颜色值
        //AddTriangleColor(cell.color, neighbor.color, neighbor.color);
        //AddTriangleColor(cell.color, edgeColor, edgeColor);

        //获取到相邻方位cell，以及相邻方位cell +1和-1 cell的实例，接下来进行颜色混合
        //三角面片3个顶点颜色分别为
        //自身颜色
        //自身颜色，自身颜色+相邻cell方位减1颜色，相邻cell颜色
        //自身颜色，自身颜色+相邻cell方位加1颜色，相邻cell颜色
        AddTriangleColor(
            cell.color,
            (cell.color+prevNeighbor.color+neighbor.color)/3.0f,
            (cell.color+nextNeighbor.color+neighbor.color)/3.0f
            );
    }

    /// <summary>
    /// 为每个三角面片的3个顶点赋颜色值
    /// </summary>
    /// <param name="color">三角面片顶点的颜色信息</param>
    //private void AddTriangleColor(Color color)
    //{
    //    colors.Add(color);
    //    colors.Add(color);
    //    colors.Add(color);
    //}

    /// <summary>
    /// 为每个三角面片的3个顶点分别赋予不同的颜色值
    /// </summary>
    /// <param name="c1">第一个顶点的颜色信息(中心点的颜色)</param>
    /// <param name="c2">第二个顶点的颜色信息</param>
    /// <param name="c3">第三个顶点的颜色信息</param>
    private void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    /// <summary>
    /// 添加单个三角面片的顶点位置信息和索引
    /// </summary>
    /// <param name="v1">顺时针 第一个顶点的Vector3</param>
    /// <param name="v2">顺时针 第二个顶点的Vector3</param>
    /// <param name="v3">顺时针 第三个顶点的Vector3</param>
    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        //获取当前vertices链表中已经录入的数量
        int vertexIndex = vertices.Count;

        //在vertices链表中添加新增的顶点位置信息
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        //在triangles链表中添加新增顶点信息的索引
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }
}