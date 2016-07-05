using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Leap;


public class OptionInputSurfaceBehaviour : MonoBehaviour {

	public float minDownVelocity = 50;
	public float minForwardVelocity = 50;

	public float historySeconds = 0.1f;
	public float minDownDistance = 3.0f;
	public float minForwardDistance = 5.0f;

	public float minPointDistance = 1.0f;

	public int countdownTime = 15;

	public HandController handController;

	public Text messageText;
	public string messageFormat;
	public string messageFormatCountdown;


	public UnityEngine.UI.Image[] tapTargets;

	public Sprite tapImage;
	public Sprite tapDoneImage;
	public Sprite tapRejectImage;

	public GameObject canvas;

	public UnityEvent onDone;
	public UnityEvent onDoneTimeout;


	private Vector3[] points;
	private int pointsIndex = 0;

	private OptionInputSurface optionResult = null;

	// Use this for initialization
	void Start () {
		if (handController == null) {
			handController = GameObject.FindObjectOfType <HandController> ();
		}

		if (handController == null) {
			Debug.LogError ("HandController is not found on the scene. This is required.");
		}


		Controller controller = handController.GetLeapController ();
		controller.EnableGesture (Gesture.GestureType.TYPE_KEY_TAP);
		controller.EnableGesture (Gesture.GestureType.TYPE_CIRCLE);
		controller.EnableGesture (Gesture.GestureType.TYPE_SCREEN_TAP);
		controller.EnableGesture (Gesture.GestureType.TYPE_SWIPE);

		controller.Config.SetFloat ("Gesture.KeyTap.MinDownVelocity", minDownVelocity);
		controller.Config.SetFloat ("Gesture.KeyTap.HistorySeconds", historySeconds);
		controller.Config.SetFloat ("Gesture.KeyTap.MinDistance", minDownDistance);
		controller.Config.SetFloat ("Gesture.ScreenTap.MinForwardVelocity", minForwardVelocity);
		controller.Config.SetFloat ("Gesture.ScreenTap.HistorySeconds", historySeconds);
		controller.Config.SetFloat ("Gesture.ScreenTap.MinDistance", minForwardDistance);
		controller.Config.Save ();

		points = new Vector3 [tapTargets.Length];

		//Show first target.
		tapTargets[0].sprite = tapImage;
		tapTargets[0].gameObject.SetActive (true);
		UpdateMessageText ();
	}
	
	// Update is called once per frame
	void Update () {
		// Get KeyTapGesture objects from the frame.

		Frame frame = handController.GetFrame ();

		GestureList gestures = frame.Gestures ();

		foreach (Gesture gesture in gestures) {
			if ((gesture.Type == Gesture.GestureType.TYPE_KEY_TAP) ||
				 (gesture.Type == Gesture.GestureType.TYPE_SCREEN_TAP)) {

				Transform trans = handController.transform;
				Pointable finger = gesture.Pointables [0];


				Vector3 pt = 
					trans.TransformPoint (finger.TipPosition.ToUnityScaled (false));

				AddPoint (pt);
			}
		}
	}



	private void AddPoint (Vector3 point) {
		if (pointsIndex == points.Length) {
			return;
		}

		// Check for point.
		for (int i = 0; i < pointsIndex; i++) {
			if (Vector3.Distance (points [i], point) < minPointDistance) {
				tapTargets [pointsIndex].sprite = tapRejectImage;
				return;
			}
		}

		points [pointsIndex] = point;

		// Show Done image.
		tapTargets [pointsIndex].sprite = tapDoneImage;

		pointsIndex++;

		if (pointsIndex == points.Length) {
			
			// Done! Calculate Plane Position and go back to the scene!
			optionResult = CalculatePlanePosition ();

			if (optionResult != null) {
				PlayerPrefs.SetInt ("option-input-surface-set", 1);

				optionResult.ApplyTo (canvas);

				StartCoroutine ("Countdown");
				onDone.Invoke ();
			}
			
		} else {

			tapTargets [pointsIndex].sprite = tapImage;
			tapTargets [pointsIndex].gameObject.SetActive (true);
			
		}
		// Message about left points.
		UpdateMessageText ();
	}


	private void UpdateMessageText () {
		if ((messageText != null) &&
		    (messageFormat != null))
			messageText.text = string.Format (messageFormat,
				pointsIndex,
				points.Length,
				points.Length - pointsIndex);
	}



	private OptionInputSurface CalculatePlanePosition () {
		Vector3 position;
		Vector3 normal; 
		Vector3 up;

		position = GetAverage (points);

		// normal.
		Vector3[] normals = new Vector3 [4];
		normals [0] = GetNormal (points [0], points [1], points [3]);
		normals [1] = GetNormal (points [1], points [3], points [2]);
		normals [2] = GetNormal (points [3], points [2], points [0]);
		normals [3] = GetNormal (points [2], points [0], points [1]);
		normal = - GetAverage (normals);
		normal.Normalize ();

		up = points [0] + points[1] - points [2] - points [3];
		up.Normalize ();

		return new OptionInputSurface (position, normal, up);
	}



	public void Confirm () {
		StopCoroutine ("Countdown");

		optionResult.store_pref ("option-input-surface");
		SceneManager.LoadScene ("FingerUI2");
	}


	private static Vector3 GetAverage (Vector3[] vectors) {
		Vector3 result = new Vector3 ();

		foreach (Vector3 v in vectors) {
			result += v;
		}

		result /= vectors.Length;
		return result;
	}

	private static Vector3 GetNormal (Vector3 a, Vector3 b, Vector3 c) {
		Vector3 result = Vector3.Cross (b - a, c - b);
		return result.normalized;
	}

	private IEnumerator Countdown () {
		for (int i = countdownTime; 0 < i; i--) {
			messageText.text = string.Format (messageFormatCountdown, i);
			yield return new WaitForSeconds (1);
		}

		// If count down timeout.
		foreach (var image in tapTargets) {
			image.gameObject.SetActive (false);
		}
		pointsIndex = 0;
		onDoneTimeout.Invoke ();

		tapTargets[0].sprite = tapImage;
		tapTargets[0].gameObject.SetActive (true);
		UpdateMessageText ();
	}
}
