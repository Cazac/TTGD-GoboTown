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
    public List<HexCell> hexCellsInChunk_List;

    public Material mat1;
    public Material mat2;

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
        hexCellsInChunk_List.RemoveAt(0);

        Debug.Log("Test Code: " + hexCellsInChunk_List.Count);
    }

    /////////////////////////////////////////////////////////////////

    public void Chunk(bool oldOrNew)
    {
        if (oldOrNew)
        {
            Chunk_OldGood();
        }
        else
        {
            Chunk_NewBroken();
        }
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

    /////////////////////////////////////////////////////////////////

    private void Chunk_OldGood()
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
       
        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        chunkedHexModel1_GO.transform.gameObject.SetActive(true);

        ColorTheChunk();
    }

    private void Chunk_NewBroken()
    {
        //Collect Info on which scripts should be included in the chunk
        CollectChunkData();




        MeshFilter[] meshFilter_Arr = gameObject.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> meshFilter_List = new List<MeshFilter>(meshFilter_Arr);
        meshFilter_List.RemoveAt(0);
        meshFilter_List.RemoveAt(1);
        meshFilter_Arr = meshFilter_List.ToArray();



        CombineInstance[] combine = new CombineInstance[meshFilter_Arr.Length];

        List<CombineInstance> combiningList_1 = new List<CombineInstance>();
        List<CombineInstance> combiningList_2 = new List<CombineInstance>();

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


        return;


        /*

        int i = 0;
        while (i < meshFilter_Arr.Length)
        {
            Mesh newestMesh = CombineMeshes(meshFilter_Arr);


            combine[i].mesh = meshFilter_Arr[i].sharedMesh;
            combine[i].transform = meshFilter_Arr[i].transform.localToWorldMatrix;
            meshFilter_Arr[i].gameObject.SetActive(false);

            i++;
        }



        while (meshFilter_Arr.Length > 0)
        {


            meshFilter_Arr.
        }


        for (int i = 0; i < meshFilter_Arr.Length; i++)
        {


        }

        */

       // Mesh newestMesh = CombineMeshes(meshFilter_Arr);
        //chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);



        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh = newestMesh;
        chunkedHexModel1_GO.transform.gameObject.SetActive(true);

        int k = 0;
        while (k < meshFilter_Arr.Length)
        {
            meshFilter_Arr[k].gameObject.SetActive(false);

            k++;
        }


        return;

        /////////////////////////////////////////////////////////////////

        List<int> matID_List = new List<int>();
        List<CombineInstance> subMeshes_1 = new List<CombineInstance>();
        List<CombineInstance> subMeshes_2 = new List<CombineInstance>();



        List<Vector3> submeshVerts_1 = new List<Vector3>();
        List<int> submeshTris_1 = new List<int>();


        Matrix4x4 localToWorld = transform.localToWorldMatrix;


        foreach (HexCell hexCell in hexCellsInChunk_List)
        {
            CombineInstance newInstance = new CombineInstance();

            newInstance.mesh = hexCell.hexObject_MeshFilter.sharedMesh;
            hexCell.hexObject_MeshFilter.gameObject.SetActive(false);

            if (hexCell.hexCellMatID == 1)
            {
                Vector3[] vert_Arr = new Vector3[hexCell.hexObject_MeshFilter.mesh.vertices.Length];
                for (int i = 0; i < hexCell.hexObject_MeshFilter.mesh.vertices.Length; i++)
                {
                    foreach (Vector3 vert in vert_Arr)
                    {
                        //Debug.Log("Test Code: Loop");
                        //submeshVerts_1.Add((vert));
                    }
                }

                subMeshes_1.Add(newInstance);
            }
            else
            {
                //subMeshes_2.Add(newInstance);
            }
        }


     


        /*
        for (int j = 0; j < subMeshes_1.Count; j++)
        {
            submeshTris_1 = submeshTris_1.Concat(new List<int>(subMeshes_1[j].mesh.GetTriangles(0))).ToList();



            //HERE IS THE ISSUES I NEED A WORLD SPACE VERSION TO MERGE THE VERTS TOGETHER
            submeshVerts_1 = submeshVerts_1.Concat(new List<Vector3>(subMeshes_1[j].mesh.vertices)).ToList();
        }

        /*
        int[] submeshTris_2 = new int[0];
        for (int j = 0; j < subMeshes_2.Count; j++)
        {
            submeshTris_2 = subMeshes_2[j].mesh.GetTriangles(0);

        }
        */


        Debug.Log("Test Code: Mesh Length(1) " + subMeshes_1.Count);
        //Debug.Log("Test Code: Mesh Length(2) " + subMeshes_2.Count);
        Debug.Log("Test Code: Verts Length(1) " + submeshVerts_1.Count);
        //Debug.Log("Test Code: Verts Length(2) " + submeshVerts_2.Count);
        Debug.Log("Test Code: Tris Length(1) " + submeshTris_1.Count);
        //Debug.Log("Test Code: Tris Length(2) " + submeshTris_2.Length);



        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh.subMeshCount = 2;
        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh.SetVertices(submeshVerts_1.ToArray());
        chunkedHexModel1_GO.transform.GetComponent<MeshFilter>().mesh.SetTriangles(submeshTris_1.ToArray(), 0);



        //chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh.SetTriangles(submeshTris_2, 0);

      


        /////////////////////////////////////////////////////////////////


        //chunkedHexModel_GO.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        chunkedHexModel1_GO.transform.gameObject.SetActive(true);

        //chunkedHexModel_GO.transform.GetComponent<MeshRenderer>().materials[0] = mat1;
        //chunkedHexModel_GO.transform.GetComponent<MeshRenderer>().materials[1] = mat2;

        //chunkedHexModel_GO.transform.GetComponent<MeshRenderer>().mater = new 


        //ColorTheChunk();
    }
    
    private void Chunk_NewBroken2()
    {
        //Collect Info on which scripts should be included in the chunk
        CollectChunkData();


        MeshFilter[] meshFilter_Arr = gameObject.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> meshFilter_List = new List<MeshFilter>(meshFilter_Arr);
        meshFilter_List.RemoveAt(0);
        meshFilter_List.RemoveAt(1);
        meshFilter_Arr = meshFilter_List.ToArray();



        Mesh mesh;
        Material[] materials;

        MeshTile[] meshTiles = new MeshTile[meshFilter_Arr.Length];
        SmartMeshData[] meshData;

         meshData = new SmartMeshData[meshTiles.Length];
        for (int i = 0; i < meshTiles.Length; i++)
        {
            meshData[i] = new SmartMeshData(meshTiles[i].mesh, meshTiles[i].materials, meshTiles[i].transform.localPosition, meshTiles[i].transform.localRotation);
        }

        Mesh combinedMesh = GetComponent<MeshFilter>().mesh = new Mesh();
        combinedMesh.name = "Combined Mesh";
        Material[] combinedMaterials;

        combinedMesh.CombineMeshesSmart(meshData, out combinedMaterials);

        GetComponent<MeshRenderer>().sharedMaterials = combinedMaterials;
    }



    public class MeshTile : MonoBehaviour
    {
        public Mesh mesh;
        public Material[] materials;
    }

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
