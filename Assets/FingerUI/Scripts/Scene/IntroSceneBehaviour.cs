using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

public class IntroSceneBehaviour : MonoBehaviour {

	public HandController controller;
	public Image movieImage;

	public int countdownTime = 5;
	public Text countdownText;


	// Use this for initialization
	void Start () {
		if (controller == null)
			controller = GameObject.FindObjectOfType <HandController> ();

		if (movieImage != null)
			movieImage.gameObject.SetActive (false);

		StartCoroutine ("countdown");
	}
	
	// Update is called once per frame
	void Update () {
		foreach (HandModel hand in controller.GetAllGraphicsHands ()) {
			Leap.Hand lhand = hand.GetLeapHand ();

			if (lhand.GrabStrength > 0.9f) {
				Confirm ();
				break;
			}
		}
	}

	public void Confirm () {
		StopCoroutine ("countdown");
		// Move to primary scene.

		SceneManager.LoadScene ("FingerUI2");
	}

	private IEnumerator countdown () {
		for (int i = countdownTime; 0 < i; i--) {
			countdownText.text = i.ToString ();
			yield return new WaitForSeconds (1);
		}

		// Show video!
		MovieTexture movie = null;

		if (movieImage != null) {
			movieImage.gameObject.SetActive (true);
			movie = movieImage.material.mainTexture as MovieTexture;
		}

		if (movie != null) {
			movie.Play ();
			yield return new WaitWhile (() => (movie.isPlaying));
		}

		Confirm ();
	}
}
