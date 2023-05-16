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
    }

    public void ListPlayersSend()
    {

    }

    public void ListPlayersReceive(object[] dataReceivedArray)
    {

    }

    public void UpdateStatsSend()
    {

    }

    public void UpdateStatsReceive(object[] dataReceivedArray)
    {

    }
}
