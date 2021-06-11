using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCoords
{
    ////////////////////////////////

    public int x;
    public int y;
    public int hSteps;
    public int l;

    /////////////////////////////////////////////////////////////////

    public HexCoords(int incomingX, int incomingY, int incomingH, int incomingL)
    {
        x = incomingX;
        y = incomingY;
        hSteps = incomingH;
        l = incomingL;
    }

    /////////////////////////////////////////////////////////////////

    /*
public static HexCoords GenerateCoords(int incomingX, int incomingY, int incomingH, int incomingL)
{
    //Create and Return The New Object
    return new HexCoords
    {
        x = incomingX,
        y = incomingY,
        hSteps = incomingH,
        l = incomingL
    };
}
*/

    /////////////////////////////////////////////////////////////////

    public string GetPrintableCoords()
    {
        string hexCoordInfo_X = "X: " + x;
        string hexCoordInfo_Y = "Y: " + y;
        string hexCoordInfo_Z = "H: " + hSteps;
        string hexCoordInfo_L = "L: " + l;

        return "Hex Coords: (" + hexCoordInfo_X + ", " + hexCoordInfo_Y + ", " + hexCoordInfo_Z + ", " + hexCoordInfo_L + ")";
    }

    /////////////////////////////////////////////////////////////////
}