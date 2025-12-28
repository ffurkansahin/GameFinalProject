using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panelleri")]
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject victoryScreen;
    [SerializeField] GameObject pauseScreen;

    [Header("Audio Settings")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip victorySound; 
    [SerializeField] AudioClip gameOverSound; 

    public static bool GameIsPaused = false;

    void Awake()
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (pauseScreen != null) pauseScreen.SetActive(false);
        if (victoryScreen != null) victoryScreen.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void ShowGameOver()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);

            if (audioSource != null && gameOverSound != null)
            {
                audioSource.PlayOneShot(gameOverSound);
            }
        }
    }

    public void ShowVictory()
    {
        if (victoryScreen != null)
        {
            victoryScreen.SetActive(true);
            Time.timeScale = 0f;
            GameIsPaused = true;

            if (audioSource != null && victorySound != null)
            {
                audioSource.PlayOneShot(victorySound);
            }
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseScreen != null)
            {
                if (pauseScreen.activeInHierarchy)
                {
                    PauseGame(false);
                }
                else
                {
                    PauseGame(true);
                }
            }
        }
    }

    public void PauseGame(bool status)
    {
        if (pauseScreen != null) pauseScreen.SetActive(status);
        GameIsPaused = status;

        if (status)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}