using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexChunk : MonoBehaviour
{
    ////////////////////////////////

    [Header("BLANKVAR")]
    public GameObject chunkedHexModel_GO;
    public List<HexCell> hexCellsInChunk_List;

    ////////////////////////////////




    [Header("Mesh Colors")]
    Vector3[] vertices_Arr;
    Vector2[] uvs_Arr;
    Color[] colors_Arr;

    public Gradient gradientColors;

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
        CollectChunkData();

        //Chunk Meshes
        MeshFilter[] meshFilter_List = gameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilter_List.Length];


        int i = 0;
        while (i < meshFilter_List.Length)
        {
            combine[i].mesh = meshFilter_List[i].sharedMesh;
            combine[i].transform = meshFilter_List[i].transform.localToWorldMatrix;
            meshFilter_List[i].gameObject.SetActive(false);

            i++;
        }

        chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        chunkedHexModel_GO.transform.gameObject.SetActive(true);


        ColorTheChunk();
    }

    public void Unchunk()
    {
        chunkedHexModel_GO.SetActive(false);

        foreach (HexCell hexCell in hexCellsInChunk_List)
        {
            hexCell.HexObject.SetActive(true);
        }
    }

    public void Rechunk()
    {
        chunkedHexModel_GO.SetActive(true);

        foreach (HexCell hexCell in hexCellsInChunk_List)
        {
            hexCell.HexObject.SetActive(false);
        }
    }

    /////////////////////////////////////////////////////////////////

    public void ColorTheChunk()
    {
        Mesh mesh = chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh;

        vertices_Arr = mesh.vertices;
        uvs_Arr = new Vector2[vertices_Arr.Length];
        colors_Arr = new Color[vertices_Arr.Length];

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



        //float random = 


        for (int i = 0; i < colors_Arr.Length; i++)
        {
            colors_Arr[i] = gradientColors.Evaluate(Random.Range(0, 1f));
        }

        mesh.colors = colors_Arr;


        mesh.RecalculateNormals();
    }










    /*
public MeshRenderer meshRenderer;
public MeshFilter meshFilter;

int vertexIndex = 0;
List<Vector3> vertices = new List<Vector3>();
List<int> triangles = new List<int>();
List<Vector2> uvs = new List<Vector2>();

bool[,,] voxelMap = new bool[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

/////////////////////////////////////////////////////////////////


void Start()
{

    //PopulateVoxelMap();
    //CreateMeshData();
    //CreateMesh();

}


    void PopulateVoxelMap()
    {

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    voxelMap[x, y, z] = true;

                }
            }
        }

    }

    void CreateMeshData()
    {

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {

                    AddVoxelDataToChunk(new Vector3(x, y, z));

                }
            }
        }

    }

    bool CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;

        return voxelMap[x, y, z];

    }

    void AddVoxelDataToChunk(Vector3 pos)
    {

        for (int p = 0; p < 6; p++)
        {

            if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
            {

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);
                uvs.Add(VoxelData.voxelUvs[0]);
                uvs.Add(VoxelData.voxelUvs[1]);
                uvs.Add(VoxelData.voxelUvs[2]);
                uvs.Add(VoxelData.voxelUvs[3]);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;

            }
        }

    }

    void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

    }
    */

    /////////////////////////////////////////////////////////////////
}