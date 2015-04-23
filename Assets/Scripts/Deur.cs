﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnitySampleAssets.CrossPlatformInput;

public class Deur : MonoBehaviour {

	// Use this for initialization
	public static bool open = true;
	public GameObject spawnPoint;
	public Camera mainCamera;
	public float wait = 0.25f;

	// Fade in/out
	public float fadeInSpeed = 1.5f;        // Speed that the screen fades from black.
	public float fadeOutSpeed = 1.5f;       // Speed that the screen fades to black.
	public Image overlay;                   // Object in HUD that fills screen with a full alpha, black image

	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player") 
		{
			if(CrossPlatformInputManager.GetButtonDown ("Jump"))
			{
				//maak het scherm zwart
				StartCoroutine(FadeToBlack(collision));
			}
		}
	}

	IEnumerator FadeToBlack(Collider2D collision)
	{
		// Lerp the colour of the texture between itself and transparent.
		float rate = fadeOutSpeed;
		float progress = 0f;
		while (progress < 1f)
		{
			overlay.color = Color.Lerp(Color.clear, Color.black, progress);
			progress += rate * Time.deltaTime;
			
			yield return null;
		}
		overlay.color = Color.black;

		// Naar nieuwe kamer
		mainCamera.transform.position = new Vector3(spawnPoint.transform.position.x, spawnPoint.transform.position.y, spawnPoint.transform.position.z);
		collision.transform.position = spawnPoint.transform.position;

		yield return new WaitForSeconds(wait);

		// Lerp the colour of the texture between itself and transparent.
		rate = fadeInSpeed;
		progress = 0f;
		while (progress < 1f)
		{
			overlay.color = Color.Lerp(Color.black, Color.clear, progress);
			progress += rate * Time.deltaTime;
			
			yield return null;
		}
		overlay.color = Color.clear;
	}
}