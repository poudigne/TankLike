using UnityEngine;
using System.Collections;

public class PlayerNameLabel : MonoBehaviour 
{
	[SerializeField] private Vector2 offset;
  [SerializeField] private Vector2 hpBaroffset;
  
  [SerializeField] private string playerName;
  
  [SerializeField] private int healthBarLeft = 110;
  [SerializeField] private int barTop = 1;
  [SerializeField] private float healthBarLength = 150.0f;
  [SerializeField] private float healthBarHeight = 5;

  private PlayerInfo playerInfo;
  private GUIStyle myStyle;

  #region Unity Engine
  void Awake()
	{
		offset = new Vector2(0.0f, -0.35f);

    myStyle = new GUIStyle();
    myStyle.normal.textColor = Color.green;
    myStyle.fontSize = 12;
    myStyle.fontStyle = FontStyle.Bold;
    myStyle.clipping = TextClipping.Overflow;

    playerInfo = transform.GetComponent<PlayerInfo>();
    if (playerInfo == null)
      Debug.LogError("PlayerInfo component not found. Please add one");
	}

	void OnGUI()
	{
	  DrawNameLabel();
	  DrawHpBar();
	}

  #endregion
  #region UI Draw 
  // This draw the player's name label under the tank
  private void DrawNameLabel()
  {
    GUIStyle style = new GUIStyle();
    GUIContent uiContent = new GUIContent(playerName);

    Vector2 sizeOfLabel = style.CalcSize(uiContent);
    Vector3 screenPosition = Camera.main.WorldToScreenPoint(GetTankPosition() + offset);

    Rect rect = new Rect(0, 0, sizeOfLabel.x, sizeOfLabel.y * 2);
    rect.x = screenPosition.x - sizeOfLabel.x / 2;
    rect.y = Screen.height - screenPosition.y - rect.height;
    GUI.Label(rect, playerName);
  }

  // This draw the player's HP under the label name
  private void DrawHpBar()
  {
    Vector3 screenPosition = Camera.main.WorldToScreenPoint( GetTankPosition() + offset);
    GUI.color = Color.red;
    GUI.HorizontalScrollbar(new Rect(screenPosition.x - healthBarLeft / 2, Screen.height - screenPosition.y - barTop, 100, 0), 0, playerInfo.health, 0, playerInfo.maxHealth); //displays a healthbar
       
    GUI.color = Color.white;
    GUI.contentColor = Color.white;
    GUI.Label(new Rect(screenPosition.x - healthBarLeft / 4, Screen.height - screenPosition.y - barTop,100, 100), "" + playerInfo.health + "/" + playerInfo.maxHealth); //displays health in text format
  }
  #endregion 

  public void SetName(string name)
  {
    playerName = name;
  }

  Vector2 GetTankPosition()
  {
    return new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
  }
}
