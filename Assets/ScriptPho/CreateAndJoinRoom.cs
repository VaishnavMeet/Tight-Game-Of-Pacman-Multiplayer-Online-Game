using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateAndJoinRoom : MonoBehaviourPunCallbacks
{
    public InputField roomtext;

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomtext.text, new RoomOptions { MaxPlayers = 2, IsOpen = true, IsVisible = true });
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("connected !!!");
        PhotonNetwork.JoinLobby();
       
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        print("disconnected");
       
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        print("Room created");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        print("filed to create room");
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomtext.text);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("Room joined");

        if (PhotonNetwork.CountOfPlayersInRooms == 0)
        {
            PhotonNetwork.NickName = "Player 1";
        }
        else
        {
            PhotonNetwork.NickName = "Player 2";
        }
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("Room joined failed");
        print(message);
    }
}
