using System.Collections;
using UnityEngine;

public class BattleMageAI : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public Transform pointA;
    public Transform pointB;

    [Header("Combat")]
    public float chaseRange = 6f;
    public float attackRange = 1.5f;
    public int damage = 15;
    public float attackCooldown = 2.0f;
    
    // Timings (Adjust these to match your animations!)
    public float attack1Delay = 0.5f; // Time until damage for Attack 1
    public float attack2Delay = 0.7f; // Time until damage for Attack 2 (Maybe slower?)

    // Internal State
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth healthScript; // Your existing health script
    private Transform player;
    private Health playerHealth;      // Reference to player's health
    private Transform currentPatrolTarget;
    
    private float lastAttackTime;
    private bool isBusy = false;      // True if attacking or hurt
    private Coroutine attackCoroutine; // Stored to cancel if hurt

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<EnemyHealth>();
        currentPatrolTarget = pointB;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<Health>();
        }
    }

    void Update()
    {
        // 1. Dead/Hurt Check
        if (healthScript.currentHealth <= 0 || isBusy)
        {
            rb.velocity = Vector2.zero; 
            return;
        }

        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // 2. Chase or Patrol
        if (distToPlayer < chaseRange)
        {
            // --- COMBAT MODE ---
            FaceTarget(player.position);

            if (distToPlayer <= attackRange)
            {
                // In range: Stop and Attack
                rb.velocity = Vector2.zero;
                anim.SetBool("run", false);

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    PerformRandomAttack(); // Pick 1 or 2
                }
            }
            else
            {
                // Chase
                anim.SetBool("run", true);
                MoveToward(player.position, chaseSpeed);
            }
        }
        else
        {
            // --- PATROL MODE ---
            Patrol();
        }
    }

    void PerformRandomAttack()
    {
        lastAttackTime = Time.time;
        isBusy = true; // Lock movement

        // 1. Pick Random Attack (0 or 1)
        int rand = Random.Range(0, 2); 
        anim.SetInteger("attackIndex", rand); // Tell Animator which one
        anim.SetTrigger("meleeAttack");       // Pull the trigger

        // 2. Start the correct damage timer
        float delay = (rand == 0) ? attack1Delay : attack2Delay;
        attackCoroutine = StartCoroutine(DealDamage(delay));
    }

    IEnumerator DealDamage(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Check if player is still in range to get hit
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        // Wait a tiny bit more for animation to finish before moving
        yield return new WaitForSeconds(0.5f); 
        isBusy = false; // Unlock movement
    }

    // Called by EnemyHealth if we get hit
    public void CancelAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        isBusy = false;
        anim.ResetTrigger("meleeAttack");
    }

    // --- MOVEMENT HELPERS ---

    void Patrol()
    {
        anim.SetBool("run", true);
        MoveToward(currentPatrolTarget.position, patrolSpeed);

        // Check X distance only to avoid getting stuck
        if (Mathf.Abs(transform.position.x - currentPatrolTarget.position.x) < 0.5f)
        {
            currentPatrolTarget = (currentPatrolTarget == pointB) ? pointA : pointB;
        }
    }

    void MoveToward(Vector2 target, float speed)
    {
        float direction = Mathf.Sign(target.x - transform.position.x);
        rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        FaceTarget(target);
    }

    void FaceTarget(Vector3 target)
    {
        if (target.x > transform.position.x && transform.localScale.x < 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (target.x < transform.position.x && transform.localScale.x > 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}