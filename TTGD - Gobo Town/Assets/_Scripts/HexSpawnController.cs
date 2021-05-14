using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexSpawnController : MonoBehaviour
{
    ////////////////////////////////

    [SerializeField]
    HexCell[,] allHexs_Arr;
    GameObject[,] allChunks_Arr;


    [Header("Prefabs")]
    public GameObject hexChunk_Prefab;
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





    private void Start()
    {
        offcenter_I = spacing_I / 2;

        SpawnHexMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnHexMap();
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }


    private void SpawnHexMap()
    {
        
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }



        allHexs_Arr = new HexCell[mapHex_RowCount, mapHex_ColumnCount];
        allChunks_Arr = new GameObject[(int)Mathf.Ceil(mapHex_RowCount / mapHex_ChunkSize), (int)Mathf.Ceil(mapHex_ColumnCount / mapHex_ChunkSize)];

        //Spawn Chunks
        for (int i = 0; i < allChunks_Arr.GetLength(0); i++)
        {
            for (int j = 0; j < allChunks_Arr.GetLength(1); j++)
            {
                allChunks_Arr[i, j] = Instantiate(hexChunk_Prefab, new Vector3(0, 0, 0), Quaternion.identity, container.transform);

                allChunks_Arr[i, j].name = "Chunk: " + i + "/" + j;
            }
        }


        // X == Left and Right
        for (int x = 0; x < mapHex_RowCount; x++)
        {
            // Y == Up and Down
            for (int y = 0; y < mapHex_ColumnCount; y++)
            {
                //Randomzie Height
                float height = Random.Range(0, 0.02f);
                height = (x * 0.01f) + (y * 0.01f);

                GameObject newHex;

                GameObject chunkContainer = GetChunkContainer(x,y);

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
                newHexCell.GenerateCellColor();
                newHexCell.UpdateCellColor(newHexCell.colorActive);

                //Store it
                allHexs_Arr[x, y] = newHexCell;
            }
        }




        foreach (HexCell hexCell in allHexs_Arr)
        {



          


        }



        StartCoroutine(ChunkMeDaddy());
     
    }


    private IEnumerator ChunkMeDaddy()
    {
        yield return new WaitForSeconds(1f);


        foreach (GameObject chunk in allChunks_Arr)
        {
            yield return new WaitForSeconds(1f);

            //Chunk Meshes
            MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);

                i++;
            }
            chunk.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            chunk.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            chunk.transform.gameObject.SetActive(true);




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

    */


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

        }

        yield break;
    }


    private GameObject GetChunkContainer(int x, int y)
    {
        return allChunks_Arr[(int)Mathf.Floor(x / mapHex_ChunkSize), (int)Mathf.Floor(y / mapHex_ChunkSize)];
    }

    void HandleInput()
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

}
