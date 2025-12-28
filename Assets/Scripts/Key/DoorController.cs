using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public string nextLevelName;
    private Animator anim;
    private bool isOpened = false;

    AudioSource audioSource;
    [SerializeField] private AudioClip doorOpenSound;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();    
        anim = GetComponent<Animator>();   
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            int currentKeys = PlayerPrefs.GetInt("LevelKeys", 0);

            if (currentKeys >= 3)
            {
                StartCoroutine(OpenDoorSequence());
            }
        }
    }

    IEnumerator OpenDoorSequence()
    {
        isOpened = true;

        audioSource.PlayOneShot(doorOpenSound, 0.5f);

        if (anim != null)
        {
            anim.SetTrigger("Open");
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.green;
        }

        yield return new WaitForSeconds(doorOpenSound.length);
        SceneManager.LoadScene(nextLevelName);
    }
}