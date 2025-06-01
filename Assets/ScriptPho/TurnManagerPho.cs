using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class TurnManagerPho : MonoBehaviourPunCallbacks, IPunObservable
{
    public static TurnManagerPho Instance;

    public enum PlayerTurn { Player1, Player2 }
    public PlayerTurn currentTurn = PlayerTurn.Player1;

    public int maxObstaclesPerPlayer = 8;
    private int player1Obstacles = 0;
    private int player2Obstacles = 0;
    private int obstaclesPlacedThisTurn = 0;

    public UnityEvent<PlayerTurn> OnTurnSwitched;

    public Text obstacleInfoText;
    public Text winText;
    PhotonView photonView;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        UpdateObstacleUI();
    }

    public bool CanPlaceObstacle()
    {
        return (PhotonNetwork.IsMasterClient && currentTurn == PlayerTurn.Player1 && player1Obstacles < maxObstaclesPerPlayer)
            || (!PhotonNetwork.IsMasterClient && currentTurn == PlayerTurn.Player2 && player2Obstacles < maxObstaclesPerPlayer);
    }

    public void ObstaclePlaced()
    {
        if (!CanPlaceObstacle())
            return;

        // Increase total and per-turn count based on current player
       

        obstaclesPlacedThisTurn++;
        Debug.Log("plced " + player1Obstacles);
        if (obstaclesPlacedThisTurn >= 1)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                player1Obstacles++;
            }
            else
            {
                player2Obstacles++;
                photonView.RPC("IncrementPlayer2Obstacles", RpcTarget.MasterClient); // Keep master updated
            }
            obstaclesPlacedThisTurn = 0; // reset counter
            photonView.RPC("SwitchTurnRPC", RpcTarget.AllBuffered);
        }

        UpdateObstacleUI(); // Always update UI after placing
    }



    public void SwitchTurn()
    {
        photonView.RPC("SwitchTurnRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void IncrementPlayer2Obstacles()
    {
        player2Obstacles++;
    }

    [PunRPC]
    void SwitchTurnRPC()
    {
        currentTurn = (currentTurn == PlayerTurn.Player1) ? PlayerTurn.Player2 : PlayerTurn.Player1;
        obstaclesPlacedThisTurn = 0; // Reset on turn switch
        Debug.Log("Turn switched to: " + currentTurn);
        OnTurnSwitched?.Invoke(currentTurn);
        UpdateObstacleUI();
    }



    public bool IsMyTurn(PlayerTurn turn) => currentTurn == turn;

    void UpdateObstacleUI()
    {
        if (obstacleInfoText != null)
        {
            var isPlayer1 = PhotonNetwork.LocalPlayer.ActorNumber == 1;
            int count = isPlayer1 ? player1Obstacles : player2Obstacles;
            obstacleInfoText.text = (maxObstaclesPerPlayer - count).ToString();
        }
    }


    public bool IsGameSetupComplete()
    {
        return player1Obstacles >= maxObstaclesPerPlayer && player2Obstacles >= maxObstaclesPerPlayer;
    }

    public int GetPlayerObstacles(PlayerTurn turn)
    {
        return turn == PlayerTurn.Player1 ? player1Obstacles : player2Obstacles;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)currentTurn);
            stream.SendNext(player1Obstacles);
            stream.SendNext(player2Obstacles);
        }
        else
        {
            currentTurn = (PlayerTurn)(int)stream.ReceiveNext();
            player1Obstacles = (int)stream.ReceiveNext();
            player2Obstacles = (int)stream.ReceiveNext();

            UpdateObstacleUI();
        }
    }
}
