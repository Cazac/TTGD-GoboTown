using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSector
{
    ////////////////////////////////
    
    [Header("Sector Coords")]
    public HexSectorCoords sectorCoords;

    [Header("BLANKVAHR")]
    public Dictionary<HexCellCoords, HexCell_Data> HexCellsData_Dict;
    public Dictionary<HexChunkCoords, HexChunk> hexChunks_Dict;

    [Header("BLANKVAR")]
    public bool hasGeneratedNeighbours;
   
    ////////////////////////////////
}
