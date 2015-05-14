/*
Author: Trevor Richardson
PlayerControllerScript.cs
04-17-2015

	Script for controlling the boss. Reuses many enemy archer functions.
	
 */

using UnityEngine;
using System.Collections;

public class BossScript : MonoBehaviour {
	
	// movement
	public bool right = true;
	float hSpeed = 0;
	float jumpForce = 200.0f;

	// applies a delay on boss hit collision
	float hitDelay = 0.5f;
	float hitReady = 0.5f;
	
	// arrow projectile
	public GameObject arrowPrefab;
	float arrowSpeed = 10.0f;
	
	// detect ground contact
	bool onGround = false;
	public Transform groundCircle;
	float groundRadius = 0.05f;
	public LayerMask GroundMask;
	
	Animator anim;

	// Variables / triggers
	public int health = 10;
	bool KOed = false;
	private bool inAction = false;
	private bool phase2 = false;
	private bool phase3 = false;
	public GameObject rightEdge;
	public GameObject leftEdge;
	bool rightEdgeTriggeredLast = false;
	public GameObject fallingArrows;
	public GameObject player;
	
	public AudioSource damagedAudio;
	public AudioSource throwAudio;
	public AudioSource KOAudio;
	public AudioSource jumpAudio;
	
	
	// Ignore collision with other enemies, get animator
	void Start () {
		anim = GetComponent<Animator>();
		Flip ();
	}
	
	// Movement & patrol controls
	void FixedUpdate () {
		
		// detect ground contact for anims
		onGround = Physics2D.OverlapCircle(groundCircle.position, groundRadius, GroundMask);
		anim.SetBool("Grounded", onGround);
		// Update vertical speed for fall anim
		anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);
		
		// enemy fell off world
		if (transform.position.y < -50) {
			StartCoroutine(KO());
			Destroy (gameObject, 5.0f);
		}
		
		// update horizontal speed in animator and move enemy
		anim.SetFloat("hSpeed", Mathf.Abs(hSpeed));
		rigidbody2D.velocity = new Vector2(hSpeed, rigidbody2D.velocity.y);
		
		// Same as player control to determine if a flip is needed
		if (hSpeed < 0 && right)
			Flip ();
		else if ( hSpeed > 0 && !right)
			Flip ();
		
	}
	
	IEnumerator Shoot() {
		anim.SetTrigger("ThrowTrigger");
		yield return new WaitForSeconds(.5f);
		if (!KOed) {
			// spawn arrow and send it in the facing direction
			GameObject enemyArrow = (GameObject)Instantiate(arrowPrefab, transform.position, Quaternion.identity);
			if (right)
				enemyArrow.rigidbody2D.velocity = new Vector2(arrowSpeed, 0);
			else {
				enemyArrow.transform.localScale = new Vector3(-enemyArrow.transform.localScale.x, enemyArrow.transform.localScale.y, enemyArrow.transform.localScale.z);
				enemyArrow.rigidbody2D.velocity = new Vector2(-arrowSpeed, 0);
			}
			// play shoot fx and destroy arrow after 3s
			throwAudio.Play();
			Destroy (enemyArrow, 3.0f);
		}
	}
	
	// Remove enemy collision boxes on death
	void Update() {
		if (!inAction && !KOed)
			StartCoroutine(StationaryBehavior());
		hitReady += Time.deltaTime;
	}
	
	IEnumerator StationaryBehavior() {
		inAction = true;
		StartCoroutine(Shoot());
		yield return new WaitForSeconds(1.5f);
		inAction = false;
	}
	
	// Flip sprite horizontally
	void Flip() {
		right = !right;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}
	
	// Trigger detection and control for each phase
	void OnTriggerEnter2D (Collider2D col)
	{
		if (col.gameObject.tag == "Arrow") {
			if (hitReady > hitDelay) {
				hitReady = 0f;
				Hit ();
		}
		}
		if (phase2) {
		if (col.gameObject.tag == "LeftEdge") {
			hSpeed = 3f;
			rigidbody2D.AddForce(new Vector2(0, jumpForce));
			jumpAudio.Play();
		}
			if (col.gameObject.tag == "RightEdge") {
			hSpeed = -3f;
			rigidbody2D.AddForce(new Vector2(0, jumpForce));
			jumpAudio.Play();
		}
		}
		else if (phase3) {
			if (col.gameObject.tag == "LeftEdge") {
				hSpeed = 5f;
				rigidbody2D.AddForce(new Vector2(0, jumpForce/1.25f));
				jumpAudio.Play();
			}
			if (col.gameObject.tag == "RightEdge") {
				hSpeed = -5f;
				rigidbody2D.AddForce(new Vector2(0, jumpForce/1.25f));
				jumpAudio.Play();
			}
		}
	}
	
	// Send KO signal to animator and initate death sequence
	IEnumerator KO() {
		gameObject.renderer.material.color = Color.black;
		anim.SetBool("GameOver", true);
		KOed = true;
		yield return new WaitForSeconds(2.0f);
		fallingArrows.SetActive(true);
		yield return new WaitForSeconds(1.5f);
		if (player.GetComponent<ArcherControlScript3>().health > 0)
			player.GetComponent<ArcherControlScript3>().gameWon = true;
		GetComponent<CircleCollider2D>().enabled = false;
		GetComponent<BoxCollider2D>().enabled = false;
		Destroy (gameObject, 2.0f);
	}

	IEnumerator Phase2() {
		yield return null;
	}

	// Boss health control for phase changes
	void Hit() {
		--health;
		damagedAudio.Play();
		anim.SetTrigger("HitTrigger");
		//anim.SetTrigger("HitTrigger");
		switch (health) {
		case 7:
		case 6:
		case 5:
			phase2 = true;
			rightEdge.SetActive(true);
			leftEdge.SetActive(true);
			break;
		case 4:
		case 3:
		case 2:
		case 1:
			phase2 = false;
			phase3 = true;
			gameObject.renderer.material.color = Color.red;
			break;
		case 0:
			phase3 = false;
			hSpeed = 0;
			StartCoroutine(KO());
			break;
		}
	}
}
