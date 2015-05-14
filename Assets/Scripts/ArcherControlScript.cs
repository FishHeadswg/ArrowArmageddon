/*
Author: Trevor Richardson
ArcherControlScript.cs
04-12-2015

	Script for controlling the player archer as well as the game environment for Level 1.
	
 */
using UnityEngine;
using System.Collections;

public class ArcherControlScript : MonoBehaviour {
	
	// movement
	float maxSpeed = 4.0f;
	public bool right = true;
	float jumpForce = 300.0f;
	
	// arrow projectile
	public GameObject arrowPrefab;
	float arrowSpeed = 10.0f;
	float shootDelay = 1.0f;
	float shootReady = 1.0f;
	
	// applies a delay on hit detection
	float hitDelay = 1.0f;
	float hitReady = 1.0f;
	
	// Applies a pushback / knockback effect on the player
	Vector2 pushBack = new Vector2(-25.0f, 0.0f);
	
	Animator anim;
	
	// Physics2D overlap circle for detecting ground contact
	bool onGround = false;
	public Transform groundCircle;
	float groundRadius = 0.05f;
	public LayerMask GroundMask;
	
	// GUI elements
	int health = 1;
	bool gameOver = false;
	bool gameOverStarted = false;
	bool gameWon = false;
	public GameObject heart1;
	public GameObject winText;
	public GameObject replayText;
	bool winDisplayed = false;
	
	// Audio
	public AudioSource jumpAudio;
	public AudioSource damagedAudio;
	public AudioSource throwAudio;
	public AudioSource KOAudio;

	// Trigger-related objects
	public GameObject movingPlat;
	public GameObject fallingArrows;
	public Transform faPosition;
	public GameObject fallingArrowPatt;
	public Transform pattPosition;
	public GameObject leftArrow;
	public Transform leftArowPos;
	public GameObject upwardArrow;
	public Transform upwardArowPos;

	bool GodMode = false;
	
	// Retrieve player animator
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	void FixedUpdate () {
		
		// player fell off world
		if (transform.position.y < -2)
			gameOver = true;
		
		// detect contact with ground
		onGround = Physics2D.OverlapCircle(groundCircle.position, groundRadius, GroundMask);
		anim.SetBool("Grounded", onGround);
		
		// zero-out movement and disable control if game over / won
		if (gameOver || gameWon) {
			anim.SetFloat ("vSpeed", 0f);
			anim.SetFloat ("hSpeed", 0f);
			return;
		}
		// update jump/fall velocity in animator
		anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);
		
		// horizontal movement
		float hSpeed = Input.GetAxis("Horizontal");
		anim.SetFloat("hSpeed", Mathf.Abs(hSpeed));
		rigidbody2D.velocity = new Vector2(hSpeed * maxSpeed, rigidbody2D.velocity.y);
		// Flip player sprite horizontally if moving left and not facing right 
		if (hSpeed < 0 && right)
			Flip ();
		// and vice versa
		else if ( hSpeed > 0 && !right)
			Flip ();
		
	}
	
	// Send KO signal to animator and wait 2 seconds to respawn
	IEnumerator GameOver() {
		anim.SetBool("GameOver", true);
		yield return new WaitForSeconds(2.0f);
		Application.LoadLevel(1);
	}

	// moving platform trigger
	IEnumerator MovePlat() {
		float targetx = movingPlat.transform.position.x + 1.6f;
		float zoomTime = 1.0f;
		float startTime = Time.time;
		while (targetx - movingPlat.transform.position.x > 0.01f) {
			movingPlat.transform.position = new Vector2(Mathf.Lerp (movingPlat.transform.position.x, 
			                                                        targetx, 
			                                                        (Time.time - startTime) / zoomTime), 
			                                            movingPlat.transform.position.y);
			yield return null;
		}
		fallingArrows = (GameObject)Instantiate(fallingArrows, faPosition.position, Quaternion.identity);
		Destroy(fallingArrows, 1.0f);
		yield return null;
	}

	// falling arrows trigger
	IEnumerator ArrowPattern() {
		while (!gameOver) {
			GameObject arrows = (GameObject)Instantiate(fallingArrowPatt, pattPosition.position, Quaternion.identity);
			Destroy (arrows, 0.75f);
			yield return new WaitForSeconds(1.0f);
		}
	}

	// following arrow trigger
	IEnumerator AfterPattern() {
		GameObject arrowLeft = (GameObject)Instantiate(leftArrow, leftArowPos.position, Quaternion.identity);
		arrowLeft.rigidbody2D.velocity = new Vector2(-arrowSpeed, 0);
		Destroy (arrowLeft, 4f);
		yield return null;
	}

	// upwards arrow trigger
	IEnumerator AfterPattern2() {
		GameObject arrowUp = (GameObject)Instantiate(upwardArrow, upwardArowPos.position, Quaternion.identity);
		arrowUp.rigidbody2D.velocity = new Vector2(0, arrowSpeed);
		Destroy (arrowUp, 4f);
		yield return null;
	}

	// Player shooting
	IEnumerator Shoot() {
		anim.SetTrigger("ThrowTrigger");
		yield return new WaitForSeconds(.5f);
		// spawn arrow and send it in the facing direction
		GameObject playerArrow = (GameObject)Instantiate(arrowPrefab, transform.position, Quaternion.identity);
		if (right)
			playerArrow.rigidbody2D.velocity = new Vector2(arrowSpeed, 0);
		else {
			playerArrow.transform.localScale = new Vector3(-playerArrow.transform.localScale.x, playerArrow.transform.localScale.y, playerArrow.transform.localScale.z);
			playerArrow.rigidbody2D.velocity = new Vector2(-arrowSpeed, 0);
		}
		// play shoot fx and destroy arrow after 1s
		throwAudio.Play();
		Destroy (playerArrow, 1.0f);
	}
	
	void Update() {
		
		// if game over disable controls and start game over coroutine
		if (gameOver) {
			if (!gameOverStarted) {
				gameOverStarted = true;
				// KO audio
				KOAudio.Play();
				StartCoroutine(GameOver());
			}
			return;
		}
		// if game won display GUI text and enable replay input, controls disabled
		if (gameWon) {
			if (!winDisplayed) {
				winDisplayed = true;
				winText.guiText.text = "Level 1 Complete!";
				replayText.guiText.text = "Press Enter or right-click to continue.";
			}
			if (Input.GetButtonDown("Fire2")) {
				Application.LoadLevel(2);
			}
			return;
		}
		
		// time buffers for shhoting and getting hit
		shootReady += Time.deltaTime;
		hitReady += Time.deltaTime;
		
		// follow player w/ camera + background
		Camera.main.transform.position = new Vector3(gameObject.transform.position.x + 1.0f, 
		                                             Camera.main.transform.position.y, 
		                                             Camera.main.transform.position.z);
		
		// if on the ground and jump key pressed, apply upward jump force + play jump fx
		if (Input.GetButtonDown("Jump") && onGround) {
			onGround = false;
			rigidbody2D.AddForce(new Vector2(0, jumpForce));
			jumpAudio.Play();
		}
		// shoot arrow on fire1, taking into account the shoot delay
		if (Input.GetButtonDown("Fire1") && shootReady > shootDelay) {
			shootReady = 0.0f;
			StartCoroutine(Shoot());
		}
		// Don't press L
		if (Input.GetKeyDown(KeyCode.L) && onGround) {
			Hit();
		}
		// God Mode
		if (Input.GetKeyDown(KeyCode.G)) {
			GodMode = true;
		}
		if (Input.GetKeyDown(KeyCode.H)) {
			GodMode = false;
		}
	}
	
	// If hit, deplete a heart, update heart GUI appropriately, play hit fx and hit anim
	// Instant death mechanic implemented, upper cases ignored
	void Hit() {
		if (GodMode)
			return;
		--health;
		//damagedAudio.Play();
		//anim.SetTrigger("HitTrigger");
		switch (health) {
		case 2:
			//heart3.SetActive(false);
			break;
		case 1:
			//heart2.SetActive(false);
			break;
		case 0:
			heart1.SetActive(false);
			// KO
			gameOver = true;
			break;
		}
	}
	
	// flip sprite horizontally, revert facing right bool
	void Flip() {
		right = !right;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}
	
	// Damage player on contract with enemy/environmental hazards and apply knockback
	void OnCollisionStay2D (Collision2D col)
	{
		if(col.gameObject.tag == "Spikes")
		{
			//rigidbody2D.AddForce(pushBack);
			// hit time buffer
			if (hitReady > hitDelay) {
				hitReady = 0f;
				Hit ();
			}
		}
	}
	
	// Trigger detection
	void OnTriggerEnter2D (Collider2D col)
	{
		if (col.gameObject.tag == "Enemy_Arrow") {
			if (hitReady > hitDelay) {
				hitReady = 0f;
				Hit ();
			}
		}
		if (col.gameObject.tag == "Move")
		{
			// remove trigger so the player doesn't trigger it multiple times
			Destroy(col.gameObject);
			StartCoroutine(MovePlat());
		}
		if (col.gameObject.tag == "Pattern")
		{
			Destroy(col.gameObject);
			StartCoroutine(ArrowPattern());
		}
		if (col.gameObject.tag == "AfterPattern")
		{
			Destroy(col.gameObject);
			StartCoroutine(AfterPattern());
		}
		if (col.gameObject.tag == "AfterPattern2")
		{
			Destroy(col.gameObject);
			StartCoroutine(AfterPattern2());
		}
		if (col.gameObject.tag == "Finish")
		{
			gameWon = true;
		}
	}
}
