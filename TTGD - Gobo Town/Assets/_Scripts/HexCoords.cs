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
    private int z;
    [SerializeField]
    private int l;

    public int X { get { return x; } }
    public int Y { get { return y; } }
    public int Z { get { return z; } }
    public int L { get { return l; } }

    /////////////////////////////////////////////////////////////////

    public static HexCoords GenerateCoords(int x, int y)
    {
        return new HexCoords(x, y);
    }

    [HideInInspector]
    public HexCoords(int x, int y)
    {
        this.x = x;
        this.y = y;
        z = 0;
        l = 0;
    }

    public string GetPrintableCoords()
    {
        string hexCoordInfo_X = "X: " + x;
        string hexCoordInfo_Y = "Y: " + y;
        string hexCoordInfo_Z = "Z: " + z;
        string hexCoordInfo_L = "Level: " + l;

        return "Hex Coords: (" + hexCoordInfo_X + " - " + hexCoordInfo_Y + " - " + hexCoordInfo_Z + " - " + hexCoordInfo_L + ")";
    }

    /////////////////////////////////////////////////////////////////
}