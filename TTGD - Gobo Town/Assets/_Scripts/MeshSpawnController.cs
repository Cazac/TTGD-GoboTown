using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshSpawnController : MonoBehaviour
{
    ////////////////////////////////

    [SerializeField]
    HexCell[,] allHexsCells_Arr;
    HexChunk[,] allHexChunks_Arr;

    [Header("Hex Map Options")]
    public bool isChunking;
    public bool isTexturing;

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

    [Header("Hex Counts")]
    public float mapHex_Height = 0.05f;
    public int mapHex_RowCount = 10;
    public int mapHex_ColumnCount = 10;
    public int mapHex_ChunkSize = 10;

    private const float spacing_I = innerRadius * 2f;
    private const float spacing_J = outerRadius * 1.5f;
    private const float offcenter_I = spacing_I / 2;


    [Header("Mesh Colors")]
    public Gradient hexCellColorGradient_PlainsColored;
    public Gradient hexCellColorGradient_PlainsTextured;

    public Material hexCellColorMaterial_PlainsColored;
    public Material hexCellColorMaterial_PlainsTextured;


    //public Gradient gradientColors;

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
            HexMap_SpawnGround();
            HexMap_RemoveOldMap();
            HexMap_SpawnAllHexChunks();
            HexMap_SpawnAllHexCells();
            HexMap_RandomizeHeight();
            HexMap_Chunk();
            HexMap_StoreAllHexCells();
        }
        else
        {
            HexMap_SpawnGround();
            HexMap_RemoveOldMap();
            HexMap_SpawnAllHexCells();
            HexMap_RandomizeHeight();
            HexMap_StoreAllHexCells();
        }

        //Finish Counting Timer
        long endingTimeTicks = DateTime.UtcNow.Ticks;
        float finishTime = ((endingTimeTicks - startingTimeTicks) / TimeSpan.TicksPerSecond);
        Debug.Log("Test Code: Map Generation Completed in: " + finishTime + "s");

        HexMap_CenterCamera();
    }




    /////////////////////////////////////////////////////////////////

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
        Mesh newMesh = new Mesh();


        List<Vector3> verts_List = new List<Vector3>();
        List<int> tris_List = new List<int>();


        float extraTrim = 0.15f;

        float x = innerRadius * mapHex_RowCount * 1.75f;
        float y = outerRadius * mapHex_ColumnCount * 1.75f;

        verts_List.Add(new Vector3(-extraTrim, 0f, -extraTrim));
        verts_List.Add(new Vector3(x + extraTrim, 0f, -extraTrim));
        verts_List.Add(new Vector3(-extraTrim, 0f, y + extraTrim));
        verts_List.Add(new Vector3(x + extraTrim, 0f, y + extraTrim));

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
                GameObject newChunkModel = Instantiate(hexChunkModel_Prefab, new Vector3(0, 0, 0), Quaternion.identity, newChunk.transform);

                //Setup Chunk Script
                newChunk.GetComponent<HexChunk>().SetupChunk(newChunkModel, i, j);

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

                if (isTexturing)
                {
                    if (0.5f > Random.Range(0f, 1f))
                    {
                        newHexCell.UpdateMaterial(hexCellColorMaterial_PlainsTextured);
                        newHexCell.GenerateCellColor(hexCellColorGradient_PlainsTextured);
                    }
                    else
                    {
                        newHexCell.UpdateMaterial(hexCellColorMaterial_PlainsColored);
                        newHexCell.GenerateCellColor(hexCellColorGradient_PlainsColored);
                    }
                }
                else
                {
                    newHexCell.UpdateMaterial(hexCellColorMaterial_PlainsColored);
                    newHexCell.GenerateCellColor(hexCellColorGradient_PlainsColored);
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

        Camera.main.transform.position = new Vector3(xPos, 0.75f, -0.25f);
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_Chunk()
    {
        foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
            hexChunk.Chunk();
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