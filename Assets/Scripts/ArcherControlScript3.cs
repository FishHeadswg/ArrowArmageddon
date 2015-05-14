/*
Author: Trevor Richardson
ArcherControlScript3.cs
04-17-2015

	Script for controlling the player archer as well as the game environment for Level 3.
	
 */
using UnityEngine;
using System.Collections;

public class ArcherControlScript3 : MonoBehaviour {
	
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
	float groundRadius = 0.1f;
	public LayerMask GroundMask;
	
	// GUI elements
	public int health = 1;
	bool gameOver = false;
	bool gameOverStarted = false;
	public bool gameWon = false;
	public GameObject heart1;
	public GameObject winText;
	public GameObject winText2;
	public GameObject replayText;
	bool winDisplayed = false;
	public bool gameWalk = false;
	
	// Audio
	public AudioSource jumpAudio;
	public AudioSource damagedAudio;
	public AudioSource throwAudio;
	public AudioSource KOAudio;
	public AudioSource bgm;
	public AudioSource win_bgm;
	
	bool GodMode = false;

	public GameObject winPath;

	
	// Send KO signal to animator and wait 2 seconds to respawn
	IEnumerator GameOver() {
		anim.SetBool("GameOver", true);
		yield return new WaitForSeconds(2.0f);
		if (gameWalk) {
			yield return new WaitForSeconds(60.0f);
			Application.LoadLevel(0);
		}
		else
			Application.LoadLevel(3);
	}
	
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
		// play shoot fx and destroy arrow after .75s
		throwAudio.Play();
		Destroy (playerArrow, .75f);
	}
	
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
		if (gameOver || gameWon && !gameWalk) {
			anim.SetFloat ("vSpeed", 0f);
			anim.SetFloat ("hSpeed", 0f);
			return;
		}
		// update jump/fall velocity in animator
		anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);
		
		// horizontal movement
		float hSpeed = Input.GetAxis("Horizontal");
		if (gameWalk)
			hSpeed = 0.15f;
		anim.SetFloat("hSpeed", Mathf.Abs(hSpeed));
		rigidbody2D.velocity = new Vector2(hSpeed * maxSpeed, rigidbody2D.velocity.y);
		// Flip player sprite horizontally if moving left and not facing right 
		if (hSpeed < 0 && right)
			Flip ();
		// and vice versa
		else if ( hSpeed > 0 && !right)
			Flip ();
		
	}
	
	void Update() {
		
		// if game over disable controls and start game over coroutine
		if (gameOver && !gameWalk) {
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
				gameWalk = true;
				winPath.SetActive(true);
				winDisplayed = true;
				bgm.Stop();
				win_bgm.Play();
				winText.guiText.text = "Congratulations!";
				winText2.guiText.text = "You survived Armageddon!";
				replayText.guiText.text = "Press Enter or right-click to replay.";

			}
			Camera.main.transform.position = new Vector3(gameObject.transform.position.x + 1.0f, 
			                                             Camera.main.transform.position.y, 
			                                             Camera.main.transform.position.z);
			if (Input.GetButtonDown("Fire2")) {
				Application.LoadLevel(0);
			}
			return;
		}
		
		// time buffers for shooting and getting hit
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
		// shoot arrow on fire1, taking into account the throw delay
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

	// Trigger detection
	void OnTriggerEnter2D (Collider2D col)
	{
		if (col.gameObject.tag == "Enemy_Arrow") {
			if (hitReady > hitDelay) {
				hitReady = 0f;
				Hit ();
			}
		}
	}

	void OnCollisionEnter2D (Collision2D col)
	{
		if (col.gameObject.tag == "Boss_Archer") {
			if (hitReady > hitDelay) {
				hitReady = 0f;
				Hit ();
			}
		}
	}
}
