using UnityEngine;

public class HexGrid : MonoBehaviour
{
    //表示每一行有多少个地图单元
    public int width = 6;

    //表示每一列有多少个地图单元
    public int height = 6;

    //存放地图单元格的预置
    public HexCell cellPrefab;

    //存放实例化的地图单元
    HexCell[] cells;

    private void Awake()
    {
        //根据长度和宽度，初始化数组大小
        cells = new HexCell[height * width];

        //从左下角开始，依次往右，每一行为 width 个单元后，上移一行
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    /// <summary>
    /// 创建一个地图单元
    /// </summary>
    /// <param name="x">地图单元是 横行中的第几个</param>
    /// <param name="z">地图单元是 粽列中的第几个</param>
    /// <param name="i">地图单元在</param>
    private void CreateCell(int x, int z, int i)
    {
        //声明一个Vector3，用来
        Vector3 position;
        position.x = x * 10f;
        position.y = 0f;
        position.z = z * 10f;

        //在数组cells的i位置实例化地图单元
        //cell用来给这个被实例化的单元设置父级和位置
        HexCell cell;
        cells[i] = Instantiate<HexCell>(cellPrefab);
        cell = cells[i];

        //设置被实例化地图单元的父级和位置
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
    }
}