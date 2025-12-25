using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerOld : MonoBehaviour
{
    [SerializeField] float groundSpeed;
    [SerializeField] float airSpeed;
    
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask wallLayer;
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D boxCollider;
    private float wallJumpCd;
    private float horizontalInput;
    private float currentMoveSpeed => isGrounded() ? groundSpeed : airSpeed;
    [Header("Jump Settings")]
// Increase these numbers! Force needs higher values than velocity.
// Try starting around 15-20 for jumpForce.
    public float jumpForce = 16f; 
    public float wallHopForce = 15f; // Force when jumping off wall (no input)
    public float wallJumpX = 12f;    // Force X when climbing/jumping (with input)
    public float wallJumpY = 14f;    // Force Y when climbing/jumping (with input)
   

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(6f, 6f, 6f);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-6f, 6f, 6f);
        }

        animator.SetBool("run", horizontalInput != 0);
        animator.SetBool("grounded", isGrounded());
        

        if (wallJumpCd > 0.2f)
        {
            rb.velocity = new Vector2(horizontalInput * currentMoveSpeed, rb.velocity.y);

            if (onWall() && !isGrounded())
            {
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;
            }
            else
                rb.gravityScale = 1;

            if (Input.GetKeyDown(KeyCode.Space))
                Jump();

        }
        else
        {
            wallJumpCd += Time.deltaTime;
        }

    }

   void Jump()
{
    // --- 1. GROUND JUMP ---
    if (isGrounded())
    {
        // CRITICAL: Reset Y velocity to 0 before adding force. 
        // This prevents the "heavy" feeling if you press jump while slightly falling.
        rb.velocity = new Vector2(rb.velocity.x, 0);

        // Apply instant upward force
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        animator.SetTrigger("jump");
    }
    // --- 2. WALL JUMP ---
    else if (onWall() && !isGrounded())
    {
        // Get direction: 1 is right, -1 is left
        float facingDirection = Mathf.Sign(transform.localScale.x);

        // Kill all current momentum so the wall jump feels snappy and consistent
        rb.velocity = Vector2.zero; 

        if (horizontalInput == 0)
        {
            // Option A: "Wall Hop" (Pushing away from wall with no input)
            // Force pushes strictly horizontal away from the wall
            rb.AddForce(new Vector2(-facingDirection * wallHopForce, 5f), ForceMode2D.Impulse);

            // Handle the Flip
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
        }
        else
        {
            // Option B: "Wall Climb/Jump" (Holding input)
            // Force pushes Up and Away
            rb.AddForce(new Vector2(-facingDirection * wallJumpX, wallJumpY), ForceMode2D.Impulse);
        }

        wallJumpCd = 0;
    }
}

    bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, new Vector2(transform.localScale.x, 0f), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public bool canAttack()
    {
        return !onWall();
    }
}
