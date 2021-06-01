using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexCoords))]
public class HexCoords_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {

        string hexCoordInfo_X = "X: " + property.FindPropertyRelative("x").intValue;
        string hexCoordInfo_Y = "Y: " + property.FindPropertyRelative("y").intValue;
        string hexCoordInfo_Z = "Z: " + property.FindPropertyRelative("z").intValue;
        string hexCoordInfo_L = "Level: " + property.FindPropertyRelative("l").intValue;


        rect.xMin = 18;
   


        label.text = "Hex Coordinates: (" + hexCoordInfo_X + " - " + hexCoordInfo_Y + " - " + hexCoordInfo_Z + " - " + hexCoordInfo_L + ")";
        EditorGUI.PrefixLabel(rect, label);

  

        /*
        label.text = "Hex Coordinates:";
        EditorGUI.PrefixLabel(rect, label);

        label.text = "(" + hexCoordInfo_X + " - " + hexCoordInfo_Z + ")";

        EditorGUI.LabelField(rect, label);


        rect.xMin = 200;
        rect.height = 18;

    */
        //rect.position = new Vector2(60, 40);
        //GUI.Label(rect, "(" + hexCoordInfo_X + " - " + hexCoordInfo_Z +")");
    }
}
