using UnityEngine;

public class LavaController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Oyuncunun Fizik Bileþenini (Rigidbody) al
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // HIZI SIFIRLA: Karakter olduðu yerde donsun
                rb.velocity = Vector2.zero;

                // YERÇEKÝMÝNÝ KAPAT: Aþaðý düþmesin, lavýn içinde asýlý kalsýn
                rb.gravityScale = 0;

                // Fiziksel etkileþimleri durdur (Ýtme/kakma olmasýn)
                rb.isKinematic = true;
            }

            // 2. Oyuncuyu Kýrmýzý Yap (Görsel Efekt)
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.red; // Kýpkýrmýzý olsun
            }

            // 3. Can Scriptine Ulaþ ve Öldür
            Health healthScript = collision.GetComponent<Health>();
            if (healthScript != null)
            {
                // Hasar ver (Health scripti sahneyi yenileme iþini yapacak)
                healthScript.TakeDamage(999);
            }
        }
    }
}