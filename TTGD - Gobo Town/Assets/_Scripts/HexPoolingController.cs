using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexPoolingController : MonoBehaviour
{
    ////////////////////////////////

    [Header("Singleton Refference")]
    public static HexPoolingController Instance;

    ////////////////////////////////

    [Header("Chunk Prefab")]
    public GameObject hexChunk_Prefab;
    public GameObject hexCell_Prefab;

    [Header("Containers")]
    public GameObject hexChunk_ActiveContainer;
    public GameObject hexChunk_InactiveContainer;

    [Header("Queues")]
    public Queue<HexChunk> hexChunk_ActivePoolQueue = new Queue<HexChunk>();
    public Queue<HexChunk> hexChunk_InactivePoolQueue = new Queue<HexChunk>();

    /////////////////////////////////////////////////////////////////

    private void Awake()
    {
        //Setup Singleton Refference
        Instance = this;
    }

    /////////////////////////////////////////////////////////////////

    private HexChunk CreateObject()
    {
        //Create The New Chunk
        HexChunk newChunk = Instantiate(hexChunk_Prefab, hexChunk_InactiveContainer.transform).GetComponent<HexChunk>();

        //Spawn The Hexs
        List<HexCell> hexsSpawned = new List<HexCell>();
        for (int y = 0; y < MapSpawnController.Instance.mapGenOpts_SO.mapGen_ChunkSize; y++)
        {
            for (int x = 0; x < MapSpawnController.Instance.mapGenOpts_SO.mapGen_ChunkSize; x++)
            {
                //Get The Hex Cell
                HexCell hexCell = Instantiate(hexCell_Prefab, newChunk.transform).GetComponent<HexCell>();

                //Add to the list Then Spawn The Mesh
                hexsSpawned.Add(hexCell);
                hexCell.SpawnCell();
            }
        }

        //Attach Hexs to the Chunk
        newChunk.hexCellsInChunk_Arr = hexsSpawned.ToArray();

        //Return the Generated Chunk
        return newChunk;
    }

    public HexChunk ActivateObject()
    {
        //Spawn A New Gameobject if The Inactive Queue is Empty
        if (hexChunk_InactivePoolQueue.Count == 0)
        {
            hexChunk_InactivePoolQueue.Enqueue(CreateObject());
        }

        //Get / Dequeue Next Script From Inactive Queue
        HexChunk poolableObject = hexChunk_InactivePoolQueue.Dequeue();

        //Set / Enqueue Next Script To Active Queue
        hexChunk_ActivePoolQueue.Enqueue(poolableObject);

        //Setup On Spawn Method
        poolableObject.Poolable_OnSpawn();

        //Return The New Chunk
        return poolableObject;
    }

    public void DeactivateObject(HexChunk poolableObject)
    {
        //Deactivate The Chunk State
        poolableObject.currentState = HexChunk.ChunkState.Inactive;

        //Clear Out The Old Combine Model Meshes
        for (int i = 0; i < poolableObject.hexCellMergedMeshes_List.Count; i++)
        {
            //Destory The Merged Mesh
            Destroy(poolableObject.hexCellMergedMeshes_List[i].gameObject);
        }

        //Build a new queue without the specified elements
        hexChunk_ActivePoolQueue = new Queue<HexChunk>(hexChunk_ActivePoolQueue.Where(x => !((x.chunkCoords.x == poolableObject.chunkCoords.x) && (x.chunkCoords.y == poolableObject.chunkCoords.y))));

        //Set / Enqueue Next Script To Active Queue
        hexChunk_InactivePoolQueue.Enqueue(poolableObject);

        //Setup On Spawn Method
        poolableObject.Poolable_OnDespawn();
    }

    /////////////////////////////////////////////////////////////////
}
