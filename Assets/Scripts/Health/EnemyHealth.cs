using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float maxHealth = 100f;
    public float currentHealth { get; private set; }
    
    [Header("Visuals")]
    [SerializeField] float flashDuration = 0.1f;
    [SerializeField] int numberOfFlashes = 3;
    
    private bool isDead = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyAI aiScript; // Reference to the AI to stop it moving

    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        aiScript = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth > 0)
        {
            animator.SetTrigger("hurt");
            StartCoroutine(FlashEffect());

            // --- THE FIX ---
            // When hurt, find the AI and tell it to STOP attacking immediately
            if (aiScript != null)
            {
                aiScript.CancelAttack();
            }
            // ----------------
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("die"); // Triggers Death animation
        
        // Disable AI and Physics
        if (aiScript != null) aiScript.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        // Destroy after 2 seconds (time for death anim)
        Destroy(gameObject, 1.5f);
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