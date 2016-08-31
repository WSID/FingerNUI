using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreUIBehaviour : MonoBehaviour {

	public char letter;
	public Text letterText;
	public Text scoreText;

	[HideInInspector]
	public GestureChoBehaviour choBehaviour;

	// Use this for initialization
	void Start () {
		BtnPointerEventBehaviour btnBehaviour = GetComponent <BtnPointerEventBehaviour> ();

		if (btnBehaviour != null)
			btnBehaviour.onPushed.AddListener (feed);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setLetter (float score, char letter) {
		this.letter = letter;
		letterText.text = letter.ToString ();
		scoreText.text = score.ToString ("P");
	}

	public void feed () {
		if (choBehaviour != null)
			choBehaviour.onUpdate.Invoke (letter);
			choBehaviour.FinishUpdate ();
	}
}
