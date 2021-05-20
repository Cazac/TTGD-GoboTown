using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexSpawnController : MonoBehaviour
{
    ////////////////////////////////

    [SerializeField]
    HexCell[,] allHexsCells_Arr;
    HexChunk[,] allHexChunks_Arr;


    [Header("Prefabs")]
    public GameObject hexChunk_Prefab;
    public GameObject hexChunkModel_Prefab;
    public GameObject hexBlank_Prefab;

    [Header("Hex Counts")]
    public int mapHex_RowCount = 10;
    public int mapHex_ColumnCount = 10;
    public int mapHex_ChunkSize = 10;

    public float spacing_I = 0.15f; 
    public float spacing_J = 0.1732f;
    private float offcenter_I;


    public GameObject container;




    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;




    public Gradient gradientColors;





    /////////////////////////////////////////////////////////////////

    private void Start()
    {
        offcenter_I = spacing_I / 2;

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
        //Create New Arrays
        allHexsCells_Arr = new HexCell[mapHex_RowCount, mapHex_ColumnCount];
        allHexChunks_Arr = new HexChunk[(int)Mathf.Ceil(mapHex_RowCount / mapHex_ChunkSize), (int)Mathf.Ceil(mapHex_ColumnCount / mapHex_ChunkSize)];

        HexMap_RemoveOldMap();
        HexMap_SpawnAllHexChunks();
        HexMap_SpawnAllHexCells();
        HexMap_Chuck();
        HexMap_StoreAllHexCells();

    





    }

 



    /////////////////////////////////////////////////////////////////

    private void HexMap_RemoveOldMap()
    {
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void HexMap_SpawnAllHexChunks()
    {
        //Spawn Chunks
        for (int i = 0; i < allHexChunks_Arr.GetLength(0); i++)
        {
            for (int j = 0; j < allHexChunks_Arr.GetLength(1); j++)
            {
                GameObject newChunk = Instantiate(hexChunk_Prefab, new Vector3(0, 0, 0), Quaternion.identity, container.transform);
                GameObject newChunkModel = Instantiate(hexChunkModel_Prefab, new Vector3(0, 0, 0), Quaternion.identity, newChunk.transform);

                newChunk.GetComponent<HexChunk>().chunkedHexModel1_GO = newChunkModel;
                allHexChunks_Arr[i, j] = newChunk.GetComponent<HexChunk>();

                allHexChunks_Arr[i, j].gameObject.name = "Chunk: " + i + "/" + j;
            }
        }
    }

    private void HexMap_SpawnAllHexCells()
    {
       //Spawn Cells
        for (int x = 0; x < mapHex_RowCount; x++)
        {
            // X == Left and Right / Y == Up and Down
            for (int y = 0; y < mapHex_ColumnCount; y++)
            {
                //Randomzie Height
                float height = Random.Range(0, 0.02f);
                height = (x * 0.01f) + (y * 0.01f);

                GameObject newHex;

                GameObject chunkContainer = GetChunkContainer(x, y);

                //Regular Spawn Position VS Offset
                if (y % 2 == 0)
                {
                    newHex = Instantiate(hexBlank_Prefab, new Vector3(y * spacing_J, height, x * spacing_I), Quaternion.identity, chunkContainer.transform);
                }
                else
                {
                    newHex = Instantiate(hexBlank_Prefab, new Vector3(y * spacing_J, height, x * spacing_I + offcenter_I), Quaternion.identity, chunkContainer.transform);
                }


                HexCell newHexCell = newHex.GetComponent<HexCell>();
                newHexCell.SetLabel(x, y);
                newHexCell.GenerateCellColor(gradientColors);
                newHexCell.UpdateCellColor(newHexCell.colorActive);

                //Store it
                allHexsCells_Arr[x, y] = newHexCell;
            }
        }
    }

    private void HexMap_StoreAllHexCells()
    {
        foreach (HexCell hexCell in allHexsCells_Arr)
        {

        }
    }

    /////////////////////////////////////////////////////////////////

    private void HexMap_Chuck()
    {
        foreach (HexChunk hexChunk in allHexChunks_Arr)
        {
            hexChunk.Chunk(false);
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

    private GameObject GetChunkContainer(int x, int y)
    {
        return allHexChunks_Arr[(int)Mathf.Floor(x / mapHex_ChunkSize), (int)Mathf.Floor(y / mapHex_ChunkSize)].gameObject;
    }

    private void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;


        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell hitCell = hit.collider.gameObject.GetComponentInParent<HexCell>();

            if (hitCell != null)
            {
                hitCell.ClickCell();
            }
        }
    }

    /////////////////////////////////////////////////////////////////
}




/*
MaterialPropertyBlock matPropBlock = new MaterialPropertyBlock();
Renderer hexRenderer = chunk.GetComponent<Renderer>();

Color colorRangeMin = Color.green;
Color colorRangeMax = Color.blue;

float value = Random.Range(0, 1f);



// Get the current value of the material properties in the renderer.
hexRenderer.GetPropertyBlock(matPropBlock);
// Assign our new value.
matPropBlock.SetColor("_Color", Color.Lerp(colorRangeMin, colorRangeMax, value));
// Apply the edited values to the renderer.
hexRenderer.SetPropertyBlock(matPropBlock);




gradient = new Gradient();

// Populate the color keys at the relative time 0 and 1 (0 and 100%)
colorKey = new GradientColorKey[2];
colorKey[0].color = Color.red;
colorKey[0].time = 0.0f;
colorKey[1].color = Color.blue;
colorKey[1].time = 1.0f;

// Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
alphaKey = new GradientAlphaKey[2];
alphaKey[0].alpha = 1.0f;
alphaKey[0].time = 0.0f;
alphaKey[1].alpha = 0.0f;
alphaKey[1].time = 1.0f;

gradient.SetKeys(colorKey, alphaKey);

// What's the color at the relative time 0.25 (25 %) ?
Debug.Log(gradient.Evaluate(0.25f));
    */
