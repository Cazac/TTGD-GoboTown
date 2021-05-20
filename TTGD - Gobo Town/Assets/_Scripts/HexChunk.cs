using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexChunk : MonoBehaviour
{
    ////////////////////////////////

    [Header("Hex Chunk Model")]
    public GameObject chunkedHexModel_GO;

    [Header("Hex Chunk Cells")]
    public List<HexCell> hexCellsInChunk_List;

    /////////////////////////////////////////////////////////////////

    public void CollectChunkData()
    {
        hexCellsInChunk_List = new List<HexCell>();

        foreach (Transform hexCell in gameObject.transform)
        {
            hexCellsInChunk_List.Add(hexCell.gameObject.GetComponent<HexCell>());
        }

        //Remove First one as it is the new chunk itself
        hexCellsInChunk_List.RemoveAt(0);
    }

    /////////////////////////////////////////////////////////////////

    public void Chunk()
    {
        //Collect Info on which scripts should be included in the chunk
        CollectChunkData();

        //Chunk Meshes
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


        chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);

        chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh.subMeshCount = 2;

        chunkedHexModel_GO.transform.gameObject.SetActive(true);


        ColorTheChunk();
    }

    public void Unchunk()
    {
        chunkedHexModel_GO.SetActive(false);

        foreach (HexCell hexCell in hexCellsInChunk_List)
        {
            hexCell.hexObject_MeshFilter.gameObject.SetActive(true);
        }
    }

    public void Rechunk()
    {
        chunkedHexModel_GO.SetActive(true);

        foreach (HexCell hexCell in hexCellsInChunk_List)
        {
            hexCell.hexObject_MeshFilter.gameObject.SetActive(false);
        }
    }

    /////////////////////////////////////////////////////////////////

    public void ColorTheChunk()
    {
        //Get Info
        Mesh mesh = chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh;
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

    public void SetupChunk(GameObject HexModel_GO, int i, int j)
    {
        chunkedHexModel_GO = HexModel_GO;
        gameObject.name = "Chunk: " + i + "/" + j;
    }

    /////////////////////////////////////////////////////////////////
}


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
