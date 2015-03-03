using UnityEngine;
using System.Collections;

public class PlayerNameLabel : MonoBehaviour 
{
	private Rect rect;
	[SerializeField]private Vector2 offset;

	public string playerName;

	void Awake()
	{
		rect = new Rect(0,0,300,100);
		offset = new Vector2(0.0f, -0.35f);
	}

	void OnGUI()
	{
		GUIStyle style = new GUIStyle();
		GUIContent uiContent = new GUIContent(playerName);
		Vector2 sizeOfLabel = style.CalcSize(uiContent);
		rect = new Rect(0,0,sizeOfLabel.x, sizeOfLabel.y*2);
    Vector2 tankPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
		Vector3 point = Camera.main.WorldToScreenPoint(tankPos + offset);
		rect.x = point.x - sizeOfLabel.x / 2;
		rect.y = Screen.height - point.y - rect.height;
		GUI.Label(rect, playerName);
	}
}
