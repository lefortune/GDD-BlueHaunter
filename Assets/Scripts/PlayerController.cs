using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	#region Movement_variables
    public float movespeed = 3;
	public float jumpforce = 800;
	float x_input;
	private bool canMove = true;
	#endregion

	#region Physics_components
	Rigidbody2D playerRB;
	int FloorLayer;
	private int floorContactCount = 0; // Counter for floor contacts
	public bool feetContact;
	// Clamping values
    public float minX;
    public float maxX; 
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
	public float hitboxTiming = 0.55f;
	public float endAnimationTiming = 0.4f;
	bool isAttacking;
	Vector2 currDirection;
	#endregion

	#region Gameplay_components
	public int damageSoulsCollected = 0;
    public int speedSoulsCollected = 0;
    public int atkspeedSoulsCollected = 0;
	#endregion

	#region Animation_components
	private Animator animator;
    private SpriteRenderer sr;
	private Color originalColor;
	#endregion

	public GameObject enemyKillText;
	public GameManager gameManager;

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
		if (!canMove) return;

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
//		if (Input.GetKeyDown(KeyCode.Space) && feetContact)
//		{
//			playerRB.velocity = new Vector2(playerRB.velocity.x, 0);
//			playerRB.AddForce(new Vector2(0, jumpforce));
//			animator.SetTrigger("Jump");
//			feetContact = false;
//		}

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
		Vector2 hitboxSize = new Vector2(2.5f, 2f);  // Adjust this to change the hitbox size
		// Define the offset to shift the hitbox downwards (or in other directions)
		Vector2 hitboxOffset = new Vector2(0, -1f); 
		// Calculate the hitbox center, offset by currDirection and hitboxOffset
		Vector2 hitboxCenter = playerRB.position + currDirection + hitboxOffset;
		FindObjectOfType<AudioManager>().Play("PlayerAttack");

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
        Vector2 hitboxSize = new Vector2(2.5f, 2f);  // Same as in AttackRoutine
        Vector2 hitboxOffset = new Vector2(0, -1f);  // Same as in AttackRoutine

        // Calculate the center of the hitbox
        Vector2 hitboxCenter = playerRB.position + currDirection + hitboxOffset;

        // Draw the hitbox for visualization
        Gizmos.DrawWireCube(hitboxCenter, hitboxSize);
    }
}

	public void TakeDamage(float value) {
		Debug.Log("Player took " + value + " dmg");
		currHealth -= value;
		FindObjectOfType<CameraFollow>().CameraShake(0.1f, 0.1f);
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
		FindObjectOfType<AudioManager>().Play("PlayerDie");
		gameManager.FreezeGame(); // Freeze all scripts
		Destroy(this.gameObject);
	}

	public void UpdateStats(int seed) {
		if (seed == 0) damageSoulsCollected += 1;
		if (seed == 1) speedSoulsCollected += 1;
		if (seed == 2) atkspeedSoulsCollected += 1; // Experimental, not implemented

		damage = 1 + (damageSoulsCollected * 0.25f);

		movespeed = 3 + (speedSoulsCollected * 0.5f);
		jumpforce = 800 + (speedSoulsCollected * 10);

		attackSpeed = Math.Max(1 - (atkspeedSoulsCollected * 0.08f), 0);
		hitboxTiming = Math.Max(0.55f - (atkspeedSoulsCollected * 0.03f), 0);
		endAnimationTiming = Math.Max(0.4f - (atkspeedSoulsCollected * 0.03f), 0);
		animator.speed = 1 + (atkspeedSoulsCollected * 0.25f);

		GameObject text = Instantiate(enemyKillText, transform.position + Vector3.up * UnityEngine.Random.Range(0.5f, 2.5f), Quaternion.identity);
        text.GetComponent<SoulTextScript>().SetTextProperties(seed);

		StartCoroutine(AbsorbSoul());
	}

	private IEnumerator AbsorbSoul()
    {
        canMove = false;
		sr.flipX = !sr.flipX;
		float temp = new float();
		temp = animator.speed;
		animator.speed = 1;
		animator.SetTrigger("AbsorbSoul");
        yield return new WaitForSeconds(2f);
		sr.flipX = !sr.flipX;
        canMove = true;
		animator.speed = temp;
    }

}
