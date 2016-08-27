using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerInfo : NetworkBehaviour
{
    public int maxHealth;
    [SyncVar]
    public int health;

    private string _playerName;

    public PlayerNameLabel playerNameLabel;

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

    void Start()
    {
        health = maxHealth;
        playerNameLabel = GetComponent<PlayerNameLabel>();
        playerName = _playerName;
        if (playerNameLabel == null)
            Debug.LogError("No \"PlayerNameLabel\" component found. Please add one.");
    }

    public void DoDamage(float damageAmount)
    {
        if (!isServer)
        {
            return;
        }
        Debug.Log("Player took damage : " + damageAmount + "HP");
        int damageInt = (int)damageAmount;
        health -= damageInt;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(transform.gameObject);
    }
}
