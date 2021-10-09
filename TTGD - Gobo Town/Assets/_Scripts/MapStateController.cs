using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapStateController : MonoBehaviour
{
    ////////////////////////////////

    [Header("Singleton Refference")]
    public static MapStateController Instance;

    ////////////////////////////////

    [Header("Sector Storage")]
    private Dictionary<HexSectorCoords, HexSector> hexSectors_Dict = new Dictionary<HexSectorCoords, HexSector>();

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


    ////////////////////////////////

    private Material[,] mergedBiomeMats_Arr;

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
        HexSetup_Initialize();

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

    private void HexSetup_Initialize()
    {
        //Initialize The Setup Values
        HexSetup_BiomeMatsArray();

        //Clean Dictionary Before Use
        hexSectors_Dict.Clear();

        //Use the Generator to create the Hexs Needed to Spawn
        HexMap_CreateBasicSector(new HexSectorCoords(0, 0));

        //Setup Chunks In Pooler
        HexPoolingController.Instance.SetupInitialPool_Chunk();
    }

    /////////////////////////////////////////////////////////////////
}
