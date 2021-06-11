using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ChunkCoords
{
    ////////////////////////////////

    public readonly int x;
    public readonly int y;

    /////////////////////////////////////////////////////////////////

    public ChunkCoords(int incomingX, int incomingY)
    {
        x = incomingX;
        y = incomingY;
    }

    /////////////////////////////////////////////////////////////////

    public string GetPrintableCoords()
    {
        string hexCoordInfo_X = "X: " + x;
        string hexCoordInfo_Y = "Y: " + y;

        return "Hex Coords: (" + hexCoordInfo_X + ", " + hexCoordInfo_Y + ")";
    }

    /////////////////////////////////////////////////////////////////
}