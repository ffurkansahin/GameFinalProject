using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Setup")]
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float wakeUpRange = 6f; // NEW: How close you must be to wake him up
    
    [Header("Combat Timings")]
    public float spawnAnimTime = 8f;   
    public float attackWindup = 0.5f;    
    public float attackCooldown = 2.0f;
    public int damage = 25;
    [Header("Knockback Settings")]
    public float knockbackForce = 15f; // How hard to push (Try 10 to 20)
    public float stunTime = 0.5f;      // How long player can't move

    [Header("References")]
    public Transform player;

    private Rigidbody2D rb;
    private Animator anim;
    private BossHealth healthScript;
    private Health playerHealth;
    
    private bool isActivated = false; // Is he awake yet?
    private bool isSpawning = false;  
    private bool isBusy = false;    
    private float lastAttackTime;
    private Coroutine attackCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<BossHealth>();

        // 1. If Player slot is empty, find by Tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) 
            {
                player = p.transform;
            }
        }

        // 2. CRITICAL FIX: Get the Health script NOW (Outside the if-statement)
        if (player != null)
        {
            // We use GetComponentInChildren just in case the script is on the sprite
            playerHealth = player.GetComponentInChildren<Health>(); 
            
            if (playerHealth == null) 
            {
                Debug.LogError("BOSS ERROR: Found 'Player' object, but it has no 'Health.cs' script attached!");
            }
        }
    }

    void Update()
    {
        // 1. Safety Checks
        if (healthScript.isDead || player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. WAKE UP LOGIC
        if (!isActivated)
        {
            if (distToPlayer < wakeUpRange)
            {
                Debug.Log("Waking up Boss!"); // <--- ADD THIS LINE
                StartCoroutine(PlaySpawnSequence());
            }
            return; // Don't do anything else until woken up
        }

        // 3. Busy Check (Spawning/Attacking/Hurt)
        if (isSpawning || isBusy)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 4. COMBAT LOGIC
        FaceTarget();

        if (distToPlayer <= attackRange)
        {
            // Attack Mode
            rb.velocity = Vector2.zero;
            anim.SetBool("run", false);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                attackCoroutine = StartCoroutine(PerformAttack());
            }
        }
        else
        {
            // Chase Mode
            anim.SetBool("run", true);
            MoveTowardPlayer();
        }
    }

    IEnumerator PlaySpawnSequence()
    {
        isActivated = true;
        isSpawning = true;
        
        // --- THE FIX ---
        // Force him to look at the player immediately before the animation plays
        if (player != null) 
        {
            FaceTarget(); 
        }
        // ----------------

        // 1. FREEZE HIM (Optional, keeps him from sliding)
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static; 

        anim.SetTrigger("spawn");
        
        yield return new WaitForSeconds(spawnAnimTime);
        
        // 2. UNFREEZE HIM
        rb.bodyType = RigidbodyType2D.Dynamic;
        isSpawning = false;
    }

   IEnumerator PerformAttack()
    {
        isBusy = true;
        lastAttackTime = Time.time;

        anim.SetTrigger("attack");
        yield return new WaitForSeconds(attackWindup);

        // Huge Hit Box Logic
        float hitRange = attackRange + 3.0f; 

        if (player != null && Vector2.Distance(transform.position, player.position) <= hitRange)
        {
            // 1. Deal Damage
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                
                // 2. APPLY KNOCKBACK (The New Part)
                StartCoroutine(ApplyKnockback());
            }
        }

        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }
    public void CancelAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        isBusy = false;
        anim.ResetTrigger("attack");
    }

    void MoveTowardPlayer()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    void FaceTarget()
    {
        if (player.position.x > transform.position.x && transform.localScale.x < 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (player.position.x < transform.position.x && transform.localScale.x > 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wakeUpRange); // Draw Wake Up Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    IEnumerator ApplyKnockback()
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        PlayerController playerMoveScript = player.GetComponent<PlayerController>();

        if (playerRb != null)
        {
            // A. Stop player's current movement
            playerRb.velocity = Vector2.zero; 

            // B. Calculate direction: Player - Boss = Direction away from Boss
            Vector2 direction = (player.position - transform.position).normalized;
            // Optional: Add a little "Up" force so they fly in an arc
            direction.y = 0.5f; 

            // C. Disable Controls (So player doesn't stop the knockback)
            if (playerMoveScript != null) playerMoveScript.enabled = false;

            // D. BOOM! Push them.
            playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

            // E. Wait for stun to finish
            yield return new WaitForSeconds(stunTime);

            // F. Re-enable Controls
            if (playerMoveScript != null) playerMoveScript.enabled = true;
        }
    }
}