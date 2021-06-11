using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerationController
{
    ////////////////////////////////

    [Header("Hex Scripts To Be Returned")]
    private static HexCell_Data[,] dataHexCells_Arr;

    ////////////////////////////////

    [Header("Generation List Sets")]
    private static int[,] mapHex_BiomeSets;
    private static int[,] mapHex_HeightSets;
    private static int[,] mapHex_MatIDSets;
    private static string[,] mapHex_ColorSets;

    ////////////////////////////////

    [Header("Map Generation Options")]
    private static MapGenerationOptions mapGenOptions;

    /////////////////////////////////////////////////////////////////

    public static HexCell_Data[,] HexMapGeneration(MapGenerationOptions incomingMapOptions)
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Set Map Gen Options
        mapGenOptions = incomingMapOptions;

        //Setup Array Values
        dataHexCells_Arr = new HexCell_Data[mapGenOptions.mapGen_SideLength, mapGenOptions.mapGen_SideLength];

        //Create Generation Arrays By Size
        mapHex_HeightSets = new int[mapGenOptions.mapGen_SideLength, mapGenOptions.mapGen_SideLength];
        mapHex_MatIDSets = new int[mapGenOptions.mapGen_SideLength, mapGenOptions.mapGen_SideLength];
        mapHex_ColorSets = new string[mapGenOptions.mapGen_SideLength, mapGenOptions.mapGen_SideLength];

        ////////////////////////////////

        //Generate Map Seed For Random Values
        HexGenUtility_SetMapSeed(mapGenOptions.mapGen_Seed);

        //Generate Biome Sets
        HexGenBiome();

        //Generate Height Sets
        HexGenHeight();

        //Generate Mats and Colors For the Hex Cells
        HexGenUtility_MatsAndColors();

        //Merge Info Together Into Storable Data Hex Cells
        HexGenUtility_MergeRawDataToHexDataCells();

        //Show Generation Time
        if (mapGenOptions.isShowingGenerationTime)
        {
            //Finish Counting Timer
            long endingTimeTicks = DateTime.UtcNow.Ticks;
            float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
            int mapHexGeneration_BiomeGrowthLoopCount = (int)Mathf.Log((float)mapGenOptions.mapGen_SideLength / mapGenOptions.mapGen_StartingBiomeNodesCount, 2);
            Debug.Log("Test Code: Biome Generation x" + mapHexGeneration_BiomeGrowthLoopCount + " Completed in: " + finishTime + "s");
            Debug.Log("Test Code: Size " + mapHex_BiomeSets.GetLength(0) + "x" + mapHex_BiomeSets.GetLength(1));
        }

        //Return The Data Set
        return dataHexCells_Arr;
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenHeight()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_HeightSets.GetLength(0); x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_HeightSets.GetLength(1); y++)
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
            else if (y >= (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize))
            {
                //Get Closer Value
                int min = Mathf.Min(x, y - (mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize));

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



        //heightSteps = ;




        //Set Final Value To Array
        mapHex_HeightSets[x, y] = heightSteps;
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

        float xScaled = (float)x / mapGenOptions.perlinZoomScale;
        float yScaled = (float)y / mapGenOptions.perlinZoomScale;


        float height = Mathf.PerlinNoise(xScaled + mapGenOptions.offsetX, yScaled + mapGenOptions.offsetY);




        //Create a Height Value the tends closer to the average of the Neutral Height
        height = (neutralHeight + height) / 2;

        //Create a Height Steps value based off of closeset step to the "Real" Height
        int heightSteps = (int)(height / mapGenOptions.mapGen_HeightPerStep);

        //Set Final Value To Array
        mapHex_HeightSets[x, y] = heightSteps;
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenBiome()
    {
        //Create Base Display List Setups
        HexGenBiome_CreateInitialBiomes();

        //Use Log() to calucale how many loops are needed to get to the side length value
        int mapHexGeneration_BiomeGrowthLoopCount = (int)Mathf.Log((float)mapGenOptions.mapGen_SideLength / mapGenOptions.mapGen_StartingBiomeNodesCount, 2);

        //For The Amount of times needed to double the inital Array, zoom out then Fill Map
        for (int i = 0; i < mapHexGeneration_BiomeGrowthLoopCount; i++)
        {
            //Zoom Then Fill To Expand Map
            HexGenBiome_ZoomOutBiome();
            HexGenBiome_FillZeros();

            if (mapGenOptions.isShowingToDos)
            {
                Debug.Log("Test Code: Needs Smoothing!");
            }
        }

        //These Converge into lakes at the end 
        HexGenBiome_PostGeneration_Ocean();
        HexGenBiome_PostGeneration_Beaches();
        //HexGeneration_PostGeneration_Rivers();
        //HexGeneration_PostGeneration_Lakes();
        //HexGeneration_PostGeneration_InterBiomes();


        if (mapGenOptions.isShowingToDos)
        {
            Debug.Log("Test Code: Use a Biome Weight System! For Color AND Height smoothing!");
        }
    }

    private static void HexGenBiome_CreateInitialBiomes()
    {
        mapHex_BiomeSets = new int[mapGenOptions.mapGen_StartingBiomeNodesCount, mapGenOptions.mapGen_StartingBiomeNodesCount];


        for (int i = 0; i < mapGenOptions.mapGen_StartingBiomeNodesCount; i++)
        {
            for (int j = 0; j < mapGenOptions.mapGen_StartingBiomeNodesCount; j++)
            {
                mapHex_BiomeSets[i, j] = Random.Range(2, 4);
            }
        }
    }

    private static void HexGenBiome_ZoomOutBiome()
    {
        int currentSize_Row = mapHex_BiomeSets.GetLength(0);
        int currentSize_Column = mapHex_BiomeSets.GetLength(1);
        int[,] newScaleMap_Arr = new int[currentSize_Row * 2, currentSize_Row * 2];



        //FILL 2 VALUE SETS PER LOOP
        for (int i = 0; i < currentSize_Row; i++)
        {
            for (int j = 0; j < currentSize_Column; j++)
            {

                Vector2 newSet_TopLeft = new Vector2(i * 2, j * 2);
                Vector2 newSet_TopRight = new Vector2((i * 2) + 1, j * 2);
                Vector2 newSet_BottomLeft = new Vector2(i * 2, (j * 2) + 1);
                Vector2 newSet_BottomRight = new Vector2((i * 2) + 1, (j * 2) + 1);


                newScaleMap_Arr[(int)newSet_TopLeft.x, (int)newSet_TopLeft.y] = mapHex_BiomeSets[i, j];

                newScaleMap_Arr[(int)newSet_TopRight.x, (int)newSet_TopRight.y] = 0;
                newScaleMap_Arr[(int)newSet_BottomLeft.x, (int)newSet_BottomLeft.y] = 0;
                newScaleMap_Arr[(int)newSet_BottomRight.x, (int)newSet_BottomRight.y] = 0;
            }
        }


        mapHex_BiomeSets = newScaleMap_Arr;
    }

    private static void HexGenBiome_FillZeros()
    {
        //Foreach Each Set Of Hexs Fill All 0s
        for (int i = 0; i < mapHex_BiomeSets.GetLength(0); i++)
        {
            for (int j = 0; j < mapHex_BiomeSets.GetLength(1); j++)
            {
                //Check if current value is 0
                if (mapHex_BiomeSets[i, j] == 0)
                {
                    //Check Current Status of I / J to determine where the value comes from
                    if (j % 2 == 0)
                    {
                        //J is an Even Value
                        HexGenBiome_FillZeros_JEven(i, j);
                    }
                    else if (i % 2 == 0)
                    {
                        //I is an Even Value
                        HexGenBiome_FillZeros_IEven(i, j);
                    }
                    else if (j + 1 == mapHex_BiomeSets.GetLength(1))
                    {
                        //J is at Max Value
                        HexGenBiome_FillZeros_JMaxed(i, j);
                    }
                    else
                    {
                        //Both Values are Odd / Use diagonals
                        HexGenBiome_FillZeros_Diagonals(i, j);
                    }
                }
            }
        }

    }

    private static void HexGenBiome_FillZeros_JEven(int i, int j)
    {
        //Random FOr Left and Right
        int rand = Random.Range(0, 2);

        switch (rand)
        {
            //Left
            case 0:
                if (HexGenBiome_CheckValidHexSpace(i - 1, j))
                {
                    //True Left
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j];
                }
                else
                {
                    //Forced Right
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j];
                }
                break;

            //Right
            case 1:
                if (HexGenBiome_CheckValidHexSpace(i + 1, j))
                {
                    //True Right
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j];
                }
                else
                {
                    //Forced Left
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j];
                }
                break;
        }
    }

    private static void HexGenBiome_FillZeros_IEven(int i, int j)
    {
        //Random For Up and Down
        int rand = Random.Range(0, 2);

        switch (rand)
        {
            //Down
            case 0:
                if (HexGenBiome_CheckValidHexSpace(i, j - 1))
                {
                    //True Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i, j - 1];
                }
                else
                {
                    //Forced Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i, j + 1];
                }
                break;

            //Up
            case 1:
                if (HexGenBiome_CheckValidHexSpace(i, j + 1))
                {
                    //True Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i, j + 1];
                }
                else
                {
                    //Forced Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i, j - 1];
                }
                break;


        }
    }

    private static void HexGenBiome_FillZeros_JMaxed(int i, int j)
    {
        if (i == 0)
        {

            //True Right / Down
            mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j - 1];

            return;

            if (HexGenBiome_CheckValidHexSpace(i + 1, j - 1))
            {

            }
            else
            {
                //Forced Left / Up
                mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j + 1];
            }
        }
        else if (i + 1 == mapHex_BiomeSets.GetLength(0))
        {

            //True Left / Down
            mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j - 1];

            return;

            if (HexGenBiome_CheckValidHexSpace(i + 1, j - 1))
            {
                //True Right / Up
                mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j - 1];
            }
            else
            {
                //Forced Left / Up
                mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j + 1];
            }
        }
        else
        {
            //Get South
            HexGenBiome_FillZeros_Diagonals(i, j);
        }
    }

    private static void HexGenBiome_FillZeros_Diagonals(int i, int j)
    {
        //Random For Diagonal Sets
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            //Left / Down
            case 0:
                if (HexGenBiome_CheckValidHexSpace(i - 1, j - 1))
                {
                    //True Left / Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j - 1];
                }
                else
                {
                    //Forced Right / Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j + 1];
                }
                break;

            //Right / Up
            case 1:
                if (HexGenBiome_CheckValidHexSpace(i + 1, j + 1))
                {
                    //True Right / Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j + 1];
                }
                else
                {
                    //Forced Left / Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j - 1];
                }
                break;

            //Left / Up
            case 2:
                if (HexGenBiome_CheckValidHexSpace(i - 1, j + 1))
                {
                    //True Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j + 1];
                }
                else
                {
                    //Forced Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j - 1];
                }
                break;

            //Right / Down
            case 3:
                if (HexGenBiome_CheckValidHexSpace(i + 1, j - 1))
                {
                    //True Right / Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j - 1];
                }
                else
                {
                    //Forced Left / Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j + 1];
                }
                break;
        }
    }

    private static bool HexGenBiome_CheckValidHexSpace(int i, int j)
    {
        if (i < 0 || j < 0)
        {
            return false;
        }

        if (mapHex_BiomeSets.GetLength(0) < i + 1)
        {
            return false;
        }

        if (mapHex_BiomeSets.GetLength(1) < j + 1)
        {
            return false;
        }

        return true;
    }

    private static void HexGenBiome_PostGeneration_Ocean()
    {
        for (int i = 0; i < mapGenOptions.mapGen_SideLength; i++)
        {
            for (int j = 0; j < mapGenOptions.mapGen_OceanSize; j++)
            {
                mapHex_BiomeSets[i, j] = 0;
                mapHex_BiomeSets[j, i] = 0;
                mapHex_BiomeSets[i, mapGenOptions.mapGen_SideLength - (j + 1)] = 0;
                mapHex_BiomeSets[mapGenOptions.mapGen_SideLength - (j + 1), i] = 0;
            }
        }
    }

    private static void HexGenBiome_PostGeneration_Beaches()
    {
        for (int i = mapGenOptions.mapGen_OceanSize; i < mapGenOptions.mapGen_SideLength - mapGenOptions.mapGen_OceanSize; i++)
        {
            for (int j = mapGenOptions.mapGen_OceanSize; j < mapGenOptions.mapGen_OceanSize + mapGenOptions.mapGen_BeachSize; j++)
            {
                mapHex_BiomeSets[i, j] = 1;
                mapHex_BiomeSets[j, i] = 1;
                mapHex_BiomeSets[i, mapGenOptions.mapGen_SideLength - (j + 1)] = 1;
                mapHex_BiomeSets[mapGenOptions.mapGen_SideLength - (j + 1), i] = 1;
            }
        }
    }

    private static void HexGenBiome_PostGeneration_BiomeSizingMaxMin()
    {
        //2 Matrix

        // one bools

        // one the nodes

        // one the "Clusters"

        bool[,] hasBeenSearched_Arr = new bool[mapGenOptions.mapGen_SideLength, mapGenOptions.mapGen_SideLength];
        //int[] = 

        //BiomeCluster[,] biomeCluster_List;





        int oceanSize = 10;


        for (int i = 0; i < mapGenOptions.mapGen_SideLength; i++)
        {
            for (int j = 0; j < oceanSize; j++)
            {
                mapHex_BiomeSets[i, j] = 0;
                mapHex_BiomeSets[j, i] = 0;
                mapHex_BiomeSets[i, mapGenOptions.mapGen_SideLength - (j + 1)] = 0;
                mapHex_BiomeSets[mapGenOptions.mapGen_SideLength - (j + 1), i] = 0;
            }
        }
    }

    private static void HexGenBiome_SmoothMap()
    {
        //Get Best Count of 8 corners to round the value ouit
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenUtility_SetMapSeed(int mapHex_Seed)
    {
        Random.InitState(mapHex_Seed);
    }

    private static void HexGenUtility_MatsAndColors()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_MatIDSets.GetLength(0); x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_MatIDSets.GetLength(1); y++)
            {
                //Get The Biome Info Cell based off of the biome in the given cell
                BiomeCellInfo biomeCellInfo = mapGenOptions.allBiomes_Arr[mapHex_BiomeSets[x, y]].GetRandomBiomeCell();

                //Set The MatID / Color Sets
                mapHex_MatIDSets[x, y] = biomeCellInfo.matID;
                mapHex_ColorSets[x, y] = ColorUtility.ToHtmlStringRGB(biomeCellInfo.gradient.Evaluate(Random.Range(0, 1f)));
            }
        }
    }

    private static void HexGenUtility_MergeRawDataToHexDataCells()
    {
        //Spawn Cells By X (Left and Right)
        for (int y = 0; y < dataHexCells_Arr.GetLength(1); y++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int x = 0; x < dataHexCells_Arr.GetLength(1); x++)
            {
                //Store The Data Collected From Other Methodsit
                dataHexCells_Arr[x, y] = new HexCell_Data
                {
                    //Flip Here, The For Loops Generate Backwards
                    hexCoords = new HexCoords(x, y, mapHex_HeightSets[x, y], 0),
                    hexCell_BiomeID = mapHex_BiomeSets[x, y],
                    hexCell_Color = mapHex_ColorSets[x, y],
                    hexCell_MatID = mapHex_MatIDSets[x, y]
                };
            }
        }
    }

    /////////////////////////////////////////////////////////////////
}