﻿using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
  public int maxHealth;
  public int health;

  [SerializeField] private string _playerName;
  
  private PlayerNameLabel playerNameLabel;
  private ChatController chatController;

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
    chatController = FindObjectOfType<ChatController>() as ChatController;
    if (chatController == null)
      Debug.LogError("ChatController not found. Please create one. ["+chatController+"]");
    health = maxHealth;
    playerNameLabel = GetComponent<PlayerNameLabel>();
    playerName = _playerName;
    if (playerNameLabel == null)
      Debug.LogError("No \"PlayerNameLabel\" component found. Please add one.");
  }
  #endregion

  public void DoDamage(float damageAmount, PlayerInfo attackerInfo)
  {
    int damageInt = (int) damageAmount;
    health -= damageInt;
    chatController.AddNewLine(string.Empty, attackerInfo.playerName + " attacked " + playerName + " for " + damageInt + " damage");
    if (health <= 0)
    {
      Die();
      chatController.AddNewLine(string.Empty, attackerInfo.playerName + " killed " + playerName);
    }
  }

  void Die()
  {
    Destroy(transform.gameObject);
  }
}
