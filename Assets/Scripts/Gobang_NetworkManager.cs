using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public enum GameState
{
    Ready = 1,
    GameOver = 3,
}

public class Gobang_NetworkManager : MonoBehaviourPunCallbacks
{
    public static Gobang_NetworkManager instance;

    public bool isGaming = false;
    public bool isPlayerMax = false;

    public GameObject player;
    public AudioSource marking_SFX;
    public PieceColor playerTurn = PieceColor.black;
    public GameState gameState = GameState.Ready;

    [Header("UI GameObject")]
    public GameObject gameOver_Obj;
    public Button readyButton;
    public GameObject waiting_Obj;
    public TextMeshProUGUI roomName_T;
    public TextMeshProUGUI winner_T;
    public TextMeshProUGUI selfName_T;
    public TextMeshProUGUI selfPiece_T;
    public TextMeshProUGUI selfReady_T;
    public TextMeshProUGUI enemyName_T;
    public TextMeshProUGUI enemyPiece_T;
    public TextMeshProUGUI enemyReady_T;
    public List<Toggle> turn_TG_List = new List<Toggle>();


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //PhotonNetwork.ConnectUsingSettings();
        SetUIState();
        SetGameSetting();
    }

    private void Update()
    {
        if (isPlayerMax == false)
            RoomisMax();
    }

    //public override void OnConnectedToMaster()
    //{
    //    base.OnConnectedToMaster();
    //    print("Connect To Server");

    //    // Join or Create room
    //    RoomOptions roomOptions = new RoomOptions();
    //    roomOptions.MaxPlayers = 2;
    //    PhotonNetwork.JoinOrCreateRoom("Gobang_0525", roomOptions, TypedLobby.Default);
    //}

    public void SetGameSetting()
    {
        // Create a network player
        if (player == null) return;
        GameObject newPlayer = PhotonNetwork.Instantiate(player.name, Vector3.zero, player.transform.rotation);

        // Init some setting on player
        if (PhotonNetwork.IsMasterClient)
        {
            newPlayer.GetComponent<PhotonView>().RPC("SetPieceColor", RpcTarget.All, PieceColor.black);
            //TODO : some setting
        }
        else
        {
            newPlayer.GetComponent<PhotonView>().RPC("SetPieceColor", RpcTarget.All, PieceColor.white);

        }


    }

    //public override void OnJoinedRoom()
    //{
    //    base.OnJoinedRoom();
    //    print("On join room");

    //    // Create a network player
    //    if (player == null) return;
    //    GameObject newPlayer = PhotonNetwork.Instantiate(player.name, Vector3.zero, player.transform.rotation);

    //    // Init some setting on player
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        newPlayer.GetComponent<PhotonView>().RPC("SetPieceColor", RpcTarget.All, PieceColor.black);
    //        //TODO : some setting
    //    }
    //    else
    //    {
    //        newPlayer.GetComponent<PhotonView>().RPC("SetPieceColor", RpcTarget.All, PieceColor.white);

    //    }

    //    readyButton.interactable = true;

    //}

    [PunRPC]
    public void ChangeTurn()
    {
        playerTurn = playerTurn == PieceColor.black ? PieceColor.white : PieceColor.black;

        
    }

    public void OnTurnUI()
    {
        var players = GameObject.FindObjectsOfType<Gobang_Player>();

        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                item.GetComponent<PhotonView>().RPC("ToogleTurn", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void GameOver(PieceColor winColor)
    {
        print("GAME OVER !!!");
        gameOver_Obj.SetActive(true);
        winner_T.text = winColor == PieceColor.black ? "黑方勝" : "白方勝";

        gameState = GameState.GameOver;
    }

    public void PlayMarkingSFX()
    {
        marking_SFX.Play();
    }

    // On Ready button Click
    public void OnClickReadyBtn()
    {
        var players = GameObject.FindObjectsOfType<Gobang_Player>();

        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                item.GetComponent<PhotonView>().RPC("SetPlayerReady", RpcTarget.All);
            }
        }
    }

    // Set UI state
    public void SetUIState()
    {
        roomName_T.text = "房號" + PhotonNetwork.CurrentRoom.Name;
        selfPiece_T.text = "";
        selfReady_T.text = "";
        enemyPiece_T.text = "";
        enemyReady_T.text = "";
    }

    public void SetName()
    {
        var players = FindObjectsOfType<Gobang_Player>();

        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
                selfName_T.text = item.GetComponent<PhotonView>().Owner.NickName;
            else
                enemyName_T.text = item.GetComponent<PhotonView>().Owner.NickName;
        }
    }

    public void SetSelfText(PieceColor pieceColor)
    {
        selfPiece_T.text = pieceColor == PieceColor.black ? "黑" : "白";
        selfReady_T.text = "未準備";
    }

    public void SetEnemyText(PieceColor pieceColor)
    {
        enemyPiece_T.text = pieceColor == PieceColor.black ? "黑" : "白";
        enemyReady_T.text = "未準備";
    }

    public void RoomisMax()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            readyButton.interactable = true;
            isPlayerMax = true;
        }
    }

    public void PlayAgain()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        print("On Left Room");

        PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        PhotonNetwork.Disconnect();
        PhotonNetwork.LoadLevel("Open");
    }
}

