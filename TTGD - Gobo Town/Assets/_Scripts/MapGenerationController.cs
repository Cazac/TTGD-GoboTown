using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct MapGenerationController
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

    public static HexSector HexMapGenerate_InitialSector(MapGenerationOptions_SO incomingMapOptions, HexSectorCoords sectorCoords)
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Set Map Gen Options
        mapGenOpts = incomingMapOptions;

        //Create a New Sector
        generatedHexSector = new HexSector();
        generatedHexSector.sectorCoords = sectorCoords;

        //Setup Array Values
        generatedHexSector.HexCellsData_Dict = new Dictionary<HexCellCoords, HexCell_Data>();
        generatedHexSector.hexChunks_Dict = new Dictionary<HexChunkCoords, HexChunk>();

        //Create Generation Arrays By Size
        mapHex_HeightSets = new int[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];
        mapHex_MatIDSets = new int[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];
        mapHex_ColorSets = new string[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];

        ////////////////////////////////

        //Generate Map Seed For Random Values
        HexGenUtility_SetMapSeed();

        //Generate Biome Sets
        HexGenBiome();

        //Generate Height Sets
        HexGenHeight();

        //Generate Mats and Colors For the Hex Cells
        HexGenUtility_MatsAndColors();

        //Merge Info Together Into Storable Data Hex Cells
        HexGenUtility_MergeRawDataToHexDataCells();

        //Show Generation Time
        if (mapGenOpts.isShowingGenerationTime)
        {
            //Finish Counting Timer
            long endingTimeTicks = DateTime.UtcNow.Ticks;
            float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
            int mapHexGeneration_BiomeGrowthLoopCount = (int)Mathf.Log((float)mapGenOpts.mapGen_SectorTotalSize / mapGenOpts.mapGen_StartingBiomeNodesCount, 2);
            Debug.Log("Sector Gen in: " + finishTime + "s" + " (Size " + mapHex_BiomeSets.GetLength(0) + "x" + mapHex_BiomeSets.GetLength(1) + " - Biome Loops: x" + mapHexGeneration_BiomeGrowthLoopCount + ")");
        }

        //Return The Sector
        return generatedHexSector;
    }

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
        mapHex_HeightSets = new int[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];
        mapHex_MatIDSets = new int[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];
        mapHex_ColorSets = new string[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];

        ////////////////////////////////

        //Generate Map Seed For Random Values
        HexGenUtility_SetMapSeed();

        //Generate Biome Sets
        HexGenBiome();

        //Generate Height Sets
        HexGenHeight();

        //Generate Mats and Colors For the Hex Cells
        HexGenUtility_MatsAndColors();

        //Merge Info Together Into Storable Data Hex Cells
        HexGenUtility_MergeRawDataToHexDataCells();

        //Return The Sector
        return generatedHexSector;
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenHeight()
    {
        //Spawn Cells By X (Left and Right)
        for (int y = 0; y < mapGenOpts.mapGen_SectorTotalSize; y++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int x = 0; x < mapGenOpts.mapGen_SectorTotalSize; x++)
            {
                //Generate Biome Heights Depending on Biome Generated at this point in the Array
                switch (mapHex_BiomeSets[x, y])
                {
                    //0 == Ocean
                    case 0:
                        HexGenHeight_Flat_Ocean(x, y);
                        break;

                    //1 == Beach
                    case 1:
                        HexGenHeight_Flat_Beach(x, y);
                        break;

                    //2 == Plains
                    case 2:
                        HexGenHeight_Perlin_Plains(x, y);
                        break;

                    //3 == Forest
                    case 3:
                        HexGenHeight_Perlin_Plains(x, y);
                        break;

                    //??? == ???
                    default:
                        break;
                }
            }
        }
    }

    private static void HexGenHeight_Flat_Ocean(int x, int y)
    {
        //Create a Height Steps value based off of closeset step to the "Real" Height
        int heightSteps = 4;

        //Set Final Value To Array
        mapHex_HeightSets[x, y] = heightSteps;
    }

    private static void HexGenHeight_Slope_Ocean(int x, int y)
    {
        /*
        //Create a Height Steps value based off of closeset step to the "Real" Height
        int heightSteps = 0;


        //Final Neutral High Step Assumed at 6~
        int highestOcean = 6;
        //int lowestOcean = 0;



        //Dont ask me how this works its all cobbled guesswork :shrug:
        if (x < mapGenOptions.mapGen_OceanSize)
        {
            //Check Corners
            if (y < mapGenOptions.mapGen_OceanSize)
            {
                //Get Closer Value
                int min = Mathf.Min(x, y);

                //Corner 
                heightSteps = highestOcean - (mapGenOptions.mapGen_OceanSize - min);
            }
            else if (y >= (mapGenOptions.mapGen_SectorLength - mapGenOptions.mapGen_OceanSize))
            {
                //Get Closer Value
                int min = Mathf.Min(x, y - (mapGenOptions.mapGen_SectorLength - mapGenOptions.mapGen_OceanSize));

                // 0 - 4 Range

                //Corner 
                heightSteps = highestOcean - (mapGenOptions.mapGen_OceanSize - min);
            }
            else
            {
                //Reg
                heightSteps = highestOcean - (mapGenOptions.mapGen_OceanSize - x);
            }
        }
        else if (x >= (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize))
        {
            //Check Corners
            if (y < mapGenOptions.mapGen_OceanSize)
            {
                //Get Closer Value
                int min = Mathf.Max(x, y);

                //Corner 
                heightSteps = highestOcean - (mapGenOptions.mapGen_OceanSize - min);
            }
            else if (y >= (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize))
            {
                //Get Closer Value
                int min = Mathf.Min(x, y - (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize));

                //Corner 
                //heightSteps = highestOcean - (mapGen_OceanSize - min);
            }
            else
            {
                //Reg
                heightSteps = highestOcean - (x - (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize));
            }
        }
        else if (y < mapGenOptions.mapGen_OceanSize)
        {
            //Check Corners
            if (x < mapGenOptions.mapGen_OceanSize || x >= (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize))
            {

            }
            else
            {
                //Reg
                //heightSteps = highestOcean - (mapGen_OceanSize - y);
            }
        }
        else if (y >= (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize))
        {
            //Check Corners
            if (x < mapGenOptions.mapGen_OceanSize || x >= (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize))
            {
                //Corner
            }
            else
            {
                //Reg
                //heightSteps = highestOcean - (y - (mapGen_SideLength - mapGen_OceanSize));
            }
        }
        else
        {
            Debug.Log("Test Code: Ooops");
        }




        /*
        if (x == mapGen_OceanSize - 1)
        {
            heightSteps = highestOcean;
        }
        else if ( (y == mapGen_OceanSize - 1))
        {
            heightSteps = highestOcean;
        }
        else if (x == (mapGen_SideLength - mapGen_OceanSize))
        {
            heightSteps = highestOcean;
        }
        else if (x == (mapGen_SideLength - mapGen_OceanSize))
        {
            heightSteps = highestOcean;
        }
        else
        {

        }
        */
        /*


       //heightSteps = ;




       //Set Final Value To Array
       mapHex_HeightSets[x, y] = heightSteps;
       */
    }

    private static void HexGenHeight_Flat_Beach(int x, int y)
    {
        //Create a Height Steps value based off of closeset step to the "Real" Height
        int heightSteps = 8;

        //Set Final Value To Array
        mapHex_HeightSets[x, y] = heightSteps;
    }

    private static void HexGenHeight_Perlin_Plains(int x, int y)
    {
        float neutralHeight = 0.5f;


        //USE HEIGHT MAPS BASED OFF BIOMES


        //float perlinZoomScale = 20;

        float xScaled = (float)x / mapGenOpts.perlinZoomScale;
        float yScaled = (float)y / mapGenOpts.perlinZoomScale;


        float height = Mathf.PerlinNoise(xScaled + mapGenOpts.offsetX, yScaled + mapGenOpts.offsetY);




        //Create a Height Value the tends closer to the average of the Neutral Height
        height = (neutralHeight + height) / 2;

        //Create a Height Steps value based off of closeset step to the "Real" Height
        int heightSteps = (int)(height / mapGenOpts.mapGen_HeightPerStep);

        //Set Final Value To Array
        mapHex_HeightSets[x, y] = heightSteps;
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenBiome()
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

        //These Converge into lakes at the end 
        //HexGenBiome_PostGeneration_Ocean();
        //HexGenBiome_PostGeneration_Beaches();
        //HexGeneration_PostGeneration_Rivers();
        //HexGeneration_PostGeneration_Lakes();
        //HexGeneration_PostGeneration_InterBiomes();


        if (mapGenOpts.isShowingToDos)
        {
            Debug.Log("Test Code: Use a Biome Weight System! For Color AND Height smoothing!");
        }
    }

    private static void HexGenBiome_CreateInitialBiomes_DEBUG()
    {
        //Create a New Biome Sets Array by Current Start Count
        mapHex_BiomeSets = new int[mapGenOpts.mapGen_StartingBiomeNodesCount, mapGenOpts.mapGen_StartingBiomeNodesCount];

        //Nested Loop All Of the Starting Nodes With Random Values
        for (int y = 0; y < mapGenOpts.mapGen_StartingBiomeNodesCount; y++)
        {
            for (int x = 0; x < mapGenOpts.mapGen_StartingBiomeNodesCount; x++)
            {
                //Add A Random Biome Starting Node 
                mapHex_BiomeSets[x, y] = Random.Range(3, 3);
            }
        }
    }

    private static void HexGenBiome_CreateInitialBiomes()
    {
        //Create a New Biome Sets Array by Current Start Count
        mapHex_BiomeSets = new int[mapGenOpts.mapGen_StartingBiomeNodesCount, mapGenOpts.mapGen_StartingBiomeNodesCount];

        //Nested Loop All Of the Starting Nodes With Random Values
        for (int y = 0; y < mapGenOpts.mapGen_StartingBiomeNodesCount; y++)
        {
            for (int x = 0; x < mapGenOpts.mapGen_StartingBiomeNodesCount; x++)
            {
                //Add A Random Biome Starting Node 
                mapHex_BiomeSets[x, y] = Random.Range(2, 4);
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
                BiomeCellInfo biomeCellInfo = mapGenOpts.allBiomes_Arr[mapHex_BiomeSets[x, y]].GetRandomBiomeCell();

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

                if (HexGenUtility_CheckChunkNullRange(x, y))
                {
                    //Add to Dictionary
                    generatedHexSector.HexCellsData_Dict.Add(mergedCell.hexCoords, mergedCell);

                }
                else
                {
                    //Add to Dictionary
                    generatedHexSector.HexCellsData_Dict.Add(mergedCell.hexCoords, null);
                }
            }
        }
    }

    private static bool HexGenUtility_CheckChunkNullRange(int x, int y)
    {
        //Get The Range of Cell to go out by
        int additiveRange = mapGenOpts.mapGen_EmptiedChunkBorderSize * mapGenOpts.mapGen_ChunkSize;

        //Check X Range
        if (x < additiveRange || x > mapGenOpts.mapGen_SectorTotalSize - additiveRange - 1)
        {
            //X Value is Out of Range, Empty The Hex
            return false;
        }

        //Check Y Range
        if (y < additiveRange || y > mapGenOpts.mapGen_SectorTotalSize - additiveRange - 1)
        {
            //Y Value is Out of Range, Empty The Hex
            return false;
        }

        //Neither X Nor Y Have Issues Return True
        return true;
    }

    /////////////////////////////////////////////////////////////////
}
