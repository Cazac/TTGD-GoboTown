using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapBiomeGenerator : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject container_GO;

    public List<Material> baseBiomeColors;

    public int[,] hexMap;
    public int startingSize_Row = 5;
    public int startingSize_Column = 5;

    public void Start()
    {
        StartCoroutine(Spawner());

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Creation();
            DestroyArray();
            DisplayArray();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            ZoomOut();
            DestroyArray();
            DisplayArray();
        }


        if (Input.GetKeyDown(KeyCode.M))
        {
            FillZeros();
            DestroyArray();
            DisplayArray();
        }
 

    }

    /////////////////////////////////////////////////////////////////

    public IEnumerator Spawner()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        Creation();
        DestroyArray();
        DisplayArray();

        int loopCount = 4;

        for (int i = 0; i < loopCount; i++)
        {
            //yield return new WaitForSeconds(1.5f);

            ZoomOut();
            FillZeros();
            //DestroyArray();
            //DisplayArray();


        }



        //Finish Counting Timer
        long endingTimeTicks = DateTime.UtcNow.Ticks;
        float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
        Debug.Log("Test Code: Biome Generation x" + loopCount + " Completed in: " + finishTime + "s");
        Debug.Log("Test Code: Size " + hexMap.GetLength(0) + "x" + hexMap.GetLength(1));


        DestroyArray();
        DisplayArray();

        yield break;
    }

    public void DestroyArray()
    {
        //Destory All Old Hex Chunks / Cells
        foreach (Transform child in container_GO.transform)
        {
            //Destroy Top-Level Child
            Destroy(child.gameObject);
        }
    }

    public void DisplayArray()
    {

        for (int i = 0; i < hexMap.GetLength(0); i++)
        {
            for (int j = 0; j < hexMap.GetLength(1); j++)
            {
                GameObject newHex = Instantiate(cubePrefab, new Vector3(i, j + 3, 0), Quaternion.identity, container_GO.transform);
                newHex.GetComponent<MeshRenderer>().material = baseBiomeColors[hexMap[i, j]]; 
            }
        }
    }

    public void Creation()
    {
        hexMap = new int[startingSize_Row, startingSize_Column];


        for (int i = 0; i < startingSize_Row; i++)
        {
            for (int j = 0; j < startingSize_Row; j++)
            {
                hexMap[i, j] = Random.Range(1, 5);
            }
        }
    }


    public void ZoomOut()
    {
        int currentSize_Row = hexMap.GetLength(0);
        int currentSize_Column = hexMap.GetLength(1);
        int[,] newScaleMap_Arr = new int[currentSize_Row * 2, currentSize_Row * 2];


        //Printing Stats - Show Visually Later
        int rowLength = hexMap.GetLength(0);
        int colLength = hexMap.GetLength(1);
        string line = "";

        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                line += "[" + hexMap[i, j] + "] ";
            }
            //Debug.Log(line);
            line = "";
        }


        //FILL 2 VALUE SETS PER LOOP
        for (int i = 0; i < currentSize_Row; i++)
        {
            for (int j = 0; j < currentSize_Column; j++)
            {

                Vector2 newSet_TopLeft = new Vector2(i * 2, j * 2);
                Vector2 newSet_TopRight = new Vector2((i * 2) + 1, j * 2);
                Vector2 newSet_BottomLeft = new Vector2(i * 2, (j * 2) + 1);
                Vector2 newSet_BottomRight = new Vector2((i * 2) + 1, (j * 2) + 1);


                newScaleMap_Arr[(int)newSet_TopLeft.x, (int)newSet_TopLeft.y] = hexMap[i, j];

                newScaleMap_Arr[(int)newSet_TopRight.x, (int)newSet_TopRight.y] = 0;
                newScaleMap_Arr[(int)newSet_BottomLeft.x, (int)newSet_BottomLeft.y] = 0;
                newScaleMap_Arr[(int)newSet_BottomRight.x, (int)newSet_BottomRight.y] = 0;
            }
        }


        hexMap = newScaleMap_Arr;
    }


    public void FillZeros()
    {
        for (int i = 0; i < hexMap.GetLength(0); i++)
        {
            for (int j = 0; j < hexMap.GetLength(1); j++)
            {
               
                if (hexMap[i, j] == 0)
                {




                    //If J is even 
                    if (j % 2 == 0)
                    {
                        //Random FOr Left and Right
                        int rand = Random.Range(0,2);

                        switch (rand)
                        {
                            //Left
                            case 0:
                                if (CheckValidHex(i - 1, j))
                                {
                                    //True Left
                                    hexMap[i, j] = hexMap[i - 1, j];
                                }
                                else
                                {
                                    //Forced Right
                                    hexMap[i, j] = hexMap[i + 1, j];
                                }
                                break;

                            //Right
                            case 1:
                                if (CheckValidHex(i + 1, j))
                                {
                                    //True Right
                                    hexMap[i, j] = hexMap[i + 1, j];
                                }
                                else
                                {
                                    //Forced Left
                                    hexMap[i, j] = hexMap[i - 1, j];
                                }
                                break;
                        }
                    }
                    else if (i % 2 == 0)
                    {
                        //Random For Up and Down
                        int rand = Random.Range(0, 2);

                        switch (rand)
                        {
                            //Down
                            case 0:
                                if (CheckValidHex(i, j - 1))
                                {
                                    //True Down
                                    hexMap[i, j] = hexMap[i, j - 1];
                                }
                                else
                                {
                                    //Forced Up
                                    hexMap[i, j] = hexMap[i, j + 1];
                                }
                                break;

                            //Up
                            case 1:
                                if (CheckValidHex(i, j + 1))
                                {
                                    //True Up
                                    hexMap[i, j] = hexMap[i, j + 1];
                                }
                                else
                                {
                                    //Forced Down
                                    hexMap[i, j] = hexMap[i, j - 1];
                                }
                                break;
                        }
                    }
                    else if (j + 1 == hexMap.GetLength(1))
                    {
                        if (i == 0)
                        {
                            if (CheckValidHex(i + 1, j - 1))
                            {
                                //True Right / Down
                                hexMap[i, j] = hexMap[i + 1, j - 1];
                            }
                            else
                            {
                                //Forced Left / Up
                                hexMap[i, j] = hexMap[i - 1, j + 1];
                            }
                        }
                        else if (i + 1 == hexMap.GetLength(0))
                        {
                            if (CheckValidHex(i + 1, j - 1))
                            {
                                //True Right / Down
                                hexMap[i, j] = hexMap[i + 1, j - 1];
                            }
                            else
                            {
                                //Forced Left / Up
                                hexMap[i, j] = hexMap[i - 1, j + 1];
                            }
                        }

                    }
                    else
                    {
                        //Random For Diagonal Sets
                        int rand = Random.Range(0, 4);

                        switch (rand)
                        {
                            //Left / Down
                            case 0:
                                if (CheckValidHex(i - 1, j - 1))
                                {
                                    //True Left / Down
                                    hexMap[i, j] = hexMap[i - 1, j - 1];
                                }
                                else
                                {
                                    //Forced Right / Up
                                    hexMap[i, j] = hexMap[i + 1, j + 1];
                                }
                                break;

                            //Right / Up
                            case 1:
                                if (CheckValidHex(i + 1, j + 1))
                                {
                                    //True Right / Up
                                    hexMap[i, j] = hexMap[i + 1, j + 1];
                                }
                                else
                                {
                                    //Forced Left / Down
                                    hexMap[i, j] = hexMap[i - 1, j - 1];
                                }
                                break;

                            //Left / Up
                            case 2:
                                if (CheckValidHex(i - 1, j + 1))
                                {
                                    //True Up
                                    hexMap[i, j] = hexMap[i - 1, j + 1];
                                }
                                else
                                {
                                    //Forced Down
                                    hexMap[i, j] = hexMap[i + 1, j - 1];
                                }
                                break;

                            //Right / Down
                            case 3:
                                if (CheckValidHex(i + 1, j - 1))
                                {
                                    //True Right / Down
                                    hexMap[i, j] = hexMap[i + 1, j - 1];
                                }
                                else
                                {
                                    //Forced Left / Up
                                    hexMap[i, j] = hexMap[i - 1, j + 1];
                                }
                                break;


                        }
                    }

                   



                    //switch ()
                    {

                    }
                    //Get random direction

                    //Check if Valid

                    //Take Value

                }
            }
        }

    }


    public bool CheckValidHex(int i, int j)
    {
        if (i < 0 || j < 0)
        {
            return false;
        }

        if (hexMap.GetLength(0) < i + 1)
        {
            return false;
        }

        if (hexMap.GetLength(1) < j + 1)
        {
            return false;
        }


        return true;
    }


    public void SmoothMap()
    {
        //Get Best Count of 8 corners to round the value ouit
    }


    /////////////////////////////////////////////////////////////////


    /*
    public void Enhance()
    {
        Dictionary<Vector2, bool> NewMap = new Dictionary<Vector2, bool>();
        int newXSize = 2 * (xSize - 1);
        int newYSize = 2 * (ySize - 1);



        for (int x = 0; x < xSize - 1; x++)
        {
            for (int y = 0; y < ySize - 1; y++)
            {
                KeyValuePair<Vector2, bool> a = Map.First(p => p.Key == new Vector2(x, y + 1));
                KeyValuePair<Vector2, bool> b = Map.First(p => p.Key == new Vector2(x + 1, y + 1));
                KeyValuePair<Vector2, bool> c = Map.First(p => p.Key == new Vector2(x, y));
                KeyValuePair<Vector2, bool> d = Map.First(p => p.Key == new Vector2(x + 1, y));
                //set in stone 
                int newX = 2 * x;
                int newY = 2 * y;


                if (!NewMap.ContainsKey(new Vector2(newX, newY)))
                    NewMap.Add(new Vector2(newX, newY), c.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 2, newY)))
                    NewMap.Add(new Vector2(newX + 2, newY), d.Value);
                if (!NewMap.ContainsKey(new Vector2(newX, newY + 2)))
                    NewMap.Add(new Vector2(newX, newY + 2), a.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 2, newY + 2)))
                    NewMap.Add(new Vector2(newX + 2, newY + 2), b.Value);

                //a or b
                KeyValuePair<Vector2, bool> newd = new KeyValuePair<Vector2, bool>(new Vector2(newX + 1, newY), Random.Range(0, 2) == 0 ? c.Value : d.Value);
                KeyValuePair<Vector2, bool> newa = new KeyValuePair<Vector2, bool>(new Vector2(newX + 1, newY + 2), Random.Range(0, 2) == 0 ? a.Value : b.Value);
                KeyValuePair<Vector2, bool> newb = new KeyValuePair<Vector2, bool>(new Vector2(newX, newY + 1), Random.Range(0, 2) == 0 ? a.Value : c.Value);
                KeyValuePair<Vector2, bool> newc = new KeyValuePair<Vector2, bool>(new Vector2(newX + 2, newY + 1), Random.Range(0, 2) == 0 ? b.Value : d.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 1, newY)))
                    NewMap.Add(newd.Key, newd.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 1, newY + 2)))
                    NewMap.Add(newa.Key, newa.Value);
                if (!NewMap.ContainsKey(new Vector2(newX, newY + 1)))
                    NewMap.Add(newb.Key, newb.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 2, newY + 1)))
                    NewMap.Add(newc.Key, newc.Value);
                //center
                bool val = true;
                if (newa.Value == newd.Value && newc.Value == newb.Value)
                    val = Random.Range(0, 2) == 0 ? newa.Value : newb.Value;
                else if (newa.Value == newd.Value)
                    val = newa.Value;
                else if (newb.Value == newc.Value)
                    val = newb.Value;
                else
                    val = new List<bool>() { newa.Value, newb.Value, newc.Value, newd.Value }[Random.Range(0, 4)];
                NewMap.Add(new Vector2(newX + 1, newY + 1), val);
            }
        }




        Dictionary<Vector2, bool> newmapForUpdate = new Dictionary<Vector2, bool>();

        foreach (var pair in NewMap)
            newmapForUpdate.Add(pair.Key, pair.Value);
        foreach (var pair in newmapForUpdate)
            if (pair.Key.x == 0 || pair.Key.x == newXSize - 1 || pair.Key.y == 0 || pair.Key.y == newYSize)
                NewMap[pair.Key] = false;
        xSize = newXSize;
        ySize = newYSize;
        Map = NewMap;

    }

    */
}
