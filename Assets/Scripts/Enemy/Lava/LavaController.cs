using UnityEngine;

public class LavaController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.velocity = Vector2.zero;

                rb.gravityScale = 0;

                rb.isKinematic = true;
            }

            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red; 
            }

            Health healthScript = collision.GetComponent<Health>();
            if (healthScript != null)
            {
                healthScript.TakeDamage(999);
            }
        }
    }
}