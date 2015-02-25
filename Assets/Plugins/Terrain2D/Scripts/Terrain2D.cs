//
// Gargore TERRAIN 2D (standard edition, version 0.1)
//

//
// IMPORTANT NOTICE: THIS FILE SHOULD NOT BE EDITED, IF YOU REALLY NEED TO
//                   MODIFY IT, CREATE A SUBCLASS WITH EXTEND (CHECK THE MANUAL)

#if !(UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
	#define TERRAIN_2D_PHYSICS
#endif
//#define COMMON_COMPUTED_BORDERS
//#define COMMON_RENDERED_BORDERS

using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class Terrain2D: MonoBehaviour {
	public enum TerrainBrushType2D { brushsize_radius, distance_proportional_radius, single_cell_hard, single_cell_soft, multi_cell_soft, distance_single_or_multi_cell_auto, smooth_brushsize_radius, smooth_distance_proportional_radius, smooth_single_cell_hard, smooth_single_cell_soft, smooth_multi_cell_soft, smooth_distance_single_or_multi_cell_auto };
	public enum TerrainLayer2D { master, material1, material2, material3, material4 }

	public bool editorEnabled = false;
	[System.NonSerialized]public bool editorWasEnabled = false;
	public float editorBrushSize = 1f;
	public TerrainBrushType2D editorBrushType = TerrainBrushType2D.brushsize_radius;
	[HideInInspector]public bool editorDigging = true;
	[HideInInspector]public bool editorDebugMessages = false;
	public TerrainLayer2D editorLayer = TerrainLayer2D.master;

	public bool editInRuntimeEnabled = false;
	public float editInRuntimeBrushSize = 1f;
	public TerrainBrushType2D editInRuntimeBrushType = TerrainBrushType2D.distance_proportional_radius;
	private bool editInverse = false;
	public bool editInverseDefault = false;
	public int editPasses = 1;
	public float editLapse = 0.1f;
	private float editNext = 0f;
	public string editKeynames = "mouse 0";
	private string keynamesInverse = "left shift";

	[HideInInspector]public bool dirty = false;
	[HideInInspector]public int dataVersion = 0;
	[HideInInspector]public float[] data = null;
	[HideInInspector]public int dataLeft = 0;
	[HideInInspector]public int dataTop = 0;
	[HideInInspector]public float[] datad = null;
	public int dataWidth = 0;
	public int dataHeight = 0;
	private int lastDataWidth = 0;
	private int lastDataHeight = 0;
	private int lastDatadWidth = 0;
	private int lastDatadHeight = 0;
	private int lastDataMaterialLayers = 0;

	[HideInInspector]public int dataMaterialLayers = 4;

	public int dataPerBlockHorizontal = 4;
	public int dataPerBlockVertical = 4;

	public Vector3 scale = Vector3.one;
	public float zOffset = 0f;
	public float zEmptyOffset = 0f;
	public float zWallsOffset = 1f;
	public float zWallsWidth = -1f;
	public float zBorderOffset = 0f;
	public float zBorderWidth = 0.1f;

	private const float DEFAULT_DATA = 0f;
	private float mousePositionX = 0f;
	private float mousePositionY = 0f;
	private bool editorMode = false;
	private Ray editorRay = new Ray(Vector3.zero, Vector3.one);
	private bool Input_GetKey_editKeyname = false;
	private bool Input_GetKey_editKeynameInverse = false;

	private bool instanceReady = false;
	private float instanceCheckNext = 0f;
	private float instanceCheckInterval = 1f;

	public Material material = null;

	public Vector3 materialTextureTiling = Vector3.one;
	public Vector3 materialTextureOffset = Vector3.zero;
	public Vector3 materialTextureUSource = new Vector3(1f, 0f, .5f);
	public Vector3 materialTextureVSource = new Vector3(0f, 1f, .75f);
	public Terrain2D layerMaster = null;
	public Terrain2D layerSlave = null;
	public float layerRenderLapse = 0.1f;
	private float layerRenderNext = 0f;
	private bool layerSlaveReady = false;
	public bool layerDefaultFilled = true;
	public bool layerDefaultOutsideFilled = false;
	public bool layerRenderFilled = true;
	public bool layerRenderEmpty = false;
	public bool layerRenderWalls = true;
	public bool layerRenderBorder = false;
	public bool layerCollideWalls = true;
#if TERRAIN_2D_PHYSICS
	public bool layerCollideWalls2D = false;
#endif

	public bool layerRenderWater = false;
	public float waterRenderLapse = 0.5f;
	private float waterRenderNext = 0f;
	public Terrain2D layerWaterReference = null;
	public int waterPasses = 5;
	public float waterPassLapse = 0.02f;
	private float waterPassNext = 0f;
	[HideInInspector]public float[] waterData = null;
	public float waterDataChange = 0.1f;
	[HideInInspector]public float waterBias = 1.0f;
	[HideInInspector]public float waterDelta = 1.0f;

	public string specialKeynames = "";

	public bool saveEnabled = false;
	public string saveKeynames = "f2";
	public string saveRecordName = "unnamed layer";
	public bool saveAutoLoadFile = false;
	public bool saveAutoLoadSerialize = true;
	public bool saveForceLoadSerialize = false;
	public bool saveForceDumpSerialize = false;

	public Terrain2D neighbourXmYm = null;
	public Terrain2D neighbourYm = null;
	public Terrain2D neighbourXpYm = null;
	public Terrain2D neighbourXm = null;
	public Terrain2D neighbourXp = null;
	public Terrain2D neighbourXmYp = null;
	public Terrain2D neighbourYp = null;
	public Terrain2D neighbourXpYp = null;

	public bool spawnEnabled = false;
	public string spawnKeynames = "mouse 1;mouse 2";
	public bool cameraManagement = false;
	public bool cameraHold = false;
	public string cameraHoldKeynames = "h";
	private bool cameraHoldActive = true;
	public GameObject objectHold = null;
	public Vector3 objectHoldDelta = Vector3.zero;

	[System.Serializable]public class TerrainBlock2D {
		public GameObject gameObject = null;
		public Mesh mesh = null;
		public MeshMetadata meshMetadata = null;
		public Mesh colliderMesh = null;
		public MeshMetadata colliderMeshMetadata = null;
		public bool useCollider = false;
		public int i = 0, j = 0;
		public int left = 0, top = 0;
		public int width = 0, height = 0;
		[System.NonSerialized]public bool dirty = false;
		[System.NonSerialized]public bool visible = false;
		[System.NonSerialized]public int count = 1999999999;
		[System.NonSerialized]public float lastRedrawTime = -999999999f;

		public TerrainBlock2D XmYm = null, Ym = null, XpYm = null;
		public TerrainBlock2D Xm = null, Xp = null;
		public TerrainBlock2D XmYp = null, Yp = null, XpYp = null;
		public Vector2[] outlineVertex1 = null;
		public Vector2[] outlineVertex2 = null;
		public Vector2[] outlineConnectedVertex1 = null;
		public Vector2[] outlineConnectedVertex2 = null;
		public bool[] outlineExistsConnectedVertex1 = null;
		public bool[] outlineExistsConnectedVertex2 = null;
	}

	public class TerrainShape2D {
		public Vector3[] shape = null;
		public Vector2[] shape2d = null;
	}

	[System.NonSerialized]public TerrainBlock2D[] blocks = null;
	[HideInInspector]public int blocksLeft = 0;
	[HideInInspector]public int blocksTop = 0;
	[HideInInspector]public int blocksWidth = 0;
	[HideInInspector]public int blocksHeight = 0;
	private int lastBlocksWidth = 0;
	private int lastBlocksHeight = 0;
	public bool debugMessages = false;
	public GUIText debugProbe = null;

	private static char[] configSplitKeynames = {';'};
	private bool keynamesGetKey(string keynames) {
		string[] keynamesArray = keynames.Split(configSplitKeynames);
		for (int i = 0; i < keynamesArray.Length; ++i) {
			try {
				if (Input.GetKey(keynamesArray[i])) return true;
			} catch { ; }
		}
		return false;
	}
	private bool keynamesGetKeyDown(string keynames) {
		string[] keynamesArray = keynames.Split(configSplitKeynames);
		for (int i = 0; i < keynamesArray.Length; ++i) {
			try {
				if (Input.GetKeyDown(keynamesArray[i])) return true;
			} catch { ; }
		}
		return false;
	}

	private bool internal_isRunning = false;
	public bool isRunning() {
		return internal_isRunning;
	}
	void Update() {
		internal_isRunning = true;
		UpdateInternal(false, Input.mousePosition.x, Input.mousePosition.y, keynamesGetKey(editKeynames), keynamesGetKey(keynamesInverse));
	}

	public void UpdateInternal(bool editorMode, float mousePositionX, float mousePositionY, bool editDig, bool editInverse, Ray ray) {
		editorRay = ray;
		UpdateInternal(editorMode, mousePositionX, mousePositionY, editDig, editInverse);
	}
	public void UpdateInternal(bool editorMode, float mousePositionX, float mousePositionY, bool editDig, bool editInverse) {
		this.editorMode = editorMode;
		this.mousePositionX = mousePositionX;
		this.mousePositionY = mousePositionY;
		this.Input_GetKey_editKeyname = editDig;
		this.Input_GetKey_editKeynameInverse = editInverse;

		if (!editorMode) {
			if (!instanceReady) {
				prepareInstance();
			} else if (Time.time > instanceCheckNext) checkInstance();
		} else {
			checkInstance();
		}

		if (!editorMode) {
			if (layerRenderWater) fluid_stat_countA = fluid_stat_countB = fluid_stat_count = fluid_stat_count0 = fluid_stat_count1 = fluid_stat_count2 = 0;
			if (Time.time > waterPassNext) {
				if (layerRenderWater && (layerWaterReference != null)) for (int i = 0; i < waterPasses; ++i) fluid_blob(layerWaterReference, true);
				waterPassNext += waterPassLapse; if (Time.time > waterPassNext + 10f) waterPassNext = Time.time + waterPassLapse;
			}
		}
		if (layerRenderWater) {
			if (Time.time > waterRenderNext) {
				waterCheckInstance();
				waterRenderNext = Time.time + waterRenderLapse;
			}
		}

		if (Time.time > layerRenderNext) {
			if (dirty) redrawInstance();
			layerRenderNext = Time.time + layerRenderLapse;
		}

		if (Time.time > editNext) {
			if (editInRuntimeEnabled || spawnEnabled) for (int i = 0; i < editPasses; ++i) editInstance();
			editNext += editLapse; if (Time.time > editNext + 10f) editNext = Time.time + editLapse;
		}
		if (!editorMode) if (saveEnabled) if (keynamesGetKeyDown(saveKeynames)) saveLayer("layer." + saveRecordName + ".txt");
		if (!editorMode) if (cameraHold) if (keynamesGetKeyDown(cameraHoldKeynames)) cameraHoldActive = !cameraHoldActive;
		if (!editorMode) if (keynamesGetKeyDown(specialKeynames)) outlineLayer();
		if (!editorMode) if (cameraHold) cameraHoldInstance();
		if (!editorMode) if (objectHold != null) objectHoldInstance();

		if (!editorMode) if (debugProbe != null) {
			int i, j;
			if (pointerInstance(out i, out j)) {

				if (data != null) {
					for (int jj = j + 4; jj >= j - 4; --jj) {
						for (int ii = i - 4; ii <= i + 4; ++ii) {
							if ((ii == i) || (jj == j)) {
								if ((ii >= 0) && (ii < dataWidth) && (jj >= 0) && (jj < dataHeight)) {

								} else {

								}
							} else {
								if ((ii >= 0) && (ii < dataWidth) && (jj >= 0) && (jj < dataHeight)) {

								} else {

								}
							}
						}

					}
				}
				if (layerWaterReference != null) {
					if ((i >= 0) && (i < dataWidth) && (j >= 0) && (j < dataHeight)) {

					} else {

					}
				}
				if (layerWaterReference != null) {
					if (layerWaterReference.pointerInstance(out i, out j)) {

						if (layerWaterReference.data != null) {
							for (int jj = j + 4; jj >= j - 4; --jj) {
								for (int ii = i - 4; ii <= i + 4; ++ii) {
									if ((ii == i) || (jj == j)) {
										if ((ii >= 0) && (ii < layerWaterReference.dataWidth) && (jj >= 0) && (jj < layerWaterReference.dataHeight)) {

										} else {

										}
									} else {
										if ((ii >= 0) && (ii < layerWaterReference.dataWidth) && (jj >= 0) && (jj < layerWaterReference.dataHeight)) {

										} else {

										}
									}
								}

							}
						}
					}
				}

			}
		}
	}

	void prepareInstance() {
		instanceReady = true;
		if ((layerMaster == null) && (data != null) && (data.Length == dataWidth * dataHeight)) {
			lastDataWidth = dataWidth;
			lastDataHeight = dataHeight;
		}

		int ddWidth = dataWidth;
		int ddHeight = dataHeight;
		if (dataPerBlockHorizontal > 0f) ddWidth = dataWidth / dataPerBlockHorizontal;
		if (dataPerBlockVertical > 0f) ddHeight = dataHeight / dataPerBlockVertical;
		if ((layerMaster == null) && (datad != null) && (datad.Length == ddWidth * ddHeight * dataMaterialLayers)) {
			lastDatadWidth = ddWidth;
			lastDatadHeight = ddHeight;
			lastDataMaterialLayers = dataMaterialLayers;
		}

		if (saveAutoLoadFile) loadLayer("layer." + saveRecordName + ".txt");
		checkInstance();
	}

	void checkInstance() {
		instanceCheckNext = Time.time + instanceCheckInterval;

		if ((neighbourXmYm != null) && (neighbourXmYm.neighbourXpYp == null)) neighbourXmYm.neighbourXpYp = this;

		if ((neighbourYm != null) && (neighbourYm.neighbourYp == null)) neighbourYm.neighbourYp = this;
		if ((neighbourYm != null) && (neighbourXmYm != null) && (neighbourYm.neighbourXm == null)) neighbourYm.neighbourXm = neighbourXmYm;
		if ((neighbourYm != null) && (neighbourXpYm != null) && (neighbourYm.neighbourXp == null)) neighbourYm.neighbourXp = neighbourXpYm;
		if ((neighbourXmYm != null) && (neighbourYm != null) && (neighbourXmYm.neighbourXp == null)) neighbourXmYm.neighbourXp = neighbourYm;
		if ((neighbourXpYm != null) && (neighbourYm != null) && (neighbourXpYm.neighbourXm == null)) neighbourXpYm.neighbourXm = neighbourYm;
		if ((neighbourXm != null) && (neighbourYm != null) && (neighbourXm.neighbourXpYm == null)) neighbourXm.neighbourXpYm = neighbourYm;
		if ((neighbourXp != null) && (neighbourYm != null) && (neighbourXp.neighbourXmYm == null)) neighbourXp.neighbourXmYm = neighbourYm;

		if ((neighbourXpYm != null) && (neighbourXpYm.neighbourXmYp == null)) neighbourXpYm.neighbourXmYp = this;

		if ((neighbourXm != null) && (neighbourXm.neighbourXp == null)) neighbourXm.neighbourXp = this;
		if ((neighbourXm != null) && (neighbourXmYm != null) && (neighbourXm.neighbourYm == null)) neighbourXm.neighbourYm = neighbourXmYm;
		if ((neighbourXm != null) && (neighbourXmYp != null) && (neighbourXm.neighbourYp == null)) neighbourXm.neighbourYp = neighbourXmYp;
		if ((neighbourXmYm != null) && (neighbourXm != null) && (neighbourXmYm.neighbourYp == null)) neighbourXmYm.neighbourYp = neighbourXm;
		if ((neighbourXmYp != null) && (neighbourXm != null) && (neighbourXmYp.neighbourYm == null)) neighbourXmYp.neighbourYm = neighbourXm;
		if ((neighbourYm != null) && (neighbourXm != null) && (neighbourYm.neighbourXmYp == null)) neighbourYm.neighbourXmYp = neighbourXm;
		if ((neighbourYp != null) && (neighbourXm != null) && (neighbourYp.neighbourXmYm == null)) neighbourYp.neighbourXmYm = neighbourXm;

		if ((neighbourXp != null) && (neighbourXp.neighbourXm == null)) neighbourXp.neighbourXm = this;
		if ((neighbourXp != null) && (neighbourXpYm != null) && (neighbourXp.neighbourYm == null)) neighbourXp.neighbourYm = neighbourXpYm;
		if ((neighbourXp != null) && (neighbourXpYp != null) && (neighbourXp.neighbourYp == null)) neighbourXp.neighbourYp = neighbourXpYp;
		if ((neighbourXpYm != null) && (neighbourXp != null) && (neighbourXpYm.neighbourYp == null)) neighbourXpYm.neighbourYp = neighbourXp;
		if ((neighbourXpYp != null) && (neighbourXp != null) && (neighbourXpYp.neighbourYm == null)) neighbourXpYp.neighbourYm = neighbourXp;
		if ((neighbourYm != null) && (neighbourXp != null) && (neighbourYm.neighbourXpYp == null)) neighbourYm.neighbourXpYp = neighbourXp;
		if ((neighbourYp != null) && (neighbourXp != null) && (neighbourYp.neighbourXpYm == null)) neighbourYp.neighbourXpYm = neighbourXp;

		if ((neighbourXmYp != null) && (neighbourXmYp.neighbourXpYm == null)) neighbourXmYp.neighbourXpYm = this;

		if ((neighbourYp != null) && (neighbourYp.neighbourYm == null)) neighbourYp.neighbourYm = this;
		if ((neighbourYp != null) && (neighbourXmYp != null) && (neighbourYp.neighbourXm == null)) neighbourYp.neighbourXm = neighbourXmYp;
		if ((neighbourYp != null) && (neighbourXpYp != null) && (neighbourYp.neighbourXp == null)) neighbourYp.neighbourXp = neighbourXpYp;
		if ((neighbourXmYp != null) && (neighbourYp != null) && (neighbourXmYp.neighbourXp == null)) neighbourXmYp.neighbourXp = neighbourYp;
		if ((neighbourXpYp != null) && (neighbourYp != null) && (neighbourXpYp.neighbourXm == null)) neighbourXpYp.neighbourXm = neighbourYp;
		if ((neighbourXm != null) && (neighbourYp != null) && (neighbourXm.neighbourXpYp == null)) neighbourXm.neighbourXpYp = neighbourYp;
		if ((neighbourXp != null) && (neighbourYp != null) && (neighbourXp.neighbourXmYp == null)) neighbourXp.neighbourXmYp = neighbourYp;

		if ((neighbourXpYp != null) && (neighbourXpYp.neighbourXmYm == null)) neighbourXpYp.neighbourXmYm = this;

		if (layerMaster != null) {
			if (layerMaster.layerSlave == null) layerMaster.layerSlave = this;
			else if (layerMaster.layerSlave != this) if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": " + gameObject.name + " layerMaster (" + layerMaster.gameObject.name + ") has other layerSlave: " + layerMaster.layerSlave.gameObject.name + " but each layerMaster can have only 1 layerSlave.");
			if (!layerSlaveReady) dumpLayer();
		}
		if (layerSlave != null) {
			if (layerSlave.layerMaster == null) layerSlave.layerMaster = this;
			else if (layerSlave.layerMaster != this) if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": " + gameObject.name + " layerSlave (" + layerSlave.gameObject.name + ") has other layerMaster: " + layerSlave.layerMaster.gameObject.name + " but each layerSlave can have only 1 layerMaster.");
		}

		if (layerMaster == null) {
			checkInstanceData();

			if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ":layerSlave: " + ((layerSlave == null) ? "null" : layerSlave.ToString()));
			if (layerSlave != null) if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ":layerSlave.data: " + ((layerSlave.data == null) ? "null" : layerSlave.data.ToString()));
			if ((layerSlave != null) && ((layerSlave.data == null) || (layerSlave.data.Length != lastDataWidth * lastDataHeight))) {
				layerSlave.data = data;
				layerSlave.dataWidth = lastDataWidth;
				layerSlave.dataHeight = lastDataHeight;
				layerSlave.lastDataWidth = lastDataWidth;
				layerSlave.lastDataHeight = lastDataHeight;
				layerSlave.lastDatadWidth = lastDatadWidth;
				layerSlave.lastDatadHeight = lastDatadHeight;
				layerSlave.layerSlaveReady = true;
				layerSlave.checkInstance();
			}
		}

		if (layerRenderWater) {
			if (((waterData == null) && (data != null)) || ((data != null) && (data.Length != waterData.Length))) {
				waterData = new float[data.Length];
				for (int i = 0; i < data.Length; ++i) waterData[i] = data[i];
			}
		}

		if (data != null) {
			if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": " + lastDataWidth + " x " + lastDataHeight + " data cells");
			blocksWidth = Mathf.CeilToInt(lastDataWidth / (dataPerBlockHorizontal * 1f));
			blocksHeight = Mathf.CeilToInt(lastDataHeight / (dataPerBlockVertical * 1f));
			checkInstanceTiling();
			if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": " + blocksWidth + " x " + blocksHeight + " blocks");
			if ((blocks == null) || (blocks.Length != blocksWidth * blocksHeight)) {
				if ((blocks == null) || (blocks.Length != lastBlocksWidth * lastBlocksHeight)) {
					lastBlocksWidth = 0;
					lastBlocksHeight = 0;
				}
				if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": creating new blocks");
				TerrainBlock2D[] blocks1 = new TerrainBlock2D[blocksWidth * blocksHeight];
				if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": before " + lastBlocksWidth + " x " + lastBlocksHeight + ", now " + blocksWidth + " x " + blocksHeight + " blocks");
				for (int i = 0; i < lastBlocksWidth; ++i) for (int j = 0; j < lastBlocksHeight; ++j) {
					if ((i >= blocksWidth) || (j >= blocksHeight)) {
						if ((blocks[i + j * lastBlocksWidth] != null) && (blocks[i + j * lastBlocksWidth].gameObject != null)) {
							if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": destroy GameObject: " + blocks[i + j * lastBlocksWidth].gameObject.name);
							Destroy(blocks[i + j * lastBlocksWidth].gameObject);
						}
					}
				}

				for (int i = 0; i < blocksWidth; ++i) for (int j = 0; j < blocksHeight; ++j) {
					if ((i < lastBlocksWidth) && (j < lastBlocksHeight)) blocks1[i + j * blocksWidth] = blocks[i + j * lastBlocksWidth];
					else {
						blocks1[i + j * blocksWidth] = new TerrainBlock2D();

					}

					if (blocks1[i + j * blocksWidth].gameObject == null) {
						blocks1[i + j * blocksWidth].gameObject = new GameObject();
					}
					if (blocks1[i + j * blocksWidth].gameObject != null) {
						TerrainBlock2D block1 = blocks1[i + j * blocksWidth];
						block1.gameObject.transform.parent = gameObject.transform;
						block1.gameObject.transform.localPosition = new Vector3(i * scale.x * dataPerBlockHorizontal, j * scale.y * dataPerBlockVertical, 0.0f);
						block1.gameObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);
						block1.gameObject.name = gameObject.name + "_" + i + "_" + j;
						block1.i = i;
						block1.j = j;
						block1.left = i * dataPerBlockHorizontal;
						block1.top = j * dataPerBlockHorizontal;
						block1.width = lastDataWidth - block1.left; if (block1.width > dataPerBlockHorizontal) block1.width = dataPerBlockHorizontal;
						block1.height = lastDataHeight - block1.top; if (block1.height > dataPerBlockVertical) block1.height = dataPerBlockVertical;
						block1.meshMetadata = new MeshMetadata();
						if (layerCollideWalls) block1.colliderMesh = new Mesh();
						if (layerCollideWalls) block1.colliderMeshMetadata = new MeshMetadata();
					}

				}
				blocks = blocks1;

				lastBlocksWidth = blocksWidth;
				lastBlocksHeight = blocksHeight;
				for (int i = 0; i < blocksWidth; ++i) for (int j = 0; j < blocksHeight; ++j) {
					if (blocks[i + j * blocksWidth] != null) {
						if ((i > 0) && (j > 0)) if (blocks[(i - 1) + (j - 1) * blocksWidth] != null) blocks[i + j * blocksWidth].XmYm = blocks[(i - 1) + (j - 1) * blocksWidth];
						if ((i > 0) && (j < blocksHeight - 1)) if (blocks[(i - 1) + (j + 1) * blocksWidth] != null) blocks[i + j * blocksWidth].XmYp = blocks[(i - 1) + (j + 1) * blocksWidth];
						if ((i < blocksWidth - 1) && (j > 0)) if (blocks[(i + 1) + (j - 1) * blocksWidth] != null) blocks[i + j * blocksWidth].XpYm = blocks[(i + 1) + (j - 1) * blocksWidth];
						if ((i < blocksWidth - 1) && (j < blocksHeight - 1)) if (blocks[(i + 1) + (j + 1) * blocksWidth] != null) blocks[i + j * blocksWidth].XpYp = blocks[(i + 1) + (j + 1) * blocksWidth];
						if ((i > 0)) if (blocks[(i - 1) + (j) * blocksWidth] != null) blocks[i + j * blocksWidth].Xm = blocks[(i - 1) + (j) * blocksWidth];
						if ((j > 0)) if (blocks[(i) + (j - 1) * blocksWidth] != null) blocks[i + j * blocksWidth].Ym = blocks[(i) + (j - 1) * blocksWidth];
						if ((i < blocksWidth - 1)) if (blocks[(i + 1) + (j) * blocksWidth] != null) blocks[i + j * blocksWidth].Xp = blocks[(i + 1) + (j) * blocksWidth];
						if ((j < blocksHeight - 1)) if (blocks[(i) + (j + 1) * blocksWidth] != null) blocks[i + j * blocksWidth].Yp = blocks[(i) + (j + 1) * blocksWidth];
					}
				}

				for (int i = 0; i < blocksWidth; ++i) for (int j = 0; j < blocksHeight; ++j) {
					blockRedraw(blocks[i + j * blocksWidth]);
				}

			}
		}

		checkInstanceTiling();
	}

	bool blockAllocGameObject(int i, int j, TerrainBlock2D block, string name, bool layerCollideWalls, bool attachToAllocList) {
		if (block.gameObject == null) {

			block.gameObject = new GameObject();

		}
		if (block.gameObject != null) {
			block.gameObject.transform.parent = gameObject.transform;
			block.gameObject.transform.localPosition = new Vector3(i * scale.x * dataPerBlockHorizontal, j * scale.y * dataPerBlockVertical, 0.0f);
			block.gameObject.transform.localRotation = Quaternion.identity;
			block.gameObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);
			if (name == null) {
				block.gameObject.name = gameObject.name + "_" + i + "_" + j;
			} else {
				block.gameObject.name = name;
			}
			block.i = i;
			block.j = j;
			block.left = i * dataPerBlockHorizontal;
			block.top = j * dataPerBlockHorizontal;
			block.width = lastDataWidth - block.left; if (block.width > dataPerBlockHorizontal) block.width = dataPerBlockHorizontal;
			block.height = lastDataHeight - block.top; if (block.height > dataPerBlockVertical) block.height = dataPerBlockVertical;
			block.meshMetadata = new MeshMetadata();
			if (layerCollideWalls) block.colliderMesh = new Mesh();
			if (layerCollideWalls) block.colliderMeshMetadata = new MeshMetadata();
			return true;
		}
		return false;
	}
	bool blockAllocGameObject(int i, int j, TerrainBlock2D block, string name, bool layerCollideWalls) {
		return blockAllocGameObject(i, j, block, name, layerCollideWalls, true);
	}
	bool blockAllocGameObject(int i, int j, TerrainBlock2D block, bool layerCollideWalls) {
		return blockAllocGameObject(i, j, block, null, layerCollideWalls);
	}
	bool blockAllocGameObject(int i, int j, TerrainBlock2D block, string name) {
		return blockAllocGameObject(i, j, block, name, layerCollideWalls);
	}
	bool blockAllocGameObject(int i, int j, TerrainBlock2D block) {
		return blockAllocGameObject(i, j, block, null);
	}

	void checkInstanceTiling() {
		if ((Mathf.Abs(scale.x) > 0f) && (dataWidth > 0f)) {
			autouv_bias_to_u = 0f;
			autouv_x_to_u = materialTextureTiling.x * materialTextureUSource.x / dataWidth;
			autouv_y_to_u = materialTextureTiling.x * materialTextureUSource.y / dataWidth;
			autouv_z_to_u = materialTextureTiling.x * materialTextureUSource.z / dataWidth;
		} else {
			autouv_bias_to_u = 0.0f;
			autouv_x_to_u = 1.0f;
			autouv_y_to_u = 0.0f;
			autouv_z_to_u = 0.5f;
		}
		if ((Mathf.Abs(scale.y) > 0f) && (dataHeight > 0f)) {
			autouv_bias_to_v = 0f;
			autouv_x_to_v = materialTextureTiling.y * materialTextureVSource.x / dataHeight;
			autouv_y_to_v = materialTextureTiling.y * materialTextureVSource.y / dataHeight;
			autouv_z_to_v = materialTextureTiling.y * materialTextureVSource.z / dataHeight;
		} else {
			autouv_bias_to_v = 0.0f;
			autouv_x_to_v = 0.0f;
			autouv_y_to_v = 1.0f;
			autouv_z_to_v = 0.75f;
		}
	}

	void checkInstanceData() {
		if ((data == null) || (data.Length != dataWidth * dataHeight) || (data.Length != lastDataWidth * lastDataHeight)) {
			if ((data == null) || (data.Length != lastDataWidth * lastDataHeight)) {
				lastDataWidth = 0;
				lastDataHeight = 0;
			}
			if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": creating new data");
			Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": creating new data");
			float[] data1 = new float[dataWidth * dataHeight];
			for (int i = 0; i < dataWidth; ++i) for (int j = 0; j < dataHeight; ++j) {
				if ((i < lastDataWidth) && (j < lastDataHeight)) data1[i + j * dataWidth] = data[i + j * lastDataWidth];
				else data1[i + j * dataWidth] = layerDefaultFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
			data = data1;
			lastDataWidth = dataWidth;
			lastDataHeight = dataHeight;
		}
		int ddWidth = dataWidth;
		int ddHeight = dataHeight;
		if (dataPerBlockHorizontal > 0f) ddWidth = dataWidth / dataPerBlockHorizontal;
		if (dataPerBlockVertical > 0f) ddHeight = dataHeight / dataPerBlockVertical;
		if ((datad == null) || (datad.Length != ddWidth * ddHeight * dataMaterialLayers) || (datad.Length != lastDatadWidth * lastDatadHeight * lastDataMaterialLayers)) {
			//Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": creating new density data: datad = " + ((datad == null) ? "null" : "" + datad.Length + "") + "; ddWidth = " + ddWidth + "; ddHeight = " + ddHeight + "; dataMaterialLayers = " + dataMaterialLayers + "; lastDatadWidth = " + lastDatadWidth + "; lastDatadHeight = " + lastDatadHeight + "; lastDataMaterialLayers = " + lastDataMaterialLayers + ";");
			if ((datad == null) || (datad.Length != lastDatadWidth * lastDatadHeight * lastDataMaterialLayers)) {
				lastDatadWidth = 0;
				lastDatadHeight = 0;
				lastDataMaterialLayers = 0;
			}
			if (debugMessages) Debug.Log("Terrain2D: checkInstance: " + gameObject.name + ": creating new density data");
			float[] datad1 = new float[ddWidth * ddHeight * dataMaterialLayers];
			for (int i = 0; i < ddWidth; ++i) for (int j = 0; j < ddHeight; ++j) for (int n = 0; n < dataMaterialLayers; ++n) {
				if ((i < lastDatadWidth) && (j < lastDatadHeight) && (n < lastDataMaterialLayers)) datad1[i + j * ddWidth + n * ddWidth * ddHeight] = datad[i + j * lastDatadWidth + n * lastDatadWidth * lastDatadHeight];
				else datad1[i + j * ddWidth + n * ddWidth * ddHeight] = layerDefaultFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
			datad = datad1;
			lastDatadWidth = ddWidth;
			lastDatadHeight = ddHeight;
			lastDataMaterialLayers = dataMaterialLayers;
		}
	}

	int waterCheckInstance_count = 0;
	void waterCheckInstance() {
		if (layerWaterReference != null) {
			scale.x = layerWaterReference.scale.x * layerWaterReference.dataWidth / dataWidth;
			scale.y = layerWaterReference.scale.y * layerWaterReference.dataHeight / dataHeight;
			if (blocks != null) {
				for (int i = 0; i < blocksWidth; ++i) for (int j = 0; j < blocksHeight; ++j) {
					blocks[i + j * blocksWidth].gameObject.transform.localPosition = new Vector3(i * scale.x * dataPerBlockHorizontal, j * scale.y * dataPerBlockVertical, 0.0f);
					blocks[i + j * blocksWidth].gameObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);
				}
			}
		}
		waterCheckInstance_count = 0;

		if ((data != null) && (waterData != null) && (data.Length == dataWidth * dataHeight) && (waterData.Length == dataWidth * dataHeight)) {
			int bi;
			int bj;
			float zeroThreshold = 0.00001f;
			float oneThreshold = 0.99999f;
			for (int i = 0; i < dataWidth; ++i) for (int j = 0; j < dataHeight; ++j) {
				bi = Mathf.FloorToInt(i / dataPerBlockHorizontal);
				bj = Mathf.FloorToInt(j / dataPerBlockHorizontal);
				if (
					(Mathf.Abs(data[i + dataWidth * j] - waterData[i + dataWidth * j]) > waterDataChange) ||
					((Mathf.Abs(data[i + dataWidth * j]) < zeroThreshold) && (Mathf.Abs(waterData[i + dataWidth * j]) > zeroThreshold)) ||
					((Mathf.Abs(data[i + dataWidth * j]) > zeroThreshold) && (Mathf.Abs(waterData[i + dataWidth * j]) < zeroThreshold)) ||
					((Mathf.Abs(data[i + dataWidth * j]) < oneThreshold) && (Mathf.Abs(waterData[i + dataWidth * j]) > oneThreshold)) ||
					((Mathf.Abs(data[i + dataWidth * j]) > oneThreshold) && (Mathf.Abs(waterData[i + dataWidth * j]) < oneThreshold))
				) {
					dirty = true;
					if ((blocks != null) && (bi < blocksWidth) && (bj < blocksHeight)) {
						++waterCheckInstance_count;
						blocks[bi + blocksWidth * bj].dirty = true;
						waterData[i + dataWidth * j] = data[i + dataWidth * j];
					}
				}
			}
		}
	}

	private TerrainShape2D[] internal_outlineShapes = null;
	public void outlineLayerEditorCheck() {
		if ((data != null) && (data.Length == dataWidth * dataHeight) && (lastDataWidth * lastDataHeight == 0)) {
			lastDataWidth = dataWidth;
			lastDataHeight = dataHeight;
		}

		int ddWidth = dataWidth;
		int ddHeight = dataHeight;
		if (dataPerBlockHorizontal > 0f) ddWidth = dataWidth / dataPerBlockHorizontal;
		if (dataPerBlockVertical > 0f) ddHeight = dataHeight / dataPerBlockVertical;
		if ((datad != null) && (datad.Length == ddWidth * ddHeight * dataMaterialLayers) && (lastDatadWidth * lastDatadHeight * lastDataMaterialLayers == 0)) {
			lastDatadWidth = ddWidth;
			lastDatadHeight = ddHeight;
			lastDataMaterialLayers = dataMaterialLayers;
		}
	}
	public void outlineLayer() {
		switch (editorLayer) {
			default: case TerrainLayer2D.master: dataLayer = -1; break;
			case TerrainLayer2D.material1: dataLayer = 0; break;
			case TerrainLayer2D.material2: dataLayer = 1; break;
			case TerrainLayer2D.material3: dataLayer = 2; break;
			case TerrainLayer2D.material4: dataLayer = 3; break;
		}
		enable_internal_draw_li_cache = true;
		TerrainBlock2D block = new TerrainBlock2D();
		block.visible = false;
		block.width = dataWidth;
		block.height = dataHeight;
		array_internal_draw_li_cache1 = null;
		array_internal_draw_li_cache2 = null;
		count_internal_draw_li_cache = 0;
		autoxyz_transform = false;
		autoxyz_right = Vector3.right;
		autoxyz_up = Vector3.up;
		autoxyz_forward = Vector3.forward;
		redraw_blob3(block, false, false, true, false, false, false);
		if ((array_internal_draw_li_cache1 != null) && (array_internal_draw_li_cache2 != null)) {
			int internal_outlineShapesCount = 0;
			for (int i = 0; (i < array_internal_draw_li_cache1.Length) && (i < array_internal_draw_li_cache2.Length) && (i < count_internal_draw_li_cache); ++i) internal_outlineShapesCount = i + 1;
			internal_outlineShapes = new TerrainShape2D[internal_outlineShapesCount];
			for (int i = 0; (i < array_internal_draw_li_cache1.Length) && (i < array_internal_draw_li_cache2.Length) && (i < count_internal_draw_li_cache); ++i) {
				internal_outlineShapes[i] = new TerrainShape2D();
				internal_outlineShapes[i].shape = new Vector3[2];
				internal_outlineShapes[i].shape[0] = gameObject.transform.position + gameObject.transform.right * array_internal_draw_li_cache1[i].x + gameObject.transform.up * array_internal_draw_li_cache1[i].y;
				internal_outlineShapes[i].shape[1] = gameObject.transform.position + gameObject.transform.right * array_internal_draw_li_cache2[i].x + gameObject.transform.up * array_internal_draw_li_cache2[i].y;
				internal_outlineShapes[i].shape2d = new Vector2[2];
				internal_outlineShapes[i].shape2d[0] = array_internal_draw_li_cache1[i];
				internal_outlineShapes[i].shape2d[1] = array_internal_draw_li_cache2[i];
			}
		} else {
			internal_outlineShapes = new TerrainShape2D[0];
		}
		enable_internal_draw_li_cache = false;
		dataLayer = -1;
	}

	public void editOutlineLayerBegin() {
		checkInstanceData();
	}
	public void editOutlineLayerEnd() {
		outlineLayer();
	}
	public void editOutlineLayerRedraw() {
		outlineLayer();
	}
	public void editOutlineLayer(int i, int j, bool shifted, float distance) {
		switch (editorLayer) {
			default: case TerrainLayer2D.master: dataLayer = -1; break;
			case TerrainLayer2D.material1: dataLayer = 0; break;
			case TerrainLayer2D.material2: dataLayer = 1; break;
			case TerrainLayer2D.material3: dataLayer = 2; break;
			case TerrainLayer2D.material4: dataLayer = 3; break;
		}
		if ((data != null) && (data.Length == dataWidth * dataHeight)) {
			TerrainBlock2D block = new TerrainBlock2D();
			block.visible = false;
			block.width = dataWidth;
			block.height = dataHeight;
			editInverse = shifted ? !editInverseDefault : editInverseDefault;
			blockPlot(block, i, j, editorBrushSize, distance, editorBrushType);
		}
		dataLayer = -1;
	}

	public Vector3[] outlineLayerShape(int i, bool recalculate) {
		if ((internal_outlineShapes != null) && (i >= 0) && (i < internal_outlineShapes.Length)) {
			if (recalculate) {
				for (int j = 0; j < internal_outlineShapes[i].shape.Length; ++j) internal_outlineShapes[i].shape[j] = gameObject.transform.position + gameObject.transform.right * internal_outlineShapes[i].shape2d[j].x + gameObject.transform.up * internal_outlineShapes[i].shape2d[j].y;
			}
			return internal_outlineShapes[i].shape;
		}
		return null;
	}
	public Vector3[] outlineLayerShape(int i) {
		return outlineLayerShape(i, true);
	}

	public void saveLayer(string filename) {
		string fichname = null;
		string rowdata;
		int data_count = 0;
		StreamWriter sw = null;
		try {
			try {
				sw = new StreamWriter(fichname = System.IO.Path.Combine(Application.dataPath, filename));
				sw.WriteLine("class=terrain2d");
				sw.WriteLine("version=" + (dataVersion + 1));
				sw.WriteLine("cols=" + dataWidth);
				sw.WriteLine("rows=" + dataHeight);
				for (int j = dataHeight - 1; j >= 0; --j) {
					rowdata = "";
					for (int i = 0; i < dataWidth; ++i) {
						if (rowdata.Length > 0) rowdata += "," + data[i + j * dataWidth];
						else rowdata = "" + data[i + j * dataWidth];
						++data_count;
					}
					sw.WriteLine("row" + j + "=" + rowdata);
				}
				sw.Close();
				Debug.Log(gameObject.name + ": saveLayer: Saved " + data_count + " values to " + fichname);
			} catch (Exception e) {
				sw.Close();
				if (debugMessages) Debug.Log(gameObject.name + ": saveLayer: Could not save file " + fichname + " because " + e.Message);
			}
		} catch (Exception e) {
			if (debugMessages) Debug.Log(gameObject.name + ": saveLayer: Could not save file " + fichname + " because " + e.Message);
		}
	}

	public static int parseInt(string inputText, int defaultValue) {
		int retv;
		if (int.TryParse(inputText, out retv)) {
			return retv;
		}
		return defaultValue;
	}
	public static float parseFloat(string inputText, float defaultValue) {
		float retv;
		if (float.TryParse(inputText, out retv)) {
			return retv;
		}
		return defaultValue;
	}

	private string alreadyLoadedLayer_file = "";
	public bool alreadyLoadedLayer(string filename) {
		if (filename == null) return true;
		if (alreadyLoadedLayer_file.Equals(filename)) {
			return true;
		} else {
			alreadyLoadedLayer_file = filename;
			return false;
		}
	}

	public void dumpLayer() {
		data = null;
		lastDataWidth = 0;
		lastDataHeight = 0;
	}

	private static char[] loadLayerSplit = {','};
	public void loadLayer(string filename) {
		string fichname = null;
		string data_line;
		int data_lines = 0;
		StreamReader sr = null;
		int equpos;
		string keyname;
		string keyvalue;
		string[] keyvalues;
		bool data_class = false;
		int data_row = -1;
		int data_rows = -1;
		int data_cols = -1;
		float data_data = 0f;
		int data_version = -1;
		bool data_version_ok = false;
		int data_count = 0;
		bool data_bounds_checked = false;
		int max_count = 99999;

		try {
			try {
				sr = new StreamReader(fichname = System.IO.Path.Combine(Application.dataPath, filename));
				do {
					--max_count; if (max_count < 0) {
						if (debugMessages) Debug.Log(gameObject.name + ": loadLayer: Could not read from file " + fichname + " and aborting because of an unknown error (after reading " + data_count + " values and " + data_lines + " lines)");
						return;
					}
					data_line = sr.ReadLine();
					++data_lines;
					if (data_line != null) {
						if ((equpos = data_line.IndexOf("=")) >= 0) {
							keyname = data_line.Substring(0, equpos).Trim();
							keyvalue = (equpos < data_line.Length - 1) ? (data_line.Substring(equpos + 1).Trim()) : "";
							if (keyname.Equals("class")) {
								data_class = keyvalue.Equals("terrain2d");
							} else if (data_class) {
								if ((keyname.Length >= 3) && keyname.Substring(0, 3).Equals("row")) {
									data_row = parseInt(keyname.Substring(3), -1);
									if (data_row >= 0) {
										if (data_version_ok) {
											keyvalues = keyvalue.Split(loadLayerSplit);
											if ((keyvalues != null) && (keyvalues.Length >= 0) && (keyvalues.Length <= 4096) && (data_rows >= 0) && (data_cols >= 0)) {
												if (!data_bounds_checked) {
													data_bounds_checked = true;
													dataWidth = data_cols;
													dataHeight = data_rows;
													checkInstanceData();
												}
												if (data_row < dataHeight) {
													for (int i = 0; i < keyvalues.Length; ++i) {
														data_data = parseFloat(keyvalues[i], -1f);
														if (data_data >= 0f) {
															if (i < dataWidth) {
																if ((data != null) && (i + data_row * dataWidth < data.Length)) {
																	data[i + data_row * dataWidth] = data_data;
																	++data_count;
																}
															}
														}
													}
												}
											}
										}
									} else if (keyname.Substring(3).Equals("s")) {
										if (data_version_ok) data_rows = parseInt(keyvalue, -1);
									}
								} else if (keyname.Equals("cols")) {
									if (data_version_ok) data_cols = parseInt(keyvalue, -1);
								} else if (keyname.Equals("id") || keyname.Equals("version")) {
									data_version = parseInt(keyvalue, -1);
									if (data_version >= 0) {
										data_version_ok = (data_version > dataVersion);
										if (data_version_ok) dataVersion = data_version;
										if (!data_version_ok) if (debugMessages) Debug.Log(gameObject.name + ": loadLayer: version is old (object version = " + dataVersion + ", file version = " + data_version + ")");
									}
								} else {
									if (debugMessages) Debug.Log(gameObject.name + ": loadLayer: unknown keyword: '" + keyname + "'");
									sr.Close();
									return;
								}
							}
						}
					}
				} while (data_line != null);
				if (data_version_ok) Debug.Log(gameObject.name + ": loadLayer: Readed file " + fichname + ", " + data_count + " terrain2d values had been loaded.");
				else Debug.Log(gameObject.name + ": loadLayer: The stored file is older than currently loaded, no data has been updated.");
			} catch (Exception e) {
				sr.Close();
				if (debugMessages) Debug.Log(gameObject.name + ": loadLayer: Could not read from file " + fichname + " because " + e.Message);
			}
		} catch (Exception e) {
			if (debugMessages) Debug.Log(gameObject.name + ": loadLayer: Could not read from file " + fichname + " because " + e.Message);
		}
	}

	private float cameraHoldDistance = 50f;
	private Vector3 cameraPosition = Vector3.zero;
	private float cameraPositionFilter = 0.007f;
	void cameraHoldInstance() {
		if (!editorMode) cameraHoldDistance -= Input.GetAxis("Mouse ScrollWheel") * cameraHoldDistance / 100f * 85.0f;
		if (cameraHoldDistance < 0f) cameraHoldDistance = 0f;
		if (cameraHoldDistance > 100f) cameraHoldDistance = 100f;
		if (cameraManagement) if (cameraHoldActive) Camera.mainCamera.transform.position = cameraPosition = cameraPosition * (1f - cameraPositionFilter) + cameraPositionFilter * (gameObject.transform.position - gameObject.transform.forward * cameraHoldDistance + mousePositionX / Screen.width * gameObject.transform.right * lastDataWidth / scale.x + mousePositionY / Screen.height * gameObject.transform.up * lastDataHeight / scale.y);
	}
	private Vector3 objectPosition = Vector3.zero;
	private Vector3 objectPosition1 = Vector3.zero;
	private float objectPositionFilter = 0.05f;
	void objectHoldInstance() {
		Vector3 spos;
		if (pointerPositionInstance(out spos)) {
			objectPosition1 = spos - gameObject.transform.forward + objectHoldDelta;
		}
		objectHold.transform.position = objectPosition = objectPosition * (1f - objectPositionFilter) + objectPositionFilter * objectPosition1;
		Vector3 angles = (objectPosition1 - objectPosition) * 25f;
		objectHold.transform.eulerAngles = new Vector3(-angles.y, angles.x, 0f);
	}

	private float digOrFill(int i, int j, bool querydata, float brushsize, float ammount) {
		if ((i >= 0) && (i < lastDataWidth) && (j >= 0) && (j < lastDataHeight)) {
			int i1 = i / dataPerBlockHorizontal;
			int j1 = j / dataPerBlockVertical;
			if ((i1 >= 0) && (i1 < blocksWidth) && (j1 >= 0) && (j1 < blocksHeight)) {
				i = i - i1 * dataPerBlockHorizontal;
				j = j - j1 * dataPerBlockVertical;
				if (querydata) {
					float retv = blockData(blocks[i1 + j1 * blocksWidth], i, j, 0, 0, 0f);
					return retv;
				}
				return blockPlot(blocks[i1 + j1 * blocksWidth], i, j, brushsize, brushsize, TerrainBrushType2D.brushsize_radius);
			}
		} else {
			if ((i < 0) && (j < 0)) {
				if (neighbourXmYm != null) {
					i += neighbourXmYm.dataWidth;
					j += neighbourXmYm.dataHeight;
					neighbourXmYm.editInverse = editInverse;
					return neighbourXmYm.digOrFill(i, j, querydata, brushsize, ammount);
				}
			} else if ((i < 0) && (j >= dataHeight)) {
				if (neighbourXmYp != null) {
					i += neighbourXmYp.dataWidth;
					j -= dataHeight;
					neighbourXmYp.editInverse = editInverse;
					return neighbourXmYp.digOrFill(i, j, querydata, brushsize, ammount);
				}
			} else if ((i >= dataWidth) && (j < 0)) {
				if (neighbourXpYm != null) {
					i -= dataWidth;
					j += neighbourXpYm.dataHeight;
					neighbourXpYm.editInverse = editInverse;
					return neighbourXpYm.digOrFill(i, j, querydata, brushsize, ammount);
				}
			} else if ((i >= dataWidth) && (j >= dataHeight)) {
				if (neighbourXpYp != null) {
					i -= dataWidth;
					j -= dataHeight;
					neighbourXpYp.editInverse = editInverse;
					return neighbourXpYp.digOrFill(i, j, querydata, brushsize, ammount);
				}
			} else if (i < 0) {
				if (neighbourXm != null) {
					i += neighbourXm.dataWidth;
					neighbourXm.editInverse = editInverse;
					return neighbourXm.digOrFill(i, j, querydata, brushsize, ammount);
				}
			} else if (i >= dataWidth) {
				if (neighbourXp != null) {
					i -= dataWidth;
					neighbourXp.editInverse = editInverse;
					return neighbourXp.digOrFill(i, j, querydata, brushsize, ammount);
				}
			} else if (j < 0) {
				if (neighbourYm != null) {
					j += neighbourYm.dataHeight;
					neighbourYm.editInverse = editInverse;
					return neighbourYm.digOrFill(i, j, querydata, brushsize, ammount);
				}
			} else if (j >= dataHeight) {
				if (neighbourYp != null) {
					j -= dataHeight;
					neighbourYp.editInverse = editInverse;
					return neighbourYp.digOrFill(i, j, querydata, brushsize, ammount);
				}
			}
		}
		return 0f;
	}

	public float query(Vector3 pos) {
		int i, j;
		if (pointerInstance(pos, out i, out j)) {
			return digOrFill(i, j, true, 0f, 0f);
		}
		return 0f;
	}

	public float dig(Vector3 pos, float brushsize, float ammount) {
		int i, j;
		if (pointerInstance(pos, out i, out j)) {
			editInverse = editInverseDefault;
			return digOrFill(i, j, false, brushsize, ammount);
		}
		return 0f;
	}

	public float fill(Vector3 pos, float brushsize, float ammount) {
		int i, j;
		if (pointerInstance(pos, out i, out j)) {
			editInverse = !editInverseDefault;
			return digOrFill(i, j, false, brushsize, ammount);
		}
		return 0f;
	}

	void editInstance() {
		int i, j;
		if (editInRuntimeEnabled && Input_GetKey_editKeyname && Input_GetKey_editKeynameInverse && pointerInstance(out i, out j)) {
			editInverse = !editInverseDefault;
			if ((i >= 0) && (i < lastDataWidth) && (j >= 0) && (j < lastDataHeight)) {
				int i1 = i / dataPerBlockHorizontal;
				int j1 = j / dataPerBlockVertical;
				if ((i1 >= 0) && (i1 < blocksWidth) && (j1 >= 0) && (j1 < blocksHeight)) {
					i = i - i1 * dataPerBlockHorizontal;
					j = j - j1 * dataPerBlockVertical;
					blockPlot(blocks[i1 + j1 * blocksWidth], i, j, editInRuntimeBrushSize, cameraHoldDistance, editInRuntimeBrushType);
				}
			}
		} else if (editInRuntimeEnabled && Input_GetKey_editKeyname && pointerInstance(out i, out j)) {
			editInverse = editInverseDefault;
			if ((i >= 0) && (i < lastDataWidth) && (j >= 0) && (j < lastDataHeight)) {
				int i1 = i / dataPerBlockHorizontal;
				int j1 = j / dataPerBlockVertical;
				if ((i1 >= 0) && (i1 < blocksWidth) && (j1 >= 0) && (j1 < blocksHeight)) {
					i = i - i1 * dataPerBlockHorizontal;
					j = j - j1 * dataPerBlockVertical;
					blockPlot(blocks[i1 + j1 * blocksWidth], i, j, editInRuntimeBrushSize, cameraHoldDistance, editInRuntimeBrushType);
				}
			}
		}
		if (!editorMode) {
			Vector3 spos;
			if ((spawnEnabled && keynamesGetKey(spawnKeynames)) && pointerPositionInstance(out spos)) {
				GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				s.transform.position = spos - gameObject.transform.forward;
				s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				s.AddComponent("Rigidbody");
				Rigidbody r = (Rigidbody)s.GetComponent("Rigidbody");
				r.constraints = RigidbodyConstraints.FreezePositionZ;
			}
		}
	}
	public bool pointerPositionInstance(Ray ray, out Vector3 pos) {
		Plane plane = new Plane(gameObject.transform.forward, gameObject.transform.position);
		float dist;
		bool intersect = plane.Raycast(ray, out dist);
		if (intersect) {
			pos = ray.origin + ray.direction * dist;
			return true;
		} else {
			pos = Vector3.zero;
			return false;
		}
	}
	public bool pointerPositionInstance(out Vector3 pos) {
		Camera camera = Camera.mainCamera;
		Vector2 mousePosition = new Vector2(mousePositionX, mousePositionY);
		Ray ray;
		if (editorMode) ray = editorRay;
		else ray = camera.ScreenPointToRay(mousePosition);
		return pointerPositionInstance(ray, out pos);
	}
	public bool pointerInstance(Vector3 pos, out int i, out int j) {
		pos -= gameObject.transform.position;
		i = Mathf.FloorToInt(Vector3.Dot(pos, gameObject.transform.right) / scale.x);
		j = Mathf.FloorToInt(Vector3.Dot(pos, gameObject.transform.up) / scale.y);
		return true;
	}
	public bool pointerInstance(Ray ray, out int i, out int j) {
		Vector3 pos;
		if (pointerPositionInstance(ray, out pos)) {
			return pointerInstance(pos, out i, out j);
		} else {
			i = -1;
			j = -1;
			return false;
		}
	}
	public bool pointerInstance(out int i, out int j) {
		Camera camera = Camera.mainCamera;
		Vector2 mousePosition = new Vector2(mousePositionX, mousePositionY);
		Ray ray;
		if (editorMode) ray = editorRay;
		else ray = camera.ScreenPointToRay(mousePosition);
		return pointerInstance(ray, out i, out j);
	}

	void redrawInstance() {
		for (int i = 0; i < blocksWidth; ++i) for (int j = 0; j < blocksHeight; ++j) {
			if (blocks[i + j * blocksWidth].dirty) {
				blockRedraw(blocks[i + j * blocksWidth]);
				if ((layerSlave != null) && (layerSlave.blocks != null) && (layerSlave.blocksWidth == blocksWidth) && (blocks.Length == layerSlave.blocks.Length) && (i < layerSlave.blocksWidth) && (j < layerSlave.blocksHeight)) layerSlave.blockRedraw(layerSlave.blocks[i + j * blocksWidth]);
				blocks[i + j * blocksWidth].dirty = false;
			}
		}
		dirty = false;
	}

	static protected int dataLayer = -1;
	float dataRead(int i, int j) {
		if (dataLayer == -1) {
			if ((data != null) && (i >= 0) && (j >= 0) && (i < lastDataWidth) && (j < lastDataHeight)) return data[i + j * lastDataWidth];
		} else {
			if (dataPerBlockHorizontal > 0f) i = i / dataPerBlockHorizontal;
			if (dataPerBlockVertical > 0f) j = j / dataPerBlockVertical;
			if ((datad != null) && (i >= 0) && (j >= 0) && (dataLayer >= 0) && (i < lastDatadWidth) && (j < lastDatadHeight) && (dataLayer < lastDataMaterialLayers)) return datad[i + j * lastDatadWidth + dataLayer * lastDatadWidth * lastDatadHeight];
		}
		return layerDefaultFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
	}
	float dataWrite(int i, int j, float newValue) {
		if (dataLayer == -1) {
			if ((data != null) && (i >= 0) && (j >= 0) && (i < lastDataWidth) && (j < lastDataHeight)) return data[i + j * lastDataWidth] = newValue;
		} else {
			if (dataPerBlockHorizontal > 0f) i = i / dataPerBlockHorizontal;
			if (dataPerBlockVertical > 0f) j = j / dataPerBlockVertical;
			if ((datad != null) && (i >= 0) && (j >= 0) && (dataLayer >= 0) && (i < lastDatadWidth) && (j < lastDatadHeight) && (dataLayer < lastDataMaterialLayers)) return datad[i + j * lastDatadWidth + dataLayer * lastDatadWidth * lastDatadHeight] = newValue;
		}
		return layerDefaultFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
	}

	void blockRedrawBorderRedraw(TerrainBlock2D block) {
		bool outlineExistsConnectedVertex1 = false;
		bool outlineExistsConnectedVertex2 = false;
		Vector2 outlineNormalVertex1;
		Vector2 outlineNormalVertex2;
		float tmp;
		if ((block.outlineConnectedVertex1 != null) && (block.outlineExistsConnectedVertex1 != null) && (block.outlineConnectedVertex1.Length == block.outlineExistsConnectedVertex1.Length) && (block.outlineConnectedVertex1.Length == block.outlineVertex1.Length)) outlineExistsConnectedVertex1 = true;
		if ((block.outlineConnectedVertex2 != null) && (block.outlineExistsConnectedVertex2 != null) && (block.outlineConnectedVertex2.Length == block.outlineExistsConnectedVertex2.Length) && (block.outlineConnectedVertex2.Length == block.outlineVertex2.Length)) outlineExistsConnectedVertex2 = true;
		if ((block.outlineVertex1 != null) && (block.outlineVertex2 != null) && (block.outlineVertex1.Length == block.outlineVertex2.Length)) {
			for (int i = 0; i < block.outlineVertex1.Length; ++i) {

				if (outlineExistsConnectedVertex1) {
					if (block.outlineExistsConnectedVertex1[i]) {
						outlineNormalVertex1 = (block.outlineVertex2[i] - block.outlineConnectedVertex1[i]).normalized;
					} else {
						outlineNormalVertex1 = (block.outlineVertex2[i] - block.outlineVertex1[i]).normalized;
					}
				} else {
					outlineNormalVertex1 = (block.outlineVertex2[i] - block.outlineVertex1[i]).normalized;
				}
				tmp = outlineNormalVertex1.x;
				outlineNormalVertex1.x = -outlineNormalVertex1.y;
				outlineNormalVertex1.y = tmp;

				if (outlineExistsConnectedVertex2) {
					if (block.outlineExistsConnectedVertex2[i]) {
						outlineNormalVertex2 = (block.outlineConnectedVertex2[i] - block.outlineVertex1[i]).normalized;
					} else {
						outlineNormalVertex2 = (block.outlineVertex2[i] - block.outlineVertex1[i]).normalized;
					}
				} else {
					outlineNormalVertex2 = (block.outlineVertex2[i] - block.outlineVertex1[i]).normalized;
				}
				tmp = outlineNormalVertex2.x;
				outlineNormalVertex2.x = -outlineNormalVertex2.y;
				outlineNormalVertex2.y = tmp;

				draw_quad_uvs = true;
				draw_quad_u1 = 0f;
				draw_quad_v1 = 0f;
				draw_quad_u2 = 1f;
				draw_quad_v2 = 0f;
				draw_quad_u3 = 1f;
				draw_quad_v3 = 1f;
				draw_quad_u4 = 0f;
				draw_quad_v4 = 1f;
				draw_quad(block, block.outlineVertex2[i].x, block.outlineVertex2[i].y, block.outlineVertex2[i].x + outlineNormalVertex2.x, block.outlineVertex2[i].y + outlineNormalVertex2.y, block.outlineVertex1[i].x + outlineNormalVertex1.x, block.outlineVertex1[i].y + outlineNormalVertex1.y, block.outlineVertex1[i].x, block.outlineVertex1[i].y, "");
				draw_quad_uvs = false;
			}
		}
	}

	void blockRedrawBorderScan(TerrainBlock2D block, TerrainBlock2D versusblock, bool same) {
		float threshold = 0.01f;
		if ((versusblock.outlineVertex1 != null) && (versusblock.outlineVertex2 != null) && (versusblock.outlineVertex1.Length == versusblock.outlineVertex2.Length)) {
			if ((block.outlineConnectedVertex1 != null) && (block.outlineConnectedVertex2 != null) && (block.outlineConnectedVertex1.Length == block.outlineVertex1.Length) && (block.outlineConnectedVertex1.Length == block.outlineConnectedVertex2.Length)) {
				if ((block.outlineExistsConnectedVertex1 != null) && (block.outlineExistsConnectedVertex2 != null) && (block.outlineExistsConnectedVertex1.Length == block.outlineVertex1.Length) && (block.outlineExistsConnectedVertex1.Length == block.outlineExistsConnectedVertex2.Length)) {
					for (int i = 0; i < block.outlineVertex1.Length; ++i) {
						for (int j = 0; j < versusblock.outlineVertex1.Length; ++j) {
							if (!(same && (i == j))) {
								if ((block.outlineVertex1[i] - versusblock.outlineVertex1[j]).magnitude < threshold) {
									block.outlineConnectedVertex1[i] = versusblock.outlineVertex2[j];
									block.outlineExistsConnectedVertex1[i] = true;
								}
								if ((block.outlineVertex1[i] - versusblock.outlineVertex2[j]).magnitude < threshold) {
									block.outlineConnectedVertex1[i] = versusblock.outlineVertex1[j];
									block.outlineExistsConnectedVertex1[i] = true;
								}
								if ((block.outlineVertex2[i] - versusblock.outlineVertex1[j]).magnitude < threshold) {
									block.outlineConnectedVertex2[i] = versusblock.outlineVertex2[j];
									block.outlineExistsConnectedVertex2[i] = true;
								}
								if ((block.outlineVertex2[i] - versusblock.outlineVertex2[j]).magnitude < threshold) {
									block.outlineConnectedVertex2[i] = versusblock.outlineVertex1[j];
									block.outlineExistsConnectedVertex2[i] = true;
								}
							}
						}
					}
				}
			}
		}
	}

	float blockRedraw_minLapse = 0.2f;
	void blockRedraw(TerrainBlock2D block) {
		if (Time.time < block.lastRedrawTime + blockRedraw_minLapse) return;

		Material useMaterial = material;

		autoxyz_transform = false;
		autoxyz_right = Vector3.right;
		autoxyz_up = Vector3.up;
		autoxyz_forward = Vector3.forward;

		if ((block.mesh = bindMesh(block.gameObject, useMaterial, false)) != null) {
			block.lastRedrawTime = Time.time;
			if (layerRenderBorder) {
				ofsz = zBorderOffset + 0f;
				enable_internal_draw_li_cache = true;
				array_internal_draw_li_cache1 = null;
				array_internal_draw_li_cache2 = null;
				count_internal_draw_li_cache = 0;
				redraw_blob3(block, false, false, true, false, false, false);
				if ((array_internal_draw_li_cache1 != null) && (array_internal_draw_li_cache2 != null)) {
					int j = 0;
					for (int i = 0; (i < array_internal_draw_li_cache1.Length) && (i < array_internal_draw_li_cache2.Length) && (i < count_internal_draw_li_cache); ++i) ++j;
					block.outlineVertex1 = new Vector2[j];
					block.outlineVertex2 = new Vector2[j];
					for (int i = 0; i < j; ++i) {
						block.outlineVertex1[i] = array_internal_draw_li_cache1[i];
						block.outlineVertex2[i] = array_internal_draw_li_cache2[i];
					}
				}
				enable_internal_draw_li_cache = false;
				array_internal_draw_li_cache1 = null;
				array_internal_draw_li_cache2 = null;
				count_internal_draw_li_cache = 0;

				if ((block.outlineVertex1 != null) && (block.outlineVertex2 != null) && (block.outlineVertex1.Length == block.outlineVertex2.Length)) {
					if (block.outlineConnectedVertex1 == null) block.outlineConnectedVertex1 = new Vector2[block.outlineVertex1.Length];
					if (block.outlineConnectedVertex2 == null) block.outlineConnectedVertex2 = new Vector2[block.outlineVertex2.Length];
					if (block.outlineExistsConnectedVertex1 == null) {
						block.outlineExistsConnectedVertex1 = new bool[block.outlineVertex1.Length];
						for (int i = 0; i < block.outlineVertex1.Length; ++i) block.outlineExistsConnectedVertex1[i] = false;
					}
					if (block.outlineExistsConnectedVertex2 == null) {
						block.outlineExistsConnectedVertex2 = new bool[block.outlineVertex2.Length];
						for (int i = 0; i < block.outlineVertex2.Length; ++i) block.outlineExistsConnectedVertex2[i] = false;
					}
					if ((block.outlineConnectedVertex1 != null) && (block.outlineConnectedVertex2 != null) && (block.outlineConnectedVertex1.Length == block.outlineConnectedVertex2.Length)) {
						if ((block.outlineExistsConnectedVertex1 != null) && (block.outlineExistsConnectedVertex2 != null) && (block.outlineExistsConnectedVertex1.Length == block.outlineExistsConnectedVertex2.Length)) {
							if (block.XmYm != null) blockRedrawBorderScan(block, block.XmYm, false);
							if (block.Ym != null) blockRedrawBorderScan(block, block.Ym, false);
							if (block.XpYm != null) blockRedrawBorderScan(block, block.XpYm, false);
							if (block.Xm != null) blockRedrawBorderScan(block, block.Xm, false);
							if (block.Xp != null) blockRedrawBorderScan(block, block.Xp, false);
							if (block.XmYp != null) blockRedrawBorderScan(block, block.XmYp, false);
							if (block.Yp != null) blockRedrawBorderScan(block, block.Yp, false);
							if (block.XpYp != null) blockRedrawBorderScan(block, block.XpYp, false);
							blockRedrawBorderScan(block, block, true);
						}
					}
				}
			}
			meshDrawClear(block.mesh, block.meshMetadata);
			if (layerCollideWalls) meshDrawClear(block.colliderMesh, block.colliderMeshMetadata);
			if (layerRenderEmpty) { ofsz = zEmptyOffset + 0f; redraw_blob3(block, true, false, false, false, false, false); }
			if (layerRenderFilled) { ofsz = zOffset + 0f; redraw_blob3(block, false, true, false, false, false, false); }
			if (layerRenderWalls) { ofsz = zWallsOffset + 0f; ofszw = zWallsWidth; redraw_blob3(block, false, false, true, false, false, false); }
#if COMMON_COMPUTED_BORDERS
			if (layerRenderBorder) { ofsz = zBorderOffset + 0f; ofszw = zBorderWidth; redraw_blob3(block, false, false, true, false, false, true); last_COMMON_COMPUTED_BORDERS = -999999f; }
#else
			if (layerRenderBorder) { ofsz = zBorderOffset + 0f; ofszw = zBorderWidth; internal_draw_li_dry_init(); redraw_blob3(block, false, false, true, false, false, true); internal_draw_li_dry_joinborders(); internal_draw_li_dry_renderborders(); internal_draw_li_dry_release(); }
#endif
			if (layerCollideWalls) { ofsz = zWallsOffset + 0f; ofszw = zWallsWidth; redraw_blob3(block, false, false, true, false, true, false); }
#if TERRAIN_2D_PHYSICS
			if (layerCollideWalls2D) {
				PolygonCollider2D collider2d;
				if (block.gameObject != null) {
					Vector2[] points;

					enable_internal_draw_li_altcache = true;
					count_internal_draw_li_altcache = 0;
					redraw_blob3(block, false, false, true, false, false, false);
					if ((array_internal_draw_li_altcache1 != null) && (array_internal_draw_li_altcache2 != null)) {
						int internal_outlineShapesCount = 0;
						for (int i = 0; (i < array_internal_draw_li_altcache1.Length) && (i < array_internal_draw_li_altcache2.Length) && (i < count_internal_draw_li_altcache); ++i) internal_outlineShapesCount = i + 1;

						if (internal_outlineShapesCount > 0) {
							if ((collider2d = (PolygonCollider2D)block.gameObject.GetComponent("PolygonCollider2D")) == null) block.gameObject.AddComponent("PolygonCollider2D");
							if ((collider2d != null) || ((collider2d = (PolygonCollider2D)block.gameObject.GetComponent("PolygonCollider2D")) != null)) {
								collider2d.pathCount = internal_outlineShapesCount;
								points = new Vector2[4];
								for (int i = 0; i < internal_outlineShapesCount; ++i) {
									points[0] = new Vector2(array_internal_draw_li_altcache1[i].x, array_internal_draw_li_altcache1[i].y);
									points[1] = new Vector2((array_internal_draw_li_altcache1[i].x + array_internal_draw_li_altcache2[i].x) * 0.5f - (array_internal_draw_li_altcache2[i].y - array_internal_draw_li_altcache1[i].y) * 0.01f, (array_internal_draw_li_altcache1[i].y + array_internal_draw_li_altcache2[i].y) * 0.5f + (array_internal_draw_li_altcache2[i].x - array_internal_draw_li_altcache1[i].x) * 0.01f);
									points[2] = new Vector2(array_internal_draw_li_altcache2[i].x, array_internal_draw_li_altcache2[i].y);
									points[3] = new Vector2(array_internal_draw_li_altcache1[i].x, array_internal_draw_li_altcache1[i].y);
									collider2d.SetPath(i, points);
								}
							}
						}
					}
					enable_internal_draw_li_altcache = false;
				}
			}
#endif
			if (layerRenderWater) { ofsz = zOffset + 0f; redraw_blob1(block, true, false, false, false, false); }
#if !COMMON_COMPUTED_BORDERS || COMMON_RENDERED_BORDERS
			meshImmediate(block.mesh, block.meshMetadata, true, true, true, true);
			if (layerCollideWalls) {
				meshImmediate(block.colliderMesh, block.colliderMeshMetadata, true, true, true, true);
				bindMeshCollider(block.gameObject, block.colliderMesh);
			}
			block.visible = true;
#endif
		}
	}

	public TerrainBrushType2D blockPlotBrushtype(float iradius, float idistance, TerrainBrushType2D ibrushtype) {
		switch (ibrushtype) {
			case TerrainBrushType2D.distance_single_or_multi_cell_auto:
				if (idistance >= 50f) ibrushtype = TerrainBrushType2D.multi_cell_soft;
				else if (idistance > 25f) ibrushtype = TerrainBrushType2D.single_cell_soft;
				else ibrushtype = TerrainBrushType2D.single_cell_hard;
				break;
			case TerrainBrushType2D.smooth_distance_single_or_multi_cell_auto:
				if (idistance >= 50f) ibrushtype = TerrainBrushType2D.smooth_multi_cell_soft;
				else if (idistance > 25f) ibrushtype = TerrainBrushType2D.smooth_single_cell_soft;
				else ibrushtype = TerrainBrushType2D.smooth_single_cell_hard;
				break;
		}
		return ibrushtype;
	}
	public float blockPlotBrushsize(float iradius, float idistance, TerrainBrushType2D ibrushtype) {
		ibrushtype = blockPlotBrushtype(iradius, idistance, ibrushtype);
		switch (ibrushtype) {
			case TerrainBrushType2D.brushsize_radius:
			case TerrainBrushType2D.smooth_brushsize_radius:
				return iradius;
			case TerrainBrushType2D.distance_proportional_radius:
			case TerrainBrushType2D.smooth_distance_proportional_radius:
				return idistance * 0.07f * iradius;
			case TerrainBrushType2D.single_cell_hard:
			case TerrainBrushType2D.smooth_single_cell_hard:
				return 1f;
			case TerrainBrushType2D.single_cell_soft:
			case TerrainBrushType2D.smooth_single_cell_soft:
				return 1f;
			case TerrainBrushType2D.multi_cell_soft:
			case TerrainBrushType2D.smooth_multi_cell_soft:
				return 2f;
			default:
				return 1f;
		}
	}
	float blockPlot(TerrainBlock2D block, int i, int j, float iradius, float idistance, TerrainBrushType2D ibrushtype) {
		float drawingrate = 0.5f * UnityEngine.Random.value;
		return blockPlot(block, i, j, drawingrate, iradius, idistance, ibrushtype);
	}
	float blockPlot(TerrainBlock2D block, int i, int j, float irate, float iradius, float idistance, TerrainBrushType2D ibrushtype) {
		float eradius = 0f;
		float erate = 0f;
		float radius = 1f;
		float drawingrate = irate;
		radius = blockPlotBrushsize(iradius, idistance, ibrushtype);
		ibrushtype = blockPlotBrushtype(iradius, idistance, ibrushtype);
		float v, vc;
		switch (ibrushtype) {
			case TerrainBrushType2D.brushsize_radius:
			case TerrainBrushType2D.distance_proportional_radius:
				v = 0; vc = 0;
				for (int jj = -Mathf.CeilToInt(radius); jj <= Mathf.CeilToInt(radius); ++jj) {
					for (int ii = -Mathf.CeilToInt(radius); ii <= Mathf.CeilToInt(radius); ++ii) {
						eradius = Mathf.Sqrt(Mathf.Pow(ii + (UnityEngine.Random.value - 0.5f), 2f) + Mathf.Pow(jj + (UnityEngine.Random.value - 0.5f), 2f)) / radius;
						if (eradius > 1f) eradius = 1f;
						erate = drawingrate * (1f - eradius);
						v += blockData(block, i, j, ii, jj, erate);
						vc += erate;
					}
				}
				return v / vc;
			case TerrainBrushType2D.single_cell_hard:
				drawingrate = 2f;
				return blockData(block, i, j, 0, 0, drawingrate);
			case TerrainBrushType2D.single_cell_soft:
				drawingrate = 0.1f;
				return blockData(block, i, j, 0, 0, drawingrate);
			case TerrainBrushType2D.multi_cell_soft:
				drawingrate = 0.1f;
				v = blockData(block, i, j, 0, 0, drawingrate);
				if (v > 0.33f) {
					blockData(block, i, j, 1, 0, drawingrate * 0.5f);
					blockData(block, i, j, -1, 0, drawingrate * 0.5f);
					blockData(block, i, j, 0, 1, drawingrate * 0.5f);
					blockData(block, i, j, 0, -1, drawingrate * 0.5f);
				}
				if (v > 0.66f) {
					blockData(block, i, j, 1, 1, drawingrate * 0.5f * 0.75f);
					blockData(block, i, j, -1, -1, drawingrate * 0.5f * 0.75f);
					blockData(block, i, j, -1, 1, drawingrate * 0.5f * 0.75f);
					blockData(block, i, j, 1, -1, drawingrate * 0.5f * 0.75f);
				}
				return v;
			case TerrainBrushType2D.smooth_brushsize_radius:
			case TerrainBrushType2D.smooth_distance_proportional_radius:
				v = 0; vc = 0;
				for (int jj = -Mathf.CeilToInt(radius); jj <= Mathf.CeilToInt(radius); ++jj) {
					for (int ii = -Mathf.CeilToInt(radius); ii <= Mathf.CeilToInt(radius); ++ii) {
						eradius = Mathf.Sqrt(Mathf.Pow(ii + (UnityEngine.Random.value - 0.5f), 2f) + Mathf.Pow(jj + (UnityEngine.Random.value - 0.5f), 2f)) / radius;
						if (eradius > 1f) eradius = 1f;
						erate = drawingrate * (1f - eradius);
						v += blockDataSmooth(block, i, j, ii, jj, erate);
						vc += erate;
					}
				}
				return v / vc;
			case TerrainBrushType2D.smooth_single_cell_hard:
				drawingrate = 2f;
				return blockDataSmooth(block, i, j, 0, 0, drawingrate);
			case TerrainBrushType2D.smooth_single_cell_soft:
				drawingrate = 0.1f;
				return blockDataSmooth(block, i, j, 0, 0, drawingrate);
			case TerrainBrushType2D.smooth_multi_cell_soft:
				drawingrate = 0.1f;
				v = blockDataSmooth(block, i, j, 0, 0, drawingrate);
				if (v > 0.33f) {
					blockDataSmooth(block, i, j, 1, 0, drawingrate * 0.5f);
					blockDataSmooth(block, i, j, -1, 0, drawingrate * 0.5f);
					blockDataSmooth(block, i, j, 0, 1, drawingrate * 0.5f);
					blockDataSmooth(block, i, j, 0, -1, drawingrate * 0.5f);
				}
				if (v > 0.66f) {
					blockDataSmooth(block, i, j, 1, 1, drawingrate * 0.5f * 0.75f);
					blockDataSmooth(block, i, j, -1, -1, drawingrate * 0.5f * 0.75f);
					blockDataSmooth(block, i, j, -1, 1, drawingrate * 0.5f * 0.75f);
					blockDataSmooth(block, i, j, 1, -1, drawingrate * 0.5f * 0.75f);
				}
				return v;
		}
		return 0f;
	}

	public bool blockDataAtNeighbour(TerrainBlock2D block, int ii, int jj, int i, int j, float delta, out float value) {
		bool ipending = false;
		bool jpending = false;

		value = 0f;

		if (ii < 0) {
				ipending = true;
		} else if (ii > 0) {
				i -= block.width;
		}
		if (jj < 0) {
				jpending = true;
		} else if (jj > 0) {
				j -= block.height;
		}
		ii += block.i;
		jj += block.j;

		Terrain2D newlayer = null;
		TerrainBlock2D newblock = null;
		if ((ii < 0) && (jj < 0)) {
			if (neighbourXmYm != null) {
				ii += neighbourXmYm.blocksWidth;
				jj += neighbourXmYm.blocksHeight;
				if ((ii + jj * neighbourXmYm.blocksWidth >= 0) && (ii + jj * neighbourXmYm.blocksWidth < neighbourXmYm.blocks.Length)) newblock = neighbourXmYm.blocks[ii + jj * neighbourXmYm.blocksWidth];
				else return false;
				newlayer = neighbourXmYm;
			}
		} else if ((ii < 0) && (jj >= blocksHeight)) {
			if (neighbourXmYp != null) {
				ii += neighbourXmYp.blocksWidth;
				jj -= blocksHeight;
				if ((ii + jj * neighbourXmYp.blocksWidth >= 0) && (ii + jj * neighbourXmYp.blocksWidth < neighbourXmYp.blocks.Length)) newblock = neighbourXmYp.blocks[ii + jj * neighbourXmYp.blocksWidth];
				else return false;
				newlayer = neighbourXmYp;
			}
		} else if ((ii >= blocksWidth) && (jj < 0)) {
			if (neighbourXpYm != null) {
				ii -= blocksWidth;
				jj += neighbourXpYm.blocksHeight;
				if ((ii + jj * neighbourXpYm.blocksWidth >= 0) && (ii + jj * neighbourXpYm.blocksWidth < neighbourXpYm.blocks.Length)) newblock = neighbourXpYm.blocks[ii + jj * neighbourXpYm.blocksWidth];
				else return false;
				newlayer = neighbourXpYm;
			}
		} else if ((ii >= blocksWidth) && (jj >= blocksHeight)) {
			if (neighbourXpYp != null) {
				ii -= blocksWidth;
				jj -= blocksHeight;
				if ((ii + jj * neighbourXpYp.blocksWidth >= 0) && (ii + jj * neighbourXpYp.blocksWidth < neighbourXpYp.blocks.Length)) newblock = neighbourXpYp.blocks[ii + jj * neighbourXpYp.blocksWidth];
				else return false;
				newlayer = neighbourXpYp;
			}
		} else if (ii < 0) {
			if (neighbourXm != null) {
				ii += neighbourXm.blocksWidth;
				if ((ii + jj * neighbourXm.blocksWidth >= 0) && (ii + jj * neighbourXm.blocksWidth < neighbourXm.blocks.Length)) newblock = neighbourXm.blocks[ii + jj * neighbourXm.blocksWidth];
				else return false;
				newlayer = neighbourXm;
			}
		} else if (ii >= blocksWidth) {
			if (neighbourXp != null) {
				ii -= blocksWidth;
				if ((ii + jj * neighbourXp.blocksWidth >= 0) && (ii + jj * neighbourXp.blocksWidth < neighbourXp.blocks.Length)) newblock = neighbourXp.blocks[ii + jj * neighbourXp.blocksWidth];
				else return false;
				newlayer = neighbourXp;
			}
		} else if (jj < 0) {
			if (neighbourYm != null) {
				jj += neighbourYm.blocksHeight;
				if ((ii + jj * neighbourYm.blocksWidth >= 0) && (ii + jj * neighbourYm.blocksWidth < neighbourYm.blocks.Length)) newblock = neighbourYm.blocks[ii + jj * neighbourYm.blocksWidth];
				else return false;
				newlayer = neighbourYm;
			}
		} else if (jj >= blocksHeight) {
			if (neighbourYp != null) {
				jj -= blocksHeight;
				if ((ii + jj * neighbourYp.blocksWidth >= 0) && (ii + jj * neighbourYp.blocksWidth < neighbourYp.blocks.Length)) newblock = neighbourYp.blocks[ii + jj * neighbourYp.blocksWidth];
				else return false;
				newlayer = neighbourYp;
			}
		} else {
			return false;
		}

		if ((newlayer == null) || (newblock == null)) return false;
		if (ipending) i += newblock.width;
		if (jpending) j += newblock.height;
		value = newlayer.blockData(newblock, i, j, delta);
		return true;
	}

	public float blockDataSmooth(TerrainBlock2D block, int i, int j, int di, int dj, float rate) {
		return blockDataSmooth(block, i + di, j + dj, rate, false);
	}
	public float blockDataSmooth(TerrainBlock2D block, int i, int j, float delta) {
		return blockDataSmooth(block, i, j, delta, false);
	}
	public float blockDataSmooth(TerrainBlock2D block, int i, int j, float delta, bool noDirtyMark) {
		float v00 = blockData(block, i, j, 0f, true);
		float vM0 = blockData(block, i + 1, j, 0f, true);
		float v0M = blockData(block, i, j + 1, 0f, true);
		float vm0 = blockData(block, i - 1, j, 0f, true);
		float v0m = blockData(block, i, j - 1, 0f, true);
		blockData(block, i + 1, j, (v00 - vM0) * delta * 0.1f * 0.25f, noDirtyMark);
		blockData(block, i, j + 1, (v00 - v0M) * delta * 0.1f * 0.25f, noDirtyMark);
		blockData(block, i - 1, j, (v00 - vm0) * delta * 0.1f * 0.25f, noDirtyMark);
		blockData(block, i, j - 1, (v00 - v0m) * delta * 0.1f * 0.25f, noDirtyMark);
		return blockData(block, i, j, ((vM0 + v0M + vm0 + v0m) - v00 * 4f) * delta * 0.1f * 0.25f, noDirtyMark);
	}

	public float blockData(TerrainBlock2D block, int i, int j, int di, int dj, float rate) {
		return blockData(block, i + di, j + dj, rate, false);
	}
	public float blockData(TerrainBlock2D block, int i, int j, float delta) {
		return blockData(block, i, j, delta, false);
	}
	public float blockData(TerrainBlock2D block, int i, int j, float delta, bool noDirtyMark) {
		float retv;
		int blockofs;
		if (block == null) return 0f;
		if ((i >= 0) && (j >= 0) && (i < block.width) && (j < block.height)) {
			float valueC = dataRead(block.left + i, block.top + j);
			bool filling = !editInverse;
			if (filling) valueC += delta;
			else valueC -= delta;
			if ((delta > 0.001f) || (delta < -0.001f)) {
				if (!noDirtyMark) {
					dirty = true;
					block.dirty = true;
					if ((i <= 0) && (j <= 0)) {
						if (block.XmYm != null) block.XmYm.dirty = true;
						if (block.Xm != null) block.Xm.dirty = true;
						if (block.Ym != null) block.Ym.dirty = true;
					} else if ((i >= block.width - 1) && (j <= 0)) {
						if (block.XpYm != null) block.XpYm.dirty = true;
						if (block.Xp != null) block.Xp.dirty = true;
						if (block.Ym != null) block.Ym.dirty = true;
					} else if ((i <= 0) && (j >= block.height - 1)) {
						if (block.XmYp != null) block.XmYp.dirty = true;
						if (block.Xm != null) block.Xm.dirty = true;
						if (block.Yp != null) block.Yp.dirty = true;
					} else if ((i >= block.width - 1) && (j >= block.height - 1)) {
						if (block.XpYp != null) block.XpYp.dirty = true;
						if (block.Xp != null) block.Xp.dirty = true;
						if (block.Yp != null) block.Yp.dirty = true;
					}
					if (i <= 0) {
						if (block.Xm != null) block.Xm.dirty = true;
					} else if (i >= block.width - 1) {
						if (block.Xp != null) block.Xp.dirty = true;
					}
					if (j <= 0) {
						if (block.Ym != null) block.Ym.dirty = true;
					} else if (j >= block.height - 1) {
						if (block.Yp != null) block.Yp.dirty = true;
					}
				}
			}
			if (valueC < 0f) valueC = 0f;
			if (valueC > 1f) valueC = 1f;
			return dataWrite(block.left + i, block.top + j, valueC);
		} else if ((i < 0) && (j < 0)) {
			if (block.XmYm != null) {
				i += block.XmYm.width;
				j += block.XmYm.height;
				return blockData(block.XmYm, i, j, delta);
			} else if (blockDataAtNeighbour(block, -1, -1, i, j, delta, out retv)) {
				return retv;
			} else {
				return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
		} else if ((i >= block.width) && (j < 0)) {
			if (block.XpYm != null) {
				i -= block.width;
				j += block.XpYm.height;
				return blockData(block.XpYm, i, j, delta);
			} else {
				return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
		} else if ((i < 0) && (j >= block.height)) {
			if (block.XmYp != null) {
				i += block.XmYp.width;
				j -= block.height;
				return blockData(block.XmYp, i, j, delta);
			} else {
				return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
		} else if ((i >= block.width) && (j >= block.height)) {
			if (block.XpYp != null) {
				i -= block.width;
				j -= block.height;
				return blockData(block.XpYp, i, j, delta);
			} else {
				return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
		} else if ((j < 0)) {
			if (block.Ym != null) {
				j += block.Ym.height;
				return blockData(block.Ym, i, j, delta);
			} else if ((neighbourYm != null) && (neighbourYm.blocksWidth == blocksWidth) && (neighbourYm.blocksHeight == blocksHeight)) {
				blockofs = block.i + (neighbourYm.blocksHeight - 1) * neighbourYm.blocksWidth;
				if ((blockofs >= 0) && (blockofs < neighbourYm.blocks.Length)) return neighbourYm.blockData(neighbourYm.blocks[blockofs], i, j + neighbourYm.blocks[blockofs].height, delta);
				else return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			} else {
				return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
		} else if ((i < 0)) {
			if (block.Xm != null) {
				i += block.Xm.width;
				return blockData(block.Xm, i, j, delta);
			} else if ((neighbourXm != null) && (neighbourXm.blocksWidth == blocksWidth) && (neighbourXm.blocksHeight == blocksHeight)) {
				blockofs = (neighbourXm.blocksWidth - 1) + block.j * neighbourXm.blocksWidth;
				if ((blockofs >= 0) && (blockofs < neighbourXm.blocks.Length)) return neighbourXm.blockData(neighbourXm.blocks[blockofs], i + neighbourXm.blocks[blockofs].width, j, delta);
				else return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			} else {
				return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
		} else if ((j >= block.height)) {
			if (block.Yp != null) {
				j -= block.height;
				return blockData(block.Yp, i, j, delta);
			} else if ((neighbourYp != null) && (neighbourYp.blocksWidth == blocksWidth) && (neighbourYp.blocksHeight == blocksHeight)) {
				blockofs = block.i + 0 * neighbourYp.blocksWidth;
				if ((blockofs >= 0) && (blockofs < neighbourYp.blocks.Length)) return neighbourYp.blockData(neighbourYp.blocks[blockofs], i, j - block.height, delta);
				else return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			} else {
				return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
		} else if ((i >= block.width)) {
			if (block.Xp != null) {
				i -= block.width;
				return blockData(block.Xp, i, j, delta);
			} else if ((neighbourXp != null) && (neighbourXp.blocksWidth == blocksWidth) && (neighbourXp.blocksHeight == blocksHeight)) {
				blockofs = 0 + block.j * neighbourXp.blocksWidth;
				if ((blockofs >= 0) && (blockofs < neighbourXp.blocks.Length)) return neighbourXp.blockData(neighbourXp.blocks[blockofs], i - block.width, j, delta);
				else return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			} else {
				return layerDefaultOutsideFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
			}
		}
		return layerDefaultFilled ? DEFAULT_DATA : (1f - DEFAULT_DATA);
	}

	bool uno = true, dos = true, tres = true, cuatro = true, cinco = true, seis = true, siete = true, ocho = true, nueve = true, cero = true;
	protected void debugflags() {
		bool unop = Input.GetKeyDown(KeyCode.Alpha1);
		bool dosp = Input.GetKeyDown(KeyCode.Alpha2);
		bool tresp = Input.GetKeyDown(KeyCode.Alpha3);
		bool cuatrop = Input.GetKeyDown(KeyCode.Alpha4);
		bool cincop = Input.GetKeyDown(KeyCode.Alpha5);
		bool seisp = Input.GetKeyDown(KeyCode.Alpha6);
		bool sietep = Input.GetKeyDown(KeyCode.Alpha7);
		bool ochop = Input.GetKeyDown(KeyCode.Alpha8);
		bool nuevep = Input.GetKeyDown(KeyCode.Alpha9);
		bool cerop = Input.GetKeyDown(KeyCode.Alpha0);
		if (unop) uno = !uno;
		if (dosp) dos = !dos;
		if (tresp) tres = !tres;
		if (cuatrop) cuatro = !cuatro;
		if (cincop) cinco = !cinco;
		if (seisp) seis = !seis;
		if (sietep) siete = !siete;
		if (ochop) ocho = !ocho;
		if (nuevep) nueve = !nueve;
		if (cerop) cero = !cero;
		if (unop || dosp || tresp || cuatrop) Debug.Log("redraw with uno = " + uno + " y dos = " + dos + " y tres = " + tres + " y cuatro = " + cuatro);
	}

	protected float internal_tiling_set_x_bias = 0f;
	protected float internal_tiling_set_y_bias = 0f;
	protected float internal_tiling_set_z_bias = 0f;
	protected float internal_tiling_set_x = 1f;
	protected float internal_tiling_set_y = 1f;
	protected float internal_tiling_set_z = 1f;
	protected void internal_tiling_set(TerrainBlock2D block) {
		if ((block != null) && (blocksWidth > 0) && (blocksHeight > 0)) {
			autouv_x_bias = 0.0f + block.i * (dataWidth / blocksWidth);
			autouv_y_bias = 0.0f + block.j * (dataHeight / blocksHeight);
			autouv_z_bias = 0.0f;
			autouv_x = 1.0f;
			autouv_y = 1.0f;
			autouv_z = 1.0f;
		} else {
			autouv_x_bias = 0.0f;
			autouv_y_bias = 0.0f;
			autouv_z_bias = 0.0f;
			autouv_x = 1.0f;
			autouv_y = 1.0f;
			autouv_z = 1.0f;
		}
		autouv_x_bias = autouv_x_bias + internal_tiling_set_x_bias;
		autouv_y_bias = autouv_y_bias + internal_tiling_set_y_bias;
		autouv_z_bias = autouv_z_bias + internal_tiling_set_z_bias;
		autouv_x = autouv_x * internal_tiling_set_x;
		autouv_y = autouv_y * internal_tiling_set_y;
		autouv_z = autouv_z * internal_tiling_set_z;
	}

	[System.NonSerialized]public bool enable_internal_draw_li_cache = false;
	[System.NonSerialized]public Vector2[] array_internal_draw_li_cache1 = null;
	[System.NonSerialized]public Vector2[] array_internal_draw_li_cache2 = null;
	[System.NonSerialized]public int count_internal_draw_li_cache = 0;
	[System.NonSerialized]private int maxcount_internal_draw_li_cache = 9999;

	[System.NonSerialized]public bool enable_internal_draw_li_altcache = false;
	[System.NonSerialized]public Vector2[] array_internal_draw_li_altcache1 = null;
	[System.NonSerialized]public Vector2[] array_internal_draw_li_altcache2 = null;
	[System.NonSerialized]public int count_internal_draw_li_altcache = 0;
	[System.NonSerialized]private int maxcount_internal_draw_li_altcache = 9999;
	bool draw_li_with_width = false;
	float draw_li_width = 0.15f;
	protected bool internal_draw_li_dry = false;
	protected TerrainBlock2D[] internal_draw_li_dry_block = null;
	protected float[] internal_draw_li_dry_x1 = null;
	protected float[] internal_draw_li_dry_y1 = null;
	protected float[] internal_draw_li_dry_x2 = null;
	protected float[] internal_draw_li_dry_y2 = null;
	protected int[] internal_draw_li_dry_link11 = null;
	protected int[] internal_draw_li_dry_link12 = null;
	protected int[] internal_draw_li_dry_link21 = null;
	protected int[] internal_draw_li_dry_link22 = null;
	protected int internal_draw_li_dry_count = -9999;
	protected TerrainBlock2D[] internal_draw_li_dry_blocks = null;
	protected int internal_draw_li_dry_blocks_count = -9999;
	protected int internal_draw_li_dry_maxcount = 9999;
	protected bool internal_draw_li_dry_renderborders_join = true;
	protected float internal_draw_li_dry_joinborders_threshold = 0.05f;
	protected void internal_draw_li_dry_init() {
		if (internal_draw_li_dry_block == null) internal_draw_li_dry_block = new TerrainBlock2D[internal_draw_li_dry_maxcount];
		if (internal_draw_li_dry_x1 == null) internal_draw_li_dry_x1 = new float[internal_draw_li_dry_maxcount];
		if (internal_draw_li_dry_y1 == null) internal_draw_li_dry_y1 = new float[internal_draw_li_dry_maxcount];
		if (internal_draw_li_dry_x2 == null) internal_draw_li_dry_x2 = new float[internal_draw_li_dry_maxcount];
		if (internal_draw_li_dry_y2 == null) internal_draw_li_dry_y2 = new float[internal_draw_li_dry_maxcount];
		if (internal_draw_li_dry_link11 == null) internal_draw_li_dry_link11 = new int[internal_draw_li_dry_maxcount];
		if (internal_draw_li_dry_link12 == null) internal_draw_li_dry_link12 = new int[internal_draw_li_dry_maxcount];
		if (internal_draw_li_dry_link21 == null) internal_draw_li_dry_link21 = new int[internal_draw_li_dry_maxcount];
		if (internal_draw_li_dry_link22 == null) internal_draw_li_dry_link22 = new int[internal_draw_li_dry_maxcount];
		internal_draw_li_dry_count = 0;
		if (internal_draw_li_dry_blocks == null) internal_draw_li_dry_blocks = new TerrainBlock2D[internal_draw_li_dry_maxcount];
		internal_draw_li_dry_blocks_count = 0;
	}
	protected bool internal_draw_li_dry_add(TerrainBlock2D block, float x1, float y1, float x2, float y2, bool autobias) {
		float biasx, biasy;
		if (internal_draw_li_dry_count < internal_draw_li_dry_maxcount) {
			internal_draw_li_dry_block[internal_draw_li_dry_count] = block;
			if (autobias) {
				biasx = block.i * scale.x * dataPerBlockHorizontal;
				biasy = block.j * scale.y * dataPerBlockVertical;
			} else {
				biasx = 0f;
				biasy = 0f;
			}
			internal_draw_li_dry_x1[internal_draw_li_dry_count] = x1 + biasx;
			internal_draw_li_dry_y1[internal_draw_li_dry_count] = y1 + biasy;
			internal_draw_li_dry_x2[internal_draw_li_dry_count] = x2 + biasx;
			internal_draw_li_dry_y2[internal_draw_li_dry_count] = y2 + biasy;
			internal_draw_li_dry_link11[internal_draw_li_dry_count] = -1;
			internal_draw_li_dry_link12[internal_draw_li_dry_count] = -1;
			internal_draw_li_dry_link21[internal_draw_li_dry_count] = -1;
			internal_draw_li_dry_link22[internal_draw_li_dry_count] = -1;
			++internal_draw_li_dry_count;
			return true;
		}
		return false;
	}
	protected void internal_draw_li_dry_blocks_init() {
		if (internal_draw_li_dry_blocks == null) internal_draw_li_dry_blocks = new TerrainBlock2D[internal_draw_li_dry_maxcount];
		internal_draw_li_dry_blocks_count = 0;
	}
	protected void internal_draw_li_dry_blocks_push(TerrainBlock2D block) {
		internal_draw_li_dry_blocks[internal_draw_li_dry_blocks_count++] = block;
	}
	protected void internal_draw_li_dry_blocks_renderborders() {
		for (int i = 0; i < internal_draw_li_dry_blocks_count; ++i) {
			internal_draw_li_dry_renderborders(internal_draw_li_dry_blocks[i]);
		}
	}
	protected void internal_draw_li_dry_blocks_release() {
		for (int i = 0; i < internal_draw_li_dry_blocks_count; ++i) internal_draw_li_dry_blocks[i] = null;
		internal_draw_li_dry_blocks_count = 0;
	}
	protected void internal_draw_li_dry_joinborders() {
		for (int i = 0; i < internal_draw_li_dry_count; ++i) {
			if (internal_draw_li_dry_block[i] == null) {
				internal_draw_li_dry_link11[i] = -1;
				internal_draw_li_dry_link12[i] = -1;
				internal_draw_li_dry_link21[i] = -1;
				internal_draw_li_dry_link22[i] = -1;
			} else {
				for (int j = 0; j < internal_draw_li_dry_count; ++j) {
					if ((i != j) && (internal_draw_li_dry_block[i] != null) && (internal_draw_li_dry_block[j] != null)) {
						// find 1 matches (primary is i)
						if ((Mathf.Abs(internal_draw_li_dry_x1[i] - internal_draw_li_dry_x1[j]) < internal_draw_li_dry_joinborders_threshold) && (Mathf.Abs(internal_draw_li_dry_y1[i] - internal_draw_li_dry_y1[j]) < internal_draw_li_dry_joinborders_threshold)) {
							internal_draw_li_dry_link11[i] = j;
							internal_draw_li_dry_link12[i] = -1;
							internal_draw_li_dry_link11[j] = i;
							internal_draw_li_dry_link12[j] = -1;
						} else if ((Mathf.Abs(internal_draw_li_dry_x1[i] - internal_draw_li_dry_x2[j]) < internal_draw_li_dry_joinborders_threshold) && (Mathf.Abs(internal_draw_li_dry_y1[i] - internal_draw_li_dry_y2[j]) < internal_draw_li_dry_joinborders_threshold)) {
							internal_draw_li_dry_link11[i] = -1;
							internal_draw_li_dry_link12[i] = j;
							internal_draw_li_dry_link21[j] = i;
							internal_draw_li_dry_link22[j] = -1;
						}
						// find 2 matches (primary is i)
						if ((Mathf.Abs(internal_draw_li_dry_x2[i] - internal_draw_li_dry_x2[j]) < internal_draw_li_dry_joinborders_threshold) && (Mathf.Abs(internal_draw_li_dry_y2[i] - internal_draw_li_dry_y2[j]) < internal_draw_li_dry_joinborders_threshold)) {
							internal_draw_li_dry_link21[i] = -1;
							internal_draw_li_dry_link22[i] = j;
							internal_draw_li_dry_link21[j] = -1;
							internal_draw_li_dry_link22[j] = i;
						} else if ((Mathf.Abs(internal_draw_li_dry_x2[i] - internal_draw_li_dry_x1[j]) < internal_draw_li_dry_joinborders_threshold) && (Mathf.Abs(internal_draw_li_dry_y2[i] - internal_draw_li_dry_y1[j]) < internal_draw_li_dry_joinborders_threshold)) {
							internal_draw_li_dry_link21[i] = j;
							internal_draw_li_dry_link22[i] = -1;
							internal_draw_li_dry_link11[j] = -1;
							internal_draw_li_dry_link12[j] = i;
						}
					}
				}
			}
		}
	}
	protected int internal_draw_li_dry_renderborders_case(float x1, float y1, float x2, float y2, out float deltax, out float deltay) {
		float magnitude;
		if (Mathf.Abs(x1 - x2) > Mathf.Abs(y1 - y2)) {
			if (x1 < x2) {
				deltax = y1 - y2;
				deltay = x2 - x1;
				magnitude = Mathf.Sqrt(deltax * deltax + deltay * deltay);
				if (magnitude > 0f) {
					deltax = deltax / magnitude;
					deltay = deltay / magnitude;
				}
				return 1;
			} else if (x1 > x2) {
				deltax = y1 - y2;
				deltay = x2 - x1;
				magnitude = Mathf.Sqrt(deltax * deltax + deltay * deltay);
				if (magnitude > 0f) {
					deltax = deltax / magnitude;
					deltay = deltay / magnitude;
				}
				return 2;
			}
		} else {
			if (y1 < y2) {
				deltax = y1 - y2;
				deltay = x2 - x1;
				magnitude = Mathf.Sqrt(deltax * deltax + deltay * deltay);
				if (magnitude > 0f) {
					deltax = deltax / magnitude;
					deltay = deltay / magnitude;
				}
				return 3;
			} else if (y1 > y2) {
				deltax = y1 - y2;
				deltay = x2 - x1;
				magnitude = Mathf.Sqrt(deltax * deltax + deltay * deltay);
				if (magnitude > 0f) {
					deltax = deltax / magnitude;
					deltay = deltay / magnitude;
				}
				return 4;
			}
		}
		deltax = 0f;
		deltay = 0f;
		return 0;
	}
	protected void internal_draw_li_dry_renderborders() {
		internal_draw_li_dry_renderborders(null);
	}
	protected void internal_draw_li_dry_renderborders(TerrainBlock2D block) {
		internal_draw_li_dry_renderborders(block, block);
	}
	protected void internal_draw_li_dry_renderborders(TerrainBlock2D block, TerrainBlock2D inblock) {
		internal_draw_li_dry_renderborders(block, inblock, 0f, 0f, 0f);
	}
	protected void internal_draw_li_dry_renderborders(TerrainBlock2D block, TerrainBlock2D inblock, bool auto) {
		internal_draw_li_dry_renderborders(block, inblock, 0f, 0f, 0f, auto);
	}
	protected void internal_draw_li_dry_renderborders(TerrainBlock2D block, TerrainBlock2D inblock, float dx, float dy, float dz) {
		internal_draw_li_dry_renderborders(block, inblock, dx, dy, dz, false);
	}
	protected void internal_draw_li_dry_renderborders(TerrainBlock2D block, TerrainBlock2D inblock, float dx, float dy, float dz, bool auto) {
		float xtype1;
		float xtype2;
		float xtyper;
		float ytype1;
		float ytype2;
		float ytyper;
		int j;

		if (block != null) internal_tiling_set(block);
		for (int i = 0; i < internal_draw_li_dry_count; ++i) if ((inblock == null) || (inblock == internal_draw_li_dry_block[i])) {
			if (block == null) internal_tiling_set(internal_draw_li_dry_block[i]);

			internal_draw_li_dry_renderborders_case(internal_draw_li_dry_x1[i], internal_draw_li_dry_y1[i], internal_draw_li_dry_x2[i], internal_draw_li_dry_y2[i], out xtype1, out ytype1);
			xtype2 = xtype1;
			ytype2 = ytype1;

			if (internal_draw_li_dry_renderborders_join) {
				if ((j = internal_draw_li_dry_link11[i]) >= 0) {
					internal_draw_li_dry_renderborders_case(internal_draw_li_dry_x1[j], internal_draw_li_dry_y1[j], internal_draw_li_dry_x2[j], internal_draw_li_dry_y2[j], out xtyper, out ytyper);
					xtype1 = (xtype1 + xtyper) * 0.5f;
					ytype1 = (ytype1 + ytyper) * 0.5f;
				} else if ((j = internal_draw_li_dry_link12[i]) >= 0) {
					internal_draw_li_dry_renderborders_case(internal_draw_li_dry_x1[j], internal_draw_li_dry_y1[j], internal_draw_li_dry_x2[j], internal_draw_li_dry_y2[j], out xtyper, out ytyper);
					xtype1 = (xtype1 + xtyper) * 0.5f;
					ytype1 = (ytype1 + ytyper) * 0.5f;
				} else {
					xtype1 = 0f;
					ytype1 = 0f;
				}
				if ((j = internal_draw_li_dry_link21[i]) >= 0) {
					internal_draw_li_dry_renderborders_case(internal_draw_li_dry_x1[j], internal_draw_li_dry_y1[j], internal_draw_li_dry_x2[j], internal_draw_li_dry_y2[j], out xtyper, out ytyper);
					xtype2 = (xtype2 + xtyper) * 0.5f;
					ytype2 = (ytype2 + ytyper) * 0.5f;
				} else if ((j = internal_draw_li_dry_link22[i]) >= 0) {
					internal_draw_li_dry_renderborders_case(internal_draw_li_dry_x1[j], internal_draw_li_dry_y1[j], internal_draw_li_dry_x2[j], internal_draw_li_dry_y2[j], out xtyper, out ytyper);
					xtype2 = (xtype2 + xtyper) * 0.5f;
					ytype2 = (ytype2 + ytyper) * 0.5f;
				} else {
					xtype2 = 0f;
					ytype2 = 0f;
				}
			}

			xtype1 *= ofszw;
			xtype2 *= ofszw;
			ytype1 *= ofszw;
			ytype2 *= ofszw;

			if (auto) {
				if (inblock == null) {
					dx = internal_draw_li_dry_block[i].i * scale.x * dataPerBlockHorizontal;
					dy = internal_draw_li_dry_block[i].j * scale.y * dataPerBlockVertical;
					dz = 0f;
				} else {
					dx = inblock.i * scale.x * dataPerBlockHorizontal;
					dy = inblock.j * scale.y * dataPerBlockVertical;
					dz = 0f;
				}
			}

			meshDrawQuadAutoNormalReverse(
				(block == null) ? internal_draw_li_dry_block[i].mesh : block.mesh,
				(block == null) ? internal_draw_li_dry_block[i].meshMetadata : block.meshMetadata,
				new Vector3(internal_draw_li_dry_x1[i] + dx, internal_draw_li_dry_y1[i] + dy, 0f + ofsz + dz),
				new Vector3(internal_draw_li_dry_x2[i] + dx, internal_draw_li_dry_y2[i] + dy, 0f + ofsz + dz),
				new Vector3(internal_draw_li_dry_x2[i] + xtype2 + dx, internal_draw_li_dry_y2[i] + ytype2 + dy, 0f + ofsz + dz),
				new Vector3(internal_draw_li_dry_x1[i] + xtype1 + dx, internal_draw_li_dry_y1[i] + ytype1 + dy, 0f + ofsz + dz),
				DEFAULT_THRESHOLD, false);
		}
	}
	protected void internal_draw_li_dry_compact() {
		int j = 0;
		for (int i = 0; i < internal_draw_li_dry_count; ++i) {
			if (internal_draw_li_dry_block[i] != null) {
				if (i > j) {
					internal_draw_li_dry_x1[j] = internal_draw_li_dry_x1[i];
					internal_draw_li_dry_y1[j] = internal_draw_li_dry_y1[i];
					internal_draw_li_dry_x2[j] = internal_draw_li_dry_x2[i];
					internal_draw_li_dry_y2[j] = internal_draw_li_dry_y2[i];

					if (internal_draw_li_dry_link11[i] >= 0) internal_draw_li_dry_link11[internal_draw_li_dry_link11[i]] = j;
					internal_draw_li_dry_link11[j] = internal_draw_li_dry_link11[i];
					if (internal_draw_li_dry_link12[i] >= 0) internal_draw_li_dry_link21[internal_draw_li_dry_link12[i]] = j;
					internal_draw_li_dry_link12[j] = internal_draw_li_dry_link12[i];
					if (internal_draw_li_dry_link21[i] >= 0) internal_draw_li_dry_link12[internal_draw_li_dry_link21[i]] = j;
					internal_draw_li_dry_link21[j] = internal_draw_li_dry_link21[i];
					if (internal_draw_li_dry_link22[i] >= 0) internal_draw_li_dry_link22[internal_draw_li_dry_link22[i]] = j;
					internal_draw_li_dry_link22[j] = internal_draw_li_dry_link22[i];

					internal_draw_li_dry_block[j] = internal_draw_li_dry_block[i];
					internal_draw_li_dry_block[i] = null;
				}
				++j;
			}
		}
		internal_draw_li_dry_count = j;
	}
	protected void internal_draw_li_dry_log() {
		for (int i = 0; i < internal_draw_li_dry_count; ++i) if (internal_draw_li_dry_block[i] != null) {
			Debug.Log("internal_draw_li_dry[" + i + "] = block(" + internal_draw_li_dry_block[i].i + ", " + internal_draw_li_dry_block[i].j + ") line(" + internal_draw_li_dry_x1[i] + ", " + internal_draw_li_dry_y1[i] + ") - (" + internal_draw_li_dry_x2[i] + ", " + internal_draw_li_dry_y2[i] + ") join(" + ((internal_draw_li_dry_link11[i] == -1) ? internal_draw_li_dry_link12[i] : internal_draw_li_dry_link11[i]) + ", " + ((internal_draw_li_dry_link21[i] == -1) ? internal_draw_li_dry_link22[i] : internal_draw_li_dry_link21[i]) + ")");
		}
	}
	protected void internal_draw_li_dry_delete(TerrainBlock2D block) {
		for (int i = 0; i < internal_draw_li_dry_count; ++i) if (internal_draw_li_dry_block[i] == block) {
			if (internal_draw_li_dry_link11[i] >= 0) internal_draw_li_dry_link11[internal_draw_li_dry_link11[i]] = -1;
			if (internal_draw_li_dry_link11[i] >= 0) internal_draw_li_dry_link11[i] = -1;
			if (internal_draw_li_dry_link12[i] >= 0) internal_draw_li_dry_link21[internal_draw_li_dry_link12[i]] = -1;
			if (internal_draw_li_dry_link12[i] >= 0) internal_draw_li_dry_link12[i] = -1;
			if (internal_draw_li_dry_link21[i] >= 0) internal_draw_li_dry_link12[internal_draw_li_dry_link21[i]] = -1;
			if (internal_draw_li_dry_link21[i] >= 0) internal_draw_li_dry_link21[i] = -1;
			if (internal_draw_li_dry_link22[i] >= 0) internal_draw_li_dry_link22[internal_draw_li_dry_link22[i]] = -1;
			if (internal_draw_li_dry_link22[i] >= 0) internal_draw_li_dry_link22[i] = -1;
			internal_draw_li_dry_block[i] = null;
		}
	}
	protected void internal_draw_li_dry_release() {
		for (int i = 0; i < internal_draw_li_dry_count; ++i) internal_draw_li_dry_block[i] = null;
		internal_draw_li_dry_count = 0;
	}
	protected void internal_draw_li(TerrainBlock2D block, float x1, float y1, float x2, float y2, string color, float width) {
		internal_tiling_set(block);
		if (internal_draw_li_dry) {
			internal_draw_li_dry_add(block, x1, y1, x2, y2, true);
		} else if (enable_internal_draw_li_cache) {
			if (array_internal_draw_li_cache1 == null) array_internal_draw_li_cache1 = new Vector2[maxcount_internal_draw_li_cache];
			if (array_internal_draw_li_cache2 == null) array_internal_draw_li_cache2 = new Vector2[maxcount_internal_draw_li_cache];
			if (count_internal_draw_li_cache < array_internal_draw_li_cache1.Length) array_internal_draw_li_cache1[count_internal_draw_li_cache] = new Vector2(x1, y1);
			if (count_internal_draw_li_cache < array_internal_draw_li_cache2.Length) array_internal_draw_li_cache2[count_internal_draw_li_cache] = new Vector2(x2, y2);
			++count_internal_draw_li_cache;
		} else if (enable_internal_draw_li_altcache) {
			if (array_internal_draw_li_altcache1 == null) array_internal_draw_li_altcache1 = new Vector2[maxcount_internal_draw_li_altcache];
			if (array_internal_draw_li_altcache2 == null) array_internal_draw_li_altcache2 = new Vector2[maxcount_internal_draw_li_altcache];
			if (count_internal_draw_li_altcache < array_internal_draw_li_altcache1.Length) array_internal_draw_li_altcache1[count_internal_draw_li_altcache] = new Vector2(x1, y1);
			if (count_internal_draw_li_altcache < array_internal_draw_li_altcache2.Length) array_internal_draw_li_altcache2[count_internal_draw_li_altcache] = new Vector2(x2, y2);
			++count_internal_draw_li_altcache;
		} else {
			if (!draw_li_with_width) {
				meshDrawQuadAutoNormalReverse(
					block.useCollider ? block.colliderMesh : block.mesh,
					block.useCollider ? block.colliderMeshMetadata : block.meshMetadata,
					new Vector3(x1, y1, ofszw + ofsz),
					new Vector3(x2, y2, ofszw + ofsz),
					new Vector3(x2, y2, 0f + ofsz),
					new Vector3(x1, y1, 0f + ofsz), DEFAULT_THRESHOLD, false);
			} else {
				float dx = -(y2 - y1) * draw_li_width;
				float dy = (x2 - x1) * draw_li_width;
				meshDrawQuadAutoNormalReverse(
					block.useCollider ? block.colliderMesh : block.mesh,
					block.useCollider ? block.colliderMeshMetadata : block.meshMetadata,
					new Vector3(x1 - dx, y1 - dy, ofszw + ofsz),
					new Vector3(x2 - dx, y2 - dy, ofszw + ofsz),
					new Vector3(x2 + dx, y2 + dy, ofszw + ofsz),
					new Vector3(x1 + dx, y1 + dy, ofszw + ofsz), DEFAULT_THRESHOLD, false);
				meshDrawQuadAutoNormalReverse(
					block.useCollider ? block.colliderMesh : block.mesh,
					block.useCollider ? block.colliderMeshMetadata : block.meshMetadata,
					new Vector3(x1, y1, ofszw + ofsz),
					new Vector3(x2, y2, ofszw + ofsz),
					new Vector3(x2, y2, 0f + ofsz),
					new Vector3(x1, y1, 0f + ofsz), DEFAULT_THRESHOLD, false);
			}
		}
	}

	float ofsz = 0f;
	float ofszw = 0f;
	protected void fdraw_li(TerrainBlock2D block, float x1, float y1, float x2, float y2, string color, float width) {
		if (uno) internal_draw_li(block, x1, y1, x2, y2, color, width);
	}
	protected void xdraw_li(TerrainBlock2D block, float x1, float y1, float x2, float y2, string color, float width) {
		if (dos) internal_draw_li(block, x1, y1, x2, y2, color, width);
	}
	protected void edraw_li(TerrainBlock2D block, float x1, float y1, float x2, float y2, string color, float width) {
		if (tres) internal_draw_li(block, x1, y1, x2, y2, color, width);
	}
	protected void ndraw_li(TerrainBlock2D block, float x1, float y1, float x2, float y2, string color, float width) {
		if (cuatro) internal_draw_li(block, x1, y1, x2, y2, color, width);
	}
	protected bool draw_tri_uvs = false;
	protected float draw_tri_u1 = 0f;
	protected float draw_tri_v1 = 0f;
	protected float draw_tri_u2 = 0f;
	protected float draw_tri_v2 = 0f;
	protected float draw_tri_u3 = 0f;
	protected float draw_tri_v3 = 0f;

	protected void draw_tri(TerrainBlock2D block, float x1, float y1, float x2, float y2, float x3, float y3, string color) {
		if (!draw_tri_uvs) {
			internal_tiling_set(block);
			meshDrawTriangleAutoNormalReverse(
				block.useCollider ? block.colliderMesh : block.mesh,
				block.useCollider ? block.colliderMeshMetadata : block.meshMetadata,
				new Vector3(x1, y1, 0f + ofsz),
				new Vector3(x2, y2, 0f + ofsz),
				new Vector3(x3, y3, 0f + ofsz),
				DEFAULT_THRESHOLD, false
			);
		} else {
			meshDrawTriangleAutoNormalReverseUVs(
				block.useCollider ? block.colliderMesh : block.mesh,
				block.useCollider ? block.colliderMeshMetadata : block.meshMetadata,
				new Vector3(x1, y1, 0f + ofsz),
				new Vector3(x2, y2, 0f + ofsz),
				new Vector3(x3, y3, 0f + ofsz),
				new Vector2(draw_tri_u1, draw_tri_v1),
				new Vector2(draw_tri_u2, draw_tri_v2),
				new Vector2(draw_tri_u3, draw_tri_v3),
				DEFAULT_THRESHOLD, false
			);
		}
	}
	protected void draw_tri_upsidedown(TerrainBlock2D block, float x1, float y1, float x2, float y2, float x3, float y3, string color) {
		draw_tri(block, x1, y1, x3, y3, x2, y2, color);
	}
	protected bool draw_tri_if_area(TerrainBlock2D block, float x1, float y1, float x2, float y2, float x3, float y3, string color) {
		draw_tri(block, x1, y1, x2, y2, x3, y3, color);
		return true;
	}

	protected bool draw_quad_uvs = false;
	protected float draw_quad_u1 = 0f;
	protected float draw_quad_v1 = 0f;
	protected float draw_quad_u2 = 0f;
	protected float draw_quad_v2 = 0f;
	protected float draw_quad_u3 = 0f;
	protected float draw_quad_v3 = 0f;
	protected float draw_quad_u4 = 0f;
	protected float draw_quad_v4 = 0f;

	protected void draw_quad(TerrainBlock2D block, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, string color) {
		if (!draw_quad_uvs) {
			internal_tiling_set(block);
			meshDrawQuadAutoNormalReverse(
				block.useCollider ? block.colliderMesh : block.mesh,
				block.useCollider ? block.colliderMeshMetadata : block.meshMetadata,
				new Vector3(x1, y1, 0f + ofsz),
				new Vector3(x2, y2, 0f + ofsz),
				new Vector3(x3, y3, 0f + ofsz),
				new Vector3(x4, y4, 0f + ofsz),
				DEFAULT_THRESHOLD, false
			);
		} else {
			meshDrawQuadAutoNormalReverseUVs(
				block.useCollider ? block.colliderMesh : block.mesh,
				block.useCollider ? block.colliderMeshMetadata : block.meshMetadata,
				new Vector3(x1, y1, 0f + ofsz),
				new Vector3(x2, y2, 0f + ofsz),
				new Vector3(x3, y3, 0f + ofsz),
				new Vector3(x4, y4, 0f + ofsz),
				new Vector2(draw_quad_u1, draw_quad_v1),
				new Vector2(draw_quad_u2, draw_quad_v2),
				new Vector2(draw_quad_u3, draw_quad_v3),
				new Vector2(draw_quad_u4, draw_quad_v4),
				DEFAULT_THRESHOLD, false
			);
		}
	}
	protected void draw_number(TerrainBlock2D block, float x, float y, float number, string color) {
	}

	protected void redraw_blob0(TerrainBlock2D block, bool drawpositive, bool drawnegative, bool drawborder, bool drawdebug) {
		float ofsx = 0f, ofsy = 0f, sizx = 1f, sizy = 1f, sepx = 1f, sepy = 1f;

		float valueC;
		float NWx = 0.0f, NWy = 0.0f;
		float NEx = 1.0f, NEy = 0.0f;
		float SWx = 0.0f, SWy = 1.0f;
		float SEx = 1.0f, SEy = 1.0f;

		int meshcalls = 0;

		for (int blobx = 0; blobx < block.width; ++blobx) {
			for (int bloby = 0; bloby < block.height; ++bloby) {
				valueC = blockData(block, blobx, bloby, 0f);
				if (drawpositive) {
					NWx = 0.5f - 0.5f * valueC; NWy = 0.5f - 0.5f * valueC;
					NEx = 0.5f + 0.5f * valueC; NEy = 0.5f - 0.5f * valueC;
					SWx = 0.5f - 0.5f * valueC; SWy = 0.5f + 0.5f * valueC;
					SEx = 0.5f + 0.5f * valueC; SEy = 0.5f + 0.5f * valueC;
					draw_quad(
						block,
						blobx * sepx + NWx * sizx, bloby * sepy + NWy * sizy,
						blobx * sepx + NEx * sizx, bloby * sepy + NEy * sizy,
						blobx * sepx + SEx * sizx, bloby * sepy + SEy * sizy,
						blobx * sepx + SWx * sizx, bloby * sepy + SWy * sizy,
						Acolor
					);
					++meshcalls;
				}
				if (drawnegative) {
					valueC = 1.0f - valueC; if (valueC < 0.0f) valueC = 0.0f;
					NWx = 0.5f - 0.5f * valueC; NWy = 0.5f - 0.5f * valueC;
					NEx = 0.5f + 0.5f * valueC; NEy = 0.5f - 0.5f * valueC;
					SWx = 0.5f - 0.5f * valueC; SWy = 0.5f + 0.5f * valueC;
					SEx = 0.5f + 0.5f * valueC; SEy = 0.5f + 0.5f * valueC;
					draw_quad(
						block,
						blobx * sepx + NWx * sizx, bloby * sepy + NWy * sizy,
						blobx * sepx + NEx * sizx, bloby * sepy + NEy * sizy,
						blobx * sepx + SEx * sizx, bloby * sepy + SEy * sizy,
						blobx * sepx + SWx * sizx, bloby * sepy + SWy * sizy,
						Bcolor
					);
					++meshcalls;
				}
				if (drawdebug) {
					float magnify = 1f;
					draw_number(block, ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy, valueC, drawnegative ? "red" : (!drawpositive ? "grey" : "black"));
					draw_number(block, ofsx + blobx * sepx + 0.15f * sizx, ofsy + bloby * sepy + 0.6f * sizy, valueC, drawnegative ? "red" : (!drawpositive ? "grey" : "black"));
					draw_number(block, ofsx + blobx * sepx + (0.5f - 0.35f / magnify) * sizx, ofsy + bloby * sepy + (0.5f + 0.1f / magnify) * sizy, valueC, drawnegative ? "red" : (!drawpositive ? "grey" : "black"));
				}
			}
		}
		Debug.Log("mesh calls were " + meshcalls);
	}

	protected void redraw_blob1(TerrainBlock2D block, bool drawpositive, bool drawnegative, bool drawborder, bool drawdebug, bool meshcollider) {
		block.useCollider = meshcollider;
		float ofsx = 0f, ofsy = 0f, sizx = 1f, sizy = 1f, sepx = 1f, sepy = 1f;

		bool blobDstep = false;
		float tStep = 0.001f;

		BBcolor = drawnegative ? "yellow" : (!drawpositive ? "red" : "black");
		float threshold = 0.0001f, threshold_droplets = 0.05f, probability_droplets = 0.5f, Fthreshold = 0.01f, Fthreshold2 = 0.1f;

		bool NN = true, WW = true, EE = true, SS = true;
		float NWx = 0.0f, NWy = 0.0f;
		float NEx = 1.0f, NEy = 0.0f;
		float SWx = 0.0f, SWy = 1.0f;
		float SEx = 1.0f, SEy = 1.0f;
		float CCx = 0.5f, CCy = 0.5f;
		float valueC = 1.0f;
		float valueN = 0.0f, valueW = 0.0f, valueE = 0.0f, valueS = 0.0f;
		bool fN = false, fS = false, fW = false, fE = false;
		float biasx, biasy;
		float deltax, deltay;

		for (int blobx = 0; blobx < block.width; ++blobx) {
			for (int bloby = 0; bloby < block.height; ++bloby) {
				valueC = blockData(block, blobx, bloby, 0, 0, 0.0f); if (blobDstep) valueC = Mathf.RoundToInt(valueC / tStep) * tStep;
				biasx = 0.5f; biasy = 0.5f;
				deltax = 0.25f; deltay = 0.25f;

				if (valueC >= 1.0f - threshold) {
					NN = WW = EE = SS = true;
					NWx = 0.0f; NWy = 0.0f;
					NEx = 1.0f; NEy = 0.0f;
					SWx = 0.0f; SWy = 1.0f;
					SEx = 1.0f; SEy = 1.0f;
					CCx = 0.5f; CCy = 0.5f;
				} else if (valueC < 0.0f + threshold) {
					NN = WW = EE = SS = false;
				} else {
					valueN = blockData(block, blobx, bloby, 0, -1, 0.0f);
					valueS = blockData(block, blobx, bloby, 0, +1, 0.0f);
					valueW = blockData(block, blobx, bloby, -1, 0, 0.0f);
					valueE = blockData(block, blobx, bloby, +1, 0, 0.0f);

					fN = valueN > Fthreshold;	fW = valueW > Fthreshold;	fE = valueE > Fthreshold;	fS = valueS > Fthreshold;
					if (fN && fW && fE) {
						fS = false;
						if (valueS > Fthreshold2) fS = true;
					}

					NN = WW = EE = SS = true;
					if (fW && fE)  biasx = 0.0f; else if (fW ^ fE)  biasx = 0.0f; else  biasx = 0.5f;
					if (fN && fS)  biasy = 0.0f; else if (fN ^ fS)  biasy = 0.0f; else  biasy = 0.5f;
					if (fW && fE) deltax = 0.5f; else if (fW ^ fE) deltax = 1.0f; else deltax = 0.5f;
					if (fN && fS) deltay = 0.5f; else if (fN ^ fS) deltay = 1.0f; else deltay = 0.5f;
					biasy = biasy * waterBias;
					deltay = deltay * waterDelta * 0.5f;
					valueN += valueC;
					valueW += valueC;
					valueE += valueC;
					valueS += valueC;
					if (valueC < threshold_droplets) {
						if (UnityEngine.Random.value > probability_droplets * (valueC - threshold) / (threshold_droplets - threshold)) NN = false;
						if (UnityEngine.Random.value > probability_droplets * (valueC - threshold) / (threshold_droplets - threshold)) WW = false;
						if (UnityEngine.Random.value > probability_droplets * (valueC - threshold) / (threshold_droplets - threshold)) EE = false;
						if (UnityEngine.Random.value > probability_droplets * (valueC - threshold) / (threshold_droplets - threshold)) SS = false;
					}
					NWx = fW ? 0.0f + biasx : 1.0f - biasx - (valueN) * deltax; NWy =          fN   ? 0.0f + biasy : 1.0f - biasy - (valueW) * deltay;
					NEx = fE ? 1.0f - biasx : 0.0f + biasx + (valueN) * deltax; NEy =          fN   ? 0.0f + biasy : 1.0f - biasy - (valueE) * deltay;
					SWx = fW ? 0.0f + biasx : 1.0f - biasx - (valueS) * deltax; SWy = (fS || (!fN)) ? 1.0f - biasy : 0.0f + biasy + (valueW) * deltay;
					SEx = fE ? 1.0f - biasx : 0.0f + biasx + (valueS) * deltax; SEy = (fS || (!fN)) ? 1.0f - biasy : 0.0f + biasy + (valueE) * deltay;
					if (NWx > NEx) { NWx = (NWx + NEx - threshold * 0.1f) * 0.5f; NEx = (NWx + NEx + threshold * 0.1f) * 0.5f; Ncolor = Ecolor = Wcolor = Scolor = "green";}
					if (SWx > SEx) { SWx = (SWx + SEx - threshold * 0.1f) * 0.5f; SEx = (SWx + SEx + threshold * 0.1f) * 0.5f; Ncolor = Ecolor = Wcolor = Scolor = "red";}
					CCx = (NWx + NEx + SWx + SEx) / 4; CCy = (NWy + NEy + SWy + SEy) / 4;
				}

				if (drawpositive) {
					if (NN) draw_tri(
						block,
						ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
						ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
						ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
						Ncolor
					);
					if (EE) draw_tri(
						block,
						ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
						ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
						ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
						Ecolor
					);
					if (SS) draw_tri(
						block,
						ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
						ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
						ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
						Scolor
					);
					if (WW) draw_tri(
						block,
						ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
						ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
						ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
						Wcolor
					);
				}
				if (drawnegative) {
					if (!NN) draw_tri(
						block,
						ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
						ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
						ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
						BcolorN
					);
					if (!EE) draw_tri(
						block,
						ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
						ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
						ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
						BcolorE
					);
					if (!SS) draw_tri(
						block,
						ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
						ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
						ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
						BcolorS
					);
					if (!WW) draw_tri(
						block,
						ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
						ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
						ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
						BcolorW
					);
				}
			}
		}
	}

	protected bool blobDstep = true;
	protected string BBcolor = "cyan";
	protected string Ncolor = "red";
	protected string Wcolor = "blue";
	protected string Ecolor = "green";
	protected string Scolor = "violet";
	protected string Bcolor = "black";
	protected string BcolorN = "red";
	protected string BcolorW = "blue";
	protected string BcolorE = "green";
	protected string BcolorS = "violet";
	protected string Bcolor2 = "grey";
	protected string Acolor = "brown";
	protected string Dcolor = "red";
	protected int DEBUGd = 0;
	protected int DEBUGdMax = 3;
	protected bool FOREd = true;
	protected bool BACKd = true;
	protected bool BORDe = true;
	protected bool FORE2 = false;
	protected void redraw_blob3(TerrainBlock2D block, bool drawpositive, bool drawnegative, bool drawborder, bool drawdebug, bool meshcollider, bool linedry) {
		block.useCollider = meshcollider;
		internal_draw_li_dry = linedry;
		float ofsx = 0f, ofsy = 0f, sizx = 1f, sizy = 1f, sepx = 1f, sepy = 1f;

		float NWx = 0.0f, NWy = 0.0f;
		float NEx = 1.0f, NEy = 0.0f;
		float SWx = 0.0f, SWy = 1.0f;
		float SEx = 1.0f, SEy = 1.0f;
		float CCx = 0.5f, CCy = 0.5f;
		float CCxC = 0.0f, CCyC = 0.0f;
		bool NN = true, WW = true, EE = true, SS = true;
		float valueC = 1.0f;
		float valueN = 0.0f, valueW = 0.0f, valueE = 0.0f, valueS = 0.0f;
		float valueNW = 0.0f, valueNE = 0.0f, valueSW = 0.0f, valueSE = 0.0f;
		bool fN = false, fS = false, fW = false, fE = false, fNW = false, fNE = false, fSW = false, fSE = false, fNWe = false, fNEe = false, fSWe = false, fSEe = false;
		float CCamp = 1.2f;
		float EEamp = 1.0f;
		float threshold = 0.05f;
		float tMin = 0.0f, tMax = 0.9f, tStep = 0.1f;

		for (int blobx = 0; blobx < block.width; ++blobx) {
			for (int bloby = 0; bloby < block.height; ++bloby) {
				valueC = blockData(block, blobx, bloby, 0, 0, 0.0f); if (blobDstep) valueC = Mathf.RoundToInt(valueC / tStep) * tStep;
				if (valueC <= 0.0f) {

					if (drawnegative) {
						NWx = 0.5f - 1.0f * 0.5f; NWy = 0.5f - 1.0f * 0.5f;
						NEx = 0.5f + 1.0f * 0.5f; NEy = 0.5f - 1.0f * 0.5f;
						SWx = 0.5f - 1.0f * 0.5f; SWy = 0.5f + 1.0f * 0.5f;
						SEx = 0.5f + 1.0f * 0.5f; SEy = 0.5f + 1.0f * 0.5f;
						draw_tri(
							block,
							ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							Bcolor
						);
						draw_tri(
							block,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							Bcolor
						);
					}

				} else if (FORE2 && (valueC >= 1.0f)) {

					if (drawpositive) {
						NWx = 0.5f - 1.0f * 0.5f; NWy = 0.5f - 1.0f * 0.5f;
						NEx = 0.5f + 1.0f * 0.5f; NEy = 0.5f - 1.0f * 0.5f;
						SWx = 0.5f - 1.0f * 0.5f; SWy = 0.5f + 1.0f * 0.5f;
						SEx = 0.5f + 1.0f * 0.5f; SEy = 0.5f + 1.0f * 0.5f;
						draw_tri(
							block,
							ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							Acolor
						);
						draw_tri(
							block,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							Acolor
						);
					}

				} else {

					valueNW = blockData(block, blobx, bloby, -1, -1, 0.0f); if (blobDstep) valueNW = Mathf.RoundToInt(valueNW / tStep) * tStep;
					valueN = blockData(block, blobx, bloby, 0, -1, 0.0f); if (blobDstep) valueN = Mathf.RoundToInt(valueN / tStep) * tStep;
					valueNE = blockData(block, blobx, bloby, +1, -1, 0.0f); if (blobDstep) valueNE = Mathf.RoundToInt(valueNE / tStep) * tStep;
					valueW = blockData(block, blobx, bloby, -1, 0, 0.0f); if (blobDstep) valueW = Mathf.RoundToInt(valueW / tStep) * tStep;
					valueE = blockData(block, blobx, bloby, +1, 0, 0.0f); if (blobDstep) valueE = Mathf.RoundToInt(valueE / tStep) * tStep;
					valueSW = blockData(block, blobx, bloby, -1, +1, 0.0f); if (blobDstep) valueSW = Mathf.RoundToInt(valueSW / tStep) * tStep;
					valueS = blockData(block, blobx, bloby, 0, +1, 0.0f); if (blobDstep) valueS = Mathf.RoundToInt(valueS / tStep) * tStep;
					valueSE = blockData(block, blobx, bloby, +1, +1, 0.0f); if (blobDstep) valueSE = Mathf.RoundToInt(valueSE / tStep) * tStep;

					if (valueC <= tMax) {
						if (valueN > tMin) { NN = true; } else { NN = false; }
						if (valueS > tMin) { SS = true; } else { SS = false; }
						if (valueW > tMin) { WW = true; } else { WW = false; }
						if (valueE > tMin) { EE = true; } else { EE = false; }
					} else if (valueC > tMax) {
						NN = true;
						SS = true;
						WW = true;
						EE = true;
					}

					NWx = 0.5f - 1.0f * 0.5f; NWy = 0.5f - 1.0f * 0.5f;
					NEx = 0.5f + 1.0f * 0.5f; NEy = 0.5f - 1.0f * 0.5f;
					SWx = 0.5f - 1.0f * 0.5f; SWy = 0.5f + 1.0f * 0.5f;
					SEx = 0.5f + 1.0f * 0.5f; SEy = 0.5f + 1.0f * 0.5f;
					if (valueN <= tMin) {
						if (valueNW <= tMin) NWy = 1.0f - (valueC + valueW) * 0.5f * EEamp;
						if (valueNE <= tMin) NEy = 1.0f - (valueC + valueE) * 0.5f * EEamp;
					}
					if (valueW <= tMin) {
						if (valueNW <= tMin) NWx = 1.0f - (valueC + valueN) * 0.5f * EEamp;
						if (valueSW <= tMin) SWx = 1.0f - (valueC + valueS) * 0.5f * EEamp;
					}
					if (valueE <= tMin) {
						if (valueNE <= tMin) NEx = (valueC + valueN) * 0.5f * EEamp;
						if (valueSE <= tMin) SEx = (valueC + valueS) * 0.5f * EEamp;
					}
					if (valueS <= tMin) {
						if (valueSW <= tMin) SWy = (valueC + valueW) * 0.5f * EEamp;
						if (valueSE <= tMin) SEy = (valueC + valueE) * 0.5f * EEamp;
					}

					if (NWx > NEx) { NWx = NEx = (NWx + NEx) / 2.0f; }
					if (SWx > SEx) { SWx = SEx = (SWx + SEx) / 2.0f; }
					if (NWy > SWy) { NWy = SWy = (NWy + SWy) / 2.0f; }
					if (NEy > SEy) { NEy = SEy = (NEy + SEy) / 2.0f; }

					CCx = 0.5f; CCy = 0.5f; CCxC = 0.0f; CCyC = 0.0f;
					if (NN) { CCx += 0.5f; CCy += valueC * CCamp; CCxC += 1.0f; CCyC += 1.0f; }
					if (SS) { CCx += 0.5f; CCy += (1.0f - valueC * CCamp); CCxC += 1.0f; CCyC += 1.0f; }
					if (WW) { CCx += valueC * CCamp; CCy += 0.5f; CCxC += 1.0f; CCyC += 1.0f; }
					if (EE) { CCx += (1.0f - valueC * CCamp); CCy += 0.5f; CCxC += 1.0f; CCyC += 1.0f; }
					if (CCxC > 0.0f) { CCx = CCx / CCxC; } else { CCx = 0.5f; }
					if (CCyC > 0.0f) { CCy = CCy / CCyC; } else { CCy = 0.5f; }
					if (NWx < 0.0f) NWx = 0.0f; else if (NWx > 1.0f) NWx = 1.0f;
					if (NWy < 0.0f) NWy = 0.0f; else if (NWy > 1.0f) NWy = 1.0f;
					if (NEx < 0.0f) NEx = 0.0f; else if (NEx > 1.0f) NEx = 1.0f;
					if (NEy < 0.0f) NEy = 0.0f; else if (NEy > 1.0f) NEy = 1.0f;
					if (SWx < 0.0f) SWx = 0.0f; else if (SWx > 1.0f) SWx = 1.0f;
					if (SWy < 0.0f) SWy = 0.0f; else if (SWy > 1.0f) SWy = 1.0f;
					if (SEx < 0.0f) SEx = 0.0f; else if (SEx > 1.0f) SEx = 1.0f;
					if (SEy < 0.0f) SEy = 0.0f; else if (SEy > 1.0f) SEy = 1.0f;
					if (CCx < NWx) CCx = NWx;
					if (CCx < SWx) CCx = SWx;
					if (CCx > NEx) CCx = NEx;
					if (CCx > SEx) CCx = SEx;
					if (CCy < NWy) CCy = NWy;
					if (CCy < NEy) CCy = NEy;
					if (CCy > SWy) CCy = SWy;
					if (CCy > SEy) CCy = SEy;
					if (!(NN || WW)) { NWx = 0.5f - 1.0f * 0.5f; NWy = 0.5f - 1.0f * 0.5f; }
					if (!(NN || EE)) { NEx = 0.5f + 1.0f * 0.5f; NEy = 0.5f - 1.0f * 0.5f; }
					if (!(SS || WW)) { SWx = 0.5f - 1.0f * 0.5f; SWy = 0.5f + 1.0f * 0.5f; }
					if (!(SS || EE)) { SEx = 0.5f + 1.0f * 0.5f; SEy = 0.5f + 1.0f * 0.5f; }

					if ((drawnegative && (!NN)) || (drawpositive && NN)) {
						draw_tri(
							block,
							ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
							NN ? Ncolor : BcolorN
						);
					}
					if (drawnegative) {
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
							ofsx + blobx * sepx + 0.0f * sizx, ofsy + bloby * sepy + 0.0f * sizy,
							ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.0f * sizy,
							Bcolor2
						);
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
							ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.0f * sizy,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							Bcolor2
						);
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.0f * sizy,
							ofsx + blobx * sepx + 1.0f * sizx, ofsy + bloby * sepy + 0.0f * sizy,
							Bcolor2
						);
					}

					if ((drawnegative && (!WW)) || (drawpositive && WW)) {
						draw_tri(
							block,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
							ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
							WW ? Wcolor : BcolorW
						);
					}
					if (drawnegative) {
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
							ofsx + blobx * sepx + 0.0f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
							ofsx + blobx * sepx + 0.0f * sizx, ofsy + bloby * sepy + 0.0f * sizy,
							Bcolor2
						);
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							ofsx + blobx * sepx + 0.0f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
							Bcolor2
						);
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							ofsx + blobx * sepx + 0.0f * sizx, ofsy + bloby * sepy + 1.0f * sizy,
							ofsx + blobx * sepx + 0.0f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
							Bcolor2
						);
					}

					if ((drawnegative && (!EE)) || (drawpositive && EE)) {
						draw_tri(
							block,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
							ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
							EE ? Ecolor : BcolorE
						);
					}
					if (drawnegative) {
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + 1.0f * sizx, ofsy + bloby * sepy + 0.0f * sizy,
							ofsx + blobx * sepx + 1.0f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
							Bcolor2
						);
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
							ofsx + blobx * sepx + 1.0f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
							ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
							Bcolor2
						);
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
							ofsx + blobx * sepx + 1.0f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
							ofsx + blobx * sepx + 1.0f * sizx, ofsy + bloby * sepy + 1.0f * sizy,
							Bcolor2
						);
					}

					if ((drawnegative && (!SS)) || (drawpositive && SS)) {
						draw_tri(
							block,
							ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
							SS ? Scolor : BcolorS
						);
					}
					if (drawnegative) {
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 1.0f * sizy,
							ofsx + blobx * sepx + 0.0f * sizx, ofsy + bloby * sepy + 1.0f * sizy,
							Bcolor2
						);
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
							ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
							ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 1.0f * sizy,
							Bcolor2
						);
						draw_tri_if_area(
							block,
							ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
							ofsx + blobx * sepx + 1.0f * sizx, ofsy + bloby * sepy + 1.0f * sizy,
							ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 1.0f * sizy,
							Bcolor2
						);
					}

					float bbwi = 1.1f;
					if (drawborder) {
						if ((NN) && (!WW)) {
							fdraw_li(
								block,
								ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
								ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
								BBcolor, bbwi
							);
						} else if ((!NN) && (WW)) {
							fdraw_li(
								block,
								ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
								ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
								BBcolor, bbwi
							);
						}
						if ((NN) && (!EE)) {
							fdraw_li(
								block,
								ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
								ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
								BBcolor, bbwi
							);
						} else if ((!NN) && (EE)) {
							fdraw_li(
								block,
								ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
								ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
								BBcolor, bbwi
							);
						}
						if ((SS) && (!WW)) {
							fdraw_li(
								block,
								ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
								ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
								BBcolor, bbwi
							);
						} else if ((!SS) && (WW)) {
							fdraw_li(
								block,
								ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
								ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
								BBcolor, bbwi
							);
						}
						if ((SS) && (!EE)) {
							fdraw_li(
								block,
								ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
								ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
								BBcolor, bbwi
							);
						} else if ((!SS) && (EE)) {
							fdraw_li(
								block,
								ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
								ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
								BBcolor, bbwi
							);
						}

						if (valueC > tMax) {
							fN = Mathf.Abs(valueN - 0.0f) < threshold;	fW = Mathf.Abs(valueW - 0.0f) < threshold;	fE = Mathf.Abs(valueE - 0.0f) < threshold;	fS = Mathf.Abs(valueS - 0.0f) < threshold;
							fNW = Mathf.Abs(valueNW - 0.0f) < threshold;	fNE = Mathf.Abs(valueNE - 0.0f) < threshold;	fSW = Mathf.Abs(valueSW - 0.0f) < threshold;	fSE = Mathf.Abs(valueSE - 0.0f) < threshold;
							fNWe = fN && fNW && fW;	fNEe = fN && fNE && fE;	fSWe = fS && fSW && fW;	fSEe = fS && fSE && fE;
							if ((fNWe && fNEe && fSWe && fSEe) || (fNWe && fSEe) || (fNEe && fSWe)) {
							} else {
								CCx = 0.5f;
								CCy = 0.5f;
								if (fNWe && fNEe) {
									xdraw_li(
										block,
										ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
										ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
										BBcolor, bbwi
									);
									xdraw_li(
										block,
										ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
										ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
										BBcolor, bbwi
									);
									if (fS) {
										xdraw_li(
											block,
											ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
											ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
											BBcolor, bbwi
										);
									}
								} else if (fNWe && fSWe) {
									xdraw_li(
										block,
										ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
										ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
										BBcolor, bbwi
									);
									xdraw_li(
										block,
										ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
										ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
										BBcolor, bbwi
									);
									if (fE) {
										xdraw_li(
											block,
											ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
											ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
											BBcolor, bbwi
										);
									}
								} else if (fNEe && fSEe) {
									xdraw_li(
										block,
										ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
										ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
										BBcolor, bbwi
									);
									xdraw_li(
										block,
										ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
										ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
										BBcolor, bbwi
									);
									if (fW) {
										xdraw_li(
											block,
											ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
											ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
											BBcolor, bbwi
										);
									}
								} else if (fSWe && fSEe) {
									xdraw_li(
										block,
										ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
										ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
										BBcolor, bbwi
									);
									xdraw_li(
										block,
										ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
										ofsx + blobx * sepx + CCx * sizx, ofsy + bloby * sepy + CCy * sizy,
										BBcolor, bbwi
									);
									if (fN) {
										xdraw_li(
											block,
											ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
											ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
											BBcolor, bbwi
										);
									}
								} else {
									if (fNWe) {
										edraw_li(
											block,
											ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
											ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
											BBcolor, bbwi
										);
										edraw_li(
											block,
											ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
											ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
											BBcolor, bbwi
										);
										if (fE) {
											edraw_li(
												block,
												ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
												ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
												BBcolor, bbwi
											);
										}
										if (fS) {
											edraw_li(
												block,
												ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
												ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
												BBcolor, bbwi
											);
										}
									} else if (fNEe) {
										edraw_li(
											block,
											ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
											ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
											BBcolor, bbwi
										);
										edraw_li(
											block,
											ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
											ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
											BBcolor, bbwi
										);
										if (fW) {
											edraw_li(
												block,
												ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
												ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
												BBcolor, bbwi
											);
										}
										if (fS) {
											edraw_li(
												block,
												ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
												ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
												BBcolor, bbwi
											);
										}
									} else if (fSWe) {
										edraw_li(
											block,
											ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
											ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
											BBcolor, bbwi
										);
										edraw_li(
											block,
											ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
											ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
											BBcolor, bbwi
										);
										if (fN) {
											edraw_li(
												block,
												ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
												ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
												BBcolor, bbwi
											);
										}
										if (fE) {
											edraw_li(
												block,
												ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
												ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
												BBcolor, bbwi
											);
										}
									} else if (fSEe) {
										edraw_li(
											block,
											ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
											ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
											BBcolor, bbwi
										);
										edraw_li(
											block,
											ofsx + blobx * sepx + 0.5f * sizx, ofsy + bloby * sepy + 0.5f * sizy,
											ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
											BBcolor, bbwi
										);
										if (fN) {
											edraw_li(
												block,
												ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
												ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
												BBcolor, bbwi
											);
										}
										if (fW) {
											edraw_li(
												block,
												ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
												ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
												BBcolor, bbwi
											);
										}
									} else {
										if (fN) {
											ndraw_li(
												block,
												ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
												ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
												BBcolor, bbwi
											);
										}
										if (fW) {
											ndraw_li(
												block,
												ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
												ofsx + blobx * sepx + NWx * sizx, ofsy + bloby * sepy + NWy * sizy,
												BBcolor, bbwi
											);
										}
										if (fE) {
											ndraw_li(
												block,
												ofsx + blobx * sepx + NEx * sizx, ofsy + bloby * sepy + NEy * sizy,
												ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
												BBcolor, bbwi
											);
										}
										if (fS) {
											ndraw_li(
												block,
												ofsx + blobx * sepx + SEx * sizx, ofsy + bloby * sepy + SEy * sizy,
												ofsx + blobx * sepx + SWx * sizx, ofsy + bloby * sepy + SWy * sizy,
												BBcolor, bbwi
											);
										}
									}
								}
							}
						}
					}

				}
				if (drawdebug) {
					float magnify = 1f;
					draw_number(block, ofsx + blobx * sepx + (0.5f - 0.35f / magnify) * sizx, ofsy + bloby * sepy + (0.5f + 0.1f / magnify) * sizy, valueC, drawnegative ? "red" : (!drawpositive ? "grey" : "black"));
				}
			}
		}
		internal_draw_li_dry = false;
	}

	float fluid_percent = 0.75f;
	float fluid_lateral = 0.75f, fluid_lateral_var = 0.45f;

	int fluid_stat_count = 0, fluid_stat_count0 = 0, fluid_stat_countA = 0, fluid_stat_countB = 0, fluid_stat_count1 = 0, fluid_stat_count2 = 0;
	float fluid_stat_ammount = 0f;

	protected float fluidto_reference(int x, int y, Terrain2D referencelayer, bool referencenegative) {
		float reference_max_fill = 0.0f;
		float reference_threshold = 0.001f;
		float reference_multiplier = 1000f;
		if (referencenegative) {
			if ((referencelayer == null) || (referencelayer.data[Mathf.FloorToInt(x * referencelayer.dataWidth / dataWidth) + referencelayer.dataWidth * Mathf.FloorToInt(y * referencelayer.dataHeight / dataHeight)] < reference_threshold)) {
				return -999f;
			} else reference_max_fill = reference_multiplier * referencelayer.data[Mathf.FloorToInt(x * referencelayer.dataWidth / dataWidth) + referencelayer.dataWidth * Mathf.FloorToInt(y * referencelayer.dataHeight / dataHeight)];
		} else {
			if ((referencelayer == null) || (referencelayer.data[Mathf.FloorToInt(x * referencelayer.dataWidth / dataWidth) + referencelayer.dataWidth * Mathf.FloorToInt(y * referencelayer.dataHeight / dataHeight)] > 1f - reference_threshold)) {
				return -999f;
			} else reference_max_fill = reference_multiplier * (1f - referencelayer.data[Mathf.FloorToInt(x * referencelayer.dataWidth / dataWidth) + referencelayer.dataWidth * Mathf.FloorToInt(y * referencelayer.dataHeight / dataHeight)]);
		}
		if (reference_max_fill > 1f) reference_max_fill = 1f;
		return reference_max_fill;
	}
	protected bool fluidto_blob(int ix, int iy, int tx, int ty, float ammount, bool balancing, Terrain2D referencelayer, bool referencenegative, bool deltaammount) {
		float tMax = 0.9f;
		float sourcev, targetv;
		float threshold = 0.0001f;
		++fluid_stat_count0;

		sourcev = data[ix + dataWidth * iy];
		if (data[ix + dataWidth * iy] < threshold) {
			data[ix + dataWidth * iy] = 0f;
			return false;
		}
		targetv = data[tx + dataWidth * ty];
		if (targetv >= (1f - threshold)) return false;
		++fluid_stat_countA;

		if (deltaammount) {
			float deltavalue = Mathf.Abs(targetv - sourcev) * 10f;
			if (deltavalue > 1f) deltavalue = 1f;
			ammount *= deltavalue;
		}

		float reference_max_fill = 1f;
		if (referencelayer) reference_max_fill = fluidto_reference(tx, ty, referencelayer, referencenegative);
		if (reference_max_fill < -99f) return false;
		if (reference_max_fill > tMax) reference_max_fill = 1f;
		++fluid_stat_countB;

		if (balancing) if (targetv >= sourcev) return false;
		if ((targetv + ammount * sourcev) > reference_max_fill) {
			ammount = (reference_max_fill - targetv) / sourcev;
			if (ammount < 0f) return false;
			if (ammount > sourcev) return false;
		}
		++fluid_stat_count;
		fluid_stat_ammount += ammount * sourcev;
		data[tx + dataWidth * ty] += ammount * sourcev;
		data[ix + dataWidth * iy] -= ammount * sourcev;
		return true;
	}
	protected void fluid_iteration(int blobx, int bloby, Terrain2D referencelayer, bool referencenegative) {
		if (!fluidto_blob(blobx, bloby, blobx, bloby - 1, fluid_percent, false, referencelayer, referencenegative, false)) {
			if (data[blobx - 1 + dataWidth * bloby] < data[blobx + 1 + dataWidth * bloby]) {
				++fluid_stat_count1;
				fluidto_blob(blobx, bloby, blobx - 1, bloby, fluid_percent * (fluid_lateral + UnityEngine.Random.value * fluid_lateral_var), false, referencelayer, referencenegative, true);
				fluidto_blob(blobx, bloby, blobx + 1, bloby, fluid_percent * (fluid_lateral - UnityEngine.Random.value * fluid_lateral_var), false, referencelayer, referencenegative, true);
			} else {
				++fluid_stat_count1;
				fluidto_blob(blobx, bloby, blobx + 1, bloby, fluid_percent * (fluid_lateral + UnityEngine.Random.value * fluid_lateral_var), false, referencelayer, referencenegative, true);
				fluidto_blob(blobx, bloby, blobx - 1, bloby, fluid_percent * (fluid_lateral - UnityEngine.Random.value * fluid_lateral_var), false, referencelayer, referencenegative, true);
			}
		}
	}
	protected void fluid_blob(Terrain2D referencelayer, bool referencenegative) {
		switch (Mathf.FloorToInt(UnityEngine.Random.value * 2)) {
			default:
			case 0:
				++fluid_stat_count2;
				for (var bloby = 1; bloby <= dataHeight - 2; ++bloby) for (var blobx = 1; blobx <= dataWidth - 2; ++blobx) if (data[blobx + dataWidth * bloby] > 0.0f) fluid_iteration(blobx, bloby, referencelayer, referencenegative);
				break;
			case 1:
				++fluid_stat_count2;
				for (var bloby = 1; bloby <= dataHeight - 2; ++bloby) for (var blobx = dataWidth - 2; blobx >= 0; --blobx) if (data[blobx + dataWidth * bloby] > 0.0f) fluid_iteration(blobx, bloby, referencelayer, referencenegative);
				break;
			case 2:
				++fluid_stat_count2;
				for (var bloby = dataHeight - 2; bloby >= 0; ++bloby) for (var blobx = 1; blobx <= dataWidth - 2; ++blobx) if (data[blobx + dataWidth * bloby] > 0.0f) fluid_iteration(blobx, bloby, referencelayer, referencenegative);
				break;
			case 3:
				++fluid_stat_count2;
				for (var bloby = dataHeight - 2; bloby >= 0; ++bloby) for (var blobx = dataWidth - 2; blobx >= 0; --blobx) if (data[blobx + dataWidth * bloby] > 0.0f) fluid_iteration(blobx, bloby, referencelayer, referencenegative);
				break;
		}
	}

	protected const float DEFAULT_THRESHOLD = 0.01f;
	protected const string DEFAULT_SHADER = "Diffuse";

	[System.Serializable]public class MeshMetadata {
		public Vector3[] vertices = null;
		public Vector3[] normals = null;
		public Vector2[] uvs = null;
		public int[] triangles = null;
		public int[] triangles2 = null;
		public int usedVertices = 0;
		public int usedTriangles = 0;
		public int usedTriangles2 = 0;
		public bool checkNormals = true;
		public bool checkUVs = false;
		public bool autogenerateNormals = false;
		public bool autogenerateUVs = true;
		public GameObject gameObject = null;
		public Mesh mesh = null;
		public bool ready = false;
	}

	static public bool autoReady = true;

	MeshMetadata defaultMeshMetadata = new MeshMetadata();

	protected float autouv_x_bias = 0.0f;
	protected float autouv_y_bias = 0.0f;
	protected float autouv_z_bias = 0.0f;
	protected float autouv_x = 1.0f;
	protected float autouv_y = 1.0f;
	protected float autouv_z = 1.0f;
	protected float autouv_bias_to_u = 0.0f;
	protected float autouv_x_to_u = 1.0f;
	protected float autouv_y_to_u = 0.0f;
	protected float autouv_z_to_u = 0.5f;
	protected float autouv_bias_to_v = 0.0f;
	protected float autouv_x_to_v = 0.0f;
	protected float autouv_y_to_v = 1.0f;
	protected float autouv_z_to_v = 0.75f;
	protected int autouv_log_count = 0;
	protected int autouv_log_rate = 125 * 1000000;

	protected bool autoxyz_transform = false;
	protected Vector3 autoxyz_right = Vector3.right;
	protected Vector3 autoxyz_up = Vector3.up;
	protected Vector3 autoxyz_forward = Vector3.forward;

	protected MeshRenderer bindMeshRenderer(MeshMetadata meta, GameObject gameObject, Material material, bool materialOnlyIfNotSet) {
		MeshRenderer meshRenderer;
		if ((meshRenderer = (MeshRenderer)gameObject.GetComponent("MeshRenderer")) == null) {
			gameObject.AddComponent("MeshRenderer");
			if ((meshRenderer = (MeshRenderer)gameObject.GetComponent("MeshRenderer")) == null) {
				return null;
			}
		}
		if (material != null) {
			if ((meshRenderer.sharedMaterial == null) || (!materialOnlyIfNotSet)) {
				meshRenderer.sharedMaterial = material;
			}
		}
		if (meta != null) meta.gameObject = gameObject;
		return meshRenderer;
	}
	protected MeshRenderer bindMeshRenderer(MeshMetadata meta, Material material, bool materialOnlyIfNotSet) {
		if ((meta != null) && (meta.gameObject == null)) meta.gameObject = gameObject;
		return bindMeshRenderer(meta, (meta != null) ? meta.gameObject : null, material, materialOnlyIfNotSet);
	}
	protected MeshRenderer bindMeshRenderer(GameObject gameObject, Material material, bool materialOnlyIfNotSet) {
		return bindMeshRenderer(null, gameObject, material, materialOnlyIfNotSet);
	}
	protected MeshRenderer bindMeshRenderer(GameObject gameObject, Material material) {
		return bindMeshRenderer(gameObject, material, false);
	}

	protected MeshRenderer bindMeshRendererFromShaderName(MeshMetadata meta, GameObject gameObject, string shaderName, bool materialOnlyIfNotSet) {
		MeshRenderer meshRenderer;
		if (meshRenderer = bindMeshRenderer(gameObject, null, materialOnlyIfNotSet)) {
			if (meshRenderer != null) {
				if (shaderName != null) {
					if ((meshRenderer.sharedMaterial == null) || (!materialOnlyIfNotSet)) {
						meshRenderer.sharedMaterial = new Material(Shader.Find(shaderName));
					}
				}
				if (meta != null) meta.gameObject = gameObject;
				return meshRenderer;
			}
		}
		return null;
	}
	protected MeshRenderer bindMeshRendererFromShaderName(MeshMetadata meta, string shaderName, bool materialOnlyIfNotSet) {
		if ((meta != null) && (meta.gameObject == null)) meta.gameObject = gameObject;
		return bindMeshRendererFromShaderName(meta, (meta != null) ? meta.gameObject : null, shaderName, materialOnlyIfNotSet);
	}
	protected MeshRenderer bindMeshRendererFromShaderName(GameObject gameObject, string shaderName, bool materialOnlyIfNotSet) {
		return bindMeshRendererFromShaderName(null, gameObject, shaderName, materialOnlyIfNotSet);
	}
	protected MeshRenderer bindMeshRendererFromShaderName(GameObject gameObject, string shaderName) {
		return bindMeshRendererFromShaderName(gameObject, shaderName, false);
	}

	protected Mesh bindMeshInternal(GameObject gameObject) {
		MeshFilter meshFilter;
		if ((meshFilter = (MeshFilter)gameObject.GetComponent("MeshFilter")) == null) {
			gameObject.AddComponent("MeshFilter");
			if ((meshFilter = (MeshFilter)gameObject.GetComponent("MeshFilter")) == null) {
				return null;
			}
		}

		Mesh mesh;
		if ((mesh = meshFilter.sharedMesh) == null) {
			meshFilter.sharedMesh = new Mesh();
			if ((mesh = meshFilter.sharedMesh) == null) {
				return null;
			}
		}

		return mesh;
	}
	protected Mesh bindMesh(MeshMetadata meta, GameObject gameObject, string shaderName, bool materialOnlyIfNotSet) {
		if (gameObject == null) return null;

		bindMeshRendererFromShaderName(meta, gameObject, shaderName, materialOnlyIfNotSet);

		if (meta != null) if (autoReady) meta.ready = true;
		if (meta != null) return meta.mesh = bindMeshInternal(gameObject);
		else return bindMeshInternal(gameObject);
	}
	protected Mesh bindMesh(MeshMetadata meta, GameObject gameObject, Material material, bool materialOnlyIfNotSet) {
		if (gameObject == null) return null;

		if (material != null) bindMeshRenderer(meta, gameObject, material, materialOnlyIfNotSet);
		else bindMeshRendererFromShaderName(gameObject, DEFAULT_SHADER, materialOnlyIfNotSet);

		if (meta != null) if (autoReady) meta.ready = true;
		if (meta != null) return meta.mesh = bindMeshInternal(gameObject);
		else return bindMeshInternal(gameObject);
	}
	protected Mesh bindMesh(MeshMetadata meta, string shaderName, bool materialOnlyIfNotSet) {
		if ((meta != null) && (meta.gameObject == null)) meta.gameObject = gameObject;
		return bindMesh(meta, (meta != null) ? meta.gameObject : null, shaderName, materialOnlyIfNotSet);
	}
	protected Mesh bindMesh(MeshMetadata meta, Material material, bool materialOnlyIfNotSet) {
		if ((meta != null) && (meta.gameObject == null)) meta.gameObject = gameObject;
		return bindMesh(meta, (meta != null) ? meta.gameObject : null, material, materialOnlyIfNotSet);
	}
	protected Mesh bindMesh(MeshMetadata meta, string shaderName) {
		return bindMesh(meta, shaderName, true);
	}
	protected Mesh bindMesh(MeshMetadata meta, Material material) {
		return bindMesh(meta, material, true);
	}
	protected Mesh bindMesh(GameObject gameObject, string shaderName, bool materialOnlyIfNotSet) {
		return bindMesh(null, gameObject, shaderName, materialOnlyIfNotSet);
	}
	protected Mesh bindMesh(GameObject gameObject, Material material, bool materialOnlyIfNotSet) {
		return bindMesh(null, gameObject, material, materialOnlyIfNotSet);
	}
	protected Mesh bindMesh(GameObject gameObject, string shaderName) {
		return bindMesh(gameObject, shaderName, true);
	}
	protected Mesh bindMesh(GameObject gameObject, Material material) {
		return bindMesh(gameObject, material, true);
	}
	protected Mesh bindMesh(GameObject gameObject) {
		return bindMesh(gameObject, DEFAULT_SHADER);
	}

	protected void bindMeshCollider(MeshMetadata meta, GameObject gameObject, Mesh mesh) {
		if (gameObject != null) {
			if (mesh != null) {
				MeshCollider meshCollider;
				if ((meshCollider = (MeshCollider)gameObject.GetComponent("MeshCollider")) == null) {
					gameObject.AddComponent("MeshCollider");
					if ((meshCollider = (MeshCollider)gameObject.GetComponent("MeshCollider")) == null) {
						return;
					}
				}

				meshCollider.sharedMesh = null;
				meshCollider.sharedMesh = mesh;
				meshCollider.convex = false;
				if (meta != null) meta.mesh = mesh;
			}
		}
	}
	protected void bindMeshCollider(GameObject gameObject, Mesh mesh) {
		bindMeshCollider(null, gameObject, mesh);
	}

	protected bool meshDrawClear(Mesh mesh) {
		return meshDrawClear(mesh, defaultMeshMetadata);
	}
	protected bool meshDrawClear(Mesh mesh, MeshMetadata meta) {
		if (autoReady && (meta != null) && (!meta.ready)) {
			bindMesh(meta, DEFAULT_SHADER, true);
		}

		if (meta != null) {
			meta.vertices = null;
			meta.normals = null;
			meta.uvs = null;
			meta.triangles = null;
			meta.usedVertices = 0;
			meta.usedTriangles = 0;
			return true;
		}
		return false;
	}
	protected bool meshDrawClear(MeshMetadata meta) {
		if (autoReady && (meta != null) && (!meta.ready)) {
			bindMesh(meta, DEFAULT_SHADER, true);
		}

		if ((meta != null) && (meta.mesh != null)) return meshDrawClear(meta.mesh, meta);
		return false;
	}

	int internal_lookForVertexNormalUV(MeshMetadata meta, Vector3 v, float threshold, Vector3 n, float nthreshold, Vector2 uv, float uvthreshold) {
		for (int i = 0; i < meta.usedVertices; ++i) if (((v - meta.vertices[i]).magnitude < threshold) && ((n - meta.normals[i]).magnitude < nthreshold) && ((uv - meta.uvs[i]).magnitude < uvthreshold)) return i;
		return -1;
	}
	int internal_lookForVertexUV(MeshMetadata meta, Vector3 v, float threshold, Vector2 uv, float uvthreshold) {
		for (int i = 0; i < meta.usedVertices; ++i) if (((v - meta.vertices[i]).magnitude < threshold) && ((uv - meta.uvs[i]).magnitude < uvthreshold)) return i;
		return -1;
	}
	int internal_lookForVertexNormal(MeshMetadata meta, Vector3 v, float threshold, Vector3 n, float nthreshold) {
		for (int i = 0; i < meta.usedVertices; ++i) if (((v - meta.vertices[i]).magnitude < threshold) && ((n - meta.normals[i]).magnitude < nthreshold)) return i;
		return -1;
	}
	int internal_lookForVertex(MeshMetadata meta, Vector3 v, float threshold) {
		for (int i = 0; i < meta.usedVertices; ++i) if ((v - meta.vertices[i]).magnitude < threshold) return i;
		return -1;
	}

	protected bool meshImmediate(Mesh mesh, bool applyVertices, bool applyNormals, bool applyUV, bool applyTriangles) {
		return meshImmediate(mesh, defaultMeshMetadata, applyVertices, applyNormals, applyUV, applyTriangles);
	}
	protected bool meshImmediate(MeshMetadata meta, bool applyVertices, bool applyNormals, bool applyUV, bool applyTriangles) {
		if ((meta != null) && (meta.mesh != null)) return meshImmediate(meta.mesh, meta, applyVertices, applyNormals, applyUV, applyTriangles);
		return false;
	}
	protected bool meshImmediate(Mesh mesh, MeshMetadata meta, bool applyVertices, bool applyNormals, bool applyUV, bool applyTriangles) {
		return meshImmediate(mesh, meta, true, true, true, true, true);
	}
	protected bool meshImmediate(Mesh mesh, MeshMetadata meta) {
		return meshImmediate(mesh, meta);
	}
	protected bool meshImmediate(MeshMetadata meta) {
		if ((meta != null) && (meta.mesh != null)) return meshImmediate(meta.mesh, meta, true, true, true, true, true);
		return false;
	}
	protected bool meshImmediate() {
		if ((defaultMeshMetadata != null) && (defaultMeshMetadata.mesh != null)) return meshImmediate(defaultMeshMetadata.mesh, defaultMeshMetadata, true, true, true, true, true);
		return false;
	}
	protected bool meshImmediate(Mesh mesh, MeshMetadata meta, bool applyVertices, bool applyNormals, bool applyUV, bool applyTriangles, bool backupTriangles2) {
		if (meta == null) return false;
		bool refreshTriangles = true;

		if (applyTriangles && (meta.triangles != null) && (meta.triangles2 != null) && (meta.usedTriangles2 == meta.usedTriangles)) {
			for (int i = 0; i < meta.usedTriangles; ++i) {
				if (meta.triangles[i] != meta.triangles2[i]) break;
				if (i == meta.usedTriangles - 1) refreshTriangles = false;
			}
		}
		if (refreshTriangles && applyTriangles) {
			mesh.Clear();
		}

		if ((applyVertices) && (meta.vertices != null)) {
			if (meta.vertices.Length > meta.usedVertices) {
				Vector3[] tmpVertices = new Vector3[meta.usedVertices];
				for (int i = 0; i < meta.usedVertices; ++i) tmpVertices[i] = meta.vertices[i];
				meta.vertices = tmpVertices;
			}
			mesh.vertices = meta.vertices;
		}

		if (applyNormals && (meta.normals != null)) {
			if (meta.normals.Length > meta.usedVertices) {
				Vector3[] tmpNormals = new Vector3[meta.usedVertices];
				for (int i = 0; i < meta.usedVertices; ++i) tmpNormals[i] = meta.normals[i];
				meta.normals = tmpNormals;
			}
			mesh.normals = meta.normals;
		}

		if (applyUV && (meta.uvs != null)) {
			if (meta.uvs.Length > meta.usedVertices) {
				Vector2[] tmpUVs = new Vector2[meta.usedVertices];
				for (int i = 0; i < meta.usedVertices; ++i) tmpUVs[i] = meta.uvs[i];
				meta.uvs = tmpUVs;
			}
			mesh.uv = meta.uvs;
		}

		if (refreshTriangles && applyTriangles && (meta.triangles != null)) {
			if (meta.triangles.Length > meta.usedTriangles) {
				int[] tmpTriangles = new int[meta.usedTriangles];
				for (int i = 0; i < meta.usedTriangles; ++i) tmpTriangles[i] = meta.triangles[i];
				meta.triangles = tmpTriangles;
			}
			mesh.triangles = meta.triangles;
		}

		if (refreshTriangles) {
			if (backupTriangles2 && (meta.triangles != null)) {
				meta.triangles2 = meta.triangles;
				meta.usedTriangles2 = meta.usedTriangles;
			} else {
				meta.triangles2 = null;
				meta.usedTriangles2 = 0;
			}
		}

		return true;
	}

	protected bool meshDrawTriangle(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, bool applyNormals, Vector3 n1, Vector3 n2, Vector3 n3, bool applyUVs, Vector2 uv1, Vector2 uv2, Vector2 uv3, float threshold, bool immediate) {
		if (autoReady && (meta != null) && (!meta.ready)) {
			bindMesh(meta, DEFAULT_SHADER, true);
		}

		if (mesh == null) return false;

		if ((v1 - v2).magnitude < threshold) return false;
		if ((v1 - v3).magnitude < threshold) return false;
		if ((v2 - v3).magnitude < threshold) return false;

		int i1, i2, i3, oi = 0;
		if (meta.vertices == null) meta.vertices = new Vector3[immediate ? (meta.usedVertices) : (meta.usedVertices * 2 + 16)];
		if (applyNormals || meta.autogenerateNormals) if (meta.normals == null) meta.normals = new Vector3[immediate ? (meta.usedVertices) : (meta.usedVertices * 2 + 16)];
		if (applyUVs || meta.autogenerateUVs) if (meta.uvs == null) meta.uvs = new Vector2[immediate ? (meta.usedVertices) : (meta.usedVertices * 2 + 16)];

		if (applyNormals && applyUVs && meta.checkNormals && meta.checkUVs) {
			i1 = internal_lookForVertexNormalUV(meta, v1, threshold, n1, threshold, uv1, threshold);
			i2 = internal_lookForVertexNormalUV(meta, v2, threshold, n2, threshold, uv2, threshold);
			i3 = internal_lookForVertexNormalUV(meta, v3, threshold, n3, threshold, uv3, threshold);
		} else if (applyNormals && meta.checkNormals) {
			i1 = internal_lookForVertexNormal(meta, v1, threshold, n1, threshold);
			i2 = internal_lookForVertexNormal(meta, v2, threshold, n2, threshold);
			i3 = internal_lookForVertexNormal(meta, v3, threshold, n3, threshold);
		} else if (applyUVs && meta.checkUVs) {
			i1 = internal_lookForVertexUV(meta, v1, threshold, uv1, threshold);
			i2 = internal_lookForVertexUV(meta, v2, threshold, uv2, threshold);
			i3 = internal_lookForVertexUV(meta, v3, threshold, uv3, threshold);
		} else {
			i1 = internal_lookForVertex(meta, v1, threshold);
			i2 = internal_lookForVertex(meta, v2, threshold);
			i3 = internal_lookForVertex(meta, v3, threshold);
		}
		if (meta.autogenerateNormals) {
			n1 = n2 = n3 = Vector3.Cross(v2 - v1, v3 - v1).normalized;
		}

		if (meta.autogenerateUVs && (!applyUVs)) {

			uv1 = new Vector2(autouv_bias_to_u + (v1.x * autouv_x + autouv_x_bias) * autouv_x_to_u + (v1.y * autouv_y + autouv_y_bias) * autouv_y_to_u + (v1.z * autouv_z + autouv_z_bias) * autouv_z_to_u, autouv_bias_to_v + (v1.x * autouv_x + autouv_x_bias) * autouv_x_to_v + (v1.y * autouv_y + autouv_y_bias) * autouv_y_to_v + (v1.z * autouv_z + autouv_z_bias) * autouv_z_to_v);
			uv2 = new Vector2(autouv_bias_to_u + (v2.x * autouv_x + autouv_x_bias) * autouv_x_to_u + (v2.y * autouv_y + autouv_y_bias) * autouv_y_to_u + (v2.z * autouv_z + autouv_z_bias) * autouv_z_to_u, autouv_bias_to_v + (v2.x * autouv_x + autouv_x_bias) * autouv_x_to_v + (v2.y * autouv_y + autouv_y_bias) * autouv_y_to_v + (v2.z * autouv_z + autouv_z_bias) * autouv_z_to_v);
			uv3 = new Vector2(autouv_bias_to_u + (v3.x * autouv_x + autouv_x_bias) * autouv_x_to_u + (v3.y * autouv_y + autouv_y_bias) * autouv_y_to_u + (v3.z * autouv_z + autouv_z_bias) * autouv_z_to_u, autouv_bias_to_v + (v3.x * autouv_x + autouv_x_bias) * autouv_x_to_v + (v3.y * autouv_y + autouv_y_bias) * autouv_y_to_v + (v3.z * autouv_z + autouv_z_bias) * autouv_z_to_v);
			++autouv_log_count; if (autouv_log_count % autouv_log_rate == 0) {
			}
		}

		if (i1 < 0) ++oi;
		if (i2 < 0) ++oi;
		if (i3 < 0) ++oi;
		if (oi > 0) {
			if (meta.vertices.Length < meta.usedVertices + oi) {
				Vector3[] tmpVertices = new Vector3[immediate ? (meta.usedVertices + oi) : (meta.usedVertices * 2 + 16)];
				for (int i = 0; i < meta.usedVertices; ++i) tmpVertices[i] = meta.vertices[i];
				meta.vertices = tmpVertices;
				if (applyNormals || meta.autogenerateNormals) {
					Vector3[] tmpNormals = new Vector3[immediate ? (meta.usedVertices + oi) : (meta.usedVertices * 2 + 16)];
					for (int i = 0; i < meta.usedVertices; ++i) tmpNormals[i] = meta.normals[i];
					meta.normals = tmpNormals;
				}
				if (applyUVs || meta.autogenerateUVs) {
					Vector2[] tmpUVs = new Vector2[immediate ? (meta.usedVertices + oi) : (meta.usedVertices * 2 + 16)];
					for (int i = 0; i < meta.usedVertices; ++i) tmpUVs[i] = meta.uvs[i];
					meta.uvs = tmpUVs;
				}
			}
		}

		if (i1 < 0) {
			if (autoxyz_transform) {
				meta.vertices[i1 = (meta.usedVertices++)] = v1.x * autoxyz_right + v1.y * autoxyz_up + v1.z * autoxyz_forward;
				if (applyNormals || meta.autogenerateNormals) meta.normals[i1] = n1.x * autoxyz_right + n1.y * autoxyz_up + n1.z * autoxyz_forward;
			} else {
				meta.vertices[i1 = (meta.usedVertices++)] = v1;
				if (applyNormals || meta.autogenerateNormals) meta.normals[i1] = n1;
			}
			if (applyUVs || meta.autogenerateUVs) meta.uvs[i1] = uv1;
		}
		if (i2 < 0) {
			if (autoxyz_transform) {
				meta.vertices[i2 = (meta.usedVertices++)] = v2.x * autoxyz_right + v2.y * autoxyz_up + v2.z * autoxyz_forward;
				if (applyNormals || meta.autogenerateNormals) meta.normals[i2] = n2.x * autoxyz_right + n2.y * autoxyz_up + n2.z * autoxyz_forward;
			} else {
				meta.vertices[i2 = (meta.usedVertices++)] = v2;
				if (applyNormals || meta.autogenerateNormals) meta.normals[i2] = n2;
			}
			if (applyUVs || meta.autogenerateUVs) meta.uvs[i2] = uv2;
		}
		if (i3 < 0) {
			if (autoxyz_transform) {
				meta.vertices[i3 = (meta.usedVertices++)] = v3.x * autoxyz_right + v3.y * autoxyz_up + v3.z * autoxyz_forward;
				if (applyNormals || meta.autogenerateNormals) meta.normals[i3] = n3.x * autoxyz_right + n3.y * autoxyz_up + n3.z * autoxyz_forward;
			} else {
				meta.vertices[i3 = (meta.usedVertices++)] = v3;
				if (applyNormals || meta.autogenerateNormals) meta.normals[i3] = n3;
			}
			if (applyUVs || meta.autogenerateUVs) meta.uvs[i3] = uv3;
		}

		if (meta.triangles == null) meta.triangles = new int[immediate ? (meta.usedTriangles) : (meta.usedTriangles * 3 + 18)];
		if (meta.triangles.Length < meta.usedTriangles + 3) {
			int[] tmpTriangles = new int[immediate ? (meta.usedTriangles + 3) : (meta.usedTriangles * 3 + 18)];
			for (int i = 0; i < meta.usedTriangles; ++i) tmpTriangles[i] = meta.triangles[i];
			meta.triangles = tmpTriangles;
		}
		meta.triangles[meta.usedTriangles++] = i1;
		meta.triangles[meta.usedTriangles++] = i2;
		meta.triangles[meta.usedTriangles++] = i3;

		if (immediate) return meshImmediate(mesh, meta, true, applyNormals || meta.autogenerateNormals, applyUVs || meta.autogenerateUVs, true);
		else return true;
	}
	protected bool meshDrawTriangle(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, float threshold, bool immediate) {
		return meshDrawTriangle(mesh, meta, v1, v2, v3, false, Vector3.zero, Vector3.zero, Vector3.zero, false, Vector2.zero, Vector2.zero, Vector2.zero, threshold, immediate);
	}
	protected bool meshDrawTriangle(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n, float threshold, bool immediate) {
		return meshDrawTriangle(mesh, meta, v1, v2, v3, true, n, n, n, false, Vector2.zero, Vector2.zero, Vector2.zero, threshold, immediate);
	}
	protected bool meshDrawTriangle(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 n, Vector2 uv1, Vector2 uv2, Vector2 uv3, float threshold, bool immediate) {
		return meshDrawTriangle(mesh, meta, v1, v2, v3, true, n, n, n, true, uv1, uv2, uv3, threshold, immediate);
	}
	protected bool meshDrawTriangle(Vector3 ofs, Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, float threshold, bool immediate) {
		return meshDrawTriangle(mesh, meta, v1 + ofs, v2 + ofs, v3 + ofs, threshold, immediate);
	}
	protected bool meshDrawTriangleAutoNormal(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, float threshold, bool immediate) {
		Vector3 normal;
		normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
		return meshDrawTriangle(mesh, meta, v1, v2, v3, normal, threshold, immediate);
	}
	protected bool meshDrawTriangle(MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, bool immediate) {
		if (meta == null) meta = defaultMeshMetadata;
		Vector3 normal;
		normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
		return meshDrawTriangle(meta.mesh, meta, v1, v2, v3, normal, DEFAULT_THRESHOLD, immediate);
	}
	protected bool meshDrawTriangle(MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3) {
		return meshDrawTriangle(meta, v1, v2, v3, true);
	}
	protected bool meshDrawTriangleAutoNormalUVs(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3, float threshold, bool immediate) {
		Vector3 normal;
		normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
		return meshDrawTriangle(mesh, meta, v1, v2, v3, normal, uv1, uv2, uv3, threshold, immediate);
	}
	protected bool meshDrawTriangleAutoNormalReverse(Mesh mesh, Vector3 v1, Vector3 v2, Vector3 v3, float threshold, bool immediate) {
		return meshDrawTriangleAutoNormalReverse(mesh, defaultMeshMetadata, v1, v2, v3, threshold, immediate);
	}
	protected bool meshDrawTriangleAutoNormalReverse(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, float threshold, bool immediate) {
		return meshDrawTriangleAutoNormal(mesh, meta, v1, v3, v2, threshold, immediate);
	}
	protected bool meshDrawTriangleAutoNormalReverseUVs(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3, float threshold, bool immediate) {
		return meshDrawTriangleAutoNormalUVs(mesh, meta, v1, v3, v2, uv1, uv2, uv3, threshold, immediate);
	}
	protected bool meshDrawTriangleAutoNormal(Vector3 ofs, Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, float threshold, bool immediate) {
		return meshDrawTriangleAutoNormal(mesh, meta, v1 + ofs, v2 + ofs, v3 + ofs, threshold, immediate);
	}

	protected bool meshDrawQuad(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, bool applyNormals, Vector3 n1, Vector3 n2, Vector3 n3, Vector3 n4, bool applyUVs, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, float threshold, bool immediate) {
		return
			meshDrawTriangle(mesh, meta, v1, v2, v3, applyNormals, n1, n2, n3, applyUVs, uv1, uv2, uv3, threshold, false) &&
			meshDrawTriangle(mesh, meta, v1, v3, v4, applyNormals, n1, n3, n4, applyUVs, uv1, uv3, uv4, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormal(Mesh mesh, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float threshold, bool immediate) {
		return meshDrawQuadAutoNormal(mesh, defaultMeshMetadata, v1, v2, v3, v4, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormal(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float threshold, bool immediate) {
		Vector3 normal;
		normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
		return meshDrawQuad(mesh, meta, v1, v2, v3, v4, true, normal, normal, normal, normal, false, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormalUVs(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, float threshold, bool immediate) {
		Vector3 normal;
		normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
		return meshDrawQuad(mesh, meta, v1, v2, v3, v4, true, normal, normal, normal, normal, true, uv1, uv2, uv3, uv4, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormal(Vector3 ofs, Mesh mesh, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float threshold, bool immediate) {
		return meshDrawQuadAutoNormal(ofs, mesh, defaultMeshMetadata, v1, v2, v3, v4, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormal(Vector3 ofs, Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float threshold, bool immediate) {
		return meshDrawQuadAutoNormal(mesh, meta, v1 + ofs, v2 + ofs, v3 + ofs, v4 + ofs, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormalReverse(Mesh mesh, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float threshold, bool immediate) {
		return meshDrawQuadAutoNormalReverse(mesh, defaultMeshMetadata, v1, v2, v3, v4, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormalReverse(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float threshold, bool immediate) {
		return meshDrawQuadAutoNormal(mesh, meta, v1, v4, v3, v2, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormalReverseUVs(Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, float threshold, bool immediate) {
		return meshDrawQuadAutoNormalUVs(mesh, meta, v1, v4, v3, v2, uv1, uv4, uv3, uv2, threshold, immediate);
	}
	protected bool meshDrawQuadAutoNormalReverse(Vector3 ofs, Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float threshold, bool immediate) {
		return meshDrawQuadAutoNormalReverse(mesh, meta, v1 + ofs, v2 + ofs, v3 + ofs, v4 + ofs, threshold, immediate);
	}
	protected bool meshDrawQuad(Vector3 ofs, Mesh mesh, MeshMetadata meta, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float threshold, bool immediate) {
		return meshDrawQuad(mesh, meta, v1 + ofs, v2 + ofs, v3 + ofs, v4 + ofs, false, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, false, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, threshold, immediate);
	}

	protected bool meshDrawSolid_icosahedron(Mesh mesh, MeshMetadata meta, Vector3 center, float radius, bool applyAxes, Vector3 leftaxis, Vector3 upaxis, Vector3 forwardaxis, bool applyNormals, bool applyUVs, float threshold, bool immediate) {
		if (meta == null) return false;
		float angle, angle5, angle10, angle15, angleu, angled;
		angleu = 30f * Mathf.PI / 180f;
		angled = -30f * Mathf.PI / 180f;
		for (int iangle = 0; iangle < 5; ++iangle) {
			angle = (iangle + 0.0f) / 5f * 2f * Mathf.PI;
			angle5 = (iangle + 0.5f) / 5f * 2f * Mathf.PI;
			angle10 = (iangle + 1.0f) / 5f * 2f * Mathf.PI;
			angle15 = (iangle + 1.5f) / 5f * 2f * Mathf.PI;
			meshDrawTriangle(meta, new Vector3(center.x + 0f,                          center.y + radius,                     center.z),                               new Vector3(center.x + radius * Mathf.Cos(angle10), center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle10)), new Vector3(center.x + radius * Mathf.Cos(angle),   center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle)),   false);
			meshDrawTriangle(meta, new Vector3(center.x + radius * Mathf.Cos(angle5),  center.y + radius * Mathf.Sin(angled), center.z + radius * Mathf.Sin(angle5)),  new Vector3(center.x + radius * Mathf.Cos(angle),   center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle)),   new Vector3(center.x + radius * Mathf.Cos(angle10), center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle10)), false);
			meshDrawTriangle(meta, new Vector3(center.x + radius * Mathf.Cos(angle10), center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle10)), new Vector3(center.x + radius * Mathf.Cos(angle15), center.y + radius * Mathf.Sin(angled), center.z + radius * Mathf.Sin(angle15)), new Vector3(center.x + radius * Mathf.Cos(angle5),  center.y + radius * Mathf.Sin(angled), center.z + radius * Mathf.Sin(angle5)),  false);
			meshDrawTriangle(meta, new Vector3(center.x + 0f,                          center.y - radius,                     center.z),                               new Vector3(center.x + radius * Mathf.Cos(angle5),  center.y + radius * Mathf.Sin(angled), center.z + radius * Mathf.Sin(angle5)),  new Vector3(center.x + radius * Mathf.Cos(angle15), center.y + radius * Mathf.Sin(angled), center.z + radius * Mathf.Sin(angle15)), false);
		}
		if (immediate) return meshImmediate(mesh, meta, true, applyNormals || meta.autogenerateNormals, applyUVs || meta.autogenerateUVs, true);
		else return true;
	}
	protected bool meshDrawSolid_icosahedron(MeshMetadata meta, Vector3 center, float radius) {
		if (meta == null) meta = defaultMeshMetadata;
		return meshDrawSolid_icosahedron(meta.mesh, meta, center, radius, false, Vector3.left, Vector3.up, Vector3.forward, false, false, DEFAULT_THRESHOLD, true);
	}

	protected bool meshDrawSolid_octahedron(Mesh mesh, MeshMetadata meta, Vector3 center, float radius, bool applyAxes, Vector3 leftaxis, Vector3 upaxis, Vector3 forwardaxis, bool applyNormals, bool applyUVs, float threshold, bool immediate) {
		if (meta == null) return false;
		float angle, angle10, angleu, angled;
		angleu = 90f * Mathf.PI / 180f;
		angled = -90f * Mathf.PI / 180f;
		for (int iangle = 0; iangle < 4; ++iangle) {
			angle = (iangle + 0.0f) / 4f * 2f * Mathf.PI;
			angle10 = (iangle + 1.0f) / 4f * 2f * Mathf.PI;
			meshDrawTriangle(meta, new Vector3(center.x + 0f, center.y + radius * Mathf.Sin(angleu), center.z), new Vector3(center.x + radius * Mathf.Cos(angle10), center.y, center.z + radius * Mathf.Sin(angle10)), new Vector3(center.x + radius * Mathf.Cos(angle),   center.y, center.z + radius * Mathf.Sin(angle)),   false);
			meshDrawTriangle(meta, new Vector3(center.x + 0f, center.y + radius * Mathf.Sin(angled), center.z), new Vector3(center.x + radius * Mathf.Cos(angle),   center.y, center.z + radius * Mathf.Sin(angle)),   new Vector3(center.x + radius * Mathf.Cos(angle10), center.y, center.z + radius * Mathf.Sin(angle10)), false);
		}
		if (immediate) return meshImmediate(mesh, meta, true, applyNormals || meta.autogenerateNormals, applyUVs || meta.autogenerateUVs, true);
		else return true;
	}
	protected bool meshDrawSolid_octahedron(MeshMetadata meta, Vector3 center, float radius) {
		if (meta == null) meta = defaultMeshMetadata;
		return meshDrawSolid_octahedron(meta.mesh, meta, center, radius, false, Vector3.left, Vector3.up, Vector3.forward, false, false, DEFAULT_THRESHOLD, true);
	}

	protected bool meshDrawSolid_tetrahedron(Mesh mesh, MeshMetadata meta, Vector3 center, float radius, bool applyAxes, Vector3 leftaxis, Vector3 upaxis, Vector3 forwardaxis, bool applyNormals, bool applyUVs, float threshold, bool immediate) {
		if (meta == null) return false;
		float angle, angle10, angle20, angleu, angled;
		angleu = -30f * Mathf.PI / 180f;
		angled = 90f * Mathf.PI / 180f;
		for (int iangle = 0; iangle < 3; ++iangle) {
			angle = (iangle + 0.0f) / 3f * 2f * Mathf.PI;
			angle10 = (iangle + 1.0f) / 3f * 2f * Mathf.PI;
			meshDrawTriangle(meta, new Vector3(center.x + 0f, center.y + radius * Mathf.Sin(angled), center.z), new Vector3(center.x + radius * Mathf.Cos(angle10), center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle10)), new Vector3(center.x + radius * Mathf.Cos(angle), center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle)),   false);
		}
		angle = (0.0f) / 3f * 2f * Mathf.PI;
		angle10 = (1.0f) / 3f * 2f * Mathf.PI;
		angle20 = (2.0f) / 3f * 2f * Mathf.PI;
		meshDrawTriangle(
			meta,
			new Vector3(center.x + radius * Mathf.Cos(angle),   center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle)),
			new Vector3(center.x + radius * Mathf.Cos(angle10), center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle10)),
			new Vector3(center.x + radius * Mathf.Cos(angle20), center.y + radius * Mathf.Sin(angleu), center.z + radius * Mathf.Sin(angle20)),
			false
		);
		if (immediate) return meshImmediate(mesh, meta, true, applyNormals || meta.autogenerateNormals, applyUVs || meta.autogenerateUVs, true);
		else return true;
	}
	protected bool meshDrawSolid_tetrahedron(MeshMetadata meta, Vector3 center, float radius) {
		if (meta == null) meta = defaultMeshMetadata;
		return meshDrawSolid_tetrahedron(meta.mesh, meta, center, radius, false, Vector3.left, Vector3.up, Vector3.forward, false, false, DEFAULT_THRESHOLD, true);
	}

	protected bool meshDrawSolid_cube(Mesh mesh, MeshMetadata meta, Vector3 center, float side, bool applyAxes, Vector3 leftaxis, Vector3 upaxis, Vector3 forwardaxis, bool applyNormals, bool applyUVs, float threshold, bool immediate) {
		if (meta == null) return false;
		float s = side / 2f;
		float x = center.x;
		float y = center.y;
		float z = center.z;

		meshDrawTriangle(meta, new Vector3(x - s, y - s, z - s), new Vector3(x + s, y - s, z - s), new Vector3(x + s, y - s, z + s), false);
		meshDrawTriangle(meta, new Vector3(x + s, y - s, z + s), new Vector3(x - s, y - s, z + s), new Vector3(x - s, y - s, z - s), false);

		meshDrawTriangle(meta, new Vector3(x + s, y + s, z - s), new Vector3(x + s, y - s, z - s), new Vector3(x - s, y - s, z - s), false);
		meshDrawTriangle(meta, new Vector3(x - s, y - s, z - s), new Vector3(x - s, y + s, z - s), new Vector3(x + s, y + s, z - s), false);

		meshDrawTriangle(meta, new Vector3(x - s, y - s, z + s), new Vector3(x + s, y - s, z + s), new Vector3(x + s, y + s, z + s), false);
		meshDrawTriangle(meta, new Vector3(x + s, y + s, z + s), new Vector3(x - s, y + s, z + s), new Vector3(x - s, y - s, z + s), false);

		meshDrawTriangle(meta, new Vector3(x + s, y - s, z - s), new Vector3(x + s, y + s, z - s), new Vector3(x + s, y + s, z + s), false);
		meshDrawTriangle(meta, new Vector3(x + s, y + s, z + s), new Vector3(x + s, y - s, z + s), new Vector3(x + s, y - s, z - s), false);

		meshDrawTriangle(meta, new Vector3(x - s, y + s, z + s), new Vector3(x - s, y + s, z - s), new Vector3(x - s, y - s, z - s), false);
		meshDrawTriangle(meta, new Vector3(x - s, y - s, z - s), new Vector3(x - s, y - s, z + s), new Vector3(x - s, y + s, z + s), false);

		meshDrawTriangle(meta, new Vector3(x + s, y + s, z + s), new Vector3(x + s, y + s, z - s), new Vector3(x - s, y + s, z - s), false);
		meshDrawTriangle(meta, new Vector3(x - s, y + s, z - s), new Vector3(x - s, y + s, z + s), new Vector3(x + s, y + s, z + s), false);

		if (immediate) return meshImmediate(mesh, meta, true, applyNormals || meta.autogenerateNormals, applyUVs || meta.autogenerateUVs, true);
		else return true;
	}
	protected bool meshDrawSolid_cube(MeshMetadata meta, Vector3 center, float side) {
		if (meta == null) meta = defaultMeshMetadata;
		return meshDrawSolid_cube(meta.mesh, meta, center, side, false, Vector3.left, Vector3.up, Vector3.forward, false, false, DEFAULT_THRESHOLD, true);
	}

	protected bool meshDrawSolid_hexahedron(Mesh mesh, MeshMetadata meta, Vector3 center, float side, bool applyAxes, Vector3 leftaxis, Vector3 upaxis, Vector3 forwardaxis, bool applyNormals, bool applyUVs, float threshold, bool immediate) {
		if (meta == null) meta = defaultMeshMetadata;
		return meshDrawSolid_cube(mesh, meta, center, side / Mathf.Sqrt(2f), applyAxes, leftaxis, upaxis, forwardaxis, applyNormals, applyUVs, threshold, immediate);
	}
	protected bool meshDrawSolid_hexahedron(MeshMetadata meta, Vector3 center, float side) {
		if (meta == null) meta = defaultMeshMetadata;
		return meshDrawSolid_cube(meta, center, side / Mathf.Sqrt(2f));
	}

}

