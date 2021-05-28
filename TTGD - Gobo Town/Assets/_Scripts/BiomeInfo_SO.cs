using UnityEngine;

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
        //Generate A Random Type Color Value
        float randomCellChance = Random.Range(0f, 1f);
        float cumulativeChance = 0f;




        for (int i = 0; i < biomeCellsInfo_Arr.Length; i++)
        {
            cumulativeChance += biomeCellsInfo_Arr[i].totalChance / 100f;

            //Debug.Log("Test Code: " + (i + 1).ToString() + "/" + biomeCellsInfo_Arr.Length + " Chance Roll: " + cumulativeChance + ">=" + randomCellChance);

            if (cumulativeChance >= randomCellChance)
            {
                //Debug.Log("Test Code: Found! (" + i + ")");
                return biomeCellsInfo_Arr[i];
            }
        }

        //Debug.Log("Test Code: Bugged!");

        return biomeCellsInfo_Arr[0];
    }

    /////////////////////////////////////////////////////////////////
}



/*
 * 
 *     public Tuple<BiomeCellInfo, int> GetRandomBiomeCell()
    {
        //Generate A Random Type Color Value
        float randomCellChance = Random.Range(0f, 1f);
        float cumulativeChance = 0f;

        for (int i = 0; i < biomeCellsInfo_Arr.Length; i++)
        {
            cumulativeChance += biomeCellsInfo_Arr[i].totalChance / 100f;

            //Debug.Log("Test Code: " + (i + 1).ToString() + "/" + biomeCellsInfo_Arr.Length + " Chance Roll: " + cumulativeChance + ">=" + randomCellChance);

            if (cumulativeChance >= randomCellChance)
            {
                //Debug.Log("Test Code: Found! (" + i + ")");
                return new Tuple<BiomeCellInfo, int>(biomeCellsInfo_Arr[i], i);
            }
        }

        //Debug.Log("Test Code: Bugged!");

        return new Tuple<BiomeCellInfo, int>(biomeCellsInfo_Arr[0], 0);
    }
*/