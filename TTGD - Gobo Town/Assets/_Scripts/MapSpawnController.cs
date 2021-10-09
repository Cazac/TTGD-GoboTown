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

    [Header("Currently Active Chunks")]
    private List<HexChunkCoords> currentlyLoaded_Chunks_List = new List<HexChunkCoords>();

    ////////////////////////////////

    [Header("Hex Map Generation Options")]
    public MapGenerationOptions_SO mapGenOpts_SO;

    ////////////////////////////////

    [Header("Dynamic Loading Hex Settings")]
    public int chunkRenderDistance;

    [Header("Camera Options")]
    private Camera currentCamera;
    public Vector2 cameraRelativePosition;

    ////////////////////////////////
    
    [Header("Hex Map Containers")]
    public GameObject setupGround_GO;
    public GameObject sectorGroundContainer_GO;
    public GameObject hexMapContainer_GO;

    [Header("Hex Map Prefabs")]
    public GameObject hexChunkModel_Prefab;

    [Header("Decoration Prefabs")]
    //public GameObject hexGrass_Prefab;

    ////////////////////////////////

    [Header("Biome Map Visuals")]
    public GameObject biomeVisualQuad_Prefab;
    public GameObject biomeVisualContainer_GO;

    ////////////////////////////////

    [Header("Current Camera Position")]
    public HexCellCoords coordsUnderCamera_Cell = new HexCellCoords(0, 0);
    public HexChunkCoords coordsUnderCamera_Chunk = new HexChunkCoords(0, 0);
    public HexSectorCoords coordsUnderCamera_Sector = new HexSectorCoords(0, 0);

    ////////////////////////////////

    private Material[,] mergedBiomeMats_Arr;

    ////////////////////////////////

    [HideInInspector]
    public SaveFile mySaveFile;

    /////////////////////////////////////////////////////////////////

    private void Awake()
    {
        //Setup Singleton Refference
        Instance = this;
    }

    private void Start()
    {
        //Check Camera Type
        if (mapGenOpts_SO.isCameraFirstPerson)
        {
            currentCamera = PlayerMovement.Instance.cameraFirstPerson;
        }
        else if (mapGenOpts_SO.isCameraThirdPerson)
        {
            currentCamera = PlayerMovement.Instance.cameraThirdPerson;
        }

        //Initialize The Setup Biomes Mat Values
        HexSetup_BiomeMatsArray();

        //Spawn The Hex Map
        HexMap_SpawnMap();
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
    }

    private void FixedUpdate()
    {
        //Check If Camera is in new chunk location
        UpdateCheck_SpawnCameraChunks();
    }

    /////////////////////////////////////////////////////////////////

    private void UpdateCheck_SpawnCameraChunks()
    {
        ////////////////////////////////

        //Record The Old Chunk
        HexChunkCoords oldHexChunkCoords = coordsUnderCamera_Chunk;

        //Update Camera Coords Under Camera
        bool isCameraInNewChunk = HexMap_UpdateCoordsUnderTheCamera();

        //Check that the current chunk is different then last frame
        if (!isCameraInNewChunk)
        {
            //Chunk is still the same, Leave it be.
            return;
        }

        ////////////////////////////////

        //Collect List Of Possible Chunks Around Camera Chunk Then Refresh Old and New Chunks For Reloading
        List<HexChunkCoords> chunksCoordsToBeLoaded_List = HexFetcherUtility.GetHexChunks_AroundCamera(coordsUnderCamera_Chunk.x, coordsUnderCamera_Chunk.y, chunkRenderDistance);
        chunksCoordsToBeLoaded_List = HexSpawn_UpdateCurrentChunkList(chunksCoordsToBeLoaded_List);

        //Load All Chunks That are listed to be loaded
        foreach (HexChunkCoords currentCoords in chunksCoordsToBeLoaded_List)
        {
            //Activate A New Chunk Then Set The Chunk As Active
            HexChunk newChunk = HexPoolingController.Instance.ActivateObject();
            newChunk.chunkCoords = currentCoords;

            //Setup The Hexs In The Array That Were Spawned By The Pooler
            HexFetcherUtility.SetHexChunk(newChunk, mapGenOpts_SO, hexSectors_Dict);
            newChunk.SetChunkActive(currentCoords, HexFetcherUtility.GetHexCellDataList_ByChunk(currentCoords, mapGenOpts_SO, hexSectors_Dict));

            //CHUNK THIS CHUNK
            newChunk.Chunk();
        }
    }

    private void UpdateCheck_RegenerateMap()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //Reset The Map
            HexSetup_ResetMap();
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

    private void HexSetup_BiomeMatsArray()
    {
        //Search For the highest Biome Mat ID to create an array size
        int highestBiomeMatID = 0;
        for (int i = 0; i < mapGenOpts_SO.allBiomes_Arr.Length; i++)
        {
            //Loop Each Biome Cell Value
            for (int j = 0; j < mapGenOpts_SO.allBiomes_Arr[i].biomeInfo_SO.biomeCellsInfo_Arr.Length; j++)
            {
                //Check if newest Value is Higher
                if (mapGenOpts_SO.allBiomes_Arr[i].biomeInfo_SO.biomeCellsInfo_Arr[j].matID > highestBiomeMatID)
                {
                    //Replace the value then keep searching
                    highestBiomeMatID = mapGenOpts_SO.allBiomes_Arr[i].biomeInfo_SO.biomeCellsInfo_Arr[j].matID;
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

            for (int j = 0; j < mapGenOpts_SO.allBiomes_Arr[i].biomeInfo_SO.biomeCellsInfo_Arr.Length; j++)
            {
                if (lastFoundID != mapGenOpts_SO.allBiomes_Arr[i].biomeInfo_SO.biomeCellsInfo_Arr[j].matID)
                {
                    lastFoundID = mapGenOpts_SO.allBiomes_Arr[i].biomeInfo_SO.biomeCellsInfo_Arr[j].matID;
                    mergedBiomeMats_Arr[i, lastFoundID] = mapGenOpts_SO.allBiomes_Arr[i].biomeInfo_SO.biomeCellsInfo_Arr[j].material;
                }
            }
        }
    }

    private void HexSetup_ResetMap()
    {
        //Loop all of the loaded chunk till te list is empty
        while (currentlyLoaded_Chunks_List.Count > 0)
        {
            //Get The Chunk Script And Deactivate it
            HexChunk hexChunk = HexFetcherUtility.GetHexChunk_ByChunk(currentlyLoaded_Chunks_List[0], mapGenOpts_SO, hexSectors_Dict);
            HexPoolingController.Instance.DeactivateObject(hexChunk);
            currentlyLoaded_Chunks_List.Remove(currentlyLoaded_Chunks_List[0]);
        }

        //Clean Dictionaries Before Use when reset
        hexSectors_Dict.Clear();
        currentlyLoaded_Chunks_List.Clear();

        //Dupplicated and changed the values so that they do not persiste after changes are made
        MapGenerationOptions_SO clonedMapGenerationOption = Instantiate(mapGenOpts_SO);
        mapGenOpts_SO = clonedMapGenerationOption;
        mapGenOpts_SO.mapGen_Seed = Random.Range(1000, 9999);

        //Change camera so that the camera coords will force update
        coordsUnderCamera_Chunk = new HexChunkCoords(999, 999);
        coordsUnderCamera_Sector = new HexSectorCoords(999, 999);
        coordsUnderCamera_Cell = new HexCellCoords(999, 999);
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_SpawnMap()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Spawn Ground Visuals
        HexSpawn_SpawnSectorGroundFloor();

        //Center Camera on Hex Map
        HexMap_CenterCamera();

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

    private void HexMap_CenterCamera()
    {
        //Figure out the X Position by size of the generated map
        float extraTrim = 0.15f;
        float x = MapGenerationOptions_SO.innerRadius * mapGenOpts_SO.mapGen_SectorTotalSize * 1.75f;
        float y = MapGenerationOptions_SO.outerRadius * mapGenOpts_SO.mapGen_SectorTotalSize * 1.75f;
        float left = -extraTrim;
        float right = x + extraTrim;
        float up = -extraTrim;
        float down = y + extraTrim;
        float xPos = Mathf.Lerp(left, right, 0.5f);
        float yPos = Mathf.Lerp(up, down, 0.5f);

        if (mapGenOpts_SO.isCameraFirstPerson)
        {
            //Set Camera Position To Look at map from a good angle
            PlayerMovement.Instance.playerFirstPerson.transform.position = new Vector3(xPos, 2, yPos);
        }
        else if (mapGenOpts_SO.isCameraThirdPerson)
        {
            //Set Camera Position To Look at map from a good angle
            currentCamera.transform.position = new Vector3(xPos, cameraRelativePosition.x, yPos);
        }
    }

    private bool HexMap_UpdateCoordsUnderTheCamera()
    {
        //Fetch Hex Cell / Chunk / Sector Under Camera 
        HexCellCoords newCellCoords_UnderCamera = HexFetcherUtility.GetHexCellCoords_ByWorldPosition(currentCamera.transform.position);

        ////////////////////////////////

        //Check If Cell Is Changed
        if ((coordsUnderCamera_Cell.x == newCellCoords_UnderCamera.x) && (coordsUnderCamera_Cell.y == newCellCoords_UnderCamera.y))
        {
            //Cell has not changed therefore -> Chunk Has Not Changed
            return false;
        }

        //Get The Old Chunk Coords
        int oldChunkCoordX = (int)Mathf.Floor((float)coordsUnderCamera_Cell.x / mapGenOpts_SO.mapGen_ChunkSize);
        int oldChunkCoordY = (int)Mathf.Floor((float)coordsUnderCamera_Cell.y / mapGenOpts_SO.mapGen_ChunkSize);
        HexChunkCoords oldChunkCoords = new HexChunkCoords(oldChunkCoordX, oldChunkCoordY);

        //Get The Old Chunk Coords
        int newChunkCoordX = (int)Mathf.Floor((float)newCellCoords_UnderCamera.x / mapGenOpts_SO.mapGen_ChunkSize);
        int newChunkCoordY = (int)Mathf.Floor((float)newCellCoords_UnderCamera.y / mapGenOpts_SO.mapGen_ChunkSize);
        HexChunkCoords newChunkCoords = new HexChunkCoords(newChunkCoordX, newChunkCoordY);

        //Update Cell Under Camera
        coordsUnderCamera_Cell = newCellCoords_UnderCamera;

        ////////////////////////////////

        //Check that the current chunk is different then last frame
        if ((oldChunkCoords.x == newChunkCoords.y) && (oldChunkCoords.x == newChunkCoords.y))
        {
            //Chunk has not changed therefore -> Chunk Has Not Changed
            return false;
        }

        //Update Chunk Under Camera and Sector
        coordsUnderCamera_Chunk = newChunkCoords;
        coordsUnderCamera_Sector = HexFetcherUtility.GetCheckNewSectors_ByChunkCoords(newChunkCoords, mapGenOpts_SO, hexSectors_Dict);

        ////////////////////////////////

        //Chunk Has Changed
        return true;
    }

    public void HexMap_CreateBasicSector(HexSectorCoords sectorCoords)
    {
        //Use the Generator to create the Hexs Needed to Spawn
        hexSectors_Dict.Add(sectorCoords, MapGeneration.HexMapGeneration_NewSector(mapGenOpts_SO, sectorCoords));
    }

    /////////////////////////////////////////////////////////////////

    private void HexSpawn_SpawnSectorGroundFloor()
    {
        //Turn Off Old Map From Setup
        setupGround_GO.SetActive(false);

        //Setup New Mesh
        Mesh newMesh = new Mesh();
        List<Vector3> verts_List = new List<Vector3>();
        List<int> tris_List = new List<int>();

        //Setup Sizing
        float extraTrim = 0.15f;
        float x = MapGenerationOptions_SO.innerRadius * mapGenOpts_SO.mapGen_SectorTotalSize * 1.75f;
        float y = MapGenerationOptions_SO.outerRadius * mapGenOpts_SO.mapGen_SectorTotalSize * 1.75f;

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
        sectorGroundContainer_GO.GetComponent<MeshFilter>().mesh = newMesh;
    }

    private List<HexChunkCoords> HexSpawn_UpdateCurrentChunkList(List<HexChunkCoords> possibleChunkCoords_List)
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
            HexChunk hexChunk = HexFetcherUtility.GetHexChunk_ByChunk(chunkCoords, mapGenOpts_SO, hexSectors_Dict);
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





    /////////////////////////////////////////////////////////////////

    public Material HexUtility_GetBiomeMaterial(int biomeID, int matID)
    {
        return mergedBiomeMats_Arr[biomeID, matID];
    }

    /////////////////////////////////////////////////////////////////












    /////////////////////////////////////////////////////////////////

    //Saved for map making later
    private void OLDCODE_HexVisuals_DisplayVisuals(HexSectorCoords sectorCoords)
    {
        /*
        //Create a new texture using the biome ints as colors 
        Texture2D texture = new Texture2D(mapGenOpts_SO.mapGen_SectorTotalSize, mapGenOpts_SO.mapGen_SectorTotalSize);
        GameObject newHex = Instantiate(biomeVisualQuad_Prefab, new Vector3((sectorCoords.x * 5f), 2f + (sectorCoords.y * 5f), -2f), Quaternion.identity, biomeVisualContainer_GO.transform);
        newHex.GetComponent<Renderer>().material.mainTexture = texture;

        int sectorOffsetX = sectorCoords.x * mapGenOpts_SO.mapGen_SectorTotalSize;
        int sectorOffsetY = sectorCoords.y * mapGenOpts_SO.mapGen_SectorTotalSize;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                //Get Sector Data Cell
                //HexCell_Data dataCell = GetHexCellData_ByCell(x, y);
                HexCell_Data dataCell = GetHexCellDataLocal_ByCell(new HexCellCoords(x + sectorOffsetX, y + sectorOffsetY));

                ColorUtility.TryParseHtmlString("#" + dataCell.hexCell_Color, out Color convertedColor);
                texture.SetPixel(x, y, convertedColor);
            }
        }

        //Apply Pixel Colors To Texture
        texture.Apply();

        //Recenter Camera
        cameraGenerated.transform.position = new Vector3(newHex.transform.position.x, newHex.transform.position.y, newHex.transform.position.z - 5);
        cameraGenerated.transform.rotation = new Quaternion(0, 0, 0, 0);
        */
    }

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

    private HexCellCoords[,] GetHexCellCoordsArr_FromXToY(HexCellCoords startingPoint, HexCellCoords endingPoint)
    {
        return null;
        /*
        int xCount = Mathf.Abs(endingPoint.x - startingPoint.x);
        int yCount = Mathf.Abs(endingPoint.y - startingPoint.y);

        //Setup Retunable Array
        HexCellCoords[,] returningHexCoords_Arr = new HexCellCoords[xCount, yCount];

        int offsetX = 0;
        int offsetY = 0;
        


        //Loop All Points Between X / Y
        for (int y = 0; y < yCount; y++)
        {
            for (int x = 0; x < xCount; x++)
            {
                //Debug.Log("Test Code: " + new HexCellCoords(x, y).GetPrintableCoords());

                //int sectorCoordX = (int)Mathf.Floor((float)x / mapGenOpts_SO.mapGen_SectorTotalSize);
                //int sectorCoordY = (int)Mathf.Floor((float)y / mapGenOpts_SO.mapGen_SectorTotalSize);
                //HexSectorCoords sectorCoords = new HexSectorCoords(sectorCoordX, sectorCoordY);

                returningHexCoords_Arr[x, y] = new HexCellCoords(x + offsetX, y +  offsetY);
            }
        }


        //Loop All Points Between X / Y
        for (int y = startingPoint.y; y < endingPoint.y; y++)
        {
            for (int x = startingPoint.x; x < endingPoint.x; x++)
            {
                Debug.Log("Test Code: " + new HexCellCoords(x, y).GetPrintableCoords());

                //int sectorCoordX = (int)Mathf.Floor((float)x / mapGenOpts_SO.mapGen_SectorTotalSize);
                //int sectorCoordY = (int)Mathf.Floor((float)y / mapGenOpts_SO.mapGen_SectorTotalSize);
                //HexSectorCoords sectorCoords = new HexSectorCoords(sectorCoordX, sectorCoordY);

                returningHexCoords_Arr[x, y] = new HexCellCoords(x, y);
            }
        }

        return returningHexCoords_Arr;
        */
    }

    private void HexGen_FillEmptiedChunks_Top(HexSectorCoords sectorCoords_Center, HexSectorCoords sectorCoords_Top)
    {
        /*
        Debug.Log("Test Code: Generating Chained Sector (Top)");

        //Get Basic Calculated Info
        int chunksPerSector = mapGenOpts_SO.mapGen_SectorTotalSize / mapGenOpts_SO.mapGen_ChunkSize;
        int offset = chunksPerSector - 1;

        //Get Starting Points To Cycle Though
        HexChunkCoords startingChunkCoordCenter_TopLeft = new HexChunkCoords(sectorCoords_Center.x * chunksPerSector, (sectorCoords_Center.y * chunksPerSector) + offset);
        HexChunkCoords startingChunkCoordCenter_BottomLeft = new HexChunkCoords(sectorCoords_Top.x * chunksPerSector, (sectorCoords_Top.y * chunksPerSector));

        //Generated lists Of Chunks Used
        List<HexChunkCoords> chunksCoordsCenter_List = GetHexChunkCoordsList_AcrossSectorHorizontally(startingChunkCoordCenter_TopLeft);
        List<HexChunkCoords> chunksCoordsTop_List = GetHexChunkCoordsList_AcrossSectorHorizontally(startingChunkCoordCenter_TopLeft);

        //Set The Flag That Neighbours have been Chained
        hexSectors_Dict[sectorCoords_Center].hasGeneratedNeighbours = true;





        //Get Hex Cells List Not Chuinks! ^^^^



        HexCellCoords[] influenceRowCenter_Arr = new HexCellCoords[mapGenOpts_SO.mapGen_SectorTotalSize];
        HexCellCoords[] influenceRowTop_Arr = new HexCellCoords[mapGenOpts_SO.mapGen_SectorTotalSize];










        //Center
        foreach (HexChunkCoords chunkCoords in chunksCoordsCenter_List)
        {
            HexCellCoords[,] dataCellsToBeLoaded_Arr = GetHexCellCoordsList_ByChunk(chunkCoords);


            for (int y = 0; y < dataCellsToBeLoaded_Arr.GetLength(0); y++)
            {
                for (int x = 0; x < dataCellsToBeLoaded_Arr.GetLength(1); x++)
                {
                    //Store The Data Collected From Other Methodsit
                    HexCell_Data mergedCell = new HexCell_Data
                    {
                        hexCoords = new HexCellCoords(dataCellsToBeLoaded_Arr[x, y].x, dataCellsToBeLoaded_Arr[x, y].y),
                        hexCell_HeightSteps = 5,
                        hexCell_BiomeID = 0,
                        hexCell_Color = "#ffffff",
                        hexCell_MatID = 0
                    };

                    hexSectors_Dict[sectorCoords_Center].HexCellsData_Dict[mergedCell.hexCoords] = mergedCell;
                }
            }
        }


        //Top
        foreach (HexChunkCoords chunkCoords in chunksCoordsTop_List)
        {
            HexCellCoords[,] dataCellsToBeLoaded_Arr = GetHexCellCoordsList_ByChunk(chunkCoords);


            for (int y = 0; y < dataCellsToBeLoaded_Arr.GetLength(0); y++)
            {
                for (int x = 0; x < dataCellsToBeLoaded_Arr.GetLength(1); x++)
                {
                    //Store The Data Collected From Other Methodsit
                    HexCell_Data mergedCell = new HexCell_Data
                    {
                        hexCoords = new HexCellCoords(dataCellsToBeLoaded_Arr[x, y].x, dataCellsToBeLoaded_Arr[x, y].y),
                        hexCell_HeightSteps = 5,
                        hexCell_BiomeID = 0,
                        hexCell_Color = "#ffffff",
                        hexCell_MatID = 0
                    };

                    hexSectors_Dict[sectorCoords_Top].HexCellsData_Dict[mergedCell.hexCoords] = mergedCell;
                }
            }
        }




    */
    }

    private List<HexChunkCoords> GetHexChunkCoordsList_AcrossSectorHorizontally(HexChunkCoords startingChunkCoords)
    {
        return null;
        /*
        //Setup Returning List
        List<HexChunkCoords> returningChunksCoords_List = new List<HexChunkCoords>();
        int chunksPerSector = mapGenOpts_SO.mapGen_SectorTotalSize / mapGenOpts_SO.mapGen_ChunkSize;

        //Get All The Center Chunk Coords Along The Line
        for (int y = 0; y < mapGenOpts_SO.mapGen_EmptiedChunkBorderSize; y++)
        {
            for (int x = 0; x < chunksPerSector; x++)
            {
                //Add The New Chunk Coord Sequentially
                returningChunksCoords_List.Add(new HexChunkCoords(startingChunkCoords.x + x, startingChunkCoords.y + y));
            }
        }

        //Return The List
        return returningChunksCoords_List;
    }

    private List<HexChunkCoords> GetHexChunkCoordsList_AcrossSectorVertically(HexChunkCoords startingChunkCoords)
    {
        //Setup Returning List
        List<HexChunkCoords> returningChunksCoords_List = new List<HexChunkCoords>();
        int chunksPerSector = mapGenOpts_SO.mapGen_SectorTotalSize / mapGenOpts_SO.mapGen_ChunkSize;

        //Get All The Center Chunk Coords Along The Line
        for (int y = 0; y < chunksPerSector; y++)
        {
            for (int x = 0; x < mapGenOpts_SO.mapGen_EmptiedChunkBorderSize; x++)
            {
                //Add The New Chunk Coord Sequentially
                returningChunksCoords_List.Add(new HexChunkCoords(startingChunkCoords.x + x, startingChunkCoords.y + y));
            }
        }

        //Return The List
        return returningChunksCoords_List;
            */
}

    /////////////////////////////////////////////////////////////////



    /////////////////////////////////////////////////////////////////
}