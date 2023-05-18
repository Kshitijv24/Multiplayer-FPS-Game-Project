using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager Instance;

    public enum EventCodesEnum : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }

    [SerializeField] List<PlayerInfo> playerInfoList = new List<PlayerInfo>();

    int index;
    List<PlayersLeaderboard> playersLeaderboardList = new List<PlayersLeaderboard>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (UIController.Instance.leaderBoard.activeInHierarchy)
            {
                UIController.Instance.leaderBoard.SetActive(false);
            }
            else
            {
                ShowLeaderboard();
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200)
        {
            EventCodesEnum eventCodesEnum = (EventCodesEnum)photonEvent.Code;
            object[] dataObjectArray = (object[])photonEvent.CustomData;

            switch(eventCodesEnum)
            {
                case EventCodesEnum.NewPlayer:
                    NewPlayerReceive(dataObjectArray);
                    break;

                case EventCodesEnum.ListPlayers:
                    ListPlayersReceive(dataObjectArray);
                    break;

                case EventCodesEnum.UpdateStat:
                    UpdateStatsReceive(dataObjectArray);
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string userName)
    {
        object[] packageObjectArray = new object[4];

        packageObjectArray[0] = userName;
        packageObjectArray[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        packageObjectArray[2] = 0;
        packageObjectArray[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodesEnum.NewPlayer,
            packageObjectArray,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }

    public void NewPlayerReceive(object[] dataReceivedArray)
    {
        PlayerInfo playerInfo = new PlayerInfo(
            (string)dataReceivedArray[0],
            (int)dataReceivedArray[1],
            (int)dataReceivedArray[2],
            (int)dataReceivedArray[3]);

        playerInfoList.Add(playerInfo);

        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        object[] package = new object[playerInfoList.Count];

        for (int i = 0; i < playerInfoList.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = playerInfoList[i].playerName;
            piece[1] = playerInfoList[i].playerActor;
            piece[2] = playerInfoList[i].playerKills;
            piece[3] = playerInfoList[i].playerDeaths;

            package[i] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodesEnum.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void ListPlayersReceive(object[] dataReceivedArray)
    {
        playerInfoList.Clear();

        for (int i = 0; i < dataReceivedArray.Length; i++)
        {
            object[] piece = (object[])dataReceivedArray[i];
            
            PlayerInfo playerInfo = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );

            playerInfoList.Add(playerInfo);

            if(PhotonNetwork.LocalPlayer.ActorNumber == playerInfo.playerActor)
            {
                index = i;
            }
        }
    }

    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodesEnum.UpdateStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void UpdateStatsReceive(object[] dataReceivedArray)
    {
        int actor = (int)dataReceivedArray[0];
        int statType = (int)dataReceivedArray[1];
        int amount = (int)dataReceivedArray[2];

        for (int i = 0; i < playerInfoList.Count; i++)
        {
            if (playerInfoList[i].playerActor == actor)
            {
                switch (statType)
                {
                    case 0: // Kills
                        playerInfoList[i].playerKills += amount;
                        Debug.Log("Player " + playerInfoList[i].playerName + " :Kills " + playerInfoList[i].playerKills);
                        break;

                    case 1: // death
                        playerInfoList[i].playerDeaths += amount;
                        Debug.Log("Player " + playerInfoList[i].playerName + " :Deaths " + playerInfoList[i].playerDeaths);
                        break;
                }

                if (i == index)
                {
                    UpdateStatDisplay();
                }

                break;
            }
        }
    }

    public void UpdateStatDisplay()
    {
        if(playerInfoList.Count > index)
        {
            UIController.Instance.killCountText.text = "Kills: " + playerInfoList[index].playerKills;
            UIController.Instance.deathCountText.text = "Deaths: " + playerInfoList[index].playerDeaths;
        }
        else
        {
            UIController.Instance.killCountText.text = "Kills: 0";
            UIController.Instance.deathCountText.text = "Deaths: 0";
        }
    }

    private void ShowLeaderboard()
    {
        UIController.Instance.leaderBoard.SetActive(true);

        foreach (PlayersLeaderboard playersLeaderboard in playersLeaderboardList)
        {
            Destroy(playersLeaderboard.gameObject);
        }
        playersLeaderboardList.Clear();

        UIController.Instance.playersLeaderboard.gameObject.SetActive(false);

        foreach (PlayerInfo playerInfo in playerInfoList)
        {
            PlayersLeaderboard newPlayersLeaderboard = Instantiate(
                UIController.Instance.playersLeaderboard, 
                UIController.Instance.playersLeaderboard.transform.parent);

            newPlayersLeaderboard.SetDetails(playerInfo.playerName, playerInfo.playerKills, playerInfo.playerDeaths);
            newPlayersLeaderboard.gameObject.SetActive(true);

            playersLeaderboardList.Add(newPlayersLeaderboard);
        }
    }
}
