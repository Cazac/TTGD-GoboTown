using UnityEngine;
using System;

[Serializable]
public class BiomeCellInfo 
{
    ////////////////////////////////

    public int matID;
    public Gradient gradient;
    public Material material;
    [Range(0,10)]
    public float weight = 1;
    public float totalChance;

    ////////////////////////////////
}
