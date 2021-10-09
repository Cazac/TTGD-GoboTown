using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct MapGenLayerBiome
{
    ////////////////////////////////

    [Header("Sector Generation Info Sets")]
    private static int[,] mapHex_BiomeSets;

    [Header("Map Generation Options")]
    private static MapGenerationOptions_SO mapGenOpts;

    /////////////////////////////////////////////////////////////////

    public static int[,] GenerateSectorValues(MapGenerationOptions_SO incomingMapOptions)
    {
        //Set Options
        mapGenOpts = incomingMapOptions;

        //Run Biome Generation
        HexGenBiomeLayer_Basic();
        //HexGenBiomeLayer_Border();


        //HexGenBiomeLayer_Lake();
        //HexGenBiomeLayer_River();
        //HexGenBiomeLayer_Beach();


        //HexGen_BiomeBlender();

        //Return Biome Map Array
        return mapHex_BiomeSets;
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenBiomeLayer_Basic()
    {
        //Create Base Display List Setups
        HexGenBiome_CreateInitialBiomes();

        //Use Log() to calucale how many loops are needed to get to the side length value
        int mapHexGeneration_BiomeGrowthLoopCount = (int)Mathf.Log((float)mapGenOpts.mapGen_SectorTotalSize / mapGenOpts.mapGen_StartingBiomeNodesCount, 2);

        //For The Amount of times needed to double the inital Array, zoom out then Fill Map
        for (int i = 0; i < mapHexGeneration_BiomeGrowthLoopCount; i++)
        {
            //Zoom Then Fill To Expand Map
            HexGenBiome_ZoomOutBiome();
            HexGenBiome_FillZeros();

            if (mapGenOpts.isShowingToDos)
            {
                Debug.Log("Test Code: Needs Smoothing!");
            }
        }




        if (mapGenOpts.isShowingToDos)
        {
            Debug.Log("Test Code: Use a Biome Weight System! Add Color AND Height smoothing!");
        }
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenBiome_CreateInitialBiomes()
    {
        //Create a New Biome Sets Array by Current Start Count
        mapHex_BiomeSets = new int[mapGenOpts.mapGen_StartingBiomeNodesCount, mapGenOpts.mapGen_StartingBiomeNodesCount];

        //Nested Loop All Of the Starting Nodes With Random Values
        for (int y = 0; y < mapGenOpts.mapGen_StartingBiomeNodesCount; y++)
        {
            for (int x = 0; x < mapGenOpts.mapGen_StartingBiomeNodesCount; x++)
            {
                //Generate A Random Type Biome Cell Value
                float randomCellChance = Random.Range(0f, 1f);
                float cumulativeChance = 0f;

                //For each avalible Biome Cell loop till the chance is met
                for (int i = 0; i < mapGenOpts.allBiomes_Arr.Length; i++)
                {
                    //Add To The cumultive chance
                    cumulativeChance += mapGenOpts.allBiomes_Arr[i].totalChance / 100f;

                    //Check if Cumulative Chance passes the randomized value
                    if (cumulativeChance >= randomCellChance)
                    {
                        //Chance has been accepted, Use the Biome Type
                        mapHex_BiomeSets[x, y] = i;
                        break;
                    }
                }
            }
        }
    }

    private static void HexGenBiome_ZoomOutBiome()
    {
        int currentSize_Row = mapHex_BiomeSets.GetLength(0);
        int currentSize_Column = mapHex_BiomeSets.GetLength(1);
        int[,] newScaleMap_Arr = new int[currentSize_Row * 2, currentSize_Row * 2];


        for (int y = 0; y < currentSize_Row; y++)
        {
            for (int x = 0; x < currentSize_Column; x++)
            {
                //Find Positions Around Old Node
                Vector2 newSet_TopLeft = new Vector2(x * 2, y * 2);
                Vector2 newSet_TopRight = new Vector2((x * 2) + 1, y * 2);
                Vector2 newSet_BottomLeft = new Vector2(x * 2, (y * 2) + 1);
                Vector2 newSet_BottomRight = new Vector2((x * 2) + 1, (y * 2) + 1);

                //Fill In The Old Value From The Biome ID Array
                newScaleMap_Arr[(int)newSet_TopLeft.x, (int)newSet_TopLeft.y] = mapHex_BiomeSets[x, y];

                //Fill In the New Values With 0s too be overwritten Later
                newScaleMap_Arr[(int)newSet_TopRight.x, (int)newSet_TopRight.y] = 0;
                newScaleMap_Arr[(int)newSet_BottomLeft.x, (int)newSet_BottomLeft.y] = 0;
                newScaleMap_Arr[(int)newSet_BottomRight.x, (int)newSet_BottomRight.y] = 0;
            }
        }

        //Set The Old Array as the new Expanded Array
        mapHex_BiomeSets = newScaleMap_Arr;
    }

    private static void HexGenBiome_FillZeros()
    {
        //Foreach Each Set Of Hexs Fill All 0s
        for (int y = 0; y < mapHex_BiomeSets.GetLength(0); y++)
        {
            for (int x = 0; x < mapHex_BiomeSets.GetLength(1); x++)
            {
                //Check if current value is 0
                if (mapHex_BiomeSets[x, y] == 0)
                {
                    //Check Current Status of I / J to determine where the value comes from
                    if (y % 2 == 0)
                    {
                        //J is an Even Value
                        HexGenBiome_FillZeros_JEven(x, y);
                    }
                    else if (x % 2 == 0)
                    {
                        //I is an Even Value
                        HexGenBiome_FillZeros_IEven(x, y);
                    }
                    else if (y + 1 == mapHex_BiomeSets.GetLength(1))
                    {
                        //J is at Max Value
                        HexGenBiome_FillZeros_JMaxed(x, y);
                    }
                    else
                    {
                        //Both Values are Odd / Use diagonals
                        HexGenBiome_FillZeros_Diagonals(x, y);
                    }
                }
            }
        }
    }

    private static void HexGenBiome_FillZeros_JEven(int x, int y)
    {
        //Random FOr Left and Right
        int rand = Random.Range(0, 2);

        switch (rand)
        {
            //Left
            case 0:
                if (HexGenBiome_CheckValidHexSpace(x - 1, y))
                {
                    //True Left
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y];
                }
                else
                {
                    //Forced Right
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x + 1, y];
                }
                break;

            //Right
            case 1:
                if (HexGenBiome_CheckValidHexSpace(x + 1, y))
                {
                    //True Right
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x + 1, y];
                }
                else
                {
                    //Forced Left
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y];
                }
                break;
        }
    }

    private static void HexGenBiome_FillZeros_IEven(int x, int y)
    {
        //Random For Up and Down
        int rand = Random.Range(0, 2);

        switch (rand)
        {
            //Down
            case 0:
                if (HexGenBiome_CheckValidHexSpace(x, y - 1))
                {
                    //True Down
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x, y - 1];
                }
                else
                {
                    //Forced Up
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x, y + 1];
                }
                break;

            //Up
            case 1:
                if (HexGenBiome_CheckValidHexSpace(x, y + 1))
                {
                    //True Up
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x, y + 1];
                }
                else
                {
                    //Forced Down
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x, y - 1];
                }
                break;


        }
    }

    private static void HexGenBiome_FillZeros_JMaxed(int x, int y)
    {
        if (x == 0)
        {

            //True Right / Down
            mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x + 1, y - 1];

            return;

            if (HexGenBiome_CheckValidHexSpace(x + 1, y - 1))
            {

            }
            else
            {
                //Forced Left / Up
                mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y + 1];
            }
        }
        else if (x + 1 == mapHex_BiomeSets.GetLength(0))
        {

            //True Left / Down
            mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y - 1];

            return;

            if (HexGenBiome_CheckValidHexSpace(x + 1, y - 1))
            {
                //True Right / Up
                mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x + 1, y - 1];
            }
            else
            {
                //Forced Left / Up
                mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y + 1];
            }
        }
        else
        {
            //Get South
            HexGenBiome_FillZeros_Diagonals(x, y);
        }
    }

    private static void HexGenBiome_FillZeros_Diagonals(int x, int y)
    {
        //Random For Diagonal Sets
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            //Left / Down
            case 0:
                if (HexGenBiome_CheckValidHexSpace(x - 1, y - 1))
                {
                    //True Left / Down
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y - 1];
                }
                else
                {
                    //Forced Right / Up
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x + 1, y + 1];
                }
                break;

            //Right / Up
            case 1:
                if (HexGenBiome_CheckValidHexSpace(x + 1, y + 1))
                {
                    //True Right / Up
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x + 1, y + 1];
                }
                else
                {
                    //Forced Left / Down
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y - 1];
                }
                break;

            //Left / Up
            case 2:
                if (HexGenBiome_CheckValidHexSpace(x - 1, y + 1))
                {
                    //True Up
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y + 1];
                }
                else
                {
                    //Forced Down
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x + 1, y - 1];
                }
                break;

            //Right / Down
            case 3:
                if (HexGenBiome_CheckValidHexSpace(x + 1, y - 1))
                {
                    //True Right / Down
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x + 1, y - 1];
                }
                else
                {
                    //Forced Left / Up
                    mapHex_BiomeSets[x, y] = mapHex_BiomeSets[x - 1, y + 1];
                }
                break;
        }
    }

    private static bool HexGenBiome_CheckValidHexSpace(int x, int y)
    {
        if (x < 0 || y < 0)
        {
            return false;
        }

        if (mapHex_BiomeSets.GetLength(0) < x + 1)
        {
            return false;
        }

        if (mapHex_BiomeSets.GetLength(1) < y + 1)
        {
            return false;
        }

        return true;
    }

    private static void HexGenBiome_PostGeneration_Ocean()
    {
        for (int y = 0; y < mapGenOpts.mapGen_SectorTotalSize; y++)
        {
            for (int x = 0; x < mapGenOpts.mapGen_OceanSize; x++)
            {
                mapHex_BiomeSets[x, y] = 0;
                mapHex_BiomeSets[y, x] = 0;
                mapHex_BiomeSets[mapGenOpts.mapGen_SectorTotalSize - (x + 1), y] = 0;
                mapHex_BiomeSets[y, mapGenOpts.mapGen_SectorTotalSize - (x + 1)] = 0;
            }
        }
    }

    private static void HexGenBiome_PostGeneration_Beaches()
    {
        for (int y = mapGenOpts.mapGen_OceanSize; y < mapGenOpts.mapGen_SectorTotalSize - mapGenOpts.mapGen_OceanSize; y++)
        {
            for (int x = mapGenOpts.mapGen_OceanSize; x < mapGenOpts.mapGen_OceanSize + mapGenOpts.mapGen_BeachSize; x++)
            {
                mapHex_BiomeSets[x, y] = 1;
                mapHex_BiomeSets[y, x] = 1;
                mapHex_BiomeSets[mapGenOpts.mapGen_SectorTotalSize - (x + 1), y] = 1;
                mapHex_BiomeSets[y, mapGenOpts.mapGen_SectorTotalSize - (x + 1)] = 1;
            }
        }
    }

    private static void HexGenBiome_PostGeneration_BiomeSizingMaxMin()
    {
        //2 Matrix

        // one bools

        // one the nodes

        // one the "Biome Clusters"

        //bool[,] hasBeenSearched_Arr = new bool[mapGenOptions.mapGen_SectorTotalSize, mapGenOptions.mapGen_SectorTotalSize];
        //int[] = 

        //BiomeCluster[,] biomeCluster_List;





        //int oceanSize = 10;


        //for (int i = 0; i < mapGenOptions.mapGen_SectorTotalSize; i++)
        {
            //for (int j = 0; j < oceanSize; j++)
            {
                //mapHex_BiomeSets[i, j] = 0;
                //mapHex_BiomeSets[j, i] = 0;
                //mapHex_BiomeSets[i, mapGenOptions.mapGen_SectorTotalSize - (j + 1)] = 0;
                //mapHex_BiomeSets[mapGenOptions.mapGen_SectorTotalSize - (j + 1), i] = 0;
            }
        }
    }

    private static void HexGenBiome_SmoothMap()
    {
        //Get Best Count of 8 corners to round the value ouit
    }

    /////////////////////////////////////////////////////////////////
}
