using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;
    
    [Header("Roll Settings")] // NEW
    public float rollForce = 15f;
    public float rollDuration = 0.5f;
    public float rollCooldown = 1f;
    private bool canRoll = true;
    private bool isRolling = false;

    [Header("Wall Settings")]
    public float wallSlidingSpeed = 2f;
    public float wallJumpForceX = 15f;
    public float wallJumpForceY = 15f;

    [Header("Gravity (Anti-Float)")]
    public float defaultGravity = 3f;   
    public float fallMultiplier = 5f;   

    [Header("Detection")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.5f;

    // Components
    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator anim;

    // State Variables
    private float horizontalInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    
    [Header("Coyote Time Settings")]
    public float coyoteTime = 0.2f; 
    private float coyoteTimeCounter; 
    private float jumpBufferCounter;
    public GameObject dustEffect;      // Drag Dust Prefab here
    public Transform dustSpawnPoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = defaultGravity;
    }

    void Update()
    {
        // 1. STOP EVERYTHING IF ROLLING
        if (isRolling) return;

        // 2. Input Processing
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 3. Jump Buffering / Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime; 
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; 
        }

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            Jump(); 
        }
        
        // --- NEW: ROLL INPUT ---
        if (Input.GetKeyDown(KeyCode.LeftShift) && canRoll && isGrounded)
        {
            StartCoroutine(PerformRoll());
        }

        // 4. Flip Character
        if (horizontalInput != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(horizontalInput) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // 5. Check Surroundings
        CheckGround();
        CheckWall();
        
        // 6. Animation Updates
        anim.SetBool("run", horizontalInput != 0 && isGrounded);
        anim.SetBool("grounded", isGrounded); // Kept your parameter name
        
        // --- NEW: Send Vertical Speed for Jump/Fall Transitions ---
        anim.SetFloat("yVelocity", rb.velocity.y); 

        // 7. Wall Slide Logic
        if (isTouchingWall && !isGrounded && horizontalInput != 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    void FixedUpdate()
    {
        if (isRolling) return; // Don't apply movement physics while rolling

        // --- MOVEMENT ---
        if (!isWallSliding)
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }

        // --- WALL JUMP ---
        else if (jumpBufferCounter > 0 && isWallSliding)
        {
            PerformWallJump();
            jumpBufferCounter = 0;
        }

        // --- GRAVITY ---
        ApplyBetterGravity();

        // --- WALL SLIDE ---
        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
    }

    // --- NEW ROLL COROUTINE ---
    IEnumerator PerformRoll()
    {
        canRoll = false;
        isRolling = true;

        // 1. GET REFERENCES
        Health health = GetComponent<Health>();
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        // 2. ACTIVATE INVINCIBILITY (God Mode)
        // This tells Health.cs to ignore all damage and red flashes
        if (health != null) health.isInvincible = true;

        // 3. DISABLE COLLISIONS (Ghost Mode)
        // This tells Unity: "Player Layer and Enemy Layer can no longer touch."
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        // 4. PERFORM THE ROLL
        anim.SetTrigger("roll");
        
        float direction = Mathf.Sign(transform.localScale.x); 
        rb.velocity = new Vector2(direction * rollForce, rb.velocity.y);

        yield return new WaitForSeconds(rollDuration);

        // 5. RESET EVERYTHING
        isRolling = false;
        
        // Turn off God Mode
        if (health != null) health.isInvincible = false;
        
        // Re-enable Collisions (So you can get hit again)
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        // 6. COOLDOWN
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        coyoteTimeCounter = 0f; 
        anim.SetTrigger("jump");
        if (dustEffect != null && dustSpawnPoint != null)
        {
            Instantiate(dustEffect, dustSpawnPoint.position, Quaternion.identity);
        }
    }

    void PerformWallJump()
    {
        float wallDir = -Mathf.Sign(transform.localScale.x);
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(wallDir * wallJumpForceX, wallJumpForceY), ForceMode2D.Impulse);
        
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void ApplyBetterGravity()
    {
        if (rb.velocity.y < 0 && !isWallSliding)
        {
            rb.gravityScale = fallMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravity;
        }
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            coll.bounds.center, 
            coll.bounds.size, 
            0f, 
            Vector2.down, 
            groundCheckDistance, 
            groundLayer
        );

        isGrounded = hit.collider != null;
    }

    private void CheckWall()
    {
        float direction = Mathf.Sign(transform.localScale.x);
        RaycastHit2D hit = Physics2D.Raycast(
            coll.bounds.center, 
            Vector2.right * direction, 
            wallCheckDistance, 
            wallLayer 
        );

        isTouchingWall = hit.collider != null;
    }

    void OnDrawGizmos()
    {
        if (coll == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(coll.bounds.center + Vector3.down * groundCheckDistance, coll.bounds.size);

        Gizmos.color = Color.blue;
        float direction = Mathf.Sign(transform.localScale.x);
        Gizmos.DrawLine(coll.bounds.center, coll.bounds.center + Vector3.right * direction * wallCheckDistance);
    }

    public bool canAttack()
    {
        // Added !isRolling so you can't attack mid-roll
        return !isWallSliding && !isRolling;
    }
}