using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexCell_Data
{
    [Header("Saved Data")]
    public int hexCell_BiomeID;
    public int hexCell_MatID;
    public string hexCell_Color;
    public int hexCell_heightStep;

    public List<HexDecoration_Data> hexCellDecorations_List;
    //public List<HexObject_Data> hexCellObjects_List;
    //public List<HexBuilding_Data> hexCellObjects_List;
    //public List<HexCreature_Data> hexCellObjects_List;

}
