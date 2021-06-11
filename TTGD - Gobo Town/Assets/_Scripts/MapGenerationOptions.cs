using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MapGenerationOptions
{
    ////////////////////////////////
    
    [Header("Hex Map Options")]
    public bool isShowingGenerationTime;
    public bool isShowingToDos;

    ////////////////////////////////
    
    [Header("Hex Gen Settings - RNG")]
    public int mapGen_Seed;

    ////////////////////////////////

    [Header("Map Gen Settings - Size")]
    public int mapGen_StartingBiomeNodesCount;
    public int mapGen_SideLength;
    public int mapGen_ChunkSize;

    ////////////////////////////////

    [Header("Map Gen Settings - Height")]
    public int mapGen_HeightMin;
    public int mapGen_HeightMax;
    public float mapGen_HeightStep;
    public float mapGen_HeightPerStep;

    ////////////////////////////////

    [Header("Map Gen Settings - Biomes")]
    public int mapGen_OceanSize;
    public int mapGen_BeachSize;

    ////////////////////////////////

    [Header("Biome Info Sets")]
    public BiomeInfo_SO[] allBiomes_Arr;

    ////////////////////////////////

    [Header("Perlin Noise Settings")]
    public float perlinZoomScale;
    public float offsetX;
    public float offsetY;

    ////////////////////////////////
}