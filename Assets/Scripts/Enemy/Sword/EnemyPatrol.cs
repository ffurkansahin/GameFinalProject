using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("AI Settings")]
    public float speed = 2f;
    public float chaseRange = 5f;  // How close player must be to start chasing
    public float attackRange = 1.5f; // How close to stop and attack
    
    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentTarget;
    private Transform player; // Reference to the player
    private MeleeEnemy combatScript; // To check if stunned
    private bool isAggro = false;

    // Attack Cooldown
    private float lastAttackTime;
    public float attackCooldown = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        combatScript = GetComponent<MeleeEnemy>();
        currentTarget = pointB;
        
        // Auto-find the player (Make sure your Player has the "Player" Tag!)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (combatScript != null && (combatScript.isStunned || !this.enabled)) return;
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // LOGIC CHANGE:
        // If we are close enough, OR if we are already angry (isAggro), keep chasing.
        if (distanceToPlayer < chaseRange || isAggro)
        {
            isAggro = true; // Lock the enemy in "Chase Mode" forever
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }
    }

    void ChasePlayer(float distance)
    {
        // Face the player
        FaceTarget(player.position);

        if (distance <= attackRange)
        {
            // STOP AND ATTACK
            rb.velocity = Vector2.zero;
            anim.SetBool("run", false);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                anim.SetTrigger("attack"); // Make sure you have an "attack" trigger in Animator!
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // RUN TOWARD PLAYER
            anim.SetBool("run", true);
            
            // Move toward player position
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        }
    }

    void Patrol()
    {
        FaceTarget(currentTarget.position);
        anim.SetBool("run", true);

        // Move toward patrol point
        if (currentTarget == pointB)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
        }

        // Switch patrol points when close
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.5f)
        {
            if (currentTarget == pointB) currentTarget = pointA;
            else currentTarget = pointB;
        }
    }

    // A smarter Flip function that looks at where we want to go
    void FaceTarget(Vector3 target)
    {
        if (target.x > transform.position.x && transform.localScale.x < 0)
        {
            // Target is to the right, but we face left -> FLIP
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else if (target.x < transform.position.x && transform.localScale.x > 0)
        {
            // Target is to the left, but we face right -> FLIP
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }
    
    private void OnDrawGizmos()
    {
        // Visualize the Chase Range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        Gizmos.color = Color.yellow;
        if(pointA && pointB) Gizmos.DrawLine(pointA.position, pointB.position);
    }
}