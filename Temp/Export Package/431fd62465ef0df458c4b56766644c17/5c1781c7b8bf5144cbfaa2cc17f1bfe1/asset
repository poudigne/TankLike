using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Controls spawning and destroying chunks.

namespace Uniblocks {

public class ChunkManager : MonoBehaviour {
		
	public GameObject ChunkObject; // Chunk prefab

	// chunk lists
	private GameObject[,,] ChunksInRange;
	private List<Chunk> FreshlySpawned;
	
	// global settings
	public static int MaxChunkSpawns;
	public int lMaxChunkSpawns = 100;
	
	public static float UpdateValue;
	public float lUpdateValue = 1.0f;
	
	public static int MaxChunkSaves;
	public int lMaxChunkSaves;
	
	// global flags
	public static bool SpawningChunks;
	public static int SavesThisFrame;
	
	// local flags	
	private bool Done;
	private int SpawnQueue;
	private Index LastRequest;
	private float UpdatesThisFrame;
	
	
	public void Awake () {
		
		ChunkManager.MaxChunkSpawns = lMaxChunkSpawns;
		ChunkManager.MaxChunkSaves = lMaxChunkSaves;
		ChunkManager.UpdateValue = lUpdateValue;
		
		Engine.ChunkScale = ChunkObject.transform.localScale;
		
		Done = true;
		ChunkManager.SpawningChunks = false;
	}
	
	
	public static GameObject GetChunk (int x, int y, int z) { // returns the gameObject of the chunk with the specified x,y,z, or returns null if the object is not instantiated
		
		// WARNING: this can be somewhat slow, since it has to iterate through potentially all existing chunks in order to find the one we're looking for. Avoid using too many times per frame.
		
		foreach (GameObject chunkObject in GameObject.FindGameObjectsWithTag("UBChunk")) {		
			
			if (chunkObject != null) {
				Chunk chunk = chunkObject.GetComponent<Chunk>();
				if (chunk.ChunkIndex.x == x &&
					chunk.ChunkIndex.y == y &&
					chunk.ChunkIndex.z == z	) {	
					return chunkObject;
				}
			}
		}
		return null;
	}
	
	public static GameObject GetChunk (Index index) {
		return GetChunk (index.x, index.y, index.z);
	}
	
	public static Chunk GetChunkComponent (int x, int y, int z) {
		GameObject chunkObject = GetChunk (x,y,z);
		if (chunkObject != null) {
			return chunkObject.GetComponent<Chunk>();
		}
		else {
			return null;
		}
	}
	
	public static Chunk GetChunkComponent (Index index) {
		return GetChunkComponent (index.x, index.y, index.z);
	}
	
	
	// ==== spawn chunks functions ====
	
	public static GameObject SpawnChunk ( int x, int y, int z ) { // spawns a single chunk (only if it's not already spawned)
		
		GameObject chunk = ChunkManager.GetChunk (x,y,z);
		if (chunk == null) {
			return GameObject.FindWithTag("UBEngine").GetComponent<ChunkManager>().DoSpawnChunk(new Index (x,y,z));
		}
		else return chunk;
	}
	
	public static GameObject SpawnChunk ( Index index ) { // spawns a single chunk (only if it's not already spawned)
		
		GameObject chunk = ChunkManager.GetChunk (index);
		if (chunk == null) {
			return GameObject.FindWithTag("UBEngine").GetComponent<ChunkManager>().DoSpawnChunk(index);
		}
		else return chunk;
	}
	GameObject DoSpawnChunk (Index index) {
		GameObject chunkObject = Instantiate(ChunkObject, index.ToVector3(), transform.rotation) as GameObject;
		Chunk chunk = chunkObject.GetComponent<Chunk>();
		chunk.DisableMesh = true; // disable mesh for chunks spawned with SpawnChunk
		chunk.EnableTimeout = true;
		return chunkObject;
	}
	
	public static void SpawnChunks ( float x, float y, float z) { // take world pos, convert to chunk index
		Index index = Engine.PositionToChunkIndex (new Vector3(x,y,z));
		GameObject.FindWithTag("UBEngine").GetComponent<ChunkManager>().TrySpawnChunks(index);
	}
	public static void SpawnChunks ( Vector3 position ) {
		Index index = Engine.PositionToChunkIndex (position);
		GameObject.FindWithTag("UBEngine").GetComponent<ChunkManager>().TrySpawnChunks(index);
	}
	
	public static void SpawnChunks ( int x, int y, int z ) { // take chunk index, no conversion needed
		GameObject.FindWithTag("UBEngine").GetComponent<ChunkManager>().TrySpawnChunks (x,y,z);
	}
	public static void SpawnChunks ( Index index ) {
		GameObject.FindWithTag("UBEngine").GetComponent<ChunkManager>().TrySpawnChunks (index.x, index.y, index.z);
	}
	
	private void TrySpawnChunks ( Index index ) {
		TrySpawnChunks (index.x, index.y, index.z);
	}	
	private void TrySpawnChunks ( int x, int y, int z ) {
		
		if (Done == true) { // if we're not spawning chunks at the moment, just spawn them normally
			StartSpawnChunks (x,y,z);
		}
		else { // if we are spawning chunks already, flag to spawn again once the previous round is finished using the last requested position as origin.
			LastRequest = new Index(x,y,z);
			SpawnQueue = 1;
		}	
	}
	
	public void Update () {
		
		if (SpawnQueue == 1 && Done == true) { // if there is a chunk spawn queued up, and if the previous one has finished, run the queued chunk spawn.
			SpawnQueue = 0;
			StartSpawnChunks (LastRequest.x, LastRequest.y, LastRequest.z);
		}
	}
	
	private void StartSpawnChunks ( int originX, int originY, int originZ ) {
			
		ChunkManager.SpawningChunks = true;
		Done = false;		
	
		int range = Engine.ChunkSpawnDistance;
	
		InitializeInRangeArray (originX, originY, originZ, range); // NEXT STEP <===== 1 
		AddSpawnedChunksToInRangeArray (originX, originY, originZ, range); // NEXT STEP <===== 2
		
	}
	
	private void InitializeInRangeArray ( int originX, int originY, int originZ, int range ) { // initializes an array that will have a space for every chunk in the specified range
	
		range = range *2; // convert radius to diameter
		
		ChunksInRange = new GameObject [range+1, range+1, range+1]; 
		
		
	}
	
	private void AddSpawnedChunksToInRangeArray ( int originX, int originY, int originZ, int range ) {
	
		var heightRange = Engine.HeightRange;
		// for all chunks, try to add it to the array if it's within range
		foreach (GameObject chunkObject in GameObject.FindGameObjectsWithTag("UBChunk")) {
			Chunk chunk = chunkObject.GetComponent<Chunk>();
			try {
				int x = chunk.ChunkIndex.x;
				int y = chunk.ChunkIndex.y;
				int z = chunk.ChunkIndex.z;
				
				if (Mathf.Abs(y) <= heightRange) { // skip chunks outside of height range
				if (Mathf.Abs(originX-x) + Mathf.Abs(originZ-z) < range * 1.3f) { // skip corners
				
					ChunksInRange [x-originX+range, y-originY+range, z-originZ+range] = chunkObject; // conversion to relative index happens here
				
				} else chunk.FlagToRemove();
				} else chunk.FlagToRemove();
			}
			catch (System.Exception) {
				chunk.FlagToRemove(); // remove chunks not in range
			}
		}
		
		StartCoroutine(SpawnMissingChunks (originX, originY, originZ, range)); // NEXT STEP <===== 3
		
	}
	
	private int TotalChunksSpawned;
	private IEnumerator SpawnMissingChunks ( int originX, int originY, int originZ, int range ) { // this checks for empty spots in the array and instantiates a chunk to fill them up.
			
		int chunksSpawned = 0;
		int heightRange =  Engine.HeightRange;
		
		FreshlySpawned = new List<Chunk>();
		
		for (int currentLoop = -range; currentLoop < range; currentLoop++) {
			for (var x=originX-currentLoop; x<=originX+currentLoop; x++) { // iterate through all potential chunk indexes within range
			for (var y=originY-currentLoop; y<=originY+currentLoop; y++) { // those are absolute indexes of chunks
			for (var z=originZ-currentLoop; z<=originZ+currentLoop; z++) { // so convert them to relative before checking the array
		
				if (Mathf.Abs(y) <= heightRange) { // skip chunks outside of height range
				if (Mathf.Abs(originX-x) + Mathf.Abs(originZ-z) < range * 1.3f) { // skip corners
				
					// conversion to relative index(ChunksInRange array is local and temporary, it doesn't store chunks according to their absolute index)
					int relativeX = x-originX+range;
					int relativeY = y-originY+range;
					int relativeZ = z-originZ+range;
					
					if (ChunksInRange[relativeX, relativeY, relativeZ] == null) { 
					
						GameObject newChunk = Instantiate(ChunkObject, new Vector3(x,y,z), transform.rotation) as GameObject; // Spawn a new chunk.
						ChunksInRange[relativeX, relativeY, relativeZ] = newChunk;
						FreshlySpawned.Add(newChunk.GetComponent<Chunk>());
						TotalChunksSpawned ++;
						
						chunksSpawned ++;
						if (chunksSpawned > ChunkManager.MaxChunkSpawns) {
							chunksSpawned = 0;
							yield return new WaitForEndOfFrame();							
						}
					}	
					
				}
				}
			}
			}
			}
		}	
		
		if (Engine.EnableMultiplayer && Network.isClient) {
			StartCoroutine(WaitForDataToArrive());// NEXT STEP (multiplayer only) <===== 3.5
		}
		else {
			StartCoroutine(SpawnVoxelsForFreshChunks ()); // NEXT STEP <===== 4
		}
	}
	
	public static int DataReceivedCount;
	private IEnumerator WaitForDataToArrive () { // wait until the data of all chunks has been received from server
		Debug.Log ("Waiting for data, total count expected is "+TotalChunksSpawned.ToString());
		while (ChunkManager.DataReceivedCount < TotalChunksSpawned) {
			yield return new WaitForEndOfFrame();
		}
		ChunkManager.DataReceivedCount = 0;
		StartCoroutine (SpawnVoxelsForFreshChunks ()); // NEXT STEP <===== 4
	}
	
	private IEnumerator SpawnVoxelsForFreshChunks () {
	
		foreach (Chunk chunk in FreshlySpawned) {
			
			if (chunk.Empty == false) { 
				UpdatesThisFrame += ChunkManager.UpdateValue;
				chunk.FlagToUpdate();
			}
					
			
			while ( UpdatesThisFrame >= 1.0f ) { // limit chunk updates per frame
				yield return new WaitForEndOfFrame();
				UpdatesThisFrame -= 1.0f;
			}
		}
		
		StartCoroutine(UpdateNeighborsOfFreshChunks()); // NEXT STEP <===== 5
	}
	private IEnumerator UpdateNeighborsOfFreshChunks () {
		
		foreach (Chunk chunk in FreshlySpawned) {
			
			chunk.GetNeighbors();
			foreach (var neighbor in chunk.NeighborChunks) {
				if (neighbor != null && neighbor.Fresh == false) {
					neighbor.FlagToUpdate();
					UpdatesThisFrame += ChunkManager.UpdateValue;
					
					while (UpdatesThisFrame > 1.0f) {
						yield return new WaitForEndOfFrame ();
						UpdatesThisFrame -= 1.0f;
					}
				}
			}
		}
		
		
		UnflagFreshChunks(); // NEXT STEP <===== 6
	}
	private void UnflagFreshChunks () {
		
		foreach (Chunk chunk in FreshlySpawned) {
			chunk.Fresh = false;
		}
		
		TotalChunksSpawned = 0;
		FreshlySpawned.Clear();	
		ChunkManager.SpawningChunks = false;
		
		Done = true;
		
	}

}

}