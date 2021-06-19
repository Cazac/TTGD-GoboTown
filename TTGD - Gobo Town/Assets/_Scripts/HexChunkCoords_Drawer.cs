using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

///////////////
/// <summary>
///     
/// HexCoords_Drawer is used as a "PropertyDrawer" for the HexCoords property of X, Y, H, L fields shown in a single line format.
/// 
/// </summary>
///////////////

[CustomPropertyDrawer(typeof(HexChunkCoords))]
public class HexChunkCoords_Drawer : PropertyDrawer
{
    /////////////////////////////////////////////////////////////////

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Get Label / Position
        label = EditorGUI.BeginProperty(position, label, property);
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);

        //Create Info Strings From Preperties
        string hexCoordInfo_X = "X: " + property.FindPropertyRelative("x").intValue;
        string hexCoordInfo_Y = "Y: " + property.FindPropertyRelative("y").intValue;
        //string hexCoordInfo_L = "Level: " + property.FindPropertyRelative("l").intValue;

        //Merge Strings Into Single Field
        EditorGUI.LabelField(contentPosition, "(" + hexCoordInfo_X + ", " + hexCoordInfo_Y + ")", EditorStyles.boldLabel);

        //Finish
        EditorGUI.EndProperty();
    }

    /////////////////////////////////////////////////////////////////
}
