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
    bool isAttacking;
    Rigidbody2D EnemyRB;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Color originalColor;
    // Start is called before the first frame update
    void Start()
    {
        EnemyRB = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalColor = spriteRenderer.color;
        currHealth = maxHealth;

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isGamePaused) return;
        if (player == null) {
            return;
        }
        Move();
        Debug.Log(this.gameObject + "says hi");
    }

    private void Move() {
        Vector2 direction = player.position - transform.position;
        direction.y = 0; // Ignore the Y component to prevent flying up or down
        EnemyRB.velocity = new Vector2(direction.normalized.x * moveSpeed, EnemyRB.velocity.y);
    }

    IEnumerator AttackRoutine() {
        isAttacking = true;

        while (isAttacking) {
            AttackPlayer();
            yield return new WaitForSeconds(enemyAttackTimer);
        }
    }

    private void AttackPlayer() {
		Vector2 center = (Vector2)transform.position + (Vector2)boxCollider.offset;
        Vector2 size = boxCollider.size; // This gives the size of the collider

        RaycastHit2D[] hits = Physics2D.BoxCastAll(center, size, 0f, Vector2.zero);

    	foreach (RaycastHit2D hit in hits) {
			if (hit.transform.CompareTag("Player")) {
                FindObjectOfType<AudioManager>().Play("EnemyAttack");
				hit.transform.GetComponent<PlayerController>().TakeDamage(enemyDamage);
                
			}
   		}
	}

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.transform.CompareTag("Player"))
        {
            if (!isAttacking) // Check if not currently attacking
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    private void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.transform.CompareTag("Player"))
        {
            isAttacking = false; // Stop attacking when player exits
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
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        // More flashes
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}
