using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCellCoords
{
    ////////////////////////////////

    public int x;
    public int y;
    public int l;

    /////////////////////////////////////////////////////////////////

    public HexCellCoords(int incomingX, int incomingY, int incomingL)
    {
        x = incomingX;
        y = incomingY;
        l = incomingL;
    }

    /////////////////////////////////////////////////////////////////

    public string GetPrintableCoords()
    {
        string hexCoordInfo_X = "X: " + x;
        string hexCoordInfo_Y = "Y: " + y;
        string hexCoordInfo_L = "L: " + l;

        return "Hex Coords: (" + hexCoordInfo_X + ", " + hexCoordInfo_Y + ", " + hexCoordInfo_L + ")";
    }

    /////////////////////////////////////////////////////////////////
}