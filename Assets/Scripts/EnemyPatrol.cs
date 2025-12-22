using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] Transform leftEdge;
    [SerializeField] Transform rightEdge;
    [SerializeField] Transform enemy;
    [SerializeField] float moveSpeed;
    [SerializeField] float stayinIdleTime;
    float idleTimer;
    private bool movingLeft;
    private Vector3 initScale;
    [SerializeField]Animator animator;

    void Awake()
    {
        initScale = enemy.localScale;
    }
    void Update()
    {
        if (movingLeft)
        {
            if (enemy.position.x >= leftEdge.position.x)
            {
                MoveInDirection(-1);
            }
            else
            {
                DirectionChange();
            }
        }
        else
        {
            if (enemy.position.x <= rightEdge.position.x)
            {
                MoveInDirection(1);
            }
            else
            {
                DirectionChange();
            }
        }

    }
    void OnDisable()
    {
        animator.SetBool("moving", false);
    }
    private void MoveInDirection(float direction)
    {
        idleTimer = 0;
        animator.SetBool("moving", true);

        enemy.localScale = new Vector3(Mathf.Abs(initScale.x) * direction, initScale.y, initScale.z);

        enemy.position = new Vector3(enemy.position.x + Time.deltaTime * direction * moveSpeed, enemy.position.y, enemy.position.z);
    }
    private void DirectionChange()
    {
        animator.SetBool("moving", false);

        idleTimer += Time.deltaTime;

        if (idleTimer > stayinIdleTime)
            movingLeft = !movingLeft;
    }
}
