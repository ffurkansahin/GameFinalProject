using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Setup")]
    public float moveSpeed = 3f;   
    public float attackRange = 6f; 
    public float wakeUpRange = 15f;

    [Header("Cinematics")]
    public float spawnAnimTime = 3.0f;
    public CameraShake cameraShake;
    public CanvasGroup darknessPanel;
    public MonoBehaviour cameraFollowScript;
    public float cameraSlideSpeed = 2.0f;

    [Header("Combat")]
    public float attackWindup = 0.5f;
    public float attackCooldown = 2.0f;
    public int damage = 25;

    [Header("Knockback")]
    public float knockbackForce = 15f;
    public float stunTime = 0.5f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip spawnRoarSound;
    public AudioClip attackSound;

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

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

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
        if (healthScript.isDead || player == null)
        {
            if (rb.bodyType != RigidbodyType2D.Static) rb.velocity = Vector2.zero;
            return;
        }

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
            if (rb.bodyType != RigidbodyType2D.Static)
            {
                rb.velocity = Vector2.zero;
            }
            return;
        }

        FaceTarget();

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            if (rb.bodyType != RigidbodyType2D.Static) rb.velocity = Vector2.zero;

            anim.SetBool("run", false);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                StartCoroutine(PerformAttack());
            }
        }
        else
        {
            anim.SetBool("run", true);
            MoveTowardPlayer();
        }
    }

    void MoveTowardPlayer()
    {
        if (rb.bodyType == RigidbodyType2D.Static) return;

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

    IEnumerator PlaySpawnSequence()
    {
        isActivated = true;
        isSpawning = true;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
        if (player.GetComponent<Rigidbody2D>() != null) player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        Transform camParams = Camera.main.transform;
        Vector3 originalCamPos = camParams.position;
        Vector3 bossCamPos = new Vector3(transform.position.x, transform.position.y, originalCamPos.z);

        if (cameraFollowScript != null) cameraFollowScript.enabled = false;
        yield return StartCoroutine(SlideCamera(camParams, bossCamPos, 1.0f));

        if (player != null) FaceTarget();

        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        anim.SetTrigger("spawn");
        if (audioSource != null && spawnRoarSound != null) audioSource.PlayOneShot(spawnRoarSound);

        if (cameraShake != null) StartCoroutine(cameraShake.Shake(spawnAnimTime, 0.2f));
        StartCoroutine(FadeDarkness(0.6f, 1.0f));

        yield return new WaitForSeconds(spawnAnimTime);

        StartCoroutine(FadeDarkness(0f, 1.0f));

        rb.bodyType = RigidbodyType2D.Dynamic;

        yield return StartCoroutine(SlideCamera(camParams, originalCamPos, 0.5f));

        if (cameraFollowScript != null) cameraFollowScript.enabled = true;
        if (pc != null) pc.enabled = true;

        isSpawning = false; 
    }

    IEnumerator PerformAttack()
    {
        isBusy = true;
        lastAttackTime = Time.time;
        anim.SetTrigger("attack");
        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

        yield return new WaitForSeconds(attackWindup);

        float hitRange = attackRange + 4.0f;

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

    IEnumerator SlideCamera(Transform cam, Vector3 targetPos, float duration)
    {
        Vector3 startPos = cam.position;
        float time = 0;
        while (time < duration) { cam.position = Vector3.Lerp(startPos, targetPos, time / duration); time += Time.deltaTime; yield return null; }
        cam.position = targetPos;
    }
    IEnumerator FadeDarkness(float targetAlpha, float duration)
    {
        if (darknessPanel == null) yield break;
        float startAlpha = darknessPanel.alpha;
        float time = 0;
        while (time < duration) { darknessPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration); time += Time.deltaTime; yield return null; }
        darknessPanel.alpha = targetAlpha;
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