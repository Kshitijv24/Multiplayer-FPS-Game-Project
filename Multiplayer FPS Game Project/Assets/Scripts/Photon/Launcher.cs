using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject menuButtons;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] GameObject roomPanel;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] GameObject errorPanel;
    [SerializeField] TMP_Text errorText;

    private void Awake()
    {
        if(Instance == null)
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
        CloseMenus();
        loadingPanel.SetActive(true);
        loadingText.text = "Connecting To Network....";

        PhotonNetwork.ConnectUsingSettings();
    }

    private void CloseMenus()
    {
        loadingPanel.SetActive(false);
        menuButtons.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
    }

    public void OpenCreateRoomPannel()
    {
        CloseMenus();
        createRoomPanel.SetActive(true);
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
            return;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 8;
        PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);

        CloseMenus();
        loadingText.text = "Creating Room....";
        loadingPanel.SetActive(true);
    }

    public void CloseErrorPanel()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Room....";
        loadingPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        loadingText.text = "Joining Lobby....";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomPanel.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed To Create Room: " + message;
        CloseMenus();
        errorPanel.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }
}
