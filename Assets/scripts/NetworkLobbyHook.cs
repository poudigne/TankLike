using UnityEngine;
using System.Collections;
using Prototype.NetworkLobby;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook {

    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        TankController tank = gamePlayer.GetComponent<TankController>();
        PlayerInfo tankInfo = gamePlayer.GetComponent<PlayerInfo>();

        tankInfo.playerName = lobby.name;
        tank.color = lobby.playerColor;
    }
}
