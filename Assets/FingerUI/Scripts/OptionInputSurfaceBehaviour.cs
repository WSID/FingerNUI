using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Leap;


public class OptionInputSurfaceBehaviour : MonoBehaviour {

	public float minDownVelocity = 50;
	public float historySeconds = 0.1f;

	public HandController handController;
	private Controller controller;

	public Text messageText;
	public string messageFormat;


	public UnityEngine.UI.Image[] tapTargets;

	public Sprite tapImage;
	public Sprite tapDoneImage;

	private Vector3[] points;
	private int pointsIndex = 0;

	// Use this for initialization
	void Start () {
		if (handController == null) {
			handController = GameObject.FindObjectOfType <HandController> ();
		}

		if (handController == null) {
			Debug.LogError ("HandController is not found on the scene. This is required.");
		}


		controller = handController.GetLeapController ();
		controller.EnableGesture (Gesture.GestureType.TYPE_KEY_TAP);
		controller.EnableGesture (Gesture.GestureType.TYPE_CIRCLE);
		controller.EnableGesture (Gesture.GestureType.TYPE_SCREEN_TAP);
		controller.EnableGesture (Gesture.GestureType.TYPE_SWIPE);

		controller.Config.SetFloat ("Gesture.KeyTap.MinDownVelocity", minDownVelocity);
		controller.Config.SetFloat ("Gesture.KeyTap.HistorySeconds", historySeconds);
		controller.Config.Save ();

		points = new Vector3 [tapTargets.Length];

		//Show first target.
		tapTargets[0].sprite = tapImage;
		tapTargets[0].gameObject.SetActive (true);
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
				Debug.LogFormat ("ADD: {0}", pt);
			}
		}
	}



	private void AddPoint (Vector3 point) {
		if (pointsIndex == points.Length)
			Debug.LogWarning ("Adding extra points!");

		points [pointsIndex] = point;

		// Show Done image.
		tapTargets [pointsIndex].sprite = tapDoneImage;

		pointsIndex++;

		if (pointsIndex == points.Length) {
			
			// Done! Calculate Plane Position and go back to the scene!
			Vector3 position;
			Vector3 normal; 

			if (CalculatePlanePosition (out position, out normal)) {
				Debug.LogFormat ("pos{0}, nor{1}", position, normal);

				OptionBehaviour.optionResults [OptionBehaviour.KEY_OPTION_INPUT_SURFACE]
				= new OptionInputSurface (position, normal, Vector3.up);

				Invoke ("ReturnToUI", 1);
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



	private bool CalculatePlanePosition (out Vector3 position, out Vector3 normal) {
		position = GetAverage (points);

		// normal.
		normal = Vector3.forward;

		return true;
	}


	private static Vector3 GetAverage (Vector3[] vectors) {
		Vector3 result = new Vector3 ();

		foreach (Vector3 v in vectors) {
			result += v;
		}

		result /= vectors.Length;
		return result;
	}


	void ReturnToUI () {
		SceneManager.LoadScene ("FingerUI2");
	}
}
