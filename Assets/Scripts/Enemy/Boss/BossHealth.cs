using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float maxHealth = 300f; // Bosses have more HP!
    public float currentHealth { get; private set; }
    public bool isDead = false;

    [Header("Visuals")]
    [SerializeField] float flashDuration = 0.1f;
    [SerializeField] int numberOfFlashes = 3;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BossAI aiScript;

    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        aiScript = GetComponent<BossAI>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            
            StartCoroutine(FlashEffect());
            
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("die");

        // Disable Physics and AI
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        if (aiScript != null) aiScript.enabled = false;

        // Destroy after animation
        Destroy(gameObject, 3f);
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