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
    private EnemyAI aiScript;

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

            if (aiScript != null)
            {
                aiScript.CancelAttack();
            }
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
        
        if (aiScript != null) aiScript.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        Destroy(gameObject, 1.5f);
    }

    public IEnumerator FlashWhite()
    {
        Color originalColor = spriteRenderer.color;
        
        spriteRenderer.color = Color.white; 
        
        spriteRenderer.color = new Color(1, 0, 0, 1);
        
        yield return new WaitForSeconds(0.1f);
        
        spriteRenderer.color = Color.white;
    }
}