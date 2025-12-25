using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] float attackCooldown;
    private Animator animator;
    private PlayerController playerController;
    private float cooldownTimer = Mathf.Infinity;

    public Transform attackPoint;
    public float attackRange;
    public int damage;

    void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && cooldownTimer > attackCooldown && playerController.canAttack())
        {
            Attack();
        }

        cooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        animator.SetTrigger("attack");

        // Detect enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, LayerMask.GetMask("Enemy"));
        
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // 1. Try finding Normal Enemy Health
            EnemyHealth normalEnemy = enemyCollider.GetComponent<EnemyHealth>();
            if (normalEnemy != null)
            {
                normalEnemy.TakeDamage(damage);
                continue; // Found one, move to next enemy
            }

            // 2. Try finding BOSS Health (NEW!)
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
        
        cooldownTimer = 0;
    }
}
    