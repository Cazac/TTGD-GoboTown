using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BiomeCellInfo))]
public class BiomeInfo_Drawer : PropertyDrawer
{
    /////////////////////////////////////////////////////////////////

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        label = EditorGUI.BeginProperty(position, label, property);
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);




    


        contentPosition.width *= 0.2f;
        contentPosition.width -= 4f;
        EditorGUI.indentLevel = 0;
        EditorGUIUtility.labelWidth = 15f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("material"), new GUIContent("M"));


        contentPosition.x += contentPosition.width + 4f;
        contentPosition.width *= 1f;
        EditorGUIUtility.labelWidth = 15f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("gradient"), new GUIContent("G"));


        contentPosition.x += contentPosition.width + 4f;
        contentPosition.width *= 1f;
        EditorGUIUtility.labelWidth = 15f;
        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("weight"), new GUIContent("W"));


        contentPosition.x += contentPosition.width + 4f + 15f;
        contentPosition.width *= 0.75f;
        EditorGUI.LabelField(contentPosition, (int)property.FindPropertyRelative("totalChance").floatValue + "%", EditorStyles.boldLabel);

        contentPosition.x += contentPosition.width + 4f + 15f;
        contentPosition.width *= 1;
        EditorGUI.LabelField(contentPosition, "#" + property.FindPropertyRelative("matID").intValue.ToString(), EditorStyles.boldLabel);



        EditorGUI.EndProperty();
    }

    /////////////////////////////////////////////////////////////////
}
