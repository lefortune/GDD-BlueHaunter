using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed;
    public Transform player;
    public float maxHealth;
    float currHealth;
    public float enemyDamage;
    public float enemyAttackTimer;
    Rigidbody2D EnemyRB;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    // Start is called before the first frame update
    void Start()
    {
        EnemyRB = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) {
            return;
        }
        Move();
    }

    private void Move() {
        Vector2 direction = player.position - transform.position;
        EnemyRB.velocity = direction.normalized * moveSpeed;
    }

    private void AttackPlayer() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 1f, Vector2.zero);
        foreach (RaycastHit2D hit in hits) {
            if (hit.transform.CompareTag("Player")) {
                hit.transform.GetComponent<PlayerController>().TakeDamage(enemyDamage);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.transform.CompareTag("Player")) {
            AttackPlayer();
        }
    }

    public void TakeDamage(float value) {
        currHealth -= value;
        if (currHealth <= 0) {
            FindObjectOfType<AudioManager>().Play("EnemyDie");
            Destroy(this.gameObject);
        }
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        // Change color to red
        spriteRenderer.color = Color.red;

        // Wait for a short duration
        yield return new WaitForSeconds(0.1f);

        // Change back to the original color
        spriteRenderer.color = originalColor;

        // Optionally, you can add more flashes
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}
