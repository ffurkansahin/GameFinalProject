using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Setup")]
    public float moveSpeed = 3f;
    public float attackRange = 3f; // Increased range for big boss
    public float wakeUpRange = 6f; 
    
    [Header("Cinematics")]
    public float spawnAnimTime = 3.0f; // Matches your slow animation
    public CameraShake cameraShake;    // Drag Main Camera here
    public CanvasGroup darknessPanel;  // Drag Darkness Panel here
    public MonoBehaviour cameraFollowScript; // DRAG YOUR "CAMERA FOLLOW" SCRIPT HERE!
    public float cameraSlideSpeed = 2.0f; // How fast camera moves to boss
    
    [Header("Combat")]
    public float attackWindup = 0.5f;    
    public float attackCooldown = 2.0f;
    public int damage = 25;
    [Header("Knockback")]
    public float knockbackForce = 15f; 
    public float stunTime = 0.5f;     

    [Header("References")]
    public Transform player;

    private Rigidbody2D rb;
    private Animator anim;
    private BossHealth healthScript;
    private Health playerHealth;
    
    private bool isActivated = false;
    private bool isSpawning = false;  
    private bool isBusy = false;    
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<BossHealth>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            playerHealth = player.GetComponentInChildren<Health>(); 
        }
    }

    void Update()
    {
        if (healthScript.isDead || player == null) return;

        // 1. WAKE UP LOGIC
        if (!isActivated)
        {
            if (Vector2.Distance(transform.position, player.position) < wakeUpRange)
            {
                StartCoroutine(PlaySpawnSequence());
            }
            return; 
        }

        if (isSpawning || isBusy)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 2. COMBAT LOGIC
        FaceTarget();
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("run", false);
            if (Time.time > lastAttackTime + attackCooldown) StartCoroutine(PerformAttack());
        }
        else
        {
            anim.SetBool("run", true);
            MoveTowardPlayer();
        }
    }

    // --- THE CINEMATIC SPAWN ---
    IEnumerator PlaySpawnSequence()
    {
        isActivated = true;
        isSpawning = true;
        Animator playerAnim = player.GetComponent<Animator>();
        if (playerAnim != null)
        {
            // 1. Reset parameters so he doesn't think he's running/falling
            playerAnim.SetBool("run", false);
            playerAnim.SetBool("grounded", true);
            playerAnim.SetFloat("yVelocity", 0);
            
            // 2. FORCE the animation state to "Idle" instantly
            // (Make sure your animation state is named "Idle" in the Animator window!)
            playerAnim.Play("Idle");
        }
        
        // 1. DISABLE CONTROLS (Player can't move)
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
        if (player.GetComponent<Rigidbody2D>() != null) player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        // 2. DISABLE CAMERA FOLLOW (So we can move camera manually)
        if (cameraFollowScript != null) cameraFollowScript.enabled = false;

        // 3. SLIDE CAMERA TO BOSS
        Transform camParams = Camera.main.transform;
        Vector3 originalCamPos = camParams.position;
        Vector3 bossCamPos = new Vector3(transform.position.x, transform.position.y, originalCamPos.z);
        
        yield return StartCoroutine(SlideCamera(camParams, bossCamPos, 1.0f));

        // 4. BOSS ROAR / EFFECTS
        if (player != null) FaceTarget(); 
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static; 

        // Shake & Darken
        if (cameraShake != null) StartCoroutine(cameraShake.Shake(spawnAnimTime, 0.2f)); // Mild shake
        StartCoroutine(FadeDarkness(0.6f, 1.0f));

        anim.SetTrigger("spawn");
        yield return new WaitForSeconds(spawnAnimTime);
        
        // 5. CLEANUP & RETURN
        StartCoroutine(FadeDarkness(0f, 1.0f));
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Slide Camera Back to Player
        yield return StartCoroutine(SlideCamera(camParams, originalCamPos, 0.5f));

        // 6. RE-ENABLE CONTROLS
        if (cameraFollowScript != null) cameraFollowScript.enabled = true;
        if (pc != null) pc.enabled = true;

        isSpawning = false;
    }

    // Helper to move camera smoothly
    IEnumerator SlideCamera(Transform cam, Vector3 targetPos, float duration)
    {
        Vector3 startPos = cam.position;
        float time = 0;

        while (time < duration)
        {
            cam.position = Vector3.Lerp(startPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cam.position = targetPos;
    }

    IEnumerator FadeDarkness(float targetAlpha, float duration)
    {
       if (darknessPanel == null) 
        {
            Debug.LogError("ERROR: Darkness Panel is NOT assigned in the Boss Inspector!");
            yield break;
        }
        
        Debug.Log("Fading darkness..."); // Check your console for this!
        float startAlpha = darknessPanel.alpha;
        float time = 0;
        while (time < duration)
        {
            darknessPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        darknessPanel.alpha = targetAlpha;
    }

    // ... (Keep your existing MoveTowardPlayer, FaceTarget, PerformAttack, ApplyKnockback functions here) ...
    // DO NOT DELETE THEM!
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

    IEnumerator PerformAttack()
    {
        isBusy = true;
        lastAttackTime = Time.time;
        anim.SetTrigger("attack");
        yield return new WaitForSeconds(attackWindup);

        float hitRange = attackRange + 3.0f; 
        if (player != null && Vector2.Distance(transform.position, player.position) <= hitRange)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                StartCoroutine(ApplyKnockback());
            }
        }
        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    IEnumerator ApplyKnockback()
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        PlayerController playerMoveScript = player.GetComponent<PlayerController>();

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero; 
            Vector2 direction = (player.position - transform.position).normalized;
            direction.y = 0.5f; 
            if (playerMoveScript != null) playerMoveScript.enabled = false;
            playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
            yield return new WaitForSeconds(stunTime);
            if (playerMoveScript != null) playerMoveScript.enabled = true;
        }
    }
}