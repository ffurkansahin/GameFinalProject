using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [SerializeField] float attackCooldown;
    [SerializeField] float range;
    [SerializeField] float colliderDistance;
    [SerializeField] float damage;
    [SerializeField] CapsuleCollider2D capsuleCollider;
    private float cooldownTimer = Mathf.Infinity;
    [SerializeField] LayerMask playerLayer;

    private Animator animator;
    private EnemyPatrol enemyPatrol;

    void Awake()
    {
        animator = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            if (cooldownTimer > attackCooldown)
            {
                Attack();
            }
        }
        if(enemyPatrol != null)
        {
            enemyPatrol.enabled = !PlayerInSight();
        }

    }

    public bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(capsuleCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
        new Vector3(capsuleCollider.bounds.size.x * range, capsuleCollider.bounds.size.y, capsuleCollider.bounds.size.z), 0, Vector2.left, 0, playerLayer);
        return hit.collider != null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(capsuleCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,  new Vector3(capsuleCollider.bounds.size.x * range, capsuleCollider.bounds.size.y, capsuleCollider.bounds.size.z));
    }

    private void Attack()
    {
        animator.SetTrigger("meleeAttack");
        cooldownTimer = 0;
    }

    private void DamagePlayer()
    {
        if (PlayerInSight())
        {
            Health playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
            playerHealth.TakeDamage(damage);
        }
    }
}