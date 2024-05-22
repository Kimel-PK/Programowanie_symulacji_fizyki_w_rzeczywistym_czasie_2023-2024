using UnityEditor;
using UnityEngine;

public static class EditorGUILayoutExtensions
{
    public static void HorizontalLine(Color color, float thickness = 1, float padding = 10)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, thickness + padding);
        rect.height = thickness;
        rect.y += padding / 2;
        rect.x -= 2; // To account for the padding at the edges
        rect.width += 6;
        EditorGUI.DrawRect(rect, color);
    }
}