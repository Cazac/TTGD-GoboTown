using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct MapGeneration
{
    ////////////////////////////////

    [Header("Hex Sector To Be Returned")]
    private static HexSector generatedHexSector;

    ////////////////////////////////

    [Header("Sector Generation Info Sets")]
    private static int[,] mapHex_BiomeSets;
    private static int[,] mapHex_HeightSets;
    private static int[,] mapHex_MatIDSets;
    private static string[,] mapHex_ColorSets;

    ////////////////////////////////

    [Header("Map Generation Options")]
    private static MapGenerationOptions_SO mapGenOpts;

    /////////////////////////////////////////////////////////////////

    public static HexSector HexMapGeneration_NewSector(MapGenerationOptions_SO incomingMapOptions, HexSectorCoords sectorCoords)
    {
        //Set Map Gen Options
        mapGenOpts = incomingMapOptions;

        //Create a New Sector
        generatedHexSector = new HexSector();
        generatedHexSector.sectorCoords = sectorCoords;

        //Setup Array Values
        generatedHexSector.HexCellsData_Dict = new Dictionary<HexCellCoords, HexCell_Data>();
        generatedHexSector.hexChunks_Dict = new Dictionary<HexChunkCoords, HexChunk>();

        //Create Generation Arrays By Size
        mapHex_MatIDSets = new int[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];
        mapHex_ColorSets = new string[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];

        ////////////////////////////////

        //Generate Map Seed For Random Values
        HexGenUtility_SetMapSeed();

        //Generate Biome Sets
        mapHex_BiomeSets = MapGenLayerBiome.GenerateSectorValues(mapGenOpts, sectorCoords);

        //Generate Height Sets
        mapHex_HeightSets = MapGenLayerHeight.GenerateSectorValues(mapGenOpts, sectorCoords, mapHex_BiomeSets);

        //Generate Mats and Colors For the Hex Cells
        HexGenUtility_MatsAndColors();

        //Merge Info Together Into Storable Data Hex Cells
        HexGenUtility_MergeRawDataToHexDataCells();

        //Return The Sector
        return generatedHexSector;
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenUtility_SetMapSeed()
    {
        //Create a Seed for the Sector Using the Map Seed +/- The Sector Coords
        int mapBaseSeed = mapGenOpts.mapGen_Seed;

        //Generate a Sector Direction Coord Prefix For the Seed Value
        int xyQuadrantValue = 0;
        if (generatedHexSector.sectorCoords.x >= 0)
        {
            if (generatedHexSector.sectorCoords.y >= 0)
            {
                //Bottom Left is 0
                xyQuadrantValue = 0;
            }
            else
            {
                //Top Left is 1
                xyQuadrantValue = 1;
            }
        }
        else
        {
            if (generatedHexSector.sectorCoords.y >= 0)
            {
                //Bottom Right is 2
                xyQuadrantValue = 2;
            }
            else
            {
                //Top Right is 3
                xyQuadrantValue = 3;
            }
        }

        //Get The Absoulte Value of the sectors Coords to use as the scaling up / down numbers
        int xValue = Mathf.Abs(generatedHexSector.sectorCoords.x);
        int yValue = Mathf.Abs(generatedHexSector.sectorCoords.y);

        //Add the Values as string decimal places Then Parse to Int
        string sectorString = xyQuadrantValue.ToString() + xValue.ToString() + yValue.ToString();
        int sectorValue = int.Parse(sectorString);

        //Scale Up The Base Seed by increments of 100 then add to the final value
        int finalSeed = sectorValue + (mapBaseSeed * 100);

        //Set The Random State With The Seed
        Random.InitState(finalSeed);
    }

    private static void HexGenUtility_MatsAndColors()
    {
        //Spawn Cells By X (Left and Right)
        for (int y = 0; y < mapGenOpts.mapGen_SectorTotalSize; y++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int x = 0; x < mapGenOpts.mapGen_SectorTotalSize; x++)
            {
                //Get The Biome Info Cell based off of the biome in the given cell
                BiomeCellInfo biomeCellInfo = mapGenOpts.allBiomes_Arr[mapHex_BiomeSets[x, y]].biomeInfo_SO.GetRandomBiomeCell();

                //Set The MatID / Color Sets
                mapHex_MatIDSets[x, y] = biomeCellInfo.matID;
                mapHex_ColorSets[x, y] = ColorUtility.ToHtmlStringRGB(biomeCellInfo.gradient.Evaluate(Random.Range(0, 1f)));
            }
        }
    }

    private static void HexGenUtility_MergeRawDataToHexDataCells()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapGenOpts.mapGen_SectorTotalSize; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapGenOpts.mapGen_SectorTotalSize; y++)
            {
                //Store The Data Collected From Other Methodsit
                HexCell_Data mergedCell = new HexCell_Data
                {
                    hexCoords = new HexCellCoords(x + (generatedHexSector.sectorCoords.x * mapGenOpts.mapGen_SectorTotalSize), y + (generatedHexSector.sectorCoords.y * mapGenOpts.mapGen_SectorTotalSize)),
                    hexCell_HeightSteps = mapHex_HeightSets[x, y],
                    hexCell_BiomeID = mapHex_BiomeSets[x, y],
                    hexCell_Color = mapHex_ColorSets[x, y],
                    hexCell_MatID = mapHex_MatIDSets[x, y]
                };

                //Add to Dictionary
                generatedHexSector.HexCellsData_Dict.Add(mergedCell.hexCoords, mergedCell);
            }
        }
    }

    /////////////////////////////////////////////////////////////////
}
