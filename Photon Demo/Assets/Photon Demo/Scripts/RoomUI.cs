using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    public Button PlayBt;
    public Text RoomNameTxt;
    public Text PlayersTxt;

    private void Awake()
    {
        PlayBt.onClick.AddListener(Play);
    }

    public void SetRoomStats(string _roomName, int currentPlayers, int maxPlayers)
    {
        RoomNameTxt.text = _roomName;
        PlayersTxt.text = currentPlayers + " / " + maxPlayers;
    }

   public void Play()
    {
        MultiplayerMenuManager.Instance.JoinSpecificRoom(RoomNameTxt.text);
    }
}
