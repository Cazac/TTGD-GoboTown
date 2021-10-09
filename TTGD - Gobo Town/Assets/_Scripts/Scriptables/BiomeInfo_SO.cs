using System;
using UnityEngine;
using Random = UnityEngine.Random;

///////////////
/// <summary>
///     
/// BiomeInfo_SO is used as a "ScriptableObject" the holds the info of the biome cells and can choose a random one for the player using "GetRandomBiomeCell()".
/// 
/// </summary>
///////////////

[CreateAssetMenu(fileName = "New BiomeInfo", menuName = "Scriptables/Biome Info")]
public class BiomeInfo_SO : ScriptableObject
{
    ////////////////////////////////

    [Header("Basic Info")]
    public string biomeName;
    public int biomeID;

    [Header("Cell Info Sets")]
    public BiomeCellInfo[] biomeCellsInfo_Arr;

    /////////////////////////////////////////////////////////////////

    public BiomeCellInfo GetRandomBiomeCell()
    {
        //Generate A Random Type Biome Cell Value
        float randomCellChance = Random.Range(0f, 1f);
        float cumulativeChance = 0f;

        //For each avalible Biome Cell loop till the chance is met
        for (int i = 0; i < biomeCellsInfo_Arr.Length; i++)
        {
            //Add To The cumultive chance
            cumulativeChance += biomeCellsInfo_Arr[i].totalChance / 100f;

            //Check if Cumulative Chance passes the randomized value
            if (cumulativeChance >= randomCellChance)
            {
                //Chance has been accepted, Return the Biome Cell Type
                return biomeCellsInfo_Arr[i];
            }
        }

        //Oh no! A biome should have been found!
        throw new NotImplementedException();
    }

    /////////////////////////////////////////////////////////////////
}