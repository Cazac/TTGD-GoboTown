using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MapGenLayerHeight
{
    ////////////////////////////////

    [Header("Sector Generation Info Sets")]
    private static int[,] mapHex_BiomeSets;
    private static int[,] mapHex_HeightSets;

    [Header("Map Generation Options")]
    private static MapGenerationOptions_SO mapGenOpts;

    /////////////////////////////////////////////////////////////////

    public static int[,] GenerateSectorValues(MapGenerationOptions_SO incomingMapOptions, int[,] incomingBiomeSets)
    {
        //Set Options
        mapGenOpts = incomingMapOptions;
        mapHex_BiomeSets = incomingBiomeSets;

        //Run Biome Generation
        HexGenHeight();

        //Return Biome Map Array
        return mapHex_HeightSets;
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenHeight()
    {
        //Set New height array
        mapHex_HeightSets = new int[mapGenOpts.mapGen_SectorTotalSize, mapGenOpts.mapGen_SectorTotalSize];

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

                    //4 == Swamp
                    case 4:
                        HexGenHeight_Perlin_Plains(x, y);
                        break;

                    //??? == ???
                    default:
                        break;
                }
            }
        }
    }

    /////////////////////////////////////////////////////////////////

    private static void HexGenHeight_Flat_Ocean(int x, int y)
    {
        //Create a Height Steps value based off of closeset step to the "Real" Height
        int heightSteps = 6;

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
}
