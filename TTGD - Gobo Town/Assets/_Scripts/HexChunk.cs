using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class HexChunk : MonoBehaviour
{
    ////////////////////////////////

    [Header("Hex Chunk Model")]
    public GameObject chunkedHexModel1_GO;
    public GameObject chunkedHexModel2_GO;

    [Header("Hex Chunk Cells")]
    public List<HexCell> hexCellsInChunk_List = new List<HexCell>();

    [Header("Hex Materials")]
    public Material mat1;
    public Material mat2;

    /////////////////////////////////////////////////////////////////

    public void CollectChunkData()
    {
        //Collect All The Hexes in the Transform of the object
        foreach (Transform hexCell in gameObject.transform)
        {
            //Add the Hex Cell To The List
            hexCellsInChunk_List.Add(hexCell.gameObject.GetComponent<HexCell>());
        }

        //Remove 2 ones as it is the new chunk mesh render itself not a hex
        hexCellsInChunk_List.RemoveAt(0);
        hexCellsInChunk_List.RemoveAt(0);
    }

    /////////////////////////////////////////////////////////////////

    public void Chunk()
    {
        //Collect Info on which scripts should be included in the chunk
        CollectChunkData();

        //Collect Info To Chunk The Meshes
        MeshFilter[] meshFilter_Arr = gameObject.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> meshFilter_List = new List<MeshFilter>(meshFilter_Arr);

        //Remove The Modeling Chunks
        meshFilter_List.RemoveAt(0);
        meshFilter_List.RemoveAt(1);
        meshFilter_Arr = meshFilter_List.ToArray();



        //CombineInstance[] combine = new CombineInstance[meshFilter_Arr.Length];

        List<CombineInstance> combiningList_1 = new List<CombineInstance>();
        List<CombineInstance> combiningList_2 = new List<CombineInstance>();


        List<List<CombineInstance>> combiningListOfLists_List = new List<List<CombineInstance>>();




        foreach (HexCell hexCell in hexCellsInChunk_List)
        {
            if (hexCell.hexCellMatID == 1)
            {
                CombineInstance currentCombiningInstance = new CombineInstance();

                currentCombiningInstance.mesh = hexCell.hexObject_MeshFilter.sharedMesh;
                currentCombiningInstance.transform = hexCell.hexObject_MeshFilter.transform.localToWorldMatrix;
                hexCell.hexObject_MeshFilter.gameObject.SetActive(false);

                combiningList_1.Add(currentCombiningInstance);
            }
            else
            {
                CombineInstance currentCombiningInstance = new CombineInstance();

                currentCombiningInstance.mesh = hexCell.hexObject_MeshFilter.sharedMesh;
                currentCombiningInstance.transform = hexCell.hexObject_MeshFilter.transform.localToWorldMatrix;
                hexCell.hexObject_MeshFilter.gameObject.SetActive(false);

                combiningList_2.Add(currentCombiningInstance);
            }
        }


        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combiningList_1.ToArray(), true, true);
        Mesh MeshSet_1 = chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh;

        chunkedHexModel2_GO.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        chunkedHexModel2_GO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combiningList_2.ToArray(), true, true);
        Mesh MeshSet_2 = chunkedHexModel2_GO.transform.GetComponent<MeshFilter>().mesh;



        meshFilter_Arr = new MeshFilter[2];
        meshFilter_Arr[0] = chunkedHexModel1_GO.transform.GetComponent<MeshFilter>();
        meshFilter_Arr[1] = chunkedHexModel2_GO.transform.GetComponent<MeshFilter>();

        Mesh newestMesh = CombineMeshes(meshFilter_Arr);
        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh = newestMesh;



        int j = 0;
        while (j < meshFilter_Arr.Length)
        {
            meshFilter_Arr[j].gameObject.SetActive(false);

            j++;
        }

        chunkedHexModel1_GO.transform.gameObject.SetActive(true);


    }

    public void Unchunk()
    {
        chunkedHexModel1_GO.SetActive(false);

        foreach (HexCell hexCell in hexCellsInChunk_List)
        {
            hexCell.hexObject_MeshFilter.gameObject.SetActive(true);
        }
    }

    public void Rechunk()
    {
        chunkedHexModel1_GO.SetActive(true);

        foreach (HexCell hexCell in hexCellsInChunk_List)
        {
            hexCell.hexObject_MeshFilter.gameObject.SetActive(false);
        }
    }

    private void Chunk_OLD()
    {
        //Collect Info on which scripts should be included in the chunk
        CollectChunkData();

        //Collect Info To Chunk The Meshes
        MeshFilter[] meshFilter_Arr = gameObject.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> meshFilter_List = new List<MeshFilter>(meshFilter_Arr);
        meshFilter_List.RemoveAt(0);
        meshFilter_Arr = meshFilter_List.ToArray();
        CombineInstance[] combine = new CombineInstance[meshFilter_Arr.Length];

        int i = 0;
        while (i < meshFilter_Arr.Length)
        {
            combine[i].mesh = meshFilter_Arr[i].sharedMesh;
            combine[i].transform = meshFilter_Arr[i].transform.localToWorldMatrix;
            meshFilter_Arr[i].gameObject.SetActive(false);

            i++;
        }

        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        chunkedHexModel1_GO.transform.gameObject.SetActive(true);

        ColorTheChunk();
    }

    /////////////////////////////////////////////////////////////////

    private Mesh CombineMeshes(MeshFilter[] incomingMeshes)
    {
        // Key: shared mesh instance ID, Value: arguments to combine meshes
        Dictionary<int, List<CombineInstance>> helperDictionary = new Dictionary<int, List<CombineInstance>>();

        // Build combine instances for each type of mesh
        foreach (MeshFilter meshFilter in incomingMeshes)
        {
            List<CombineInstance> combineInstanceLocal_List = new List<CombineInstance>();


            helperDictionary.Add(meshFilter.sharedMesh.GetInstanceID(), combineInstanceLocal_List);


            //Create A New Combine Instance
            CombineInstance meshCombineInstante = new CombineInstance();
            meshCombineInstante.mesh = meshFilter.sharedMesh;
            meshCombineInstante.transform = meshFilter.transform.localToWorldMatrix;

            //Add it to the List
            combineInstanceLocal_List.Add(meshCombineInstante);
        }

        //Debug.Log("Test Code: Count " + helperDictionary.Count);

        //Combine meshes and build combine instance for combined meshes
        List<CombineInstance> combineInstance_List = new List<CombineInstance>();
        foreach (var e in helperDictionary)
        {
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(e.Value.ToArray());
            CombineInstance ci = new CombineInstance();
            ci.mesh = mesh;
            combineInstance_List.Add(ci);
        }

        // And now combine everything
        Mesh result = new Mesh();
        result.CombineMeshes(combineInstance_List.ToArray(), false, false);

        // It is a good idea to clean unused meshes now
        foreach (CombineInstance combineInstance in combineInstance_List)
        {
            Destroy(combineInstance.mesh);
        }

        return result;
    }

    /////////////////////////////////////////////////////////////////

    public void ColorTheChunk()
    {
        //Get Info
        Mesh mesh = chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices_Arr = mesh.vertices;
        Color[] colors_Arr = new Color[vertices_Arr.Length];

        //Loop through all color Verts
        for (int i = 0; i < colors_Arr.Length; i++)
        {
            //54 is the count of verts on a hex
            colors_Arr[i] = hexCellsInChunk_List[(int)Mathf.Floor(i / 54)].colorActive;
        }

        //Set Color Array on Mesh
        mesh.colors = colors_Arr;

        //Not Needed ??
        mesh.RecalculateNormals();
    }

    public void SetupChunk(GameObject HexModel1_GO, GameObject HexModel2_GO, int i, int j)
    {
        chunkedHexModel1_GO = HexModel1_GO;
        chunkedHexModel2_GO = HexModel2_GO;
        gameObject.name = "Chunk: " + i + "/" + j;
    }

    /////////////////////////////////////////////////////////////////
}

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
