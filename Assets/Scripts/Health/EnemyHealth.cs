using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float maxHealth = 100f;
    public float currentHealth { get; private set; }
    
  
    
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
            StartCoroutine(FlashWhite());

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

    public IEnumerator FlashWhite()
    {
        // Save original material/color
        Color originalColor = spriteRenderer.color;
        
        // 1. Flash WHITE (Bright impact)
        spriteRenderer.color = Color.white; 
        // Note: If you use a custom shader, you might need a "FlashAmount" float instead.
        // For standard Sprites, setting color to White only works if the material allows it, 
        // otherwise it just brightens it. 
        
        // BETTER SIMPLE WAY: Toggle visibility or turn Red instantly
        spriteRenderer.color = new Color(1, 0, 0, 1); // Pure Red
        
        yield return new WaitForSeconds(0.1f);
        
        // 2. Return to normal
        spriteRenderer.color = Color.white; // Or originalColor
    }
}