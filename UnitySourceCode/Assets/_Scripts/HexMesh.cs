﻿using UnityEngine;
using System.Collections.Generic;

//依赖MeshFilter和MeshRenderer组件
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class HexMesh : MonoBehaviour
{
    //存储通过vertices计算生成后的mesh
    private Mesh hexMesh;

    //存储所有正六边形的顶点位置信息
    //private List<Vector3> vertices;
    private static List<Vector3> vertices = new List<Vector3>();

    //索引，每个三角面片顶点的渲染顺序
    //private List<int> triangles;
    private static List<int> triangles = new List<int>();

    //为了检测射线碰撞Collider
    private MeshCollider meshCollider;

    //存储cell每个顶点的颜色信息
    //private List<Color> colors;
    private static List<Color> colors = new List<Color>();

    private void Awake()
    {
        //初始化MeshFilter组件的，实例化hexMesh，并给其命名
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex Mesh";

        //为HexMesh物体添加MeshCollider组件
        meshCollider = gameObject.AddComponent<MeshCollider>();

        //初始化vertices、triangles链表 用于存储顶点和面片信息
        //vertices = new List<Vector3>();
        //triangles = new List<int>();

        //初始化colors链表，用于存储顶点颜色信息
        //colors = new List<Color>();
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
        //Vector3 center = cell.transform.localPosition;

        //这里是获cell扰动后的位置
        Vector3 center = cell.Position;

        //这两个Vector3变量，是新的cell自身颜色区域中，两个新的顶点信息，其每个顶点距离cell中心为75%外接圆半径
        //Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        //Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        //通过六边形一条边上的两个端点信息，计算出细分的中间两个点的信息
        EdgeVertices e = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );

        if (cell.HasRiver)
        {
            //检测当前边缘是否有河流穿过
            if (cell.HasRiverThroughEdge(direction))
            {
                //如果有河流穿过，就降低中间顶点的高度，使其成为河床最低点
                e.v3.y = cell.StreamBedY;

                //使用带河流的构建方式
                //TriangulateWithRiver(direction, cell, center, e);

                //检测是否为河流的起点或者终点
                //如果是起点或终点，那就使用特殊的方法构建
                if (cell.HasRiverBeginOrEnd)
                {
                    TriangulateWithRiverBeginOrEnd(direction, cell, center, e);
                }
                else
                {
                    TriangulateWithRiver(direction, cell, center, e);
                }
            }
        }
        else
        {
            //在计算出各个点的位置信息后，直接构建三角面片
            //这个是不带河流的构建方式
            TriangulateEdgeFan(center, e, cell.Color);
        }

        //这两个Vector3变量，是原本构成cell一个三角面片的其中两个顶点位置。现在是颜色混合区域的两个顶点位置。
        //Vector3 v4 = center + HexMetrics.GetFirstCorner(direction);
        //Vector3 v5 = center + HexMetrics.GetSecondCorner(direction);

        //颜色混合区域变为了矩形，V3和V4的位置，其实是通过V1和V2顶点分别加上矩形区域的高来计算得出的
        //具体可以查看HexMetrics.GetBridge方法的说明
        //Vector3 bridge = HexMetrics.GetBridge(direction);
        //Vector3 v4 = v1 + bridge;
        //Vector3 v5 = v2 + bridge;
        //该段代码移至 TriangulateConnection 方法中

        //为了增加地图的整体细节，这里在cell六边形每个边的中点上，增加一个顶点
        //让原来的6个面片变为12个。
        //Vector3 e1 = Vector3.Lerp(v1, v2, 0.5f);

        //这里将正六边形的每个边均分为3份，面数是原来的3倍
        //Vector3 e1 = Vector3.Lerp(v1, v2, 1f / 3f);
        //Vector3 e2 = Vector3.Lerp(v1, v2, 2f / 3f);

        //根据中点位置计算出其余两个顶点的信息
        /*
        AddTriangle(
            center,
            //center + HexMetrics.GetFirstCorner(direction),
            //center + HexMetrics.GetSecondCorner(direction)

            //因为将颜色混合区域、cell自身颜色区域分开了，这里首先构建cell自身颜色区域的三角面片
            //center + HexMetrics.GetFirstSolidCorner(direction),
            //center + HexMetrics.GetSecondSolidCorner(direction)

            //使用声明的新变量，替换之前计算得出的结果
            v1, v2
        );
        */

        //这里添加的顶点也变为了一个边的中点
        //这3个顶点是原来六边形一个三角面片的一半
        //AddTriangle(center, v1, e1);

        //将计算好的颜色混合区域定点位置信息，添加到添加到链表中
        //AddQuad(v1, v2, v4, v5);
        //该段代码移至 TriangulateConnection 方法中

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
        //HexCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;

        //获取与自身当前相邻的cell的颜色值
        //每个cell会在 Triangulate(HexCell cell) 方法中将与自身相邻的cell遍历一次
        //?? 为 可空合并运算符，即cell.GetNeighbor(direction)的值为null时，使用 cell的值
        //HexCell neighbor = cell.GetNeighbor(direction) ?? cell;
        //该段代码移至 TriangulateConnection 方法中

        //获取当前相邻方位cell，索引值+1相邻cell的实例
        //HexCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;

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
        //AddTriangleColor(
        //    cell.color,
        //    (cell.color + prevNeighbor.color + neighbor.color) / 3.0f,
        //    (cell.color + nextNeighbor.color + neighbor.color) / 3.0f
        //    );

        //这里为cell的三角面片每个顶点赋值颜色，因为cell自身不再参与颜色混合，所以只有自身颜色
        //AddTriangleColor(cell.color);

        //这里构建原来六边形一个三角面片的另一半
        //AddTriangle(center, e1, v2);
        //AddTriangleColor(cell.color);

        //添加新增顶点的的位置信息和颜色信息
        //AddTriangle(center, e1, e2);
        //AddTriangleColor(cell.color);
        //AddTriangle(center, e2, v2);

        //AddTriangleColor(cell.color);

        //为颜色混合区域的4个顶点分别赋值颜色
        //其中v1 v2是cell自身颜色，v4 v4是混合后的颜色
        //AddQuadColor(
        //    cell.color,
        //    cell.color,
        //    (cell.color + prevNeighbor.color + neighbor.color) / 3.0f,
        //    (cell.color + nextNeighbor.color + neighbor.color) / 3.0f
        //    );

        //矩形两色混合区域的中间过渡色
        //Color bridgeColor = (cell.color + neighbor.color) * 0.5f;

        //新的矩形颜色混合区域顶点颜色赋值
        //AddQuadColor(cell.color, bridgeColor);

        //组成cell的每个三角形区域，有1个矩形混色区域和2个三角形三色混合区域
        //生成其中一个三角形三色混合区域
        //AddTriangle(v1, center + HexMetrics.GetFirstCorner(direction), v4);

        //为第一个三角形三色混合区域赋值颜色
        //自身颜色、三个相邻cell的平均色、矩形混合区域中间色
        //AddTriangleColor(
        //    cell.color,
        //    (cell.color + prevNeighbor.color + neighbor.color) / 3f,
        //    bridgeColor
        //    );

        //第二个三角形三色混合区域
        //AddTriangle(v2, v5, center + HexMetrics.GetSecondCorner(direction));

        //第二个三角形三色混合区域的颜色
        //AddTriangleColor(
        //    cell.color,
        //    bridgeColor,
        //    (cell.color + nextNeighbor.color + neighbor.color) / 3f
        //    );

        //只生成NE、E、SE这三个方位的连接
        //if (direction <= HexDirection.SE)
        //{
        //    TriangulateConnection(direction, cell, v1, v2);
        //}

        //因为对六边形的每个边进行了细分，所以要把新的顶点也传入构建矩形连接区域的方法中
        //这样矩形区域使用新增的顶点后边缘之间才能吻合
        //if (direction <= HexDirection.SE)
        //{
        //    TriangulateConnection(direction, cell, v1, e1, e2, v2);
        //}

        //TriangulateConnection方法增加新的参数，自身不在进行顶点的计算了
        if (direction <= HexDirection.SE)
        {
            TriangulateConnection(direction, cell, e);
        }
    }

    /// <summary>
    /// 为每个三角面片的3个顶点赋颜色值
    /// </summary>
    /// <param name="color">三角面片顶点的颜色信息</param>
    private void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

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
    /// 添加单个三角面片的顶点位置信息和索引，顶点带扰动
    /// </summary>
    /// <param name="v1">顺时针 第一个顶点的Vector3</param>
    /// <param name="v2">顺时针 第二个顶点的Vector3</param>
    /// <param name="v3">顺时针 第三个顶点的Vector3</param>
    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        //获取当前vertices链表中已经录入的数量
        int vertexIndex = vertices.Count;

        //在vertices链表中添加新增的顶点位置信息
        //vertices.Add(v1);
        //vertices.Add(v2);
        //vertices.Add(v4);

        //这里的坐标变为扰动后的坐标
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));

        //在triangles链表中添加新增顶点信息的索引
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    /// <summary>
    /// 添加单个三角面片的顶点位置信息和索引，顶点不扰动！
    /// </summary>
    /// <param name="v1">顺时针 第一个顶点的Vector3</param>
    /// <param name="v2">顺时针 第二个顶点的Vector3</param>
    /// <param name="v3">顺时针 第三个顶点的Vector3</param>
    private void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    /// <summary>
    /// 创建颜色混合区域的三角面片定点信息和索引，这个区域是一个四边形，所以有4个顶点
    /// 参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/2-7-1.png
    /// </summary>
    /// <param name="v1">三角面片第一个顶点位置信息</param>
    /// <param name="v2">三角面片第二个顶点位置信息</param>
    /// <param name="v3">三角面片第三个顶点位置信息</param>
    /// <param name="v4">三角面片第四个顶点位置信息</param>
    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        //获取当前vertices链表中已经录入的数量
        int vertexIndex = vertices.Count;

        //在vertices链表中添加新增的顶点位置信息
        //vertices.Add(v1);
        //vertices.Add(v2);
        //vertices.Add(v4);
        //vertices.Add(v5);

        //这里的坐标变为扰动后的坐标
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        vertices.Add(Perturb(v4));

        //在triangles链表中添加新增顶点信息的索引
        //两个三角面片组成了颜色混合区域，分别为：V1V3V2 和 V2V3V4
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    /// <summary>
    /// 为四边形颜色混合区域的每个顶点赋值颜色
    /// </summary>
    /// <param name="c1">第一个顶点的颜色信息</param>
    /// <param name="c2">第二个顶点的颜色信息</param>
    /// <param name="c3">第三个顶点的颜色信息</param>
    /// <param name="c4">第四个顶点的颜色信息</param>

    private void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }


    /// <summary>
    /// 为四边形颜色混合区域的每个顶点赋值颜色
    /// 因为该区域只负责混合2个cell的颜色，所以4个顶点只需要2个颜色
    /// </summary>
    /// <param name="c1">cell祖神颜色</param>
    /// <param name="c2">混合后的颜色</param>
    private void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    /// <summary>
    /// 使用一个颜色值构建四边形区域，河道在cell中心时候用到
    /// </summary>
    /// <param name="color">cell自身的颜色值</param>
    private void AddQuadColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }


    /// <summary>
    /// 构建阶梯状连接区域
    /// 这里不再使用单一的顶点，而是直接使用cell与阶梯区域相连接的边，通过计算得出边上的顶点位置以及每个顶点的颜色
    /// </summary>
    /// <param name="begin">第一个cell与相邻阶梯化区域的边上顶点</param>
    /// <param name="beginCell">第一个cell的实例</param>
    /// <param name="end">第二个cell与相邻阶梯化区域的边上顶点</param>
    /// <param name="endCell">第二个cell的实例</param>
    private void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell)
    {
        //通过插值计算出相邻cell边的每个坐标点
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        //通过插值计算出相邻cell边每个坐标点的颜色
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

        //构建阶梯的第一段
        TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

        //循环生成中间部分
        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
            TriangulateEdgeStrip(e1, c1, e2, c2);
        }

        //构建阶梯的最后一段
        TriangulateEdgeStrip(e2, c2, end, endCell.Color);
    }


    /// <summary>
    /// 构建cell其中一个三角面片的颜色混合区域
    /// </summary>
    /// <param name="direction">颜色混合区域的方位</param>
    /// <param name="cell">cell自身实例，用于取得cell位置和颜色 也是三角面片的第一个顶点</param>
    /// <param name="v1">自身颜色三角面片 的第二个顶点</param>
    /// <param name="v2">自身颜色三角面片 的第三个顶点</param>
    //private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)

    //六边形增加了新的顶点，这里要修改参数列表，接收新的顶点
    //private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 e1, Vector3 e2, Vector3 v2)

    //这里不再使用单个顶点，而直接使用EdgeVertices进行顶点计算
    private void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
    {
        //HexCell neighbor = cell.GetNeighbor(direction) ?? cell;

        HexCell neighbor = cell.GetNeighbor(direction);

        //当一个方位没有相邻的cell时，不生成双色混合区域
        if (neighbor == null)
        {
            return;
        }

        //参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/2-8-1.png
        //先计算出颜色混合区域的高度，在通过v1 v2计算出v3 v5，这样就知道了矩形颜色混合区域的四个顶点了
        Vector3 bridge = HexMetrics.GetBridge(direction);
        //Vector3 v4 = v1 + bridge;
        //Vector3 v5 = v2 + bridge;
        //这里为连接相邻cell的v3 v4顶点加上其所在cell的高度
        //v4.y = v5.y = neighbor.Elevation * HexMetrics.elevationStep;

        //这里在获取相邻cell的位置时，也是使用了扰动后的坐标位置
        //v4.y = v5.y = neighbor.Position.y;

        //这里要计算与矩形连接区域相邻的，另一侧cell新增的两个顶点位置信息
        //Vector3 e3 = Vector3.Lerp(v4, v5, 1f / 3f);
        //Vector3 e4 = Vector3.Lerp(v4, v5, 2f / 3f);

        //先计算出两个相邻cell的高度差
        bridge.y = neighbor.Position.y - cell.Position.y;

        //利用高度差和第一个cell的坐标，获得连接区域另外一边的4个顶点位置
        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

        //使得相邻地图单元的中间顶点坐标也下降到河床最低点位置，不然会有破面产生
        if (cell.HasRiverThroughEdge(direction))
        {
            e2.v3.y = neighbor.StreamBedY;
        }

        //进行矩形颜色混合区域的三角面片构建和赋值顶点颜色
        //AddQuad(v1, v2, v4, v5);
        //AddQuadColor(cell.color, neighbor.color);
        //以上方法注释掉，使用新的 TriangulateEdgeTerraces  进行替换
        //TriangulateEdgeTerraces(v1, v2, cell, v4, v5, neighbor);
        //在这里新加入判断，当两个相邻cell的连接类型为Slope的时候，才会创建阶梯化连接
        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            //TriangulateEdgeTerraces(v1, v2, cell, v4, v5, neighbor);

            //这里也使用EdgeVertices计算的顶点来构建矩形
            //TriangulateEdgeTerraces(e1.v1, e1.v5, cell, e2.v1, e2.v5, neighbor);

            //将新的顶点信息传入构建阶梯连接区域的方法中
            TriangulateEdgeTerraces(e1, cell, e2, neighbor);
        }
        else
        {
            //当连接类型不为Slope的时候，连接区域是矩形的
            //AddQuad(v1, v2, v4, v5);
            //AddQuadColor(cell.color, neighbor.color);

            //这里使用新增的顶点进行连接区域的构建
            //AddQuad(v1, e1, v4, e3);
            //AddQuadColor(cell.color, neighbor.color);
            //AddQuad(e1, e2, e3, e4);
            //AddQuadColor(cell.color, neighbor.color);
            //AddQuad(e2, v2, e4, v5);
            //AddQuadColor(cell.color, neighbor.color);

            //这里也使用EdgeVertices计算的顶点来构建矩形
            TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color);
        }

        //获取相邻方位的下一个方位 的cell
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

        //这里三个彼此相邻的cell都存在的时候，才会创建三角形混合区域
        //if (nextNeighbor != null)
        //为了避免三角形混合区域的重叠，这里只需要生成NE和E方位的即可
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            //声明一个新的vector3变量来存储高度改变后的顶点位置
            //v5的本质其实就是v2 + HexMetrics.GetBridge(direction.Next()加上高度值
            //Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());

            //这里也使用EdgeVertices计算的顶点来构建矩形
            Vector3 v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());

            //v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;

            //这里在获取相邻cell的位置时，也是使用了扰动后的坐标位置
            v5.y = nextNeighbor.Position.y;

            //参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-10-1.png
            //这里要注意，只是要找出3个cell中高度最低的一个
            //因为三角形连接区域的3个cell，其坐标是固定的，找出最低的一个时，其他两个cell的入参顺序就是固定的了

            //注意，教程4.1是有错误的但是最后给的代码是对的，这里注释掉的语句是教程错误的语句
            if (cell.Elevation <= neighbor.Elevation)
            {
                //并且cell1高度小于cell3
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    //cell1最低
                    //TriangulateCorner(v2, cell, v5, nextNeighbor, v5, nextNeighbor);
                    //TriangulateCorner(v2, cell, v5, neighbor, v5, nextNeighbor);

                    //这里也使用EdgeVertices计算的顶点来构建矩形
                    TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                }
                else
                {
                    //cell3 最低
                    //TriangulateCorner(v5, nextNeighbor, v2, cell, v5, nextNeighbor);
                    //TriangulateCorner(v5, nextNeighbor, v2, cell, v5, neighbor);

                    //这里也使用EdgeVertices计算的顶点来构建矩形
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                }
            }
            //如果cell1>cell2，且cell2<cell3
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
                //cell2最低
                //TriangulateCorner(v5, nextNeighbor, v5, nextNeighbor, v2, cell);
                //TriangulateCorner(v5, neighbor, v5, nextNeighbor, v2, cell);

                //这里也使用EdgeVertices计算的顶点来构建矩形
                TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
            }
            else
            {
                //cell3最低
                //TriangulateCorner(v5, nextNeighbor, v2, cell, v5, nextNeighbor);
                //TriangulateCorner(v5, nextNeighbor, v2, cell, v5, neighbor);

                //这里也使用EdgeVertices计算的顶点来构建矩形
                TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
            }

            //v2 + HexMetrics.GetBridge(direction.Next()) 为三角形的最后一个顶点位置
            //首先通过HexMetrics.GetBridge(direction.Next()获取 相邻的第二个cell的矩形连接区域宽度，可以理解为一个向量
            //v2顶点位置再加上这个向量，得出了三角形最后一个顶点的位置
            //AddTriangle(v2, v5, v2 + HexMetrics.GetBridge(direction.Next()));

            //AddTriangle(v2, v5, v5);
            //AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }
    }

    /// <summary>
    /// 阶梯化矩形连接区域
    /// </summary>
    /// <param name="beginLeft">cell到neighbor连接区域的第一个起点</param>
    /// <param name="beginRight">cell到neighbor连接区域的第二个起点</param>
    /// <param name="beginCell">cell自身实例，用于获取颜色</param>
    /// <param name="endLeft">连接区域 连接到的neighbor的第一个终点</param>
    /// <param name="endRight">连接区域 连接到的neighbor的第二个终点</param>
    /// <param name="endCell">连接到的neighbor实例，用于获取颜色</param>
    //private void TriangulateEdgeTerraces(
    //Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
    //Vector3 endLeft, Vector3 endRight, HexCell endCell)
    //{
    //    //这里先生成阶梯的第一个矩形面片。通过给定插值来计算出矩形面片的另外两个顶点
    //    Vector3 v4 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
    //    Vector3 v5 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
    //    Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

    //    AddQuad(beginLeft, beginRight, v4, v5);
    //    AddQuadColor(beginCell.Color, c2);

    //    //阶梯的其他矩形面片，可以通过循环来生成
    //    //旧的矩形面片终点V3 V4，就是新面片的起点 V1 V2
    //    //然后再利用插值计算新面片的终点即可
    //    //颜色计算同理
    //    for (int i = 2; i < HexMetrics.terraceSteps; i++)
    //    {
    //        Vector3 v1 = v4;
    //        Vector3 v2 = v5;
    //        Color c1 = c2;
    //        v4 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
    //        v5 = HexMetrics.TerraceLerp(beginRight, endRight, i);
    //        c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
    //        AddQuad(v1, v2, v4, v5);
    //        AddQuadColor(c1, c2);
    //    }

    //    //连接阶梯的剩余区域
    //    AddQuad(v4, v5, endLeft, endRight);
    //    AddQuadColor(c2, endCell.Color);
    //}

    /// <summary>
    /// 构建三角形连接区域的方法
    /// 判断相邻3个cell高低的工作，在TriangulateConnection方法中实现了，这里只负责创建连接区域
    /// 注意，TriangulateConnection方法只是对入参的顺序做了调整，但是并没有告知3个cell之间相对的连接类型
    /// 所以要在这个方法中对连接类型进行判断，这样才能决三角形连接区域定用什么方式进行三角剖分
    /// </summary>
    /// <param name="bottom">bottom cell的坐标</param>
    /// <param name="bottomCell">bottom cell的实例</param>
    /// <param name="left">left cell的坐标</param>
    /// <param name="leftCell">left cell的实例</param>
    /// <param name="right">right cell的坐标</param>
    /// <param name="rightCell">right cell的实例</param>
    private void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        //这里先获取Left和Right两个cell，相较于Bottom cell的高度类型，这样才能决定怎样做三角剖分
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        //这里通过获取的Left和Right 相较于Bottom的连接类型进行判断，具体三个cell的高度关系
        //判断完成后，直接调用对应的方法构建三角形连接区域，而不使用之前通用的方法构建
        if (leftEdgeType == HexEdgeType.Slope)
        {
            //这是SSF类型正常情况，即2个cell高度为1，一个cell高度为0
            if (rightEdgeType == HexEdgeType.Slope)
            {
                //这里判断为SSF类型
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }

            //SSF变体1 即2个cell高度为0，一个cell高度为1，且高度为1的cell在左侧
            else if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
                //Slope-Cliff连接类型
                //bottom最低，left比bottom高1，right比bottom高1及以上
                TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }

        else if (rightEdgeType == HexEdgeType.Slope)
        {
            //SSF变体2 即2个cell高度为0，一个cell高度为1，且高度为1的cell在右侧
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                //Slope-Cliff连接 镜像 类型
                //bottom最低，right比bottom高1，left比right高1及以上
                TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }

        //bottom最低，与left和right高差都大于1，并且left和right高差为1，称为 CCS类型
        //如果left比right高1，那么就是CCSL，反之right比left高1，那就是CCSR
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            //CCSR
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }
            //CCSL
            else
            {
                TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            }
        }
        else
        {
            //这里先使用旧的方法来构建三角形连接区域，也就是没有阶梯化的那种
            //经过连接类型判断后，这个方法就会被代替掉
            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
        }
    }

    /// <summary>
    /// 针对SSF组合类型 创建阶梯状的三角形连接区域
    /// SFF及其变体的组合，参考图
    /// http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-11-1.png
    /// http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-12-1.png
    /// </summary>
    /// <param name="begin">初始cell位置</param>
    /// <param name="beginCell">初始cell实例</param>
    /// <param name="left">左侧cell位置</param>
    /// <param name="leftCell">左侧cell实例</param>
    /// <param name="right">右侧cell位置</param>
    /// <param name="rightCell">右侧cell实例</param>
    private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {

        //计算出与begin相邻的两个cell，每个阶梯的顶点和其对应的颜色
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

        //与矩形阶梯区域不同的是，阶梯三角形连接区域最下端是一个三角形，这里先构建这个三角形
        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.Color, c3, c4);

        //循环获取中间部分的顶点位置和颜色信息
        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }

        //构建剩余的部分
        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
    }

    /// <summary>
    /// 针对Slope-Cliff连接类型 创建三角形连接区域
    /// 参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-13-3.png
    /// </summary>
    /// <param name="begin">初始cell位置</param>
    /// <param name="beginCell">初始cell实例</param>
    /// <param name="left">左侧cell位置</param>
    /// <param name="leftCell">左侧cell实例</param>
    /// <param name="right">右侧cell位置</param>
    /// <param name="rightCell">右侧cell实例</param>
    private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        //这里将Slope-Cliff类型的三角形连接区域拆分成两部分进行构建
        //示意图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-14-2.png
        //即三角形一个边进行阶梯化，阶梯化后的端点，都与另一条边上的一点相连
        //边上一点，是通过bottom与right高度差，在进行插值得到的
        //float b = 1f / (rightCell.Elevation - beginCell.Elevation);

        //这里注意，在进行CCSL和CCSR类型三角形连接区域构建时，上下翻转，插值就为负数
        //这里将计算结构变为正数后再进行插值计算
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }

        //Vector3 boundary = Vector3.Lerp(begin, right, b);
        //这里使用扰动后的坐标点计算分界点
        Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
        Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

        //测试通过插值找到的三角形上一点是否正确
        //AddTriangle(begin, left, boundary);
        //AddTriangleColor(beginCell.color, leftCell.color, boundaryColor);

        //构建底部三角面片
        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

        //如果left和right的高度关系为Slope，也就是高度差1
        //那么就使用构建底部构建三角面片的方法，只不过入参是上下翻转的
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        //如果left和right的高度关系为Cliff，也就是高度差大于1
        //那么直接只用一个三角面片填补这个区域即可
        else
        {
            //AddTriangle(left, right, boundary);
            //这里不再扰动分界点
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }

    /// <summary>
    /// Slope-Cliff连接类型种 创建底部区域
    /// 这里将Slope-Cliff的三角形连接区域分为了两部分
    /// 当两个cell高度差为1时，上下均要从边界点进行阶梯化
    /// 当两个cell高度差大于1时，只需要细分下半部分即可
    /// 参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-15-1.png
    /// http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-15-2.png
    /// </summary>
    /// <param name="begin">初始cell位置</param>
    /// <param name="beginCell">初始cell实例</param>
    /// <param name="left">左侧cell位置</param>
    /// <param name="leftCell">左侧cell实例</param>
    /// <param name="right">Cliff斜面的分界点位置</param>
    /// <param name="rightCell">Cliff斜面的分界点的颜色</param>
    private void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary, Color boundaryColor)
    {
        //与构建其他阶梯状区域类似，首先构建第一个三角面片
        //Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
        //由于在此方法中，并没有用v2继续计算其他顶点，所以在这里先对v2进行扰动，之后代码中直接使用
        Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

        //AddTriangle(begin, v2, boundary);
        //这里收束到边界点的时候，边界点不再进行扰动
        //AddTriangleUnperturbed(Perturb(begin), Perturb(v2), boundary);
        //使用扰动后的v2点
        AddTriangleUnperturbed(Perturb(begin), v2, boundary);
        AddTriangleColor(beginCell.Color, c2, boundaryColor);

        //循环创建中间部分的三角面片
        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            //v2 = HexMetrics.TerraceLerp(begin, left, i);
            //先对V2进行扰动
            v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            //AddTriangle(v1, v2, boundary);
            //这里收束到边界点的时候，边界点不再进行扰动
            //AddTriangleUnperturbed(Perturb(v1), Perturb(v2), boundary);
            //使用扰动后的v2点
            AddTriangleUnperturbed(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }

        //构建剩余区域
        //AddTriangle(v2, left, boundary);
        //这里收束到边界点的时候，边界点不再进行扰动
        //AddTriangleUnperturbed(Perturb(v2), Perturb(left), boundary);
        //使用扰动后的v2点
        AddTriangleUnperturbed(v2, Perturb(left), boundary);
        AddTriangleColor(c2, leftCell.Color, boundaryColor);
    }

    /// <summary>
    /// 针对Slope-Cliff连接 镜像 类型 创建三角形连接区域
    /// 参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/3-16-1.png
    /// 这里镜像类型的构建与Slope-Cliff相似，只是调整了构建三角形的时候，顶点的顺序
    /// 方法中注释掉的代码是 TriangulateCornerTerracesCliff 不同的部分
    /// </summary>
    /// <param name="begin">初始cell位置</param>
    /// <param name="beginCell">初始cell实例</param>
    /// <param name="left">左侧cell位置</param>
    /// <param name="leftCell">左侧cell实例</param>
    /// <param name="right">右侧cell位置</param>
    /// <param name="rightCell">右侧cell实例</param>
    private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        //float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        //Vector3 boundary = Vector3.Lerp(begin, right, b);
        //Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);

        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }

        //Vector3 boundary = Vector3.Lerp(begin, left, b);
        //这里使用扰动后的坐标点计算分界点
        Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
        Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

        //TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);
        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            //AddTriangle(left, right, boundary);
            //这里不再扰动分界点
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }

    /// <summary>
    /// 通过世界内的一个点(vector3)，经过彩色噪点图扰动后，返回扰动后的Vect3
    /// </summary>
    /// <param name="position">世界坐标内的点</param>
    /// <returns>经过噪点图扰动后的点坐标</returns>
    private Vector3 Perturb(Vector3 position)
    {
        //利用世界空间内一点，在彩色噪点图上进行采样，得到彩色噪点图内一点的RGBA信息
        Vector4 sample = HexMetrics.SampleNoise(position);

        //使用原始坐标加上噪点图的采样坐标，得到扰动后坐标
        //position.x += sample.x;
        //position.y += sample.y;
        //position.z += sample.z;

        //将采样后的扰动结果控制在1到-1之间
        //position.x += sample.x * 2f - 1f;
        //position.y += sample.y * 2f - 1f;
        //position.z += sample.z * 2f - 1f;

        //增加了每个点的扰动强度
        position.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
        //为了让cell表面变得平坦，这里不再在垂直方向上进行扰动。
        //position.y += (sample.y * 2f - 1f) * HexMetrics.cellPerturbStrength;
        position.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;

        return position;
    }

    /// <summary>
    /// 使用计算好的5个顶点，对cell的六边形其中一个三角面片进行细分
    /// </summary>
    /// <param name="center">cell中心点位置</param>
    /// <param name="edge">一条边上细分后的5个顶点信息</param>
    /// <param name="color">cell的颜色</param>
    private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
    {
        AddTriangle(center, edge.v1, edge.v2);
        AddTriangleColor(color);
        //AddTriangle(center, edge.v2, edge.v4);
        //AddTriangleColor(color);
        //加入了新的顶点信息
        AddTriangle(center, edge.v2, edge.v3);
        AddTriangleColor(color);
        AddTriangle(center, edge.v3, edge.v4);
        AddTriangleColor(color);

        AddTriangle(center, edge.v4, edge.v5);
        AddTriangleColor(color);
    }

    /// <summary>
    /// 创建2个cell之间细分后的的连接区域
    /// </summary>
    /// <param name="e1">第一个cell一条边上的4个顶点</param>
    /// <param name="c1">第一个cell的颜色</param>
    /// <param name="e2">第二个cell一条边上的4个顶点</param>
    /// <param name="c2">第二个cell的颜色</param>
    private void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2)
    {
        AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        AddQuadColor(c1, c2);
        //AddQuad(e1.v2, e1.v4, e2.v2, e2.v4);
        //AddQuadColor(c1, c2);
        //添加了新的顶点信息
        AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        AddQuadColor(c1, c2);
        AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        AddQuadColor(c1, c2);

        AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
        AddQuadColor(c1, c2);
    }

    /// <summary>
    /// 当cell中有河流的时候，使用这个方法来进行构建
    /// 参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/6-13-1.png
    /// </summary>
    /// <param name="direction">河流方向</param>
    /// <param name="cell">cell这身实例</param>
    /// <param name="center">cell中心点实际位置</param>
    /// <param name="e">河流穿过的这个边，在这条边上所有的顶点的位置信息</param>
    private void TriangulateWithRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        //河流宽度为二分之一cell边长，又已知边长与外接圆半径(cell外径outerRadius)相同
        //为了保持河道在cell中央的时候不会变形，且没有破面等现象产生
        //所以要将当前河流穿过区域左右两侧的外径，之前顶点与中心重合，现在变为各自距离中心四分之一处
        //Vector3 centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
        //Vector3 centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;


        Vector3 centerL, centerR;
        //这里将河道情况分开讨论了
        //1 笔直穿过cell的河道
        //2 出入口相邻的河道
        //3 出入口间隔一条边的河道
        if (cell.HasRiverThroughEdge(direction.Opposite()))//先判断是否是笔直穿过
        {
            centerL = center + HexMetrics.GetFirstSolidCorner(direction.Previous()) * 0.25f;
            centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
        }
        else if (cell.HasRiverThroughEdge(direction.Next()))//出入口相邻的河道 出口在下面
        {
            centerL = center;
            //centerR = Vector3.Lerp(center, e.v5, 0.5f);
            //虽然河道宽度一直没有改变，但是因为弯道造成了一种挤压感
            //这里将弯道内侧的顶点偏移量减小，环节视觉上的挤压感
            centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
        }
        else if (cell.HasRiverThroughEdge(direction.Previous()))//出入口相邻的河道 出口在上面
        {
            //centerL = Vector3.Lerp(center, e.v1, 0.5f);
            centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
            centerR = center;
        }
        else
        {
            //如果不是笔直的河道，就将河流端点聚拢在cell中心
            centerL = centerR = center;
        }

        //重新计算cell中心点的位置，让其偏离河道弯折处，这样河道在转弯的地方就不会显得狭窄了
        center = Vector3.Lerp(centerL, centerR, 0.5f);

        //根据两侧新的顶点位置，计算出其余顶点的位置
        //EdgeVertices m = new EdgeVertices(
        //    Vector3.Lerp(centerL, e.v1, 0.5f),
        //    Vector3.Lerp(centerR, e.v5, 0.5f)
        //);

        //这里使用新的顶点计算方式，关键是两侧顶点的偏移量
        //参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/6-14-2.png
        //注意梯形中位线部分，中间河道为 1/4+1/4，只看左侧顶点偏移，为1/4的一半，也就是1/8
        //并且中位线是底边长的3/4，以中位线为计算基础，左右两个顶点其实各偏移了中位线的6/1
        //可以这么理解： 1/4 + 1/4 + (1/8 +1/8) 这是中位线宽度，其中的1/4其实是中位线的1/3、而一侧偏移量是1/8，也就是中位线的1/6
        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(centerL, e.v1, 0.5f),
            Vector3.Lerp(centerR, e.v5, 0.5f),
            1f / 6f
        );

        //将河道中心的顶点高度下降
        m.v3.y = center.y = e.v3.y;

        //通过计算后的顶点构建连接区域
        TriangulateEdgeStrip(m, cell.Color, e, cell.Color);

        //之前构建的是梯形中位线到底边的部分
        //这里构建中位线到顶边的部分
        //由于之前所有方法均不适用于这里，所以手动添加顶点
        //首先构建河道两侧三角形区域
        AddTriangle(centerL, m.v1, m.v2);
        AddTriangleColor(cell.Color);
        AddTriangle(centerR, m.v4, m.v5);
        AddTriangleColor(cell.Color);

        //构建河道中央两个四边形
        AddQuad(centerL, center, m.v2, m.v3);
        AddQuadColor(cell.Color);
        AddQuad(center, centerR, m.v3, m.v4);
        AddQuadColor(cell.Color);
    }

    /// <summary>
    /// 构建河流起点或终点的cell
    /// </summary>
    /// <param name="direction">河流进入或者流出的方向</param>
    /// <param name="cell">cell自身实例</param>
    /// <param name="center"></param>
    /// <param name="e"></param>
    private void TriangulateWithRiverBeginOrEnd(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        //计算中位线上的5个顶点位置
        //这里注意，由于是河流的终点或起点，所以河流整体是向内聚拢的，5个顶点对中位线进行等分
        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(center, e.v1, 0.5f),
            Vector3.Lerp(center, e.v5, 0.5f)
        );

        //依然保持河道高度
        m.v3.y = e.v3.y;

        //构建中位线到cell边缘的梯形，这里是由4个矩形组成的，跟连接区域类似，所以使用了构建连接区域的方法
        TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
        //构建顶点到中位线的区域
        TriangulateEdgeFan(center, m, cell.Color);
    }
}