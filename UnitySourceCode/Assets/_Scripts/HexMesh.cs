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

        //这两个Vector3变量，是新的cell自身颜色区域中，两个新的顶点信息，其每个顶点距离cell中心为75%外接圆半径
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        //这两个Vector3变量，是原本构成cell一个三角面片的其中两个顶点位置。现在是颜色混合区域的两个顶点位置。
        //Vector3 v3 = center + HexMetrics.GetFirstCorner(direction);
        //Vector3 v4 = center + HexMetrics.GetSecondCorner(direction);

        //颜色混合区域变为了矩形，V3和V4的位置，其实是通过V1和V2顶点分别加上矩形区域的高来计算得出的
        //具体可以查看HexMetrics.GetBridge方法的说明
        //Vector3 bridge = HexMetrics.GetBridge(direction);
        //Vector3 v3 = v1 + bridge;
        //Vector3 v4 = v2 + bridge;
        //该段代码移至 TriangulateConnection 方法中

        //根据中点位置计算出其余两个顶点的信息
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

        //将计算好的颜色混合区域定点位置信息，添加到添加到链表中
        //AddQuad(v1, v2, v3, v4);
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
        AddTriangleColor(cell.color);

        //为颜色混合区域的4个顶点分别赋值颜色
        //其中v1 v2是cell自身颜色，v3 v4是混合后的颜色
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
        //AddTriangle(v1, center + HexMetrics.GetFirstCorner(direction), v3);

        //为第一个三角形三色混合区域赋值颜色
        //自身颜色、三个相邻cell的平均色、矩形混合区域中间色
        //AddTriangleColor(
        //    cell.color,
        //    (cell.color + prevNeighbor.color + neighbor.color) / 3f,
        //    bridgeColor
        //    );

        //第二个三角形三色混合区域
        //AddTriangle(v2, v4, center + HexMetrics.GetSecondCorner(direction));

        //第二个三角形三色混合区域的颜色
        //AddTriangleColor(
        //    cell.color,
        //    bridgeColor,
        //    (cell.color + nextNeighbor.color + neighbor.color) / 3f
        //    );

        //只生成NE、E、SE这三个方位的连接
        if (direction <= HexDirection.SE)
        {
            TriangulateConnection(direction, cell, v1, v2);
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
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

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
    //private void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    //{
    //    colors.Add(c1);
    //    colors.Add(c2);
    //    colors.Add(c3);
    //    colors.Add(c4);
    //}

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
    /// 构建cell其中一个三角面片的颜色混合区域
    /// </summary>
    /// <param name="direction">颜色混合区域的方位</param>
    /// <param name="cell">cell自身实例，用于取得cell位置和颜色 也是三角面片的第一个顶点</param>
    /// <param name="v1">自身颜色三角面片 的第二个顶点</param>
    /// <param name="v2">自身颜色三角面片 的第三个顶点</param>
    private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {
        //HexCell neighbor = cell.GetNeighbor(direction) ?? cell;

        HexCell neighbor = cell.GetNeighbor(direction);

        //当一个方位没有相邻的cell时，不生成双色混合区域
        if (neighbor == null)
        {
            return;
        }

        //参考图 http://magi-melchiorl.gitee.io/pages/Pics/Hexmap/2-8-1.png
        //先计算出颜色混合区域的高度，在通过v1 v2计算出v3 v4，这样就知道了矩形颜色混合区域的四个顶点了
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        //这里为连接相邻cell的v3 v4顶点加上其所在cell的高度
        v3.y = v4.y = neighbor.Elevation * HexMetrics.elevationStep;

        //进行矩形颜色混合区域的三角面片构建和赋值顶点颜色
        //AddQuad(v1, v2, v3, v4);
        //AddQuadColor(cell.color, neighbor.color);
        //以上方法注释掉，使用新的 TriangulateEdgeTerraces  进行替换
        //TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
        //在这里新加入判断，当两个相邻cell的连接类型为Slope的时候，才会创建阶梯化连接
        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
        }
        else
        {
            //当连接类型不为Slope的时候，连接区域是矩形的
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.color, neighbor.color);
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
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;

            //v2 + HexMetrics.GetBridge(direction.Next()) 为三角形的最后一个顶点位置
            //首先通过HexMetrics.GetBridge(direction.Next()获取 相邻的第二个cell的矩形连接区域宽度，可以理解为一个向量
            //v2顶点位置再加上这个向量，得出了三角形最后一个顶点的位置
            //AddTriangle(v2, v4, v2 + HexMetrics.GetBridge(direction.Next()));
            AddTriangle(v2, v4, v5);

            AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
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
    private void TriangulateEdgeTerraces(
        Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
        Vector3 endLeft, Vector3 endRight, HexCell endCell
    )
    {
        //这里先生成阶梯的第一个矩形面片。通过给定插值来计算出矩形面片的另外两个顶点
        Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

        AddQuad(beginLeft, beginRight, v3, v4);
        AddQuadColor(beginCell.color, c2);

        //阶梯的其他矩形面片，可以通过循环来生成
        //旧的矩形面片终点V3 V4，就是新面片的起点 V1 V2
        //然后再利用插值计算新面片的终点即可
        //颜色计算同理
        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c2;
            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2);
        }

        //连接阶梯的剩余区域
        AddQuad(v3, v4, endLeft, endRight);
        AddQuadColor(c2, endCell.color);
    }
}