using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] float maxHealth;
    public float currentHealth { get; private set; }
    bool isDead = false;
    public bool isInvincible = false;
    Animator animator;

    [SerializeField] float invulnerabilityDuration;
    [SerializeField] int numberOfFlashes;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        if (currentHealth > 0)
        {
            animator.SetTrigger("hurt");
            StartCoroutine(InvulnerabilityFlash());
        }
        else
        {
            if (!isDead)
            {
                animator.SetTrigger("die");
                GetComponent<PlayerController>().enabled = false;
                GetComponent<PlayerAttack>().enabled = false;
                isDead = true;
            }

        }
    }
    public void IncreaseHealth(float amount)
    {
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
