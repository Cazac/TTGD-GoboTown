using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

///////////////
/// <summary>
///     
/// BiomeChance_GUI is used as a "Editor" for the BiomeSelectionChance inspector view to change the whole classes view style in inspector while formated.
/// 
/// </summary>
///////////////

[CustomEditor(typeof(MapGenerationOptions_SO))]
public class MapGenerationOptions_GUI : Editor
{
    /////////////////////////////////////////////////////////////////

    public override void OnInspectorGUI()
    {
        //Get The Biome Info
        MapGenerationOptions_SO mapGenerationOptions = (MapGenerationOptions_SO)target;

        //Create A Spacing bar
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //Show Information From The BiomeInfo Which is modded by the "BiomeInfo_Drawer"
        base.OnInspectorGUI();

        //Spacing bar
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //Create A Button To Refresh Chances
        if (GUILayout.Button("Reset Biome Weights"))
        {
            //Reset Weights
            ResetWeights(mapGenerationOptions);
        }

        //If The GUI had a Change Recalcualte the Chances
        if (GUI.changed)
        {
            //Refresh Chances
            RefreshChances(mapGenerationOptions);
        }
    }

    /////////////////////////////////////////////////////////////////

    private void RefreshChances(MapGenerationOptions_SO mapGenerationOptions)
    {
        if (mapGenerationOptions.allBiomes_Arr == null || mapGenerationOptions.allBiomes_Arr.Length == 0)
        {
            return;
        }

        //Collect Info On All of the Weights 
        float maxedTotal = 0;
        foreach (BiomeChanceInfo biomeCell in mapGenerationOptions.allBiomes_Arr)
        {
            maxedTotal += biomeCell.weight;
        }

        //Display A Calculated Weight Dependant on others
        for (int i = 0; i < mapGenerationOptions.allBiomes_Arr.Length; i++)
        {
            mapGenerationOptions.allBiomes_Arr[i].totalChance = (mapGenerationOptions.allBiomes_Arr[i].weight / maxedTotal) * 100f;
        }
    }

    private void ResetWeights(MapGenerationOptions_SO mapGenerationOptions)
    {
        //Set All The Weights Back To 1
        for (int i = 0; i < mapGenerationOptions.allBiomes_Arr.Length; i++)
        {
            mapGenerationOptions.allBiomes_Arr[i].weight = 1f;
        }

        //Refresh It Again
        RefreshChances(mapGenerationOptions);
    }

    /////////////////////////////////////////////////////////////////
}
