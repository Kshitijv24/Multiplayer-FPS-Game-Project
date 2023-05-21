using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager Instance;

    public enum EventCodesEnum : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public GameState state;
    public Transform mapCameraPoint;

    [SerializeField] int killsToWin;
    [SerializeField] float waitAfterEnding;

    List<PlayerInfo> playerInfoList = new List<PlayerInfo>();
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
            state = GameState.Playing;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && state != GameState.Ending)
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
        object[] package = new object[playerInfoList.Count + 1];

        package[0] = state;

        for (int i = 0; i < playerInfoList.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = playerInfoList[i].name;
            piece[1] = playerInfoList[i].actor;
            piece[2] = playerInfoList[i].kills;
            piece[3] = playerInfoList[i].deaths;

            package[i + 1] = piece;
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

        state = (GameState)dataReceivedArray[0];

        for (int i = 1; i < dataReceivedArray.Length; i++)
        {
            object[] piece = (object[])dataReceivedArray[i];
            
            PlayerInfo playerInfo = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );

            playerInfoList.Add(playerInfo);

            if(PhotonNetwork.LocalPlayer.ActorNumber == playerInfo.actor)
            {
                index = i - 1;
            }
        }
        StateCheck();
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
            if (playerInfoList[i].actor == actor)
            {
                switch (statType)
                {
                    case 0: // Kills
                        playerInfoList[i].kills += amount;
                        Debug.Log("Player " + playerInfoList[i].name + " :Kills " + playerInfoList[i].kills);
                        break;

                    case 1: // death
                        playerInfoList[i].deaths += amount;
                        Debug.Log("Player " + playerInfoList[i].name + " :Deaths " + playerInfoList[i].deaths);
                        break;
                }

                if (i == index)
                {
                    UpdateStatDisplay();
                }

                if (UIController.Instance.leaderBoard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }

                break;
            }
        }
        ScoreCheck();
    }

    public void UpdateStatDisplay()
    {
        if(playerInfoList.Count > index)
        {
            UIController.Instance.killCountText.text = "Kills: " + playerInfoList[index].kills;
            UIController.Instance.deathCountText.text = "Deaths: " + playerInfoList[index].deaths;
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

        List<PlayerInfo> sortedList = SortPlayers(playerInfoList);

        foreach (PlayerInfo playerInfo in sortedList)
        {
            PlayersLeaderboard newPlayersLeaderboard = Instantiate(
                UIController.Instance.playersLeaderboard, 
                UIController.Instance.playersLeaderboard.transform.parent);

            newPlayersLeaderboard.SetDetails(playerInfo.name, playerInfo.kills, playerInfo.deaths);
            newPlayersLeaderboard.gameObject.SetActive(true);

            playersLeaderboardList.Add(newPlayersLeaderboard);
        }
    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> playersList)
    {
        List<PlayerInfo> sortedList = new List<PlayerInfo>();

        while(sortedList.Count < playersList.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = playersList[0];

            foreach (PlayerInfo player in playersList)
            {
                if (!sortedList.Contains(player))
                {
                    if (player.kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.kills;
                    }
                }
            }
            sortedList.Add(selectedPlayer);
        }

        return sortedList;
    }

    private void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (PlayerInfo player in playerInfoList)
        {
            if(player.kills >= killsToWin && killsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if(winnerFound)
        {
            if(PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayersSend();
            }
        }
    }

    private void StateCheck()
    {
        if(state == GameState.Ending)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        state = GameState.Ending;
        
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        UIController.Instance.endScreen.SetActive(true);
        ShowLeaderboard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCameraPoint.position;
        Camera.main.transform.rotation = mapCameraPoint.rotation;

        StartCoroutine(EndCoroutine());
    }


    private IEnumerator EndCoroutine()
    {
        yield return new WaitForSeconds(waitAfterEnding);
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(0);
    }
}
