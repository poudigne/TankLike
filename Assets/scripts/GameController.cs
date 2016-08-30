using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;

using Games.Utils;

public class GameController : NetworkBehaviour
{


    private GameObject tankPrefab;

    [SerializeField]
    private int currentPlayerIndex;

    private GameController instance = null;

    private const string GAME_SCENE_NAME = "Game";
    private const string PLAYER_SCENE_NAME = "PlayerCreation";
    const int MAX_PLAYER = 8;
    [SerializeField]
    private bool hasGameStarted = false;

    private GameObject[] playerList = new GameObject[MAX_PLAYER];
    private int turnCount;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            print("Duplicate GameController self-destructing!");
        }
        else
        {
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

    // executed by the server only
    public void StartGame()
    {
        if (!hasGameStarted)
        {
            playerList = ShufflePlayer(playerList);
            hasGameStarted = true;
        }
    }
    bool hasPrinted = false;
    void Update()
    {
        if (!hasGameStarted || !isServer)
        {
            return;
        }

        if (!hasPrinted)
        {
            foreach (GameObject player in playerList)
            {
                if (player == null)
                    continue;
                TankController tank = player.GetComponent<TankController>();
                PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
                string dbgstr = String.Format("Player {0}, IsTurn? {1}", playerInfo.playerName, tank.isMyTurn);
                Debug.Log(dbgstr);
            }
            hasPrinted = true;
        }

        GameObject currentPlayer = playerList[currentPlayerIndex];
        Debug.Log("CurrentPlayerIndex = " + currentPlayerIndex);
        if (currentPlayer != null)
        {
            Debug.Log("We have a current player.");
            TankController tank = currentPlayer.GetComponent<TankController>();
            PlayerInfo playerInfo = currentPlayer.GetComponent<PlayerInfo>();
            if (!tank.isMyTurn && tank.hasPlayed)
            {
                Debug.Log("CurrentPlayer has Played. Finding next player");
                IncreasePlayerIndex();
            }
            else if (!tank.isMyTurn && !tank.hasPlayed)
            {
                Debug.Log("New Player Found !!");
                RpcSetNewTurn(currentPlayer);
            }
        }
        else
        {
            IncreasePlayerIndex();
        }
    }

    void IncreasePlayerIndex()
    {
        if (currentPlayerIndex == playerList.Count() - 1)
        {
            currentPlayerIndex = 0;
            turnCount++;
        }
        else
        {
            currentPlayerIndex++;
        }
    }

    [ClientRpc]
    private void RpcNotifyNewTurn(string _playerName)
    {
        if (isLocalPlayer)
        {
            Debug.Log("It's your turn.");
        }
        else
        {
            Debug.Log(String.Format("It's {0}'s turn", _playerName));
        }
    }

    public bool PlayerHasPlayed()
    {
        for (int i = 0; i < playerList.Count(); i++)
        {
            if (playerList[i] == null)
                continue;
            if (playerList[i].GetComponent<TankController>().isMyTurn)
                return false;
        }
        return true;
    }

    public void RegisterPlayer(GameObject gamePlayer)
    {

        TankController tank = gamePlayer.GetComponent<TankController>();
        tank.isMyTurn = false;
        tank.hasPlayed = false;
        PlayerInfo tankInfo = gamePlayer.GetComponent<PlayerInfo>();
        tankInfo.UUID = UUID.GetUniqueID();

        playerList[GetNextAvailableSlot()] = gamePlayer;
    }

    private int GetNextAvailableSlot()
    {
        for (int i = 0; i < playerList.Count(); i++)
        {
            if (playerList[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    //// Set the next player in turn to play
    //public void PlayNext()
    //{
    //    if (!isServer)
    //        return;
    //    bool foundNext = false;
    //    while (!foundNext)
    //    {
    //        if (currentPlayerIndex >= playerList.Count() - 1)
    //            currentPlayerIndex = 0;
    //        else
    //            currentPlayerIndex++;
    //        GameObject tank = playerList[currentPlayerIndex];
    //        Debug.Log("PlayerIndex " + currentPlayerIndex);
    //        if (tank != null)
    //        {
    //            PlayerInfo playerInfo = tank.GetComponent<PlayerInfo>();
    //            Debug.Log("Tank has been found. " + playerInfo.playerName + "'s turn. ");
    //            SetNewTurn(tank);
    //            foundNext = true;
    //        }
    //    }
    //}

    // Change the status of playability for each tank in order to only have 1 player wich is his turn
    [ClientRpc]
    private void RpcSetNewTurn(GameObject tank)
    {
        TankController tankController = tank.GetComponent<TankController>();
        PlayerInfo playerInfo = tank.GetComponent<PlayerInfo>();
        tankController.SetHasFired(false);
        tankController.isMyTurn = true;
        //RpcNotifyNewTurn(playerInfo.playerName);
        //tankController.HookUIElements();
    }

    // Shuffle the player play order RANDOMLY because... it's... random... you know.
    public static GameObject[] ShufflePlayer(GameObject[] arr)
    {
        List<KeyValuePair<int, GameObject>> list = arr.Select(s => new KeyValuePair<int, GameObject>(UnityEngine.Random.Range(0, 100), s)).ToList();
        var sorted = from item in list
                     orderby item.Key
                     select item;
        GameObject[] result = new GameObject[arr.Length];
        int index = 0;
        foreach (KeyValuePair<int, GameObject> pair in sorted)
        {
            result[index] = pair.Value;
            index++;
        }
        return result;
    }
}