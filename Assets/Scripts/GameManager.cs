using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        // Singleton pattern: Easier to access from other scripts
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void HitStop(float duration)
    {
        if (Time.timeScale > 0) // Don't freeze if already paused
        {
            StartCoroutine(DoHitStop(duration));
        }
    }

    IEnumerator DoHitStop(float duration)
    {
        // 1. FREEZE TIME
        Time.timeScale = 0.0f; 

        // 2. Wait (We must use explicit unscaled time because time is stopped)
        yield return new WaitForSecondsRealtime(duration); 

        // 3. UNFREEZE TIME
        Time.timeScale = 1.0f;
    }
}