using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BiomeChanceInfo))]
public class BiomeChanceInfo_Drawer : PropertyDrawer
{
    /////////////////////////////////////////////////////////////////

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Get Label / Position
        label = EditorGUI.BeginProperty(position, label, property);
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);

        //Create a 20% width minus the label tag space of 4
        contentPosition.width *= 0.4f;
        contentPosition.width -= 4f;
        EditorGUI.indentLevel = 0;
        EditorGUIUtility.labelWidth = 15f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("biomeInfo_SO"), new GUIContent("B"));

        //Space the indentation then set width
        contentPosition.x += contentPosition.width + 4f;
        contentPosition.width *= 1f;
        EditorGUIUtility.labelWidth = 15f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("weight"), new GUIContent("W"));

        //Space the indentation then set width
        contentPosition.x += contentPosition.width + 4f + 15f;
        contentPosition.width *= 0.75f;
        EditorGUI.LabelField(contentPosition, (int)property.FindPropertyRelative("totalChance").floatValue + "%", EditorStyles.boldLabel);

        //Finish
        EditorGUI.EndProperty();
    }

    /////////////////////////////////////////////////////////////////
}
