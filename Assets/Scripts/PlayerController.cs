using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

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
	#endregion


    // Start is called before the first frame update
    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
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

		if (movement == Vector2.zero) {
			animator.SetBool("Moving", false);
		} else {
			animator.SetBool("Moving", true);
		}
		animator.SetFloat("DirX", currDirection.x);

		// keeps bro inside bounds
		Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;
	}

    bool isFloor(GameObject obj) {
		return obj.layer == LayerMask.NameToLayer ("Floor");
	}

    // use coll.gameObject if you need a reference coll's GameObject
	void OnCollisionEnter2D(Collision2D coll) {
		if (isFloor(coll.gameObject)) {
			floorContactCount++;
			feetContact = true; // Set to true on first contact
    }
	}

	void OnCollisionExit2D(Collision2D coll) {
		if (isFloor(coll.gameObject)) {
			floorContactCount--;
			if (floorContactCount <= 0) {
				feetContact = false; // Only set to false if no more contacts
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
		//FindObjectOfType<AudioManager>().Play("");
		RaycastHit2D[] hits = Physics2D.BoxCastAll(playerRB.position + currDirection, new Vector2(0.5f, 0.5f), 0f, Vector2.zero);
		foreach (RaycastHit2D hit in hits) {
			if (hit.transform.CompareTag("Enemy")) {
				hit.transform.GetComponent<EnemyController>().TakeDamage(damage);
			}
		}
		yield return new WaitForSeconds(hitboxTiming);
		isAttacking = false;
	}

	public void TakeDamage(float value) {
		currHealth -= value;
		if (currHealth <= 0) {
			Die();
		}
	}
	public void Heal(float value) {
		currHealth += value;
		currHealth = Mathf.Min(currHealth, maxHealth);
	}

	public void Die() {
		Destroy(this.gameObject);
		//GameObject gm = GameObject.FindWithTag("GameController");
		//gm.GetComponent<GameManager>().LoseGame();
	}

}
