using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Uniblocks {

public class EngineSettings : EditorWindow {

	
	[MenuItem ("Window/Uniblocks: Engine Settings")]
	static void Init () {
		
		EngineSettings window = (EngineSettings)EditorWindow.GetWindow (typeof (EngineSettings));
		window.Show();
	}
	
	public void OnGUI () {
		
		if (GameObject.FindWithTag("UBEngine") == null || GameObject.FindWithTag("UBEngine").GetComponent<Engine>() == null) {
			EditorGUILayout.LabelField ("Cannot find an Engine game object in the scene!");
			return;
		}
		
		else if (GameObject.FindWithTag("UBEngine").GetComponent<ChunkManager>() == null) {
			EditorGUILayout.LabelField ("The Engine game object does not have a ChunkManager component!");
			return;
		}
		
		else {
			
			Engine engine = GameObject.FindWithTag("UBEngine").GetComponent<Engine>();
			
			EditorGUILayout.BeginVertical();
			
			GUILayout.Space (10);
			
			engine.lWorldName = EditorGUILayout.TextField ( "World name", engine.lWorldName );
			
			GUILayout.Space (20);
			
			GUILayout.Label ("Chunk settings");
			
			engine.lChunkSpawnDistance = EditorGUILayout.IntField ( "Chunk spawn distance", engine.lChunkSpawnDistance );
			engine.lHeightRange = EditorGUILayout.IntField ( "Chunk height range", engine.lHeightRange );
			engine.lChunkSideLength = EditorGUILayout.IntField ( "Chunk side length", engine.lChunkSideLength );
			engine.lTextureUnit = EditorGUILayout.FloatField ( "Texture unit", engine.lTextureUnit );
			engine.lGenerateMeshes = EditorGUILayout.Toggle ( "Generate meshes", engine.lGenerateMeshes);
			engine.lGenerateColliders = EditorGUILayout.Toggle ( "Generate colliders", engine.lGenerateColliders );
			engine.lShowBorderFaces = EditorGUILayout.Toggle ( "Show border faces", engine.lShowBorderFaces );
			
			GUILayout.Space (20);
			GUILayout.Label ("Events settings");
			engine.lSendCameraLookEvents = EditorGUILayout.Toggle ( "Send camera look events", engine.lSendCameraLookEvents);
			engine.lSendCursorEvents = EditorGUILayout.Toggle ( "Send cursor events", engine.lSendCursorEvents );
			
			GUILayout.Space (20);
			GUILayout.Label ("Data settings");
			engine.lSaveVoxelData = EditorGUILayout.Toggle ( "Save/load voxel data", engine.lSaveVoxelData );
			
			GUILayout.Space (20);
			GUILayout.Label ("Multiplayer");
			engine.lEnableMultiplayer = EditorGUILayout.Toggle ( "Enable multiplayer", engine.lEnableMultiplayer );
			engine.lMultiplayerTrackPosition = EditorGUILayout.Toggle ( "Track player position", engine.lMultiplayerTrackPosition );
			engine.lChunkTimeout = EditorGUILayout.FloatField ( "Chunk timeout (0=off)", engine.lChunkTimeout );
			
			GUILayout.Space (40);
			GUILayout.Label ("Performance");
			ChunkManager chunkManager = GameObject.FindWithTag("UBEngine").GetComponent<ChunkManager>();
			
			chunkManager.lMaxChunkSpawns = EditorGUILayout.IntField ("Chunk spawns limit", chunkManager.lMaxChunkSpawns);
			chunkManager.lMaxChunkSaves = EditorGUILayout.IntField ("Chunk saves limit", chunkManager.lMaxChunkSaves);
			
			float meshUpdatesPerFrame = 1.0f / chunkManager.lUpdateValue;			
			meshUpdatesPerFrame = EditorGUILayout.FloatField ("Mesh updates limit", meshUpdatesPerFrame);
			chunkManager.lUpdateValue = 1.0f / meshUpdatesPerFrame;			
			if (meshUpdatesPerFrame == 0) chunkManager.lUpdateValue = 1.0f;
			
			
			
			
			if (GUI.changed) {
				UnityEditor.PrefabUtility.ReplacePrefab(engine.gameObject, UnityEditor.PrefabUtility.GetPrefabParent(engine.gameObject), ReplacePrefabOptions.ConnectToPrefab);
			}
		
			EditorGUILayout.EndVertical();
		}
	}	
}

}