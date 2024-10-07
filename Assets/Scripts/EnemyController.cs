using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed;
    public Transform player;
    Vector2 currDirection;

    #region Attack variables
    public float maxHealth;
    float currHealth;
    public float enemyDamage;
    public float enemyAttackSpeed;
    public float hitboxTiming = 0.2f;
    bool isAtPlayer;
    bool isAttacking;
    bool isDying = false;
    private Coroutine attackCoroutine;
    #endregion

    #region Animation variables
    Rigidbody2D EnemyRB;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Color originalColor;
    #endregion

    #region Gameplay variables
    public int seed;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        EnemyRB = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalColor = spriteRenderer.color;
        currHealth = maxHealth;

        seed = Random.Range(0, 2);
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isGamePaused) return;
        if (player == null) {
            return;
        }
        if (!isAttacking && !isDying) {
            Move();
        }
        if (isAtPlayer && attackCoroutine == null && !isDying && !isAttacking) {
            attackCoroutine = StartCoroutine(AttackRoutine());
        }
    }

    private void Move() {
        Vector2 direction = player.position - transform.position;
        direction.y = 0; // Ignore the Y component to prevent flying up or down
        EnemyRB.velocity = new Vector2(direction.normalized.x * moveSpeed, EnemyRB.velocity.y);
        if (EnemyRB.velocity.x < 0) {
            currDirection = Vector2.left;
            spriteRenderer.flipX = true;
        } else if (EnemyRB.velocity.x > 0) {
            currDirection = Vector2.right;
            spriteRenderer.flipX = false;
        }
    }

    IEnumerator AttackRoutine() {
        while (isAtPlayer && !isDying) {
            Debug.Log("Enemy winds up");
            isAttacking = true;
            EnemyRB.velocity = Vector2.zero;
            animator.SetTrigger("attack");
            yield return new WaitForSeconds(hitboxTiming);

            Vector2 hitboxSize = new Vector2(1.6f, 2.2f);  // Adjust this to change the hitbox size
            // Define the offset to shift the hitbox downwards (or in other directions)
            Vector2 hitboxOffset = new Vector2(-0.2f, 0f); 
            // Calculate the hitbox center, offset by currDirection and hitboxOffset
            Vector2 hitboxCenter = EnemyRB.position + currDirection / 2 + hitboxOffset;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(hitboxCenter, hitboxSize, 0f, Vector2.zero, 0f); // Direction is zero for the box

            foreach (RaycastHit2D hit in hits) {
                if (hit.transform.CompareTag("Player")) {
                    FindObjectOfType<AudioManager>().Play("EnemyAttack");
                    hit.transform.GetComponent<PlayerController>().TakeDamage(enemyDamage);
                    Debug.Log($"Hit: {hit}");
                } else Debug.Log("Miss");
            }
            yield return new WaitForSeconds(enemyAttackSpeed);
            isAttacking = false;
            attackCoroutine = null;
        }
        
    }

    void OnDrawGizmosSelected()
    {
        // Set the color for the Gizmo
        Gizmos.color = Color.red;
        if (player != null)
        {
            Vector2 size = new Vector2(1.6f, 2f);
            Vector2 offset = new Vector2(-0.2f, 0f); 
            Vector2 center = EnemyRB.position + currDirection / 2 + offset;

            // Draw the hitbox for visualization
            Gizmos.DrawWireCube(center, size);
        }
    }

    private void AttackPlayer() {
        
	}

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.transform.CompareTag("Player"))
        {
            isAtPlayer = true;
        }
    }

    private void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.transform.CompareTag("Player"))
        {
            isAtPlayer = false;
        }
    }

    public void TakeDamage(float value) {
        currHealth -= value;
        if (currHealth <= 0) {
            Die();
        }
        StartCoroutine(FlashRed());
    }

    public void Die()
    {
        isDying = true;
        animator.SetTrigger("die");
        FindObjectOfType<AudioManager>().Play("EnemyDie");
        FindObjectOfType<PlayerController>().UpdateStats(seed);
        Debug.Log("Seed:" + seed);
        Destroy(this.gameObject, 1f);
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
