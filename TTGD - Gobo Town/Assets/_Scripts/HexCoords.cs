using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HexCoords
{
    ////////////////////////////////

    [SerializeField]
    private int x;
    [SerializeField]
    private int y;
    [SerializeField]
    private int h;
    [SerializeField]
    private int l;

    ////////////////////////////////

    public int X { get { return x; } }
    public int Y { get { return y; } }
    public int Hsteps { get { return h; } }
    public int L { get { return l; } }

    /////////////////////////////////////////////////////////////////

    public static HexCoords GenerateCoords(int incomingX, int incomingY, int incomingH)
    {
        //Create and Return The New Object
        return new HexCoords(incomingX, incomingY, incomingH);
    }

    public HexCoords(int incomingX, int incomingY, int incomingH)
    {
        //Set New Values
        x = incomingX;
        y = incomingY;
        h = incomingH;

        //Not Used Yet
        l = 0;
    }

    /////////////////////////////////////////////////////////////////

    public string GetPrintableCoords()
    {
        string hexCoordInfo_X = "X: " + x;
        string hexCoordInfo_Y = "Y: " + y;
        string hexCoordInfo_Z = "H: " + h;
        string hexCoordInfo_L = "L: " + l;

        return "Hex Coords: (" + hexCoordInfo_X + ", " + hexCoordInfo_Y + ", " + hexCoordInfo_Z + ", " + hexCoordInfo_L + ")";
    }

    /////////////////////////////////////////////////////////////////
}