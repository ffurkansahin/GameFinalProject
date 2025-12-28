using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void HitStop(float duration)
    {
        if (Time.timeScale > 0)
        {
            StartCoroutine(DoHitStop(duration));
        }
    }

    IEnumerator DoHitStop(float duration)
    {
        Time.timeScale = 0.0f; 

        yield return new WaitForSecondsRealtime(duration); 

        Time.timeScale = 1.0f;
    }
}