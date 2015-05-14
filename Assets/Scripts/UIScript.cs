/*
Author: Trevor Richardson
UIScript.cs
04-17-2015

	Script for controlling UI, mainly the title screen.
	
 */
using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class UIScript : MonoBehaviour {

	// Variables / objects
	float speed = 10f;
	public GameObject play, exit;
	public Transform barrage;
	public Sprite playButton, playButton_s, exitButton, exitButton_s;
	public UISprite playSprite, exitSprite;
	bool selectionReady = false;
	bool playSelected = true;
	float alpha = 1.0f;
	public Texture blackTexture;
	public AudioSource treeHit, shoot, menu;


	// Shoot arrow at tree
	void Start () {
		rigidbody2D.velocity = new Vector2(speed, 0);

		SetPlayButton(true);
	}
	
	// Selection control
	void Update () {
		if (Input.GetAxis("Vertical") > 0) {
			SetPlayButton(true);
			menu.Play();
		}
		if (Input.GetAxis("Vertical") < 0) {
			SetExitButton(true);
			menu.Play();
		}
		if (selectionReady && playSelected && (Input.GetButtonDown("Fire1") || Input.GetKey(KeyCode.Return))) {
			play.rigidbody2D.isKinematic = false;
			exit.rigidbody2D.isKinematic = false;
			StartCoroutine(LoadFade());
			}
		}

	// Load transition after selecting play.
	IEnumerator LoadFade() {
		shoot.Play();
		foreach (Transform child in barrage) 
			child.gameObject.rigidbody2D.velocity = new Vector2(speed + 5, 0);
		shoot.Play();
		yield return new WaitForSeconds(1.0f);
		Application.LoadLevel(1);
		}

	// Move the buttons up after hitting the floor
	void moveUp() {
		HOTween.To(playSprite.transform, 2.0f, "localPosition", new Vector3(0f, 50f, 0f), true, EaseType.EaseInBounce, 0);
		HOTween.To(exitSprite.transform, 2.0f, "localPosition", new Vector3(0f, 40f, 0f), true, EaseType.EaseInBounce, 0);
	}

	// Play button control
	void SetPlayButton(bool status) {
		if (status) {
			playSprite.spriteName = playButton_s.name;
			exitSprite.spriteName = exitButton.name;
			playSelected = true;
		}
		else 
			playSprite.spriteName = playButton.name;
	}

	// Exit button control
	void SetExitButton(bool status) {
		if (status) {
			exitSprite.spriteName = exitButton_s.name;
			playSprite.spriteName = playButton.name;
			playSelected = false;
		}
		else 
			exitSprite.spriteName = exitButton.name;
	}

	// Shake tree upon arrow impact
	void OnTriggerEnter2D(Collider2D other) {
		rigidbody2D.velocity = new Vector2(0, 0);
		StartCoroutine(TreeShake(other));
		collider2D.enabled = false;
	}

	// Shake the tree, cause the buttons to fall/rise and spawn the load transition arrows
	IEnumerator TreeShake(Collider2D other) {
		treeHit.Play();
		other.gameObject.transform.position = new Vector2(other.gameObject.transform.position.x + .01f, other.gameObject.transform.position.y);
		yield return new WaitForSeconds(0.05f);
		other.gameObject.transform.position = new Vector2(other.gameObject.transform.position.x - .01f, other.gameObject.transform.position.y);
		yield return new WaitForSeconds(0.05f);
		other.gameObject.transform.position = new Vector2(other.gameObject.transform.position.x + .01f, other.gameObject.transform.position.y);
		yield return new WaitForSeconds(0.05f);
		other.gameObject.transform.position = new Vector2(other.gameObject.transform.position.x - .01f, other.gameObject.transform.position.y);

		play.rigidbody2D.isKinematic = false;
		exit.rigidbody2D.isKinematic = false;
		yield return new WaitForSeconds(0.05f);

		while(playSprite.rigidbody2D.angularVelocity != 0f || playSprite.rigidbody2D.velocity.magnitude != 0f)
			yield return null;
		play.rigidbody2D.isKinematic = true;
		exit.rigidbody2D.isKinematic = true;
		barrage.gameObject.SetActive(true);
		moveUp();
		selectionReady = true;

		yield return null;
	}

}
