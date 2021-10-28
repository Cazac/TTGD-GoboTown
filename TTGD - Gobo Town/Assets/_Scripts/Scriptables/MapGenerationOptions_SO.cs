using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Generation Options", menuName = "Scriptables/Generation Options")]
public class MapGenerationOptions_SO : ScriptableObject
{
    ////////////////////////////////

    [Header("Hex Map Options")]
    public bool isShowingGenerationTime;
    public bool isShowingToDos;

    ////////////////////////////////

    [Header("Camera Control Type")]
    public bool isCameraFirstPerson;
    public bool isCameraThirdPerson;

    ////////////////////////////////

    [Header("Hex Gen Settings - RNG")]
    [Range(1000, 9999)]
    public int mapGen_Seed;

    ////////////////////////////////

    [Header("Map Gen Settings - Size")]
    [Tooltip("This value needs to be Log(2) of 'SectorTotalSize'")]
    public int mapGen_StartingBiomeNodesCount;
    [Tooltip("This value needs to be a divisable value of 'SectorTotalSize'")]
    public int mapGen_ChunkSize;
    [Tooltip("This value needs to be 2^ of 'StartingBiomeNodesCount' and a divisable by 'ChunkSize'")]
    public int mapGen_SectorTotalSize;

    ////////////////////////////////

    [Header("Map Gen Settings - Height")]
    public int mapGen_HeightMin;
    public int mapGen_HeightMax;
    public float mapGen_HeightPerStep;

    ////////////////////////////////

    [Header("Map Gen Settings - Biomes")]
    public int mapGen_OceanSize;
    public int mapGen_BeachSize;

    ////////////////////////////////

    [Header("Height Noise Settings")]
    public float perlinZoomScale_Height;
    public float perlinOffsetX_Height;
    public float perlinOffsetY_Height;

    ////////////////////////////////

    [Header("Color Noise Settings")]
    public float perlinZoomScale_Color;
    public float perlinOffsetX_Color;
    public float perlinOffsetY_Color;
    public float colorRandomizationRange;
    public bool isColorStyleChaos;

    ////////////////////////////////

    [Header("Biome Info Sets")]
    public BiomeChanceInfo[] allBiomes_Arr;

    ////////////////////////////////

    [Header("Hex Uneditable Sizes")]
    public const float outerRadius = 0.15f;
    //public const float outerRadius = 0.1f;
    public const float innerRadius = outerRadius * 0.866025404f;
    public const float spacing_I = innerRadius * 2f;
    public const float spacing_J = outerRadius * 1.5f;
    public const float offcenter_I = spacing_I / 2;

    ////////////////////////////////
}
