using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	#region Movement_variables
    public float movespeed;
	public float jumpforce;
	float x_input;
	#endregion

	#region Physics_components
	Rigidbody2D playerRB;
	int FloorLayer;
	private int floorContactCount = 0; // Counter for floor contacts
	public bool feetContact;
	// Clamping values
    public float minX = 0f;
    public float maxX = 1000f; 
	#endregion

	#region Health_variables
	public float maxHealth = 5;
	float currHealth = 5;
	public Slider HPSlider;
	#endregion

	#region Attack_variables
	public float damage = 1;
	public float attackSpeed = 1;
	float attackTimer;
	public float hitboxTiming = 0.1f;
	public float endAnimationTiming = 0.1f;
	bool isAttacking;
	Vector2 currDirection;
	#endregion

	#region Animation_components
	private Animator animator;
    private SpriteRenderer sr;
	private Color originalColor;
	#endregion

	public Image deathOverlay;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
		originalColor = sr.color;
		attackTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
		x_input = Input.GetAxisRaw("Horizontal");
		Move();
		if (Input.GetMouseButtonDown(0)) {
			if (attackTimer <= 0) {
				Attack();
			}
		}
		attackTimer -= Time.deltaTime;
    }

	private void Move() {
		Vector2 movement = new Vector2(x_input * movespeed, playerRB.velocity.y);
		playerRB.velocity = movement;
		if (Input.GetKeyDown(KeyCode.Space) && feetContact)
		{
			playerRB.velocity = new Vector2(playerRB.velocity.x, 0);
			playerRB.AddForce(new Vector2(0, jumpforce));
			animator.SetTrigger("Jump");
			feetContact = false;
		}

		bool isMovingHorizontally = Mathf.Abs(x_input) > 0.01f && feetContact;
		if (!isAttacking) {  // Prevent movement/jump animation if attacking
			if (isMovingHorizontally) {
				// Play moving animation only if grounded and moving horizontally
				animator.SetBool("Moving", true);
			} else {
				animator.SetBool("Moving", false);
			}

			// If the player is not moving at all and is grounded, set idle animation
			if (x_input == 0 && feetContact) {
				animator.SetBool("Moving", false);  // This will revert to the idle state in Animator
			}
    	}

		if (x_input > 0) {
			currDirection = Vector2.right;  // Facing right
			sr.flipX = false;  // Ensure sprite faces right
		} else if (x_input < 0) {
			currDirection = Vector2.left;  // Facing left
			sr.flipX = true;  // Ensure sprite faces left
		}
		animator.SetFloat("DirX", currDirection.x);

		// keeps bro inside bounds
		Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;
	}

    bool isFloor(GameObject obj) {
		return obj.layer == LayerMask.NameToLayer("Floor") || obj.transform.CompareTag("Enemy");
	}

    // use coll.gameObject if you need a reference coll's GameObject
	void OnCollisionEnter2D(Collision2D coll) {
		if (isFloor(coll.gameObject)) {
			floorContactCount++;
			feetContact = true; // Set to true on first contact
			animator.SetBool("grounded", true);
    }
	}

	void OnCollisionExit2D(Collision2D coll) {
		if (isFloor(coll.gameObject)) {
			floorContactCount--;
			if (floorContactCount <= 0) {
				feetContact = false; // Only set to false if no more contacts
				animator.SetBool("grounded", false);
        }
    }
    }


	private void Attack() {
		attackTimer = attackSpeed;
		StartCoroutine(AttackRoutine());
	}

	IEnumerator AttackRoutine() {
		isAttacking = true;
		animator.SetTrigger("attack");
		yield return new WaitForSeconds(hitboxTiming);
		// Define the size of the hitbox (make it bigger)
		Vector2 hitboxSize = new Vector2(1.5f, 1.5f);  // Adjust this to change the hitbox size
		// Define the offset to shift the hitbox downwards (or in other directions)
		Vector2 hitboxOffset = new Vector2(0, -1f);  // Shift hitbox down by 0.5 units
		// Calculate the hitbox center, offset by currDirection and hitboxOffset
		Vector2 hitboxCenter = playerRB.position + currDirection + hitboxOffset;

		RaycastHit2D[] hits = Physics2D.BoxCastAll(hitboxCenter, hitboxSize, 0f, Vector2.zero);
    	foreach (RaycastHit2D hit in hits) {
			if (hit.transform.CompareTag("Enemy")) {
				hit.transform.GetComponent<EnemyController>().TakeDamage(damage);
				FindObjectOfType<AudioManager>().Play("EnemyHurt");
			}
   		}
		yield return new WaitForSeconds(endAnimationTiming);
		isAttacking = false;
	}

	void OnDrawGizmosSelected()
{
    // Set the color for the Gizmo
    Gizmos.color = Color.red;
	if (playerRB != null)
    {
        // Define hitbox size and offset (same as in AttackRoutine)
        Vector2 hitboxSize = new Vector2(1.5f, 1.5f);  // Same as in AttackRoutine
        Vector2 hitboxOffset = new Vector2(0, -1f);  // Same as in AttackRoutine

        // Calculate the center of the hitbox
        Vector2 hitboxCenter = playerRB.position + currDirection + hitboxOffset;

        // Draw the hitbox for visualization
        Gizmos.DrawWireCube(hitboxCenter, hitboxSize);
    }
}

	public void TakeDamage(float value) {
		currHealth -= value;
		if (currHealth <= 0) {
			Die();
		}
		HPSlider.value = currHealth / maxHealth;
		StartCoroutine(FlashRed());
	}

	private IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
        // More flashes
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }

	public void Heal(float value) {
		currHealth += value;
		currHealth = Mathf.Min(currHealth, maxHealth);
		HPSlider.value = currHealth / maxHealth;
	}

	public void Die() {
		GameManager.Instance.isGamePaused = true; // Freeze all scripts
		deathOverlay.gameObject.SetActive(true);
		Destroy(this.gameObject);
	}

	public void GetBuff() {
		damage += 1;
		attackSpeed -= 0.5f;
		movespeed += 3;
	}

}
