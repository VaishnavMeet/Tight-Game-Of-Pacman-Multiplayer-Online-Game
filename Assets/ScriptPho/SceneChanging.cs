using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("Game"); 
    }

    public void LoadSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
