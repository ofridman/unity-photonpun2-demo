using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;

public class MultiplayerMenuManager :  MonoBehaviourPunCallbacks, IChatClientListener
{

    [Header("Menu")]
    public GameObject Menu;
    public GameObject LoadingGo;
    public GameObject Lobby;
    public GameObject Room;
    public GameObject RoomSelector;

    public Transform RoomSelectorGrid;

    [Header("Room")]
    public Text PlayersInRoom;
    public Button RoomBt;

    public GameObject RoomPanel;
    public GameObject ChatPanel;

    public GameObject PlayerPrefabUI;
    public Transform PlayersInRoomGrid;

    [Header("Lobby")]
    public Text PingTxt;
    public Text TotalPlayersConnectedTxt;
    public Text TotalPlayersInRoomTxt;
    public Text CurrentUserLevelTxt;

    [Header("Create Room")]
    public GameObject CreateRoomPanel;
    public Text RoomNameTxt;
    public Text MaxPlayersTxt;

    [Header("Prefabs")]
    public GameObject RoomPrefabUI;

    public GameObject ChatMessageUI;

    [Header("Chat")]
    public Transform ChatMessagesGrid;


    public static MultiplayerMenuManager Instance;

    private List<RoomUI> spawnedRooms = new List<RoomUI>();
    private List<PlayersUI> spawnedPlayers = new List<PlayersUI>();

    // for Testing MatchMaking:
    private int myPlayerLevel = 0;

    private bool roomCreatedByUser;

    private ChatClient chatClient;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        DontDestroyOnLoad(this);
    }
    private void Start()
    {

        InvokeRepeating("UpdatePing", 1f, 5f);
        InvokeRepeating("UpdateLobbyStats", 1f, 5f);

        // If has logged in before
        if (PlayerPrefs.HasKey(ManagerUtilities.UsernameKey))
        {
            LoadingGo.SetActive(true);
            PhotonNetwork.NickName = PlayerPrefs.GetString(ManagerUtilities.UsernameKey);


            PhotonNetwork.ConnectUsingSettings();

        }
    }

    // Clicked "Connect" button
    public void ConnectToPhoton(Text name)
    {
        PhotonNetwork.OfflineMode = false;


        if (!PhotonNetwork.IsConnectedAndReady)
        {
            LoadingGo.SetActive(true);
            PlayerPrefs.SetString("username", name.text);
            PhotonNetwork.NickName = name.text;
            PhotonNetwork.ConnectUsingSettings();

        }
        else
        {

            OnJoinedLobby();
        }

    }
    #region Lobby

    void UpdatePing()
    {
        PingTxt.text = "Ping " + PhotonNetwork.GetPing() + " ms";

    }
    void UpdateLobbyStats()
    {
        TotalPlayersConnectedTxt.text = "Connected Players " + PhotonNetwork.CountOfPlayers;
        TotalPlayersInRoomTxt.text = "Players in Rooms " + PhotonNetwork.CountOfPlayersInRooms;

    }
    // Matchmaking based on player's level
    public void Matchmaking()
    {
        LoadingGo.SetActive(true);
        ExitGames.Client.Photon.Hashtable expectedProperties = new ExitGames.Client.Photon.Hashtable();
        expectedProperties.Add("level", myPlayerLevel);
        // it will lead to a photon callback
        PhotonNetwork.JoinRandomRoom(expectedProperties, 0);
    }
    // Disconnect from Photon
    public void Logout()
    {
        PhotonNetwork.Disconnect();
        PlayerPrefs.DeleteKey(ManagerUtilities.UsernameKey);
        Lobby.SetActive(false);
        Menu.SetActive(true);
    }

    public void ChangeUserLvl(Text lvlTxt)
    {
        myPlayerLevel = int.Parse(lvlTxt.text);
        ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
        properties["level"] = myPlayerLevel;
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        CurrentUserLevelTxt.text = "User level " + myPlayerLevel.ToString();

    }

    #endregion
    #region Room Selector
    public void EnterRoomSelector()
    {
        RoomSelector.SetActive(true);
        Lobby.SetActive(false);
    }
  
    public void ExitFromRoomSelector()
    {
        Lobby.SetActive(true);
        RoomSelector.SetActive(false);

    }

    public void JoinSpecificRoom(string _roomName)
    {
        PhotonNetwork.JoinRoom(_roomName);
    }
    #endregion
    #region Room

    // Only master client can start the game
    public void ClickPlay()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            properties["mode"] = "playing";
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
            photonView.RPC("LoadGameScene", RpcTarget.All);


        }
    }

    [PunRPC]
    void LoadGameScene()
    {
        LoadingGo.SetActive(true);
        PhotonNetwork.LoadLevel(1);
    }
    
    public void LeaveRoom()
    {
        LoadingGo.SetActive(true);
        chatClient.Unsubscribe(new string[] { PhotonNetwork.CurrentRoom.Name });
      // Destroy UI messages
        foreach (var child in ChatMessagesGrid.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject != ChatMessagesGrid.gameObject)
                Destroy(child.gameObject);

        }

        PhotonNetwork.LeaveRoom();

    }
    public void ActivateRoomPanel()
    {
        RoomPanel.SetActive(true);
        ChatPanel.SetActive(false);

    }
    public void ActivateChatPanel()
    {
        RoomPanel.SetActive(false);
        ChatPanel.SetActive(true);

    }
    #endregion
    #region Create Room
    public void ClickCreateRoom()
    {
        CreateRoomPanel.SetActive(true);
        Lobby.SetActive(false);
    }
    public void ClickLeaveCreateRoom()
    {
        CreateRoomPanel.SetActive(false);
        Lobby.SetActive(true);

    }
    // Create a custom room
    public void CreateRoom()
    {
        roomCreatedByUser = true;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(MaxPlayersTxt.text);
        PhotonNetwork.CreateRoom(RoomNameTxt.text, roomOptions, TypedLobby.Default);
        LoadingGo.SetActive(true);
    }

    #endregion

    #region CallBacks

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        if (roomCreatedByUser)
        {
            roomCreatedByUser = false;
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("mode", "waiting");
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);


            return;
        }
        // Matchmaking
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        hashtable.Add("level", myPlayerLevel);
        hashtable.Add("mode", "waiting");

        PhotonNetwork.CurrentRoom.SetPropertiesListedInLobby(new string[1] { "level" });

        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        TryCreatingRoom();

    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        TryCreatingRoom();

    }
 
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        LoadingGo.SetActive(false);

        Lobby.SetActive(true);
        Room.SetActive(false);

    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        UpdatePlayersUI();

    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        UpdatePlayersUI();

    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        if (RoomSelectorGrid == null)
            return;
        int roomListLength = roomList.Count;
        for (int i = 0; i < roomListLength; i++)
        {
            RoomInfo room = roomList[i];
            if (i < spawnedRooms.Count)
                spawnedRooms[i].SetRoomStats(room.Name, room.PlayerCount, (int)room.MaxPlayers);
            else
            {
                GameObject instance = Instantiate(RoomPrefabUI, RoomSelectorGrid);
                RoomUI instanceRoomUI = instance.GetComponent<RoomUI>();
                instanceRoomUI.SetRoomStats(room.Name, room.PlayerCount, (int)room.MaxPlayers);
                spawnedRooms.Add(instanceRoomUI);
            }

        }
        if (roomListLength < spawnedRooms.Count)
        {
            for (int i = roomListLength; i < spawnedRooms.Count; i++)
            {
                DestroyImmediate(spawnedRooms[i].gameObject);
            }
            spawnedRooms.RemoveAll(item => item == null);

        }
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        PhotonNetwork.JoinLobby();

    }

    public override void OnJoinedLobby()
    {
        LoadingGo.SetActive(false);
        Menu.SetActive(false);
        Lobby.SetActive(true);

        ExitGames.Client.Photon.Hashtable hastable = new ExitGames.Client.Photon.Hashtable();
        myPlayerLevel = (int)UnityEngine.Random.Range(0, 5);
        hastable.Add("level", myPlayerLevel);

        CurrentUserLevelTxt.text = "User level " + myPlayerLevel.ToString();
        PhotonNetwork.SetPlayerCustomProperties(hastable);

        chatClient = new ChatClient(this);
        chatClient.ChatRegion = "EU";

        chatClient.Connect(ManagerUtilities.ChatAppID, ManagerUtilities.ChatAppVersion, new Photon.Chat.AuthenticationValues(PhotonNetwork.LocalPlayer.NickName));

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        LoadingGo.SetActive(false);
        Lobby.SetActive(false);
        RoomSelector.SetActive(false);
        CreateRoomPanel.SetActive(false);
        Room.SetActive(true);
        RoomBt.Select();

        chatClient.Subscribe(new string[] { PhotonNetwork.CurrentRoom.Name });

        if ((string)PhotonNetwork.CurrentRoom.CustomProperties["mode"] == "playing")
        {

            LoadGameScene();

        }
        UpdatePlayersUI();
    }
    #endregion
    void UpdatePlayersUI()
    {
        if (PlayersInRoom == null)
            return;
        PlayersInRoom.text = "Players in this room " + PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        int playerListLength = PhotonNetwork.PlayerListOthers.Length;
        for (int i = 0; i < playerListLength; i++)
        {
            var player = PhotonNetwork.PlayerListOthers[i];
            if (i < spawnedPlayers.Count)
                spawnedPlayers[i].SetPlayerStats(player.NickName, (int)player.CustomProperties["level"]);
            else
            {
                GameObject instance = Instantiate(PlayerPrefabUI, PlayersInRoomGrid);
                PlayersUI instancePlayerUI = instance.GetComponent<PlayersUI>();
                instancePlayerUI.SetPlayerStats(player.NickName, (int)player.CustomProperties["level"]);
                spawnedPlayers.Add(instancePlayerUI);
            }

        }

        if (playerListLength < spawnedPlayers.Count)
        {
            for (int i = playerListLength; i < spawnedPlayers.Count; i++)
            {
                DestroyImmediate(spawnedPlayers[i].gameObject);
            }
            spawnedPlayers.RemoveAll(item => item == null);

        }
    }
    void TryCreatingRoom()
    {


        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new string[1] { "level" };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "level", myPlayerLevel } };
        roomOptions.MaxPlayers = (byte)ManagerUtilities.MaxPlayersPerRoom;

        PhotonNetwork.CreateRoom(ManagerUtilities.DefaultRoomName + System.Guid.NewGuid(), roomOptions, TypedLobby.Default);


    }
  
    private void OnLevelWasLoaded(int level)
    {
        if (level == 1)
            CancelInvoke();
        else if (Instance != null)
            Destroy(this.gameObject);
    }

    #region Chat
    void LateUpdate()
    {

        if (chatClient != null)
            chatClient.Service();
    }
    public void ClickSendMessage(Text text)
    {
        chatClient.PublishMessage(PhotonNetwork.CurrentRoom.Name, text.text);

        text.text = "";
    }
    void AddMessage(string _message, string _sender)
    {
        GameObject instance = Instantiate(ChatMessageUI, ChatMessagesGrid);
        ChatMessageUI chatMessageUI = instance.GetComponent<ChatMessageUI>();
        chatMessageUI.SetValues(_sender, _message);
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (ChatMessagesGrid == null)
            return;

        string msgs = "";
        for (int i = 0; i < senders.Length; i++)
        {
            AddMessage(messages[i].ToString(), senders[i]);

        }

    }

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnDisconnected()
    {
    }

    public void OnConnected()
    {


    }

    public void OnChatStateChange(ChatState state)
    {
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
    }

    public void OnUnsubscribed(string[] channels)
    {
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnUserSubscribed(string channel, string user)
    {

    }

    public void OnUserUnsubscribed(string channel, string user)
    {
    }

    #endregion
}
