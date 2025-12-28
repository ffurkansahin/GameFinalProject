using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Can Ayarlar�")]
    [SerializeField] float maxHealth;
    public float currentHealth { get; private set; }

    [Header("�l�m Ayarlar�")]
    public GameObject deathEffect;
    public float restartDelay = 1.5f;

    bool isDead = false;
    public bool isInvincible = false;
    Animator animator;

    [SerializeField] float invulnerabilityDuration;
    [SerializeField] int numberOfFlashes;
    private SpriteRenderer spriteRenderer;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip dieSound;
    public AudioClip collectHealthSound;
    private UIManager uiManager;
    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        uiManager = FindObjectOfType<UIManager>();
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth > 0)
        {
            audioSource.PlayOneShot(hurtSound, 0.1f);
            animator.SetTrigger("hurt");
            StartCoroutine(InvulnerabilityFlash());
        }
        else
        {
            if (!isDead)
            {
                Die();
            }
        }
    }

    void Die()
    {
        isDead = true;
        audioSource.PlayOneShot(dieSound, 0.1f);
        animator.SetTrigger("die");

        if (GetComponent<PlayerController>() != null)
            GetComponent<PlayerController>().enabled = false;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        uiManager.ShowGameOver();
    }

    public void IncreaseHealth(float amount)
    {
        audioSource.PlayOneShot(collectHealthSound, 0.1f);
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    public IEnumerator InvulnerabilityFlash()
    {
        Physics2D.IgnoreLayerCollision(3, 4, true);

        float flashDuration = invulnerabilityDuration / (numberOfFlashes * 2);
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRenderer.color = new Color(1, 0, 0, 0.8f);
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        Physics2D.IgnoreLayerCollision(3, 4, false);
    }

}