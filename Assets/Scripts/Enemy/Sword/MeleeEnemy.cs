using System.Collections;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float range = 1.5f;
    [SerializeField] float colliderDistance = 1f;
    [SerializeField] int damage = 20;
    
    // NEW: Adjust this number to match when the sword actually hits (e.g., 0.4 seconds)
    [SerializeField] float damageDelay = 0.4f; 
    
    [SerializeField] CapsuleCollider2D capsuleCollider;
    [SerializeField] LayerMask playerLayer;
    
    [Header("Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float stunTime = 0.5f; 
    public bool isStunned = false; 

    private float cooldownTimer = Mathf.Infinity;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyPatrol enemyPatrol;
    private Health playerHealth;

    void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        // Find player once
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null) playerHealth = player.GetComponent<Health>();
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (isStunned) return;

        bool playerInSight = PlayerInSight();

        if (playerInSight)
        {
            if (cooldownTimer > attackCooldown)
            {
                Attack();
            }
        }

        if(enemyPatrol != null) enemyPatrol.enabled = !playerInSight;
    }

    private void Attack()
    {
        anim.SetTrigger("meleeAttack");
        cooldownTimer = 0;
        
        // THE FIX: Wait for the animation to reach the "Hit" frame, then deal damage
        StartCoroutine(DealDamageAfterDelay());
    }

    private IEnumerator DealDamageAfterDelay()
    {
        // 1. Wait for the sword to swing (adjust this number in Inspector)
        yield return new WaitForSeconds(damageDelay);

        // 2. Check if player is STILL in range (so they can dodge)
        if (PlayerInSight() && playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    public bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            capsuleCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(capsuleCollider.bounds.size.x * range, capsuleCollider.bounds.size.y, capsuleCollider.bounds.size.z), 
            0, Vector2.left, 0, playerLayer);

        return hit.collider != null;
    }

    // --- KEEPING YOUR EXISTING HEALTH/PHYSICS LOGIC BELOW ---

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if(anim != null) anim.SetTrigger("hurt");
        if (currentHealth <= 0) Die();
    }
    
    public void ApplyKnockback(Transform attacker)
    {
        Vector2 direction = (transform.position - attacker.position).normalized;
        StopCoroutine(ResetStun()); 
        isStunned = true;
        if (enemyPatrol != null) enemyPatrol.enabled = false; 
        rb.velocity = Vector2.zero; 
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        StartCoroutine(ResetStun());
    }

    IEnumerator ResetStun()
    {
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
        if (enemyPatrol != null) enemyPatrol.enabled = true;
    }

   void Die()
    {
        if(anim != null) anim.SetBool("isDead", true);
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static; 
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        if (enemyPatrol != null) enemyPatrol.enabled = false;
        Destroy(gameObject, 2.0f); 
    }
    
    void OnDrawGizmos()
    {
        if(capsuleCollider != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(capsuleCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,  new Vector3(capsuleCollider.bounds.size.x * range, capsuleCollider.bounds.size.y, capsuleCollider.bounds.size.z));
        }
    }
}