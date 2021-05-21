using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexSpawnController : MonoBehaviour
{
    ////////////////////////////////

    [SerializeField]
    HexCell[,] allHexsCells_Arr;
    HexChunk[,] allHexChunks_Arr;

    [Header("Hex Map Options")]
    public bool isChunking;

    [Header("Containers")]
    public GameObject ground_GO;
    public GameObject hexMapContainer_GO;

    [Header("Prefabs")]
    public GameObject hexMesh_Prefab;
    public GameObject hexChunk_Prefab;
    public GameObject hexChunkModel_Prefab;

    [Header("Hex Sizes")]
    public const float outerRadius = 0.05f;
    public const float innerRadius = outerRadius * 0.866025404f;
    private const float spacing_I = innerRadius * 2f;
    private const float spacing_J = outerRadius * 1.5f;
    private const float offcenter_I = spacing_I / 2;

    [Header("Map Settings")]
    public float mapHex_Height = 0.05f;
    public int mapHex_RowCount = 10;
    public int mapHex_ColumnCount = 10;
    public int mapHex_ChunkSize = 10;
    public int mapHex_Seed = 135135;

    [Header("Mesh Colors")]
    public Gradient hexCellColorGradient_PlainsColored;
    public Gradient hexCellColorGradient_PlainsTextured1;
    public Gradient hexCellColorGradient_PlainsTextured2;
    public Gradient hexCellColorGradient_PlainsTextured3;

    [Header("Mesh Materials")]
    public Material hexCellColorMaterial_PlainsColored;
    public Material hexCellColorMaterial_PlainsTextured1;
    public Material hexCellColorMaterial_PlainsTextured2;
    public Material hexCellColorMaterial_PlainsTextured3;

    [Header("Randomization States To Be Used")]
    private Random.State mapGeneration_SeededStated; //= Random.state;

    /////////////////////////////////////////////////////////////////

    private void Start()
    {
        //Spawn All Of the Hex Map
        HexMap_Spawn();
    }

    private void Update()
    {
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

        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_Spawn()
    {
        //Start Counting Timer
        long startingTimeTicks = DateTime.UtcNow.Ticks;

        //Create New Arrays
        allHexsCells_Arr = new HexCell[mapHex_RowCount, mapHex_ColumnCount];
        allHexChunks_Arr = new HexChunk[(int)Mathf.Ceil(mapHex_RowCount / mapHex_ChunkSize), (int)Mathf.Ceil(mapHex_ColumnCount / mapHex_ChunkSize)];

        //Create Either a Chunked or Non-Chunked Version
        if (isChunking)
        {
            HexMap_SetMapSeed();
            HexMap_SpawnGround();
            HexMap_RemoveOldMap();
            HexMap_SpawnAllHexChunks();
            HexMap_SpawnAllHexCells();
            HexMap_RandomizeHeight();
            HexMap_Chunk();
            HexMap_StoreAllHexCells();
            HexMap_CenterCamera();
        }
        else
        {
            HexMap_SetMapSeed();
            HexMap_SpawnGround();
            HexMap_RemoveOldMap();
            HexMap_SpawnAllHexCells();
            HexMap_RandomizeHeight();
            HexMap_StoreAllHexCells();
            HexMap_CenterCamera();
        }

        //Finish Counting Timer
        long endingTimeTicks = DateTime.UtcNow.Ticks;
        float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
        Debug.Log("Test Code: Map Generation Completed in: " + finishTime + "s");
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
        ground_GO.GetComponent<MeshFilter>().mesh = newMesh;
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
                //Create Gameobjects
                GameObject newHex;
                GameObject cellChunk;

                //Check For Chunking
                if (isChunking)
                {
                    //Use a Chunk Container
                    cellChunk = GetCellChunkContainer(x, y);
                }
                else
                {
                    //Use The Top Level Container
                    cellChunk = hexMapContainer_GO;
                }

                //Regular Spawn Position VS Offset Spacing
                if (y % 2 == 0)
                {
                    newHex = Instantiate(hexMesh_Prefab, new Vector3(y * spacing_J, mapHex_Height, x * spacing_I), Quaternion.identity, cellChunk.transform);
                }
                else
                {
                    newHex = Instantiate(hexMesh_Prefab, new Vector3(y * spacing_J, mapHex_Height, x * spacing_I + offcenter_I), Quaternion.identity, cellChunk.transform);
                }

                //Setup Cell
                HexCell newHexCell = newHex.GetComponent<HexCell>();
                newHexCell.GenerateHexMesh_Hard();
                newHexCell.SetLabel(x, y);


                int randomColorType = Random.Range(0, 4);


                switch (randomColorType)
                {
                    case 0:
                        newHexCell.UpdateMaterial(1, hexCellColorMaterial_PlainsColored);
                        newHexCell.GenerateCellColor(hexCellColorGradient_PlainsColored);
                        break;

                    case 1:
                        newHexCell.UpdateMaterial(2, hexCellColorMaterial_PlainsTextured1);
                        newHexCell.GenerateCellColor(hexCellColorGradient_PlainsTextured1);
                        break;

                    case 2:
                        newHexCell.UpdateMaterial(3, hexCellColorMaterial_PlainsTextured2);
                        newHexCell.GenerateCellColor(hexCellColorGradient_PlainsTextured2);
                        break;

                    case 3:
                        newHexCell.UpdateMaterial(4, hexCellColorMaterial_PlainsTextured3);
                        newHexCell.GenerateCellColor(hexCellColorGradient_PlainsTextured3);
                        break;
                }

     


                newHexCell.UpdateCellColor(newHexCell.colorActive);


                //Store it
                allHexsCells_Arr[x, y] = newHexCell;
            }
        }
    }

    private void HexMap_RandomizeHeight()
    {
        foreach (HexCell hexCell in allHexsCells_Arr)
        {
            //Generate Random Heights
            hexCell.GenerateHeight_Random();
        }
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

        Camera.main.transform.position = new Vector3(xPos, 0.7f, -0.55f);
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_Chunk()
    {
        foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
            hexChunk.Chunk(hexChunkModel_Prefab);
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

    private GameObject GetCellChunkContainer(int x, int y)
    {
        return allHexChunks_Arr[(int)Mathf.Floor(x / mapHex_ChunkSize), (int)Mathf.Floor(y / mapHex_ChunkSize)].gameObject;
    }

    private void HandleInput()
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

    /////////////////////////////////////////////////////////////////
}