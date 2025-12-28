using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Settings")]
    public float comboResetTime = 1.0f;
    private int comboStep = 0;
    private float lastAttackTime;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange;
    public int damage;
    public CameraShake camShake;
    public GameObject bloodEffect;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip attackSound;

    private Animator animator;
    private PlayerController playerController;

    void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (UIManager.GameIsPaused) return;
       
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        if (Input.GetMouseButtonDown(0) && playerController.canAttack())
        {
            PerformComboAttack();
        }
    }

    private void PerformComboAttack()
    {
        comboStep++;
        if (comboStep > 2)
        {
            comboStep = 1;
        }

        audioSource.PlayOneShot(attackSound, 0.1f);
        animator.Play("Attack" + comboStep, 0, 0f);

        lastAttackTime = Time.time;
        StartCoroutine(DealDamage());
    }

    private IEnumerator DealDamage()
    {

        yield return new WaitForSeconds(0.1f);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, LayerMask.GetMask("Enemy"));
        if (hitEnemies.Length > 0)
        {
            if (GameManager.instance != null) GameManager.instance.HitStop(0.1f);
        }

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (bloodEffect != null)
            {
                Vector2 direction = (enemyCollider.transform.position - transform.position).normalized;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0, 0, angle);

                Vector3 spawnPos = enemyCollider.transform.position + (Vector3)(direction * 0.5f);
                Instantiate(bloodEffect, spawnPos, rotation);
            }
            if (hitEnemies.Length > 0)
            {
                if (CinemachineShake.instance != null)
                {
                    CinemachineShake.instance.ShakeCamera(0.5f, 0.15f);
                }
            }

            EnemyHealth normalEnemy = enemyCollider.GetComponent<EnemyHealth>();
            if (normalEnemy != null)
            {
                normalEnemy.TakeDamage(damage);
                continue;
            }

            BossHealth boss = enemyCollider.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                continue;
            }

            MeleeEnemy oldEnemy = enemyCollider.GetComponent<MeleeEnemy>();
            if (oldEnemy != null)
            {
                oldEnemy.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }


}