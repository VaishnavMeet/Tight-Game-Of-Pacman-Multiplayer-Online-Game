using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum PlayerTurn { Player1, Player2 }
    public PlayerTurn currentTurn = PlayerTurn.Player1;

    public int maxObstaclesPerPlayer = 8;
    private int player1Obstacles = 0;
    private int player2Obstacles = 0;

    public UnityEvent<PlayerTurn> OnTurnSwitched;

    public Text obstacleInfoText;
    public Text winText;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CanPlaceObstacle()
    {
        return currentTurn == PlayerTurn.Player1 ? player1Obstacles < maxObstaclesPerPlayer : player2Obstacles < maxObstaclesPerPlayer;
    }

    public void ObstaclePlaced()
    {
        if (currentTurn == PlayerTurn.Player1)
        {
            player1Obstacles++;
            
        }
        else
        {
            player2Obstacles++;
           
        }
        
        SwitchTurn();
    }
    void UpdateObstacleUI()
    {
        if (obstacleInfoText != null)
        {
            int remaining = maxObstaclesPerPlayer - GetPlayerObstacles(currentTurn);
            obstacleInfoText.text = remaining.ToString();
        }
    }
    public bool IsMyTurn(PlayerTurn turn) => currentTurn == turn;

    public void SwitchTurn()
    {
        currentTurn = (currentTurn == PlayerTurn.Player1) ? PlayerTurn.Player2 : PlayerTurn.Player1;
        //Debug.Log($"Now it's {currentTurn}'s turn");
        OnTurnSwitched?.Invoke(currentTurn);
        UpdateObstacleUI();
    }

    public bool IsGameSetupComplete()
    {
        return player1Obstacles >= maxObstaclesPerPlayer && player2Obstacles >= maxObstaclesPerPlayer;
    }

    public int GetPlayerObstacles(PlayerTurn turn)
    {
        return turn == PlayerTurn.Player1 ? player1Obstacles : player2Obstacles;
    }
}
