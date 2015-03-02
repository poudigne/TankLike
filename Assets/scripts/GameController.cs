using System.Collections.Generic;
using System.Text;
using UnityEngine;

using Objects.Interfaces;
using Objects.Implementations;

public class GameController : MonoBehaviour 
{
	public GameObject tankPrefab;
	public GameObject spawnPoint;
	public GameObject minXPos;
	public GameObject maxXPos;

	List<IPlayer> playerList { get; set; }
	List<IPlayer> playerOrder { get; set; }
	IPlayer currentTurn { get; set; }

	public GameController()
	{
		playerList = new List<IPlayer>();
		playerOrder = new List<IPlayer>();
	}

	public void CreatePlayer(string name, int tankID)
	{
		playerList.Add(new Player(name, tankID));
	}

	void Start()
	{
		// float randomXPos = Random.Range(minXPos.transform.position.x, maxXPos.transform.position.x);
		// GameObject tank = Instantiate(tankPrefab, new Vector3(randomXPos, spawnPoint.transform.position.y, 0.0f), Quaternion.identity) as GameObject;
		// CreatePlayer("John ",tank.GetInstanceID());

	}

}
