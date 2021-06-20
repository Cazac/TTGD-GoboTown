using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapSpawnController : MonoBehaviour
{
    ////////////////////////////////

    [Header("Singleton Refference")]
    public static MapSpawnController Instance;

    ////////////////////////////////

    [Header("Sector Storage")]
    private Dictionary<HexSectorCoords, HexSector> hexSectors_Dict = new Dictionary<HexSectorCoords, HexSector>();

    //public HexSector debugHexSector;

    [Header("Currently Active Chunks")]
    private List<HexChunkCoords> currentlyLoaded_Chunks_List = new List<HexChunkCoords>();

    ////////////////////////////////

    [Header("Hex Map Bool Options")]
    public bool isShowingBiomeVisuals;

    [Header("Hex Map Generation Options")]
    public MapGenerationOptions_SO mapGenOpts_SO;

    ////////////////////////////////

    [Header("Dynamic Loading Hex Settings")]
    public int chunkRenderDistance;

    [Header("Camera Options")]
    public Camera cameraGenerated;
    public Vector2 cameraRelativePosition;

    ////////////////////////////////
    
    [Header("Hex Map Containers")]
    public GameObject setupGround_GO;
    public GameObject scalingGround_GO;
    public GameObject hexMapContainer_GO;

    [Header("Hex Map Prefabs")]
    public GameObject hexMesh_Prefab;
    public GameObject hexChunk_Prefab;
    public GameObject hexChunkModel_Prefab;

    [Header("Decoration Prefabs")]
    public GameObject hexGrass_Prefab;

    ////////////////////////////////

    [Header("Biome Map Visuals")]
    public GameObject biomeVisualQuad_Prefab;
    public GameObject biomeVisualContainer_GO;

    ////////////////////////////////

    [Header("Hex Uneditable Sizes")]
    public const float outerRadius = 0.1f;
    public const float innerRadius = outerRadius * 0.866025404f;
    public const float spacing_I = innerRadius * 2f;
    public const float spacing_J = outerRadius * 1.5f;
    public const float offcenter_I = spacing_I / 2;

    ////////////////////////////////


    public HexCellCoords lastCellCoords_UnderCamera = new HexCellCoords(0, 0, 0);

    [HideInInspector]
    public Material[,] mergedBiomeMats_Arr;

    [HideInInspector]
    public SaveFile mySaveFile;

    ////////////////////////////////





    /////////////////////////////////////////////////////////////////

    private void Awake()
    {
        //Setup Singleton Refference
        Instance = this;
    }

    private void Start()
    {
        //Generate an Initial Sector and Preform Setup 
        HexMap_Initialize();

        //Spawn The Hex Map
        HexMap_Spawn();
    }

    private void Update()
    {
        //Check For Respawing Map Inputs
        UpdateCheck_RegenerateMap();

        //Text Highlight Clicked Cells
        UpdateCheck_HexClicking();

        //Check For Re-Chunking Inputs
        UpdateCheck_ReChunking();

        //Not Used Yet
        UpdateCheck_SavingLoading();

        //Debug.Log("Test Code: Sector Count: " + hexSectors_Dict.Count);
    }

    private void FixedUpdate()
    {
        //Check If Camera is in new location
        HexSpawn_CameraChunks();
    }

    /////////////////////////////////////////////////////////////////

    private void MapSetup_BiomeMatsArray()
    {
        //Search For the highest Biome Mat ID to create an array size
        int highestBiomeMatID = 0;
        for (int i = 0; i < mapGenOpts_SO.allBiomes_Arr.Length; i++)
        {
            //Loop Each Biome Cell Value
            for (int j = 0; j < mapGenOpts_SO.allBiomes_Arr[i].biomeCellsInfo_Arr.Length; j++)
            {
                //Check if newest Value is Higher
                if (mapGenOpts_SO.allBiomes_Arr[i].biomeCellsInfo_Arr[j].matID > highestBiomeMatID)
                {
                    //Replace the value then keep searching
                    highestBiomeMatID = mapGenOpts_SO.allBiomes_Arr[i].biomeCellsInfo_Arr[j].matID;
                }
            }
        }

        //Create a new Array with Sizes Using Biome Count + highest Biome Mat ID found
        mergedBiomeMats_Arr = new Material[mapGenOpts_SO.allBiomes_Arr.Length, highestBiomeMatID + 1];

        //Search the Biome Sets For each New Material then add them to the new compressed / merged array
        for (int i = 0; i < mapGenOpts_SO.allBiomes_Arr.Length; i++)
        {
            //A Starting Value to 0 does not overwrite
            int lastFoundID = -1;

            for (int j = 0; j < mapGenOpts_SO.allBiomes_Arr[i].biomeCellsInfo_Arr.Length; j++)
            {
                if (lastFoundID != mapGenOpts_SO.allBiomes_Arr[i].biomeCellsInfo_Arr[j].matID)
                {
                    lastFoundID = mapGenOpts_SO.allBiomes_Arr[i].biomeCellsInfo_Arr[j].matID;
                    mergedBiomeMats_Arr[i, lastFoundID] = mapGenOpts_SO.allBiomes_Arr[i].biomeCellsInfo_Arr[j].material;
                }
            }
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_Initialize()
    {
        //Initialize The Setup Values
        MapSetup_BiomeMatsArray();

        //Clean Dictionary Before Use
        hexSectors_Dict.Clear();

        //Use the Generator to create the Hexs Needed to Spawn
        hexSectors_Dict.Add(new HexSectorCoords(0, 0), MapGenerationController.HexMapGenerate_InitialSector(mapGenOpts_SO, new HexSectorCoords(0, 0)));

        //Generate Other Defaults
        HexMap_CreateNewSector(new HexSectorCoords(1, 0));
        HexMap_CreateNewSector(new HexSectorCoords(-1, 0));
        HexMap_CreateNewSector(new HexSectorCoords(0, 1));
        HexMap_CreateNewSector(new HexSectorCoords(0, -1));
        
        //Setup Chunks In Pooler
        HexPoolingController.Instance.SetupInitialPool_Chunk();
    }

    private void HexMap_CreateNewSector(HexSectorCoords sectorCoords)
    {
        //Use the Generator to create the Hexs Needed to Spawn
        hexSectors_Dict.Add(sectorCoords, MapGenerationController.HexMapGeneration_NewSector(mapGenOpts_SO, sectorCoords));
    }

    private void HexMap_Spawn()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Clear Out All Old Maps
        HexSpawn_RemoveOldMap();

        //Spawn Ground Visuals
        HexSpawn_SpawnInitialSectorGround();

        //Check if Camera Focus is Biome Map or Hex Map For Visuals
        if (isShowingBiomeVisuals)
        {
            //Destory Old Maps Then Show All Biome Maps Visually
            HexVisuals_DestroyVisuals();
            foreach (KeyValuePair<HexSectorCoords, HexSector> sector_KeyVal in hexSectors_Dict)
            {
                HexVisuals_DisplayVisuals(sector_KeyVal.Key);
            }
        }
        else
        {
            //Center Camera on Hex Map
            HexVisuals_CenterCamera();
        }

        //Show Generation Time
        if (mapGenOpts_SO.isShowingGenerationTime)
        {
            //Finish Counting Timer
            long endingTimeTicks = DateTime.UtcNow.Ticks;
            float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
            int mapHexGeneration_BiomeGrowthLoopCount = (int)Mathf.Log((float)mapGenOpts_SO.mapGen_SectorTotalSize / mapGenOpts_SO.mapGen_StartingBiomeNodesCount, 2);
            Debug.Log("Map Spawn in: " + finishTime + "s" + " (Chunks/Hexs: " + (Mathf.Pow(chunkRenderDistance, 2) + 1) + " x " + mapGenOpts_SO.mapGen_ChunkSize + " = " + (Mathf.Pow(chunkRenderDistance, 2) + 1) * mapGenOpts_SO.mapGen_ChunkSize + ")");
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexSpawn_SpawnInitialSectorGround()
    {
        //Turn Off Old Map From Setup
        setupGround_GO.SetActive(false);

        //Setup New Mesh
        Mesh newMesh = new Mesh();
        List<Vector3> verts_List = new List<Vector3>();
        List<int> tris_List = new List<int>();

        //Setup Sizing
        float extraTrim = 0.15f;
        float x = innerRadius * mapGenOpts_SO.mapGen_SectorTotalSize * 1.75f;
        float y = outerRadius * mapGenOpts_SO.mapGen_SectorTotalSize * 1.75f;

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

    private void HexSpawn_CameraChunks()
    {
        ////////////////////////////////

        //Fetch Hex Cell Under Camera 
        HexCellCoords newCellCoords_UnderCamera = GetHexCellCoords_UnderCamera();

        //Check For New Sector Generatation
        GetCheckNewSectors_ByCellCoords(newCellCoords_UnderCamera);

        /*
        //Get New Chunk Array Positions
        int oldChunkCoordX = lastCellCoords_UnderCamera.x / mapGenOpts_SO.mapGen_ChunkSize;
        int oldChunkCoordY = lastCellCoords_UnderCamera.y / mapGenOpts_SO.mapGen_ChunkSize;
        int chunkCoordX = newCellCoords_UnderCamera.x / mapGenOpts_SO.mapGen_ChunkSize;
        int chunkCoordY = newCellCoords_UnderCamera.y / mapGenOpts_SO.mapGen_ChunkSize;

        if (lastCellCoords_UnderCamera.x - 1 < 0)
        {
            oldChunkCoordX -= 1;
        }

        if (lastCellCoords_UnderCamera.y - 1 < 0)
        {
            oldChunkCoordY -= 1;
        }

        if (newCellCoords_UnderCamera.x - 1 < 0)
        {
            chunkCoordX -= 1;
        }


        if (newCellCoords_UnderCamera.y - 1 < 0)
        {
            chunkCoordY -= 1;
        }
        */

        //Debug.Log("Test Code: " + newCellCoords_UnderCamera.GetPrintableCoords());



        //Get New Chunk Array Positions
        int oldChunkCoordX = (int)Mathf.Floor((float)lastCellCoords_UnderCamera.x / mapGenOpts_SO.mapGen_ChunkSize);
        int oldChunkCoordY = (int)Mathf.Floor((float)lastCellCoords_UnderCamera.y / mapGenOpts_SO.mapGen_ChunkSize);
        int chunkCoordX = (int)Mathf.Floor((float)newCellCoords_UnderCamera.x / mapGenOpts_SO.mapGen_ChunkSize);
        int chunkCoordY = (int)Mathf.Floor((float)newCellCoords_UnderCamera.y / mapGenOpts_SO.mapGen_ChunkSize);



        //Update Cell Under Camera
        lastCellCoords_UnderCamera = newCellCoords_UnderCamera;

        //Check that the current chunk is different then last frame
        if ((chunkCoordX == oldChunkCoordX) && (chunkCoordY == oldChunkCoordY))
        {
            //Chunk is still the same, Leave it be.
            return;
        }

        ////////////////////////////////

        //Collect List Of Possible Chunks Around Camera Chunk Then Refresh Old and New Chunks For Reloading
        List<HexChunkCoords> chunksCoordsToBeLoaded_List = GetHexChunks_AroundCamera(chunkCoordX, chunkCoordY, chunkRenderDistance);
        chunksCoordsToBeLoaded_List = HexSpawn_RefreshActiveChunkList(chunksCoordsToBeLoaded_List);

        //Load All Chunks That are listed to be loaded
        foreach (HexChunkCoords currentCoords in chunksCoordsToBeLoaded_List)
        {
            //Activate A New Chunk Then Set The Chunk As Active
            HexChunk newChunk = HexPoolingController.Instance.ActivateObject();
            newChunk.chunkCoords = currentCoords;

            //Setup The Hexs In The Array That Were Spawned By The Pooler
            SetHexChunk(newChunk);
            newChunk.SetChunkActive(currentCoords, GetHexCellDataList_ByChunk(currentCoords));

            //CHUNK THIS CHUNK
            newChunk.Chunk();
        }

    }

    private List<HexChunkCoords> HexSpawn_RefreshActiveChunkList(List<HexChunkCoords> possibleChunkCoords_List)
    {
        //Create the returning list
        List<HexChunkCoords> outputChunkCoords_List = new List<HexChunkCoords>();
        List<HexChunkCoords> keeperChunkCoords_List = new List<HexChunkCoords>();
        List<HexChunkCoords> removableChunkCoords_List = new List<HexChunkCoords>();

        //Remove Or Keep Chunks That have Avaliblity In the New Possible Set
        for (int i = 0; i < currentlyLoaded_Chunks_List.Count; i++)
        {
            //Check If The Chunk is Still in Range
            if (possibleChunkCoords_List.Contains(new HexChunkCoords(currentlyLoaded_Chunks_List[i].x, currentlyLoaded_Chunks_List[i].y)))
            {
                //Store These Chunks To Not Be Loaded Or Removed
                keeperChunkCoords_List.Add(currentlyLoaded_Chunks_List[i]);
            }
            else
            {
                //Store These Chunks To Be Removed
                removableChunkCoords_List.Add(currentlyLoaded_Chunks_List[i]);
            }
        }

        //Loop All Removing Chunks For Deactivation
        foreach (HexChunkCoords chunkCoords in removableChunkCoords_List)
        {
            //Get The Chunk Script And Deactivate it
            HexChunk hexChunk = GetHexChunk_ByChunk(chunkCoords);
            HexPoolingController.Instance.DeactivateObject(hexChunk);
            currentlyLoaded_Chunks_List.Remove(chunkCoords);
        }

        //Filter The New Spawn Chunks as possibles minus the Keepers
        outputChunkCoords_List = possibleChunkCoords_List.Except(keeperChunkCoords_List).ToList();

        //Add the New Outputed Chunks to the Current List
        for (int i = 0; i < outputChunkCoords_List.Count; i++)
        {
            currentlyLoaded_Chunks_List.Add(outputChunkCoords_List[i]);
        }

        //Return the List
        return outputChunkCoords_List;
    }

    /////////////////////////////////////////////////////////////////

    private void HexVisuals_CenterCamera()
    {
        //Figure out the X Position by size of the generated map
        float extraTrim = 0.15f;
        float x = innerRadius * mapGenOpts_SO.mapGen_SectorTotalSize * 1.75f;
        float y = outerRadius * mapGenOpts_SO.mapGen_SectorTotalSize * 1.75f;
        float left = -extraTrim;
        float right = x + extraTrim;
        float up = -extraTrim;
        float down = y + extraTrim;
        float xPos = Mathf.Lerp(left, right, 0.5f);
        float yPos = Mathf.Lerp(up, down, 0.5f);

        //Set Camera Position To Look at map from a good angle
        cameraGenerated.transform.position = new Vector3(xPos, cameraRelativePosition.x, yPos);
    }

    private void HexVisuals_DestroyVisuals()
    {
        //Destory All Old Hex Chunks / Cells
        foreach (Transform child in biomeVisualContainer_GO.transform)
        {
            //Destroy Top-Level Child
            Destroy(child.gameObject);
        }
    }

    private void HexVisuals_DisplayVisuals(HexSectorCoords sectorCoords)
    {
        //Create a new texture using the biome ints as colors 
        Texture2D texture = new Texture2D(mapGenOpts_SO.mapGen_SectorTotalSize, mapGenOpts_SO.mapGen_SectorTotalSize);
        GameObject newHex = Instantiate(biomeVisualQuad_Prefab, new Vector3((sectorCoords.x  * 5f), 2f + (sectorCoords.y * 5f), -2f), Quaternion.identity, biomeVisualContainer_GO.transform);
        newHex.GetComponent<Renderer>().material.mainTexture = texture;

        int sectorOffsetX = sectorCoords.x * mapGenOpts_SO.mapGen_SectorTotalSize;
        int sectorOffsetY = sectorCoords.y * mapGenOpts_SO.mapGen_SectorTotalSize;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                //Get Sector Data Cell
                //HexCell_Data dataCell = GetHexCellData_ByCell(x, y);
                HexCell_Data dataCell = GetHexCellDataLocal_ByCell(new HexCellCoords(x + sectorOffsetX, y + sectorOffsetY, 0));

                ColorUtility.TryParseHtmlString("#" + dataCell.hexCell_Color, out Color convertedColor);
                texture.SetPixel(x, y, convertedColor);
            }
        }

        //Apply Pixel Colors To Texture
        texture.Apply();

        //Recenter Camera
        cameraGenerated.transform.position = new Vector3(newHex.transform.position.x, newHex.transform.position.y, newHex.transform.position.z - 5);
        cameraGenerated.transform.rotation = new Quaternion(0,0,0,0);
    }

    /////////////////////////////////////////////////////////////////

    private void UpdateCheck_RegenerateMap()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isShowingBiomeVisuals)
            {
                //Destory Old Maps Then Show All Biome Maps Visually
                HexVisuals_DestroyVisuals();
                foreach (KeyValuePair<HexSectorCoords, HexSector> sector_KeyVal in hexSectors_Dict)
                {
                    HexVisuals_DisplayVisuals(sector_KeyVal.Key);
                }
            }
            else
            {
                //Load A New Seed then Spawn a new Map
                mapGenOpts_SO.mapGen_Seed = Random.Range(100000, 999999);

                //Generate an Initial Sector and Preform Setup 
                HexMap_Initialize();

                //Spawn The Hex Map
                HexMap_Spawn();
            }
        }
    }

    private void UpdateCheck_ReChunking()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            //HexMap_Unchunk();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            //HexMap_Rechunk();
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
            //HexMap_CreateSave();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            //HexMap_LoadSave();
        }
    }

    /////////////////////////////////////////////////////////////////

    private HexCell_Data GetHexCellDataLocal_ByCell(HexCellCoords cellCoords)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByCellCoords(cellCoords);

        //Get Sector Size Offsets To Access Them In The Array
        //int sectorOffsetX = sectorCoords.x * mapGenOpts_SO.mapGen_SectorTotalSize;
        //int sectorOffsetY = sectorCoords.y * mapGenOpts_SO.mapGen_SectorTotalSize;

        //Return The locally Positioned Cell In the Array of the Sector Then Return The Full Cell
        return hexSectors_Dict[sectorCoords].HexCellsData_Dict[cellCoords]; // [cellCoords.x - sectorOffsetX, cellCoords.y - sectorOffsetY];
    }

    private HexCell_Data[,] GetHexCellDataList_ByChunk(HexChunkCoords chunkCoords)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByCellChunk(chunkCoords);

        //Generate A New Array Of Values To Be Returned
        HexCell_Data[,] returningHexCellDatas_Arr = new HexCell_Data[mapGenOpts_SO.mapGen_ChunkSize, mapGenOpts_SO.mapGen_ChunkSize];

        //Get Sector Size Offsets To Access Them In The Array
        //int sectorOffsetX = sectorCoords.x * mapGenOpts_SO.mapGen_ChunkSize;
        //int sectorOffsetY = sectorCoords.y * mapGenOpts_SO.mapGen_ChunkSize;

        //Get Sector Size Offsets To Access Them In The Array
        //int chunkOffsetX = chunkCoords.x * mapGenOpts_SO.mapGen_ChunkSize;
        //int chunkOffsetY = chunkCoords.y * mapGenOpts_SO.mapGen_ChunkSize;

        //Loop all Hexs In Chunk
        for (int y = 0; y < mapGenOpts_SO.mapGen_ChunkSize; y++)
        {
            for (int x = 0; x < mapGenOpts_SO.mapGen_ChunkSize; x++)
            {
                int cellX = x + (chunkCoords.x * mapGenOpts_SO.mapGen_ChunkSize);
                int cellY = y + (chunkCoords.y * mapGenOpts_SO.mapGen_ChunkSize);

                //Debug.Log("Test Code: Cell X " + cellX);
                //Debug.Log("Test Code: Cell Y " + cellY);
      

                //Add Returning Data Cell From Sector Data Cell Array
                returningHexCellDatas_Arr[x, y] = hexSectors_Dict[sectorCoords].HexCellsData_Dict[new HexCellCoords(cellX, cellY, 0)];
            }
        }

        //Return The local Cells
        return returningHexCellDatas_Arr;
    }

    /////////////////////////////////////////////////////////////////

    private void SetHexChunk(HexChunk incomingChunk)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByCellChunk(incomingChunk.chunkCoords);

        //Get Sector Size Offsets To Access Them In The Array
        //int sectorOffsetX = sectorCoords.x * mapGenOpts_SO.mapGen_ChunkSize;
        //int sectorOffsetY = sectorCoords.y * mapGenOpts_SO.mapGen_ChunkSize;

        //Get Sector Size Offsets To Access Them In The Array
        //int chunkOffsetX = chunkCoords.x * mapGenOpts_SO.mapGen_ChunkSize;
        //int chunkOffsetY = chunkCoords.y * mapGenOpts_SO.mapGen_ChunkSize;

        //Debug.Log("Test Code: " + hexSectors_Dict[sectorCoords].hexChunks_Arr.GetLength(0));
        //Debug.Log("Test Code: Sect X " + sectorOffsetX);
        //Debug.Log("Test Code: Sect Y " + sectorOffsetY);

        if (hexSectors_Dict[sectorCoords].hexChunks_Dict.ContainsKey(incomingChunk.chunkCoords))
        {
            //Set The Chunk As A Replacement Chunk
            hexSectors_Dict[sectorCoords].hexChunks_Dict[incomingChunk.chunkCoords] = incomingChunk;
        }
        else
        {

            //Set The Chunk As A New Chunk
            hexSectors_Dict[sectorCoords].hexChunks_Dict.Add(incomingChunk.chunkCoords, incomingChunk);
        }


        // [incomingChunk.chunkCoords.x, incomingChunk.chunkCoords.y - sectorOffsetY] = incomingChunk;
    }

    private HexChunk GetHexChunk_ByChunk(HexChunkCoords chunkCoords)
    {
        //Check For New Sector Generatation
        HexSectorCoords sectorCoords = GetCheckNewSectors_ByCellChunk(chunkCoords);

        //Get Sector Size Offsets To Access Them In The Array
        //int sectorOffsetX = sectorCoords.x * mapGenOpts_SO.mapGen_ChunkSize;
        //int sectorOffsetY = sectorCoords.y * mapGenOpts_SO.mapGen_ChunkSize;

        //Return The local Cell
        return hexSectors_Dict[sectorCoords].hexChunks_Dict[chunkCoords]; // [chunkCoords.x - sectorOffsetX, chunkCoords.y - sectorOffsetY];
    }

    private List<HexChunkCoords> GetHexChunks_AroundCamera(int X, int Y, int rangeAddition)
    {
        //Create a Returnable List of Values of corect Hexs
        List<HexChunkCoords> chunksCoordsAroundCamera_List = new List<HexChunkCoords>();

        //Get Corners
        int rightCorner = X + rangeAddition + 1;
        int leftCorner = X - rangeAddition;
        int topCorner = Y + rangeAddition + 1;
        int bottomCorner = Y - rangeAddition;

        //Loop The Square From All 4 Corners
        for (int y = bottomCorner; y < topCorner; y++)
        {
            for (int x = leftCorner; x < rightCorner; x++)
            {
                //Add New Possible Chunk
                chunksCoordsAroundCamera_List.Add(new HexChunkCoords(x, y));
            }
        }

        //Return The New List
        return chunksCoordsAroundCamera_List;
    }

    /////////////////////////////////////////////////////////////////

    private HexCellCoords GetHexCellCoords_UnderCamera()
    {
        //Get Camera Position
        Vector3 cameraPos = cameraGenerated.transform.position;

        //Get Sector Coords Using Hex Cell Scale
        int cellCoordX = (int)Math.Round(cameraPos.x / spacing_J, 0); 
        int cellCoordY = (int)Math.Round(cameraPos.z / spacing_I, 0);
        HexCellCoords cellCoords = new HexCellCoords(cellCoordX, cellCoordY, 0);

        return cellCoords;
    }

    /////////////////////////////////////////////////////////////////

    public Material GetBiomeMaterial(int biomeID, int matID)
    {
        return mergedBiomeMats_Arr[biomeID, matID];
    }

    /////////////////////////////////////////////////////////////////

    private HexSectorCoords GetCheckNewSectors_ByCellCoords(HexCellCoords cellCoords)
    {
        //Get Sector Coords Using Hex Cell Scale
        int sectorCoordX = (int)Mathf.Floor((float)cellCoords.x / mapGenOpts_SO.mapGen_SectorTotalSize);
        int sectorCoordY = (int)Mathf.Floor((float)cellCoords.y / mapGenOpts_SO.mapGen_SectorTotalSize);
        HexSectorCoords sectorCoords = new HexSectorCoords(sectorCoordX, sectorCoordY);

        //Check If Sector Has Been Generated Before
        if (!hexSectors_Dict.ContainsKey(sectorCoords))
        {
            //Generate The New Sector If Needed
            HexMap_CreateNewSector(sectorCoords);
        }

        //Return The coords For Other Methods To Use
        return sectorCoords;
    }

    private HexSectorCoords GetCheckNewSectors_ByCellChunk(HexChunkCoords chunkCoords)
    {
        //Get Sector Coords Using Hex Chunk Scale
        int sectorCoordX = (int)Mathf.Floor((float)chunkCoords.x / mapGenOpts_SO.mapGen_ChunkSize);
        int sectorCoordY = (int)Mathf.Floor((float)chunkCoords.y / mapGenOpts_SO.mapGen_ChunkSize);
        HexSectorCoords sectorCoords = new HexSectorCoords(sectorCoordX, sectorCoordY);

        //Check If Sector Has Been Generated Before
        if (!hexSectors_Dict.ContainsKey(sectorCoords))
        {
            //Generate The New Sector If Needed
            HexMap_CreateNewSector(sectorCoords);
        }

        //Return The coords For Other Methods To Use
        return sectorCoords;
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

    private void OLDCODE_HexMap_HideAllHexes()
    {
        /*
        //Spawn Cells By X (Left and Right)
        for (int x = 0; x < mapGenOpts_SO.mapGen_SideLength; x++)
        {
            //Spawn Cells By Y (Up and Down)
            for (int y = 0; y < mapGenOpts_SO.mapGen_SideLength; y++)
            {
                allHexsCells_Arr[x, y].gameObject.SetActive(false);
            }
        }
        */
    }

    private void OLDCODE_HexSaving_LoadSave()
    {
        //Load Save File

        //Recreate All Tiles From Save

    }

    private void OLDCODE_HexSaving_CreateSave()
    {
        //Save the Data
        //Serializer.Save("HexMap.gobo", mySaveFile);

        //Delete All Tiles

    }

    private bool OLDCODE_CheckChunkRangeBounds(int X, int Y)
    {
        //Default False Bools
        bool validRange = false;
        bool validRangeX = false;
        bool validRangeY = false;

        //Generate a Max Chunk Value Allowed
        int chunksAllowed = mapGenOpts_SO.mapGen_SectorTotalSize / mapGenOpts_SO.mapGen_ChunkSize;

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

    private List<HexChunkCoords> OLDCODE_GetValidChunks_AroundCamera(int X, int Y, int rangeAddition)
    {

        return null;
        /*
        //Create a Returnable List of Values of corect Hexs
        List<HexChunkCoords> acceptedRangeValueSets_List = new List<HexChunkCoords>();



        int rightCorner = Mathf.Clamp(X + rangeAddition + 1, 0, mapGenOpts_SO.mapGen_SectorTotalSize - 0);
        int leftCorner = Mathf.Clamp(X - rangeAddition, 0, mapGenOpts_SO.mapGen_SectorTotalSize - 1);

        int topCorner = Mathf.Clamp(Y + rangeAddition + 1, 0, mapGenOpts_SO.mapGen_SectorTotalSize - 0);
        int bottomCorner = Mathf.Clamp(Y - rangeAddition, 0, mapGenOpts_SO.mapGen_SectorTotalSize - 1);




        for (int x = leftCorner; x < rightCorner; x++)
        {
            for (int y = bottomCorner; y < topCorner; y++)
            {

                //Check Bounds
                if (CheckChunkRangeBounds(x, y))
                {
                    //Add New Possible Chunk
                    acceptedRangeValueSets_List.Add(new HexChunkCoords(x, y));
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
        acceptedRangeValueSets_List.Add(new HexChunkCoords(X, Y));

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
                acceptedRangeValueSets_List.Add(new HexChunkCoords(New_X, New_Y));
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
                acceptedRangeValueSets_List.Add(new HexChunkCoords(New_X, New_Y));
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
                acceptedRangeValueSets_List.Add(new HexChunkCoords(New_X, New_Y));
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
                acceptedRangeValueSets_List.Add(new HexChunkCoords(New_X, New_Y));
            }
        }

        //Return The New List
        return acceptedRangeValueSets_List;
        */
    }

    /////////////////////////////////////////////////////////////////









   




    /////////////////////////////////////////////////////////////////
}