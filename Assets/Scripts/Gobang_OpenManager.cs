using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public enum InRoomState
{
    NotChoose = 0,
    JoinOrCreate = 1,
    Join = 2,
}

public class Gobang_OpenManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField nameInput_IF;
    public TMP_InputField roomInput_IF;

    public InRoomState inRoomState = InRoomState.NotChoose;

    public List<string> rooms = new List<string>(); 

    public void OnPlayButtonClick()
    {
        if(nameInput_IF.text == "")
        {
            Gobang_ErrorManager.instance.SentErrorMessage("請輸入名字");
            print("Name ERROR");
            return;
        }

        // Connect on Web
        inRoomState = InRoomState.JoinOrCreate;
        PhotonNetwork.LocalPlayer.NickName = nameInput_IF.text;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnJoinButtonClick()
    {
        if (string.IsNullOrEmpty(nameInput_IF.text) || string.IsNullOrEmpty(roomInput_IF.text))
        {
            Gobang_ErrorManager.instance.SentErrorMessage("請輸入名字或房號");
            print("Name Or RoomName ERROR");
            return;
        }

        // Connect on Web
        inRoomState = InRoomState.Join;
        PhotonNetwork.LocalPlayer.NickName = nameInput_IF.text;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("On Connected To Master");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        print("On Joined Lobby");

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;

        if (inRoomState == InRoomState.Join)
        {
            PhotonNetwork.JoinRoom(roomInput_IF.text);
        }
        else
        {
            print("join or create");
            if (rooms.Count == 0)
            {
                print("0");
                PhotonNetwork.CreateRoom(Random.Range(1, 999).ToString("000"), roomOptions, TypedLobby.Default);
            }
            else
            {
                print("1");
                string roomName = rooms[Random.Range(0, rooms.Count)];
                PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
            }
            
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        PhotonNetwork.Disconnect();
        print("disconnect");
        Gobang_ErrorManager.instance.SentErrorMessage(message);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("On Joined Room");

        PhotonNetwork.LoadLevel("Main");
    }

    

    public void Quit()
    {
        Application.Quit();
    }
}
