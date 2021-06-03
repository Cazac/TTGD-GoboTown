using UnityEditor;
using UnityEngine;

///////////////
/// <summary>
///     
/// BiomeInfo_Drawer is used as a "PropertyDrawer" for the BiomeCellInfo property field shown in a list view format.
/// 
/// </summary>
///////////////

[CustomPropertyDrawer(typeof(BiomeCellInfo))]
public class BiomeInfo_Drawer : PropertyDrawer
{
    /////////////////////////////////////////////////////////////////

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Get Label / Position
        label = EditorGUI.BeginProperty(position, label, property);
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);

        //Create a 20% width minus the label tag space of 4
        contentPosition.width *= 0.2f;
        contentPosition.width -= 4f;
        EditorGUI.indentLevel = 0;
        EditorGUIUtility.labelWidth = 15f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("material"), new GUIContent("M"));

        //Space the indentation by adding the current width + label tag space of 4
        contentPosition.x += contentPosition.width + 4f;
        contentPosition.width *= 1f;
        EditorGUIUtility.labelWidth = 15f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("gradient"), new GUIContent("G"));

        //Space the indentation then set width
        contentPosition.x += contentPosition.width + 4f;
        contentPosition.width *= 1f;
        EditorGUIUtility.labelWidth = 15f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("weight"), new GUIContent("W"));

        //Space the indentation then set width
        contentPosition.x += contentPosition.width + 4f + 15f;
        contentPosition.width *= 0.75f;
        EditorGUI.LabelField(contentPosition, (int)property.FindPropertyRelative("totalChance").floatValue + "%", EditorStyles.boldLabel);

        //Space the indentation then set width
        contentPosition.x += contentPosition.width + 4f + 15f;
        contentPosition.width *= 1;
        EditorGUI.LabelField(contentPosition, "#" + property.FindPropertyRelative("matID").intValue.ToString(), EditorStyles.boldLabel);

        //Finish
        EditorGUI.EndProperty();
    }

    /////////////////////////////////////////////////////////////////
}
