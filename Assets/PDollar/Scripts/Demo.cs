using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using PDollarGestureRecognizer;

public class Demo : MonoBehaviour {

	public Transform gestureOnScreenPrefab;

	private List<Gesture> trainingSet = new List<Gesture>();

	private List<Point> points = new List<Point>();
	private int strokeId = -1;

	private Vector3 virtualKeyPosition = Vector2.zero;
	private Rect drawArea;

	private RuntimePlatform platform;
	private int vertexCount = 0;

	private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
	private LineRenderer currentGestureLineRenderer;

	private Leap.Controller controller;

	//GUI
	private string message;
	private bool recognized;
	private bool isUserGestureStarted = false;
	private string newGestureName = "";
	private GUIStyle guiStyle = new GUIStyle();

	void Start () {

		platform = Application.platform;
		drawArea = new Rect(445, 150, 3*(Screen.width/10), 6*(Screen.height/10));
		//drawArea = new Rect(Screen, Screen.height - 3*(Screen.height/10), 3*(Screen.width/10), 6*(Screen.height/10));

		//Load pre-made gestures
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/yong-finger-HanGle");
		foreach (TextAsset gestureXml in gesturesXml)
			trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

		//Load user custom gestures
		string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
		foreach (string filePath in filePaths)
			trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));

		controller = new Leap.Controller ();
	}

	void Update () {

		Leap.Frame frame = controller.Frame();

		Leap.HandList hands = frame.Hands;
		//Debug.Log ("number of hands : " + hands.Count);

		int fingerCount = 0;

		Leap.Finger finger = null;

		if (hands.Count >= 1) {



			Leap.Hand firstHand = hands[0];
			Leap.FingerList fingers = firstHand.Fingers;
			fingerCount = fingers.Count;
			if(fingers.Count > 0){
				finger = fingers[1];

				float x = (float)finger.TipPosition.x * 1.4f;

				virtualKeyPosition = new Vector3(x+620, finger.TipPosition.y+80, finger.TipPosition.z);
				Debug.Log("finger position : " + (virtualKeyPosition.x+600) + ", " + (virtualKeyPosition.y+80) + " , " + virtualKeyPosition.z);
			}
		}
/*
		if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touchCount > 0) {
				virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
			}
		} else{
			if (Input.GetMouseButton(0)) {

				virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			}
		}
*/
		if (drawArea.Contains(virtualKeyPosition)) {

			if(finger.TipPosition.z >= 0 ){
				isUserGestureStarted = false;
			}

			//if (Input.GetMouseButtonDown(0)) {
			if(fingerCount > 0 && finger.TipPosition.z < 0 && !isUserGestureStarted){

				isUserGestureStarted = true;

				if (recognized) {

					recognized = false;
					strokeId = -1;

					points.Clear();

					foreach (LineRenderer lineRenderer in gestureLinesRenderer) {

						lineRenderer.SetVertexCount(0);
						Destroy(lineRenderer.gameObject);
					}

					gestureLinesRenderer.Clear();
				}

				++strokeId;
				
				Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
				currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();
				
				gestureLinesRenderer.Add(currentGestureLineRenderer);
				
				vertexCount = 0;
			}
			
			//if (Input.GetMouseButton(0)) {
			if (fingerCount > 0 && isUserGestureStarted) {
				points.Add(new Point(virtualKeyPosition.x, -virtualKeyPosition.y, strokeId));

				currentGestureLineRenderer.SetVertexCount(++vertexCount);
				currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));
			}

			recognized = true;
			
			Gesture candidate = new Gesture(points.ToArray());
			Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

			message =  " \" " + gestureResult.GestureClass + " \" (HanGle Match Rate : " + gestureResult.Score + ")";
		}


	}

	void OnGUI() {

		GUI.Box(drawArea, "Draw Area");

		guiStyle.fontSize = 30; //change the font size
		guiStyle.normal.textColor = Color.cyan; 

		//Font myFont = (Font)Resources.Load("arial", typeof(Font));
		//guiStyle.font = myFont;

		GUI.Label(new Rect(300, 50, 500, 50), message, guiStyle);

		/*if (GUI.Button(new Rect(Screen.width - 100, 10, 100, 30), "Recognize")) {

			recognized = true;

			Gesture candidate = new Gesture(points.ToArray());
			Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());
			
			message = " \" " + gestureResult.GestureClass + " \" (HangGle Match Rate : " + gestureResult.Score + ")";
		}

		GUI.Label(new Rect(Screen.width - 200, 150, 70, 30), "Add as: ");
		newGestureName = GUI.TextField(new Rect(Screen.width - 150, 150, 100, 30), newGestureName);

		if (GUI.Button(new Rect(Screen.width - 50, 150, 50, 30), "Add") && points.Count > 0 && newGestureName != "") {

			string fileName = String.Format("{0}/{1}-{2}.xml", Application.persistentDataPath, newGestureName, DateTime.Now.ToFileTime());

			#if !UNITY_WEBPLAYER
				GestureIO.WriteGesture(points.ToArray(), newGestureName, fileName);
			#endif

			trainingSet.Add(new Gesture(points.ToArray(), newGestureName));

			newGestureName = "";
		}*/
	}
}

