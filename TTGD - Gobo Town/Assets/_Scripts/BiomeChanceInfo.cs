using UnityEngine;
using System;

[Serializable]
public class BiomeChanceInfo 
{
    ////////////////////////////////

    public BiomeInfo_SO biomeInfo_SO;
    [Range(0, 10)]
    public float weight = 1;
    public float totalChance;

    ////////////////////////////////
}
