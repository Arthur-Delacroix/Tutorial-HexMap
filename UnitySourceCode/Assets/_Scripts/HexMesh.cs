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

    void Awake()
    {
        //初始化MeshFilter组件的，实例化hexMesh，并给其命名
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex Mesh";

        //初始化vertices和triangles组件
        vertices = new List<Vector3>();
        triangles = new List<int>();
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

        //依次读取数组中的Hex Cell实例，录入每个Hex Cell的顶点信息
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        //将所有的顶点位置信息，顶点位置信息的索引存储到链表中
        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();

        //重新计算法线方向，使得三角面片可以正确的显示出来
        hexMesh.RecalculateNormals();
    }

    /// <summary>
    /// 通过单个Hex Cell实例，计算其6个顶点位置，并创建三角形面片
    /// </summary>
    /// <param name="cell">单个Hex Cell的实例</param>
    private void Triangulate(HexCell cell)
    {
        //获取单个cell的中点位置
        Vector3 center = cell.transform.localPosition;

        //根据中点位置计算出其余两个顶点的信息
        //AddTriangle(
        //    center,
        //    center + HexMetrics.corners[0],
        //    center + HexMetrics.corners[1]
        //);

        //根据中点位置计算出其余的顶点位置信息
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(
                center,
                center + HexMetrics.corners[i],
                center + HexMetrics.corners[i + 1]
            );
        }
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