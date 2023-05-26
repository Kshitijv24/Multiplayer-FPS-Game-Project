using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    public static bool hasSetNickName;

    public string[] mapsArray;
    public bool changeMapBetweenRounds;

    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject menuButtons;
    [SerializeField] TMP_Text loadingText;
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] GameObject roomPanel;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] GameObject errorPanel;
    [SerializeField] TMP_Text errorText;
    [SerializeField] GameObject roomBrowserPanel;
    [SerializeField] RoomButton roomButton;
    [SerializeField] TMP_Text playerNameLabel;
    [SerializeField] GameObject nameInputPanel;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] string levelToPlay;
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject testRoomButton;
    
    List<RoomButton> roomButtonList = new List<RoomButton>();
    List<TMP_Text> playerNameList = new List<TMP_Text>();
    string playerName = "Player Name";


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

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

#if UNITY_EDITOR
        testRoomButton.SetActive(true);
#endif

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #region Normal Methods
    private void CloseMenus()
    {
        loadingPanel.SetActive(false);
        menuButtons.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
        roomBrowserPanel.SetActive(false);
        nameInputPanel.SetActive(false);
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
        PhotonNetwork.CreateRoom("Room Name: " + roomNameInputField.text, roomOptions);

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

    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserPanel.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseMenus();
        loadingText.text = "Joining Room....";
        loadingPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text player in playerNameList)
        {
            Destroy(player.gameObject);
        }
        playerNameList.Clear();

        Player[] playerArray = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerArray.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = playerArray[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            playerNameList.Add (newPlayerLabel);
        }
    }

    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            PhotonNetwork.NickName = nameInputField.text;
            
            PlayerPrefs.SetString(playerName, nameInputField.text);

            CloseMenus();
            menuButtons.SetActive(true);
            hasSetNickName = true;
        }
    }

    public void StartGame()
    {
        //PhotonNetwork.LoadLevel(levelToPlay);
        PhotonNetwork.LoadLevel(mapsArray[Random.Range(0, mapsArray.Length)]);
    }

    public void QuickJoin()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 8;

        PhotonNetwork.CreateRoom("Test", roomOptions);
        CloseMenus();
        loadingText.text = "Creating Test Room....";
        loadingPanel.SetActive(true);
    }

    #endregion

    #region Photon Override Methods

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;

        loadingText.text = "Joining Lobby....";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if(!hasSetNickName)
        {
            CloseMenus();
            nameInputPanel.SetActive(true);

            if(PlayerPrefs.HasKey(playerName))
            {
                nameInputField.text = PlayerPrefs.GetString(playerName);
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString(playerName);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomPanel.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        if(PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
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

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton roomButton in roomButtonList)
        {
            Destroy(roomButton.gameObject);
        }
        roomButtonList.Clear();
        roomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newRoomButton = Instantiate(roomButton, roomButton.transform.parent);
                newRoomButton.SetButtonDetails(roomList[i]);
                newRoomButton.gameObject.SetActive(true);

                roomButtonList.Add(newRoomButton);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        playerNameList.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    #endregion
}
