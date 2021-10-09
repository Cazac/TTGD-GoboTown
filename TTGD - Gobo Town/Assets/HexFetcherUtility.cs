using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HexFetcherUtility
{
    /////////////////////////////////////////////////////////////////




    public static HexCellCoords GetHexCellCoords_ByWorldPosition(Vector3 searchPosition)
    {
        //Get Camera Position
        Vector3 cameraPos = searchPosition;

        //Get Sector Coords Using Hex Cell Scale
        int cellCoordX = (int)Math.Round(cameraPos.x / MapGenerationOptions_SO.spacing_J, 0);
        int cellCoordY = (int)Math.Round(cameraPos.z / MapGenerationOptions_SO.spacing_I, 0);
        HexCellCoords cellCoords = new HexCellCoords(cellCoordX, cellCoordY);

        //Return The Coords
        return cellCoords;
    }

    /////////////////////////////////////////////////////////////////

    public static List<HexChunkCoords> GetHexChunks_AroundCamera(int X, int Y, int rangeAddition)
    {
        //Create a Returnable List of Values of corect Hexs
        List<HexChunkCoords> chunksCoordsAroundCamera_List = new List<HexChunkCoords>();

        //Get Corners
        int rightCorner = X + rangeAddition + 1;
        int leftCorner = X - rangeAddition;
        int topCorner = Y + rangeAddition + 1;
        int bottomCorner = Y - rangeAddition;

        //Loop The Square From All 4 Corners
        for (int y = bottomCorner; y < topCorner; y++)
        {
            for (int x = leftCorner; x < rightCorner; x++)
            {
                //Add New Possible Chunk
                chunksCoordsAroundCamera_List.Add(new HexChunkCoords(x, y));
            }
        }

        //Return The New List
        return chunksCoordsAroundCamera_List;
    }

    /////////////////////////////////////////////////////////////////

    public static HexChunk GetHexChunk_ByChunk(HexChunkCoords chunkCoords, MapGenerationOptions_SO mapGenOpts_SO, Dictionary<HexSectorCoords, HexSector> hexSectors_Dict)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByChunkCoords(chunkCoords, mapGenOpts_SO, hexSectors_Dict);

        //Return The local Cell
        return hexSectors_Dict[sectorCoords].hexChunks_Dict[chunkCoords];
    }

    public static HexChunk GetHexChunk_ByCell(HexCellCoords cellCoords, MapGenerationOptions_SO mapGenOpts_SO, Dictionary<HexSectorCoords, HexSector> hexSectors_Dict)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByCellCoords(cellCoords, mapGenOpts_SO, hexSectors_Dict);

        //Get The Chunk Coords
        int chunkCoordX = (int)Mathf.Floor((float)cellCoords.x / mapGenOpts_SO.mapGen_ChunkSize);
        int chunkCoordY = (int)Mathf.Floor((float)cellCoords.y / mapGenOpts_SO.mapGen_ChunkSize);
        HexChunkCoords chunkCoords = new HexChunkCoords(chunkCoordX, chunkCoordY);

        //Return The local Cell
        return hexSectors_Dict[sectorCoords].hexChunks_Dict[chunkCoords];
    }

    public static HexChunkCoords GetHexChunkCoords_ByCell(HexCellCoords cellCoords, MapGenerationOptions_SO mapGenOpts_SO, Dictionary<HexSectorCoords, HexSector> hexSectors_Dict)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByCellCoords(cellCoords, mapGenOpts_SO, hexSectors_Dict);

        //Get The Chunk Coords
        int chunkCoordX = (int)Mathf.Floor((float)cellCoords.x / mapGenOpts_SO.mapGen_ChunkSize);
        int chunkCoordY = (int)Mathf.Floor((float)cellCoords.y / mapGenOpts_SO.mapGen_ChunkSize);
        return new HexChunkCoords(chunkCoordX, chunkCoordY);
    }

    /////////////////////////////////////////////////////////////////




    /////////////////////////////////////////////////////////////////

    public static HexCellCoords GetHexCellCoords_TopRightOfSector(HexSectorCoords sectorCoords, MapGenerationOptions_SO mapGenOpts_SO)
    {
        //Get The Coord
        int farthestRightCoord = (sectorCoords.x * mapGenOpts_SO.mapGen_SectorTotalSize) + mapGenOpts_SO.mapGen_SectorTotalSize - 1;
        int farthestTopCoord = (sectorCoords.y * mapGenOpts_SO.mapGen_SectorTotalSize) + mapGenOpts_SO.mapGen_SectorTotalSize - 1;
        HexCellCoords finalHexCellCoord = new HexCellCoords(farthestRightCoord, farthestTopCoord);

        //Return It
        return finalHexCellCoord;
    }

    public static HexCellCoords GetHexCellCoords_TopLeftofSector(HexSectorCoords sectorCoords, MapGenerationOptions_SO mapGenOpts_SO)
    {
        //Get The Coord
        int farthestLeftCoord = (sectorCoords.x * mapGenOpts_SO.mapGen_SectorTotalSize) - mapGenOpts_SO.mapGen_SectorTotalSize - 1;
        int farthestTopCoord = (sectorCoords.y * mapGenOpts_SO.mapGen_SectorTotalSize) + mapGenOpts_SO.mapGen_SectorTotalSize - 1;
        HexCellCoords finalHexCellCoord = new HexCellCoords(farthestLeftCoord, farthestTopCoord);

        //Return It
        return finalHexCellCoord;
    }

    public static HexCellCoords GetHexCellCoords_BottomLeftOfSector(HexSectorCoords sectorCoords, MapGenerationOptions_SO mapGenOpts_SO)
    {
        //Get The Coord
        int farthestLeftCoord = (sectorCoords.x * mapGenOpts_SO.mapGen_SectorTotalSize);
        int farthestBottomCoord = (sectorCoords.y * mapGenOpts_SO.mapGen_SectorTotalSize);
        HexCellCoords finalHexCellCoord = new HexCellCoords(farthestLeftCoord, farthestBottomCoord);

        //Return It
        return finalHexCellCoord;
    }

    public static HexCellCoords GetHexCellCoords_BottomRightOfSector(HexSectorCoords sectorCoords, MapGenerationOptions_SO mapGenOpts_SO)
    {
        //Get The Coord
        int farthestRightCoord = (sectorCoords.x * mapGenOpts_SO.mapGen_SectorTotalSize) + mapGenOpts_SO.mapGen_SectorTotalSize - 1;
        int farthestBottomCoord = (sectorCoords.y * mapGenOpts_SO.mapGen_SectorTotalSize) - mapGenOpts_SO.mapGen_SectorTotalSize - 1;
        HexCellCoords finalHexCellCoord = new HexCellCoords(farthestRightCoord, farthestBottomCoord);

        //Return It
        return finalHexCellCoord;
    }

    /////////////////////////////////////////////////////////////////

    public static HexCell_Data GetHexCellDataLocal_ByCell(HexCellCoords cellCoords, MapGenerationOptions_SO mapGenOpts_SO, Dictionary<HexSectorCoords, HexSector> hexSectors_Dict)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByCellCoords(cellCoords, mapGenOpts_SO, hexSectors_Dict);

        //Return The locally Positioned Cell In the Array of the Sector Then Return The Full Cell
        return hexSectors_Dict[sectorCoords].HexCellsData_Dict[cellCoords];
    }

    public static HexCell_Data[,] GetHexCellDataList_ByChunk(HexChunkCoords chunkCoords, MapGenerationOptions_SO mapGenOpts_SO, Dictionary<HexSectorCoords, HexSector> hexSectors_Dict)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByChunkCoords(chunkCoords, mapGenOpts_SO, hexSectors_Dict);

        //Generate A New Array Of Values To Be Returned
        HexCell_Data[,] returningHexCellDatas_Arr = new HexCell_Data[mapGenOpts_SO.mapGen_ChunkSize, mapGenOpts_SO.mapGen_ChunkSize];

        //Loop all Hexs In Chunk
        for (int y = 0; y < mapGenOpts_SO.mapGen_ChunkSize; y++)
        {
            for (int x = 0; x < mapGenOpts_SO.mapGen_ChunkSize; x++)
            {
                //Get Cell Coords
                int cellX = x + (chunkCoords.x * mapGenOpts_SO.mapGen_ChunkSize);
                int cellY = y + (chunkCoords.y * mapGenOpts_SO.mapGen_ChunkSize);

                //Add Returning Data Cell From Sector Data Cell Array
                returningHexCellDatas_Arr[x, y] = hexSectors_Dict[sectorCoords].HexCellsData_Dict[new HexCellCoords(cellX, cellY)];
            }
        }

        //Return The local Cells
        return returningHexCellDatas_Arr;
    }

    public static HexCellCoords[,] GetHexCellCoordsList_ByChunk(HexChunkCoords chunkCoords, MapGenerationOptions_SO mapGenOpts_SO, Dictionary<HexSectorCoords, HexSector> hexSectors_Dict)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByChunkCoords(chunkCoords, mapGenOpts_SO, hexSectors_Dict);

        //Generate A New Array Of Values To Be Returned
        HexCellCoords[,] returningHexCellDatas_Arr = new HexCellCoords[mapGenOpts_SO.mapGen_ChunkSize, mapGenOpts_SO.mapGen_ChunkSize];

        //Loop all Hexs In Chunk
        for (int y = 0; y < mapGenOpts_SO.mapGen_ChunkSize; y++)
        {
            for (int x = 0; x < mapGenOpts_SO.mapGen_ChunkSize; x++)
            {
                int cellCoordX = x + (chunkCoords.x * mapGenOpts_SO.mapGen_ChunkSize);
                int cellCoordY = y + (chunkCoords.y * mapGenOpts_SO.mapGen_ChunkSize);
                HexCellCoords cellCoords = new HexCellCoords(cellCoordX, cellCoordY);

                //Add Returning Data Cell From Sector Data Cell Array
                returningHexCellDatas_Arr[x, y] = cellCoords;
            }
        }

        //Return The local Cells
        return returningHexCellDatas_Arr;
    }

    /////////////////////////////////////////////////////////////////

    public static HexSectorCoords GetCheckNewSectors_ByCellCoords(HexCellCoords cellCoords, MapGenerationOptions_SO mapGenOpts_SO, Dictionary<HexSectorCoords, HexSector> hexSectors_Dict)
    {
        //Get Sector Coords Using Hex Cell Scale
        int sectorCoordX = (int)Mathf.Floor((float)cellCoords.x / mapGenOpts_SO.mapGen_SectorTotalSize);
        int sectorCoordY = (int)Mathf.Floor((float)cellCoords.y / mapGenOpts_SO.mapGen_SectorTotalSize);
        HexSectorCoords sectorCoords = new HexSectorCoords(sectorCoordX, sectorCoordY);

        //Check If Sector Has Been Generated Before
        if (!hexSectors_Dict.ContainsKey(sectorCoords))
        {
            //Generate The New Sector If Needed
            MapSpawnController.Instance.HexMap_CreateBasicSector(sectorCoords);
        }

        //Return The coords For Other Methods To Use
        return sectorCoords;
    }

    public static HexSectorCoords GetCheckNewSectors_ByChunkCoords(HexChunkCoords chunkCoords, MapGenerationOptions_SO mapGenOpts_SO, Dictionary<HexSectorCoords, HexSector> hexSectors_Dict)
    {
        //Get Sector Coords Using Hex Chunk Scale
        int sectorCoordX = (int)Mathf.Floor((float)chunkCoords.x / mapGenOpts_SO.mapGen_ChunkSize);
        int sectorCoordY = (int)Mathf.Floor((float)chunkCoords.y / mapGenOpts_SO.mapGen_ChunkSize);
        HexSectorCoords sectorCoords = new HexSectorCoords(sectorCoordX, sectorCoordY);

        //Check If Sector Has Been Generated Before
        if (!hexSectors_Dict.ContainsKey(sectorCoords))
        {
            //Generate The New Sector If Needed
            MapSpawnController.Instance.HexMap_CreateBasicSector(sectorCoords);
        }

        //Return The coords For Other Methods To Use
        return sectorCoords;
    }

    /////////////////////////////////////////////////////////////////
}
