/*
Author: Trevor Richardson
ArrowScript.cs
04-17-2015

	Controls button falling on title screen.
	
 */
using UnityEngine;
using System.Collections;

public class ButtonFall : MonoBehaviour {

	public AudioSource fall;
	bool hitGround = false;

	void OnCollisionEnter2D (Collision2D col) {
		if (!hitGround) {
			hitGround = true;
			fall.Play();
		}
	}
}
