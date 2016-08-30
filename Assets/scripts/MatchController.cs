using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MatchController : NetworkBehaviour
{

    GameController gameController;

    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
    }
    // Use this for initialization
    void Start()
    {
        if (isServer)
            gameController.StartGame();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
