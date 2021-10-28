using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MapGenLayerBorders
{
    ////////////////////////////////

    [Header("Sector Generation Info Sets")]
    private static int[,] mapHex_BiomeSets;

    [Header("Map Generation Options")]
    private static MapGenerationOptions_SO mapGenOpts;

    /////////////////////////////////////////////////////////////////

    public static int[,] GenerateSectorValues(MapGenerationOptions_SO incomingMapOptions, HexSectorCoords sectorCoords, int[,] incomingBiomeSets)
    {
        //Set Options
        mapGenOpts = incomingMapOptions;
        mapHex_BiomeSets = incomingBiomeSets;


        HexGenBorderLayer_Basic();


        //Return Biome Map Array
        return mapHex_BiomeSets;
    }

    /////////////////////////////////////////////////////////////////


    private static void HexGenBorderLayer_Basic()
    {
        int borderSize = 3;





        //mapGenOpts


    }



    private static void HexGenBorderLayer_LeftLong()
    {
        int borderSize = 3;





        //mapGenOpts.mapGen_SectorTotalSize

            //Array of [0,0,0], [0,0,0], [0,0,0], [0,0,0]


    }





    /////////////////////////////////////////////////////////////////



    private void HexGen_FillEmptiedChunks_Right(HexSectorCoords sectorCoords_Center, HexSectorCoords sectorCoords_Right)
    {

             /*
        Debug.Log("Test Code: Generating Chained Sector (Right)");

        //Get Basic Calculated Info
        int chunksPerSector = mapGenOpts_SO.mapGen_SectorTotalSize / mapGenOpts_SO.mapGen_ChunkSize;
        int offset = chunksPerSector - 1;

        //Set The Flag That Neighbours have been Chained
        hexSectors_Dict[sectorCoords_Center].hasGeneratedNeighbours = true;


        Debug.Log("Test Code: " + sectorCoords_Right.GetPrintableCoords());


        HexCellCoords sectorCenter_TopRightCellCoords = GetHexCellCoords_TopRightOfSector(sectorCoords_Center);
        //HexCellCoords sectorCenter_BottomRightCellCoord = GetHexCellCoords_BottomRightOfSector(sectorCoords_Center);

        //HexCellCoords sectorRight_TopLeftCellCoord = GetHexCellCoords_TopRightOfSector(sectorCoords_Right);
        HexCellCoords sectorRight_BottomLeftCellCoords = GetHexCellCoords_BottomLeftOfSector(sectorCoords_Right);



        //Debug.Log("Test Code: Top Right: " + sectorCenter_TopRightCellCoords.GetPrintableCoords());
        //Debug.Log("Test Code: Pre Bottom Left " + sectorRight_BottomLeftCellCoords.GetPrintableCoords());

        //Bonus Size From THe Horizontal Style
        int verticalOffset = (mapGenOpts_SO.mapGen_EmptiedChunkBorderSize + 1) * mapGenOpts_SO.mapGen_ChunkSize;
        int horizontalOffset = mapGenOpts_SO.mapGen_EmptiedChunkBorderSize * mapGenOpts_SO.mapGen_ChunkSize;

        //Add Offsets
        sectorCenter_TopRightCellCoords = new HexCellCoords(sectorCenter_TopRightCellCoords.x - horizontalOffset, sectorCenter_TopRightCellCoords.y - verticalOffset);
        sectorRight_BottomLeftCellCoords = new HexCellCoords(sectorRight_BottomLeftCellCoords.x + horizontalOffset, sectorRight_BottomLeftCellCoords.y + verticalOffset);


        Debug.Log("Test Code: Top Right: " + sectorCenter_TopRightCellCoords.GetPrintableCoords());
        Debug.Log("Test Code: Bottom Left " + sectorRight_BottomLeftCellCoords.GetPrintableCoords());


        HexCellCoords[,] hexCellCoordsBetween = GetHexCellCoordsArr_FromXToY(sectorCenter_TopRightCellCoords, sectorRight_BottomLeftCellCoords);


        Debug.Log("Test Code: " + hexCellCoordsBetween.GetLength(0));
        Debug.Log("Test Code: " + hexCellCoordsBetween.GetLength(1));


        for (int y = 0; y < hexCellCoordsBetween.GetLength(1); y++)
        {
            for (int x = 0; x < hexCellCoordsBetween.GetLength(0); x++)
            {


                //Store The Data Collected From Other Methodsit
                HexCell_Data mergedCell = new HexCell_Data
                {
                    hexCoords = new HexCellCoords(hexCellCoordsBetween[x, y].x, hexCellCoordsBetween[x, y].y),
                    hexCell_HeightSteps = 5,
                    hexCell_BiomeID = 0,
                    hexCell_Color = "#ffffff",
                    hexCell_MatID = 0
                };

                Debug.Log("Test Code: Adding Cell " + mergedCell.hexCoords.GetPrintableCoords());

                //Get Sector Coords Using Hex Cell Scale
                int sectorCoordX = (int)Mathf.Floor((float)mergedCell.hexCoords.x / mapGenOpts_SO.mapGen_SectorTotalSize);
                int sectorCoordY = (int)Mathf.Floor((float)mergedCell.hexCoords.y / mapGenOpts_SO.mapGen_SectorTotalSize);
                HexSectorCoords sectorCoords = new HexSectorCoords(sectorCoordX, sectorCoordY);

                hexSectors_Dict[sectorCoords_Center].HexCellsData_Dict[mergedCell.hexCoords] = mergedCell;
            }
        }












        MapGenerationController.HexGenEmpitedChunks_MergeHorrizontal();




        HexCellCoords[] influenceRowCenter_Arr = new HexCellCoords[mapGenOpts_SO.mapGen_SectorTotalSize];
        HexCellCoords[] influenceRowTop_Arr = new HexCellCoords[mapGenOpts_SO.mapGen_SectorTotalSize];









        /*
        //Center
        foreach (HexChunkCoords chunkCoords in chunksCoordsCenter_List)
        {
            HexCellCoords[,] dataCellsToBeLoaded_Arr = GetHexCellCoordsList_ByChunk(chunkCoords);


            for (int y = 0; y < dataCellsToBeLoaded_Arr.GetLength(0); y++)
            {
                for (int x = 0; x < dataCellsToBeLoaded_Arr.GetLength(1); x++)
                {
                    //Store The Data Collected From Other Methodsit
                    HexCell_Data mergedCell = new HexCell_Data
                    {
                        hexCoords = new HexCellCoords(dataCellsToBeLoaded_Arr[x, y].x, dataCellsToBeLoaded_Arr[x, y].y),
                        hexCell_HeightSteps = 5,
                        hexCell_BiomeID = 0,
                        hexCell_Color = "#ffffff",
                        hexCell_MatID = 0
                    };

                    hexSectors_Dict[sectorCoords_Center].HexCellsData_Dict[mergedCell.hexCoords] = mergedCell;
                }
            }
        }


        //Top
        foreach (HexChunkCoords chunkCoords in chunksCoordsTop_List)
        {
            HexCellCoords[,] dataCellsToBeLoaded_Arr = GetHexCellCoordsList_ByChunk(chunkCoords);


            for (int y = 0; y < dataCellsToBeLoaded_Arr.GetLength(0); y++)
            {
                for (int x = 0; x < dataCellsToBeLoaded_Arr.GetLength(1); x++)
                {
                    //Store The Data Collected From Other Methodsit
                    HexCell_Data mergedCell = new HexCell_Data
                    {
                        hexCoords = new HexCellCoords(dataCellsToBeLoaded_Arr[x, y].x, dataCellsToBeLoaded_Arr[x, y].y),
                        hexCell_HeightSteps = 5,
                        hexCell_BiomeID = 0,
                        hexCell_Color = "#ffffff",
                        hexCell_MatID = 0
                    };

                    hexSectors_Dict[sectorCoords_Top].HexCellsData_Dict[mergedCell.hexCoords] = mergedCell;
                }
            }
        }
        


    */


    }
 


    /////////////////////////////////////////////////////////////////
}
