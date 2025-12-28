using UnityEngine;

public class KeyCollectible : MonoBehaviour
{
    [SerializeField] private AudioClip keyCollectSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            int currentKeys = PlayerPrefs.GetInt("LevelKeys", 0);

            PlayerPrefs.SetInt("LevelKeys", currentKeys + 1);
            PlayerPrefs.Save(); 
            AudioSource.PlayClipAtPoint(keyCollectSound, transform.position, 1.5f);

            Destroy(gameObject);
        }
    }
}