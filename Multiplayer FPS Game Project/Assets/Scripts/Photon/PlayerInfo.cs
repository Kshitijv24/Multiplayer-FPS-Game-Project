using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor;
    public int kills;
    public int deaths;

    public PlayerInfo(string _playerName, int _playerActor, int _playerKills, int _playerDeaths)
    {
        this.name = _playerName;
        this.actor = _playerActor;
        this.kills = _playerKills;
        this.deaths = _playerDeaths;
    }
}
