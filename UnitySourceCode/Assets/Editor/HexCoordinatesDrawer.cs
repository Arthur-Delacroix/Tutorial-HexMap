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

        //绘制坐标的类型名称
        position = EditorGUI.PrefixLabel(position, label);

        //利用HexCoordinates中重载的ToString方法，显示坐标
        GUI.Label(position, coordinates.ToString());
    }
}