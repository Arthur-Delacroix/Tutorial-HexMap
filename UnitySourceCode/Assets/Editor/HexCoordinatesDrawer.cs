using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer
{
    //重新绘制X Z的坐标表示
    public override void OnGUI(
        Rect position, SerializedProperty property, GUIContent label)
    {
        //将X和Z的坐标从HexCoordinates中提取出来
        HexCoordinates coordinates = new HexCoordinates(
            property.FindPropertyRelative("x").intValue,
            property.FindPropertyRelative("z").intValue);

        //新建一个Rect变量，不知道什么原因，类型名称没有左对齐
        //这里将位置偏移17px，左对齐
        Rect titlePos =
            new Rect(
            position.x + 17.0f,
            position.y,
            position.width,
            position.height
            );

        //绘制坐标的类型名称
        position = EditorGUI.PrefixLabel(titlePos, label);

        //将左对齐的量偏移回来
        position =
            new Rect(
            position.x - 17.0f,
            position.y,
            position.width,
            position.height
            );

        //利用HexCoordinates中重载的ToString方法，显示坐标
        GUI.Label(position, coordinates.ToString());
    }
}