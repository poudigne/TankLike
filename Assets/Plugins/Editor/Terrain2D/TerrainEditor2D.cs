//
// Gargore TERRAIN 2D (standard edition, version 0.1)
//

//
// IMPORTANT NOTICE: THIS FILE SHOULD NOT BE EDITED, IF YOU REALLY NEED TO
//                   MODIFY IT, CREATE A SUBCLASS WITH EXTEND (CHECK THE MANUAL)

using UnityEngine;
using UnityEditor;
using System.Collections;

// WARNING WARNING! THIS FILE IS STILL IN BETA. USE WITH CAUTION.

[CanEditMultipleObjects]
[CustomEditor(typeof(Terrain2D))]public class TerrainEditor2D: Editor {
	static private int usedEvents = 0;
	private Vector3 cursorPos = Vector3.zero;
	static private Vector3[] boundPoints = null;
	private Vector3[] cursorPoints = null;
	static private Vector3[] outlinePoints = null;
	private int cursorPointsCount = 20;
	static private Terrain2D lastSelected = null;
	static private float lastSelectedTime = 0f;
	static private float lastSelectedTimeLapse = 1f;
	static private float lastRedrawTime = 0f;
	static private float lastRedrawTimeLapse = 0.25f;
	static private float lastIdleRedrawTime = 0f;
	static private float lastIdleRedrawTimeLapse = 5f;
	static private float lastForceLoadSerializeTime = 0f;
	static private float lastForceLoadSerializeTimeLapse = 1f;
	static private bool mouseDown = false;
	static private Terrain2D.TerrainLayer2D lastEditorLayer = Terrain2D.TerrainLayer2D.master;
	
	static private void drawTerrain2D(GameObject gameObject, Terrain2D terrain2D) {
		if (!terrain2D.isRunning()) {
			
			// EN PRIMER LUGAR HAY QUE DETECTAR SI DATA ESTA GENERADO E INICIALIZAR lastDataWidth y lastDataHeight
			terrain2D.outlineLayerEditorCheck();
			
			if (terrain2D.saveForceLoadSerialize || (terrain2D.saveAutoLoadSerialize && (!terrain2D.alreadyLoadedLayer("layer." + terrain2D.saveRecordName + ".txt")))) {
				if (terrain2D.saveForceLoadSerialize) {
					terrain2D.saveForceLoadSerialize = false;
					if (Time.realtimeSinceStartup <= lastForceLoadSerializeTime + lastForceLoadSerializeTimeLapse) terrain2D.dataVersion = 0;
					lastForceLoadSerializeTime = Time.realtimeSinceStartup;
				}
				terrain2D.loadLayer("layer." + terrain2D.saveRecordName + ".txt");
				terrain2D.outlineLayer();
				lastIdleRedrawTime = Time.realtimeSinceStartup;
				lastEditorLayer = terrain2D.editorLayer;
			}

			if (terrain2D.saveForceDumpSerialize) {
				terrain2D.saveForceDumpSerialize = false;
				terrain2D.dumpLayer();
			}
			Terrain2D terrain2Dbounds = (terrain2D.layerWaterReference != null) ? terrain2D.layerWaterReference : terrain2D;
			float width = terrain2Dbounds.dataWidth * terrain2Dbounds.scale.x;
			float height = terrain2Dbounds.dataHeight * terrain2Dbounds.scale.y;
			if (width <= 0f) width = 1f;
			if (height <= 0f) height = 1f;

			if ((boundPoints == null) || (boundPoints.Length != 5)) boundPoints = new Vector3[5];
			boundPoints[0] = gameObject.transform.position + gameObject.transform.right * 0f + gameObject.transform.up * 0f;
			boundPoints[1] = gameObject.transform.position + gameObject.transform.right * width + gameObject.transform.up * 0f;
			boundPoints[2] = gameObject.transform.position + gameObject.transform.right * width + gameObject.transform.up * height;
			boundPoints[3] = gameObject.transform.position + gameObject.transform.right * 0f + gameObject.transform.up * height;
			boundPoints[4] = boundPoints[0];
			Handles.DrawPolyLine(boundPoints);

			if ((terrain2D.editorLayer != lastEditorLayer) || (Time.realtimeSinceStartup > lastIdleRedrawTime + lastIdleRedrawTimeLapse)) {
				terrain2D.outlineLayer();
				lastIdleRedrawTime = Time.realtimeSinceStartup;
				lastEditorLayer = terrain2D.editorLayer;
			}

			int ii;
			ii = 0; while ((ii < 9999) && ((outlinePoints = terrain2D.outlineLayerShape(ii)) != null)) {
				Handles.DrawPolyLine(outlinePoints);
				++ii;
			}
		}
	}

	private void editTerrain2D(GameObject gameObject, Terrain2D terrain2D) {
		if (!terrain2D.isRunning()) {
			if (terrain2D.editorEnabled) {
				Event e = Event.current;
				if ((!terrain2D.editorWasEnabled) && (Tools.current != Tool.None)) {
					Tools.current = Tool.None;
				}
				terrain2D.editorWasEnabled = true;
				if ((Tools.current == Tool.None) && ((e.modifiers & EventModifiers.Alt) == 0)) {

					if ((Event.current.type == EventType.MouseDown) && (Event.current.button == 0)) {
						mouseDown = true;
						terrain2D.editOutlineLayerBegin();
						++usedEvents;
						Event.current.Use();
					}


					if (e.type == EventType.Layout) {
						HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive)); //somehow this allows e.Use() to actually function and block mouse input
					}
					Vector2 mousePosition = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);
					Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
					Plane plane = new Plane(gameObject.transform.position, gameObject.transform.position + gameObject.transform.right, gameObject.transform.position + gameObject.transform.up);
					float dist;
					if (plane.Raycast(ray, out dist)) cursorPos = ray.origin + ray.direction * dist;
					else cursorPos = gameObject.transform.position;

					Vector3 pos = cursorPos - terrain2D.transform.position;
					int ic = Mathf.FloorToInt(Vector3.Dot(pos, terrain2D.transform.right) / terrain2D.scale.x);
					int jc = Mathf.FloorToInt(Vector3.Dot(pos, terrain2D.transform.up) / terrain2D.scale.y);
					

					if ((cursorPoints == null) || (cursorPoints.Length != cursorPointsCount)) cursorPoints = new Vector3[cursorPointsCount];
					float dx;
					float dy;
					float brushsize = terrain2D.blockPlotBrushsize(terrain2D.editorBrushSize, dist, terrain2D.editorBrushType);
					for (int i = 0; i < cursorPointsCount; ++i) {
						dx = brushsize * Mathf.Cos(2f * Mathf.PI * i / (cursorPointsCount - 1f));
						dy = brushsize * Mathf.Sin(2f * Mathf.PI * i / (cursorPointsCount - 1f));
						cursorPoints[i] = new Vector3(cursorPos.x + dx, cursorPos.y + dy, cursorPos.z);
					}
					Handles.DrawPolyLine(cursorPoints);
					
					if ((Event.current.type == EventType.MouseDown) && (Event.current.button == 0)) {
						mouseDown = true;
						terrain2D.editOutlineLayerBegin();
						lastRedrawTime = Time.realtimeSinceStartup;
					}
					if ((Event.current.type == EventType.MouseUp) && (Event.current.button == 0)) {
						mouseDown = false;
						terrain2D.editOutlineLayerEnd();
						lastRedrawTime = Time.realtimeSinceStartup;
					}

					if (mouseDown) {
						if ((e.modifiers & EventModifiers.Shift) == 0) {
							terrain2D.editOutlineLayer(ic, jc, false, dist);
						} else {
							terrain2D.editOutlineLayer(ic, jc, true, dist);
						}
						if (Time.realtimeSinceStartup > lastRedrawTime + lastRedrawTimeLapse) {
							terrain2D.editOutlineLayerRedraw();
							lastRedrawTime = Time.realtimeSinceStartup;
							lastIdleRedrawTime = Time.realtimeSinceStartup;
						}
						SceneView.RepaintAll();
					}

				} else if (Tools.current != Tool.None) {
					terrain2D.editorEnabled = false;
				}
			} else {
				terrain2D.editorWasEnabled = false;
			}
		}
	}

	[DrawGizmo(GizmoType.NotSelected)]static void RenderCustomGizmo(Transform objectTransform, GizmoType gizmoType) {
		if ((objectTransform != null) && (objectTransform.gameObject != null)) {
			Terrain2D terrain2D;
			if ((terrain2D = (Terrain2D)objectTransform.gameObject.GetComponent("Terrain2D")) != null) {
				if ((terrain2D == lastSelected) && (lastSelectedTime + lastSelectedTimeLapse > Time.realtimeSinceStartup)) Handles.color = Color.white;
				else Handles.color = Color.grey;
				drawTerrain2D(objectTransform.gameObject, terrain2D);
			}
		}
	}

	private int OnSceneGUI_count = 0;
	void OnSceneGUI() {
		++OnSceneGUI_count;
		GameObject selectedGameObject;
		if ((selectedGameObject = Selection.activeGameObject) != null) {
			Terrain2D terrain2D;
			if ((terrain2D = (Terrain2D)selectedGameObject.GetComponent("Terrain2D")) != null) {
				lastSelected = terrain2D;
				lastSelectedTime = Time.realtimeSinceStartup;
				Handles.color = Color.white;
				drawTerrain2D(selectedGameObject, terrain2D);
				editTerrain2D(selectedGameObject, terrain2D);
			}
		}
	}
}

