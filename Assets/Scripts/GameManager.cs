using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool isGamePaused = false;
    public Image deathOverlay;
    public Button restartButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FreezeGame() {
        Instance.isGamePaused = true;
        deathOverlay.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void RestartGame()
    {
        deathOverlay.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        SceneManager.LoadScene("MainMenu");
        Instance.isGamePaused = false;
    }
}