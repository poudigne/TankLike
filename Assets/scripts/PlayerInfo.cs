using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerInfo : NetworkBehaviour
{
    public int maxHealth;
    [SyncVar]
    public int health;

    [SerializeField]
    private string _playerName;

    public PlayerNameLabel playerNameLabel;
    public ChatController chatController;

    #region Property
    public string playerName
    {
        get { return _playerName; }
        set
        {
            _playerName = value;
            if (playerNameLabel != null)
                playerNameLabel.SetName(_playerName);
        }
    }
    #endregion


    #region Unity Engine

    void Start()
    {

        if (chatController == null)
            Debug.LogError("ChatController not found. Please create one.");
        health = maxHealth;
        playerNameLabel = GetComponent<PlayerNameLabel>();
        playerName = _playerName;
        if (playerNameLabel == null)
            Debug.LogError("No \"PlayerNameLabel\" component found. Please add one.");
    }
    #endregion

    public void DoDamage(float damageAmount, string player_name)
    {
        if (!isServer)
        {
            return;
        }
        int damageInt = (int)damageAmount;
        health -= damageInt;
        ChatController chatController = FindObjectOfType<ChatController>();
        chatController.AddNewLine(string.Empty, player_name + " attacked " + playerName + " for " + damageInt + " damage");
        if (health <= 0)
        {
            Die();
            chatController.AddNewLine(string.Empty, player_name + " killed " + playerName);
        }
    }

    void Die()
    {
        Destroy(transform.gameObject);
    }
}
