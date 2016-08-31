using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using PDollarGestureRecognizer;

/// <summary>
/// Recognizing Behaviour that recognizes chosung.
/// </summary>
/// 
/// Chosung will be recognized by drawing Letters.
/// 
public class GestureChoBehaviour : MonoBehaviour {

	/// <summary>
	/// Flips Y coordination
	/// </summary>
	/// 
	/// This is because of inverted Y Coordination of GUI.
	public bool flipY;


	/// <summary>
	/// Point Count Threshole.
	/// </summary>
	/// 
	/// If point count does not over this, points will be ignored.
	/// 
	/// This is to ignore short flick.
	public int thresholePointCount = 64;

	/// <summary>
	/// Score Threshole.
	/// </summary>
	/// 
	/// If recognization score does not over this, it will not be updated.
	public float thresholeScore = 0.80f;

	public ScoreUIBehaviour[] scoreUIs;


	// Events...

	/// <summary>
	/// Emitted on update.
	/// </summary>
	/// char: A Recognized character.
	public CharEvent onUpdate;
	public UnityEvent onUpdateFinished;


	// Public State.

	/// <summary>
	/// Recognized chosung.
    /// </summary>
	[HideInInspector]
	public int numberRecognized = 1;



	// Inner State

	/// <summary>
	/// List of gestures
	/// </summary>
	/// This is recognizer specific states.
	private MultiMap<char, Gesture> gestures = new MultiMap<char, Gesture> ();

	/// <summary>
	/// Array of gestures
	/// </summary>
	/// This is recognizer specific states.
	/// This is required by PDollarGestureRecognizer.
	private Dictionary <char, Gesture[]> gesturesArray;
	private Dictionary <char, Result> gesturesResult;



	// Use this for initialization
	void Start () {
		numberRecognized = scoreUIs.Length;

		if (numberRecognized == 0)
			numberRecognized = 1;

		foreach (ScoreUIBehaviour ui in scoreUIs) {
			ui.gameObject.SetActive (false);
			ui.choBehaviour = this;
		}

		// Load Gestures
		TextAsset[] gesturesXml =
			Resources.LoadAll<TextAsset> ("GestureSet/HangulConsonant");
		
		foreach (TextAsset gestureXml in gesturesXml) {
			Gesture gesture = GestureIO.ReadGestureFromXML (gestureXml.text);
			gestures.Add (gesture.Name[0], gesture);
		}

		gesturesArray = new Dictionary<char, Gesture[]> ();
		gesturesResult = new Dictionary<char, Result> ();

		foreach (KeyValuePair <char, List<Gesture>> pair in gestures) {
			gesturesArray [pair.Key] =
				pair.Value.ToArray ();
		}
	}
	
	// Update is called once per frame
	void Update () {
	}


	/// <summary>
	/// Performs recognization for chosung.
	/// </summary>
	/// <param name="points">Points as Vector3</param>
	/// 
	/// Upon recognization, this will emit event onUpdate with recognized character.
	public void Recognize (List<Stroke> strokes) {
		if (strokes.Count == 0) return;

		int pcount = strokes.Sum (s => s.Count);

		if (pcount < thresholePointCount) return;

		Point[] gpoints = new Point[ pcount ];

		int gi = 0;
		for (int s = 0; s < strokes.Count; s++) {
			
			Stroke stroke = strokes [s];

			for (int i = 0; i < stroke.Count; i++) {

				Vector3 point = stroke [i];

				gpoints [gi] = new Point (
					point.x,
					flipY ? -(point.y) : (point.y),
					s);

				gi++;
			}
		}



		Gesture input = new Gesture (gpoints);

		int ni;


		// Gets recognize result by letters.
		foreach (KeyValuePair <char, Gesture[]> pair in gesturesArray) {
			gesturesResult[pair.Key] =
				PointCloudRecognizer.Classify (input, pair.Value);
		}

		// Gets best resulting letters.
		List<Result> results = 
			gesturesResult
				.OrderByDescending ((pair) => (pair.Value.Score))	// Sort pairs by value's score.
				.Take (numberRecognized)							// Take 4 pairs.
				.Select (pair => pair.Value)						// Get pair's value.
				.ToList ();
		
		// Display them on the button.
		for (ni = 0; ni < scoreUIs.Length; ni++) {
			if (ni < results.Count) {
				scoreUIs [ni].gameObject.SetActive (true);
				scoreUIs [ni].setLetter (results [ni].Score, results [ni].GestureClass [0]);
			}
			else {
				scoreUIs [ni].gameObject.SetActive (false);
			}
		}

		// Check for recognization score.
		if ((0 < results.Count) && (thresholeScore <= results [0].Score)) {
			onUpdate.Invoke (results [0].GestureClass [0]);
		}
		else {
			onUpdate.Invoke ('\0');
		}
	}

	public void FinishUpdate () {
		foreach (ScoreUIBehaviour ui in scoreUIs) {
			ui.gameObject.SetActive (false);
		}
		onUpdateFinished.Invoke ();
	}
}
