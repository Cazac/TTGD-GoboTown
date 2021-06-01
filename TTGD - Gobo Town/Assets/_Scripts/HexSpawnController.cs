using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexSpawnController : MonoBehaviour
{
    ////////////////////////////////

    [Header("Hex Scripts Storage")]
    [SerializeField]
    private HexCell_Data[,] dataHexCells_Arr;
    [SerializeField]
    private HexCell[,] allHexsCells_Arr;
    [SerializeField]
    private HexChunk[,] allHexChunks_Arr;

    ////////////////////////////////

    [Header("Camera Options")]
    public Camera cameraGenerated;
    public Vector2 cameraRelativePosition;

    [Header("Hex Map Options")]
    public bool isSlowSpawning;
    public bool isShowingGenerationTime;
    public bool isGeneratingMap;

    [Header("Hex Map Containers")]
    public GameObject scalingGround_GO;
    public GameObject hexMapContainer_GO;

    [Header("Hex Map Prefabs")]
    public GameObject hexMesh_Prefab;
    public GameObject hexChunk_Prefab;
    public GameObject hexChunkModel_Prefab;

    [Header("Decoration Prefabs")]
    public GameObject hexGrass_Prefab;

    ////////////////////////////////

    [Header("Hex Uneditable Sizes")]
    public const float outerRadius = 0.1f;
    public const float innerRadius = outerRadius * 0.866025404f;
    private const float spacing_I = innerRadius * 2f;
    private const float spacing_J = outerRadius * 1.5f;
    private const float offcenter_I = spacing_I / 2;

    [Header("Hex Map Settings (Sizing)")]
    private int mapHexGeneration_BiomeGrowthLoopCount;
    public int mapHexGeneration_BiomeStartCount = 20;
    public int mapHexGeneration_SideLength = 640;
    public int mapHex_RowCount = 100;
    public int mapHex_ColumnCount = 100;
    public int mapHex_ChunkSize = 10;

    [Header("Hex Map Settings (Height)")]
    public int mapHeightMin = 0;
    public int mapHeightMax = 5;
    public float mapHeightStep = 0.025f;

    [Header("Hex Map Settings (RNG)")]
    public int mapHex_Seed = 135135;

    [Header("Biome Info Sets")]
    public BiomeInfo_SO biomeInfo_Plains;
    public BiomeInfo_SO biomeInfo_Forest;

    public static Material[,] mergedBiomeMats_Arr;

    [Header("Randomization States To Be Used")]
    private Random.State mapGeneration_SeededStated; //= Random.state;

    [HideInInspector]
    public SaveFile mySaveFile;

    private int biomeMaterialCap = 10;



    public GameObject biomeVisualQuad_Prefab;
    public GameObject biomeVisualContainer_GO;

    public List<Material> biomeVisualColors_List;

    public int[,] hexMap;


    /////////////////////////////////////////////////////////////////

    private void Start()
    {
        /*
        //Check For Issues Before Spawning
        Tuple<bool, string> results = ErrorChecker();
        if (!results.Item1)
        {
            Debug.Log("Test Code: Error Check Failed (" + results.Item2 + ")");
          
        */



        if (isGeneratingMap)
        {
            //Spawn The Visual Biome Set
            HexGeneration_Spawn();
        }
        else
        {
            //Spawn All Of the Hex Map
            HexMap_Spawn();
        }
    }

    private void Update()
    {
        ////////////////////////////////

        if (Input.GetKeyDown(KeyCode.R))
        {
            mapHex_Seed = Random.Range(100000, 999999);
            HexMap_SetMapSeed();
            HexMap_Spawn();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            HexMap_Unchunk();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            HexMap_Rechunk();
        }

        ////////////////////////////////

        if (Input.GetKeyDown(KeyCode.O))
        {
            HexMap_CreateSave();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            HexMap_LoadSave();
        }

        ////////////////////////////////

        if (Input.GetKeyDown(KeyCode.T))
        {
            HexGeneration_Spawn();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            HexGeneration_ZoomOutBiome();
            HexVisuals_DestroyVisuals();
            HexVisuals_DisplayVisuals();
        }


        if (Input.GetKeyDown(KeyCode.M))
        {
            HexGeneration_FillZeros();
            HexVisuals_DestroyVisuals();
            HexVisuals_DisplayVisuals();
        }

        ////////////////////////////////

        if (Input.GetMouseButtonDown(0))
        {
            MouseInput_Click();
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexGeneration_Spawn()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;


        mapHexGeneration_BiomeGrowthLoopCount = (int) Mathf.Log((float)mapHexGeneration_SideLength / mapHexGeneration_BiomeStartCount, 2);


        //Create Base Display
        HexGeneration_Creation();

 
        for (int i = 0; i < mapHexGeneration_BiomeGrowthLoopCount; i++)
        {
            //Zoom Then Fill To Expand Map
            HexGeneration_ZoomOutBiome();
            HexGeneration_FillZeros();

            //Need Smoothing
        }

        //These Converge into lakes at the end 
        //HexGeneration_PostGeneration_Rivers();
        //HexGeneration_PostGeneration_Lakes();
        //HexGeneration_PostGeneration_Beaches();
        //HexGeneration_PostGeneration_InterBiomes();
        HexGeneration_PostGeneration_Ocean();

        if (isShowingGenerationTime)
        {
            //Finish Counting Timer
            long endingTimeTicks = DateTime.UtcNow.Ticks;
            float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
            Debug.Log("Test Code: Biome Generation x" + mapHexGeneration_BiomeGrowthLoopCount + " Completed in: " + finishTime + "s");
            Debug.Log("Test Code: Size " + hexMap.GetLength(0) + "x" + hexMap.GetLength(1));
        }

  

        //Show the Map Visually
        HexVisuals_DestroyVisuals();
        HexVisuals_DisplayVisuals();
    }



    private void HexMap_Spawn()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Create New Arrays
        dataHexCells_Arr = new HexCell_Data[mapHex_RowCount, mapHex_ColumnCount];
        allHexsCells_Arr = new HexCell[mapHex_RowCount, mapHex_ColumnCount];
        allHexChunks_Arr = new HexChunk[(int)Mathf.Ceil(mapHex_RowCount / mapHex_ChunkSize), (int)Mathf.Ceil(mapHex_ColumnCount / mapHex_ChunkSize)];

        //Generate Map
        HexMap_SetMapSeed();
        HexMap_GenerateAllHexCells();



        //Spawn Map



        HexMap_SetupMatsArray();
        HexMap_SpawnGround();
        HexMap_RemoveOldMap();
        HexMap_SpawnAllHexChunks();
        HexMap_SpawnAllHexCells();
        HexMap_CenterCamera();
        HexMap_ColorCellsOnMap();
        HexMap_RandomizeHeight();
        HexMap_StoreAllHexCells();
        HexMap_Chunk();
        HexMap_SpawnDecoration();

        if (isShowingGenerationTime)
        {
            //Finish Counting Timer
            long endingTimeTicks = DateTime.UtcNow.Ticks;
            float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
            Debug.Log("Test Code: Map Generation Completed in: " + finishTime + "s");
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_GenerateAllHexCells()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_RowCount; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_ColumnCount; y++)
            {
                //Create Gameobject And Find Chunk
                //HexCell_Data hexCellData = 


                //hexCellData.hexCell_BiomeID = 0;
                //hexCellData.hexCell_Color = "";
                //hexCellData.hexCell_MatID = 0;
                //hexCellData.hexCell_heightStep = 0;

                //Store it
                dataHexCells_Arr[x, y] = new HexCell_Data();
            }
        }
    }

    private void HexMap_GenerateCellColors()
    {

    }




    private void HexGeneration_Creation()
    {
        hexMap = new int[mapHexGeneration_BiomeStartCount, mapHexGeneration_BiomeStartCount];


        for (int i = 0; i < mapHexGeneration_BiomeStartCount; i++)
        {
            for (int j = 0; j < mapHexGeneration_BiomeStartCount; j++)
            {
                hexMap[i, j] = Random.Range(1, 3);
            }
        }
    }

    private void HexGeneration_ZoomOutBiome()
    {
        int currentSize_Row = hexMap.GetLength(0);
        int currentSize_Column = hexMap.GetLength(1);
        int[,] newScaleMap_Arr = new int[currentSize_Row * 2, currentSize_Row * 2];



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

    private void HexGeneration_FillZeros()
    {
        //Foreach Each Set Of Hexs Fill All 0s
        for (int i = 0; i < hexMap.GetLength(0); i++)
        {
            for (int j = 0; j < hexMap.GetLength(1); j++)
            {
                //Check if current value is 0
                if (hexMap[i, j] == 0)
                {
                    //Check Current Status of I / J to determine where the value comes from
                    if (j % 2 == 0)
                    {
                        //J is an Even Value
                        HexGeneration_FillZeros_JEven(i, j);
                    }
                    else if (i % 2 == 0)
                    {
                        //I is an Even Value
                        HexGeneration_FillZeros_IEven(i, j);
                    }
                    else if (j + 1 == hexMap.GetLength(1))
                    {
                        //J is at Max Value
                        HexGeneration_FillZeros_JMaxed(i, j);
                    }
                    else
                    {
                        //Both Values are Odd / Use diagonals
                        HexGeneration_FillZeros_Diagonals(i, j);
                    }
                }
            }
        }

    }

    private void HexGeneration_FillZeros_JEven(int i, int j)
    {
        //Random FOr Left and Right
        int rand = Random.Range(0, 2);

        switch (rand)
        {
            //Left
            case 0:
                if (HexGeneration_CheckValidHexSpace(i - 1, j))
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
                if (HexGeneration_CheckValidHexSpace(i + 1, j))
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

    private void HexGeneration_FillZeros_IEven(int i, int j)
    {
        //Random For Up and Down
        int rand = Random.Range(0, 2);

        switch (rand)
        {
            //Down
            case 0:
                if (HexGeneration_CheckValidHexSpace(i, j - 1))
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
                if (HexGeneration_CheckValidHexSpace(i, j + 1))
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

    private void HexGeneration_FillZeros_JMaxed(int i, int j)
    {
        if (i == 0)
        {

            //True Right / Down
            hexMap[i, j] = hexMap[i + 1, j - 1];

            return;

            if (HexGeneration_CheckValidHexSpace(i + 1, j - 1))
            {

            }
            else
            {
                //Forced Left / Up
                hexMap[i, j] = hexMap[i - 1, j + 1];
            }
        }
        else if (i + 1 == hexMap.GetLength(0))
        {

            //True Left / Down
            hexMap[i, j] = hexMap[i - 1, j - 1];

            return;

            if (HexGeneration_CheckValidHexSpace(i + 1, j - 1))
            {
                //True Right / Up
                hexMap[i, j] = hexMap[i + 1, j - 1];
            }
            else
            {
                //Forced Left / Up
                hexMap[i, j] = hexMap[i - 1, j + 1];
            }
        }
        else
        {
            //Get South
            HexGeneration_FillZeros_Diagonals(i, j);
        }
    }

    private void HexGeneration_FillZeros_Diagonals(int i, int j)
    {
        //Random For Diagonal Sets
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            //Left / Down
            case 0:
                if (HexGeneration_CheckValidHexSpace(i - 1, j - 1))
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
                if (HexGeneration_CheckValidHexSpace(i + 1, j + 1))
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
                if (HexGeneration_CheckValidHexSpace(i - 1, j + 1))
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
                if (HexGeneration_CheckValidHexSpace(i + 1, j - 1))
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

    private bool HexGeneration_CheckValidHexSpace(int i, int j)
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

    private void HexGeneration_PostGeneration_Ocean()
    {
        int oceanSize = 10;


        for (int i = 0; i < mapHexGeneration_SideLength; i++)
        {
            for (int j = 0; j < oceanSize; j++)
            {
                hexMap[i, j] = 0;
                hexMap[j, i] = 0;
                hexMap[i, mapHexGeneration_SideLength - (j + 1)] = 0;
                hexMap[mapHexGeneration_SideLength - (j + 1), i] = 0;
            }
        }
    }

    public void SmoothMap()
    {
        //Get Best Count of 8 corners to round the value ouit
    }

    /////////////////////////////////////////////////////////////////

    public void HexVisuals_DestroyVisuals()
    {
        //Destory All Old Hex Chunks / Cells
        foreach (Transform child in biomeVisualContainer_GO.transform)
        {
            //Destroy Top-Level Child
            Destroy(child.gameObject);
        }
    }

    public void HexVisuals_DisplayVisuals()
    {
        Texture2D texture = new Texture2D(mapHexGeneration_SideLength, mapHexGeneration_SideLength);
        GameObject newHex = Instantiate(biomeVisualQuad_Prefab, new Vector3(0, 5f, 0), Quaternion.identity, biomeVisualContainer_GO.transform);
        newHex.GetComponent<Renderer>().material.mainTexture = texture;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color color = biomeVisualColors_List[hexMap[x, y]].color;
                texture.SetPixel(x, y, color);
            }
        }


        texture.Apply();

        //Recenter Camera
        cameraGenerated.transform.position = new Vector3(newHex.transform.position.x, newHex.transform.position.y, newHex.transform.position.z - 5);
        cameraGenerated.transform.rotation = new Quaternion(0,0,0,0);


        return;
        for (int i = 0; i < hexMap.GetLength(0); i++)
        {
            for (int j = 0; j < hexMap.GetLength(1); j++)
            {
                //GameObject newHex = Instantiate(biomeVisualQuad_Prefab, new Vector3(i, j + 3, 0), Quaternion.identity, biomeVisualContainer_GO.transform);
                //newHex.GetComponent<MeshRenderer>().material = biomeVisualColors_List[hexMap[i, j]];
            }
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_SetMapSeed()
    {
        Random.InitState(mapHex_Seed);
    }

    private void HexMap_RemoveOldMap()
    {
        //Destory All Old Hex Chunks / Cells
        foreach (Transform child in hexMapContainer_GO.transform)
        {
            //Destroy Top-Level Child
            Destroy(child.gameObject);
        }
    }

    private void HexMap_SpawnGround()
    {
        //Setup New Mesh
        Mesh newMesh = new Mesh();
        List<Vector3> verts_List = new List<Vector3>();
        List<int> tris_List = new List<int>();

        //Setup Sizing
        float extraTrim = 0.15f;
        float x = innerRadius * mapHex_RowCount * 1.75f;
        float y = outerRadius * mapHex_ColumnCount * 1.75f;

        //Set Square Verts
        verts_List.Add(new Vector3(-extraTrim, 0f, -extraTrim));
        verts_List.Add(new Vector3(x + extraTrim, 0f, -extraTrim));
        verts_List.Add(new Vector3(-extraTrim, 0f, y + extraTrim));
        verts_List.Add(new Vector3(x + extraTrim, 0f, y + extraTrim));

        //Create Tris
        tris_List.Add(2);
        tris_List.Add(1);
        tris_List.Add(0);

        tris_List.Add(1);
        tris_List.Add(2);
        tris_List.Add(3);

        //Set Mesh Info
        newMesh.vertices = verts_List.ToArray();
        newMesh.triangles = tris_List.ToArray();

        //Refresh Normals Info
        newMesh.RecalculateNormals();
        newMesh.Optimize();

        //Set Mesh To Gameobject
        scalingGround_GO.GetComponent<MeshFilter>().mesh = newMesh;
    }

    private void HexMap_SpawnAllHexChunks()
    {
        //Spawn Chunks By I
        for (int i = 0; i < allHexChunks_Arr.GetLength(0); i++)
        {
            //Spawn Chunks By J
            for (int j = 0; j < allHexChunks_Arr.GetLength(1); j++)
            {
                //Spawn New Chunk Objects
                GameObject newChunk = Instantiate(hexChunk_Prefab, new Vector3(0, 0, 0), Quaternion.identity, hexMapContainer_GO.transform);

                //Setup Chunk Script
                newChunk.GetComponent<HexChunk>().SetupChunk(i, j);

                //Record Chunk Script For Later
                allHexChunks_Arr[i, j] = newChunk.GetComponent<HexChunk>();
            }
        }
    }

    private void HexMap_SpawnAllHexCells()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_RowCount; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_ColumnCount; y++)
            {
                //Create Gameobject And Find Chunk
                GameObject newHex;
                GameObject cellChunk = GetChunkFromCellLocation(x, y);

                //Regular Spawn Position VS Offset Spacing
                if (y % 2 == 0)
                {
                    newHex = Instantiate(hexMesh_Prefab, new Vector3(y * spacing_J, mapHeightStep, x * spacing_I), Quaternion.identity, cellChunk.transform);
                }
                else
                {
                    newHex = Instantiate(hexMesh_Prefab, new Vector3(y * spacing_J, mapHeightStep, x * spacing_I + offcenter_I), Quaternion.identity, cellChunk.transform);
                }

                //Setup Cell
                HexCell newHexCell = newHex.GetComponent<HexCell>();
                newHexCell.GenerateHexMesh_Hard();
                newHexCell.SetLabel(x, y);

       

                //Store it
                allHexsCells_Arr[x, y] = newHexCell;
            }
        }
    }

    private void HexMap_ColorCellsOnMap()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_RowCount; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_ColumnCount; y++)
            {


                //Force 0
                int biomeID = Random.Range(0, 0);
                BiomeCellInfo biomeCellInfo;

                if (biomeID == 0)
                {
                    biomeCellInfo = biomeInfo_Forest.GetRandomBiomeCell();

                }
                else
                {
                    biomeCellInfo = biomeInfo_Plains.GetRandomBiomeCell();
                }

               


                


                allHexsCells_Arr[x, y].UpdateMaterial(biomeID, biomeCellInfo.matID, biomeCellInfo.material);
                allHexsCells_Arr[x, y].GenerateCellColor(biomeCellInfo.gradient);
                allHexsCells_Arr[x, y].UpdateCellColor(allHexsCells_Arr[x, y].hexCellColor_Main);
         
            }
        }


    }

    private void HexMap_RandomizeHeight()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < allHexsCells_Arr.GetLength(0); x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < allHexsCells_Arr.GetLength(1); y++)
            {
                allHexsCells_Arr[x, y].GenerateHeight_Perlin(x, y, mapHex_RowCount, mapHex_ColumnCount); // 1024
            }

        }

        /*
        foreach (HexCell hexCell in allHexsCells_Arr)
        {
            //Generate Random Heights
            hexCell.GenerateHeight_Random(mapHeightMin, mapHeightMax, mapHeightStep);
        }
        */
    }

    private void HexMap_StoreAllHexCells()
    {
        foreach (HexCell hexCell in allHexsCells_Arr)
        {

        }
    }

    private void HexMap_CenterCamera()
    {

        float extraTrim = 0.15f;
        float x = innerRadius * mapHex_RowCount * 1.75f;
        float left = -extraTrim;
        float right = x + extraTrim;
        float xPos = Mathf.Lerp(left, right, 0.5f);

        cameraGenerated.transform.position = new Vector3(xPos, cameraRelativePosition.x, cameraRelativePosition.y);
    }

    private void HexMap_SpawnDecoration()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_RowCount; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_ColumnCount; y++)
            {

                int randomID = Random.Range(0, 5);

                if (randomID == 0)
                {
                    //Debug.Log("Test Code: Grasss");

                    //GameObject grass = Instantiate(hexGrass_Prefab, new Vector3(0, 0, 0), Quaternion.Euler(0, Random.Range(0, 360f), 0), allHexsCells_Arr[x, y].gameObject.transform);

                    //grass.transform.localPosition = new Vector3(0f, 0.1f, 0f);


                }






            }
        }
    }

    private void HexMap_HideAllHexes()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_RowCount; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_ColumnCount; y++)
            {
                allHexsCells_Arr[x, y].gameObject.SetActive(false);
            }
        }
    }

    private void HexMap_ShowAllHexes()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_RowCount; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_ColumnCount; y++)
            {
                allHexsCells_Arr[x, y].gameObject.SetActive(true);
            }
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_Chunk()
    {
        foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
            hexChunk.Chunk(hexChunkModel_Prefab, mergedBiomeMats_Arr);
        }
    }

    private void HexMap_Unchunk()
    {
        Debug.Log("Test Code: Unchunk");

        foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
            hexChunk.Unchunk();
        }
    }

    private void HexMap_Rechunk()
    {
        Debug.Log("Test Code: Rechunk");

        foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
            hexChunk.Rechunk();
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_LoadSave()
    {
        //Load Save File

        //Recreate All Tiles From Save

    }

    private void HexMap_CreateSave()
    {
        //Save the Data
        Serializer.Save("HexMap.gobo", mySaveFile);

        //Delete All Tiles

    }

    /////////////////////////////////////////////////////////////////

    private void MouseInput_Click()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);


        if (Physics.Raycast(inputRay, out RaycastHit hit))
        {
            HexCell hitCell = hit.collider.gameObject.transform.parent.GetComponentInParent<HexCell>();

            if (hitCell != null)
            {
                hitCell.ClickCell();
            }
        }
    }

    private void HexMap_SetupMatsArray()
    {
        //Create a list to automate the mat list creation
        List<BiomeInfo_SO> biomeInfoSets_List = new List<BiomeInfo_SO>();

        //Add All of the Biomes Here
        biomeInfoSets_List.Add(biomeInfo_Forest);
        biomeInfoSets_List.Add(biomeInfo_Plains);


        //Create Array Size Using Biome Count / Random Cap
        mergedBiomeMats_Arr = new Material[biomeInfoSets_List.Count, biomeMaterialCap];




        for (int i = 0; i < biomeInfoSets_List.Count; i++)
        {
            int lastFoundID = -1;

            for (int j = 0; j < biomeInfoSets_List[i].biomeCellsInfo_Arr.Length; j++)
            {
                if (lastFoundID != biomeInfoSets_List[i].biomeCellsInfo_Arr[j].matID)
                {
                    lastFoundID = biomeInfoSets_List[i].biomeCellsInfo_Arr[j].matID;
                    mergedBiomeMats_Arr[i, lastFoundID] = biomeInfoSets_List[i].biomeCellsInfo_Arr[j].material;

                }
            }
        }
    }

    private Tuple<bool, string> ErrorChecker()
    {
        Tuple<bool, string> returningError_Tuple = new Tuple<bool, string>(true, "");


        return returningError_Tuple;
    }

    private GameObject GetChunkFromCellLocation(int x, int y)
    {
        return allHexChunks_Arr[(int)Mathf.Floor(x / mapHex_ChunkSize), (int)Mathf.Floor(y / mapHex_ChunkSize)].gameObject;
    }

    public static Material GetChunkMaterial(int i, int j)
    {
        return mergedBiomeMats_Arr[i, j];
    }

    /////////////////////////////////////////////////////////////////

    private void OLDCODE_SlowHeightAnimation()
    {
        // This Requires the Animation Componanted added back onto the cells

        /*
        yield return new WaitForEndOfFrame();

        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Create New Arrays
        allHexsCells_Arr = new HexCell[mapHex_RowCount, mapHex_ColumnCount];
        allHexChunks_Arr = new HexChunk[(int)Mathf.Ceil(mapHex_RowCount / mapHex_ChunkSize), (int)Mathf.Ceil(mapHex_ColumnCount / mapHex_ChunkSize)];

        //Create Either a Chunked or Non-Chunked Version
        HexMap_SetMapSeed();
        HexMap_SpawnGround();
        HexMap_RemoveOldMap();
        HexMap_SpawnAllHexChunks();
        HexMap_SpawnAllHexCells();
        HexMap_CenterCamera();
        HexMap_ColorCellsOnMap();
        HexMap_RandomizeHeight();
        HexMap_StoreAllHexCells();


        //yield return new WaitForSeconds(1f);

        HexMap_HideAllHexes();

        //yield return new WaitForSeconds(1f);



        float coloringTime = 4f;
        float timePerLoop = coloringTime / (mapHex_RowCount * mapHex_ColumnCount);



        float loopsPerWaitCycle = 20 / timePerLoop * 0.001f;
        float currentCellsPassed = 0;


        int dim = mapHex_RowCount;

        for (int k = 0; k < dim * 2; k++)
        {
            for (int j = 0; j <= k; j++)
            {
                int i = k - j;
                if (i < dim && j < dim)
                {

                    if (currentCellsPassed >= loopsPerWaitCycle)
                    {
                        currentCellsPassed = 0;

                        //Wait 1 Millisecond
                        yield return new WaitForFixedUpdate();
                    }
                    else
                    {
                        currentCellsPassed++;
                    }

                    allHexsCells_Arr[i, j].gameObject.SetActive(true);
                    allHexsCells_Arr[i, j].hexObject_GO.GetComponent<Animator>().Play("Popup");

                }
            }

        }





        //Finish Counting Timer
        long endingTimeTicks = DateTime.UtcNow.Ticks;
        float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
        Debug.Log("Test Code: Map Generation Completed in: " + finishTime + "s");

        yield break;
        */
    }

    private void OLDCODE_SlowColorAnimation()
    {
        /*
       yield return new WaitForEndOfFrame();

       //Start Counting Timer
       long startingTimeTicks = DateTime.UtcNow.Ticks;

       //Create New Arrays
       allHexsCells_Arr = new HexCell[mapHex_RowCount, mapHex_ColumnCount];
       allHexChunks_Arr = new HexChunk[(int)Mathf.Ceil(mapHex_RowCount / mapHex_ChunkSize), (int)Mathf.Ceil(mapHex_ColumnCount / mapHex_ChunkSize)];

       //Create Either a Chunked or Non-Chunked Version
       HexMap_SetMapSeed();
       HexMap_SpawnGround();
       HexMap_RemoveOldMap();
       HexMap_SpawnAllHexChunks();
       HexMap_SpawnAllHexCells();
       HexMap_CenterCamera();
       HexMap_ColorCellsOnMap();
       HexMap_RandomizeHeight();
       HexMap_StoreAllHexCells();


       //yield return new WaitForSeconds(1f);

       HexMap_HideAllHexes();

       //yield return new WaitForSeconds(1f);



        float coloringTime = 2f;
        float timePerLoop = coloringTime / (mapHex_RowCount * mapHex_ColumnCount);



        float loopsPerWaitCycle = 20 / timePerLoop * 0.001f;
        float currentCellsPassed = 0;


        int dim = mapHex_RowCount;

        for (int k = 0; k < dim * 2; k++)
        {
            for (int j = 0; j <= k; j++)
            {
                int i = k - j;
                if (i < dim && j < dim)
                {

                    if (currentCellsPassed >= loopsPerWaitCycle)
                    {
                        currentCellsPassed = 0;

                        //Wait 1 Millisecond
                        yield return new WaitForFixedUpdate();
                    }
                    else
                    {
                        currentCellsPassed++;
                    }

                    int randomColorType = Random.Range(0, 4);
                    switch (randomColorType)
                    {
                        case 0:
                            allHexsCells_Arr[i, j].UpdateMaterial(1, hexCellColorMaterial_PlainsColored);
                            allHexsCells_Arr[i, j].GenerateCellColor(hexCellColorGradient_PlainsColored);
                            break;

                        case 1:
                            allHexsCells_Arr[i, j].UpdateMaterial(1, hexCellColorMaterial_PlainsColored);
                            allHexsCells_Arr[i, j].GenerateCellColor(hexCellColorGradient_PlainsTextured1);
                            break;

                        case 2:
                            allHexsCells_Arr[i, j].UpdateMaterial(2, hexCellColorMaterial_PlainsTextured2);
                            allHexsCells_Arr[i, j].GenerateCellColor(hexCellColorGradient_PlainsTextured2);
                            break;

                        case 3:
                            allHexsCells_Arr[i, j].UpdateMaterial(2, hexCellColorMaterial_PlainsTextured2);
                            allHexsCells_Arr[i, j].GenerateCellColor(hexCellColorGradient_PlainsTextured3);
                            break;
                    }

                    allHexsCells_Arr[i, j].UpdateCellColor(allHexsCells_Arr[i, j].colorActive);


                }
            }

        }







         //Finish Counting Timer
       long endingTimeTicks = DateTime.UtcNow.Ticks;
       float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
       Debug.Log("Test Code: Map Generation Completed in: " + finishTime + "s");

       yield break;
   */
    }

    private void OLDCODE_HowToSetCodeGradients()
    {

        //Might Need this for mergeiung color between biomes


        /*
        GradientColorKey[] colorKey;
        GradientAlphaKey[] alphaKey;

        gradientColors = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = new Color(0, 1, 0, 1);
        colorKey[0].time = 0.0f;
        colorKey[1].color = new Color(0.2f, 0.6f, 0.2f, 1);
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 0.0f;
        alphaKey[1].time = 1.0f;

        gradientColors.SetKeys(colorKey, alphaKey);
        */

    }

    private void OLDCODE_HowToMakeEditorFancy()
    {
        /*
        public override void OnInspectorGUI()
        {

            BiomeCellSet_SO cellSet = (BiomeCellSet_SO)target;

            //Title Bar
            EditorGUILayout.InspectorTitlebar(true, cellSet);
            GUILayout.Space(10);

            //Spacing bar
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


            //base.OnInspectorGUI();

 

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Material");
                cellSet.material = (Material)EditorGUILayout.ObjectField(cellSet.material, typeof(Material), false);

                GUILayout.Label("Gradient");
                cellSet.gradient = EditorGUILayout.GradientField(cellSet.gradient);
            }
            GUILayout.EndHorizontal();





            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Color"))
            {
                //cube.GenerateColor();
            }

            if (GUILayout.Button("Reset"))
            {
                //cube.Reset();
            }

            GUILayout.EndHorizontal();


            //serializedObject.ApplyModifiedProperties();


            //rect.position = new Vector2(60, 40);
            //GUI.Label(rect, "(" + hexCoordInfo_X + " - " + hexCoordInfo_Z +")");
        }
        */
    }

    private void OLDCODE_SaveMeshAsset()
    {
        //AssetDatabase.CreateAsset(allHexsCells_Arr[0,0].hexObject_MeshFilter.mesh, "Assets/NewHexMesh.mesh");
        //AssetDatabase.SaveAssets(); 
    }
    
    /////////////////////////////////////////////////////////////////
}