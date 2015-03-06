using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Objects.Interfaces;
using Objects.Implementations;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
  public Transform[] rowEntryArray;

  [SerializeField] private GameObject[] playerList;
  [SerializeField] private GameObject tankPrefab;
  [SerializeField] private LevelManager levelManager;

  [SerializeField] private int currentPlayerIndex;

  [SerializeField] private float spawnY = 2.0f;
  [SerializeField] private float minSpawnX = -4.6f;
  [SerializeField] private float maxSpawnX = 4.6f;
  
  private GameController instance = null;

  private bool playerSpawned = false;

  private const string GAME_SCENE_NAME = "Game";
  private const string PLAYER_SCENE_NAME = "PlayerCreation";


  #region Unity Engine

  void Awake()
  {
    playerList = new GameObject[rowEntryArray.Length];

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
    if (Application.loadedLevelName == PLAYER_SCENE_NAME)
      AwakePlayerScene();
  }

  void Update()
  {
    if (Application.loadedLevelName == GAME_SCENE_NAME && !playerSpawned)
    {
      playerSpawned = true;
      AwakeGameScene();
    }
  }

  // Handler for when the Game scene awake
  private void AwakeGameScene()
  {
    playerList = ShufflePlayer(playerList);
    PlayNext();
  }

  // Handler for when the player creationg / selectionne scene Awake
  private void AwakePlayerScene()
  {

  }

  // Set the next player in turn to play
  public void PlayNext()
  {
    bool foundNext = false;
    while (!foundNext)
    {
      if (currentPlayerIndex >= playerList.Length -1 )
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
    foreach (var currentTank in playerList)
    {
      if (currentTank == null) continue;

      TankController tankController = currentTank.GetComponent<TankController>();
      tankController.isMyTurn = tank == currentTank;
      tankController.hasFired = false;
      tankController.HookUIElements();
    }
  }

  #endregion

  // Shuffle the player play order RANDOMLY because... it's ... shuffle... you know.
  public static GameObject[] ShufflePlayer(GameObject[] arr)
  {
    List<KeyValuePair<int, GameObject>> list = arr.Select(s => new KeyValuePair<int, GameObject>(Random.Range(0, 100), s)).ToList();
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

  // Create the array of tank that will fight in the field.
  public void Play()
  {
    if (Application.loadedLevelName != PLAYER_SCENE_NAME)
      return;

    int index = 0;

    foreach (var row in rowEntryArray)
    {
      Toggle toggle = row.GetComponentInChildren<Toggle>();
      InputField inputField = row.GetComponentInChildren<InputField>();
      if (toggle.isOn)
      {
        
        float randomXPos = Random.Range(minSpawnX, maxSpawnX);
        Vector3 spawnPos = new Vector3(randomXPos, spawnY, 0.0f);
        GameObject playerTank = Instantiate(tankPrefab, spawnPos, Quaternion.identity) as GameObject;
        GameObject.DontDestroyOnLoad(playerTank);
        TankController tankController = playerTank.GetComponent<TankController>();
        tankController.isMyTurn = false;
        PlayerInfo info = playerTank.GetComponent<PlayerInfo>();
        info.playerName = inputField.text;
        playerList[index] = playerTank;

        index++;
      }
    }
    levelManager.LoadLevel("Game");

    
  }
}
