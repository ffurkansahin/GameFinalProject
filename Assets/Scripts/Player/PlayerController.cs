using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;

    [Header("Roll Settings")]
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

    // --- DOUBLE JUMP DE���KEN� ---
    private bool canDoubleJump = true;

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
    public GameObject dustEffect;
    public Transform dustSpawnPoint;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip walkSound;
    private float footstepTimer;
    public float footstepDelay = 0.3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = defaultGravity;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (UIManager.GameIsPaused) return;

        if (isRolling) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (coyoteTimeCounter > 0f)
            {
                Jump();
            }
            else if (canDoubleJump && !isWallSliding)
            {
                Jump();
                canDoubleJump = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canRoll && isGrounded)
        {
            StartCoroutine(PerformRoll());
        }

        if (horizontalInput != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(horizontalInput) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        CheckGround();
        CheckWall();

        anim.SetBool("run", horizontalInput != 0 && isGrounded);
        anim.SetBool("grounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);

        if (isTouchingWall && !isGrounded && horizontalInput != 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        if (isGrounded && horizontalInput != 0 && !isRolling)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                PlaySound(walkSound, 0.05f);
                footstepTimer = footstepDelay;
            }
        }
    }

    void FixedUpdate()
    {
        if (isRolling) return;

        if (!isWallSliding)
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }

        else if (jumpBufferCounter > 0 && isWallSliding)
        {
            PerformWallJump();
            jumpBufferCounter = 0;
        }

        ApplyBetterGravity();

        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
    }

    IEnumerator PerformRoll()
    {
        canRoll = false;
        isRolling = true;

        Health health = GetComponent<Health>();
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (health != null) health.isInvincible = true;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        anim.SetTrigger("roll");

        float direction = Mathf.Sign(transform.localScale.x);
        rb.velocity = new Vector2(direction * rollForce, rb.velocity.y);

        yield return new WaitForSeconds(rollDuration);

        isRolling = false;

        if (health != null) health.isInvincible = false;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);

        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        coyoteTimeCounter = 0f;

        PlaySound(jumpSound, 0.1f);
        
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

    public bool canAttack()
    {
        return !isWallSliding;
    }

    void OnDrawGizmos()
    {
        if (coll == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(coll.bounds.center + Vector3.down * groundCheckDistance, coll.bounds.size);

        Gizmos.color = Color.blue;
        float direction = transform.localScale.x > 0 ? 1 : -1;
        Gizmos.DrawLine(coll.bounds.center, coll.bounds.center + Vector3.right * direction * wallCheckDistance);
    }
    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}