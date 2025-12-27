using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public Transform pointA;
    public Transform pointB;

    [Header("Combat Ranges")]
    public float chaseRange = 5f;   // Distance to spot player
    public float attackRange = 1.2f; // Distance to stop and swing

    [Header("Animation Timings (Seconds)")]
    public float reactAnimTime = 1.0f;   // Length of "React" clip
    public float attackWindupTime = 0.4f; // Time until sword actually hits
    public float returnAnimTime = 0.8f;   // Length of "Return" (pickup) clip
    public float attackCooldown = 2.0f;   // Time between attacks
    private Coroutine attackCoroutine;

    [Header("Combat Stats")]
    public int damage = 10;

    // Internal State
    private Transform player;
    private Transform currentPatrolTarget;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth healthScript;
    private Health playerHealth; // Reference to player's health
    
    private bool isAggro = false;    // Has seen player?
    private bool isBusy = false;     // Is attacking, reacting, or stunned?
    private float lastAttackTime;

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
        // 1. Safety Checks
        if (healthScript.currentHealth <= 0 || isBusy)
        {
            rb.velocity = Vector2.zero; // Stop moving if busy/dead
            return; 
        }

        // 2. Locate Player
        if (player == null) return;
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // 3. Logic Tree
        if (isAggro)
        {
            // --- COMBAT MODE ---
            FaceTarget(player.position);

            if (distToPlayer <= attackRange)
            {
                // In range? Stop and Attack
                rb.velocity = Vector2.zero;
                anim.SetBool("run", false); // Matches "Walk" in your image

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    attackCoroutine = StartCoroutine(PerformAttackSequence());
                }
            }
            else
            {
                // Out of range? Chase
                anim.SetBool("run", true);
                MoveToward(player.position, chaseSpeed);
            }
        }
        else
        {
            // --- PATROL MODE ---
            // If player is close, trigger REACT
            if (distToPlayer < chaseRange)
            {
                StartCoroutine(PerformReactSequence());
            }
            else
            {
                Patrol();
            }
        }
        
    }

    // --- COROUTINES (The "Scripted" Events) ---

    IEnumerator PerformReactSequence()
    {
        MoveToward(player.position, chaseSpeed);
        isBusy = true; // Lock movement
        rb.velocity = Vector2.zero;
        
        anim.SetTrigger("alert"); // Triggers "React" node in your image
        
        yield return new WaitForSeconds(reactAnimTime); // Wait for scream
        
        isAggro = true; // Now we are angry forever
        isBusy = false; // Unlock movement
    }

    IEnumerator PerformAttackSequence()
    {
        isBusy = true; // Lock movement during full combo
        lastAttackTime = Time.time;

        // 1. Start Attack Animation
        anim.SetTrigger("meleeAttack"); // Triggers "Attack" node
        
        // 2. Wait for the "Hit" moment (no events needed!)
        yield return new WaitForSeconds(attackWindupTime);

        // 3. Deal Damage (if player is still there)
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }

        // 4. Wait for the "Return" animation to finish
        // Logic: Attack anim finishes -> Transitions to Return -> Return Finishes
        // We wait here so he stands still while picking up the weapon
        yield return new WaitForSeconds(returnAnimTime); 

        isBusy = false; // Combo done, can chase again
    }

    // --- MOVEMENT HELPERS ---

    void Patrol()
    {
        anim.SetBool("run", true);
        MoveToward(currentPatrolTarget.position, patrolSpeed);

        // THE FIX: Check only X distance (Horizontal), ignore Y height
        float distanceToTargetX = Mathf.Abs(transform.position.x - currentPatrolTarget.position.x);

        // If we are horizontally close enough (0.5f), switch points
        if (distanceToTargetX < 0.5f)
        {
            // Switch current target
            if (currentPatrolTarget == pointB)
            {
                currentPatrolTarget = pointA;
            }
            else
            {
                currentPatrolTarget = pointB;
            }

            // Optional: Flip immediately so he doesn't moonwalk for a split second
            FaceTarget(currentPatrolTarget.position);
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
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else if (target.x < transform.position.x && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public void CancelAttack()
    {
        if (attackCoroutine != null) 
        {
            StopCoroutine(attackCoroutine); // Stops the damage timer instantly
        }
        
        isBusy = false; // Allows him to move again later
        // Optional: Reset animation triggers to prevent weird looping
        anim.ResetTrigger("meleeAttack"); 
    }
}