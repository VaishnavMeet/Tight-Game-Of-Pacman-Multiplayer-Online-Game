using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks
{

    public Text roomNameText; // Drag and drop via Inspector
    private bool isConnectedToLobby = false;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

}
