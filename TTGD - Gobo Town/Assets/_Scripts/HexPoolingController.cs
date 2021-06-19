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

    [Header("Sets the startup amount of meshes to pre-load")]
    public int intialPoolCount;

    [Header("Chunk Prefab")]
    public GameObject hexChunk_Prefab;
    public GameObject hexCell_Prefab;

    [Header("Containers")]
    public GameObject activeChunks_Container;
    public GameObject inactiveChunks_Container;

    [Header("Queues")]
    public Queue<HexChunk> activepool_Queue = new Queue<HexChunk>();
    public Queue<HexChunk> inactivepool_Queue = new Queue<HexChunk>();

    /////////////////////////////////////////////////////////////////

    private void Awake()
    {
        //Setup Singleton Refference
        Instance = this;
    }

    /////////////////////////////////////////////////////////////////

    public void SetupInitialPool()
    {
        for (int i = 0; i < intialPoolCount; i++)
        {
            //Create and Enqueue A New Object
            inactivepool_Queue.Enqueue(CreateObject());
        }
    }

    /////////////////////////////////////////////////////////////////

    private HexChunk CreateObject()
    {
        //Create The New Chunk
        HexChunk newChunk = Instantiate(hexChunk_Prefab, inactiveChunks_Container.transform).GetComponent<HexChunk>();

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
        if (inactivepool_Queue.Count == 0)
        {
            inactivepool_Queue.Enqueue(CreateObject());
        }

        //Get / Dequeue Next Script From Inactive Queue
        HexChunk poolableObject = inactivepool_Queue.Dequeue();

        //Set / Enqueue Next Script To Active Queue
        activepool_Queue.Enqueue(poolableObject);

        //Setup On Spawn Method
        poolableObject.Poolable_OnSpawn();

        //Return The New Chunk
        return poolableObject;
    }

    public void DeactivateObject(HexChunk poolableObject)
    {
        //Build a new queue without the specified elements
        activepool_Queue = new Queue<HexChunk>(activepool_Queue.Where(x => !((x.chunkCoords.x == poolableObject.chunkCoords.x) && (x.chunkCoords.y == poolableObject.chunkCoords.y))));

        //Set / Enqueue Next Script To Active Queue
        inactivepool_Queue.Enqueue(poolableObject);

        //Setup On Spawn Method
        poolableObject.Poolable_OnDespawn();
    }

    public void DeactivateObjects_All()
    {
        int counter = activepool_Queue.Count;
        for (int i = 0; i < counter; i++)
        {
            HexChunk poolableObject = activepool_Queue.Dequeue();
            inactivepool_Queue.Enqueue(poolableObject);
            poolableObject.Poolable_OnDespawn();
        }
    }

    /////////////////////////////////////////////////////////////////
}
