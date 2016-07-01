using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class OptionInputSurfaceBehaviour : MonoBehaviour {

	public float minDownVelocity = 50;
	public float historySeconds = 0.1f;

	public HandController handController;
	private Controller controller;

	private List<Vector3> points;

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

		points = new List<Vector3> ();
	}
	
	// Update is called once per frame
	void Update () {
		// Get KeyTapGesture objects from the frame.

		Frame frame = handController.GetFrame ();

		GestureList gestures = frame.Gestures ();

		foreach (Gesture gesture in gestures) {
			if ((gesture.Type == Gesture.GestureType.TYPE_KEY_TAP) ||
				 (gesture.Type == Gesture.GestureType.TYPE_SCREEN_TAP)) {

				Vector3 pt = 
					handController.transform.TransformPoint (
						gesture.Pointables [0].TipPosition.ToUnityScaled (false));

				AddPoint (pt);
			}
		}
	}

	private void AddPoint (Vector3 point) {
		points.Add (point);

		if (points.Count == 4) {
			// Done! Calculate Plane Position and go back to the scene!
			Vector3 position;
			Vector3 normal;

			CalculatePlanePosition (Vector3.forward, out position, out normal);
			
		} else {
			// Message about left points.
		}
	}

	private bool CalculatePlanePosition (Vector3 direction, out Vector3 position, out Vector3 normal) {
		// position.
		Vector3 sum = points[0] + points[1] + points[2] + points[3];

		position = sum * 0.25f;


		// normal.
		normal = Vector3.forward;

		return true;
	}
}
