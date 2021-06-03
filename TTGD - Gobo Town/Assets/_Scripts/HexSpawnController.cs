using System;
using System.Collections.Generic;
using System.Linq;
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
    public bool isShowingBiomeVisuals;
    public bool isShowingGenerationTime;

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

    [Header("Map Generation Settings")]
    public int mapGen_StartingBiomeNodesCount = 20;
    public int mapGen_SideLength = 160;
    public int mapGen_OceanSize = 2;
    public int mapGen_BeachSize = 2;


    [Header("Old Spawns")]
    public int mapHex_RowCount = 100;
    public int mapHex_ColumnCount = 100;
    public int mapHex_ChunkSize = 10;

    [Header("Hex Map Settings (Height)")]
    public int mapHeightMin = 0;
    public int mapHeightMax = 5;
    public float mapHeightStep = 0.025f;
    public static readonly float hexCell_HeightPerStep = 0.04f;

    [Header("Hex Map Settings (RNG)")]
    public int mapHex_Seed = 135135;

    ////////////////////////////////

    [Header("Biome Info Sets")]
    public BiomeInfo_SO[] allBiomes_Arr;
    public BiomeInfo_SO biomeInfo_Ocean;
    public BiomeInfo_SO biomeInfo_Plains;
    public BiomeInfo_SO biomeInfo_Forest;
    private readonly int biomeMaterialCap = 10;



    [Header("Randomization States To Be Used")]
    private Random.State mapGeneration_SeededStated; //= Random.state;


    [Header("Biome Visuals")]
    public GameObject biomeVisualQuad_Prefab;
    public GameObject biomeVisualContainer_GO;
    public List<Material> biomeVisualColoredMaterials_List;


    ////////////////////////////////


    //Generation List Sets
    public int[,] mapHex_BiomeSets;
    public int[,] mapHex_HeightSets;
    public int[,] mapHex_MatIDSets;
    public string[,] mapHex_ColorSets;

    [Header("Perline Noise Movement")]
    private float oldOffsetX = 0;
    private float oldOffsetY = 0;
    public float offsetX = 0;
    public float offsetY = 0;

    public float perlinZoomScale = 30;
    public static Material[,] mergedBiomeMats_Arr;

    [HideInInspector]
    public SaveFile mySaveFile;

    public int hexCountAllowedFromCamera = 5;

    public HexCell lastCameraCell;


    List<HexCell> currentHexsLoaded_List = new List<HexCell>();

    /////////////////////////////////////////////////////////////////

    private void Start()
    {
        //Spawn The Visual Biome Set
        HexGen_Spawn();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.C))
        {
            ReloadHexsAroundCamera();
        }



        if (oldOffsetX != offsetX)
        {
            oldOffsetX = offsetX;
            HexGen_PerlinHeight();
        }

        if (oldOffsetY != offsetY)
        {
            oldOffsetY = offsetY;
            HexGen_PerlinHeight();
        }








        ////////////////////////////////

        if (Input.GetKeyDown(KeyCode.R))
        {
            mapHex_Seed = Random.Range(100000, 999999);
            HexMap_SetMapSeed();
            HexGen_Spawn();
            //HexMap_Spawn_OLD();
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
            HexGen_Spawn();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            HexGen_PerlinHeight();
        }

        ////////////////////////////////

        if (Input.GetMouseButtonDown(0))
        {
            MouseInput_Click();
        }
    }

    private void ReloadHexsAroundCamera()
    {
        GenerateAllowedHexsAroundCamera();


    }

    private void GenerateAllowedHexsAroundCamera()
    {
   

        //Get Camera Position
        Vector3 cameraPos = cameraGenerated.transform.position;

        //Get The Hex X Position From Camera Spacing
        int xPos = (int)Math.Round(cameraPos.z / spacing_I, 0);
        int yPos = (int)Math.Round(cameraPos.x / spacing_J, 0); 

        //Clamp Positions to the array size
        xPos = Mathf.Clamp(xPos, 0, mapGen_SideLength - 1);
        yPos = Mathf.Clamp(yPos, 0, mapGen_SideLength - 1);


        HexCell newCameraCell = allHexsCells_Arr[xPos, yPos];

        if (lastCameraCell == null)
        {
            //currentCameraCell = allHexsCells_Arr[xPos, yPos];
            //currentCameraCell.ClickCell();
        }
        else if (lastCameraCell.name != allHexsCells_Arr[xPos, yPos].name)
        {
            //currentCameraCell = allHexsCells_Arr[xPos, yPos];
            //currentCameraCell.ClickCell();
        }
        else
        {
            //currentCameraCell.ClickCell();
            //currentCameraCell = allHexsCells_Arr[xPos, zPos];
            //currentCameraCell.ClickCell();
        }


        

        int rightCorner = Mathf.Clamp(xPos + hexCountAllowedFromCamera + 1, 0, mapGen_SideLength - 0);
        int leftCorner = Mathf.Clamp(xPos - hexCountAllowedFromCamera, 0, mapGen_SideLength - 1);

        int topCorner = Mathf.Clamp(yPos + hexCountAllowedFromCamera + 1, 0, mapGen_SideLength - 0);
        int bottomCorner = Mathf.Clamp(yPos - hexCountAllowedFromCamera, 0, mapGen_SideLength - 1);


        //Debug.Log("Test Code: Last " + lastCameraCell.hexCoords.GetPrintableCoords());
        //Debug.Log("Test Code: New " + newCameraCell.hexCoords.GetPrintableCoords());

        //Match X and Y Coords To Compare 
        if (lastCameraCell == null)
        {
            lastCameraCell = newCameraCell;
        }
        else if ((lastCameraCell.hexCoords.X == newCameraCell.hexCoords.X) && (lastCameraCell.hexCoords.Y == newCameraCell.hexCoords.Y))
        {
            Debug.Log("Test Code: This Does Not Trigger!");
            //Do Nothing
            return;
        }
        else
        {
            lastCameraCell = newCameraCell;
        }



        //Destroy All hexes
        HexGen_ClearOldMaps();


        List<HexCell_Data> wantedHexs_List = new List<HexCell_Data>();

        for (int x = leftCorner; x < rightCorner; x++)
        {
            for (int y = bottomCorner; y < topCorner; y++)
            {

                //HexCell question14 = currentHexsLoaded_List.FirstOrDefault(z => z.hexCoords.X == 14);




                //Just Load the Hex For Now
                //if (currentHexsLoaded_List.Contains())
                {

                }


                wantedHexs_List.Add(dataHexCells_Arr[x, y]);
                //allHexsCells_Arr[x, y].ClickCell();
            }
        }

        //Spawn Them
        ConfirmAllowedHexsAroundCamera(wantedHexs_List);




        //Debug.Log("Test Code: " + allHexsCells_Arr[xPos, yPos].name);

        //currentCameraCell = allHexsCells_Arr[xPos, zPos];

        //Debug.Log("Test Code: x:" + xPos);
        //Debug.Log("Test Code: z:" + zPos);

        //Get Closest Node To Camera Position
        //dataHexCells_Arr[]

        //Get all 4 cornoer nodes away from cetner point

        //Get All Nodes Between 4 corners




    }

    private void ConfirmAllowedHexsAroundCamera(List<HexCell_Data> wantedHexs_List)
    {
        foreach (HexCell_Data hexCellData in wantedHexs_List)
        {
            //Create Gameobject And Find Chunk
            GameObject newHex;
            //GameObject cellChunk = GetChunkFromCellLocation(x, y);

            //Regular Spawn Position VS Offset Spacing
            if (hexCellData.hexCoords.Y % 2 == 0)
            {
                newHex = Instantiate(hexMesh_Prefab, new Vector3(hexCellData.hexCoords.Y * spacing_J, mapHeightStep, hexCellData.hexCoords.X * spacing_I), Quaternion.identity, hexMapContainer_GO.transform);
            }
            else
            {
                newHex = Instantiate(hexMesh_Prefab, new Vector3(hexCellData.hexCoords.Y * spacing_J, mapHeightStep, hexCellData.hexCoords.X * spacing_I + offcenter_I), Quaternion.identity, hexMapContainer_GO.transform);
            }

            //Setup Cell
            HexCell newHexCell = newHex.GetComponent<HexCell>();
            newHexCell.CreateCellFromData(hexCellData);
            newHexCell.SetLabel(hexCellData.hexCoords.X, hexCellData.hexCoords.Y);

            //Store it
            allHexsCells_Arr[hexCellData.hexCoords.X, hexCellData.hexCoords.Y] = newHexCell;
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexGen_Spawn()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        dataHexCells_Arr = new HexCell_Data[mapGen_SideLength, mapGen_SideLength];
        allHexsCells_Arr = new HexCell[mapGen_SideLength, mapGen_SideLength];

        //Create Array of Biomes
        allBiomes_Arr = new BiomeInfo_SO[3];
        allBiomes_Arr[0] = biomeInfo_Ocean;
        allBiomes_Arr[1] = biomeInfo_Plains;
        allBiomes_Arr[2] = biomeInfo_Forest;

        //Create Base Display List Setups
        HexGenBiome_CreateInitialBiomes();

        //Create Other List Setups
        mapHex_HeightSets = new int[mapGen_SideLength, mapGen_SideLength];
        mapHex_MatIDSets = new int[mapGen_SideLength, mapGen_SideLength];
        mapHex_ColorSets = new string[mapGen_SideLength, mapGen_SideLength];

        //Use Log() to calucale how many loops are needed to get to the side length value
        int mapHexGeneration_BiomeGrowthLoopCount = (int)Mathf.Log((float)mapGen_SideLength / mapGen_StartingBiomeNodesCount, 2);

        for (int i = 0; i < mapHexGeneration_BiomeGrowthLoopCount; i++)
        {
            //Zoom Then Fill To Expand Map
            HexGenBiome_ZoomOutBiome();
            HexGenBiome_FillZeros();

            //Need Smoothing
        }

        //These Converge into lakes at the end 
        //HexGeneration_PostGeneration_Rivers();
        //HexGeneration_PostGeneration_Lakes();
        //HexGeneration_PostGeneration_Beaches();
        //HexGeneration_PostGeneration_InterBiomes();
        HexGenBiome_PostGeneration_Ocean();


        HexMap_SetupMatsArray();

        HexGen_PerlinHeight();
        HexGen_MatsAndColors();
   


        //Merge Info Together Into Storable Data Hex Cells
        HexGen_MergeDataToHexCells();


        //HexMap_SpawnAllHexChunks();
        HexGen_ClearOldMaps();
        HexGen_SpawnAllHexesFromData();

        //Ground Visuals
        HexGen_SpawnGround();

        //Check if Camera Focus is Biome Map or Hex Map
        if (isShowingBiomeVisuals)
        {
            //Show the Biome Map Visually
            HexVisuals_DestroyVisuals();
            HexVisuals_DisplayVisuals();
        }
        else
        {
            //Center Camera on Hex Map
            HexGen_CenterCamera();
        }

       


        //Show Generation Time
        if (isShowingGenerationTime)
        {
            //Finish Counting Timer
            long endingTimeTicks = DateTime.UtcNow.Ticks;
            float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
            Debug.Log("Test Code: Biome Generation x" + mapHexGeneration_BiomeGrowthLoopCount + " Completed in: " + finishTime + "s");
            Debug.Log("Test Code: Size " + mapHex_BiomeSets.GetLength(0) + "x" + mapHex_BiomeSets.GetLength(1));
        }
    }

    private void HexMap_Spawn_OLD()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Create New Arrays
        dataHexCells_Arr = new HexCell_Data[mapHex_RowCount, mapHex_ColumnCount];
        allHexsCells_Arr = new HexCell[mapHex_RowCount, mapHex_ColumnCount];
        allHexChunks_Arr = new HexChunk[(int)Mathf.Ceil(mapHex_RowCount / mapHex_ChunkSize), (int)Mathf.Ceil(mapHex_ColumnCount / mapHex_ChunkSize)];

        //Generate Map
        HexMap_SetMapSeed();
        //HexMap_GenerateAllHexCells();



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
    
    private void HexGen_PerlinHeight()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_HeightSets.GetLength(0); x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_HeightSets.GetLength(1); y++)
            {
                //Generate Biome Heights Depending on Biome Generated at this point in the Array
                switch (mapHex_BiomeSets[x, y])
                {
                    //0 == Ocean
                    case 0:
                        HexGen_FlatHeight_Ocean(x, y);
                        break;

                    //1 == Plains
                    case 1:
                        HexGen_PerlinHeight_Plains(x, y);
                        break;

                    //1 == Forest
                    case 2:
                        HexGen_PerlinHeight_Plains(x, y);
                        break;

                    //??? == ???
                    default:
                        break;
                }
            }
        }
    }

    private void HexGen_FlatHeight_Ocean(int x, int y)
    {
        //Create a Height Steps value based off of closeset step to the "Real" Height
        int heightSteps = 4;

        //Set Final Value To Array
        mapHex_HeightSets[x, y] = heightSteps;
    }

    private void HexGen_PerlinHeight_Plains(int x, int y)
    {
        float neutralHeight = 0.5f;


        //USE HEIGHT MAPS BASED OFF BIOMES


        //float perlinZoomScale = 20;

        float xScaled = (float)x / perlinZoomScale;
        float yScaled = (float)y / perlinZoomScale;


        float height = Mathf.PerlinNoise(xScaled + offsetX, yScaled + offsetY);




        //Create a Height Value the tends closer to the average of the Neutral Height
        height = (neutralHeight + height) / 2;

        //Create a Height Steps value based off of closeset step to the "Real" Height
        int heightSteps = (int)(height / hexCell_HeightPerStep);

        //Set Final Value To Array
        mapHex_HeightSets[x, y] = heightSteps;
    }

    private void HexGen_MatsAndColors()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapHex_MatIDSets.GetLength(0); x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapHex_MatIDSets.GetLength(1); y++)
            {
                //Get The Biome Info Cell based off of the biome in the given cell
                BiomeCellInfo biomeCellInfo = allBiomes_Arr[mapHex_BiomeSets[x, y]].GetRandomBiomeCell();

                //Set The MatID / Color Sets
                mapHex_MatIDSets[x, y] = biomeCellInfo.matID;
                mapHex_ColorSets[x, y] = ColorUtility.ToHtmlStringRGB(biomeCellInfo.gradient.Evaluate(Random.Range(0, 1f)));
            }
        }
    }

    private void HexGen_MergeDataToHexCells()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < dataHexCells_Arr.GetLength(1); x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < dataHexCells_Arr.GetLength(1); y++)
            {
                //Store The Data Collected From Other Methodsit
                dataHexCells_Arr[x, y] = new HexCell_Data
                {
                    hexCoords = new HexCoords(x, y, mapHex_HeightSets[x, y]),
                    hexCell_BiomeID = mapHex_BiomeSets[x, y],
                    hexCell_Color = mapHex_ColorSets[x, y],
                    hexCell_MatID = mapHex_MatIDSets[x, y]
                };
            }
        }
    }

    private void HexGen_SpawnAllHexesFromData()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < dataHexCells_Arr.GetLength(0); x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < dataHexCells_Arr.GetLength(1); y++)
            {
                //Create Gameobject And Find Chunk
                GameObject newHex;
                //GameObject cellChunk = GetChunkFromCellLocation(x, y);

                //Regular Spawn Position VS Offset Spacing
                if (y % 2 == 0)
                {
                    newHex = Instantiate(hexMesh_Prefab, new Vector3(y * spacing_J, mapHeightStep, x * spacing_I), Quaternion.identity, hexMapContainer_GO.transform);
                }
                else
                {
                    newHex = Instantiate(hexMesh_Prefab, new Vector3(y * spacing_J, mapHeightStep, x * spacing_I + offcenter_I), Quaternion.identity, hexMapContainer_GO.transform);
                }

                //Setup Cell
                HexCell newHexCell = newHex.GetComponent<HexCell>();
                newHexCell.CreateCellFromData(dataHexCells_Arr[x, y]);
                newHexCell.SetLabel(x, y);

                //Store it
                allHexsCells_Arr[x, y] = newHexCell;
            }
        }
    }

    private void HexGen_ClearOldMaps()
    {
        //Destory All Old Hex Chunks / Cells
        foreach (Transform child in hexMapContainer_GO.transform)
        {
            //Destroy Top-Level Child
            Destroy(child.gameObject);
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexGen_SpawnGround()
    {
        //Setup New Mesh
        Mesh newMesh = new Mesh();
        List<Vector3> verts_List = new List<Vector3>();
        List<int> tris_List = new List<int>();

        //Setup Sizing
        float extraTrim = 0.15f;
        float x = innerRadius * mapGen_SideLength * 1.75f;
        float y = outerRadius * mapGen_SideLength * 1.75f;

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

    private void HexGen_CenterCamera()
    {
        //Figure out the X Position by size of the generated map
        float extraTrim = 0.15f;
        float x = innerRadius * mapGen_SideLength * 1.75f;
        float left = -extraTrim;
        float right = x + extraTrim;
        float xPos = Mathf.Lerp(left, right, 0.5f);

        //Set Camera Position To Look at map from a good angle
        cameraGenerated.transform.position = new Vector3(xPos, cameraRelativePosition.x, cameraRelativePosition.y);
    }

    /////////////////////////////////////////////////////////////////

    private void HexGenBiome_CreateInitialBiomes()
    {
        mapHex_BiomeSets = new int[mapGen_StartingBiomeNodesCount, mapGen_StartingBiomeNodesCount];


        for (int i = 0; i < mapGen_StartingBiomeNodesCount; i++)
        {
            for (int j = 0; j < mapGen_StartingBiomeNodesCount; j++)
            {
                mapHex_BiomeSets[i, j] = Random.Range(1, 3);
            }
        }
    }

    private void HexGenBiome_ZoomOutBiome()
    {
        int currentSize_Row = mapHex_BiomeSets.GetLength(0);
        int currentSize_Column = mapHex_BiomeSets.GetLength(1);
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


                newScaleMap_Arr[(int)newSet_TopLeft.x, (int)newSet_TopLeft.y] = mapHex_BiomeSets[i, j];

                newScaleMap_Arr[(int)newSet_TopRight.x, (int)newSet_TopRight.y] = 0;
                newScaleMap_Arr[(int)newSet_BottomLeft.x, (int)newSet_BottomLeft.y] = 0;
                newScaleMap_Arr[(int)newSet_BottomRight.x, (int)newSet_BottomRight.y] = 0;
            }
        }


        mapHex_BiomeSets = newScaleMap_Arr;
    }

    private void HexGenBiome_FillZeros()
    {
        //Foreach Each Set Of Hexs Fill All 0s
        for (int i = 0; i < mapHex_BiomeSets.GetLength(0); i++)
        {
            for (int j = 0; j < mapHex_BiomeSets.GetLength(1); j++)
            {
                //Check if current value is 0
                if (mapHex_BiomeSets[i, j] == 0)
                {
                    //Check Current Status of I / J to determine where the value comes from
                    if (j % 2 == 0)
                    {
                        //J is an Even Value
                        HexGenBiome_FillZeros_JEven(i, j);
                    }
                    else if (i % 2 == 0)
                    {
                        //I is an Even Value
                        HexGenBiome_FillZeros_IEven(i, j);
                    }
                    else if (j + 1 == mapHex_BiomeSets.GetLength(1))
                    {
                        //J is at Max Value
                        HexGenBiome_FillZeros_JMaxed(i, j);
                    }
                    else
                    {
                        //Both Values are Odd / Use diagonals
                        HexGenBiome_FillZeros_Diagonals(i, j);
                    }
                }
            }
        }

    }

    private void HexGenBiome_FillZeros_JEven(int i, int j)
    {
        //Random FOr Left and Right
        int rand = Random.Range(0, 2);

        switch (rand)
        {
            //Left
            case 0:
                if (HexGenBiome_CheckValidHexSpace(i - 1, j))
                {
                    //True Left
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j];
                }
                else
                {
                    //Forced Right
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j];
                }
                break;

            //Right
            case 1:
                if (HexGenBiome_CheckValidHexSpace(i + 1, j))
                {
                    //True Right
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j];
                }
                else
                {
                    //Forced Left
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j];
                }
                break;
        }
    }

    private void HexGenBiome_FillZeros_IEven(int i, int j)
    {
        //Random For Up and Down
        int rand = Random.Range(0, 2);

        switch (rand)
        {
            //Down
            case 0:
                if (HexGenBiome_CheckValidHexSpace(i, j - 1))
                {
                    //True Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i, j - 1];
                }
                else
                {
                    //Forced Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i, j + 1];
                }
                break;

            //Up
            case 1:
                if (HexGenBiome_CheckValidHexSpace(i, j + 1))
                {
                    //True Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i, j + 1];
                }
                else
                {
                    //Forced Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i, j - 1];
                }
                break;


        }
    }

    private void HexGenBiome_FillZeros_JMaxed(int i, int j)
    {
        if (i == 0)
        {

            //True Right / Down
            mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j - 1];

            return;

            if (HexGenBiome_CheckValidHexSpace(i + 1, j - 1))
            {

            }
            else
            {
                //Forced Left / Up
                mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j + 1];
            }
        }
        else if (i + 1 == mapHex_BiomeSets.GetLength(0))
        {

            //True Left / Down
            mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j - 1];

            return;

            if (HexGenBiome_CheckValidHexSpace(i + 1, j - 1))
            {
                //True Right / Up
                mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j - 1];
            }
            else
            {
                //Forced Left / Up
                mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j + 1];
            }
        }
        else
        {
            //Get South
            HexGenBiome_FillZeros_Diagonals(i, j);
        }
    }

    private void HexGenBiome_FillZeros_Diagonals(int i, int j)
    {
        //Random For Diagonal Sets
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            //Left / Down
            case 0:
                if (HexGenBiome_CheckValidHexSpace(i - 1, j - 1))
                {
                    //True Left / Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j - 1];
                }
                else
                {
                    //Forced Right / Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j + 1];
                }
                break;

            //Right / Up
            case 1:
                if (HexGenBiome_CheckValidHexSpace(i + 1, j + 1))
                {
                    //True Right / Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j + 1];
                }
                else
                {
                    //Forced Left / Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j - 1];
                }
                break;

            //Left / Up
            case 2:
                if (HexGenBiome_CheckValidHexSpace(i - 1, j + 1))
                {
                    //True Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j + 1];
                }
                else
                {
                    //Forced Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j - 1];
                }
                break;

            //Right / Down
            case 3:
                if (HexGenBiome_CheckValidHexSpace(i + 1, j - 1))
                {
                    //True Right / Down
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i + 1, j - 1];
                }
                else
                {
                    //Forced Left / Up
                    mapHex_BiomeSets[i, j] = mapHex_BiomeSets[i - 1, j + 1];
                }
                break;
        }
    }

    private bool HexGenBiome_CheckValidHexSpace(int i, int j)
    {
        if (i < 0 || j < 0)
        {
            return false;
        }

        if (mapHex_BiomeSets.GetLength(0) < i + 1)
        {
            return false;
        }

        if (mapHex_BiomeSets.GetLength(1) < j + 1)
        {
            return false;
        }


        return true;
    }

    private void HexGenBiome_PostGeneration_Ocean()
    {
        for (int i = 0; i < mapGen_SideLength; i++)
        {
            for (int j = 0; j < mapGen_OceanSize; j++)
            {
                mapHex_BiomeSets[i, j] = 0;
                mapHex_BiomeSets[j, i] = 0;
                mapHex_BiomeSets[i, mapGen_SideLength - (j + 1)] = 0;
                mapHex_BiomeSets[mapGen_SideLength - (j + 1), i] = 0;
            }
        }
    }

    private void HexGenBiome_PostGeneration_Beach()
    {
        for (int i = 0; i < mapGen_SideLength; i++)
        {
            for (int j = 0; j < mapGen_OceanSize; j++)
            {
                mapHex_BiomeSets[i, j] = 0;
                mapHex_BiomeSets[j, i] = 0;
                mapHex_BiomeSets[i, mapGen_SideLength - (j + 1)] = 0;
                mapHex_BiomeSets[mapGen_SideLength - (j + 1), i] = 0;
            }
        }
    }

    private void HexGenBiome_PostGeneration_BiomeSizingMaxMin()
    {
        //2 Matrix

        // one bools

        // one the nodes

        // one the "Clusters"

        bool[,] hasBeenSearched_Arr = new bool[mapGen_SideLength, mapGen_SideLength];
        //int[] = 

        //BiomeCluster[,] biomeCluster_List;
        




        int oceanSize = 10;


        for (int i = 0; i < mapGen_SideLength; i++)
        {
            for (int j = 0; j < oceanSize; j++)
            {
                mapHex_BiomeSets[i, j] = 0;
                mapHex_BiomeSets[j, i] = 0;
                mapHex_BiomeSets[i, mapGen_SideLength - (j + 1)] = 0;
                mapHex_BiomeSets[mapGen_SideLength - (j + 1), i] = 0;
            }
        }
    }

    private void HexGenBiome_SmoothMap()
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
        //Create a new texture using the biome ints as colors 
        Texture2D texture = new Texture2D(mapGen_SideLength, mapGen_SideLength);
        GameObject newHex = Instantiate(biomeVisualQuad_Prefab, new Vector3(0, 2f, -2f), Quaternion.identity, biomeVisualContainer_GO.transform);
        newHex.GetComponent<Renderer>().material.mainTexture = texture;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color color = biomeVisualColoredMaterials_List[mapHex_BiomeSets[x, y]].color;
                texture.SetPixel(x, y, color);
            }
        }

        //Apply Pixel Colors To Texture
        texture.Apply();

        //Recenter Camera
        cameraGenerated.transform.position = new Vector3(newHex.transform.position.x, newHex.transform.position.y, newHex.transform.position.z - 5);
        cameraGenerated.transform.rotation = new Quaternion(0,0,0,0);
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
                int biomeID = Random.Range(0, 2);
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
                //allHexsCells_Arr[x, y].GenerateHeight_Perlin(xScaled, yScaled, mapHex_RowCount, mapHex_ColumnCount); // 1024
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
        biomeInfoSets_List.Add(biomeInfo_Ocean);
        biomeInfoSets_List.Add(biomeInfo_Plains);
        biomeInfoSets_List.Add(biomeInfo_Forest);

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

    private GameObject GetChunkFromCellLocation(int x, int y)
    {
        return allHexChunks_Arr[(int)Mathf.Floor(x / mapHex_ChunkSize), (int)Mathf.Floor(y / mapHex_ChunkSize)].gameObject;
    }

    public static Material GetSearchableMaterial(int biomeID, int matID)
    {
        return mergedBiomeMats_Arr[biomeID, matID];
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