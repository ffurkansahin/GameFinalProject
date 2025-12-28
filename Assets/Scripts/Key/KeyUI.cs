using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class KeyUI : MonoBehaviour
{
    public Text keyText;

    void Start()
    {
        PlayerPrefs.SetInt("LevelKeys", 0);
        PlayerPrefs.Save();
    }

    void Update()
    {
        int currentKeys = PlayerPrefs.GetInt("LevelKeys", 0);

        keyText.text = "x " + currentKeys + " / 3";
    }
}