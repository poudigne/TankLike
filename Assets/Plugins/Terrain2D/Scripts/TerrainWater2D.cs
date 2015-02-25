//
// Gargore TERRAIN 2D (standard edition, version 0.1)
//

//
// IMPORTANT NOTICE: THIS FILE SHOULD NOT BE EDITED, IF YOU REALLY NEED TO
//                   MODIFY IT, CREATE A SUBCLASS WITH EXTEND (CHECK THE MANUAL)

using UnityEngine;
using System.Collections;

public class TerrainWater2D: MonoBehaviour {
	public Terrain2D targetTerrain2D = null;
	public float targetRadius = 1f;
	public float targetSourceRate = 1f;
	public float targetDrainRate = 0f;
	public float targetLapse = 0.01f;
	private float targetNext = 0f;
	
	void Update() {
		if (targetTerrain2D != null) {
			if (Time.time > targetNext) {
				if ((targetTerrain2D.data != null) && (targetTerrain2D.data.Length == targetTerrain2D.dataWidth * targetTerrain2D.dataHeight) && (targetTerrain2D.scale.x > 0f) && (targetTerrain2D.scale.y > 0f)) {
					Vector3 pos = gameObject.transform.position - targetTerrain2D.transform.position;
					int i = Mathf.FloorToInt(Vector3.Dot(pos, targetTerrain2D.transform.right) / targetTerrain2D.scale.x);
					int j = Mathf.FloorToInt(Vector3.Dot(pos, targetTerrain2D.transform.up) / targetTerrain2D.scale.y);
					if ((i >= 0) && (i < targetTerrain2D.dataWidth) && (j >= 0) && (j < targetTerrain2D.dataHeight)) {
						targetTerrain2D.data[i + j * targetTerrain2D.dataWidth] += (targetSourceRate - targetDrainRate);
						if (targetTerrain2D.data[i + j * targetTerrain2D.dataWidth] < 0f) targetTerrain2D.data[i + j * targetTerrain2D.dataWidth] = 0f;
						if (targetTerrain2D.data[i + j * targetTerrain2D.dataWidth] > 1f) targetTerrain2D.data[i + j * targetTerrain2D.dataWidth] = 1f;
					}
				}
				targetNext += targetLapse; if (Time.time > targetNext + 10f) targetNext = Time.time + targetLapse;
			}
		}
	}
}

