using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class DebugUI : NetworkBehaviour
{

    Text valIsMyTurn;
    Text valHasPlayed;
    Text valClientType;

    TankController tank;

    // Use this for initialization
    void Start()
    {
        tank = GetComponent<TankController>();
        valIsMyTurn = GameObject.Find("Value_IsMyTurn").GetComponent<Text>();
        valHasPlayed = GameObject.Find("Value_HasPlayed").GetComponent<Text>();
        valClientType = GameObject.Find("Value_ClientType").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;
        valIsMyTurn.text = tank.isMyTurn.ToString();
        valHasPlayed.text = tank.hasPlayed.ToString();
        valClientType.text = (isServer) ? "Host" : "Client";
    }
}
