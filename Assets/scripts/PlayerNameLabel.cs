using UnityEngine;
using System.Collections;

public class PlayerNameLabel : MonoBehaviour 
{
	private Rect rect;
	[SerializeField]private Vector2 offset;

  [SerializeField]private string playerName;

  #region Unity Engine
  void Awake()
	{
		rect = new Rect(0,0,300,100);
		offset = new Vector2(0.0f, -0.35f);
	}

	void OnGUI()
	{
		var style = new GUIStyle();
		var uiContent = new GUIContent(playerName);
		var sizeOfLabel = style.CalcSize(uiContent);
		rect = new Rect(0,0,sizeOfLabel.x, sizeOfLabel.y*2);
    var tankPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
		var point = Camera.main.WorldToScreenPoint(tankPos + offset);
		rect.x = point.x - sizeOfLabel.x / 2;
		rect.y = Screen.height - point.y - rect.height;
		GUI.Label(rect, playerName);
  }
  #endregion


  public void SetName(string name)
  {
    playerName = name;
  }
}
