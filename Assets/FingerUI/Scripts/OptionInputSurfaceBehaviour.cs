using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Leap;

/// <summary>
/// Main component for OptionInputSurface.unity
/// </summary>
public class OptionInputSurfaceBehaviour : MonoBehaviour {

	#region TapSetting
	public float minDownVelocity = 50;
	public float minForwardVelocity = 50;

	public float historySeconds = 0.1f;

	public float minDownDistance = 3.0f;
	public float minForwardDistance = 5.0f;
	#endregion


	/// <summary>
	/// The minimum point distance.
	/// </summary>
	/// When points are too close, it is likely to have wrong direction.
	public float minPointDistance = 1.0f;

	/// <summary>
	/// The countdown time.
	/// </summary>
	/// Countdown time for user confirmation.
	public int countdownTime = 15;

	public HandController handController;


	#region Messages
	public Text messageText;

	/// <summary>
	/// The message format.
	/// </summary>
	/// 0: int: number of done points.
	/// 1: int: number of all points.
	/// 2: int: number of left points.
	public string messageFormat;

	/// <summary>
	/// The message format for counting down confirmation.
	/// </summary>
	/// 0: left seconds.
	public string messageFormatCountdown;
	#endregion


	#region TapImages
	/// <summary>
	/// Image to show.
	/// </summary>
	/// The image will be shown one by one.
	public UnityEngine.UI.Image[] tapTargets;

	public Sprite tapImage;
	public Sprite tapDoneImage;
	public Sprite tapRejectImage;
	#endregion

	/// <summary>
	/// The canvas.
	/// </summary>
	/// The canvas to adjust view position.
	/// When tapping is done, canvas transformation will be set,
	/// so that user can check new input surface.
	public GameObject canvas;

	/// <summary>
	/// On Done Event
	/// </summary>
	/// When user tapped all corner, this will be invoked.
	public UnityEvent onDone;

	/// <summary>
	/// On Done Timeout Event
	/// </summary>
	/// When use didn't pushed confirmation button in time,
	/// this will be called.
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
		if (!CheckPoint (point)) {
			tapTargets [pointsIndex].sprite = tapRejectImage;
			return;
		}

		points [pointsIndex] = point;

		// Show Done image.
		if (!ShowNext ()) {
			
			// Done! Calculate Plane Position and go back to the scene!
			optionResult = CalculatePlanePosition ();

			if (optionResult != null) {
				optionResult.ApplyTo (canvas);

				StartCoroutine ("Countdown");
				onDone.Invoke ();
			}
		}

		// Message about left points.
		UpdateMessageText ();
	}

	/// <summary>
	/// Checks the point.
	/// </summary>
	/// The point will be checked by,
	/// 
	/// - Distances between previous points.
	/// 
	/// <returns>Whether the point would be accepted.</returns>
	/// <param name="point">Point to check</param>
	private bool CheckPoint (Vector3 point) {
		for (int i = 0; i < pointsIndex; i++) {
			if (Vector3.Distance (points [i], point) < minPointDistance) {
				return false;
			}
		}
		return true;
	}


	/// <summary>
	/// Shows the next tap image
	/// </summary>
	/// Mark the current tap target as done, and show next target.
	/// <returns>Whether this has next target.</returns>
	private bool ShowNext () {
		// Set current target as done.
		tapTargets [pointsIndex].sprite = tapDoneImage;
		pointsIndex++;

		// Move to next target.
		if (pointsIndex <= points.Length) {

			tapTargets [pointsIndex].sprite = tapImage;
			tapTargets [pointsIndex].gameObject.SetActive (true);

			return true;
		}
		else {
			return false;
		}

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

		PlayerPrefs.SetInt ("option-input-surface-set", 1);

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
