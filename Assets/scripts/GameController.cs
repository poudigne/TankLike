using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{

    private List<GameObject> playerList;
    private List<GamePlayer> playerList2;
    private GameObject tankPrefab;

    [SerializeField]
    private int currentPlayerIndex;

    private GameController instance = null;

    private const string GAME_SCENE_NAME = "Game";
    private const string PLAYER_SCENE_NAME = "PlayerCreation";
    const int MAX_PLAYER = 8;


    void Awake()
    {
        playerList = new List<GameObject>();
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

    private int? GetNextAvailableSlot()
    {
        if (!isServer)
            return null;

        for (int i = 0; i < playerList.Count(); i++)
        {
            if (playerList[i] == null)
                return i;
        }
        Debug.LogWarning("Server is full !");
        return null;
    }


    // Set the next player in turn to play
    public void PlayNext()
    {
        if (!isServer)
            return;
        bool foundNext = false;
        while (!foundNext)
        {
            if (currentPlayerIndex >= playerList.Count - 1)
                currentPlayerIndex = 0;
            else
                currentPlayerIndex++;
            GameObject tank = playerList[currentPlayerIndex];
            Debug.Log("PlayerIndex " + currentPlayerIndex);
            if (tank != null)
            {
                PlayerInfo playerInfo = tank.GetComponent<PlayerInfo>();
                Debug.Log("Tank has been found. " + playerInfo.playerName + "'s turn. ");
                SetNewTurn(tank);
                foundNext = true;
            }
        }
    }

    // Change the status of playability for each tank in order to only have 1 player wich is his turn
    private void SetNewTurn(GameObject tank)
    {
        if (!isServer)
            return;

        foreach (var currentTank in playerList)
        {
            if (currentTank == null) continue;

            TankController tankController = currentTank.GetComponent<TankController>();
            tankController._isMyTurn = tank == currentTank;
            tankController.SetHasFired(false);
            tankController.HookUIElements();
        }
    }

    // Shuffle the player play order RANDOMLY because... it's ... shuffle... you know.
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

    internal void AddPlayerInfo(NetworkPlayer player)
    {
        playerList2.Add(new GamePlayer(player));
        AddLineToChat("System", player.ipAddress + " joined the server");
    }
    internal void SetPlayerReady(NetworkPlayer _player)
    {
        if (!isServer)
            return;

        GamePlayer player = playerList2.FirstOrDefault(p => p.networkPlayer.guid == _player.guid);
        if (player == null)
            return;

        player.isReady = !player.isReady;
        if (player.isReady)
        {
            AddLineToChat("System", player.GetIpAddress + " is ready!");
            StartGameIfAllReady();
        }
        else
            AddLineToChat("System", player.GetIpAddress + " is not ready!");
    }

    private void StartGameIfAllReady()
    {
        if (!isServer)
            return;
        if (CheckIfAllReady())
        {
            RpcLoadScene();
        }
    }

    private bool CheckIfAllReady()
    {
        foreach (GamePlayer player in playerList2)
        {
            if (!player.isReady)
            {
                return false;
            }
        }
        return true;
    }

    [ClientRpc]
    void RpcLoadScene()
    {
        if (isLocalPlayer)
        {
            SceneManager.LoadScene(GAME_SCENE_NAME);
        }
    }



    // this is temporary

    public float chatWidth = 500.0f;
    public float chatHeight = 300.0f;

    private Vector2 scrollPosition = Vector2.zero;
    private List<string> saidStuff = new List<string>();
    void OnGUI()
    {
        var ViewPort = (25 * (saidStuff.Count - 4));
        if (ViewPort < 0)
        {
            ViewPort = 0;
        }
        scrollPosition = GUI.BeginScrollView(new Rect(10, 10, 420, 100), scrollPosition, new Rect(0, 0, 400, 100 + ViewPort), false, true);
        var HeightOfBox = (25 * (saidStuff.Count - 4));
        if (HeightOfBox < 0)
        {
            HeightOfBox = 0;
        }
        GUI.Box(new Rect(0, 0, 400, 100 + HeightOfBox), " ");
        for (var i = 0; i < saidStuff.Count; i++)
        {
            GUI.Label(new Rect(0, (25 * i), 400, 23), saidStuff[i]);
        }
        GUI.EndScrollView();
    }
    void AddLineToChat(string name, string text)
    {

        saidStuff.Add(text);
        if (text == "/Help")
        {
            //...
        }
        if (text == "/Top")
        {
            scrollPosition = Vector2.zero;
        }
        else
        {
            Vector2 LowestSpot = new Vector2(0, 25 * (saidStuff.Count - 4));
            if (LowestSpot.y < 0)
            {
                LowestSpot.y = 0;
            }
            scrollPosition = LowestSpot;
        }
        if (text == "/Clear")
        {
            saidStuff = new List<string>();
            AddLineToChat("", "Cleared chat");
        }
    }
    internal class GamePlayer
    {
        public NetworkPlayer networkPlayer;
        public bool isReady { get; set; }

        public GamePlayer(NetworkPlayer player)
        {
            networkPlayer = player;
            isReady = false;
        }

        public string GetIpAddress { get { return networkPlayer.ipAddress; } }
    }
}