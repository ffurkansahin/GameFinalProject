using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float maxHealth = 300f;
    public float currentHealth { get; private set; }
    public bool isDead = false;

    [Header("Visuals")]
    [SerializeField] float flashDuration = 0.1f;
    [SerializeField] int numberOfFlashes = 3;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip hurtSound; 
    public AudioClip dieSound;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BossAI aiScript;

    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        aiScript = GetComponent<BossAI>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            if (audioSource != null && hurtSound != null)
            {
                audioSource.PlayOneShot(hurtSound);
            }
            StartCoroutine(FlashEffect());
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("die");

        if (audioSource != null && dieSound != null)
        {
            audioSource.PlayOneShot(dieSound);
        }

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        if (aiScript != null) aiScript.enabled = false;

        UIManager ui = FindObjectOfType<UIManager>();

        StartCoroutine(WaitAndShowVictory(ui));


        Destroy(gameObject, 4f);
    }

    IEnumerator WaitAndShowVictory(UIManager ui)
    {
        yield return new WaitForSeconds(2f);

        ui.ShowVictory();
    }

    private IEnumerator FlashEffect()
    {
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }
    }
}