using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSector
{
    ////////////////////////////////
    
    public HexSectorCoords sectorCoords;

    [Header("BLANKVAHR")]
    public Dictionary<HexCellCoords, HexCell_Data> HexCellsData_Dict;
    public Dictionary<HexChunkCoords, HexChunk> hexChunks_Dict;
    //public HexCell_Data[,] DataOnly_HexCells_Arr;
    //public HexChunk[,] hexChunks_Arr;
   
    ////////////////////////////////
}
