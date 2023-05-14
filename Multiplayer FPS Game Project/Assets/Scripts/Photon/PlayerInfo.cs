using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    [SerializeField] string playerName;
    [SerializeField] int playerActor;
    [SerializeField] int playerKills;
    [SerializeField] int playerDeaths;

    public PlayerInfo(string _playerName, int _playerActor, int _playerKills, int _playerDeaths)
    {
        this.playerName = _playerName;
        this.playerActor = _playerActor;
        this.playerKills = _playerKills;
        this.playerDeaths = _playerDeaths;
    }
}
