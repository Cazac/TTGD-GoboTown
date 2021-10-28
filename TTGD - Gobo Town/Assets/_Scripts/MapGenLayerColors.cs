
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MapGenLayerColors
{
    ////////////////////////////////

    [Header("Sector Generation Info Sets")]
    private static int[,] mapHex_BiomeSets;
    private static int[,] mapHex_MatIDSets;
    private static string[,] mapHex_ColorSets;

    [Header("Map Generation Options")]
    private static MapGenerationOptions_SO mapGenOpts;

    /////////////////////////////////////////////////////////////////





    public static Tuple<int[,], string[,]> GenerateSectorValues(MapGenerationOptions_SO incomingMapOptions, HexSectorCoords sectorCoords, int[,] incomingBiomeSets)
    {
        //Set Options
        mapGenOpts = incomingMapOptions;
        mapHex_BiomeSets = incomingBiomeSets;

        mapHex_MatIDSets = new int[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];
        mapHex_ColorSets = new string[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];

        if (mapGenOpts.isColorStyleChaos)
        {
            HexGenColors_Basic(sectorCoords);
        }
        else
        {
            HexGenColors_Advanced(sectorCoords);
        }




        //Return Biome Map Array
        return new Tuple<int[,], string[,]>(mapHex_MatIDSets, mapHex_ColorSets);
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenColors_Advanced(HexSectorCoords sectorCoords)
    {


        //Loop All Hexes For All colors
        for (int y = 0; y < mapGenOpts.mapGen_SectorTotalSize; y++)
        {
            //Loop All Hexes For All colors
            for (int x = 0; x < mapGenOpts.mapGen_SectorTotalSize; x++)
            {

                //Get The Biome Info Cell based off of the biome in the given cell
                BiomeCellInfo biomeCellInfo = mapGenOpts.allBiomes_Arr[mapHex_BiomeSets[x, y]].biomeInfo_SO.GetRandomBiomeCell();

         


                //Need to use the sector scale as well as an offset
                float xPositional = x + ((float)sectorCoords.x * mapGenOpts.mapGen_SectorTotalSize);
                float yPositional = y + ((float)sectorCoords.y * mapGenOpts.mapGen_SectorTotalSize);

                float xScaled = xPositional / mapGenOpts.perlinZoomScale_Color;
                float yScaled = yPositional / mapGenOpts.perlinZoomScale_Color;


                float colorValue = Mathf.PerlinNoise(xScaled, yScaled);




                //Debug.Log(colorValue);

                //colorRange


                float additveRange = UnityEngine.Random.Range(-mapGenOpts.colorRandomizationRange, mapGenOpts.colorRandomizationRange);

                float finalColor = Mathf.Clamp(colorValue + additveRange, 0f, 1f);

                Debug.Log(finalColor);

                //Set The MatID / Color Sets
                mapHex_MatIDSets[x, y] = biomeCellInfo.matID;
                mapHex_ColorSets[x, y] = ColorUtility.ToHtmlStringRGB(biomeCellInfo.gradient.Evaluate(finalColor));
            }
        }
    }


    private static void HexGenColors_Basic(HexSectorCoords sectorCoords)
    {


        //Loop All Hexes For All colors
        for (int y = 0; y < mapGenOpts.mapGen_SectorTotalSize; y++)
        {
            //Loop All Hexes For All colors
            for (int x = 0; x < mapGenOpts.mapGen_SectorTotalSize; x++)
            {
                //Get The Biome Info Cell based off of the biome in the given cell
                BiomeCellInfo biomeCellInfo = mapGenOpts.allBiomes_Arr[mapHex_BiomeSets[x, y]].biomeInfo_SO.GetRandomBiomeCell();

                //Set The MatID / Color Sets
                mapHex_MatIDSets[x, y] = biomeCellInfo.matID;
                mapHex_ColorSets[x, y] = ColorUtility.ToHtmlStringRGB(biomeCellInfo.gradient.Evaluate(UnityEngine.Random.Range(0, 1f)));
            }
        }
    }

    /////////////////////////////////////////////////////////////////
}
