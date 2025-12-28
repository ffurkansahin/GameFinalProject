using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject pauseScreen;
    public static bool GameIsPaused = false;

    void Awake()
    {
        gameOverScreen.SetActive(false);
        pauseScreen.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameOverScreen.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
     public void Menu()
    {
        SceneManager.LoadScene(0);
    }
     public void Quit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(pauseScreen.activeInHierarchy)
            {
                PauseGame(false);
            }
            else
            {
                PauseGame(true);
            }
        }
    }

    public void PauseGame(bool status)
    {
        pauseScreen.SetActive(status);
        GameIsPaused = status;

        if(status)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }      
}
