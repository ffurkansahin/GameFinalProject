using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Settings")]
    public float comboResetTime = 1.0f; // Time before combo resets to 0
    private int comboStep = 0;
    private float lastAttackTime;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange;
    public int damage;
    public CameraShake camShake;
    public GameObject bloodEffect;
      
    
    // References
    private Animator animator;
    private PlayerController playerController;

    void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // 1. Reset Combo if you stop attacking for too long
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboStep = 0;
        }

        // 2. Input Check
        if(Input.GetMouseButtonDown(0) && playerController.canAttack())
        {
            PerformComboAttack();
        }
    }

    private void PerformComboAttack()
    {
        // 1. Calculate Combo Step
        comboStep++;
        if (comboStep > 2) 
        {
            comboStep = 1; 
        }

        // 2. FORCE THE ANIMATION (The Fix)
        // Instead of SetTrigger, we tell the Animator: "Play this state NOW, from the start."
        // Make sure your Animator States are named exactly "Attack1" and "Attack2"
        animator.Play("Attack" + comboStep, 0, 0f);

        // 3. Reset Timer & Deal Damage
        lastAttackTime = Time.time;
        StartCoroutine(DealDamage());
    }

    private IEnumerator DealDamage()
    {
        
        // Wait 0.1s so damage happens when sword hits (adjust this if needed)
        yield return new WaitForSeconds(0.1f);

        // Detect enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, LayerMask.GetMask("Enemy"));
        if (hitEnemies.Length > 0)
        {
            // 0.05f is subtle. 0.1f is heavy. Try both!
            if (GameManager.instance != null) GameManager.instance.HitStop(0.1f);
        }
        
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (bloodEffect != null)
            {
                // Calculate direction: Player -> Enemy
                Vector2 direction = (enemyCollider.transform.position - transform.position).normalized;
                
                // Calculate rotation angle to face that direction
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                
                // Spawn the blood at the enemy's position, facing away from player
               Vector3 spawnPos = enemyCollider.transform.position + (Vector3)(direction * 0.5f);
                Instantiate(bloodEffect, spawnPos, rotation);
            }
            if (hitEnemies.Length > 0)
    {
            // 0.1s duration, 0.1f magnitude (Very short, snappy shake)
             if (CinemachineShake.instance != null) 
        {
            CinemachineShake.instance.ShakeCamera(0.5f, 0.15f); 
        }
    }
            // 1. Try finding Normal Enemy Health
            EnemyHealth normalEnemy = enemyCollider.GetComponent<EnemyHealth>();
            if (normalEnemy != null)
            {
                normalEnemy.TakeDamage(damage);
                continue; 
            }

            // 2. Try finding BOSS Health
            BossHealth boss = enemyCollider.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                continue;
            }

            // 3. Old fallback (MeleeEnemy)
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