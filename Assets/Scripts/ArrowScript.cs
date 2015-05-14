/*
Author: Trevor Richardson
ArrowScript.cs
04-17-2015

	Simple script to destroy an arrow if it hits an environmental object.
	
 */
using UnityEngine;
using System.Collections;


public class ArrowScript : MonoBehaviour
{

		void OnTriggerEnter2D (Collider2D col)
		{
				if (col.gameObject.name != "Player_Archer" 
		    	&& col.gameObject.name != "Player_Archer02" 
		    	&& col.gameObject.name != "Player_Archer03" 
		    	&& col.gameObject.tag != "Enemy_Archer" 
		    	&& col.gameObject.tag != "Boss_Archer" 
		    	&& col.gameObject.tag != "AfterPattern2" 
		    	&& col.gameObject.tag != "Move") {
						Destroy (gameObject);
				}
		}
}
