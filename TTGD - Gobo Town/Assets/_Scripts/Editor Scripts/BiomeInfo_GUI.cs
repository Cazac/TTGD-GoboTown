using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

///////////////
/// <summary>
///     
/// BiomeInfo_GUI is used as a "Editor" for the BiomeInfo_SO inspector view to change the whole classes view style in inspector while formated..
/// 
/// </summary>
///////////////

[CustomEditor(typeof(BiomeInfo_SO))]
public class BiomeInfo_GUI : Editor
{
    /////////////////////////////////////////////////////////////////

    public override void OnInspectorGUI()
    {
        //Get The Biome Info
        BiomeInfo_SO biomeInfoSet = (BiomeInfo_SO)target;

        //Create A Spacing bar
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //Show Information From The BiomeInfo Which is modded by the "BiomeInfo_Drawer"
        base.OnInspectorGUI();

        //Spacing bar
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //Create A Button To Refresh Chances
        if (GUILayout.Button("Reset Weights"))
        {
            //Reset Weights
            ResetWeights(biomeInfoSet);
        }

        //If The GUI had a Change Recalcualte the Chances
        if (GUI.changed)
        {
            //Refresh Chances
            RefreshChances(biomeInfoSet);
            RefreshMatIDs(biomeInfoSet);
        }
    }

    /////////////////////////////////////////////////////////////////

    private void RefreshChances(BiomeInfo_SO biomeInfoSet)
    {
        if (biomeInfoSet.biomeCellsInfo_Arr == null || biomeInfoSet.biomeCellsInfo_Arr.Length == 0)
        {
            return;
        }

        //Collect Info On All of the Weights 
        float maxedTotal = 0;
        foreach (BiomeCellInfo biomeCell in biomeInfoSet.biomeCellsInfo_Arr)
        {
            maxedTotal += biomeCell.weight;
        }

        //Display A Calculated Weight Dependant on others
        for (int i = 0; i < biomeInfoSet.biomeCellsInfo_Arr.Length; i++)
        {
            biomeInfoSet.biomeCellsInfo_Arr[i].totalChance = (biomeInfoSet.biomeCellsInfo_Arr[i].weight / maxedTotal) * 100f;
        }
    }

    private void RefreshMatIDs(BiomeInfo_SO biomeInfoSet)
    {
        if (biomeInfoSet.biomeCellsInfo_Arr == null || biomeInfoSet.biomeCellsInfo_Arr.Length == 0)
        {
            return;
        }

        //Create List of All Mats Then Collect All of them
        List<Material> dupesMats_List = new List<Material>();
        foreach (BiomeCellInfo biomeCell in biomeInfoSet.biomeCellsInfo_Arr)
        {
            //Add to list
            dupesMats_List.Add(biomeCell.material);
        }

        //Compress List Of Mats to Uniques Only
        List<Material> compressedMats_List = dupesMats_List.Distinct().ToList();

        //Display A Calculated Weight Dependant on others
        for (int i = 0; i < biomeInfoSet.biomeCellsInfo_Arr.Length; i++)
        {
            //Search Of Index of new list by current Mat
            biomeInfoSet.biomeCellsInfo_Arr[i].matID = compressedMats_List.IndexOf(biomeInfoSet.biomeCellsInfo_Arr[i].material);
        }
    }

    private void ResetWeights(BiomeInfo_SO biomeInfoSet)
    {
        //Set All The Weights Back To 1
        for (int i = 0; i < biomeInfoSet.biomeCellsInfo_Arr.Length; i++)
        {
            biomeInfoSet.biomeCellsInfo_Arr[i].weight = 1f;
        }

        //Refresh It Again
        RefreshChances(biomeInfoSet);
    }

    /////////////////////////////////////////////////////////////////
}