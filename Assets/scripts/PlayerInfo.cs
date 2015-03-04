using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
  public int health;

  [SerializeField] private string _playerName;

  private PlayerNameLabel playerNameLabel;

  #region Property
  public string playerName
  {
    get { return _playerName; }
    set
    {
      _playerName = value;
      playerNameLabel.SetName(_playerName);
    }
  }
  #endregion
  

  #region Unity Engine

  void Start()
  {
    playerNameLabel = GetComponent<PlayerNameLabel>();
    playerName = _playerName;
    if (playerNameLabel == null)
      Debug.LogError("No \"PlayerNameLabel\" component found. Please add one.");
  }
  #endregion
}
