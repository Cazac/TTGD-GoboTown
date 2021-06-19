using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexSectorCoords 
{
    ////////////////////////////////

    public readonly int x;
    public readonly int y;

    /////////////////////////////////////////////////////////////////

    public HexSectorCoords(int incomingX, int incomingY)
    {
        x = incomingX;
        y = incomingY;
    }

    /////////////////////////////////////////////////////////////////

    public string GetPrintableCoords()
    {
        string hexCoordInfo_X = "X: " + x;
        string hexCoordInfo_Y = "Y: " + y;

        return "SECTOR Coords: (" + hexCoordInfo_X + ", " + hexCoordInfo_Y + ")";
    }

    /////////////////////////////////////////////////////////////////
}
