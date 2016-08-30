using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{

    public Text valIsMyTurn;
    public Text valHasPlayed;

    TankController tank;

    // Use this for initialization
    void Start()
    {
        tank = GetComponent<TankController>();
        valIsMyTurn = GameObject.Find("Value_IsMyTurn").GetComponent<Text>();
        valHasPlayed = GameObject.Find("Value_HasPlayed").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        valIsMyTurn.text = tank.isMyTurn.ToString();
        valHasPlayed.text = tank.hasPlayed.ToString();
    }
}
