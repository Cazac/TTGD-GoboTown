using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapSpawnController : MonoBehaviour
{
    ////////////////////////////////

    [Header("Chunk Storage")]
    private Dictionary<ChunkCoords, HexChunk> hexChunks_Dict = new Dictionary<ChunkCoords, HexChunk>();
    private List<ChunkCoords> currentlyLoaded_Chunks_List = new List<ChunkCoords>();

    [Header("Hex Scripts Storage")]
    private HexCell_Data[,] dataHexCells_Arr;
    private HexCell[,] allHexsCells_Arr;

    ////////////////////////////////

    [Header("Camera Options")]
    public Camera cameraGenerated;
    public Vector2 cameraRelativePosition;

    [Header("Hex Map Options")]
    //public bool isShowingAllHexs;
    public bool isShowingBiomeVisuals;
    public bool isShowingGenerationTime;
    public bool isShowingToDos;

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

    [Header("Hex Gen Settings - RNG")]
    public int mapHex_Seed = 135135;

    //"Randomization States To Be Used
    private Random.State mapGeneration_SeededStated; //= Random.state;

    [Header("Map Gen Settings - Size")]
    public int mapGen_StartingBiomeNodesCount = 20;
    public int mapGen_SideLength = 160;
    public int mapGen_ChunkSize = 10;

    [Header("Map Gen Settings - Height")]
    public int mapHeightMin = 0;
    public int mapHeightMax = 5;
    public float mapHeightStep = 0.025f;
    public static readonly float hexCell_HeightPerStep = 0.04f;

    [Header("Map Gen Settings - Biomes")]
    public int mapGen_OceanSize = 2;
    public int mapGen_BeachSize = 2;

    ////////////////////////////////

    [Header("Biome Info Sets")]
    public BiomeInfo_SO[] allBiomes_Arr;

    [Header("Biome Map Visuals")]
    public GameObject biomeVisualQuad_Prefab;
    public GameObject biomeVisualContainer_GO;
    public List<Material> biomeVisualColoredMaterials_List;

    ////////////////////////////////

    [Header("Perlin Noise Settings")]
    public float perlinZoomScale = 30;
    public float offsetX = 0;
    public float offsetY = 0;


    [Header("Dynamic Loading Hex Settings")]
    public int chunkRenderDistance;
    public HexCell lastCameraCell;
    private HexCell_Data currentHexCell_UnderCamera;

    ////////////////////////////////

    [HideInInspector]
    public static Material[,] mergedBiomeMats_Arr;

    [HideInInspector]
    public SaveFile mySaveFile;



    /////////////////////////////////////////////////////////////////

    private void Start()
    {
        //Initialize The Setup Values
        Initialize_BiomeMatsArray();

        //Generate Then Spawn The Hex Map
        HexMap_Spawn();

       


        //HexGen_Spawn_OLD();

        //If Hidden Map Style Then Refresh at Start
        //if (!isShowingAllHexs)
        {
            //HexSpawn_HexsAroundCamera();
            //HexSpawn_ChunksAroundCamera();
        }
        //else
        {
            //HexSpawn_SpawnAllHexChunks();
            //HexSpawn_SpawnAllHexesFromData();

            //HexMap_Chunk();
        }
    }

    private void Update()
    {
        //Check If the perlin noise map offsets have been changed in inspector
        //UpdateCheck_PerlinOffsetChange();

        //Check For Respawing Map Inputs
        UpdateCheck_RegenerateMap();

        //Text Highlight Clicked Cells
        UpdateCheck_HexClicking();

        //Check For Re-Chunking Inputs
        UpdateCheck_ReChunking();

        //Not Used Yet
        UpdateCheck_SavingLoading();
    }

    private void FixedUpdate()
    {
        HexSpawn_ChunksAroundCamera();
    }

    /////////////////////////////////////////////////////////////////

  

    private void HexMap_Spawn()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Use the Generator to create the Hexs Needed to Spawn
        dataHexCells_Arr = MapGenerationController.HexMapGeneration(GenerateMapOptions());
        allHexsCells_Arr = new HexCell[mapGen_SideLength, mapGen_SideLength];

        //Clear Out All Old Maps
        HexSpawn_RemoveOldMap();

        //Spawn Ground Visuals
        HexSpawn_SpawnGround();

        //Check if Camera Focus is Biome Map or Hex Map For Visuals
        if (isShowingBiomeVisuals)
        {
            //Show the Biome Map Visually
            HexVisuals_DestroyVisuals();
            HexVisuals_DisplayVisuals();
        }
        else
        {
            //Center Camera on Hex Map
            HexVisuals_CenterCamera();
        }

        //Spawn Initial Pass Around Camera
        HexSpawn_ChunksAroundCamera();

        //Show Generation Time
        if (isShowingGenerationTime)
        {
            //Finish Counting Timer
            long endingTimeTicks = DateTime.UtcNow.Ticks;
            float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
            int mapHexGeneration_BiomeGrowthLoopCount = (int)Mathf.Log((float)mapGen_SideLength / mapGen_StartingBiomeNodesCount, 2);
            Debug.Log("Test Code: Biome Generation x" + mapHexGeneration_BiomeGrowthLoopCount + " Completed in: " + finishTime + "s");
            //Debug.Log("Test Code: Size " + mapHex_BiomeSets.GetLength(0) + "x" + mapHex_BiomeSets.GetLength(1));
        }
    }

    private MapGenerationOptions GenerateMapOptions()
    {
        MapGenerationOptions mapOptions = new MapGenerationOptions
        {
            isShowingGenerationTime = isShowingGenerationTime,
            isShowingToDos = isShowingToDos,

            mapGen_Seed = mapHex_Seed,
            mapGen_StartingBiomeNodesCount = mapGen_StartingBiomeNodesCount,
            mapGen_SideLength = mapGen_SideLength,
            mapGen_ChunkSize = mapGen_ChunkSize,
            mapGen_HeightMin = mapHeightMin,
            mapGen_HeightMax = mapHeightMax,
            mapGen_HeightStep = mapHeightStep,
            mapGen_HeightPerStep = hexCell_HeightPerStep,

            mapGen_OceanSize = mapGen_OceanSize,
            mapGen_BeachSize = mapGen_BeachSize,
            allBiomes_Arr = allBiomes_Arr,

            perlinZoomScale = perlinZoomScale,
            offsetX = offsetX,
            offsetY = offsetY
        };


        return mapOptions;
    }


    /////////////////////////////////////////////////////////////////

    private void HexSpawn_SpawnGround()
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

    private void HexSpawn_RemoveOldMap()
    {
        //Destory All Old Hex Chunks / Cells
        foreach (Transform child in hexMapContainer_GO.transform)
        {
            //Destroy Top-Level Child
            Destroy(child.gameObject);
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexVisuals_CenterCamera()
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
                //Color color = biomeVisualColoredMaterials_List[mapHex_BiomeSets[x, y]].color;
                //texture.SetPixel(x, y, color);
            }
        }

        //Apply Pixel Colors To Texture
        texture.Apply();

        //Recenter Camera
        cameraGenerated.transform.position = new Vector3(newHex.transform.position.x, newHex.transform.position.y, newHex.transform.position.z - 5);
        cameraGenerated.transform.rotation = new Quaternion(0,0,0,0);
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_HideAllHexes()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapGen_SideLength; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapGen_SideLength; y++)
            {
                allHexsCells_Arr[x, y].gameObject.SetActive(false);
            }
        }
    }

    private void HexMap_ShowAllHexes()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapGen_SideLength; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapGen_SideLength; y++)
            {
                allHexsCells_Arr[x, y].gameObject.SetActive(true);
            }
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_Chunk()
    {
        //foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
           // hexChunk.Chunk(hexChunkModel_Prefab, mergedBiomeMats_Arr);
        }
    }

    private void HexMap_Unchunk()
    {
        //Debug.Log("Test Code: Unchunk");

        //foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
            //hexChunk.Unchunk();
        }
    }

    private void HexMap_Rechunk()
    {
        //Debug.Log("Test Code: Rechunk");

        //foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
            //hexChunk.Rechunk();
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

    private void HexSpawn_SpawnAllHexesFromData()
    {
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < dataHexCells_Arr.GetLength(0); x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < dataHexCells_Arr.GetLength(1); y++)
            {
                //Create Gameobject And Find Chunk
                GameObject newHex;
                GameObject cellChunk = GetSearchable_ChunkFromCellCoords(x, y);

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
                newHexCell.CreateCell_CreateFromData(dataHexCells_Arr[x, y]);

                //Store it
                allHexsCells_Arr[x, y] = newHexCell;
            }
        }
    }

    private void HexSpawn_SpawnAllHexChunks()
    {

        /*
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
        */
    }

    private void HexSpawn_ChunksAroundCamera()
    {
        ////////////////////////////////

        //Get Camera Position
        Vector3 cameraPos = cameraGenerated.transform.position;

        //Get The Hex X Position From Camera Spacing
        int xPos = (int)Math.Round(cameraPos.z / spacing_I, 0);
        int yPos = (int)Math.Round(cameraPos.x / spacing_J, 0);

        //Clamp Positions to the array size
        xPos = Mathf.Clamp(xPos, 0, mapGen_SideLength - 1);
        yPos = Mathf.Clamp(yPos, 0, mapGen_SideLength - 1);

        //Check If Current Cam Position is Null
        if (currentHexCell_UnderCamera == null)
        {
            //Attempt a "False" node of Y + Chunk Size, so the rest of the code will update on Frame 1 When Comparing the under camera node.
            int yPos_Modded = Mathf.Clamp(yPos + mapGen_ChunkSize, 0, mapGen_SideLength - 1);

            //Get Cell Under Camera
            currentHexCell_UnderCamera = dataHexCells_Arr[xPos, yPos_Modded];
        }

        //Check Easy Hex Positions - Perhaps Redundant?
        if ((xPos == currentHexCell_UnderCamera.hexCoords.x) && (yPos == currentHexCell_UnderCamera.hexCoords.y))
        {
            //Cancel
            //return;
        }


        //Get New Chunk Array Positions
        int oldChunkCoordX = (int)Mathf.Floor(currentHexCell_UnderCamera.hexCoords.x / mapGen_ChunkSize);
        int oldChunkCoordY = (int)Mathf.Floor(currentHexCell_UnderCamera.hexCoords.y / mapGen_ChunkSize);
        int chunkCoordX = (int)Mathf.Floor(dataHexCells_Arr[xPos, yPos].hexCoords.x / mapGen_ChunkSize);
        int chunkCoordY = (int)Mathf.Floor(dataHexCells_Arr[xPos, yPos].hexCoords.y / mapGen_ChunkSize);
        
        //Update Cell Under Camera
        currentHexCell_UnderCamera = dataHexCells_Arr[xPos, yPos];

        //Check Harder Chunk Positions
        if ((chunkCoordX == oldChunkCoordX) && (chunkCoordY == oldChunkCoordY))
        {
            //Cancel
            return;
        }

        ////////////////////////////////

        //Clean Old Map
        //HexGen_ClearOldMaps();


        ////////////////////////////////

        //Get Cell Under Camera
        HexCell_Data hexCellUnderCamera = dataHexCells_Arr[xPos, yPos];

        ////////////////////////////////

        

        //Use this later for reloading only inactive chunks
        if (hexChunks_Dict.ContainsKey(new ChunkCoords(chunkCoordX, chunkCoordY)))
        {
            //Debug.Log("Test Code: Not Null");
            //return;
        }





        //Collect List Of Possible Chunks Around Camera Chunk
        List<ChunkCoords> possibleChunkPositions_List = GetValidChunks_AroundCamera(chunkCoordX, chunkCoordY, chunkRenderDistance);

        //Refresh Old and New Chunks For Reloading
        possibleChunkPositions_List = HexGen_RefreshChunks(possibleChunkPositions_List);

        foreach (ChunkCoords currentCoords in possibleChunkPositions_List)
        {
            //Spawn New Chunk Object
            GameObject newChunk = Instantiate(hexChunk_Prefab, new Vector3(0, 0, 0), Quaternion.identity, hexMapContainer_GO.transform);





            //Setup Chunk Script
            hexChunks_Dict[new ChunkCoords(currentCoords.x, currentCoords.y)] = newChunk.GetComponent<HexChunk>();


         


            //Create List Of Hexs For the Chunk
            List<HexCell_Data> wantedHexs_List = hexChunks_Dict[new ChunkCoords(currentCoords.x, currentCoords.y)].CreateChunk_DynamicLoader(currentCoords.x, currentCoords.y, mapGen_ChunkSize, dataHexCells_Arr);

            //Spawn The Hexs Inside the Chunk
            HexSpawn_ListedHexs(wantedHexs_List, hexChunks_Dict[new ChunkCoords(currentCoords.x, currentCoords.y)].transform);
        }





        //Old???
        //allHexChunksV2_Arr[chunkCoordX, chunkCoordY].CreateChunk(chunkCoordX, chunkCoordY);


        //Middle Chunk Needed???
        //currentChunks_List.Add(allHexChunksV2_Arr[chunkCoordX, chunkCoordY]);





        //Create Array Of All Hexs inside of the Chunk
        //foreach (HexChunk hexChunk in currentChunks_List)
        {
            
        }





        //Chunk Together?
        //hexChunk.Chunk(hexChunkModel_Prefab, mergedBiomeMats_Arr);
    }

    private List<ChunkCoords> HexGen_RefreshChunks(List<ChunkCoords> possibleChunkCoords_List)
    {
        //Create the returning list
        List<ChunkCoords> outputChunkCoords_List = new List<ChunkCoords>();
        List<ChunkCoords> keeperChunkCoords_List = new List<ChunkCoords>();
        List<ChunkCoords> removableChunkCoords_List = new List<ChunkCoords>();

        //Remove Or Keep Chunks That have Avaliblity In the New Possible Set
        for (int i = 0; i < currentlyLoaded_Chunks_List.Count; i++)
        {

            if (possibleChunkCoords_List.Contains(new ChunkCoords(currentlyLoaded_Chunks_List[i].x, currentlyLoaded_Chunks_List[i].y)))
            {
                keeperChunkCoords_List.Add(currentlyLoaded_Chunks_List[i]);
            }
            else
            {
                removableChunkCoords_List.Add(currentlyLoaded_Chunks_List[i]);
            }
        }

        foreach (ChunkCoords chunkCoords in removableChunkCoords_List)
        {
            if (hexChunks_Dict[chunkCoords] != null)
            { 
                hexChunks_Dict[chunkCoords].currentState = HexChunk.ChunkState.Inactive;

                currentlyLoaded_Chunks_List.Remove(chunkCoords);

                //Destroy Top-Level Child
                Destroy(hexChunks_Dict[chunkCoords].gameObject);
            }
        }


        outputChunkCoords_List = possibleChunkCoords_List.Except(keeperChunkCoords_List).ToList();


        for (int i = 0; i < outputChunkCoords_List.Count; i++)
        {
            currentlyLoaded_Chunks_List.Add(outputChunkCoords_List[i]);
        }



        return outputChunkCoords_List;
    }

   

    private void HexSpawn_ListedHexs(List<HexCell_Data> wantedHexs_List, Transform chunkContainer)
    {
        foreach (HexCell_Data hexCellData in wantedHexs_List)
        {
            //Create Gameobject And Find Chunk
            GameObject newHex;

            //Regular Spawn Position VS Offset Spacing
            if (hexCellData.hexCoords.y % 2 == 0)
            {
                newHex = Instantiate(hexMesh_Prefab, new Vector3(hexCellData.hexCoords.y * spacing_J, mapHeightStep, hexCellData.hexCoords.x * spacing_I), Quaternion.identity, chunkContainer);
            }
            else
            {
                newHex = Instantiate(hexMesh_Prefab, new Vector3(hexCellData.hexCoords.y * spacing_J, mapHeightStep, hexCellData.hexCoords.x * spacing_I + offcenter_I), Quaternion.identity, chunkContainer);
            }

            //Setup Cell
            HexCell newHexCell = newHex.GetComponent<HexCell>();
            newHexCell.CreateCell_CreateFromData(hexCellData);

            //Store it
            allHexsCells_Arr[hexCellData.hexCoords.x, hexCellData.hexCoords.y] = newHexCell;
        }
    }

    /////////////////////////////////////////////////////////////////

    private void Initialize_BiomeMatsArray()
    {
        //Search For the highest Biome Mat ID to create an array size
        int highestBiomeMatID = 0;
        for (int i = 0; i < allBiomes_Arr.Length; i++)
        {
            //Loop Each Biome Cell Value
            for (int j = 0; j < allBiomes_Arr[i].biomeCellsInfo_Arr.Length; j++)
            {
                //Check if newest Value is Higher
                if (allBiomes_Arr[i].biomeCellsInfo_Arr[j].matID > highestBiomeMatID)
                {
                    //Replace the value then keep searching
                    highestBiomeMatID = allBiomes_Arr[i].biomeCellsInfo_Arr[j].matID;
                }
            }
        }

        //Create a new Array with Sizes Using Biome Count + highest Biome Mat ID found
        mergedBiomeMats_Arr = new Material[allBiomes_Arr.Length, highestBiomeMatID + 1];

        //Search the Biome Sets For each New Material then add them to the new compressed / merged array
        for (int i = 0; i < allBiomes_Arr.Length; i++)
        {
            //A Starting Value to 0 does not overwrite
            int lastFoundID = -1;

            for (int j = 0; j < allBiomes_Arr[i].biomeCellsInfo_Arr.Length; j++)
            {
                if (lastFoundID != allBiomes_Arr[i].biomeCellsInfo_Arr[j].matID)
                {
                    lastFoundID = allBiomes_Arr[i].biomeCellsInfo_Arr[j].matID;
                    mergedBiomeMats_Arr[i, lastFoundID] = allBiomes_Arr[i].biomeCellsInfo_Arr[j].material;
                }
            }
        }
    }

    /////////////////////////////////////////////////////////////////

    private void UpdateCheck_RegenerateMap()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //if (isShowingAllHexs)
            {
                //Load A New Seed then Spawn a new Map
                //mapHex_Seed = Random.Range(100000, 999999);
                //HexGen_SetMapSeed();
                //HexGen_Spawn_OLD();
            }
            //else
            {
                //HexSpawn_ChunksAroundCamera();
                //HexSpawn_HexsAroundCamera();
            }
        }
    }

    private void UpdateCheck_ReChunking()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            HexMap_Unchunk();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            HexMap_Rechunk();
        }
    }

    private void UpdateCheck_HexClicking()
    {
        //Check For Click Input
        if (Input.GetMouseButtonDown(0))
        {
            //Spawn a Ray From Mouse To World
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If an object was hit then check for Hex Cell
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                //Look for the HexCell on the hit object
                HexCell hitCell = hit.collider.gameObject.transform.parent.GetComponentInParent<HexCell>();

                //Check if a HexCell was found by the hit
                if (hitCell != null)
                {
                    //Click Action the Cell
                    hitCell.ClickCell();
                }
            }
        }
    }

    private void UpdateCheck_SavingLoading()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            HexMap_CreateSave();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            HexMap_LoadSave();
        }
    }

    /////////////////////////////////////////////////////////////////

    private GameObject GetSearchable_ChunkFromCellCoords(int x, int y)
    {
        ChunkCoords chunkCoords = new ChunkCoords((int)Mathf.Floor(x / mapGen_ChunkSize), (int)Mathf.Floor(y / mapGen_ChunkSize));
        return hexChunks_Dict[chunkCoords].gameObject;
        

        //return allHexChunks_Arr[(int)Mathf.Floor(x / mapGen_ChunkSize), (int)Mathf.Floor(y / mapGen_ChunkSize)].gameObject;
    }

    public static Material GetSearchable_BiomeMaterial(int biomeID, int matID)
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

    private void OLDCODE_SetCodeGradients()
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

    private void OLDCODE_SaveMeshAsset()
    {
        //AssetDatabase.CreateAsset(allHexsCells_Arr[0,0].hexObject_MeshFilter.mesh, "Assets/NewHexMesh.mesh");
        //AssetDatabase.SaveAssets(); 
    }

    private void OLDCODE_HexSpawn_HexsAroundCamera()
    {
        /*
        //Get Camera Position
        Vector3 cameraPos = cameraGenerated.transform.position;

        //Get The Hex X Position From Camera Spacing
        int xPos = (int)Math.Round(cameraPos.z / spacing_I, 0);
        int yPos = (int)Math.Round(cameraPos.x / spacing_J, 0);

        //Clamp Positions to the array size
        xPos = Mathf.Clamp(xPos, 0, mapGen_SideLength - 1);
        yPos = Mathf.Clamp(yPos, 0, mapGen_SideLength - 1);

        //Get Cell Under Camera
        HexCell hexCellUnderCamera = allHexsCells_Arr[xPos, yPos];

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
            lastCameraCell = hexCellUnderCamera;
        }
        else if ((lastCameraCell.hexCoords.x == hexCellUnderCamera.hexCoords.x) && (lastCameraCell.hexCoords.y == hexCellUnderCamera.hexCoords.y))
        {
            Debug.Log("Test Code: This Does Not Trigger!");
            //Do Nothing
            return;
        }
        else
        {
            lastCameraCell = hexCellUnderCamera;
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
        //HexSpawn_ListedHexs(wantedHexs_List);




        //Debug.Log("Test Code: " + allHexsCells_Arr[xPos, yPos].name);

        //currentCameraCell = allHexsCells_Arr[xPos, zPos];

        //Debug.Log("Test Code: x:" + xPos);
        //Debug.Log("Test Code: z:" + zPos);

        //Get Closest Node To Camera Position
        //dataHexCells_Arr[]

        //Get all 4 cornoer nodes away from cetner point

        //Get All Nodes Between 4 corners


    */

    }

    private void OLDCODE_UpdateCheck_PerlinOffsetChange()
    {
        //if (oldOffsetX != offsetX)
        {
            //oldOffsetX = offsetX;
            //HexGenHeight_Height();
        }

        //if (oldOffsetY != offsetY)
        {
            //oldOffsetY = offsetY;
            //HexGenHeight_Height();
        }
    }

    private void OLDCODE_HexGen_Spawn()
    {
        /*
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Setup Array Values
        dataHexCells_Arr = new HexCell_Data[mapGen_SideLength, mapGen_SideLength];
        allHexsCells_Arr = new HexCell[mapGen_SideLength, mapGen_SideLength];


        //allHexChunksV2_Arr = new HexChunk[(int)Mathf.Ceil(mapGen_SideLength / mapGen_ChunkSize), (int)Mathf.Ceil(mapGen_SideLength / mapGen_ChunkSize)];


        //Create Generation Arrays By Size
        //mapHex_HeightSets = new int[mapGen_SideLength, mapGen_SideLength];
        //mapHex_MatIDSets = new int[mapGen_SideLength, mapGen_SideLength];
        //mapHex_ColorSets = new string[mapGen_SideLength, mapGen_SideLength];

        ////////////////////////////////

        //Generate Map Seed For Random Values
        //HexGen_SetMapSeed();

        //Create Base Display List Setups
        //HexGenBiome_CreateInitialBiomes();

        //Use Log() to calucale how many loops are needed to get to the side length value
        int mapHexGeneration_BiomeGrowthLoopCount = (int)Mathf.Log((float)mapGen_SideLength / mapGen_StartingBiomeNodesCount, 2);

        //For The Amount of times needed to double the inital Array, zoom out then Fill Map
        for (int i = 0; i < mapHexGeneration_BiomeGrowthLoopCount; i++)
        {
            //Zoom Then Fill To Expand Map
            //HexGenBiome_ZoomOutBiome();
            //HexGenBiome_FillZeros();

            if (isShowingToDos)
            {
                Debug.Log("Test Code: Needs Smoothing!");
            }
        }

        //These Converge into lakes at the end 
        //exGenBiome_PostGeneration_Ocean();
        //HexGenBiome_PostGeneration_Beaches();
        //HexGeneration_PostGeneration_Rivers();
        //HexGeneration_PostGeneration_Lakes();
        //HexGeneration_PostGeneration_InterBiomes();


        if (isShowingToDos)
        {
            Debug.Log("Test Code: Use a Biome Weight System! For Color AND Height smoothing!");
        }



        //HexMap_SetupMatsArray();

        //HexGenHeight_Height();
        //HexGen_MatsAndColors();
   


        //Merge Info Together Into Storable Data Hex Cells
        //HexGen_MergeRawDataToHexDataCells();


        //HexMap_SpawnAllHexChunks();
        HexGen_ClearOldMaps();




        //Ground Visuals
        HexSpawn_SpawnGround();

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
            HexVisuals_CenterCamera();
        }

       


        //Show Generation Time
        if (isShowingGenerationTime)
        {
            //Finish Counting Timer
            long endingTimeTicks = DateTime.UtcNow.Ticks;
            float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
            Debug.Log("Test Code: Biome Generation x" + mapHexGeneration_BiomeGrowthLoopCount + " Completed in: " + finishTime + "s");
            //Debug.Log("Test Code: Size " + mapHex_BiomeSets.GetLength(0) + "x" + mapHex_BiomeSets.GetLength(1));
        }
        */
    }

    /////////////////////////////////////////////////////////////////





    private List<ChunkCoords> GetValidChunks_AroundCamera(int X, int Y, int rangeAddition)
    {
        //Create a Returnable List of Values of corect Hexs
        List<ChunkCoords> acceptedRangeValueSets_List = new List<ChunkCoords>();



        int rightCorner = Mathf.Clamp(X + rangeAddition + 1, 0, mapGen_SideLength - 0);
        int leftCorner = Mathf.Clamp(X - rangeAddition, 0, mapGen_SideLength - 1);

        int topCorner = Mathf.Clamp(Y + rangeAddition + 1, 0, mapGen_SideLength - 0);
        int bottomCorner = Mathf.Clamp(Y - rangeAddition, 0, mapGen_SideLength - 1);




        for (int x = leftCorner; x < rightCorner; x++)
        {
            for (int y = bottomCorner; y < topCorner; y++)
            {

                //Check Bounds
                if (CheckChunkRangeBounds(x, y))
                {
                    //Add New Possible Chunk
                    acceptedRangeValueSets_List.Add(new ChunkCoords(x, y));
                }


            }
        }




        //Return The New List
        return acceptedRangeValueSets_List;


        //Create Value List To 
        List<int> ValuesList = new List<int>();

        //Base values for width / height always added
        ValuesList.Add(1);
        ValuesList.Add(0);
        ValuesList.Add(-1);

        //Extra values added dependant on size
        for (int index = rangeAddition; index >= 2; index--)
        {
            ValuesList.Add(index);
            ValuesList.Add(-index);
        }

        //Add the Camera Position Itself 
        acceptedRangeValueSets_List.Add(new ChunkCoords(X, Y));

        ////////////////////////////////

        //North
        foreach (int x_Value in ValuesList)
        {
            //Set X / Y values based off direction
            int New_X = X + x_Value;
            int New_Y = Y + rangeAddition;

            //Check Bounds
            if (CheckChunkRangeBounds(New_X, New_Y))
            {
                //Add New Possible Chunk
                acceptedRangeValueSets_List.Add(new ChunkCoords(New_X, New_Y));
            }
        }

        //South 
        foreach (int x_Value in ValuesList)
        {
            //Set X / Y values based off direction
            int New_X = X + x_Value;
            int New_Y = Y - rangeAddition;

            //Check Bounds
            if (CheckChunkRangeBounds(New_X, New_Y))
            {
                //Add New Possible Chunk
                acceptedRangeValueSets_List.Add(new ChunkCoords(New_X, New_Y));
            }
        }

        //East
        foreach (int y_Value in ValuesList)
        {
            //Set X / Y Values
            int New_X = X + rangeAddition;
            int New_Y = Y - y_Value;

            //Check Bounds
            if (CheckChunkRangeBounds(New_X, New_Y))
            {
                //Add New Possible Chunk
                acceptedRangeValueSets_List.Add(new ChunkCoords(New_X, New_Y));
            }
        }

        //West
        foreach (int y_Value in ValuesList)
        {
            //Set X / Y Values
            int New_X = X - rangeAddition;
            int New_Y = Y - y_Value;

            //Check Bounds
            if (CheckChunkRangeBounds(New_X, New_Y))
            {
                //Add New Possible Chunk
                acceptedRangeValueSets_List.Add(new ChunkCoords(New_X, New_Y));
            }
        }

        //Return The New List
        return acceptedRangeValueSets_List;
    }


    public bool CheckChunkRangeBounds(int X, int Y)
    {
        //Default False Bools
        bool validRange = false;
        bool validRangeX = false;
        bool validRangeY = false;

        //Generate a Max Chunk Value Allowed
        int chunksAllowed = mapGen_SideLength / mapGen_ChunkSize;

        //Valid X Vaule
        if (X >= 0 && X <= chunksAllowed)
        {
            validRangeX = true;
        }

        //Valid Y Vaule
        if (Y >= 0 && Y <= chunksAllowed)
        {
            validRangeY = true;
        }

        //Check Both Values Together
        if (validRangeX == true && validRangeY == true)
        {
            validRange = true;
        }

        //Return Chunk Validity
        return validRange;
    }


    /////////////////////////////////////////////////////////////////
}