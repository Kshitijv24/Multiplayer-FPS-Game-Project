using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    [SerializeField] TMP_Text buttonText;

    RoomInfo roomInfo;

    public void SetButtonDetails(RoomInfo inputInfo)
    {
        roomInfo = inputInfo;
        buttonText.text = roomInfo.Name;
    }

    public void OpenRoom()
    {
        Launcher.Instance.JoinRoom(roomInfo);
    }
}
